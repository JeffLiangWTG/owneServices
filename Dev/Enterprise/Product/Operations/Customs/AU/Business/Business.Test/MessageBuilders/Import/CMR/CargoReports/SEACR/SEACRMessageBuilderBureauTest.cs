using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SEACRMessageBuilderBureauTest : CMRSeaCargoDepotTestCase
	{
		public void TestIsBureau()
		{
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = CompanyABN;
			CreateOBL250805001ForwardingConsol(Factory);
			var consol = Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, OBL250805001_OceanBillNum));
			var synchroniser = new CMRSeaCargoSynchroniser(consol);
			var oceanBill = synchroniser.OceanBill;
			oceanBill.CB_IsBureau = true;
			var house = oceanBill.HouseBills[0];

			var builder = new SEACRMessageBuilder(house, OwnerABN);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			builder.Messages = house.Messages;
			var message = builder.PopulateMessagesReturningResult();

			AssertEquals("Message Owner", OwnerABN, message.EM_MessageOwner);
		}

		public void TestIsNotBureau()
		{
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = CompanyABN;
			CreateOBL250805001ForwardingConsol(Factory);
			var consol = Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, OBL250805001_OceanBillNum));
			var synchroniser = new CMRSeaCargoSynchroniser(consol);
			var oceanBill = synchroniser.OceanBill;

			oceanBill.CB_IsBureau = false;
			var house = oceanBill.HouseBills[0];

			var builder = new SEACRMessageBuilder(house, OwnerABN);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			builder.Messages = house.Messages;
			var message = builder.PopulateMessagesReturningResult();

			AssertEquals("Message Owner", ZString.Empty, message.EM_MessageOwner);
		}

		const string OwnerABN = "61852570977";
		const string CompanyABN = "14001592650";
	}
}
