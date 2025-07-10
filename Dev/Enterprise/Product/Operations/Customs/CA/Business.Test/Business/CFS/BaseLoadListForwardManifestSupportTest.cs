using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.CA.Business.Testing
{
	public abstract class BaseLoadListForwardManifestSupportTest : TestCaseWithFactory
	{
		#region SetUp

		protected ACIHouseBillMessage message1, message2, message3, message4, message5, message6;
		protected CFSLoadListConsol loadList;
		protected CFSShipment shipment2;
		protected CFSShipment shipment3;
		protected CFSShipment shipment4;
		protected CFSShipment shipment5;

		protected override void SetUp()
		{
			base.SetUp();

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Canada);

			message1 = GetACIHouseBillMessage("10207000067891", "WH", "91651234567891");
			message2 = GetACIHouseBillMessage("10207000067892", "WH", "91651234567891");
			message3 = GetACIHouseBillMessage("10207000067893", "WH", "91651234567891");
			message4 = GetACIHouseBillMessage("10207000067894", "WH", "91651234567891");
			message5 = GetACIHouseBillMessage("10207000067895", "WH", "91651234567891");
			message6 = GetACIHouseBillMessage("10207000067896", "WH", "91651234567892");

			loadList = Factory.New<CFSLoadListConsol>();

			CusEntryNumber num1 = loadList.Numbers.AddNew();
			num1.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			num1.CE_EntryNum = "9165 1234567891";

			shipment2 = loadList.Shipments.AddNew();

			CusEntryNumber num3 = shipment2.Numbers.AddNew();
			num3.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			num3.CE_EntryNum = "1020 7000067892";

			shipment3 = loadList.Shipments.AddNew();
			message3.EM_LinkedObject = shipment3;

			CusEntryNumber num4 = shipment3.Numbers.AddNew();
			num4.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			num4.CE_EntryNum = "1020 7000067893";

			shipment4 = Factory.New<CFSShipment>();
			message4.EM_LinkedObject = shipment4;

			CusEntryNumber num5 = shipment4.Numbers.AddNew();
			num5.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			num5.CE_EntryNum = "1020 7000067894";

			shipment5 = loadList.Shipments.AddNew();
			message5.EM_LinkedObject = shipment5;

			CusEntryNumber num6 = shipment5.Numbers.AddNew();
			num6.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			num6.CE_EntryNum = "1020 7000067896";

			Factory.Save();
		}

		ACIHouseBillMessage GetACIHouseBillMessage(string houseCCN, string snpType, string primaryCCN)
		{
			var message = Factory.New<ACIHouseBillMessage>();
			message.EM_MessageSubType = MessageTypeList.Codes.ManifestForwardHouse;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = string.Format(
				@"UNH+1+GOVCBR:D:11B:UN
BGM+714+{0}+4
RFF+AFM:10207:{1}
RFF+UCN:UCR555
DOC+23+:24
DOC+85+{2}
RCS+15
FTX+ACB+++SOM B 2 B COMMENTS
TDT+11++1
UNS+D", houseCCN, snpType, primaryCCN).Replace("\r\n", "'");

			message.PrimaryCCN = primaryCCN;
			message.SetSystemDefinedValue(EDIMessage.Schema.SNPType, (ZString)snpType);
			Factory.Save();
			return message;
		}
		#endregion
	}
}
