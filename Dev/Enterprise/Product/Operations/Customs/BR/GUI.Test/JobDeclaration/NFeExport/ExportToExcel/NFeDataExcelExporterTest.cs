using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.BR.Business.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.BR.Business.Constants;
using Order = Enterprise.Freight.Forwarding.Orders.Business.Order;

namespace Enterprise.Customs.BR.GUI.Testing
{
	public class NFeDataExcelExporterTest : TestCaseWithFactory
	{
		public void TestExportDataToExcel_ISW()
		{
			ExportDataToExcel(BRJobMessageTypeList.Codes.ImportSiscomex);
		}

		public void TestExportDataToExcel_IMP()
		{
			ExportDataToExcel(BRJobMessageTypeList.Codes.Import);
		}

		void ExportDataToExcel(ZString shipmentType)
		{
			ReferenceTestDataHelper.CreateNCMTETariffBRCharacteristic(Factory);
			ReferenceTestDataHelper.CreateNVETariffBRCharacteristic(Factory);
			ReferenceTestDataHelper.CreateReferenceDataForIPITaxRegimeList(Factory);
			ReferenceTestDataHelper.CreateReferenceDataForTaxRegimeList(Factory, shipmentType);
			ReferenceTestDataHelper.CreateReferenceDataForICMSTaxRegimeList(Factory);
			ReferenceTestDataHelper.CreateReferenceDataForICMSLegalBaseList(Factory);

			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "Importer";
			importer.OH_Code = "TS1";
			importer.PrimaryRegistrationNumber.Number = "97442770000126";

			var orgSupplier = Factory.New<OrgHeader>();
			orgSupplier.OH_FullName = "S_TEST";
			var orgManufacturer = Factory.New<OrgHeader>();
			orgManufacturer.OH_FullName = "M_TEST";
			var orgManufacturerAddress = orgManufacturer.Addresses.AddNew();

			var order1 = Factory.New<Order>();
			order1.BuyerPK = orgManufacturer.PK;
			order1.SupplierPK = orgSupplier.PK;
			order1.JD_OrderNumber = "ActualOrder1";
			var orderLine1 = order1.OrderLines.AddNew();
			orderLine1.JO_LineNo = 1;
			orderLine1.JO_SubLineNo = 1;

			var order2 = Factory.New<Order>();
			order2.BuyerPK = orgManufacturer.PK;
			order2.SupplierPK = orgSupplier.PK;
			order2.JD_OrderNumber = "ActualOrder2";
			var orderLine2 = order2.OrderLines.AddNew();
			orderLine1.JO_LineNo = 2;
			orderLine1.JO_SubLineNo = 2;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = shipmentType;
			declaration.JE_DeclarationReference = "B00001011";
			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;
			declaration.JE_OH_Importer = importer.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice.JZ_InvoiceNumber = "INV001";
			invoice.JZ_OH_Supplier = orgSupplier.PK;

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_LineNo = 1;
			invoiceLine1.JI_Tariff = "11111111";
			invoiceLine1.JI_LinePrice = 600m;
			invoiceLine1.ComplementaryDescription = "Test Complement 1";

			invoiceLine1.JI_PartNo = "PC50";
			invoiceLine1.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._2;
			invoiceLine1.JI_OA_ManufacturerAddress = orgManufacturerAddress.PK;
			invoiceLine1.JI_Description = "Description";
			invoiceLine1.JI_InvoiceQuantity = 5;
			invoiceLine1.JI_InvoiceUQ = "PCS";
			invoiceLine1.JI_CustomsUnitQty = "KG";
			invoiceLine1.JI_CustomsQuantity = 10m;
			invoiceLine1.JI_VolumeUQ = "3m";
			invoiceLine1.JI_Weight = 20000m;
			invoiceLine1.JI_WeightUQ = "G";
			invoiceLine1.JI_NetWeight = 50000m;
			invoiceLine1.JI_NetWeightUQ = "G";
			invoiceLine1.DutyTaxRegime = TaxRegimeList.Codes.FullCollection;
			invoiceLine1.IPITaxRegime = IPITaxRegimeList.Codes.FullCollection;
			invoiceLine1.PisCofinsTaxRegime = TaxRegimeList.Codes.FullCollection;
			invoiceLine1.ICMSTaxRegime = ICMSTaxRegimeList.Codes.Reduction;
			invoiceLine1.ICMSLegalBase = "01";
			invoiceLine1.JI_ICMSFormula = ICMSFormulaList.Codes.BCR;
			invoiceLine1.JI_ICMSBaseValueReductionPercentage = 5.36m;
			invoiceLine1.JI_ICMSTotalAmountReductionPercentage = 6.78m;

			invoiceLine1.JI_PrimaryPreference = Constants.RatePreferenceType.ExTariff;
			invoiceLine1.DutyRateIsOverridden = true;
			invoiceLine1.DutyVigentRateValue = 1m;
			invoiceLine1.IPIRateIsOverridden = true;
			invoiceLine1.IPIVigentRateValue = 2m;
			invoiceLine1.PisRateIsOverridden = true;
			invoiceLine1.PisVigentRateValue = 4m;
			invoiceLine1.CofinsRateIsOverridden = true;
			invoiceLine1.CofinsVigentRateValue = 6m;
			invoiceLine1.JI_ICMSRate = 7m;

			var antidumping = invoiceLine1.Taxes.AddNew();
			antidumping.JLT_Type = Constants.RateCodes.Antidumping;
			antidumping.JLT_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.AdValoremRate;
			antidumping.JLT_Rate = 9m;

			invoiceLine1.ICMSFCPRateValue = 1.5m;

			invoiceLine1.JI_JO = orderLine1.PK;

			invoiceLine1.Charges.AddNew(ImportCustomsChargeTypeList.Codes.OverseasFreightCollect, 20.0m, declaration.LocalCurrencyCode);
			invoiceLine1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 10.0m, declaration.LocalCurrencyCode);
			invoiceLine1.Charges.AddNew(ImportCustomsChargeTypeList.Codes.OtherExpensesICMS, 10.0m, declaration.LocalCurrencyCode);

