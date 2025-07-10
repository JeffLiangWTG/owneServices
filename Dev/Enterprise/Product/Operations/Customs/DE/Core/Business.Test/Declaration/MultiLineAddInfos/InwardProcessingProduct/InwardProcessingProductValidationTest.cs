using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Customs.Business.Testing.ValidationTestHelper;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	public class InwardProcessingProductValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Tariff_ValidFormat()
		{
			const string theCnCodeMustHave8Digits = "The CN Code must have 8 digits.";
			CombineAssertions(() =>
			{
				product.FormattedTariff = "1234567";
				AssertHasMessageError("Length is not 8", product.CSI_TariffInfo, theCnCodeMustHave8Digits);
				product.FormattedTariff = "1234567A";
				AssertHasMessageError("Contains the character", product.CSI_TariffInfo, theCnCodeMustHave8Digits);
				product.FormattedTariff = "12345678";
				AssertNoMessageError("Valid format", product.CSI_TariffInfo, theCnCodeMustHave8Digits);
			});
		}

		public void TestCheckCSI_Tariff_ValidCode()
		{
			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);
			const string eun = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var tariffType = helper.CreateNewOrGetExistingTariffType(eun, "XX");
			factory.Save();
			helper.CreateTariff(eun, tariffType.PK, "12345678", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			factory.Save();

			const string theEnteredCnCodeIsNotValid = "The entered CN Code is not valid.";
			CombineAssertions(() =>
			{
				product.FormattedTariff = "12341234";
				AssertHasMessageError("Invalid", product.CSI_TariffInfo, theEnteredCnCodeIsNotValid);
				product.FormattedTariff = "12345678";
				AssertNoMessageError("Valid", product.CSI_TariffInfo, theEnteredCnCodeIsNotValid);
			});
		}

		public void TestCheckCSI_Description_Mandatory()
		{
			product.FormattedTariff = "12345678";
			AssertYouHaveNotEnteredMessageError(product.CSI_DescriptionInfo);
		}

		public void TestCheckCSI_SubType_MandatoryAndValid()
		{
			product.FormattedTariff = "12345678";
			AssertInvalidCodeOrEmptyMessageError(product.CSI_SubTypeInfo, "#", YieldTypeList.Codes.B);
		}

		public void TestCheckCSI_AdditionalDescription_Mandatory()
		{
			product.FormattedTariff = "12345678";
			AssertYouHaveNotEnteredMessageError(product.CSI_AdditionalDescriptionInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var instruction = Factory.CreateInwardProcessingInstruction();
			instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.J;
			var invoiceHeader = instruction.JobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			product = invoiceLine.InwardProcessingProducts.AddNew();
		}

		InwardProcessingProduct product;
	}
}
