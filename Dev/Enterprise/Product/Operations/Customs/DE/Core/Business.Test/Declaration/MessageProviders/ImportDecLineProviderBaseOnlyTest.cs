using System.Linq;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.Business.CustomValues;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(ImportDecLineProvider))]
	sealed class ImportDecLineProviderBaseOnlyTest : ImportDecLineProviderAbstractTest<ImportDecLineProvider>
	{
		public void TestSequenceNumber()
		{
			entryLine.CL_LineNumber = 2;
			AssertEquals(2, Provider.SequenceNumber);
		}

		public void TestRequestedPreviousProcedure()
		{
			invoiceLine.JI_Procedure = "40005F0";
			AssertEquals("4000", Provider.RequestedPreviousProcedure);
		}

		public void TestGoodsDescription()
		{
			invoiceLine.JI_Description = "Schuhe mit Schleifen";
			AssertEquals("Schuhe mit Schleifen", Provider.GoodsDescription);
		}

		public void TestNetMassMeasure()
		{
			invoiceLine.JI_CustomsQuantity = 2.25;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_CustomsQuantity = 1.11;
			AssertEquals(3.36m, Provider.NetMassMeasure);
		}

		public void TestOriginCountry()
		{
			invoiceLine.JI_CountryOfOrigin = "RU";
			AssertEquals("RU", Provider.OriginCountry);
		}

		public void TestSupplementaryInformation()
		{
			var addOnInfo = Factory.New<GenAddOnColumn>();
			addOnInfo.XA_Name = "SupplementaryInformation";
			addOnInfo.XA_ParentTableCode = "JI";
			addOnInfo.XA_ParentID = invoiceLine.PK;
			addOnInfo.XA_Data = "Import Line Additional Information";
			AssertEquals("Import Line Additional Information", Provider.SupplementaryInformation);
		}

		public void TestCommodityCode()
		{
			invoiceLine.JI_Tariff = "12345";
			AssertEquals("12345", Provider.CommodityCode);
		}

		public void TestAdditionalProcedure()
		{
			invoiceLine.JI_Procedure = "40005F0";
			var code2 = invoiceLine.AdditionalProcedureCodes.AddNew();
			code2.CY_Type = EU.Business.CusCodeDataTypeList.Codes.AdditionalProcedureCode;
			code2.CY_Code = "40009F0";
			AssertContainsExactElementsInAnyOrder(new[] { "5F0", "9F0" }, Provider.AdditionalProcedure);
		}

		public void TestAdditionalProcedure_WrongType()
		{
			var addProc = invoiceLine.AdditionalProcedureCodes.AddNew();
			addProc.CY_Type = "NOT";
			addProc.CY_Code = "40009F0";
			AssertEquals(0, Provider.AdditionalProcedure.Count);
		}

		public void TestSupplementaryCodes()
		{
			invoiceLine.JI_SupplementaryCode1 = "7002";
			invoiceLine.JI_SupplementaryCode2 = "7003";
			var code3 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			code3.CY_Type = EU.Business.CusCodeDataTypeList.Codes.SupplementaryCode;
			code3.CY_Code = "7005";
			AssertContainsExactElementsInAnyOrder(new[] { "7002", "7003", "7005" }, Provider.SupplementaryCodes);
		}

		public void TestSupplementaryCodes_WrongCode()
		{
			var addProc = invoiceLine.AdditionalProcedureCodes.AddNew();
			addProc.CY_Type = "NOT";
			addProc.CY_Code = "7005";
			AssertEquals(0, Provider.SupplementaryCodes.Count);
		}

		public void TestPackage()
		{
			declaration.Packages.RemoveAndDeleteAll();

			var package1 = declaration.Packages.AddNew();
			var package1Pivot = package1.InvoiceLinePivotCollection.AddNew();
			package1Pivot.CHC_JI = invoiceLine.PK;
			package1Pivot.CHC_NumberOfPacks = 5;
			var package2 = declaration.Packages.AddNew();
			var package2Pivot = package2.InvoiceLinePivotCollection.AddNew();
			package2Pivot.CHC_JI = invoiceLine.PK;
			package2Pivot.CHC_NumberOfPacks = 5;
			CombineAssertions(() =>
			{
				var package = Provider.Package;
				AssertNotNull("Package not null", package);
				AssertEquals("Cached", package, Provider.Package);
			});
		}

		public void TestPackage_ManyPackagesOnTheLine()
		{
			declaration.Packages.RemoveAndDeleteAll();

			var package1 = declaration.Packages.AddNew();
			package1.CW_MarksAndNos = "AB123456";
			package1.CW_PackType = "BO";
			package1.CW_PackQty = 15;
			var package2 = declaration.Packages.AddNew();
			package2.CW_MarksAndNos = "AB478965";
			package2.CW_PackType = "AE";

			var package1Pivot = package1.InvoiceLinePivotCollection.AddNew();
			package1Pivot.CHC_JI = invoiceLine.PK;
			package1Pivot.CHC_NumberOfPacks = 5;

			CombineAssertions(() =>
			{
				var package = Provider.Package;
				AssertEquals("Cached", package, Provider.Package);
				AssertEquals("Mapped to Package 1: Marks and numbers", "AB123456", package.MarksNumbers);
				AssertEquals("Mapped to Package 1: Pack Type", "BO", package.Kind);
				AssertEquals("Mapped to Package 1: Pack Qty", 5, package.Quantity);
			});
		}

		public void TestForeignTradeStatisticsQuantity()
		{
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceLine.JI_LinePrice = 1245.45;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_LinePrice = 1.23;
			AssertEquals(1247m, Provider.ForeignTradeStatisticsQuantity);
		}

		public void TestForeignTradeStatisticsQuantity_RoundingValuesLessThan1()
		{
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceLine.JI_LinePrice = 0.1;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_LinePrice = 0.2;
			AssertEquals("Rounded value < 1 becomes 1", 1m, Provider.ForeignTradeStatisticsQuantity);
		}

		public void TestForeignTradeStatisticsGrossMassMeasure()
		{
			invoiceLine.JI_Weight = 550.58;
			invoiceLine.JI_WeightUQ = "KG";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_Weight = 1.23;
			invoiceLine2.JI_WeightUQ = "KG";
			AssertEquals(551.8m, Provider.ForeignTradeStatisticsGrossMassMeasure);
		}

		public void TestForeignTradeStatisticsGrossMassMeasureMixedUnits()
		{
			invoiceLine.JI_Weight = 100;
			invoiceLine.JI_WeightUQ = "LB";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_Weight = 10000;
			invoiceLine2.JI_WeightUQ = "G";
			AssertEquals(55.4m, Provider.ForeignTradeStatisticsGrossMassMeasure);
		}

		public void TestForeignTradeStatisticsGrossMassMeasureBothHeaderAndLines()
		{
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_Weight = 100;
			invoiceLine.JI_Weight = 550.58;
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			AssertEquals(decimal.Zero, Provider.ForeignTradeStatisticsGrossMassMeasure);
		}

		public void TestForeignTradeStatisticsGrossMassMeasure_Empty()
		{
			invoice.JZ_Weight = 0;
			invoiceLine.JI_Weight = 0;
			AssertEquals("GrossMassMeasure can't be zero on line, if zero on header", 0.1m, Provider.ForeignTradeStatisticsGrossMassMeasure);
		}

		public void TestForeignTradeStatisticsGrossMassMeasure_SmallValue()
		{
			invoice.JZ_Weight = 0;
			invoiceLine.JI_Weight = 0.03m;
			AssertEquals("0.1, if 0.03 rounds to 0", 0.1m, Provider.ForeignTradeStatisticsGrossMassMeasure);
		}

		public void TestAssessmentCustomsValue()
		{
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceLine.JI_LinePrice = 502.58;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_LinePrice = 1.21;
			AssertEquals(503.79m, Provider.AssessmentCustomsValue);
		}

		public void TestAssessmentCustomsValue_Format()
		{
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceLine.JI_LinePrice = 502.58;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_LinePrice = 1.22;
			AssertEquals("503.8", Provider.AssessmentCustomsValue.ToString());
		}

		public void TestAssessmentCustomsValue_DeclarationDV1()
		{
			declaration.ZG_IsHighValueOvrd = true;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceLine.JI_LinePrice = 502.58;
			AssertEquals(decimal.Zero, Provider.AssessmentCustomsValue);
		}

		public void TestAssessmentCustomsValue_ConcessionE01()
		{
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceLine.JI_LinePrice = 502.58;
			invoiceLine.JI_Procedure = "7005E01";
			AssertEquals(decimal.Zero, Provider.AssessmentCustomsValue);
		}

		public void TestAssessmentAmount()
		{
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;

			invoiceLine.JI_CustomsThirdQuantity = 10.58;
			invoiceLine.JI_CustomsThirdUnitQty = "KGM1";
			invoiceLine2.JI_CustomsThirdQuantity = 9.11;
			invoiceLine2.JI_CustomsThirdUnitQty = "KGM1";

			invoiceLine.JI_CustomsFourthQuantity = 10.58;
			invoiceLine.JI_CustomsFourthUnitQty = "KGM2";
			invoiceLine2.JI_CustomsFourthQuantity = 8.01;
			invoiceLine2.JI_CustomsFourthUnitQty = "KGM2";
			CombineAssertions(() =>
			{
				var assessmentAmount = Provider.AssessmentAmount;
				AssertEquals("Count", 2, assessmentAmount.Count);
				AssertEquals("Cached", assessmentAmount, Provider.AssessmentAmount);

				var amount = assessmentAmount.First();
				AssertEquals(19.69m, amount.Quantity);
				AssertEquals("KGM", amount.MeasurementUnit);
				AssertEquals("1", amount.Qualifier);

				var amount2 = assessmentAmount.Last();
				AssertEquals(18.59m, amount2.Quantity);
				AssertEquals("KGM", amount2.MeasurementUnit);
				AssertEquals("2", amount2.Qualifier);
			});
		}

		public void TestAssessmentAmount_EmptyCustomsThirdQty()
		{
			invoiceLine.JI_CustomsThirdQuantity = 0;
			invoiceLine.JI_CustomsThirdUnitQty = "KGM1";
			AssertEquals(false, Provider.AssessmentAmount.Any());
		}

		public void TestAssessmentAmount_EmptyCustomsFourthQty()
		{
			invoiceLine.JI_CustomsFourthQuantity = 0;
			invoiceLine.JI_CustomsFourthUnitQty = "KGM2";
			AssertEquals(false, Provider.AssessmentAmount.Any());
		}

		public void TestAssessmentSpecificRate()
		{
			var charge = invoiceLine.Charges.AddNew();
			charge.J7_ChargeType = ImportChargeCodeList.Codes.SRC;
			charge.J7_Amount = 105.40;

			CombineAssertions(() =>
			{
				var specialRates = Provider.AssessmentSpecificRate;
				AssertEquals("Count", 1, specialRates.Count);
				AssertEquals("Cached", specialRates, Provider.AssessmentSpecificRate);

				var specialRate = specialRates.First();
				AssertEquals(105.40m, specialRate.Value);
				AssertEquals("C", specialRate.Type);
			});
		}

		public void TestAssessmentSpecificRateSumByType()
		{
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;

			var charge = invoiceLine.Charges.AddNew();
			charge.J7_ChargeType = ImportChargeCodeList.Codes.SRC;
			charge.J7_Amount = 105.40;

			var charge2 = invoiceLine.Charges.AddNew();
			charge2.J7_ChargeType = ImportChargeCodeList.Codes.SRN;
			charge2.J7_Amount = 99.99;

			var charge3 = invoiceLine2.Charges.AddNew();
			charge3.J7_ChargeType = ImportChargeCodeList.Codes.SRC;
			charge3.J7_Amount = 10.10;

			CombineAssertions(() =>
			{
				var specialRates = Provider.AssessmentSpecificRate;
				AssertEquals("Count", 2, specialRates.Count);

				var specialRate1 = specialRates.FirstOrDefault(x => x.Type == "C");
				var specialRate2 = specialRates.FirstOrDefault(x => x.Type == "N");
				AssertEquals(115.50m, specialRate1.Value);
				AssertEquals(99.99m, specialRate2.Value);
			});
		}

		public void TestAssessmentSpecificRate_WrongType()
		{
			var charge = invoiceLine.Charges.AddNew();
			charge.J7_ChargeType = "SRT";
			charge.J7_Amount = 105.40;
			AssertEquals(0, Provider.AssessmentSpecificRate.Count);
		}

		public void TestAssessmentContentInformation()
		{
			invoiceLine.JI_CustomsThirdQuantity = 2020.20;
			var contentInfo = invoiceLine.ContentInformationTypes.AddNew();
			contentInfo.CY_Type = CusCodeDataTypeList.Codes.ContentInformationType;
			contentInfo.CY_Data = "0.02";
			contentInfo.CY_Code = "E";

			CombineAssertions(() =>
			{
				var conetentInfos = Provider.AssessmentContentInformation;
				AssertEquals("Count", 1, conetentInfos.Count);
				AssertEquals("Cached", conetentInfos, Provider.AssessmentContentInformation);

				var result = conetentInfos.First();
				AssertEquals(0.02m, result.DegreePercentage);
				AssertEquals("E", result.ContentType);
			});
		}

		public void TestAssessmentContentInformation_Empty()
		{
			invoiceLine.JI_CustomsThirdQuantity = 0;
			var contentInfo = invoiceLine.ContentInformationTypes.AddNew();
			contentInfo.CY_Type = CusCodeDataTypeList.Codes.ContentInformationType;
			contentInfo.CY_Data = "0.02";
			contentInfo.CY_Code = "E";
			AssertEquals(0, Provider.AssessmentContentInformation.Count);
		}

		public void TestAssessmentContentInformation_WrongType()
		{
			invoiceLine.JI_CustomsThirdQuantity = 2020.20;
			var contentInfo = invoiceLine.ContentInformationTypes.AddNew();
			contentInfo.CY_Type = "CTY";
			contentInfo.CY_Data = "0.02";
			contentInfo.CY_Code = "E";
			AssertEquals(0, Provider.AssessmentContentInformation.Count);
		}

		public void TestExciseDuty()
		{
			var tariff = invoiceLine.CusLineTariffDetails.AddNew();
			tariff.BZ_Tariff = "ABC";
			tariff.BZ_PercentAlcohol = 12.5;
			tariff.BZ_TobaccoRetailPrice = 4.50;
			tariff.BZ_Qty1 = 10;
			tariff.BZ_UQ1 = "KGMS";
			tariff.BZ_Type = "EXC";
			tariff.BZ_ParentTableCode = "JI";

			CombineAssertions(() =>
			{
				var exciseDuties = Provider.ExciseDuty;
				AssertEquals("Count", 1, exciseDuties.Count);
				AssertEquals("Cached", exciseDuties, Provider.ExciseDuty);

				var result = exciseDuties.First();
				AssertEquals("ABC", result.Code);
				AssertEquals(12.5m, result.DegreePercentage);
				AssertEquals(45m, result.Value);

				var amount = result.Amount;
				AssertEquals("Cached", amount, result.Amount);
				AssertEquals(10m, amount.Quantity);
				AssertEquals("KGM", amount.MeasurementUnit);
				AssertEquals("S", amount.Qualifier);
			});
		}

		public void TestExciseDuty_WrongType()
		{
			var tariff = invoiceLine.CusLineTariffDetails.AddNew();
			tariff.BZ_Tariff = "ABC";
			tariff.BZ_PercentAlcohol = 12.5;
			tariff.BZ_TobaccoRetailPrice = 4.50;
			tariff.BZ_Qty1 = 10;
			tariff.BZ_UQ1 = "KGMS";
			tariff.BZ_Type = "EXT";
			tariff.BZ_ParentTableCode = "JI";

			AssertEquals(0, Provider.ExciseDuty.Count);
		}

		public void TestDocuments()
		{
			var document1 = invoiceLine.SupportingDocuments.AddNew();
			document1.CSI_Code = "AAAA";
			invoiceLine.SupportingDocuments.AddNew();

			var documents = Provider.Documents;
			AssertEquals("Count", 2, documents.Count);
			AssertEquals("Cached", documents, Provider.Documents);
		}

		public void TestDocumentWriteOffQuantity()
		{
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;

			var document1 = invoiceLine.SupportingDocuments.AddNew();
			document1.CSI_Code = "AAAA";
			document1.CSI_ReferenceNumber = "REF1";
			document1.CSI_Quantity = 10.1m;
			var document2 = invoiceLine.SupportingDocuments.AddNew();
			document2.CSI_Code = "BBBB";
			document2.CSI_ReferenceNumber = "REF1";
			document2.CSI_Quantity = 12.2m;
			var document3 = invoiceLine.SupportingDocuments.AddNew();
			document3.CSI_Code = "AAAA";
			document3.CSI_ReferenceNumber = "REF2";
			document3.CSI_Quantity = 13.3m;
			var document4 = invoiceLine2.SupportingDocuments.AddNew();
			document4.CSI_Code = "AAAA";
			document4.CSI_ReferenceNumber = "REF1";
			document4.CSI_Quantity = 14.4m;

			var documents = Provider.Documents;
			CombineAssertions(() =>
			{
				AssertEquals("Count", 3, documents.Count);
				AssertEquals("Type AAAA REF1", 24.5m, documents.First(x => x.DocumentType == "AAAA" && x.ReferenceNumber == "REF1").WriteOff.Quantity);
				AssertEquals("Type BBBB REF1", 12.2m, documents.First(x => x.DocumentType == "BBBB" && x.ReferenceNumber == "REF1").WriteOff.Quantity);
				AssertEquals("Type AAAA REF2", 13.3m, documents.First(x => x.DocumentType == "AAAA" && x.ReferenceNumber == "REF2").WriteOff.Quantity);
			});
		}

		public void TestDocumentDistinctByTypeAndReference()
		{
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;

			var document1 = invoiceLine.SupportingDocuments.AddNew();
			document1.CSI_Code = "AAAA";
			document1.CSI_ReferenceNumber = "REF1";
			var document2 = invoiceLine.SupportingDocuments.AddNew();
			document2.CSI_Code = "BBBB";
			document2.CSI_ReferenceNumber = "REF1";
			var document3 = invoiceLine.SupportingDocuments.AddNew();
			document3.CSI_Code = "AAAA";
			document3.CSI_ReferenceNumber = "REF2";
			var document4 = invoiceLine2.SupportingDocuments.AddNew();
			document4.CSI_Code = "AAAA";
			document4.CSI_ReferenceNumber = "REF1";
			var document5 = invoiceLine2.SupportingDocuments.AddNew();
			document5.CSI_Code = "CCCC";
			document5.CSI_ReferenceNumber = "REF1";

			var documents = Provider.Documents;
			CombineAssertions(() =>
			{
				AssertEquals("Count", 4, documents.Count);
				AssertEquals("Type AAAA REF1", 1, documents.Count(x => x.DocumentType == "AAAA" && x.ReferenceNumber == "REF1"));
				AssertEquals("Type BBBB REF1", 1, documents.Count(x => x.DocumentType == "BBBB" && x.ReferenceNumber == "REF1"));
				AssertEquals("Type AAAA REF2", 1, documents.Count(x => x.DocumentType == "AAAA" && x.ReferenceNumber == "REF2"));
				AssertEquals("Type CCCC REF1", 1, documents.Count(x => x.DocumentType == "CCCC" && x.ReferenceNumber == "REF1"));
			});
		}

		protected override ImportDecLineProvider GetProvider() => new ImportDecLineProviderForTest(entryLine);

		internal sealed class ImportDecLineProviderForTest : ImportDecLineProvider
		{
			public ImportDecLineProviderForTest(CusEntryLine entryLine) : base(entryLine)
			{
			}
		}
	}
}