			if (shipmentType == BRJobMessageTypeList.Codes.ImportSiscomex)
			{
				var nve = invoiceLine1.NVECusCodeDataCollection.GetFirstElementHaving("BA");
				nve.CY_Data = "9999";

				nve = invoiceLine1.NVECusCodeDataCollection.GetFirstElementHaving("BB");
				nve.CY_Data = "9999";
			}

			var permit = invoiceLine1.Permits.AddNew();
			permit.CSI_ReferenceNumber = "1512452";
			permit.CSI_Quantity = 20;
			permit.CSI_UnitOfQuantity = "BOX";

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_LineNo = 2;
			invoiceLine2.JI_Tariff = "552642";
			invoiceLine2.JI_LinePrice = 400m;
			invoiceLine2.ComplementaryDescription = "Test Complement 2";

			invoiceLine2.JI_PartNo = "PC60";
			invoiceLine2.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._2;
			invoiceLine2.JI_OA_ManufacturerAddress = orgManufacturerAddress.PK;
			invoiceLine2.JI_Description = "Description";
			invoiceLine2.JI_InvoiceQuantity = 1;
			invoiceLine2.JI_InvoiceUQ = "BAG";
			invoiceLine2.JI_CustomsUnitQty = "KG";
			invoiceLine2.JI_CustomsQuantity = 1m;
			invoiceLine2.JI_VolumeUQ = "3m";
			invoiceLine2.JI_Weight = 196m;
			invoiceLine2.JI_WeightUQ = "KG";
			invoiceLine2.JI_NetWeight = 150m;
			invoiceLine2.JI_NetWeightUQ = "KG";
			invoiceLine2.DutyTaxRegime = TaxRegimeList.Codes.FullCollection;
			invoiceLine2.IPITaxRegime = IPITaxRegimeList.Codes.FullCollection;
			invoiceLine2.PisCofinsTaxRegime = TaxRegimeList.Codes.FullCollection;
			invoiceLine2.ICMSTaxRegime = ICMSTaxRegimeList.Codes.NonIncident;
			invoiceLine2.ICMSLegalBase = "02";

			invoiceLine2.JI_PrimaryPreference = Constants.RatePreferenceType.ExTariff;
			invoiceLine2.DutyRateIsOverridden = true;
			invoiceLine2.DutyVigentRateValue = 1m;
			invoiceLine2.IPIRateIsOverridden = true;
			invoiceLine2.IPIVigentRateValue = 2m;
			invoiceLine2.PisRateIsOverridden = true;
			invoiceLine2.PisVigentRateValue = 4m;
			invoiceLine2.CofinsRateIsOverridden = true;
			invoiceLine2.CofinsVigentRateValue = 6m;
			invoiceLine2.JI_ICMSRate = 7m;

