using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EDIOrgOpportunityValue))]
	public class EDIOrgOpportunityValueTest : OrgOpportunityValueTest
	{
		[ExpectNoExceptions]
		public void TestDeleteOfMaintenanceItemPriorToSalesItemDoesNotCauseMaintenanceItemToBeReAdded()
		{
			EDIOrgOpportunity opp = Factory.New<EDIOrgOpportunity>();
			opp.P8_OH = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			EDIOrgOpportunityValue value1 = opp.ValueItems.AddNew();
			value1.PV_RevenueType = "SAL";
			value1.PV_Value = 500m;
			opp.ValueItems.Sort(new SortInfo(OrgOpportunityValueSchema.PV_RevenueType.Name, ListSortDirection.Ascending));
			AssertEquals("Precondition - MAI must be first for this error to occur", "MAI", opp.ValueItems[0].PV_RevenueType);
			AssertEquals("Precondition - MAI must be first for this error to occur", "SAL", opp.ValueItems[1].PV_RevenueType);

			Factory.Save();
			opp.Delete();
			Factory.Save();
		}

		public void TestSalesValueSetsMaintenanceValue()
		{
			OrgOpportunity opp = Factory.New<OrgOpportunity>();
			EDIOrgOpportunityValue value1 = (EDIOrgOpportunityValue)opp.ValueItems.AddNew();

			value1.PV_RevenueType = "SAL";
			AssertEquals("Only 1 item", 1, opp.ValueItems.Count);

			value1.PV_Value = 500m;
			AssertEquals("maintenance item added ", 2, opp.ValueItems.Count);
			AssertEquals("Added as 20% of sales", 100m, opp.ValueItems[1].PV_Value);

			value1.PV_DiscountPercent = 50m;
			AssertEquals(2, opp.ValueItems.Count);
			AssertEquals("Changing discount on sales doesn't changes maintenance as maint is based on non-discounted value", 100m, opp.ValueItems[1].PV_Value);

			opp.ValueItems.RemoveAndDelete(opp.ValueItems[1]);

			EDIOrgOpportunityValue value2 = (EDIOrgOpportunityValue)opp.ValueItems.AddNew();
			value2.PV_RevenueType = "MAI";
			AssertEquals("Maintenance added manually", 2, opp.ValueItems.Count);

			value1.PV_Value = 1000m;
			value1.PV_DiscountPercent = 0m;
			AssertEquals("Maintenance not re-added", 2, opp.ValueItems.Count);
			AssertEquals("Existing maintenance item updated", 200m, value2.PV_Value);

			value1.PV_DiscountPercent = 30m;
			AssertEquals("Discount not taken into account", 200m, value2.PV_Value);

			value1.PV_Value = 2000m;
			AssertEquals("Maintenance re-calculated", 400m, value2.PV_Value);

			value1.PV_RevenueType = "REN";
			value1.PV_Value = 4000m;
			AssertEquals("Non SALES item doesn't affect maintenance item", 400m, value2.PV_Value);
		}

		public override void TestValue()
		{
			Assert("This test is not applicable to the subclass", true);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			OrgOpportunity opp = Factory.New<OrgOpportunity>();
			return opp.ValueItems.AddNew();
		}
	}
}
