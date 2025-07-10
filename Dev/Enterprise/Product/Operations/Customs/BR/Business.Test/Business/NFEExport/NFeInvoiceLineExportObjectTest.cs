using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.BR.Business.Constants;
using CustomsChargeTypeList = Enterprise.Customs.Business.CustomsChargeTypeList;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(NFeInvoiceLineExportObject))]
	class NFeInvoiceLineExportObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestNFeExportObjectLines()
		{
			ReferenceTestDataHelper.CreateNCMTETariffBRCharacteristic(Factory);
			ReferenceTestDataHelper.CreateNVETariffBRCharacteristic(Factory);
			ReferenceTestDataHelper.CreateReferenceDataForIPITaxRegimeList(Factory);
			ReferenceTestDataHelper.CreateReferenceDataForTaxRegimeList(Factory, BRJobMessageTypeList.Codes.ImportSiscomex);
			ReferenceTestDataHelper.CreateReferenceDataForICMSTaxRegimeList(Factory);
			ReferenceTestDataHelper.CreateRefCusRateCodeAndType(Factory);
			ReferenceTestDataHelper.CreateReferenceDataForICMSLegalBaseList(Factory);

			var orgSupplier = Factory.New<OrgHeader>();
			orgSupplier.OH_FullName = "S_TEST";

			var orgManufacturer = Factory.New<OrgHeader>();
			orgManufacturer.OH_FullName = "M_TEST";
			var orgManufacturerAddress = orgManufacturer.Addresses.AddNew();

			var newCurrency = RefCurrency.New(Factory);
			newCurrency.RX_Code = "MDD";
			newCurrency.SetCustomsRate(ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10), 2.1111m);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV01";
			invoice.JZ_InvoiceAmount = 100m;
			invoice.JZ_IncoTerm = "CIF";
			invoice.JZ_RX_NKInvoice_Currency = "MDD";
			invoice.JZ_OH_Supplier = orgSupplier.PK;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LineNo = 1;
			invoiceLine.JI_PartNo = "PROD01";
			invoiceLine.JI_Tariff = "11111111";
			invoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._2;
			invoiceLine.JI_OA_ManufacturerAddress = orgManufacturerAddress.PK;
			invoiceLine.FullGoodsDescription = "PRODUCT DESCRIPTION";
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.ComplementaryDescription = "Test Complement 1";
			invoiceLine.JI_Weight = 96.1234m;
			invoiceLine.JI_NetWeight = 50.456m;
			invoiceLine.JI_InvoiceQuantity = 50m;
			invoiceLine.JI_InvoiceUQ = "KG";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.DutyTaxRegime = "1";
			invoiceLine.IPITaxRegime = "1";
			invoiceLine.PisCofinsTaxRegime = "1";
			invoiceLine.ICMSTaxRegime = "4";
			invoiceLine.ICMSLegalBase = "01";
			invoiceLine.JI_ICMSBaseValueReductionPercentage = 5.36m;
			invoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ExTariff;
			invoiceLine.DutyRateIsOverridden = true;
			invoiceLine.DutyVigentRateValue = 3m;
			invoiceLine.IPIRateIsOverridden = true;
			invoiceLine.IPIVigentRateValue = 5m;
			invoiceLine.PisRateIsOverridden = true;
			invoiceLine.PisVigentRateValue = 6m;
			invoiceLine.CofinsRateIsOverridden = true;
			invoiceLine.CofinsVigentRateValue = 1m;
			invoiceLine.JI_ICMSRate = 7m;
			invoiceLine.ICMSFCPRateValue = 8m;
			invoiceLine.JI_ICMSTotalAmountReductionPercentage = 10m;
			invoiceLine.JI_ICMSFormula = ICMSFormulaList.Codes.BCR;

			var antidumping = invoiceLine.Taxes.AddNew();
			antidumping.JLT_Type = Constants.RateCodes.Antidumping;
			antidumping.JLT_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.AdValoremRate;
			antidumping.JLT_Rate = 9m;

			invoiceLine.NVECusCodeDataCollection.GetFirstElementHaving("BA").CY_Data = "9999";
			invoiceLine.NVECusCodeDataCollection.GetFirstElementHaving("BB").CY_Data = "9999";

			var permit = invoiceLine.Permits.AddNew();
			permit.CSI_ReferenceNumber = "1512452";
			permit.CSI_Quantity = 20;
			permit.CSI_UnitOfQuantity = "BOX";

			permit = invoiceLine.Permits.AddNew();
			permit.CSI_ReferenceNumber = "8878787";
			permit.CSI_Quantity = 15;
			permit.CSI_UnitOfQuantity = "BAG";

			invoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 10.0m, newCurrency.RX_Code);
			invoiceLine.Charges.AddNew(ImportCustomsChargeTypeList.Codes.OverseasFreightCollect, 20.0m, newCurrency.RX_Code);
			invoiceLine.Charges.AddNew(ImportCustomsChargeTypeList.Codes.OtherExpensesICMS, 16.0m, newCurrency.RX_Code);

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("2000010001", ZDateTime.Now);

			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine.JI_CL = entryLine.PK;

			var order = Factory.New<Order>();
			order.BuyerPK = orgManufacturer.PK;
			order.SupplierPK = orgSupplier.PK;
			order.JD_OrderNumber = "ActualOrder";
			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_LineNo = 1;
			orderLine.JO_SubLineNo = 1;

			invoiceLine.JI_JO = orderLine.PK;

			var feeTypes = new string[]
			{
				ChargeTypesList.Codes.DTY,
				RateTypes.IPI,
				RateTypes.PIS,
				RateTypes.Cofins,
				RateTypes.ICMS,
				RateTypes.Antidumping,
				Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax,
				Core.Constants.Customs.Universal.RefCusTaxOrFee.Codes.NoDiscountCode,
				Core.Constants.Customs.Universal.RefCusTaxOrFee.Codes.FiftyPercentDiscountCode,
				Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.SiscomexUsageEntryFee
			};

			var baseValue = 1m;
			feeTypes.ForEach(feeType =>
			{
				CreateFeeAndSetValues(entryLine, feeType, baseValue++);
			});

			var licDeclaration = Factory.New<JobDeclaration>();
			licDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var licInvoiceLine = licDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			licInvoiceLine.DrawbackCANumber = "100";

			invoiceLine.JI_ParentID = licInvoiceLine.PK;
			invoiceLine.JI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;

			var ipiSpecialRate = invoiceLine.SpecialCaseTaxes.AddNew();
			ipiSpecialRate.TaxType = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
			ipiSpecialRate.TaxGroup = Constants.RateCodes.IPI;
			ipiSpecialRate.UnitOfMeasure = "KG";
			ipiSpecialRate.Quantity = 1m;
			ipiSpecialRate.RateOrUnitValue = 5m;

			var pisSpecialRate = invoiceLine.SpecialCaseTaxes.AddNew();
			pisSpecialRate.TaxType = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
			pisSpecialRate.TaxGroup = Constants.RateCodes.PIS;
			pisSpecialRate.UnitOfMeasure = "KG";
			pisSpecialRate.Quantity = 1m;
			pisSpecialRate.RateOrUnitValue = 6m;

			var cofinsSpecialRate = invoiceLine.SpecialCaseTaxes.AddNew();
			cofinsSpecialRate.TaxType = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
			cofinsSpecialRate.TaxGroup = Constants.RateCodes.Cofins;
			cofinsSpecialRate.UnitOfMeasure = "KG";
			cofinsSpecialRate.Quantity = 1m;
			cofinsSpecialRate.RateOrUnitValue = 1m;

			var antidumpingSpecialRate = invoiceLine.SpecialCaseTaxes.AddNew();
			antidumpingSpecialRate.TaxType = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
			antidumpingSpecialRate.TaxGroup = Constants.RateCodes.Antidumping;
			antidumpingSpecialRate.UnitOfMeasure = "KG";
			antidumpingSpecialRate.Quantity = 1m;
			antidumpingSpecialRate.RateOrUnitValue = 9m;

			var nfe = declaration.NFeExportObject.Entries.Cast<NFeEntryExportObject>().First();

			var nfeItem = nfe.Lines.Cast<NFeInvoiceLineExportObject>().First();

			CombineAssertions(() =>
			{
				AssertEquals("InvoiceLineNumber", 1, nfeItem.InvoiceLineNumber.ToZInt());
				AssertEquals("InvoiceNumber", "INV01", nfeItem.InvoiceNumber);
				AssertEquals("ProductCode", "PROD01", nfeItem.ProductCode);

				AssertEquals("GoodsDescription", "PRODUCT DESCRIPTION", nfeItem.GoodsDescription);
				nfeItem.GoodsDescription = "CHANGED DESCRIPTION";
				AssertEquals("GoodsDescription", "CHANGED DESCRIPTION", nfeItem.GoodsDescription);

				AssertEquals("SupplierName", "S_TEST", nfeItem.SupplierName);
				AssertEquals("ManufacturerName", "M_TEST", nfeItem.ManufacturerName);

				AssertEquals("InvoiceQuantity", 50m, nfeItem.InvoiceQuantity);
				AssertEquals("InvoiceQuantityUQ", "KG", nfeItem.InvoiceQuantityUQ);
				AssertEquals("CustomsQuantityUQ", "KG", nfeItem.CustomsQuantityUQ);
				AssertEquals("CustomsQuantity", 50.456m, nfeItem.CustomsQuantity);
				AssertEquals("FOBValue", 47.37m, nfeItem.FOBValue);
				AssertEquals("FreightValue", 9.47m, nfeItem.FreightValue);
				AssertEquals("InsuranceValue", 4.74m, nfeItem.InsuranceValue);
				AssertEquals("CIFValue", 61.58m, nfeItem.CIFValue);
				AssertEquals("GrossWeight", 96.123m, nfeItem.GrossWeight);
				AssertEquals("NetWeight", 50.456m, nfeItem.NetWeight);

				AssertEquals("DutyTaxRegime", "RECOLHIMENTO INTEGRAL", nfeItem.DutyTaxRegime);
				AssertEquals("DutyBaseAmount", 11.11m, nfeItem.DutyBaseAmount);
				AssertEquals("DutyRate", 3m, nfeItem.DutyRate);
				AssertEquals("DutyAmount", 1.11m, nfeItem.DutyAmount);
				AssertEquals("IPITaxRegime", "Exemption", nfeItem.IPITaxRegime);
				AssertEquals("IPIBaseAmount", 22.22m, nfeItem.IPIBaseAmount);
				AssertEquals("IPIRate", 5m, nfeItem.IPIRate);
				AssertEquals("IPIAmount", 2.22m, nfeItem.IPIAmount);
				AssertEquals("PISCofinsTaxRegime", "RECOLHIMENTO INTEGRAL", nfeItem.PISCofinsTaxRegime);
				AssertEquals("PISBaseAmount", 33.33m, nfeItem.PISBaseAmount);
				AssertEquals("PISRate", 6m, nfeItem.PISRate);
				AssertEquals("PISAmount", 3.33m, nfeItem.PISAmount);
				AssertEquals("CofinsBaseAmount", 44.44m, nfeItem.CofinsBaseAmount);
				AssertEquals("CofinsRate", 1m, nfeItem.CofinsRate);
				AssertEquals("CofinsAmount", 4.44m, nfeItem.CofinsAmount);
				AssertEquals("ICMSTaxRegime", "Reduction", nfeItem.ICMSTaxRegime);
				AssertEquals("ICMSLegalBase", "ICMS Legal Base 01", nfeItem.ICMSLegalBase);
				AssertEquals("ICMSBaseAmount", 116.72m, nfeItem.ICMSBaseAmount);
				AssertEquals("ICMSRate", 7m, nfeItem.ICMSRate);
				AssertEquals("ICMSReductionPercentage", 5.36m, nfeItem.ICMSReductionPercentage);
				AssertEquals("ICMSAmount", 7.35m, nfeItem.ICMSAmount);
				AssertEquals("FCPRate", 8m, nfeItem.FCPRate);
				AssertEquals("FCPAmount", 9.34m, nfeItem.FCPAmount);
				AssertEquals("AntidumpingBaseAmount", 66.67m, nfeItem.AntidumpingBaseAmount);
				AssertEquals("AntidumpingRate", 9m, nfeItem.AntidumpingRate);
				AssertEquals("AntidumpingAmount", 6.67m, nfeItem.AntidumpingAmount);
				AssertEquals("Addition", "1", nfeItem.Addition);
				AssertEquals("Nve", "BA;BA;Outros,BB;BB;", nfeItem.Nve);
				AssertEquals("ManufacturerIndicator", ManufacturerIndicatorList.Descriptions._2, nfeItem.ManufacturerIndicator);
				AssertEquals("IcmsTotalAmountReduction", 10m, nfeItem.IcmsTotalAmountReduction);
				AssertEquals("AfrmmAmount", 7.7777m, nfeItem.AfrmmAmount);
				AssertEquals("ImportLicenseFineAmount", 18.89m, nfeItem.ImportLicenseFineAmount);
				AssertEquals("EICAmount", 7.58m, nfeItem.EICAmount);
				AssertEquals("OrderNumber", order.JD_OrderNumber, nfeItem.OrderNumber);
				AssertEquals("OrderLineNumberAndSubLine", "1", nfeItem.OrderLineNumberAndSubLine);
				AssertEquals("SiscomexUsageFee", 11.1110m, nfeItem.SiscomexUsageFee);
				AssertEquals("ConcessionActNumber", "100", nfeItem.ConcessionActNumber);
				AssertEquals("IPISpecialRateUQ", "KG", nfeItem.IPISpecialRateUQ);
				AssertEquals("IPISpecialRateQuantity", 1m, nfeItem.IPISpecialRateQuantity);
				AssertEquals("IPISpecialRateAmount", 5m, nfeItem.IPISpecialRateAmount);
				AssertEquals("PISSpecialRateUQ", "KG", nfeItem.PISSpecialRateUQ);
				AssertEquals("PISSpecialRateQuantity", 1m, nfeItem.PISSpecialRateQuantity);
				AssertEquals("PISSpecialRateAmount", 6m, nfeItem.PISSpecialRateAmount);
				AssertEquals("CofinsSpecialRateUQ", "KG", nfeItem.CofinsSpecialRateUQ);
				AssertEquals("CofinsSpecialRateQuantity", 1m, nfeItem.CofinsSpecialRateQuantity);
				AssertEquals("CofinsSpecialRateAmount", 1m, nfeItem.CofinsSpecialRateAmount);
				AssertEquals("AntidumpingSpecialRateUQ", "KG", nfeItem.AntidumpingSpecialRateUQ);
				AssertEquals("AntidumpingSpecialRateQuantity", 1m, nfeItem.AntidumpingSpecialRateQuantity);
				AssertEquals("AntidumpingSpecialRateAmount", 9m, nfeItem.AntidumpingSpecialRateAmount);
				AssertEquals("Permits", "1512452;20;Box,8878787;15;Bag", nfeItem.Permits);
				AssertEquals("Complement", "Test Complement 1", nfeItem.Complement);
			});
		}

		public void TestNFeExportObjectLinesWithApportion()
		{
			var newCurrency = RefCurrency.New(Factory);
			newCurrency.RX_Code = "MDD";
			newCurrency.SetCustomsRate(ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10), 2.1111m);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV01";
			invoice.JZ_InvoiceAmount = 200m;
			invoice.JZ_IncoTerm = "CIF";
			invoice.JZ_RX_NKInvoice_Currency = newCurrency.Code;
			invoice.Charges.AddNew(ImportCustomsChargeTypeList.Codes.OtherExpensesICMS, 16m, newCurrency.Code);

			JobComInvoiceLine CreateInvoiceLine(ZShort lineNo)
			{
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_LineNo = lineNo;
				invoiceLine.JI_PartNo = "PROD01";
				invoiceLine.JI_Tariff = "001";
				invoiceLine.JI_Description = "PRODUCT DESCRIPTION";
				invoiceLine.JI_LinePrice = 100m;
				invoiceLine.JI_NetWeight = 50m;
				invoiceLine.JI_InvoiceQuantity = 50m;
				invoiceLine.JI_InvoiceUQ = "KG";
				invoiceLine.JI_CustomsUnitQty = "KG";
				invoiceLine.DutyTaxRegime = "1";
				invoiceLine.IPITaxRegime = "1";
				invoiceLine.PisCofinsTaxRegime = "1";
				invoiceLine.ICMSTaxRegime = "1";
				invoiceLine.ICMSFCPRateValue = 8;
				invoiceLine.JI_ICMSBaseValueReductionPercentage = 5.36m;
				invoiceLine.JI_ICMSFormula = ICMSFormulaList.Codes.BCR;

				invoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ExTariff;
				invoiceLine.DutyRateIsOverridden = true;
				invoiceLine.DutyVigentRateValue = 1m;
				invoiceLine.IPIRateIsOverridden = true;
				invoiceLine.IPIVigentRateValue = 2m;
				invoiceLine.PisRateIsOverridden = true;
				invoiceLine.PisVigentRateValue = 4m;
				invoiceLine.CofinsRateIsOverridden = true;
				invoiceLine.CofinsVigentRateValue = 6m;
				invoiceLine.JI_ICMSRate = 7m;

				var antidumping = invoiceLine.Taxes.AddNew();
				antidumping.JLT_Type = Constants.RateCodes.Antidumping;
				antidumping.JLT_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.AdValoremRate;
				antidumping.JLT_Rate = 9m;

				return invoiceLine;
			}

			var invoiceLine1 = CreateInvoiceLine(1);
			var invoiceLine2 = CreateInvoiceLine(2);

			declaration.ResumeApportionment();

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("2000010001", ZDateTime.Now);

			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;

			var feeTypes = new string[] { ChargeTypesList.Codes.DTY, RateTypes.IPI, RateTypes.PIS, RateTypes.Cofins, RateTypes.ICMS, RateTypes.Antidumping, Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax, Core.Constants.Customs.Universal.RefCusTaxOrFee.Codes.NoDiscountCode, Core.Constants.Customs.Universal.RefCusTaxOrFee.Codes.FiftyPercentDiscountCode };
			var baseValue = 1m;
			feeTypes.ForEach(feeType =>
			{
				CreateFeeAndSetValues(entryLine, feeType, baseValue++);
			});

			var nfe = declaration.NFeExportObject.Entries.Cast<NFeEntryExportObject>().First();
			var nfeItem = nfe.Lines.Cast<NFeInvoiceLineExportObject>().First();

			CombineAssertions(() =>
			{
				AssertEquals("DutyBaseAmount", 11.11m, nfeItem.DutyBaseAmount);
				AssertEquals("DutyRate", 1m, nfeItem.DutyRate);
				AssertEquals("DutyAmount", 1.11m, nfeItem.DutyAmount);
				AssertEquals("IPIBaseAmount", 22.22m, nfeItem.IPIBaseAmount);
				AssertEquals("IPIRate", 2m, nfeItem.IPIRate);
				AssertEquals("IPIAmount", 2.22m, nfeItem.IPIAmount);
				AssertEquals("PISBaseAmount", 33.33m, nfeItem.PISBaseAmount);
				AssertEquals("PISRate", 4m, nfeItem.PISRate);
				AssertEquals("PISAmount", 3.33m, nfeItem.PISAmount);
				AssertEquals("CofinsBaseAmount", 44.44m, nfeItem.CofinsBaseAmount);
				AssertEquals("CofinsRate", 6m, nfeItem.CofinsRate);
				AssertEquals("CofinsAmount", 4.44m, nfeItem.CofinsAmount);
				AssertEquals("ICMSBaseAmount", 80.33m, nfeItem.ICMSBaseAmount);
				AssertEquals("ICMSRate", 7m, nfeItem.ICMSRate);
				AssertEquals("ICMSReductionPercentage", 5.36m, nfeItem.ICMSReductionPercentage);
				AssertEquals("ICMSAmount", 5.62m, nfeItem.ICMSAmount);
				AssertEquals("FCPRate", 8m, nfeItem.FCPRate);
				AssertEquals("FCPAmount", 6.43m, nfeItem.FCPAmount);
				AssertEquals("AntidumpingBaseAmount", 66.67m, nfeItem.AntidumpingBaseAmount);
				AssertEquals("AntidumpingRate", 9m, nfeItem.AntidumpingRate);
				AssertEquals("AntidumpingAmount", 6.67m, nfeItem.AntidumpingAmount);
				AssertEquals("AfrmmAmount", 3.8889m, nfeItem.AfrmmAmount);
				AssertEquals("ImportLicenseFineAmount", 18.89m, nfeItem.ImportLicenseFineAmount);
				AssertEquals("EICAmount", 3.79m, nfeItem.EICAmount);
				AssertEquals("IcmsTotalAmountReduction", 0m, nfeItem.IcmsTotalAmountReduction);
			});
		}

		void CreateFeeAndSetValues(CusEntryLine entryLine, string feeType, decimal baseValue)
		{
			var fee = entryLine.Fees.GetOrAddFeeByFeeType(feeType);
			fee.CF_BaseValue = baseValue * 11.1111m;
			fee.CF_ChargeAmount = baseValue * 1.1111m;
		}

		#region Overrides of BusinessObjectBaseTestCase

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NFeInvoiceLineExportObject(Factory.New<JobComInvoiceLine>());
		}

		#endregion
	}
}