			var antidumping2 = invoiceLine2.Taxes.AddNew();
			antidumping2.JLT_Type = Constants.RateCodes.Antidumping;
			antidumping2.JLT_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.AdValoremRate;
			antidumping2.JLT_Rate = 9m;

			invoiceLine2.ICMSFCPRateValue = 2.5m;

			invoiceLine2.JI_JO = orderLine2.PK;

			invoiceLine2.Charges.AddNew(ImportCustomsChargeTypeList.Codes.OverseasFreightCollect, 20.0m, declaration.LocalCurrencyCode);
			invoiceLine2.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 50.0m, declaration.LocalCurrencyCode);
			invoiceLine1.Charges.AddNew(ImportCustomsChargeTypeList.Codes.OtherExpensesICMS, 60.0m, declaration.LocalCurrencyCode);

			var entryheader1 = declaration.ActiveEntryHeaders.AddNew();
			entryheader1.MovementReferenceNumberSetter("2000010001", new ZDateTime(2023, 01, 01));
			entryheader1.CH_Status = BRMessageStatusList.Codes.Accepted;
			entryheader1.CH_EntryReleaseDate = new ZDateTime(2023, 08, 12);

			var entryLine1 = entryheader1.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine1.InvoiceLines.Add(invoiceLine1);
			entryLine1.InvoiceLines.Add(invoiceLine2);

			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine2.JI_CL = entryLine1.PK;

			var feeTypes = new string[] { ChargeTypesList.Codes.DTY, RateTypes.IPI, RateTypes.PIS, RateTypes.Cofins, RateTypes.ICMS, RateTypes.Antidumping, Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax, Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.SiscomexUsageEntryFee, Core.Constants.Customs.Universal.RefCusTaxOrFee.Codes.NoDiscountCode, Core.Constants.Customs.Universal.RefCusTaxOrFee.Codes.FiftyPercentDiscountCode };
			var baseValue = 1m;
			feeTypes.ForEach(feeType =>
			{
				CreateFeeAndSetValues(entryLine1, feeType, baseValue++);
			});

			var licDeclaration = Factory.New<JobDeclaration>();
			licDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var licInvoiceHeader = licDeclaration.Invoices.AddNew();
			var licInvoiceLine1 = licInvoiceHeader.InvoiceLines.AddNew();
			var licInvoiceLine2 = licInvoiceHeader.InvoiceLines.AddNew();
			licInvoiceLine1.DrawbackCANumber = "100";
			licInvoiceLine2.DrawbackCANumber = "200";

			var ipiSpecialRate1 = invoiceLine1.SpecialCaseTaxes.AddNew();
			ipiSpecialRate1.TaxType = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
			ipiSpecialRate1.TaxGroup = Constants.RateCodes.IPI;
			ipiSpecialRate1.UnitOfMeasure = "KG";
			ipiSpecialRate1.Quantity = 1m;
			ipiSpecialRate1.RateOrUnitValue = 5m;

			var pisSpecialRate1 = invoiceLine1.SpecialCaseTaxes.AddNew();
			pisSpecialRate1.TaxType = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
			pisSpecialRate1.TaxGroup = Constants.RateCodes.PIS;
			pisSpecialRate1.UnitOfMeasure = "KG";
			pisSpecialRate1.Quantity = 1m;
			pisSpecialRate1.RateOrUnitValue = 6m;

			var cofinsSpecialRate1 = invoiceLine1.SpecialCaseTaxes.AddNew();
			cofinsSpecialRate1.TaxType = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
			cofinsSpecialRate1.TaxGroup = Constants.RateCodes.Cofins;
			cofinsSpecialRate1.UnitOfMeasure = "KG";
			cofinsSpecialRate1.Quantity = 1m;
			cofinsSpecialRate1.RateOrUnitValue = 1m;

			var antidumpingSpecialRate1 = invoiceLine1.SpecialCaseTaxes.AddNew();
			antidumpingSpecialRate1.TaxType = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
			antidumpingSpecialRate1.TaxGroup = Constants.RateCodes.Antidumping;
			antidumpingSpecialRate1.UnitOfMeasure = "KG";
			antidumpingSpecialRate1.Quantity = 1m;
			antidumpingSpecialRate1.RateOrUnitValue = 9m;

