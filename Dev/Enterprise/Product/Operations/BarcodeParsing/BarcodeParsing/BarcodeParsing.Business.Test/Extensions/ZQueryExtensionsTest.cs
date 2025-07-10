using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	class ZQueryExtensionsTest : BarcodeParsingTestCase
	{
		#region TestAddGuidFilterOrIsNullIfGuidIsEmpty

		public void TestAddGuidFilterOrIsNullIfGuidIsEmpty()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var buyer2 = Helper.CreateOrg("Buyer2");

			var ruleSet1 = Helper.CreateRuleSet(buyer1);
			var ruleSet2 = Helper.CreateRuleSet(buyer2);
			var ruleSet3 = Helper.CreateRuleSet(null);

			AssertResults(buyer1.PK, ruleSet1);
			AssertResults(buyer2.PK, ruleSet2);

			// Adding a Guid.Empty in the filter should result in a check for 'Guid Column IS NULL', which would return ResultSet3 which has No Buyer.			
			AssertResults(ZGuid.Empty, ruleSet3);
		}

		void AssertResults(ZGuid buyerPK, BarcodeRuleSet expectedRuleSet)
		{
			var query = new ZQuery();
			query.AddGuidFilterOrIsNullIfGuidIsEmpty(BarcodeRuleSetSchema.BRS_OH_Buyer, buyerPK);
			query.AddToFilter(BarcodeRuleSetSchema.BRS_IsSystem, false);

			AssertContainsExactElementsInAnyOrder(new[] { expectedRuleSet }, Factory.Load<BarcodeRuleSet>(query));
		}

		#endregion
	}
}
