using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ComponentAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCA_Name()
		{
			cNSC.CA_Category = CNSCCategories.Codes.CNS;
			chemicalSubstance.AddInfoValidation.ValidateCA_Name();
			AssertHasMessageErrorContaining(chemicalSubstance.CA_NameInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(chemicalSubstance.CA_NameInfo, ListValidation.InvalidCodeMessageError);

			chemicalSubstance.CA_Name = "chemical name";
			chemicalSubstance.AddInfoValidation.ValidateCA_Name();
			AssertNoMessageErrorContaining(chemicalSubstance.CA_NameInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(chemicalSubstance.CA_NameInfo, ListValidation.InvalidCodeMessageError);
			chemicalSubstance.CA_Name = NuclearSubstanceSpecificationCodes.Codes.D2O;
			chemicalSubstance.AddInfoValidation.ValidateCA_Name();
			AssertNoMessageErrorContaining(chemicalSubstance.CA_NameInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(chemicalSubstance.CA_NameInfo, ListValidation.InvalidCodeMessageError);
			cNSC.CA_Category = CNSCCategories.Codes.NS;
			chemicalSubstance.CA_Name = "";
			chemicalSubstance.AddInfoValidation.ValidateCA_Name();
			AssertHasMessageErrorContaining(chemicalSubstance.CA_NameInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(chemicalSubstance.CA_NameInfo, ListValidation.InvalidCodeMessageError);
			chemicalSubstance.CA_Name = "chemical name";
			chemicalSubstance.AddInfoValidation.ValidateCA_Name();
			AssertNoMessageErrorContaining(chemicalSubstance.CA_NameInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(chemicalSubstance.CA_NameInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCA_Type()
		{
			var cnscpgaHeader = Factory.New<CNSCPGAHeader>();

			var component = cnscpgaHeader.Components.AddNew();
			AssertNoMessageErrors(component.CA_TypeInfo);

			component.CA_Type = "XX";
			AssertNoMessageErrors(component.CA_TypeInfo);

			var ecccpgaHeader = Factory.New<ECCCPGAHeader>();
			ecccpgaHeader.CA_WENProgramInd = YesNoList.Codes.Yes;
			component = ecccpgaHeader.Components.AddNew();

			Customs.Business.Testing.ValidationTestHelper.AssertInvalidCodeMessageError(component.CA_TypeInfo, "XX", IDTypeCodes.Codes.CV);
		}

		public void TestCheckCA_NameForECCC()
		{
			eCCC.CA_ODSProgramInd = "Y";
			eCCCComponent.AddInfoValidation.ValidateCA_Name();
			AssertHasMessageErrorContaining(eCCCComponent.CA_NameInfo, MandatoryValidation.YouHaveNotEntered);

			eCCCComponent.CA_Name = "name";
			AssertNoMessageErrorContaining(eCCCComponent.CA_NameInfo, MandatoryValidation.YouHaveNotEntered);

			var component = eCCC.Components.AddNew();
			component.AddInfoValidation.ValidateCA_Name();
			AssertHasMessageError(component.CA_NameInfo, "Component collection must have only 1 element(s)");
		}

		public void TestCheckCA_Qty()
		{
			chemicalSubstance.AddInfoValidation.ValidateCA_Qty();
			AssertHasMessageErrorContaining(chemicalSubstance.CA_QtyInfo, MandatoryValidation.YouHaveNotEntered);
			chemicalSubstance.CA_Qty = 1.0m;
			chemicalSubstance.AddInfoValidation.ValidateCA_Qty();
			AssertNoMessageErrorContaining(chemicalSubstance.CA_QtyInfo, MandatoryValidation.YouHaveNotEntered);

			hCComponent.AddInfoValidation.ValidateCA_Qty();
			AssertHasMessageErrorContaining(hCComponent.CA_QtyInfo, MandatoryValidation.YouHaveNotEntered);
			hCComponent.CA_Name = "Test";
			hCComponent.CA_Qty = 0.0m;
			AssertHasMessageErrorContaining(hCComponent.CA_QtyInfo, string.Format(CultureInfo.InvariantCulture, ComponentAddInfoValidation.QuantityAndUQCannotBeBlank, hCComponent.CA_Name));

			hCComponent.CA_Name = "Test";
			hCComponent.CA_Qty = 1.0m;
			hCComponent.AddInfoValidation.ValidateCA_Qty();
			AssertNoMessageErrorContaining(hCComponent.CA_QtyInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(hCComponent.CA_QtyInfo, string.Format(CultureInfo.InvariantCulture, ComponentAddInfoValidation.QuantityAndUQCannotBeBlank, hCComponent.CA_Name));

			hCComponent.CA_Qty = 1.0m;
			hCComponent.AddInfoValidation.ValidateCA_Qty();
			AssertNoMessageErrorContaining(hCComponent.CA_QtyInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(hCComponent.CA_QtyInfo, string.Format(CultureInfo.InvariantCulture, ComponentAddInfoValidation.QuantityAndUQCannotBeBlank, hCComponent.CA_Name));
		}

		public void TestCheckCA_UQ()
		{
			chemicalSubstance.CA_Qty = 10.33m;
			chemicalSubstance.AddInfoValidation.ValidateCA_UQ();
			AssertHasMessageErrorContaining(chemicalSubstance.CA_UQInfo, MandatoryValidation.YouHaveNotEntered);
			chemicalSubstance.CA_UQ = "KGM";
			chemicalSubstance.AddInfoValidation.ValidateCA_UQ();
			AssertNoMessageErrorContaining(chemicalSubstance.CA_UQInfo, MandatoryValidation.YouHaveNotEntered);
			AssertInvalidWarningCore(chemicalSubstance.CA_UQInfo, "AA", "CC", ListValidation.InvalidCodeMessage.ToString());

			ValidationTestHelper.AssertInvalidCodeMessageError(eCCCComponent.CA_UQInfo, "CC", "MGM");
			ValidationTestHelper.AssertInvalidCodeMessageError(eCCCComponent.CA_UQInfo, "AA", "ODK");

			hCComponent.CA_Name = "Test";
			hCComponent.AddInfoValidation.ValidateCA_UQ();
			AssertHasMessageErrorContaining(hCComponent.CA_UQInfo, string.Format(CultureInfo.InvariantCulture, ComponentAddInfoValidation.QuantityAndUQCannotBeBlank, hCComponent.CA_Name));

			hCComponent.CA_Name = "Test";
			hCComponent.CA_UQ = "KGM";
			hCComponent.AddInfoValidation.ValidateCA_UQ();
			AssertNoMessageErrorContaining(hCComponent.CA_UQInfo, string.Format(CultureInfo.InvariantCulture, ComponentAddInfoValidation.QuantityAndUQCannotBeBlank, hCComponent.CA_Name));

			hCComponent.CA_UQ = "KGM";
			hCComponent.AddInfoValidation.ValidateCA_UQ();
			AssertNoMessageErrorContaining(hCComponent.CA_UQInfo, string.Format(CultureInfo.InvariantCulture, ComponentAddInfoValidation.QuantityAndUQCannotBeBlank, hCComponent.CA_Name));
		}

		static void AssertInvalidWarningCore(ZPropertyInfo info, ZString invalidCode, ZString validCode, string invalidNotificationText)
		{
			if (info.PropertyType == typeof(ZString))
			{
				info.Value = invalidCode;
				TestCaseWithFactory.AssertHasWarningContaining(info, invalidNotificationText);
				info.Value = validCode;
				TestCaseWithFactory.AssertNoWarningContaining(info, invalidNotificationText);
			}
		}

		public void TestCheckCA_Origin()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(eCCCComponent.CA_OriginInfo, "~", "AU");
		}

		public void TestCheckCA_Concentration()
		{
			var messageError = "Concentration should not be greater than 100 and the minimum value should not be less than 0";
			var cnscpgaHeader = Factory.New<CNSCPGAHeader>();
			var component = cnscpgaHeader.Components.AddNew();

			component.CA_Concentration = -1m;
			AssertHasMessageErrorContaining(component.CA_ConcentrationInfo, messageError);

			component.CA_Concentration = 105m;
			AssertHasMessageErrorContaining(component.CA_ConcentrationInfo, messageError);

			component.CA_Concentration = 13.66m;
			AssertNoMessageErrorContaining(component.CA_ConcentrationInfo, messageError);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			cNSC = Factory.New<CNSCPGAHeader>();
			chemicalSubstance = cNSC.Components.AddNew();

			eCCC = Factory.New<ECCCPGAHeader>();
			eCCCComponent = eCCC.Components.AddNew();

			hC = Factory.New<HCPGAHeader>();
			hCComponent = hC.Components.AddNew();
		}
		CNSCPGAHeader cNSC;
		Component chemicalSubstance;
		ECCCPGAHeader eCCC;
		Component eCCCComponent;
		HCPGAHeader hC;
		Component hCComponent;
		#endregion
	}
}
