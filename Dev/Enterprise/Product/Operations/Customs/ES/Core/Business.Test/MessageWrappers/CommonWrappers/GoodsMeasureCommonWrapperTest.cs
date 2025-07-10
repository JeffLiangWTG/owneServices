using System;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class GoodsMeasureCommonWrapperTest : WrapperHelperTest<GoodsMeasureCommonWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if entryLine is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","entryLine"), () => new GoodsMeasureCommonWrapper(null));
		}

		public void TestGrossWeight()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
				invoiceLine.JI_Weight = 1.124m;
				wrapper = GetWrapper(entryLine);
				AssertEquals("Expected filled GrossWeight with 1 invoice line", 1.124m, wrapper.GrossWeight);

				var invLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
				invLine2.JI_WeightUQ = Core.Constants.Weight.Kilograms;
				invLine2.JI_Weight = 2.321m;
				wrapper = GetWrapper(entryLine);
				AssertEquals("Expected filled GrossWeight with 2 invoice lines", 3.445m, wrapper.GrossWeight);

				var invLine3 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
				invLine3.JI_WeightUQ = Core.Constants.Weight.Grams;
				invLine3.JI_Weight = 1000.0000m;
				wrapper = GetWrapper(entryLine);
				AssertEquals("Expected filled GrossWeight with 3 invoice lines", 4.445m, wrapper.GrossWeight);

				wrapper = new GoodsMeasureCommonWrapper(40.123456m, 20.987654m);
				AssertEquals("Expected filled GrossWeight with given gross mass in the constructor", 40.123456m, wrapper.GrossWeight);
			});
		}

		public void TestNetWeight()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_CustomsUnitQty = Core.Constants.Weight.Kilograms;
				invoiceLine.JI_CustomsQuantity = 1.124m;
				wrapper = GetWrapper(entryLine);
				AssertEquals("Expected filled NetWeight with 1 invoice line", 1.124m, wrapper.NetWeight);

				var invLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
				invLine2.JI_CustomsUnitQty = Core.Constants.Weight.Kilograms;
				invLine2.JI_CustomsQuantity = 2.321m;
				wrapper = GetWrapper(entryLine);
				AssertEquals("Expected filled NetWeight with 2 invoice lines", 3.445m, wrapper.NetWeight);

				var invLine3 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
				invLine3.JI_CustomsUnitQty = Core.Constants.Weight.Grams;
				invLine3.JI_CustomsQuantity = 1000.0000m;
				wrapper = GetWrapper(entryLine);
				AssertEquals("Expected filled NetWeight with 3 invoice lines", 4.445m, wrapper.NetWeight);

				wrapper = new GoodsMeasureCommonWrapper(40.123456m, 20.987654m);
				AssertEquals("Expected filled NetWeight with given net mass in the constructor", 20.987654m, wrapper.NetWeight);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();

			AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

			wrapper = GetWrapper(entryLine);
		}
		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
		CusEntryLine entryLine;
		GoodsMeasureCommonWrapper wrapper;

		GoodsMeasureCommonWrapper GetWrapper(CusEntryLine entryLine) => new GoodsMeasureCommonWrapper(entryLine);

		protected override GoodsMeasureCommonWrapper GetProvider() => wrapper;
	}
}
