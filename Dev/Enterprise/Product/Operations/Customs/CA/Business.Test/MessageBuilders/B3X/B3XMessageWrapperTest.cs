using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.US;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CA.Business.MessageBuilders.Testing
{
	sealed class B3XMessageWrapperTest : TestCaseWithFactory
	{
		public void TestIClassificationLine1Properties_DutiesAndTaxes()
		{
			#region Create Test Data
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			var invoice = declaration.B2AsClaimedForInvoices.AddNew();
			var line1 = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			Factory.Save();
			var cpt1 = line1.DutiesAndTaxes.AddNew();
			cpt1.C1_Override = true;
			cpt1.C1_TaxType = DutyAndTaxTypes.Codes.CPT;
			cpt1.C1_Amount = 33m;
			var cta1 = line1.DutiesAndTaxes.AddNew();
			cta1.C1_Override = true;
			cta1.C1_TaxType = DutyAndTaxTypes.Codes.CTA;
			cta1.C1_Amount = 34m;
			var dty1 = line1.DutiesAndTaxes.AddNew();
			dty1.C1_Override = true;
			dty1.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			dty1.C1_Amount = 35m;
			dty1.C1_Code = "AB";
			var exs1 = line1.DutiesAndTaxes.AddNew();
			exs1.C1_Override = true;
			exs1.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			exs1.C1_Amount = 36m;
			exs1.C1_Code = "BC";
			var sur1 = line1.DutiesAndTaxes.AddNew();
			sur1.C1_Override = true;
			sur1.C1_TaxType = DutyAndTaxTypes.Codes.SUR;
			sur1.C1_Amount = 37m;
			sur1.Quantity = 38m;
			sur1.C1_UnitOfMeasure = "M3";
			sur1.C1_Code = "CD";
			sur1.C1_ExemptCode = SIMACodes.Codes.C10;
			var add1 = line1.DutiesAndTaxes.AddNew();
			add1.C1_Override = true;
			add1.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			add1.C1_Amount = 39m;
			add1.Quantity = 40m;
			add1.C1_UnitOfMeasure = "M4";
			add1.C1_Code = "DE";
			add1.C1_ExemptCode = SIMACodes.Codes.C10;
			var cvd1 = line1.DutiesAndTaxes.AddNew();
			cvd1.C1_Override = true;
			cvd1.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			cvd1.C1_Amount = 43;
			cvd1.Quantity = 44;
			cvd1.C1_UnitOfMeasure = "M5";
			cvd1.C1_Code = "EF";
			cvd1.C1_ExemptCode = SIMACodes.Codes.C10;
			var saf1 = line1.DutiesAndTaxes.AddNew();
			saf1.C1_Override = true;
			saf1.C1_TaxType = DutyAndTaxTypes.Codes.SAF;
			saf1.C1_Amount = 45m;
			saf1.C1_Code = "FG";
			saf1.C1_ExemptCode = SIMACodes.Codes.C20;
			var ded1 = line1.Charges.AddNew();
			ded1.J7_ChargeType = CustomsChargeTypeList.Codes.DeductionCharge;
			ded1.J7_Amount = 46m;
			ded1.J7_RX_NKCurrency = "USD";
			var exd1 = line1.DutiesAndTaxes.AddNew();
			exd1.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			exd1.C1_Amount = 47m;
			exd1.C1_DutyType = DutyAndTaxManager.CombinedDuty.Excise;
			#endregion

			IB3Header b3Header = new B3XMessageWrapper(declaration);
			var classificationLine1 = b3Header.PositiveClassificationLines.First();
			AssertEquals("SalesTaxAmount", 33m, classificationLine1.SalesTaxAmount);
			AssertEquals("CTAAmount", 34m, classificationLine1.CTAAmount);
			AssertEquals("DeductionChargeAmountAndCurrency.Amount", 46m, classificationLine1.DeductionChargeAmountAndCurrency.Amount);
			AssertEquals("DeductionChargeAmountAndCurrency.Currency.RX_Code", "USD", classificationLine1.DeductionChargeAmountAndCurrency.Currency.RX_Code);
			AssertEquals("CustomsDutyCode", "AB", classificationLine1.CustomsDutyCode);
			AssertEquals("ExciseCode", "BC", classificationLine1.ExciseCode);
			AssertEquals("SurtaxQuantity", 38m, classificationLine1.SurtaxQuantity);
			AssertEquals("SurtaxUnitOfMeasure", "M3", classificationLine1.SurtaxUnitOfMeasure);
			AssertEquals("SurtaxCode", "CD", classificationLine1.SurtaxCode);
			AssertEquals("SurtaxStatementCode", "N", classificationLine1.SurtaxStatementCode);
			AssertEquals("HasSurtax", true, classificationLine1.HasSurtax);
			AssertEquals("ADDAmount", 39m, classificationLine1.ADDAmount);
			AssertEquals("ADDQuantity", 40m, classificationLine1.ADDQuantity);
			AssertEquals("ADDUnitOfMeasure", "M4", classificationLine1.ADDUnitOfMeasure);
			AssertEquals("ADDCode", "DE", classificationLine1.ADDCode);
			AssertEquals("ADDIsOverride", true, classificationLine1.ADDIsOverride);
			AssertEquals("HasADD", true, classificationLine1.HasADD);
			AssertEquals("CVDAmount", 43m, classificationLine1.CVDAmount);
			AssertEquals("CVDQuantity", 44m, classificationLine1.CVDQuantity);
			AssertEquals("CVDUnitOfMeasure", "M5", classificationLine1.CVDUnitOfMeasure);
			AssertEquals("CVDCode", "EF", classificationLine1.CVDCode);
			AssertEquals("CVDIsOverride", true, classificationLine1.CVDIsOverride);
			AssertEquals("HasCVD", true, classificationLine1.HasCVD);
			AssertEquals("SIMAStatementCode", "N", classificationLine1.SIMAStatementCode);
			AssertEquals("SafeguardCode", "FG", classificationLine1.SafeguardCode);
			AssertEquals("HasSafeguard", true, classificationLine1.HasSafeguard);
			AssertEquals("SafeguardStatementCode", "U", classificationLine1.SafeguardStatementCode);
			AssertEquals("ExciseDutyAmount", 47m, classificationLine1.ExciseDutyAmount);
		}

		public void TestRefreshCachedValues()
		{
			IB3Header b3Header = new B3XMessageWrapper(declaration);
			var posClassificationLines = new List<IClassificationLine1>(b3Header.PositiveClassificationLines);
			AssertEquals("PositiveClassificationLines count", 4, posClassificationLines.Count);

			var classificationLine = b3Header.PositiveClassificationLines.First(x => x.ClassificationNumber == "0301104567" && x.SIMACode == SIMACodes.Codes.C31);
			AssertEquals("CustomsQuantity", 1000m, classificationLine.CustomsQuantity);
			AssertEquals("CustomsUnitQty", CustomsUnitOfMeasureList.Codes.Litre, classificationLine.CustomsUnitQty);
			AssertEquals("InvoiceQuantity", 999m, classificationLine.InvoiceQuantity);
			AssertEquals("InvoiceUQ", CustomsUnitOfMeasureList.Codes.Tube, classificationLine.InvoiceUQ);
			AssertEquals("TotalLinePrice", 1000m, classificationLine.TotalLinePrice.Amount);
			AssertEquals("CustomsValue", 1000m, classificationLine.CustomsValue.Amount);
			AssertEquals("FOB.Amount", 1000m, classificationLine.FOB.Amount);
			AssertEquals("FOB.Currency", "CAD", classificationLine.FOB.Currency.Code);

			var b3NegSubHeaders = new List<IB3SubHeader>(b3Header.NegativeB3SubHeaders);
			AssertEquals("NegativeB3SubHeaders count", 2, b3NegSubHeaders.Count);

			var negClassificationLines = new List<IClassificationLine1>(b3Header.NegativeClassificationLines);
			AssertEquals("NegativeClassificationLines count", 3, negClassificationLines.Count);

			var newline = declaration.B2AsClaimedForInvoices.AddNew();
			newline.JZ_InvoiceNumber = "XXX";
			newline.InvoiceLines.AddNew();
			var newline2 = declaration.B2AsAccountedForInvoices.AddNew();
			newline2.JZ_InvoiceNumber = "XXX";
			newline2.InvoiceLines.AddNew();

			b3Header.RefreshCachedValues();

			posClassificationLines = new List<IClassificationLine1>(b3Header.PositiveClassificationLines);
			AssertEquals("PositiveClassificationLines count", 5, posClassificationLines.Count);

			b3NegSubHeaders = new List<IB3SubHeader>(b3Header.NegativeB3SubHeaders);
			AssertEquals("NegativeB3SubHeaders count", 3, b3NegSubHeaders.Count);

			negClassificationLines = new List<IClassificationLine1>(b3Header.NegativeClassificationLines);
			AssertEquals("NegativeClassificationLines count", 4, negClassificationLines.Count);
		}

		#region TestIB3HeaderProperties
		public void TestIB3HeaderProperties()
		{
			IB3Header b3Header = new B3XMessageWrapper(declaration);
			AssertEquals("BatchNumber", "<<UNIQUE BATCH NUMBER PLACE HOLDER>>", b3Header.BatchNumber);
			AssertEquals("B3TypeCode", "X", b3Header.B3TypeCode);
			AssertEquals("PaymentCode", "D", b3Header.PaymentCode);
			AssertEquals("CBSAOffice", "351", b3Header.CBSAOffice);
			AssertEquals("PortOfUnlading", ZString.Empty, b3Header.PortOfUnlading);
			AssertEquals("WarehouseNumber", ZString.Empty, b3Header.WarehouseNumber);
			AssertEquals("TransactionNumber", "000067897", b3Header.TransactionNumber);
			AssertEquals("AccountSecurityCode", "12345", b3Header.AccountSecurityCode);
			AssertEquals("BusinessNumber", "3021", b3Header.BusinessNumber);

			AssertEquals("Importer", "IMPORTER NAME", b3Header.Importer.E2_CompanyName);
			AssertEquals("GSTNumber", "0987654321", b3Header.GSTNumber);
			AssertEquals("TransportMode", "1", b3Header.TransportMode);

			AssertEquals("B3BInputReleases Count", 1, b3Header.B3BInputReleases.Count());
			var release1 = b3Header.B3BInputReleases.First();
			AssertEquals("CargoControlNumber", "2CSA", release1.CargoControlNumber);
			AssertEquals("DateOfRelease", declaration.JE_EntryAuthorisationDate, release1.DateOfRelease);

			declaration.CA_OriginalTransactionNo = "12345000067886";
			b3Header = new B3XMessageWrapper(declaration);
			AssertEquals("B3BInputReleases Count", 2, b3Header.B3BInputReleases.Count());
			release1 = b3Header.B3BInputReleases.First();
			var release2 = b3Header.B3BInputReleases.ElementAt(1);
			AssertEquals("CargoControlNumber", "212112345678987654321", release1.CargoControlNumber);
			AssertEquals("DateOfRelease", declaration.JE_EntryAuthorisationDate, release1.DateOfRelease);
			AssertEquals("CargoControlNumber", "123456789321566549877", release2.CargoControlNumber);
			AssertEquals("DateOfRelease", ZDateTime.Empty, release2.DateOfRelease);

			AssertEquals("CarrierCodeAtImportation", "2CSA", b3Header.CarrierCodeAtImportation);
			AssertEquals("TotalValueForDuty", 80793.64m, b3Header.TotalValueForDuty);
			AssertEquals("B3Comments", ZString.Empty, b3Header.B3Comments);

			var b3PosSubHeaders = new List<IB3SubHeader>(b3Header.PositiveB3SubHeaders);
			AssertEquals("PositiveB3SubHeaders count", 0, b3PosSubHeaders.Count);

			var b3SubHeaders = new List<IB3SubHeader>(b3Header.NegativeB3SubHeaders);
			AssertEquals("NegativeB3SubHeaders count", 2, b3SubHeaders.Count);
			var accountedInvoices = declaration.B2AsAccountedForInvoices;
			accountedInvoices.ApplySort(JobComInvoiceHeader.Schema.JZ_InvoiceNumber, ListSortDirection.Ascending);

			var classLines = new List<IClassificationLine1>(b3Header.NegativeClassificationLines);
			AssertEquals("NegativeClassificationLines count", 3, classLines.Count);
			var allInvoiceLines = declaration.InvoiceLines.Cast<JobComInvoiceLine>();
			AssertTotalAmounts(b3Header.PositiveTotalAmounts, 3.5m, -33.02m, 2.5m, 2.5m, -24.52m);
			AssertTotalAmounts(b3Header.NegativeTotalAmounts, 0m, 0m, 0, 0m, 0m);
		}

		internal static void AssertTotalAmounts(ITotalAmounts amounts, decimal excise, decimal gst, decimal sima, decimal duty, decimal all)
		{
			AssertEquals("TotalExciseTax", excise, amounts.TotalExciseTax);
			AssertEquals("TotalGST", gst, amounts.TotalGST);
			AssertEquals("TotalSIMAAssessment", sima, amounts.TotalSIMAAssessment);
			AssertEquals("TotalCustomsDuty", duty, amounts.TotalCustomsDuty);
			AssertEquals("TotalAllDutyAndTaxes", all, amounts.TotalAllDutyAndTaxes);
		}

		#endregion

		#region TestB3MessageContent

		#endregion
		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			CACustomsDataRegistry.Instance.DefaultExciseTaxFromCustomsTariff.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var factory = JobComInvoiceLineTestHelper.PopulateDutiesAndTaxesRefFilesReturningFactory();
			var helper = new DeclarationTestHelper(factory, true);
			var canada = factory.Load<RefCountry>(Constants.CountryGuids.Canada);

			var oriDeclaration = factory.New<JobDeclaration>();
			oriDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			oriDeclaration.TransactionNumber.AccountSecurityCode = "12345";
			oriDeclaration.TransactionNumber.SequentialNumber = "00006788";
			oriDeclaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "212112345678987654321";
			oriDeclaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = ZString.Empty;
			oriDeclaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "123456789321566549877";
			factory.Save();

			declaration = factory.New<JobDeclaration>();
			declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.CustomsMessageToPrintOnB3.Description, "Note to be printed on B3");
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			declaration.JE_OH_Supplier = helper.CreateOrganisation("SUP", "SUPPLIER NAME", "AUMEL", "SUPPLIER ADDRESS", "MELBOURNE", "VIC", "3000", "123 4567").PK;
			var importer = helper.CreateOrganisation("IMP", "IMPORTER NAME", "CATOR", "IMPORTER ADDRESS", "IMPORTER CITY", "123 4567");
			importer.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = ZString.Empty;
			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "3021", canada);
			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial, "1234", canada);
			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForGoodsServicesHarmonizedSalesTax, "0987654321", canada);
			((OrgImpAddInfo)importer.CountryData.ImpAddInfo).ZO_IsImporterDirectPayment = true;
			declaration.JE_OH_Importer = importer.PK;
			declaration.CA_B2Type = "SC";
			declaration.CA_OriginalTransactionNo = "12345000067876";
			declaration.JE_CustomsOffice = "351";
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "00006789";
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2010, 4, 30, 12, 41, 25);
			declaration.JE_OH_ShippingLine = helper.ShippingLine.PK;
			declaration.JE_OH_Forwarder = helper.ExportForwarder.PK;
			var notifyParty = helper.CreateOrganisation("NTY", "NOTIFY", "USNYC", "1 street", "New York City", "NY", "12345", "123454678990");
			notifyParty.MainAddress.Postcode = "10022";
			declaration.JE_OH_NotifyParty = notifyParty.PK;
			declaration.ShippingLine.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "2121", canada);
			declaration.Forwarder.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "1212", canada);
			declaration.CA_AnySightDepositAmount = 50m;
			declaration.JE_PaymentMethod = "D";
			declaration.CA_AmendmentTo = "B2";
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_CarrierCode = "2CSA";

			//Invoice 1
			var supplier1 = helper.CreateOrganisation("SUP", "SUPPLIER NAME", "AUMEL", "SUPPLIER ADDRESS", "MELBOURNE", "VIC", "3000", "123 4567").PK;
			invoice = declaration.B2AsAccountedForInvoices.AddNew();
			invoice.JZ_InvoiceNumber = "1";
			invoice.Charges.AddNew(CAChargeTypeList.Codes.OverseasFreight, 100m);
			invoice.JZ_OH_Supplier = supplier1;
			invoice.ExporterDocumentaryAddress.OrganisationPK = helper.CreateOrganisation("SUP", "EXPORTER NAME", "USCHI", "EXPORTER ADDRESS", "CHICARGO", "IL", "12321", "123 4567").PK;
			invoice.CA_RN_NKExport = Constants.CountryCodes.UnitedStates;
			invoice.JZ_ValuationDateOverride = new ZDateTime(2010, 4, 30, 12, 41, 25);
			invoice.CA_USStateOfExport = USStatesList.Codes.NewYork;
			invoice.CA_TradeZone = "168B";
			invoice.CA_USPortOfExit = "2813";
			invoice.CA_TreatmentCode = TariffTreatmentCodes.Codes.MostFavouredNation;
			invoice.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Day;
			invoice.CA_TimeLimit = 20;
			helper.USD.SetCustomsRate(ZDateTime.Today, ZDateTime.Today, 1.111111);
			invoice.JZ_RX_NKInvoice_Currency = helper.USD.RX_Code;
			invoice.JZ_Weight = 250;
			invoice.JZ_WeightUQ = "LB";

			//Invoice Lines
			var accLine1 = invoice.AsAccountForFilteredInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(accLine1, 1, 1, 1000, 1000, 666, 333, Constants.Weight.Kilograms, 222);
			accLine1.CA_OriginalLineNo = "1";
			JobComInvoiceLineTestHelper.AddTariffRecord(accLine1);
			var accLine2 = invoice.AsAccountForFilteredInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(accLine2, 2, 1, 700, 500, 250, 100, Constants.Weight.Tonnes, 180);
			accLine2.CA_OriginalLineNo = "2";

			var dutieOrTax = accLine2.DutiesAndTaxes.AddNew();
			dutieOrTax.C1_Override = true;
			dutieOrTax.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			dutieOrTax.C1_ExemptCode = SIMACodes.Codes.C31;
			dutieOrTax.C1_Amount = 1m;
			dutieOrTax = accLine2.DutiesAndTaxes.AddNew();
			dutieOrTax.C1_Override = true;
			dutieOrTax.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			dutieOrTax.C1_Amount = 3m;
			dutieOrTax = accLine2.DutiesAndTaxes.AddNew();
			dutieOrTax.C1_Override = true;
			dutieOrTax.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			dutieOrTax.C1_Amount = 3m;
			dutieOrTax = accLine2.DutiesAndTaxes.AddNew();
			dutieOrTax.C1_Override = true;
			dutieOrTax.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			dutieOrTax.C1_Amount = 4m;

			//Invoice 2
			var supplier2 = helper.CreateOrganisation("SUP", "SUPPLIER NAME2", "USNYC", "SUPPLIER ADDRESS2", "NEW YOUK", "NY", "12345", "123 4567").PK;
			invoice = declaration.B2AsAccountedForInvoices.AddNew();
			invoice.JZ_InvoiceNumber = "7";
			invoice.Charges.AddNew(CAChargeTypeList.Codes.OverseasFreight, 0.01m);
			invoice.JZ_OH_Supplier = supplier2;
			invoice.CA_RN_NKExport = Constants.CountryCodes.NewZealand;
			invoice.CA_USPortOfExit = "3216";
			invoice.CA_TreatmentCode = TariffTreatmentCodes.Codes.MostFavouredNation;
			invoice.JZ_ValuationDateOverride = new ZDateTime(2010, 5, 30, 12, 41, 25);
			invoice.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Month;
			invoice.CA_TimeLimit = 2;
			invoice.JZ_RX_NKInvoice_Currency = helper.CAD.RX_Code;
			invoice.CA_TradeZone = "";
			invoice.JZ_Weight = 50;
			invoice.JZ_WeightUQ = "LB";

			//Invoice Lines
			var accLine3 = invoice.AsAccountForFilteredInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(accLine3, 3, 2, 1000, 1000, 666, 333, Constants.Weight.Kilograms);
			accLine3.JI_Tariff = "0301104567";
			accLine3.JI_CustomsQuantity = 1000;
			accLine3.CA_OriginalLineNo = "1/SL";
			accLine3.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Litre;
			JobComInvoiceLineTestHelper.AddTariffRecord(accLine3);

			invoice = declaration.B2AsClaimedForInvoices.AddNew();
			invoice.JZ_InvoiceNumber = "7";
			invoice.Charges.AddNew(CAChargeTypeList.Codes.OverseasFreight, 0.01m);
			invoice.JZ_OH_Supplier = supplier2;
			invoice.CA_RN_NKExport = Constants.CountryCodes.NewZealand;
			invoice.CA_USPortOfExit = "3216";
			invoice.CA_TreatmentCode = TariffTreatmentCodes.Codes.MostFavouredNation;
			invoice.JZ_ValuationDateOverride = new ZDateTime(2010, 5, 30, 12, 41, 25);
			invoice.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Month;
			invoice.CA_TimeLimit = 2;
			invoice.JZ_RX_NKInvoice_Currency = helper.CAD.RX_Code;
			invoice.CA_TradeZone = "";
			invoice.JZ_Weight = 50;
			invoice.JZ_WeightUQ = "LB";

			//Invoice Lines
			var acfLine1 = invoice.AsClaimForFilteredInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(acfLine1, 5, 2, 1000, 1000, 666, 333, Constants.Weight.Kilograms);
			acfLine1.JI_Tariff = "0301104567";
			acfLine1.JI_CustomsQuantity = 1000;
			acfLine1.CA_OriginalLineNo = "1";
			acfLine1.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Litre;
			acfLine1.CA_CalculationMethod = CalculationMethods.Codes.DeliveredDutyPaid;
			acfLine1.JI_InvoiceQuantity = 999;
			acfLine1.JI_InvoiceUQ = CustomsUnitOfMeasureList.Codes.Tube;
			JobComInvoiceLineTestHelper.AddTariffRecord(acfLine1);

			dutieOrTax = acfLine1.DutiesAndTaxes.AddNew();
			dutieOrTax.C1_Override = true;
			dutieOrTax.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			dutieOrTax.C1_ExemptCode = SIMACodes.Codes.C31;
			dutieOrTax.C1_Amount = 1.5m;
			dutieOrTax = acfLine1.DutiesAndTaxes.AddNew();
			dutieOrTax.C1_Override = true;
			dutieOrTax.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			dutieOrTax.C1_Amount = 2.5m;
			dutieOrTax = acfLine1.DutiesAndTaxes.AddNew();
			dutieOrTax.C1_Override = true;
			dutieOrTax.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			dutieOrTax.C1_Amount = 3.5m;
			dutieOrTax = acfLine1.DutiesAndTaxes.AddNew();
			dutieOrTax.C1_Override = true;
			dutieOrTax.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			dutieOrTax.C1_Amount = 2.5m;
			acfLine1.CA_CVforCurrConv = 80797.64m;
			acfLine1.CA_CVforCurrConvOvr = true;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
		}

		internal static void DeleteCusCode(string code, OrgHeader orgHeader)
		{
			orgHeader.CustomsCodes.RemoveAndDelete(orgHeader.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(code, Constants.CountryCodes.Canada));
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;

		#endregion
	}
}
