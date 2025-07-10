using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare.Testing
{
	[TestedType(typeof(ProfitShareDetailCollection))]
	public class ProfitShareDetailCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ProfitShareDetailCollection>
	{
		#region Retrieval

		public void TestGetProfitShareForOrg()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			OrgHeader org2 = Factory.New<OrgHeader>();

			ProfitShareDetailCollection profitShares = new ProfitShareDetailCollection();
			AssertNull(profitShares.GetProfitShareForOrg(org1));
			AssertNull(profitShares.GetProfitShareForOrg(org2));

			profitShares.Add(new ProfitShareDetail(org1, Factory, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent));
			AssertNotNull(profitShares.GetProfitShareForOrg(org1));
			AssertNull(profitShares.GetProfitShareForOrg(org2));

			profitShares.Add(new ProfitShareDetail(org2, Factory, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent));
			AssertNotNull(profitShares.GetProfitShareForOrg(org1));
			AssertNotNull(profitShares.GetProfitShareForOrg(org2));
		}

		#endregion

		#region Implementation

		protected override ProfitShareDetailCollection GetCollectionToTest()
		{
			return new ProfitShareDetailCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ProfitShareDetail(OrgHeader.New(Factory), Factory, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent);
		}

		#endregion
	}
}
