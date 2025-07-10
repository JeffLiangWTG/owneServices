using System;
using System.Linq;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DeclarationAESCommodityWrapperTest : WrapperHelperTest<DeclarationAESCommodityWrapper>
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

		public void TestGoodsDescription()
		{
			invoiceLine.JI_Description = "description\r\nof\ngoods";
			CombineAssertions(() =>
			{
				AssertEquals("Expected filled GoodsDescription", "description of goods", wrapper.GoodsDescription);

				invoiceLine.JI_Description = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
				var expectedTrimDescription = "12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012";
				AssertEquals("Expected filled GoodsDescription", expectedTrimDescription, wrapper.GoodsDescription);
			});
		}

		public void TestCusCode()
		{
			invoiceLine.ZG_CusNumber = "0010111-1";
			AssertEquals("Expected filled CusCode", "0010111-1", wrapper.CusCode);
		}

		public void TestCommodityCode()
		{
			var commodityCode = wrapper.CommodityCode;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled CommodityCode", commodityCode);
				AssertSame("Cached CommodityCode", wrapper.CommodityCode, commodityCode);
			});
		}

		public void TestDangerousGoods()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty DangerousGoods", 0, wrapper.DangerousGoods.Count);

				var subs = Factory.New<UNDGSubstance>();
				subs.DG_Code = "1001";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
				invoiceLine.UNDGs.FirstItemForBinding[0].LinkDefault(subs);

				wrapper = GetWrapper(entryLine);
				var dangerousGoods = wrapper.DangerousGoods;
				AssertEquals("Expected filled DangerousGoods", 1, dangerousGoods.Count);
				AssertSame("Cached DangerousGoods", wrapper.DangerousGoods, dangerousGoods);

				var dangerousGoodsList = dangerousGoods.ToList();
				AssertEquals("Expected filled DangerousGoods, SequenceNumber", "1", dangerousGoodsList[0].SequenceNumber);
				AssertEquals("Expected filled DangerousGoods, UNDangerousCode", "1001", dangerousGoodsList[0].UNDangerousCode);
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
			invoiceLine = invoice.InvoiceLines.AddNew();

			AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

			wrapper = GetWrapper(entryLine);
		}

		JobComInvoiceLine invoiceLine;
		CusEntryLine entryLine;
		DeclarationAESCommodityWrapper wrapper;

		DeclarationAESCommodityWrapper GetWrapper(CusEntryLine entryLine) => new DeclarationAESCommodityWrapper(entryLine);

		protected override DeclarationAESCommodityWrapper GetProvider() => wrapper;
	}
}
