using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class InvestigationItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestINV_Description()
		{
			var item = Factory.NewWithValidTestData<InvestigationItem>();
			item.INV_Description = string.Empty;
			AssertHasError(item.INV_DescriptionInfo, "Please enter a Description.");
			item.INV_Description = "This is description";
			AssertNoError(item.INV_DescriptionInfo, "Please enter a Description.");
		}

		public void TestINV_Type()
		{
			var item = Factory.NewWithValidTestData<InvestigationItem>();
			item.INV_Type = string.Empty;
			AssertHasError(item.INV_TypeInfo, "Please enter a Type.");
			item.INV_Type = "SVC";
			AssertHasError(item.INV_TypeInfo, "Enter a valid Type.");
			item.INV_Type = InvestigationItemTypes.Codes.Question;
			AssertNoError(item.INV_TypeInfo, "Enter a valid Type.");
			item.INV_Type = InvestigationItemTypes.Codes.MaterialRequest;
			AssertNoError(item.INV_TypeInfo, "Enter a valid Type.");
		}

		public void TestINV_ItemText()
		{
			var item = Factory.NewWithValidTestData<InvestigationItem>();
			item.INV_ItemText = string.Empty;
			AssertHasError(item.INV_ItemTextInfo, "Please enter an Item Text.");
			item.INV_ItemText = "This is Item Text.";
			AssertNoError(item.INV_ItemTextInfo, "Please enter an Item Text.");
		}
	}
}
