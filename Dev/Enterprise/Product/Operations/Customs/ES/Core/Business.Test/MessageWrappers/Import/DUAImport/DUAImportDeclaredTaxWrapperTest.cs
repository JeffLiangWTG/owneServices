using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.Universal;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DUAImportDeclaredTaxWrapperTest : WrapperHelperTest<DUAImportDeclaredTaxWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null CusEntryLineFee", () => new DUAImportDeclaredTaxWrapper(null, false));
		}

		public void TestTaxClass()
		{
			entryLineFee.CF_ChargeType = EntryLineFeeData.ChargeType;

			CombineAssertions(() =>
			{
				AssertEquals("Expected filled TaxClass", EntryLineFeeData.ChargeType, wrapper.TaxClass);

				wrapper = new DUAImportDeclaredTaxWrapper(entryLineFee, true);
				AssertEquals("Expected filled TaxClass with 3IG", EntryLineFeeData.ChargeTypeCanary, wrapper.TaxClass);
			});
		}

		public void TestTaxableIncome()
		{
			entryLineFee.CF_BaseValue = EntryLineFeeData.BaseValue;
			AssertEquals("Expected filled TaxableIncome", EntryLineFeeData.BaseValue, wrapper.TaxableIncome);
		}

		public void TestTaxRate()
		{
			entryLineFee.CF_Rate = EntryLineFeeData.Rate;
			AssertEquals("Expected filled TaxRate", EntryLineFeeData.Rate, wrapper.TaxRate);
		}

		public void TestMaxMinIndicator()
		{
			entryLineFee.MaxMin = EntryLineFeeData.MaxMin;
			AssertEquals("Expected filled MaxMinIndicator", EntryLineFeeData.MaxMin, wrapper.MaxMinIndicator);
		}

		public void TestFiscalUnit()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, MapDirectionList.Codes.BTH, "Description", false);
			helper.CreateCusMap(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, EntryLineFeeData.MethodOfCalculationCW1, EntryLineFeeData.MethodOfCalculationCustoms, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), CountryCodes.Spain);
			Factory.Save();

			CombineAssertions(() =>
			{
				entryLineFee.CF_MethodOfCalculation = ZString.Empty;
				wrapper = new DUAImportDeclaredTaxWrapper(entryLineFee, false);
				AssertEquals("Expected filled FiscalUnit with % if empty method of calculation", EntryLineFeeData.MethodOfCalculationPercent, wrapper.FiscalUnit);

				entryLineFee.CF_MethodOfCalculation = EntryLineFeeData.MethodOfCalculationPercent;
				wrapper = new DUAImportDeclaredTaxWrapper(entryLineFee, false);
				AssertEquals("Expected filled FiscalUnit with % if method of calculation is %", EntryLineFeeData.MethodOfCalculationPercent, wrapper.FiscalUnit);

				entryLineFee.CF_MethodOfCalculation = EntryLineFeeData.MethodOfCalculationCW1;
				wrapper = new DUAImportDeclaredTaxWrapper(entryLineFee, false);
				AssertEquals("Expected filled FiscalUnit with mapped value if method of calculation is not % or empty", EntryLineFeeData.MethodOfCalculationCustoms, wrapper.FiscalUnit);

				entryLineFee.CF_MethodOfCalculation = EntryLineFeeData.MethodOfCalculationNotMapped;
				wrapper = new DUAImportDeclaredTaxWrapper(entryLineFee, false);
				AssertEquals("Expected filled FiscalUnit with original value if method of calculation is not % or empty but the value is not mapped", EntryLineFeeData.MethodOfCalculationNotMapped, wrapper.FiscalUnit);

				entryLineFee.CF_MethodOfCalculation = EntryLineFeeData.MethodOfCalculationPVP;
				wrapper = new DUAImportDeclaredTaxWrapper(entryLineFee, false);
				AssertEquals("Expected filled FiscalUnit with % if method of calculation is PVP", EntryLineFeeData.MethodOfCalculationPercent, wrapper.FiscalUnit);
			});
		}

		public void TestFee()
		{
			entryLineFee.CF_ChargeAmount = EntryLineFeeData.ChargeAmount;
			AssertEquals("Expected filled Fee", EntryLineFeeData.ChargeAmount, wrapper.Fee);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			entryLineFee = entryLine.Fees.AddNew();
			wrapper = new DUAImportDeclaredTaxWrapper(entryLineFee, false);
		}

		CusEntryLineFee entryLineFee;
		DUAImportDeclaredTaxWrapper wrapper;

		protected override DUAImportDeclaredTaxWrapper GetProvider() => wrapper;
	}
}
