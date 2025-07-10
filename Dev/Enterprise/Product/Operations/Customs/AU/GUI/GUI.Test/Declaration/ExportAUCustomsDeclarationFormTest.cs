using System;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing;

[TestedType(typeof(ZAUCustomsDeclarationForm))]
public class ExportAUCustomsDeclarationFormTest : AUCustomsDeclarationFormTest
{
	public override ZString MessageTypeForFormBashing => Common.Shared.SharedJobMessageTypeList.Codes.Export;

	protected override BaseJobComInvoiceLine CreateInvoiceLineForPerformanceTest(BaseJobComInvoiceHeader invoiceHeader, int index, int invoiceIndex)
	{
		InitialiseTariffs();

		var invoiceLine = (JobComInvoiceLine)invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine.JI_Tariff = ExportTariffs[(index + (invoiceIndex * 10)) % ExportTariffs.Length];
		return invoiceLine;
	}

	protected ZString[] ExportTariffs
	{
		get
		{
			return new ZString[] {
				"0104.20.00", "0202.30.11", "0303.31.00", "0405.10.00", "0506.90.00", "0603.10.41", "0709.30.00", "0805.10.10", "0909.30.00", "1008.30.00",
				"1108.19.00", "1209.99.90", "1302.14.01", "1403.00.02", "1504.30.10", "1604.15.00", "1702.60.00", "1806.32.90", "1904.10.11", "2004.90.21",
				"2106.90.21", "2204.21.10", "2302.50.00", "2403.10.90", "2516.12.00", "2616.90.00", "2707.60.00", "2812.10.10", "2903.69.00", "3006.70.00",
				"3104.10.00", "3206.49.00", "3301.90.01", "3402.90.90", "3504.00.00", "3604.90.00", "3702.53.00", "3806.90.02", "3901.10.00", "4002.59.00",
				"4104.41.29", "4205.00.00", "4302.19.20", "4407.91.00", "4502.00.00", "4602.90.00", "4701.00.00", "4802.61.00", "4901.91.00", "5003.10.00",
				"5101.30.40", "5205.28.00", "5311.00.00", "5401.10.00", "5503.10.00", "5608.90.00", "5705.00.00", "5804.10.00", "5908.00.00", "6002.40.00",
				"6104.39.00", "6203.19.02", "6302.10.00", "6403.59.00", "6506.10.00", "6603.20.00", "6701.00.00", "6704.90.00", "6801.00.00", "6914.90.00",
				"7001.00.00", "7103.99.13", "7202.92.00", "7305.19.00", "7408.29.00", "7501.20.00", "7607.19.00", "7616.99.00", "7806.00.00", "7907.00.00",
				"8001.10.00", "8108.30.00", "8202.99.00", "8304.00.00", "8404.90.00", "8509.40.00", "8606.20.00", "8705.20.20", "8803.10.00", "8908.00.00",
				"9005.10.00", "9106.20.00", "9204.20.00", "9307.00.00", "9405.92.00", "9506.39.90", "9608.10.00", "9706.00.00", "9801.00.00", "9902.10.10" };
		}
	}

	bool initialised;
	void InitialiseTariffs()
	{
		if (!initialised)
		{
			initialised = true;

			var refDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = refDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Export);

			foreach (var tariffCode in ExportTariffs)
			{
				refDataHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, tariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Tariff " + tariffCode, taxOrFeeCode: "GST");
			}
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		useCustomsReferenceDataRegItem = AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, UseCustomsReferenceDataValue);
	}

	protected override void TearDown()
	{
		base.TearDown();
		useCustomsReferenceDataRegItem?.Dispose();
	}

	IDisposable useCustomsReferenceDataRegItem;
	protected virtual bool UseCustomsReferenceDataValue => true;
}
