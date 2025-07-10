using Enterprise.Customs.Business.Testing;
using static Enterprise.Customs.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class CustomsQuantityConverterTest : BaseCustomsQuantityConverterTest
	{
		public void TestCalculateFromNetWeightToCustomsQtyCoreProducesCorrectResultWhenMappingOfUnitsIsRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var customsQuantityConverter = invoiceLine.CustomsQuantityConverter;

			invoiceLine.JI_NetWeight = 100m;
			invoiceLine.JI_CustomsUnitQty = RefCusCodeList.CustomsUq.Weight.Kilogram;

			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Grams;
			AssertEquals(0.1m, customsQuantityConverter.CalculateFromNetWeightToCustomsQtyCore());

			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Milligrams;
			AssertEquals(0.0001m, customsQuantityConverter.CalculateFromNetWeightToCustomsQtyCore());

			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Ounces;
			AssertEquals(2.834952m, customsQuantityConverter.CalculateFromNetWeightToCustomsQtyCore());
		}

		public void TestCustomsWeightIsMappedToWeightUnitIfPossible()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AssertEquals(Core.Constants.Weight.Kilograms, invoiceLine.CustomsQuantityConverter.GetEffectiveWeightUnit(RefCusCodeList.CustomsUq.Weight.Kilogram));
			AssertEquals(Core.Constants.Weight.Tonnes, invoiceLine.CustomsQuantityConverter.GetEffectiveWeightUnit(RefCusCodeList.CustomsUq.Weight.Tonne));
		}
	}
}
