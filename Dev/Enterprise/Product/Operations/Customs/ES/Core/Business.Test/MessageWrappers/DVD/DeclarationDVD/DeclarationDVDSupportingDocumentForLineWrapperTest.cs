using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DeclarationDVDSupportingDocumentForLineWrapperTest : WrapperHelperTest<DeclarationDVDSupportingDocumentForLineWrapper>
	{
		public void TestUnitOfMeasure()
		{
			document.CSI_UnitOfQuantity = "KGM";
			AssertEquals("Expected filled UnitOfMeasure", "KGM", wrapper.UnitOfMeasure);
		}

		public void TestQuantity()
		{
			document.CSI_Quantity = 2.3m;
			AssertEquals("Expected filled Quantity", 2.3m, wrapper.Quantity);
		}

		public void TestCurrency()
		{
			document.CSI_RX_NKCurrency = "EUR";
			AssertEquals("Expected filled Currency", "EUR", wrapper.Currency);
		}

		public void TestAmount()
		{
			document.CSI_Value = 2.3m;
			AssertEquals("Expected filled Amount", 2.3m, wrapper.Amount);
		}

		protected override void SetUp()
		{
			base.SetUp();
			document = Factory.New<SupportingDocument>();
			wrapper = new DeclarationDVDSupportingDocumentForLineWrapper(document);
		}

		SupportingDocument document;
		DeclarationDVDSupportingDocumentForLineWrapper wrapper;

		protected override DeclarationDVDSupportingDocumentForLineWrapper GetProvider() => wrapper;
	}
}
