using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobDeclarationGoodsLocationValidationTest : TestCaseWithFactory
{
	public void TestLocationOfGoodsIsMandatory()
	{
		var expectedErrorMessage = "You have not entered a Location of Goods (C0392)";
		var entryInstruction = exportDeclaration.CustomsEntryInstructions.AddNew();
		exportDeclaration.CustomsEntryInstructions.AddNew().CEI_SubStyle = "D";

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(exportDeclaration, true))
		{
			AssertEquals("[PRE-CONDITION] GoodsLocationDescription", "", exportDeclaration.GoodsLocationDescription);
			exportDeclaration.ValidateGoodsLocationDescription();
			AssertHasMessageErrorContaining(exportDeclaration.GoodsLocationDescriptionInfo, expectedErrorMessage);

			entryInstruction.CEI_SubStyle = "E";
			exportDeclaration.ValidateGoodsLocationDescription();
			AssertNoMessageErrorContaining(exportDeclaration.GoodsLocationDescriptionInfo, expectedErrorMessage);

			entryInstruction.CEI_SubStyle = "";
			exportDeclaration.GoodsLocation.CGL_Qualifier = "Y";
			AssertEquals("[PRE-CONDITION] GoodsLocationDescription", "Y", exportDeclaration.GoodsLocationDescription);
			exportDeclaration.ValidateGoodsLocationDescription();
			AssertNoMessageErrorContaining(exportDeclaration.GoodsLocationDescriptionInfo, expectedErrorMessage);
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(exportDeclaration, false))
		{
			entryInstruction.CEI_SubStyle = "";
			exportDeclaration.GoodsLocation.CGL_Qualifier = "";
			exportDeclaration.ValidateGoodsLocationDescription();
			AssertNoMessageErrorContaining(exportDeclaration.GoodsLocationDescriptionInfo, expectedErrorMessage);
		}

		exportDeclaration.JE_MessageType = "IMP";
		AssertNoExceptionThrown("Calling ValidateGoodsLocationDescription when declaration is Import", () => exportDeclaration.ValidateGoodsLocationDescription());
	}

	public void TestReplicateGoodsLocationNotificationsInGoodsLocationDescription()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(exportDeclaration, true))
		{
			var goodsLocation = exportDeclaration.GoodsLocation;
			goodsLocation.CGL_Qualifier = "Y";
			goodsLocation.CGL_Type = "B";

			goodsLocation.CGL_AdditionalIdentifier = "";
			AssertHasMessageErrorContaining("[PRE-CONDITION]", goodsLocation.CGL_AdditionalIdentifierInfo, MandatoryValidation.YouHaveNotEntered);

			exportDeclaration.ValidateGoodsLocationDescription();
			const string expectedMessageError = "There are errors within the 'Location of Goods', please click on 'More..' to view the error information.";
			AssertHasMessageErrorContaining(exportDeclaration.GoodsLocationDescriptionInfo, expectedMessageError);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		exportDeclaration = Factory.New<JobDeclaration>();
		exportDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
	}

	JobDeclaration exportDeclaration;
}
