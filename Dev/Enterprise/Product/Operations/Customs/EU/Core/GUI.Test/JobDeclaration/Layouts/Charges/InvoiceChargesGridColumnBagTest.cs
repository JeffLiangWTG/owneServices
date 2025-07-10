using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing;

[TestedType(typeof(InvoiceChargesGridColumnBag))]
sealed class InvoiceChargesGridColumnBagTest : TestCaseWithFactory
{
	public void TestAmountInLocalCurrencyCalcEditColumn()
	{
		var company = Factory.New<GlbCompany>();
		company.GC_Code = "C01";
		company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Poland;
		company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Poland;
		company.Branches.AddNew().GB_IsActive = true;
		Factory.Save();

		using (DisposableEnvironment.ForCompany("C01"))
		{
			AssertNotEquals("Pre-condition: should be EU company with non-EUR currency", Core.Constants.CurrencyCodes.EuropeanUnion, GlbCompany.CurrentCompany.CustomsCurrency.Code);

			AssertNotNull(ColumnBag.AmountInLocalCurrencyCalcEditColumn);

			var columnInfo = ColumnBag.AmountInLocalCurrencyCalcEditColumn.CreateGridColumnInfo();

			CombineAssertions(() =>
			{
				AssertType<ZCalcEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", nameof(IEUCommonInvoiceCharge.AmountInLocalCurrency), columnInfo.ColumnName);
				AssertEquals("Width", 80, columnInfo.Width);
				AssertEquals("Caption", $"{Core.Constants.CurrencyCodes.Poland} Value", columnInfo.CaptionResourceString.Caption);
			});
		}
	}

	public void TestAmountCorrectionCalcEditColumn()
	{
		AssertNotNull(ColumnBag.AmountCorrectionCalcEditColumn);

		var columnInfo = ColumnBag.AmountCorrectionCalcEditColumn.CreateGridColumnInfo();

		CombineAssertions(() =>
		{
			AssertType<ZCalcEditColumnStyleInfo>(columnInfo);
			AssertEquals("ColumnName", nameof(IEUCommonInvoiceCharge.AmountCorrection), columnInfo.ColumnName);
			AssertEquals("Width", 100, columnInfo.Width);
		});
	}

	InvoiceChargesGridColumnBag ColumnBag => InvoiceChargesGridColumnBag.Instance;
}
