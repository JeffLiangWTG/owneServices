using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	class EMCSJobComInvoiceHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestJZ_InvoiceNumber_OptionalIfConsolidatedDocument()
		{
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.InvoiceNumberInfo);
				declaration.SetConsolidatedDocument();
				ValidationTestHelper.AssertFieldIsNotMandatory(declaration.InvoiceNumberInfo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
		}
		EMCSJobDeclaration declaration;
	}
}
