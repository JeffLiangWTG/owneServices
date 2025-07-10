using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DUACompleteImportLineWrapperTest : DUAImportCommonLineWrapperTest
	{
		public void TestREACode()
		{
			invoiceLine.ZG_REAProductCode = WrapperHelperTest<DUACompleteImportLineWrapper>.EntryLineData.ReaCode;
			AssertEquals("Expected filled REACode", WrapperHelperTest<DUACompleteImportLineWrapper>.EntryLineData.ReaCode, wrapper.REACode);
		}

		public void TestPositiveAdjustment()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected filled PositiveAdjustment with 0", 0m, wrapper.PositiveAdjustment);

				SetChargesForAdjustmentCalculation();
				AssertEquals("Expected filled PositiveAdjustment", 33m, wrapper.PositiveAdjustment);

				invoiceLine.Charges.AddNew(ESCustomsChargeTypeList.Codes.ExportedGoodsValueForOutwardProcessing, 33m, declaration.LocalCurrencyCode);
				declaration.ResumeApportionment();

				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
				entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
				wrapper = new DUACompleteImportLineWrapper(entryLine, false);
				AssertEquals("Expected filled PositiveAdjustment with EGV charge added", 66m, wrapper.PositiveAdjustment);
			});
		}

		public void TestNegativeAdjustment()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected filled NegativeAdjustment with 0", 0m, wrapper.NegativeAdjustment);

				SetChargesForAdjustmentCalculation();
				AssertEquals("Expected filled NegativeAdjustment with positive value when negative", 22m, wrapper.NegativeAdjustment);

				var oFT = invoiceLine.Charges.AddNew(UCCCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge, -25m, declaration.LocalCurrencyCode);
				oFT.J7_IsDutiable = false;
				oFT.J7_IsIncludedInITOT = true;
				declaration.ResumeApportionment();

				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
				entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
				wrapper = new DUACompleteImportLineWrapper(entryLine, false);
				AssertEquals("Expected filled NegativeAdjustment with negative value when positive", -3m, wrapper.NegativeAdjustment);
			});
		}

		public void TestStatisticalValue()
		{
			entryLine.CL_StatisticalValue = WrapperHelperTest<DUACompleteImportLineWrapper>.EntryLineData.TotalGoodValue;
			AssertEquals("Expected filled StatisticalValue", WrapperHelperTest<DUACompleteImportLineWrapper>.EntryLineData.TotalGoodValue, wrapper.StatisticalValue);
		}

		protected override void SetUp()
		{
			base.SetUp();

			wrapper = (DUACompleteImportLineWrapper)GetWrapper(entryLine);
		}

		DUACompleteImportLineWrapper wrapper;

		protected override DUAImportCommonLineWrapper GetWrapper(CusEntryLine cusEntryLine) => new DUACompleteImportLineWrapper(cusEntryLine, false);

		void SetChargesForAdjustmentCalculation()
		{
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			var groupHeader = invoice.GroupHeader;

			var oNS = groupHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.BuyingCommissionsCharge, 30m, declaration.LocalCurrencyCode);
			oNS.J7_IsDutiable = true;
			oNS.J7_IsIncludedInITOT = false;
			var oFT = invoiceLine.Charges.AddNew(UCCCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge, 20m, declaration.LocalCurrencyCode);
			oFT.J7_IsDutiable = false;
			oFT.J7_IsIncludedInITOT = true;
			var oL1 = invoiceLine.Charges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 3m, declaration.LocalCurrencyCode);
			oL1.J7_IsDutiable = true;
			oL1.J7_IsIncludedInITOT = false;
			var oL2 = invoiceLine.Charges.AddNew(UCCCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge, 2m, declaration.LocalCurrencyCode);
			oL2.J7_IsDutiable = false;
			oL2.J7_IsIncludedInITOT = true;

			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice.JZ_InvoiceAmount = 100m;
			invoiceLine.JI_LinePrice = 100m;
			declaration.ResumeApportionment();

			AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

			wrapper = new DUACompleteImportLineWrapper(entryLine, false);
		}
	}
}
