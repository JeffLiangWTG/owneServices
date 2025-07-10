using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.US;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class JobComInvoiceLineIDutyAndTaxDataTest : TestCaseWithFactory
	{
		public void TestSuspendedMarkApportionmentDirtyWhenGetCurrConv()
		{
			var dec = invoiceLine.Declaration;
			dec.ResumeApportionment();
			Assert(!dec.ApportionmentDirty);

			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.RateDescCalcOnly;
			invoiceLine.CA_CustomsValueOvr = false;
			invoiceLine.CA_CVforCurrConvOvr = false;
			dec.ResumeApportionment();
			Assert(!dec.ApportionmentDirty);
		}

		public void TestDutyAndTaxManagerIsDisposedOnDelete()
		{
			var dutyAndTaxManager = invoiceLine.DutyAndTaxManager;
			AssertEquals(false, dutyAndTaxManager.isDisposed);
			invoiceLine.Delete();
			AssertEquals(true, dutyAndTaxManager.isDisposed);
		}

		public void TestIDutyAndTaxDataProperties()
		{
			var dutyAndTaxData = (IDutyAndTaxData)invoiceLine;
			AssertEquals("CalculationMethod", invoiceLine.CA_CalculationMethod, dutyAndTaxData.CalculationMethod);

			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.DutyDeferral;
			AssertEquals("CalculationMethod", CalculationMethods.Codes.NoRemission, dutyAndTaxData.CalculationMethod);

			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.NoRemission;

			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.SoftwareRemission;
			invoiceLine.JI_ParentID = ZGuid.NewZGuid();
			AssertEquals("CalculationMethod", CalculationMethods.Codes.NoRemission, dutyAndTaxData.CalculationMethod);

			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.NoRemission;

			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.RepairsRemission;
			invoiceLine.JI_ParentID = ZGuid.NewZGuid();
			AssertEquals("CalculationMethod", CalculationMethods.Codes.NoRemission, dutyAndTaxData.CalculationMethod);

			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.NoRemission;
			AssertEquals("CalculationMethod", CalculationMethods.Codes.NoRemission, dutyAndTaxData.CalculationMethod);

			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.DutyDeferral;
			AssertEquals("CalculationMethod", CalculationMethods.Codes.DutyDeferral, dutyAndTaxData.CalculationMethod);

			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.OneOneTwentiethRemission;
			AssertEquals("CalculationMethod", CalculationMethods.Codes.OneOneTwentiethRemission, dutyAndTaxData.CalculationMethod);

			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.WarrantyRepairsRemission;
			AssertEquals("CalculationMethod", CalculationMethods.Codes.WarrantyRepairsRemission, dutyAndTaxData.CalculationMethod);

			AssertEquals("ExchangeRate", invoiceHeader.JZ_InvoiceCurrExRate, dutyAndTaxData.ExchangeRate);
			AssertEquals("EffectiveDutyDate", invoiceHeader.EffectiveValuationDate, dutyAndTaxData.EffectiveDutyDate);

			AssertEquals("TreatmentCode", invoiceLine.CA_TreatmentCode, dutyAndTaxData.TreatmentCode);
			invoiceLine.CA_TreatmentCode = ZString.Empty;
			AssertEquals("TreatmentCode", invoiceHeader.CA_TreatmentCode, dutyAndTaxData.TreatmentCode);

			AssertEquals("FOBValue", invoiceLine.JI_Calc_FOB, dutyAndTaxData.FOBValue);
			AssertEquals("ClassificationNumber", invoiceLine.JI_Tariff, dutyAndTaxData.ClassificationNumber);
			AssertEquals("TariffCode", invoiceLine.CA_99TariffCode, dutyAndTaxData.TariffCode);
			AssertEquals("AdjustmentCode", invoiceLine.CA_ADJCode, dutyAndTaxData.AdjustmentCode);
			AssertEquals("AdjustmentValue", invoiceLine.CA_ADJValue, dutyAndTaxData.AdjustmentValue);
			AssertEquals("ValueForCurrencyConversion", invoiceLine.CA_CVforCurrConv, dutyAndTaxData.ValueForCurrencyConversion);
			AssertEquals("CustomsValue", invoiceLine.CA_CustomsValue, dutyAndTaxData.CustomsValue);
			AssertEquals("MonthlyTimeLimit", invoiceHeader.CA_TimeLimit, dutyAndTaxData.MonthlyTimeLimit);
			AssertEquals("CustomsQuantity", invoiceLine.JI_CustomsQuantity, dutyAndTaxData.CustomsQuantity);
			AssertEquals("CustomsUnits", invoiceLine.JI_CustomsUnitQty, dutyAndTaxData.CustomsUnits);
			AssertEquals("CustomsQuantity2", invoiceLine.JI_CustomsSecondQuantity, dutyAndTaxData.CustomsQuantity2);
			AssertEquals("CustomsUnits2", invoiceLine.JI_CustomsSecondUnitQty, dutyAndTaxData.CustomsUnits2);
			AssertEquals("CustomsQuantity3", invoiceLine.JI_CustomsThirdQuantity, dutyAndTaxData.CustomsQuantity3);
			AssertEquals("CustomsUnits3", invoiceLine.JI_CustomsThirdUnitQty, dutyAndTaxData.CustomsUnits3);

			AssertEquals("CA_CustomsValue", 30m, invoiceLine.CA_CustomsValue);
			invoiceLine.CA_CustomsValueOvr = false;
			invoiceLine.Declaration.ResumeApportionment();
			AssertNotEquals("CA_CustomsValue", 30m, invoiceLine.CA_CustomsValue);
			AssertEquals("CA_CustomsValue", invoiceLine.DutyAndTaxManager.GetCustomsValueForDuty(), invoiceLine.CA_CustomsValue);

			AssertEquals("CA_CVforCurrConv", 20m, invoiceLine.CA_CVforCurrConv);
			invoiceLine.CA_CVforCurrConvOvr = false;
			invoiceLine.Declaration.ResumeApportionment();
			AssertNotEquals("CA_CVforCurrConv", 20m, invoiceLine.CA_CVforCurrConv);
			AssertEquals("CA_CVforCurrConv", invoiceLine.DutyAndTaxManager.GetValueForCurrencyConversion(), invoiceLine.CA_CVforCurrConv);
			AssertEquals("CA_ValueForTax", invoiceLine.DutyAndTaxManager.CalculatedValueForTax, invoiceLine.CA_ValueForTax);
		}

		public void TestDeliveredDutyPaidFOBAndCIF()
		{
			invoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 30);
			invoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 38.06m);
			invoiceLine.Declaration.ResumeApportionment();
			AssertEquals("JI_Calc_FOB", 50m, invoiceLine.JI_Calc_FOB);
			AssertEquals("JI_Calc_CIF", 160m, invoiceLine.JI_Calc_CIF);

			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.DeliveredDutyPaid;
			invoiceLine.CA_CustomsValueOvr = false;
			invoiceLine.CA_CVforCurrConvOvr = false;

			invoiceLine.CA_ADJCode = AmountTypes.Codes.Dollar;
			invoiceLine.CA_ADJValue = 20;
			invoiceLine.Declaration.ResumeApportionment();
			AssertEquals("CA_CVforCurrConv", 70m, invoiceLine.CA_CVforCurrConv);
			AssertEquals("JI_Calc_FOB", 50m, invoiceLine.JI_Calc_FOB);
			AssertEquals("JI_Calc_CIF", 160m, invoiceLine.JI_Calc_CIF);

			invoiceLine.CA_ADJCode = AmountTypes.Codes.Percent;
			invoiceLine.CA_ADJValue = 10;
			invoiceLine.Declaration.ResumeApportionment();
			AssertEquals("CA_CVforCurrConv", 55m, invoiceLine.CA_CVforCurrConv);
			AssertEquals("JI_Calc_FOB", 50m, invoiceLine.JI_Calc_FOB);
			AssertEquals("JI_Calc_CIF", 160m, invoiceLine.JI_Calc_CIF);
		}

		public void TestB3SubHeaderLineLevelProperties()
		{
			AssertEquals("B3SubHeaderNumber", 1, invoiceLine.CA_B3SubHeaderNumber);
			AssertEquals("FreightCharges", 121.94m, invoiceLine.FreightCharges);

			AssertEquals("EffectiveCountryAndStateOfOrigin", Core.Constants.CountryCodes.Canada, invoiceLine.EffectiveCountryAndStateOfOrigin);
			AssertEquals("CountryAndStateOfOrigin", Core.Constants.CountryCodes.Canada, invoiceLine.CountryAndStateOfOrigin);
			invoiceLine.JI_CountryOfOrigin = ZString.Empty;
			AssertEquals("EffectiveCountryAndStateOfOrigin", "UAL", invoiceLine.EffectiveCountryAndStateOfOrigin);
			AssertEquals("CountryAndStateOfOrigin", "UAL", invoiceLine.CountryAndStateOfOrigin);

			AssertEquals("EffectiveCountryAndStateOfExport", Core.Constants.CountryCodes.Canada, invoiceLine.EffectiveCountryAndStateOfExport);
			AssertEquals("CountryAndStateOfExport", Core.Constants.CountryCodes.Canada, invoiceLine.CountryAndStateOfExport);
			invoiceLine.CA_RN_NKExport = ZString.Empty;
			AssertEquals("EffectiveCountryAndStateOfExport", "UAL", invoiceLine.EffectiveCountryAndStateOfExport);
			AssertEquals("CountryAndStateOfExport", "", invoiceLine.CountryAndStateOfExport);

			AssertEquals("EffectiveTreatmentCode", TariffTreatmentCodes.Codes.Chile, invoiceLine.EffectiveTreatmentCode);
			invoiceLine.CA_TreatmentCode = ZString.Empty;
			AssertEquals("EffectiveTreatmentCode", TariffTreatmentCodes.Codes.UnitedStates, invoiceLine.EffectiveTreatmentCode);
		}

		public void TestPivotProperties()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PARTNUM";
			var supRelation = part.RelatedOrganisations.AddSupplier(Factory.New<MasterFiles.Business.OrgHeader>());
			var importPivot = part.PivotsForBinding.AddNew();
			importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			importPivot.CI_TariffNum = "1234567890";
			importPivot.CCA_GSTStatusCode = "12";
			importPivot.CCA_ETExemption = "56";
			importPivot.CCA_ETRateCode = "E99";

			invoiceLine.InvoiceHeader.JZ_OH_Supplier = supRelation.OU_OH;
			invoiceLine.JI_PartNo = "PARTNUM";
			AssertNotNull(invoiceLine.Part);

			IDutyAndTaxData data = invoiceLine;
			AssertNotNull(data);
			AssertEquals("DefaultGSTStatusCode", "12", data.DefaultGSTStatusCode);
			AssertEquals("DefaultETExemptionCode", "56", data.DefaultETExemptionCode);
			AssertEquals("DefaultETRateCode", "E99", data.DefaultETRateCode);
		}

		public void TestDefaultGSTStatusCode_WarrantyRepairLine()
		{
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.WarrantyRepairsRemission;

			var data = invoiceLine as IDutyAndTaxData;
			AssertEquals("DefaultGSTStatusCode", "66", data.DefaultGSTStatusCode);
		}

		public void TestDefaultGSTAndETRtAndETEx_Classification()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PARTNUM";
			var supRelation = part.RelatedOrganisations.AddSupplier(Factory.New<MasterFiles.Business.OrgHeader>());
			var importPivot = part.PivotsForBinding.AddNew();
			importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;

			var classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CC_TariffNum = "1234567890";

			var caClassification = classification.Details;
			caClassification.CCA_GSTStatusCode = "48";
			caClassification.CCA_ETRateCode = "E01";
			caClassification.CCA_ETExemption = "85";
			caClassification.CCA_ParentID = classification.PK;
			caClassification.CCA_ParentTableCode = classification.TablePrefix;

			importPivot.CI_CC = classification.PK;

			invoiceLine.InvoiceHeader.JZ_OH_Supplier = supRelation.OU_OH;
			invoiceLine.JI_PartNo = "PARTNUM";
			AssertNotNull(invoiceLine.Part);

			var data = invoiceLine as IDutyAndTaxData;
			AssertNotNull(data);
			AssertEquals("DefaultGSTStatusCode", "48", data.DefaultGSTStatusCode);
			AssertEquals("DefaultETExemptionCode", "85", data.DefaultETExemptionCode);
			AssertEquals("DefaultETRateCode", "E01", data.DefaultETRateCode);

			importPivot.CI_TariffNum = "1234567890";
			importPivot.CCA_GSTStatusCode = "12";
			importPivot.CCA_ETExemption = "56";
			importPivot.CCA_ETRateCode = "E99";

			var line2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			line2.JI_PartNo = "PARTNUM";
			AssertNotNull(line2.Part);

			data = line2;
			AssertNotNull(data);
			AssertEquals("DefaultGSTStatusCode", "12", data.DefaultGSTStatusCode);
			AssertEquals("DefaultETExemptionCode", "56", data.DefaultETExemptionCode);
			AssertEquals("DefaultETRateCode", "E99", data.DefaultETRateCode);

			var line3 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			line3.JI_CC = classification.PK;

			data = line2;
			AssertNotNull(data);
			AssertEquals("DefaultGSTStatusCode", "12", data.DefaultGSTStatusCode);
			AssertEquals("DefaultETExemptionCode", "56", data.DefaultETExemptionCode);
			AssertEquals("DefaultETRateCode", "E99", data.DefaultETRateCode);
		}

		public void TestDutyDateWhenLvxAttachToLvs()
		{
			JobDeclaration lvsJob = Factory.New<JobDeclaration>();
			lvsJob.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			Factory.Save();

			JobDeclaration lvxJob = Factory.New<JobDeclaration>();
			lvxJob.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			lvxJob.JE_EntryAuthorisationDate = ZDateTime.BrettsBirthday;
			JobComInvoiceHeader invoice = lvxJob.LVXInvoiceHeader;
			invoice.JZ_InvoiceNumber = "INV123";
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(invoice, lvsJob);

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10m;
			invoiceLine.JI_Tariff = "1234567890";
			invoiceLine.CA_99TariffCode = "4901";
			invoiceLine.JI_CustomsUnitQty = TariffTreatmentCodes.Codes.NewZealand;
			invoiceLine.JI_CustomsQuantity = 10;
			lvsJob.ResumeApportionment();

			using (new BaseJobDeclaration.InvoicesOverrideDeclarationSupporter(lvsJob))
			{
				var dutyAndTaxData = (IDutyAndTaxData)invoiceLine;
				AssertEquals("EffectiveDutyDate", ZDateTime.BrettsBirthday, dutyAndTaxData.EffectiveDutyDate);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			var usd = Enterprise.MasterFiles.Business.RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);
			usd.SetCustomsRate(new ZDateTime(2000, 1, 1), ZDateTime.MaxSmallDateTimeValue, 0.719424m);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
			invoiceHeader.JZ_InvoiceCurrExRate = 10;
			invoiceHeader.CA_TimeLimit = 10;
			invoiceHeader.CA_TreatmentCode = TariffTreatmentCodes.Codes.UnitedStates;
			invoiceHeader.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceHeader.JZ_RW_NKOriginState = USStatesList.Codes.Alabama;
			invoiceHeader.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
			invoiceHeader.CA_USStateOfExport = USStatesList.Codes.Alabama;
			invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.CA_B3SubHeaderNumber = 1;
			invoiceLine.JI_LinePrice = 20;
			invoiceLine.JI_Tariff = "22101000";
			invoiceLine.CA_99TariffCode = "4901";
			invoiceLine.CA_ADJCode = AmountTypes.Codes.Percent;
			invoiceLine.CA_ADJValue = 10;
			invoiceLine.CA_CVforCurrConvOvr = true;
			invoiceLine.CA_CVforCurrConv = 20;
			invoiceLine.CA_CustomsValueOvr = true;
			invoiceLine.CA_CustomsValue = 30;
			invoiceLine.JI_CustomsQuantity = 40;
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Kilogram;
			invoiceLine.JI_CustomsSecondQuantity = 50;
			invoiceLine.JI_CustomsSecondUnitQty = CustomsUnitOfMeasureList.Codes.Gram;
			invoiceLine.JI_CustomsThirdQuantity = 60;
			invoiceLine.JI_CustomsThirdUnitQty = CustomsUnitOfMeasureList.Codes.Litre;
			invoiceLine.CA_TreatmentCode = TariffTreatmentCodes.Codes.Chile;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Canada;
			invoiceLine.CA_RN_NKExport = Core.Constants.CountryCodes.Canada;
			invoiceLine.Charges.AddNew(CAChargeTypeList.Codes.OverseasFreight, 100m, Core.Constants.CurrencyCodes.UnitedStates);
			invoiceLine.ApportionedCharges.AddNew(CAChargeTypeList.Codes.OverseasFreight, 50m, Core.Constants.CurrencyCodes.Canada);
		}

		JobComInvoiceLine invoiceLine;
		JobComInvoiceHeader invoiceHeader;

		#endregion
	}
}
