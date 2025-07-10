using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.Testing.UtilityClasses;

namespace Enterprise.Accounting.Business.Testing
{
	class DocBuilderSection_InvoiceRecipient_Dates_TransactionReferencesTest : TestCaseWithFactory
	{
		public void TestSectionContains_RecipientConsumptionTaxRegimeDescription_And_RecipientConsumptionTaxRegimeHeading_Macros()
		{
			var systemTemplate = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System);
			var excelWorkSheet = ConfigurableTemplateTestHelper.GetExcelWorkSheetFromSection(systemTemplate, "Invoice Recipient + Dates + Transaction References");

			var expectedRightHandAddressContent =
@"{C}-[REGISTRATION #]   {K}-[<ARInvoice.RecipientTaxIDNumberInRecipientCountry>]   {BU}-[<HideRowIf(""<ARInvoice.RecipientTaxIDNumberInRecipientCountry>""=="""" || ""<ARInvoice.RecipientTaxIDNumberInRecipientCountry>""==""<ARInvoice.RecipientTaxIDNumber>""|| ""<ARInvoice.RecipientTaxIDNumberInRecipientCountry>""==""<ARInvoice.RecipientLocalBusinessRegNumber>"")>]
{BU}-[<HideRowIf(""<ARInvoice.RecipientTaxIDNumberInRecipientCountry>""=="""" || ""<ARInvoice.RecipientTaxIDNumberInRecipientCountry>""==""<ARInvoice.RecipientTaxIDNumber>""|| ""<ARInvoice.RecipientTaxIDNumberInRecipientCountry>""==""<ARInvoice.RecipientLocalBusinessRegNumber>"")>]
{BU}-[<HideRowIf(""<ARInvoice.RecipientConsumptionTaxRegimeDescription>"" == """")>]
{C}-[<ShrinkToFit><ARInvoice.RecipientConsumptionTaxRegimeHeading>]   {K}-[<ShrinkToFit><ARInvoice.RecipientConsumptionTaxRegimeDescription>]   {BU}-[<HideRowIf(""<ARInvoice.RecipientConsumptionTaxRegimeDescription>"" == """")>]
{BU}-[<HideRowIf(""<ARInvoice.RecipientConsumptionTaxRegimeDescription>"" == """")>]
{BU}-[<HideRowIf(""<ARInvoice.RecipientLocalState>""=="""" || ""<CompanyCountryCode>"" != ""IN"")>]";

			var expectedLeftHandAddressContent =
@"{AG}-[REGISTRATION #]   {AO}-[<ARInvoice.RecipientTaxIDNumberInRecipientCountry>]   {BU}-[<HideRowIf(""<ARInvoice.RecipientTaxIDNumberInRecipientCountry>""=="""" || ""<ARInvoice.RecipientTaxIDNumberInRecipientCountry>""==""<ARInvoice.RecipientTaxIDNumber>""|| ""<ARInvoice.RecipientTaxIDNumberInRecipientCountry>""==""<ARInvoice.RecipientLocalBusinessRegNumber>"")>]
{BU}-[<HideRowIf(""<ARInvoice.RecipientTaxIDNumberInRecipientCountry>""=="""" || ""<ARInvoice.RecipientTaxIDNumberInRecipientCountry>""==""<ARInvoice.RecipientTaxIDNumber>""|| ""<ARInvoice.RecipientTaxIDNumberInRecipientCountry>""==""<ARInvoice.RecipientLocalBusinessRegNumber>"")>]
{BU}-[<HideRowIf(""<ARInvoice.RecipientConsumptionTaxRegimeDescription>"" == """")>]
{AG}-[<ShrinkToFit><ARInvoice.RecipientConsumptionTaxRegimeHeading>]   {AO}-[<ShrinkToFit><ARInvoice.RecipientConsumptionTaxRegimeDescription>]   {BU}-[<HideRowIf(""<ARInvoice.RecipientConsumptionTaxRegimeDescription>"" == """")>]
{BU}-[<HideRowIf(""<ARInvoice.RecipientConsumptionTaxRegimeDescription>"" == """")>]
{BU}-[<HideRowIf(""<ARInvoice.RecipientLocalState>""=="""" || ""<CompanyCountryCode>"" != ""IN"")>]";

			var excelWorkSheetContent = excelWorkSheet.ToString();

			AssertContainsInOrder("The section should contain <ARInvoice.RecipientConsumptionTaxRegimeHeading> and <ARInvoice.RecipientConsumptionTaxRegimeDescription> macros in #LeftHandAddress area", excelWorkSheetContent, "#LeftHandAddress", expectedLeftHandAddressContent);
			AssertContainsInOrder("The section should contain <ARInvoice.RecipientConsumptionTaxRegimeHeading> and <ARInvoice.RecipientConsumptionTaxRegimeDescription> macros in #RightHandAddress area", excelWorkSheetContent, "#RightHandAddress", expectedRightHandAddressContent);
		}
	}
}
