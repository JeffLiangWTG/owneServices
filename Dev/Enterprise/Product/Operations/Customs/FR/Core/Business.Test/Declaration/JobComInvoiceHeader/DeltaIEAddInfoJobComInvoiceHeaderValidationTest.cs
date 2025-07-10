using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	sealed class DeltaIEAddInfoJobComInvoiceHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZG_AgreedPlaceCode()
		{
			var messageError = "Incoterm Place Code or Country Code is required";

			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				declaration.ZG_AgreedPlaceCode = "2";
				AssertNoMessageErrorContaining("No message error is expected when ZG_AgreedPlaceCode value is set.", declaration.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining("No message error is expected when ZG_AgreedPlaceCode value is set.", declaration.ZG_AgreedPlaceCodeInfo, messageError);

				declaration.ZG_AgreedPlaceCode = ZString.Empty;
				AssertHasMessageErrorContaining("A message error is expected for UCC6 and export declarations when ZG_AgreedPlaceCode value is empty", declaration.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining("A message error is expected for UCC6 and export declarations when ZG_AgreedPlaceCode value is empty", declaration.ZG_AgreedPlaceCodeInfo, messageError);

				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				declaration.ZG_AgreedPlaceCode = ZString.Empty;
				AssertNoMessageErrorContaining("No validation is expected for UCC6 and import declarations.", declaration.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining("No validation is expected for UCC6 and import declarations.", declaration.ZG_AgreedPlaceCodeInfo, messageError);
			}
		}

		public void TestCheckZG_AgreedPlaceCode_SameAsIncoTermOnInvoice()
		{
			var messageErrorOrWarning = "Incoterm place code values do not match between Declaration and Invoice Header.";

			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfigurationAndReturnMock(declaration, "IsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoiceCore", true), true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				var invoiceHeader = declaration.Invoices.AddNew();
				declaration.ZG_AgreedPlaceCode = "1";
				invoiceHeader.ZG_AgreedPlaceCode = "2";
				AssertHasMessageError("A message error is expected for UCC6 and export declaration when Incoterm place code values do not match between Declaration and Invoice Header.", invoiceHeader.ZG_AgreedPlaceCodeInfo, messageErrorOrWarning);
				invoiceHeader.ZG_AgreedPlaceCode = "1";
				AssertNoMessageErrorContaining("No message error is expected when Incoterm place code values match between Declaration and Invoice Header.", invoiceHeader.ZG_AgreedPlaceCodeInfo, messageErrorOrWarning);

				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				declaration.ZG_AgreedPlaceCode = "1";
				invoiceHeader.ZG_AgreedPlaceCode = "2";
				AssertHasWarning("A warning is expected for UCC6 and import declaration when Incoterm place code values do not match between Declaration and Invoice Header.", invoiceHeader.ZG_AgreedPlaceCodeInfo, messageErrorOrWarning);
				invoiceHeader.ZG_AgreedPlaceCode = "1";
				AssertNoWarningContaining("No warning is expected when Incoterm place code values match between Declaration and Invoice Header.", invoiceHeader.ZG_AgreedPlaceCodeInfo, messageErrorOrWarning);
			}
		}
	}
}
