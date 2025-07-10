using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DeclarationDVDTaxWrapperTest : WrapperHelperTest<DeclarationDVDTaxWrapper>
	{
		public void TestBaseUnit()
		{
			var wrapper = new DeclarationDVDTaxWrapper("KGM", ZDecimal.Zero, ZDecimal.Zero);
			AssertEquals("Expected filled BaseUnit", "KGM", wrapper.BaseUnit);
		}

		public void TestBaseQuantity()
		{
			var wrapper = new DeclarationDVDTaxWrapper(ZString.Empty, 10.20m, ZDecimal.Zero);
			AssertEquals("Expected filled BaseQuantity", 10.20m, wrapper.BaseQuantity);
		}

		public void TestBaseAmount()
		{
			var wrapper = new DeclarationDVDTaxWrapper(ZString.Empty, ZDecimal.Zero, 111.222m);
			AssertEquals("Expected filled BaseAmount", 111.222m, wrapper.BaseAmount);
		}

		public void TestGetTaxesList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();

			AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

			entryLine.CL_CustomsValue = 200.45m;

			var fee1 = entryLine.Fees.AddNew();
			fee1.CF_MethodOfCalculation = EntryLineFeeData.MethodOfCalculationPercent;
			fee1.CF_BaseValue = 42.560m;

			var fee2 = entryLine.Fees.AddNew();
			fee2.CF_MethodOfCalculation = "KGM";
			fee2.CF_BaseValue = 25.874m;

			var fee3 = entryLine.Fees.AddNew();
			fee3.CF_MethodOfCalculation = "AAA";
			fee3.CF_BaseValue = 30.963m;

			var taxes = DeclarationDVDTaxWrapper.GetTaxesList(entryLine).ToList();
			AssertEquals("Expected filled Taxes with all fees with method of calculation not % + the baseAmount element", 3, taxes.Count);
			AssertEquals("Expected first Taxes element to only have BaseAmount", 200.45m, taxes[0].BaseAmount);
			AssertEquals("Expected first Taxes element to have empty BaseUnit", ZString.Empty, taxes[0].BaseUnit);
			AssertEquals("Expected first Taxes element to have 0 BaseQuantity", ZDecimal.Zero, taxes[0].BaseQuantity);

			AssertEquals("Expected second Taxes element to have 0 BaseAmount", ZDecimal.Zero, taxes[1].BaseAmount);
			AssertEquals("Expected second Taxes element to have filled BaseUnit", "KGM", taxes[1].BaseUnit);
			AssertEquals("Expected second Taxes element to have filled BaseQuantity", 25.874m, taxes[1].BaseQuantity);

			AssertEquals("Expected third Taxes element to have 0 BaseAmount", ZDecimal.Zero, taxes[2].BaseAmount);
			AssertEquals("Expected third Taxes element to have filled BaseUnit", "AAA", taxes[2].BaseUnit);
			AssertEquals("Expected third Taxes element to have filled BaseQuantity", 30.963m, taxes[2].BaseQuantity);
		}

		protected override DeclarationDVDTaxWrapper GetProvider() => new DeclarationDVDTaxWrapper("KGM", 10.20m, 111.222m);
	}
}
