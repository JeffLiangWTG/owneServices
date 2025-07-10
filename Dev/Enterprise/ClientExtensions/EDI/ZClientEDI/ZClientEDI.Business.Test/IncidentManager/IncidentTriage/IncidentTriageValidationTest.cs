using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class IncidentTriageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestIMT_Type()
		{
			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			triage.IMT_Type = string.Empty;
			AssertHasError(triage.IMT_TypeInfo, "Please enter a Node Type.");

			triage.IMT_Type = IncidentTriageTypes.Codes.Support;
			AssertNoErrors(triage.IMT_TypeInfo);
			triage.IMT_Type = "INV";
			AssertHasError(triage.IMT_TypeInfo, "Enter a valid Node Type.");
			triage.IMT_Type = IncidentTriageTypes.Codes.Service;
			AssertNoErrors(triage.IMT_TypeInfo);
			triage.IMT_Type = IncidentTriageTypes.Codes.Compliance;
			AssertNoErrors(triage.IMT_TypeInfo);
		}

		public void TestIMT_Level()
		{
			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			triage.IMT_Level = string.Empty;
			AssertHasError(triage.IMT_LevelInfo, "Please enter a Node Level.");

			triage.IMT_Level = IncidentTriageLevels.Codes.PreliminaryAssignment;
			AssertNoErrors(triage.IMT_LevelInfo);
			triage.IMT_Level = "X";
			AssertHasError(triage.IMT_LevelInfo, "Enter a valid Node Level.");
			triage.IMT_Level = IncidentTriageLevels.Codes.Diagnostics;
			AssertNoErrors(triage.IMT_LevelInfo);
		}

		public void TestCheckProductDetails()
		{
			var errorMessage = "Product, Product Area, and Sec./Svc./Req. settings cannot be partially populated. You can either fill in all three fields, leave them all empty, or provide a value only for the Product field.";
			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			triage.IMT_Product = "";
			triage.IMT_ProductArea = "";
			triage.IMT_Module = "";

			Assert(string.IsNullOrEmpty(triage.IMT_Product) && string.IsNullOrEmpty(triage.IMT_ProductArea) && string.IsNullOrEmpty(triage.IMT_Module));
			AssertNoError("There should be no errors if all properties are empty",triage.IMT_ProductInfo, errorMessage);
			AssertNoError("There should be no errors if all properties are empty",triage.IMT_ProductAreaInfo, errorMessage);
			AssertNoError("There should be no errors if all properties are empty", triage.IMT_ModuleInfo, errorMessage);

			//There should be no errors if only Product is not empty
			triage.IMT_Product = "ENT";
			Assert(!string.IsNullOrEmpty(triage.IMT_Product));
			Assert(string.IsNullOrEmpty(triage.IMT_ProductArea) && string.IsNullOrEmpty(triage.IMT_Module));
			AssertNoError(triage.IMT_ProductInfo, errorMessage);
			AssertNoError(triage.IMT_ProductAreaInfo, errorMessage);
			AssertNoError(triage.IMT_ModuleInfo, errorMessage);

			triage.IMT_ProductArea = "ARC";
			Assert(!string.IsNullOrEmpty(triage.IMT_Product));
			Assert(!string.IsNullOrEmpty(triage.IMT_ProductArea));
			Assert(string.IsNullOrEmpty(triage.IMT_Module));
			AssertHasError(triage.IMT_ProductInfo, errorMessage);
			AssertHasError(triage.IMT_ProductAreaInfo, errorMessage);
			AssertHasError(triage.IMT_ModuleInfo, errorMessage);

			triage.IMT_Module = "RDB";
			Assert(!string.IsNullOrEmpty(triage.IMT_Product));
			Assert(!string.IsNullOrEmpty(triage.IMT_ProductArea));
			Assert(!string.IsNullOrEmpty(triage.IMT_Module));
			AssertNoError("There should be no errors if all properties are not empty", triage.IMT_ProductInfo, errorMessage);
			AssertNoError("There should be no errors if all properties are not empty", triage.IMT_ProductAreaInfo, errorMessage);
			AssertNoError("There should be no errors if all properties are not empty", triage.IMT_ModuleInfo, errorMessage);

			triage.IMT_Product = "ENT";
			triage.IMT_ProductArea = "";
			triage.IMT_Module = "";
			Assert(!string.IsNullOrEmpty(triage.IMT_Product));
			Assert(string.IsNullOrEmpty(triage.IMT_ProductArea));
			Assert(string.IsNullOrEmpty(triage.IMT_Module));
			AssertNoError("There should be no errors if only Product is not empty", triage.IMT_ProductInfo, errorMessage);
			AssertNoError("There should be no errors if only Product is not empty", triage.IMT_ProductAreaInfo, errorMessage);
			AssertNoError("There should be no errors if only Product is not empty", triage.IMT_ModuleInfo, errorMessage);
		}

		public void TestIMT_SupportDescription()
		{
			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			triage.IMT_SupportDescription = string.Empty;
			AssertHasError(triage.IMT_SupportDescriptionInfo, "Please enter a Support Description.");
		}

		public void TestPublishedDescriptionShouldBeMandatoryIfIMT_IsPublished()
		{
			var triage = Factory.NewWithValidTestData<IncidentTriage>();

			AssertEquals("Precondition: Default should be false", false, triage.IMT_IsPublished);
			AssertEquals("Precondition", true, triage.PublishedDescriptionText.IsEmpty);
			AssertNoErrors("Empty published description is allowed", triage.PublishedDescriptionTextInfo);

			triage.IMT_IsPublished = true;
			AssertHasError("Published description must be entered", triage.PublishedDescriptionTextInfo, "Please enter a Published Description.");

			triage.PublishedDescriptionText = "Hello";
			AssertNoErrors("Entering published description should remove error", triage.PublishedDescriptionTextInfo);

			triage.IMT_IsPublished = true;
			AssertNoErrors("Published description should be allowed when not published", triage.PublishedDescriptionTextInfo);
		}

		public void TestCheckIMT_Module()
		{
			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			triage.IMT_Module = "ABC";
			triage.IMT_SetProductAreaByMenuItem = true;
			triage.IMT_Module = "";
			AssertHasError(triage.IMT_ModuleInfo, "Please enter a Sec./Svc./Req..");

			triage.IMT_SetProductAreaByMenuItem = false;
			triage.IMT_Module = "ABC";
			triage.IMT_Module = "";
			AssertNoError(triage.IMT_ModuleInfo, "Please enter a Sec./Svc./Req..");
		}
	}
}