			var ipiSpecialRate2 = invoiceLine2.SpecialCaseTaxes.AddNew();
			ipiSpecialRate2.TaxType = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
			ipiSpecialRate2.TaxGroup = Constants.RateCodes.IPI;
			ipiSpecialRate2.UnitOfMeasure = "KG";
			ipiSpecialRate2.Quantity = 1m;
			ipiSpecialRate2.RateOrUnitValue = 5m;

			var pisSpecialRate2 = invoiceLine2.SpecialCaseTaxes.AddNew();
			pisSpecialRate2.TaxType = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
			pisSpecialRate2.TaxGroup = Constants.RateCodes.PIS;
			pisSpecialRate2.UnitOfMeasure = "KG";
			pisSpecialRate2.Quantity = 1m;
			pisSpecialRate2.RateOrUnitValue = 6m;

			var cofinsSpecialRate2 = invoiceLine2.SpecialCaseTaxes.AddNew();
			cofinsSpecialRate2.TaxType = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
			cofinsSpecialRate2.TaxGroup = Constants.RateCodes.Cofins;
			cofinsSpecialRate2.UnitOfMeasure = "KG";
			cofinsSpecialRate2.Quantity = 1m;
			cofinsSpecialRate2.RateOrUnitValue = 1m;

			var antidumpingSpecialRate2 = invoiceLine2.SpecialCaseTaxes.AddNew();
			antidumpingSpecialRate2.TaxType = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
			antidumpingSpecialRate2.TaxGroup = Constants.RateCodes.Antidumping;
			antidumpingSpecialRate2.UnitOfMeasure = "KG";
			antidumpingSpecialRate2.Quantity = 1m;
			antidumpingSpecialRate2.RateOrUnitValue = 9m;

			invoiceLine1.JI_ParentID = licInvoiceLine1.PK;
			invoiceLine1.JI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;

			invoiceLine2.JI_ParentID = licInvoiceLine2.PK;
			invoiceLine2.JI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;

			var nfeExportObject = new NFeExportObject(declaration);
			using (var form = new NFeExportForm(nfeExportObject))
			{
				form.Show();

				var notifications = new ExcelExporterGuiNotifications(form);
				var excelExporter = new NFeDataExcelExporter(nfeExportObject, form.LinesGrid, notifications);

				using (var expectedStream = GetType().Assembly.GetManifestResourceStream($"Enterprise.Customs.BR.GUI.Testing.JobDeclaration.NFeExport.ExportToExcel.B00001011_{shipmentType}.xls"))
				using (var expectedExcelLoader = new ExcelInterface())
				using (var actualStream = new MemoryStream())
				using (var actualExcelLoader = new ExcelInterface())
				{
					expectedExcelLoader.LoadExcelFile(expectedStream);

					excelExporter.ExportDataToExcel(actualStream);
					actualExcelLoader.LoadExcelFile(actualStream);
					//actualExcelLoader.SaveToFile("C:\\git\\wtg\\CargoWise\\Dev\\Enterprise\\Product\\Operations\\Customs\\BR\\GUI.Test\\JobDeclaration\\NFeExport\\ExportToExcel\\B00001011.xls");

					AssertMultilineASCIIEquals(expectedExcelLoader.WorkSheets[0].ToString(), actualExcelLoader.WorkSheets[0].ToString());
				}
			}
		}

		public void TestTotalPropertyNames()
		{
			var entryExportObject = new NFeEntryExportObject(Factory.New<Business.CusEntryHeader>());

			string[] totalPropertyNames = new[]
			{
				NFeInvoiceLineExportObject.Schema.FOBValue,
				NFeInvoiceLineExportObject.Schema.FreightValue,
				NFeInvoiceLineExportObject.Schema.InsuranceValue,
				NFeInvoiceLineExportObject.Schema.CIFValue,
				NFeInvoiceLineExportObject.Schema.GrossWeight,
				NFeInvoiceLineExportObject.Schema.NetWeight
			};

			foreach (var propertyName in totalPropertyNames)
			{
				AssertEquals($"NFeExportObject should have property Total{propertyName}", typeof(ZDecimal), entryExportObject.GetPropertyType(NFeDataExcelExporter.TotalPropertyPrefix + propertyName));
			}
		}

		void CreateFeeAndSetValues(Customs.Business.CusEntryLine entryLine, string feeType, decimal baseValue)
		{
			var fee = entryLine.Fees.GetOrAddFeeByFeeType(feeType);
			fee.CF_BaseValue = baseValue;
			fee.CF_ChargeAmount = baseValue * 10m;
		}
	}
}
