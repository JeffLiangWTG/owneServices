using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class SetUpDefaultsForNewChildInvoiceCollectionTest : TestCaseWithFactory
	{
		public void TestDefaultsForAdditionalInvoiceCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var firstInvoice = declaration.Invoices.AddNew();
			AssertEquals(firstInvoice.JZ_ValuationCode, ZString.Empty);
			firstInvoice.JZ_ValuationCode = "11";

			var secondInvoice = declaration.Invoices.AddNew();
			AssertEquals(secondInvoice.JZ_ValuationCode, "11");
			secondInvoice.JZ_ValuationCode = "12";

			var thirdInvoice = declaration.Invoices.AddNew();
			AssertEquals(thirdInvoice.JZ_ValuationCode, "12");
		}

		public void TestDefaultForNewElementCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			Assert("Precondition: INCO term on shipment is emtpy.", declaration.JE_ShipmentIncoTerm.IsEmpty);

			var invoice1 = declaration.Invoices.AddNew();
			Assert("INCO term on invoice header is emtpy as the one on parent is emtpy.", invoice1.JZ_IncoTerm.IsEmpty);

			declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			var invoice2 = declaration.Invoices.AddNew();
			AssertEquals("INCO term on invoice header should copy from parent.", Core.Constants.IncoTerms.FreeOnBoard, invoice2.JZ_IncoTerm);
		}

		public void TestDefaultIncoTermPlaceCode_WhenIsUcc6()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.EUD_AgreedPlaceCode = "EUXXX";
				declaration.JE_ShipmentIncoTermPlace = "ABC";

				var invoice = declaration.Invoices.AddNew();
				CombineAssertions("When Is UCC6", () =>
				{
					AssertEquals(nameof(invoice.JZ_IncoTermPlace), "EUXXX", invoice.ZG_AgreedPlaceCode);
					AssertEquals(nameof(invoice.ZG_AgreedPlaceCode), "ABC", invoice.JZ_IncoTermPlace);
				});
			}
		}

		public void TestDefaultIncoTermPlaceCode_WhenIsNotUcc6()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				declaration.ZG_AgreedPlaceCode = "ABC";
				declaration.JE_ShipmentIncoTermPlace = "3";

				var invoice = declaration.Invoices.AddNew();
				CombineAssertions("When Is not UCC6", () =>
				{
					AssertEquals(nameof(invoice.ZG_AgreedPlaceCode), "ABC", invoice.ZG_AgreedPlaceCode);
					AssertEquals(nameof(invoice.JZ_IncoTermPlace), "3", invoice.JZ_IncoTermPlace);
				});
			}
		}
	}
}
