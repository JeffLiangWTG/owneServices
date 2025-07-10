using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class VisualizerMenuItemValidationTest : TestCaseWithFactory
	{
		public void TestValidateSU_PrimaryDocPackItemId()
		{
			var menuItem = Factory.New<VisualizerMenuItem>();
			var validation = new VisualizerMenuItemValidation(menuItem);

			validation.ValidateSU_PrimaryDocPackItemId();
			AssertNoErrors("SU_PrimaryDocPackItemId", menuItem.SU_PrimaryDocPackItemIdInfo);

			var document1 = menuItem.Documents.AddNew();
			document1.SI_SU = menuItem.PK;

			validation.ValidateSU_PrimaryDocPackItemId();
			AssertNoErrors("SU_PrimaryDocPackItemId", menuItem.SU_PrimaryDocPackItemIdInfo);

			var document2 = menuItem.Documents.AddNew();
			document2.SI_SU = menuItem.PK;

			validation.ValidateSU_PrimaryDocPackItemId();
			AssertHasError("SU_PrimaryDocPackItemId", menuItem.SU_PrimaryDocPackItemIdInfo,
				"Since you have more than one document, you have to select a primary document.");

			menuItem.SU_PrimaryDocPackItemId = ZGuid.NewZGuid();

			validation.ValidateSU_PrimaryDocPackItemId();
			AssertHasError("SU_PrimaryDocPackItemId", menuItem.SU_PrimaryDocPackItemIdInfo,
				"The Primary Document was not found within the document pack");

			menuItem.SU_PrimaryDocPackItemId = document1.PK;

			validation.ValidateSU_PrimaryDocPackItemId();
			AssertNoErrors("SU_PrimaryDocPackItemId", menuItem.SU_PrimaryDocPackItemIdInfo);
		}

		public void TestValidateSU_FilterList()
		{
			const string errorMessage = "Macro has the following compilation errors";

			var menuItem = Factory.New<VisualizerMenuItem>();
			var validation = new VisualizerMenuItemValidation(menuItem);

			validation.ValidateAll();
			AssertNoRowErrorContaining(menuItem, errorMessage);

			menuItem.SU_FilterList = "bad macro";

			validation.ValidateAll();
			AssertHasRowErrorContaining(menuItem, errorMessage);

			menuItem.SU_FilterList = "1 == 1";

			validation.ValidateAll();
			AssertNoRowErrorContaining(menuItem, errorMessage);
		}

		public void TestValidateSU_DeliveryRestrictionMacro()
		{
			var menuItem = Factory.New<VisualizerMenuItem>();
			var validation = new VisualizerMenuItemValidation(menuItem);

			menuItem.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.NON);
			validation.ValidateSU_DeliveryRestrictionMacro();
			Assert("No Error expected", !menuItem.SU_DeliveryRestrictionMacroInfo.HasErrors());

			menuItem.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.CNH);
			validation.ValidateSU_DeliveryRestrictionMacro();
			Assert("No Error expected", !menuItem.SU_DeliveryRestrictionMacroInfo.HasErrors());

			menuItem.SU_DeliveryRestrictionMacro = "JK_TransportMode==\"SEA\"";
			Assert("No Error expected", !menuItem.SU_DeliveryRestrictionMacroInfo.HasErrors());

			menuItem.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.UDF);
			menuItem.SU_DeliveryRestrictionMacro = "#$#$";
			AssertHasError(menuItem.SU_DeliveryRestrictionMacroInfo, "Syntax error at line 1 position 5.");

			menuItem.SU_DeliveryRestrictionMacro = "1 == 1";
			Assert("No Error expected", !menuItem.SU_DeliveryRestrictionMacroInfo.HasErrors());
		}

		public void TestValidateSU_EmailSubjectLine()
		{
			const string errorMessage = "Macro has the following compilation errors";

			var menuItem = Factory.New<VisualizerMenuItem>();
			var validation = new VisualizerMenuItemValidation(menuItem);

			validation.ValidateAll();
			AssertNoErrorContaining(menuItem.SU_EmailSubjectLineInfo, errorMessage);
			AssertNoRowErrorContaining(menuItem, errorMessage);

			menuItem.SU_EmailSubjectLine = "<>>";

			validation.ValidateAll();
			AssertHasErrorContaining(menuItem.SU_EmailSubjectLineInfo, errorMessage);

			menuItem.SU_EmailSubjectLine = "ABC: <ABC>";

			validation.ValidateAll();
			AssertNoErrorContaining(menuItem.SU_EmailSubjectLineInfo, errorMessage);
		}
	}
}
