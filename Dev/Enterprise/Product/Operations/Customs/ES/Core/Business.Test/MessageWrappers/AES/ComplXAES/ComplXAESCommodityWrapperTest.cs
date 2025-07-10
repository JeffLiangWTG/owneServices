using System;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class ComplXAESCommodityWrapperTest : WrapperHelperTest<ComplXAESCommodityWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if entryLine is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","entryLine"), () => GetWrapper(null));

				var entryLine = Factory.New<CusEntryLine>();
				AssertExceptionThrown("Constructor Throws Exception if InvoiceLines is null", typeof(ArgumentOutOfRangeException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value '0' cannot be less than or equal to 0.","InvoiceLines"), () => GetWrapper(entryLine));
			});
		}

		public void TestGoodsMeasure()
		{
			var goodsMeasure = wrapper.GoodsMeasure;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled GoodsMeasure", goodsMeasure);
				AssertSame("Cached GoodsMeasure", wrapper.GoodsMeasure, goodsMeasure);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();

			AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

			wrapper = GetWrapper(entryLine);
		}

		CusEntryLine entryLine;
		ComplXAESCommodityWrapper wrapper;

		ComplXAESCommodityWrapper GetWrapper(CusEntryLine entryLine) => new ComplXAESCommodityWrapper(entryLine);

		protected override ComplXAESCommodityWrapper GetProvider() => wrapper;
	}
}
