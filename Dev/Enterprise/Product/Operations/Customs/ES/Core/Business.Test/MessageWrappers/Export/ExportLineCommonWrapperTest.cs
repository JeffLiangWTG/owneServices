using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class ExportLineCommonWrapperTest : WrapperHelperTest<ExportLineCommonWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("Null Entry Line", () => new ExportLineCommonWrapper(null));
				AssertExceptionThrown<ArgumentOutOfRangeException>("No InvoiceLines", () => new ExportLineCommonWrapper(Factory.New<CusEntryLine>()));
			});
		}

		public void TestGoodsItemNumber()
		{
			AssertEquals("Expected filled GoodsItemNumber", EntryLineData.ItemNumber, wrapper.GoodsItemNumber);
		}

		public void TestGrossWeightInKG()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_Weight = 200.4455M;
				invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
				AssertEquals("Expected filled GrossWeightInKG when weight > 1 rounded to the upper integer unit", 201M, wrapper.GrossWeightInKG);

				invoiceLine.JI_Weight = 0.9886M;
				AssertEquals("Expected filled GrossWeightInKG when weight < 1", 0.989M, wrapper.GrossWeightInKG);

				invoiceLine.JI_Weight = 200.4455M;
				var invLine1 = entryLine.InvoiceLines.AddNew();

				invLine1.JI_Weight = 4;
				invLine1.JI_WeightUQ = Core.Constants.Weight.Kilograms;
				AssertEquals("Expected filled GrossWeightInKG with the sum of gross weights in all invoice lines in entryline", 205M, wrapper.GrossWeightInKG);
			});
		}

		public void TestNetWeightInKG()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
				invoiceLine.JI_NetWeight = 2.3454m;
				invoiceLine.JI_CustomsUnitQty = Core.Constants.Weight.Kilograms;
				invoiceLine.JI_CustomsQuantity = 1.1234m;
				AssertEquals("Expected filled NetWeightInKG with 1 invoice line", 1.123m, wrapper.NetWeightInKG);

				var invLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
				invLine2.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
				invLine2.JI_NetWeight = 2.3454m;
				invLine2.JI_CustomsUnitQty = Core.Constants.Weight.Kilograms;
				invLine2.JI_CustomsQuantity = 2.3211m;
				AssertEquals("Expected filled NetWeightInKG with 2 invoice line", 3.445m, wrapper.NetWeightInKG);

				var invLine3 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
				invLine3.JI_NetWeightUQ = Core.Constants.Weight.Grams;
				invLine3.JI_NetWeight = 2000.000m;
				invLine3.JI_CustomsUnitQty = Core.Constants.Weight.Grams;
				invLine3.JI_CustomsQuantity = 1000.0000m;
				AssertEquals("Expected filled NetWeightInKG with 3 invoice line", 4.445m, wrapper.NetWeightInKG);
			});
		}

		public void TestOtherUnitsNumber()
		{
			invoiceLine.JI_CustomsThirdQuantity = EntryLineData.ThirdQuantity;
			AssertEquals("Expected filled OtherUnitsNumber", EntryLineData.ThirdQuantity, wrapper.OtherUnitsNumber);
		}

		public void TestOtherUnitsQualifier()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateCusMapType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, MapDirectionList.Codes.BTH, "Local Customs Quantity Units", true);
			helper.CreateCusMap(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, EntryLineFeeData.MethodOfCalculationCW1, EntryLineFeeData.MethodOfCalculationCustoms, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), Core.Constants.CountryCodes.Spain);
			Factory.Save();

			CombineAssertions(() =>
			{
				invoiceLine.JI_CustomsThirdUnitQty = EntryLineData.ThirdQtyUnitCW1;
				AssertEquals("Expected filled OtherUnitsQualifier with mapped value", EntryLineData.ThirdQtyUnitCustoms, wrapper.OtherUnitsQualifier);

				invoiceLine.JI_CustomsThirdUnitQty = EntryLineData.ThirdQtyUnitNotMapped;
				AssertEquals("Expected filled OtherUnitsQualifier with original value because the value is not mapped", EntryLineData.ThirdQtyUnitNotMapped, wrapper.OtherUnitsQualifier);
			});
		}

		public void TestTotalGoodValueInEuros()
		{
			entryLine.CL_StatisticalValue = EntryLineData.TotalGoodValue;
			AssertEquals("Expected filled TotalGoodValueInEuros", EntryLineData.TotalGoodValue, wrapper.TotalGoodValueInEuros);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge", true, mergeResult);

			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

			wrapper = new ExportLineCommonWrapper(entryLine);
		}
		JobComInvoiceLine invoiceLine;
		CusEntryLine entryLine;
		ExportLineCommonWrapper wrapper;

		protected override ExportLineCommonWrapper GetProvider() => wrapper;
	}
}
