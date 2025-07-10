using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	[TestedType(typeof(TradeChainPartnerMessageManager))]
	sealed class TradeChainPartnerMessageManagerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSendMessage()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "FENIMP";
			org.OH_FullName = "FENIX IMPORTS INC";

			var orgTCP1 = Factory.New<OrgHeader>();
			orgTCP1.OH_Code = "FENVEN";
			orgTCP1.OH_FullName = "DOLE FRESH VEGETABLES";

			var tcp1Addr = orgTCP1.Addresses.AddNew();
			tcp1Addr.Address1 = "500 S ALTA ST";
			tcp1Addr.Address2 = "";
			tcp1Addr.OA_RN_NKCountryCode = "US";
			tcp1Addr.City = "GONZALES";
			tcp1Addr.Postcode = "93926";
			tcp1Addr.State = "CA";

			var orgImpAddInfo = OrgImpAddInfo.Get(org);

			var tcp1 = orgImpAddInfo.TradeChainPartners.AddNew();
			tcp1.CA_Org = org.PK;
			tcp1.CA_Address = tcp1Addr.PK;
			tcp1.CA_CSAIDType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			tcp1.CA_Action = CSAActionTypeList.Codes.ReqAdd;
			tcp1.CA_CSAID = "645321789";

			var wrapper = OrgHeaderTCPMessageWrapper.New(org);

			var tcpSend = new TradeChainPartnerMessageManager(wrapper);
			tcpSend.TCPCollection.Cast<TradeChainPartnerSendingObject>().ForEach(x => x.SendOption = true);
			tcpSend.SendMessage();
			AssertEquals(CSAStatusList.Codes.AwaitingAdd, tcp1.CA_CSAStatus);

			tcp1.CA_CSAStatus = CSAStatusList.Codes.Added;
			tcp1.CA_Action = CSAActionTypeList.Codes.ReqAdd;
			Factory.Save();

			tcpSend.SendMessage();
			AssertEquals(CSAStatusList.Codes.AwaitingAdd, tcp1.CA_CSAStatus);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var org = Factory.New<OrgHeader>();
			var wrapper = OrgHeaderTCPMessageWrapper.New(org);
			return new TradeChainPartnerMessageManager(wrapper);
		}
	}
}
