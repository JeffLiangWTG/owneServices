using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class ComplXAESGoodsShipmentWrapperTest : WrapperHelperTest<ComplXAESGoodsShipmentWrapper>
	{
		public void TestNatureOfTransaction()
		{
			invoiceHeader.JZ_ValuationCode = "21";
			AssertEquals("Expected filled NatureOfTransaction", "21", wrapper.NatureOfTransaction);
		}

		public void TestDeliveryTerms()
		{
			CombineAssertions(() =>
			{
				var deliveryTerms = wrapper.DeliveryTerms;
				AssertNotNull("Expected filled DeliveryTerms", deliveryTerms);
				AssertSame("Cached DeliveryTerms", wrapper.DeliveryTerms, deliveryTerms);
			});
		}

		public void TestConsignment()
		{
			var consignment = wrapper.Consignment;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Consignment", consignment);
				AssertSame("Cached Consignment", wrapper.Consignment, consignment);
			});
		}

		public void TestLines()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected 1 Line (mandatory at least one)", 1, wrapper.Lines.Count);

				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_Tariff = "2203001011";

				var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine3.JI_Tariff = "2203001012";

				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

				entryHeader = declaration.CustomsEntryHeaders[0];
				wrapper = GetWrapper(entryHeader);

				var lines = wrapper.Lines;

				AssertEquals("Expected 3 Lines", 3, lines.Count);
				AssertSame("Cached Lines", wrapper.Lines, lines);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "11";

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);

			entryHeader = declaration.CustomsEntryHeaders[0];

			wrapper = GetWrapper(entryHeader);
		}

		JobDeclaration declaration;

		JobComInvoiceHeader invoiceHeader;
		CusEntryHeader entryHeader;
		ComplXAESGoodsShipmentWrapper wrapper;

		ComplXAESGoodsShipmentWrapper GetWrapper(CusEntryHeader entryHeader) => new ComplXAESGoodsShipmentWrapper(entryHeader);

		protected override ComplXAESGoodsShipmentWrapper GetProvider() => wrapper;
	}
}
