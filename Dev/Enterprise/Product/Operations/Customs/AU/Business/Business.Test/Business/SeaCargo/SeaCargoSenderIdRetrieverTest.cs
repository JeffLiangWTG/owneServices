using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SeaCargoSenderIdRetrieverTest : TestCaseWithFactory
	{
		public void TestGetSenderIDForDepotWhenBranchSet()
		{
			Env.Registry.SetAUCustomsSeaCargoDepotMailboxForBranch(otherBranch.PK.ToGuid(), "123");
			Env.Registry.SetAUCustomsSeaCargoDepotMailboxForCompany(otherBranch.Company.PK.ToGuid(), null);
			AssertEquals("123", SeaCargoSenderIdRetriever.GetSenderID(otherBranch, jobContainer.TableName));
		}

		public void TestGetSenderIDForDepotWhenCompanySet()
		{
			Env.Registry.SetAUCustomsSeaCargoDepotMailboxForCompany(otherBranch.Company.PK.ToGuid(), "123");
			AssertEquals("123", SeaCargoSenderIdRetriever.GetSenderID(otherBranch, jobShipment.TableName));
		}

		public void TestGetSenderIDForDepotWhenCompanyAndBranchSet()
		{
			Env.Registry.SetAUCustomsSeaCargoDepotMailboxForBranch(otherBranch.PK.ToGuid(), "123");
			Env.Registry.SetAUCustomsSeaCargoDepotMailboxForCompany(otherBranch.Company.PK.ToGuid(), "456");
			AssertEquals("123", SeaCargoSenderIdRetriever.GetSenderID(otherBranch, jobContainer.TableName));
		}

		public void TestGetSenderIDForDepotWhenNotSet()
		{
			Env.Registry.SetAUCustomsSeaCargoDepotMailboxForBranch(otherBranch.PK.ToGuid(), null);
			Env.Registry.SetAUCustomsSeaCargoDepotMailboxForCompany(otherBranch.Company.PK.ToGuid(), null);
			AssertEquals(ZString.Empty, SeaCargoSenderIdRetriever.GetSenderID(otherBranch, jobConsol.TableName));
		}

		public void TestGetSenderIDForForwardingWhenBranchSet()
		{
			Env.Registry.SetAUCustomsSenderIDForBranch(otherBranch.PK.ToGuid(), "123");
			Env.Registry.SetAUCustomsSenderIDForCpmpany(otherBranch.Company.PK.ToGuid(), null);
			AssertEquals("123", SeaCargoSenderIdRetriever.GetSenderID(otherBranch, cusSCAHouseBill.TableName));
		}

		protected override void SetUp()
		{
			base.SetUp();
			otherBranch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "SYD");
			jobContainer = (BusinessObject)Factory.New<Integration.Freight.ICommonContainer>();
			jobShipment = (BusinessObject)Factory.New<Integration.Freight.ICommonShipment>();
			jobConsol = (BusinessObject)Factory.New<Integration.Freight.ICommonConsol>();
			cusSCAHouseBill = (BusinessObject)Factory.New<Integration.Customs.AU.ICusSCAHouse>();
		}

		GlbBranch otherBranch;

		BusinessObject jobContainer;
		BusinessObject jobShipment;
		BusinessObject jobConsol;
		BusinessObject cusSCAHouseBill;
	}
}
