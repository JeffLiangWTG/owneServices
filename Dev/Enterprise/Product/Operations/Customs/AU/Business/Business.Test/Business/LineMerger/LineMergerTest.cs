using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class LineMergerTest : TransactionedTestCase
	{
		public void TestUtiliseIsLandedCostOnly()
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var importer = factory.New<OrgHeader>();
			importer.MiscServ.OM_IMIsGSTDeferred = true;
			declaration.JE_OH_Importer = importer.PK;
			declaration.Importer.MiscServ.OM_IMShowDutyOnWarehouseEntries = true;

			declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew("OFT", 500, "AUD");

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine1_1 = invoice1.JobComInvoiceLines.AddNew();
			var invoiceLine2_1 = invoice1.JobComInvoiceLines.AddNew();

			SetDefaultValuesToInvoiceLines(invoice1);

			invoiceLine1_1.JI_LinePrice = 1000m;
			invoiceLine1_1.JI_IsPackToBondForLine = true;
			invoiceLine1_1.JI_Tariff = "22042190 35";
			invoiceLine2_1.JI_LinePrice = 4000m;
			invoiceLine2_1.JI_IsPackToBondForLine = false;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("One header", 1, declaration.CustomsEntryHeaders.Count);
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("Two lines", 2, entryHeader.MergedLines.Count);

			Assert(invoiceLine1_1.CusEntryLine.Fees.Cast<CusEntryLineFee>().Any(x => x.CF_IsLandedCostOnly));
			Assert(invoiceLine2_1.CusEntryLine.Fees.Cast<CusEntryLineFee>().All(x => !x.CF_IsLandedCostOnly));
		}

		public void TestUpdateEntryLineTILV()
		{
			JobDeclaration declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew("OFT", 500, "AUD");

			JobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine1_1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1_1.JI_LinePrice = 1000m;

			JobComInvoiceLine invoiceLine2_1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2_1.JI_LinePrice = 4000m;

			JobComInvoiceLine invoiceLine3_1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine3_1.JI_LinePrice = 5000m;

			invoiceLine3_1.AddInfo.ZA_TILV = "0 AUD";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("100.00 AUD", invoiceLine1_1.CusEntryLine.EntryLineAddInfo.TILVMoney.ToString());
			AssertEquals("400.00 AUD", invoiceLine2_1.CusEntryLine.EntryLineAddInfo.TILVMoney.ToString());
			AssertEquals("0.00 AUD", invoiceLine3_1.CusEntryLine.EntryLineAddInfo.TILVMoney.ToString());
		}

		public void TestProblemWithCachedDutyCalculator()
		{
			CMRTariffRatePeriodSnapshot tariff = CMRTariffRatePeriodSnapshot.New(factory);
			tariff.TT_CustomsValueRate = 10m;
			tariff.TT_QuantityUnit = "NO";
			tariff.TT_QuantityRate = 12000m;
			tariff.TT_PreferenceSchemeType = "GEN";
			tariff.TT_TariffClassificationNumber = "00000000";
			tariff.TT_RateNumber = "001";
			tariff.TT_StartDate = new ZDateTime(1900, 1, 1);

			JobDeclaration testDec = JobDeclaration.New(factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			testDec.JE_DateOfFirstArrival = new ZDateTime(1900, 1, 1);

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();

			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_InvoiceAmount = 10000m;
			line.JI_Tariff = "0000000014";
			line.JI_LinePrice = 10000m;
			line.AddInfo.ZA_ORG = "US";
			line.AddInfo.ZA_PST = "GEN";
			line.JI_CustomsUnitQty = "NO";
			line.JI_CustomsQuantity = 1;

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();

			CusEntryLine entryLine = line.CusEntryLine;
			AssertEquals("Duty calculated", 13000m, entryLine.DutyAmount);

			line.JI_CustomsQuantity = 2;
			merger = new LineMerger(testDec);
			merger.DoMerge();

			entryLine = line.CusEntryLine;
			AssertEquals("Duty calculated again", 25000m, entryLine.DutyAmount);
		}

		public void TestCalculateDeclarationChargeForCMR()
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var tax1 = helper.CreateTaxOrFee("DSN", 50M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			tax1.ZZF_Threshold = 10000m;
			var tax2 = helper.CreateTaxOrFee("DSH", 152M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			tax2.ZZF_Threshold = 10000m;
			helper.CreateTaxOrFee("Q1S", 42M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			factory.Save();

			JobDeclaration testDec = JobDeclaration.New(factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_TransportMode = Core.Constants.TransportModes.Sea;

			CusContainer container = testDec.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(invoice);
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var deminimus = UniversalReferenceHelper.GetDeminimus(factory);

			invoice.JZ_InvoiceAmount = deminimus + 1;
			line.JI_LinePrice = deminimus + 1;

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();
			AssertEquals("One CusEntryHeader", 1, testDec.CustomsEntryHeaders.Count);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];
			AssertEquals("DeclarationCharge should have been calculated", true, entryHeader.Charges.GetAmount(CusEntryChargeTypeList.Codes.DeclarationProcessingCharge) > 0m);
			AssertEquals("AQIS Processing Charge should have been calculated", true, entryHeader.Charges.GetAmount(CusEntryChargeTypeList.Codes.AQISProcessingCharge) > 0m);
			AssertEquals("AQIS Container Charge should not have been calculated - container charges have been removed with AQIS Bio-security fee changes of 1-Dec-2015", false, entryHeader.Charges.GetAmount(CusEntryChargeTypeList.Codes.AQISContainerCharges) > 0m);
		}

		public void TestDutyOverridenNotSavedEntryFeeForCMR()
		{
			JobDeclaration testDec = JobDeclaration.New(factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();

			SetDefaultValuesToInvoiceLines(invoice);
			line.AddInfo.ZA_DTY = 200m;
			line2.AddInfo.ZA_DTY = 100m;

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();
			AssertEquals("Merged lines", 1, testDec.CustomsEntryHeaders[0].MergedLines.Count);

			CusEntryLine entryLine = testDec.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("Duty overriden amount saved with 'DTO'", 0m, entryLine.Fees.GetAmount(CusEntryChargeTypeList.Codes.DutyOverride));
			AssertEquals("Duty overriden amount saved with 'DTY'", 300m, entryLine.Fees.GetAmount(CusEntryChargeTypeList.Codes.DutyAmount));
		}

		public void TestStandardDutyAmountIsPersisted()
		{
			JobDeclaration testDec = JobDeclaration.New(factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();

			SetDefaultValuesToInvoiceLines(invoice);
			line.AddInfo.ZA_STD = 200m;
			line2.AddInfo.ZA_STD = 100m;

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();
			AssertEquals("Merged lines", 1, testDec.CustomsEntryHeaders[0].MergedLines.Count);

			CusEntryLine entryLine = testDec.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("Standard duty overriden", 300m, entryLine.Fees.GetAmount(CusEntryChargeTypeList.Codes.StandardDutyOverriden));
		}

		public void TestIsCalculatingDuty()
		{
			var testDec = JobDeclaration.New(factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			var importer = factory.New<OrgHeader>();
			importer.OH_Code = "AUI";
			importer.AUIsDutyDeferred = true;
			testDec.JE_OH_Importer = importer.PK;
			factory.Save();

			var invoice = testDec.Invoices.AddNew();
			_ = invoice.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(invoice);

			var merger = new LineMerger(testDec);
			merger.DoMerge();

			var entryHeader = (CusEntryHeader)testDec.ActiveEntryHeaders[0];
			entryHeader.IsCalculatingDuty = true;
			merger.DoMerge();
			AssertEquals(false, entryHeader.IsCalculatingDuty);
		}

		public void TestFlatRateAndFlatUQIsSet()
		{
			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var helper = new UniversalReferenceTestDataHelper(factory);
				var impTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Import);
				var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "2204219035", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Tariff Code Description", taxOrFeeCode: "GEN");
				helper.CreateTariffUOM(tariff1, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "LA");
				helper.CreateTariffUOM(tariff1, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.AdditionalUOMType, "L");
				factory.Save();

				var testDec = JobDeclaration.New(factory);
				testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

				var invoice = testDec.Invoices.AddNew();
				invoice.JZ_RX_NKInvoice_Currency = "AUD";

				var line = invoice.JobComInvoiceLines.AddNew();
				line.JI_LinePrice = 10000m;
				line.AddInfo.ZA_PST = "GEN";
				line.JI_Tariff = "22042190 35";

				testDec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

				var entryLine = line.CusEntryLine;
				AssertEquals("Flat rate is set", 62.64000m, entryLine.CL_FlatAmount);
				AssertEquals("Flat rate UQ is set", "LA", entryLine.CL_FlatAmountUQ);

				line.JI_CustomsQuantity = 2m;
				testDec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				AssertEquals("Duty Calculated", 625.28m, entryLine.DutyAmount);
			}
		}

		public void TestFlatRateAndFlatUQIsSet_AUCAHECC()
		{
			var testDec = JobDeclaration.New(factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			var invoice = testDec.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "AUD";

			var line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 10000m;
			line.AddInfo.ZA_PST = "GEN";
			line.JI_Tariff = "22042190 35";

			testDec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entryLine = line.CusEntryLine;
			AssertEquals("Flat rate is set", 62.64000m, entryLine.CL_FlatAmount);
			AssertEquals("Flat rate UQ is set", "LA", entryLine.CL_FlatAmountUQ);

			line.JI_CustomsQuantity = 2m;
			testDec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Duty Calculated", 625.28m, entryLine.DutyAmount);
		}

		public void TestAssignLineNumbersForCMR()
		{
			JobDeclaration testDec = JobDeclaration.New(factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();

			SetDefaultValuesToInvoiceLines(invoice);

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();
			AssertEquals("One header", 1, testDec.CustomsEntryHeaders.Count);
			AssertEquals("One line", 1, testDec.CustomsEntryHeaders[0].MergedLines.Count);

			JobComInvoiceLine line3 = invoice.JobComInvoiceLines.AddNew();
			line3.JI_Tariff = "Test";
			merger = new LineMerger(testDec);
			merger.DoMerge();
			AssertEquals("One header", 1, testDec.CustomsEntryHeaders.Count);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];
			AssertEquals("Two lines", 2, entryHeader.MergedLines.Count);

			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			entryHeader.EntryNumber = "Num";

			JobComInvoiceLine line4 = invoice.JobComInvoiceLines.AddNew();
			line4.JI_Tariff = "Test2";
			merger = new LineMerger(testDec);
			merger.DoMerge();
			AssertEquals("One header", 1, testDec.CustomsEntryHeaders.Count);
			entryHeader = testDec.CustomsEntryHeaders[0];
			AssertEquals("Three lines", 3, entryHeader.MergedLines.Count);
			AssertEquals("Line number for new line", true, line4.MergedLineNumber.StartsWith("3"));
		}

		public void TestCountervailingSecurityAmount()
		{
			AssertAggregateAmount(Customs.AU.Declaration.Business.AUAddInfo.Schema.ZA_CSA, CusEntryChargeTypeList.Codes.CountervailingSecurityAmount);
		}

		public void TestDumpingSecurityAmount()
		{
			AssertAggregateAmount(Customs.AU.Declaration.Business.AUAddInfo.Schema.ZA_DSA, CusEntryChargeTypeList.Codes.DumpingSecurityAmount);
		}

		public void TestCountervailingDuty()
		{
			AssertAggregateAmount(Customs.AU.Declaration.Business.AUAddInfo.Schema.ZA_CVD, CusEntryChargeTypeList.Codes.CountervailingDuty);
		}

		public void TestInterimCountervailingDuty()
		{
			AssertAggregateAmount(Customs.AU.Declaration.Business.AUAddInfo.Schema.ZA_ICV, CusEntryChargeTypeList.Codes.InterimCountervailingDuty);
		}

		public void TestDumpingDuty()
		{
			AssertAggregateAmount(Customs.AU.Declaration.Business.AUAddInfo.Schema.ZA_DMP, CusEntryChargeTypeList.Codes.DumpingDuty);
		}

		public void TestInterimDumpingDuty()
		{
			AssertAggregateAmount(Customs.AU.Declaration.Business.AUAddInfo.Schema.ZA_IDP, CusEntryChargeTypeList.Codes.InterimAntiDumpingDuty);
		}

		public void TestDuty()
		{
			AssertAggregateAmount(Customs.AU.Declaration.Business.AUAddInfo.Schema.ZA_DTY, CusEntryChargeTypeList.Codes.DutyOverride);
		}

		void AssertAggregateAmount(string addInfoColumnName, string entryChargeType)
		{
			JobDeclaration testDec = factory.New<JobDeclaration>();
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();

			SetDefaultValuesToInvoiceLines(invoice);
			line1.AddInfo[addInfoColumnName] = 100m;
			line2.AddInfo[addInfoColumnName] = 150m;

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();

			AssertEquals("One header", 1, testDec.CustomsEntryHeaders.Count);
			AssertEquals("One line", 1, testDec.CustomsEntryHeaders[0].MergedLines.Count);
			CusEntryLine entryLine = testDec.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals(entryChargeType, 250m, entryLine.Fees.GetOrAddFeeByFeeType(entryChargeType).CF_ChargeAmount);
		}

		#region Merge Customs Header and Line Count

		/// <summary>
		/// A Header with two invoice lines that have the same details except for Second Quantity
		/// Expected Result : One Cus Entry header with one cus entry line that has aggregated second quantities
		/// </summary>
		public void TestAddUpSecondQuantityFromAddInfo()
		{
			testJobDeclaration.JE_DateOfFirstArrival = new ZDateTime(2003, 11, 15);
			testJobDeclaration.JE_ExportDate = new ZDateTime(2003, 11, 15);
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			header1.JZ_InvoiceAmount = 1000;
			header1.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			header1.JZ_ValuationBasis = "UT";
			header1.JZ_Nature10PackCount = 10;
			SetDefaultValuesToInvoiceLines(header1);

			JobComInvoiceLine line1 = header1.JobComInvoiceLines.AddNew();
			line1.AddInfo.ZA_UQ2 = "L";
			line1.AddInfo.ZA_QT2 = 10m;
			JobComInvoiceLine line2 = header1.JobComInvoiceLines.AddNew();
			line2.AddInfo.ZA_UQ2 = "L";
			line2.AddInfo.ZA_QT2 = 20m;

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();

			AssertEquals("There should be one Cus Entry Header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("There should be one Cus Entry lines", 1, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("Second Quantity", 30m, testJobDeclaration.CustomsEntryHeaders[0].MergedLines[0].SecondQuantity);
		}

		//Both headers and lines are nature10
		public void TestMergeOnSameNature()
		{
			testJobDeclaration.JE_DateOfFirstArrival = new ZDateTime(2003, 11, 15);
			testJobDeclaration.JE_ExportDate = new ZDateTime(2003, 11, 15);
			testJobDeclaration.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 10000m, "AUD");
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			header1.JZ_InvoiceAmount = 1000;
			header1.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			header1.JZ_ValuationBasis = "UT";
			header1.JZ_Nature10PackCount = 10;
			line1 = header1.JobComInvoiceLines.AddNew();
			line1.JI_InvoiceQuantity = 10;
			line1.JI_InvoiceUQ = "NO";
			line1.JI_Tariff = "2203.00.31";
			line1.JI_LinePrice = 900;

			header2 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header2.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			header2.JZ_InvoiceAmount = 1000;
			header2.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			header2.JZ_ValuationBasis = "UT";
			header2.JZ_Nature10PackCount = 20;
			line2 = header2.JobComInvoiceLines.AddNew();
			line2.JI_InvoiceQuantity = 10;
			line2.JI_InvoiceUQ = "LA";
			line2.JI_Tariff = "2402.20.20";
			line2.JI_LinePrice = 500;

			testJobDeclaration.DoMerge();
			AssertEquals("One Entry Header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("Two Entry Lines", 2, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		/// <summary>
		/// One invoice header with two lines that have the same details except for instrument type and code
		/// Expected result : One Cus Entry header with two lines
		/// </summary>
		public void TestDoMergeWithDifferentInstrumentCode()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			header1.JZ_InvoiceAmount = 1000.0m;

			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();

			header1.JZ_OH_Supplier = GetValidSupplier();
			SetDefaultValuesToInvoiceLines(header1);
			SetSpecificValueToInvoiceLines(line1, JobComInvoiceLine.Schema.JI_ConcessionOrder, "BL");
			SetSpecificValueToInvoiceLines(line2, JobComInvoiceLine.Schema.JI_ConcessionOrder, "MD1");

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One Customs Entry Header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("Merged Line Count", 2, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		/// <summary>
		/// One invoice header with three lines that have exactly same tariff, statistical codes, treatment codes and add info string
		/// Expected Result : all lines are merged
		/// </summary>
		public void TestDoMergeWithOneSupplier()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			header1.JZ_InvoiceAmount = 1000.0m;

			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			line3 = header1.JobComInvoiceLines.AddNew();

			header1.JZ_OH_Supplier = GetValidSupplier();
			SetDefaultValuesToInvoiceLines(header1);

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One Customs Entry Header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("Merged Line Count", 1, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestMergeLinesWithDifferentEffectiveDutyDate()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testJobDeclaration.JE_ExportDate = DateTime.Today;

			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_RX_NKInvoice_Currency = uSDCurrency.RX_Code;
			header1.JZ_InvoiceAmount = 1000.0m;
			line1 = header1.JobComInvoiceLines.AddNew();
			header1.JZ_OH_Supplier = GetValidSupplier();
			SetDefaultValuesToInvoiceLines(header1);

			header2 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header2.JZ_RX_NKInvoice_Currency = uSDCurrency.RX_Code;
			header2.JZ_InvoiceAmount = 2000m;
			line2 = header2.JobComInvoiceLines.AddNew();
			header2.JZ_OH_Supplier = header1.JZ_OH_Supplier;
			SetDefaultValuesToInvoiceLines(header2);

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("There should be one custom entry header", 1, testJobDeclaration.CustomsEntryHeaders.Count);

			header1.AddInfo.AddInfoLine = "EFD=010204";
			header2.AddInfo.AddInfoLine = "EFD=020204";
			merger.DoMerge();
			AssertEquals("There should be two custom entry headers", 2, testJobDeclaration.CustomsEntryHeaders.Count);
		}

		public void TestMergedLinesAreMergedInLineOrder()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_OH_Supplier = GetValidSupplier();
			header1.JZ_InvoiceAmount = 1000.0m;
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;

			const string FirstTariff = "4901.99.90 05";
			line1 = header1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = FirstTariff;

			const string SecondTariff = "4819.20.00 10";
			line2 = header1.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = SecondTariff;

			const string ThirdTariff = "4202.91.90 22";
			line3 = header1.JobComInvoiceLines.AddNew();
			line3.JI_Tariff = ThirdTariff;

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			CusEntryLineCollection mergedLines = testJobDeclaration.CustomsEntryHeaders[0].MergedLines;
			AssertEquals("Precondition : Merged Line Count", 3, mergedLines.Count);
			AssertEquals("First Tariff", FirstTariff, mergedLines[0].RandomLine.JI_Tariff);
			AssertEquals("Second Tariff", SecondTariff, mergedLines[1].RandomLine.JI_Tariff);
			AssertEquals("Third Tariff", ThirdTariff, mergedLines[2].RandomLine.JI_Tariff);
		}

		[ExpectNoExceptions()]
		public void TestDoMergeTwice()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			line3 = header1.JobComInvoiceLines.AddNew();

			header1.JZ_OH_Supplier = GetValidSupplier();
			SetDefaultValuesToInvoiceLines(header1);

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(testJobDeclaration.MergeManager);
			factory.Save();
			merger.DoMerge();
			factory.Save();
		}

		/// <summary>
		/// One invoice header with three invoice lines.
		/// Two of the lines have the exactly same details
		/// The other line has a different TreatmentCode
		/// Expected Result : We end up with two merged lines in one CusEntryHeader
		/// </summary>
		public void TestDoMergeWithOneSupplierOnTariffAndDifferentOneField()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			line3 = header1.JobComInvoiceLines.AddNew();

			header1.JZ_OH_Supplier = GetValidSupplier();
			SetDefaultValuesToInvoiceLines(header1);
			line3.AddInfo.ZA_TreatmentCode_Hidden = treatmentCode2;

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();

			AssertEquals("Customs Entry Header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("Merged Line Count", 2, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		/// <summary>
		/// A declaration has two invoice headers that have the same details.
		/// Each has three invoice lines that all have the exactly same details
		/// Expected Result : We end up with 1 CusEntryHeader that has 1 merged line.
		/// </summary>
		public void TestDoMergeWithSameSupplierForTwoHeaders()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_OH_Supplier = GetValidSupplier();
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			header1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			line3 = header1.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header1);

			header2 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header2.JZ_OH_Supplier = GetValidSupplier();
			header2.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			header2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			header2.JobComInvoiceLines.AddNew();
			header2.JobComInvoiceLines.AddNew();
			header2.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header2);

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One Custom Entry Header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("Merged Line Count", 1, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		/// <summary>
		/// A declaration has two headers that have different suppliers. Each header has three invoice lines
		/// All six lines have the same details
		/// Expected Result : Two CusEntryHeaders that have one merged line
		/// </summary>
		public void TestDoMergeWithDifferentSuppliersForTwoHeaders()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testJobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_OH_Supplier = GetValidSupplier();
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			line3 = header1.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header1);

			header2 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header2.JZ_OH_Supplier = GetValidSupplier2(header1.JZ_OH_Supplier);
			header2.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			header2.JobComInvoiceLines.AddNew();
			header2.JobComInvoiceLines.AddNew();
			header2.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header2);

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One Custom Entry Header", 2, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("Merged Line Count", 1, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("Merged Line Count", 1, testJobDeclaration.CustomsEntryHeaders[1].MergedLines.Count);
		}

		/// <summary>
		/// A declaration has 'No merge' option
		/// An invoice header that has three invoice lines that have the same part numbers
		/// Expected result : One cus entry header and three entry lines
		/// </summary>
		public void TestDontMerge()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_OH_Supplier = GetValidSupplier();
			header1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			line3 = header1.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header1);

			AUOrgSupplierPart part = factory.New<AUOrgSupplierPart>();
			OrgPartRelation relation = part.RelatedOrganisations.AddNew();
			relation.OU_OH = header1.JZ_OH_Supplier;
			part.OP_PartNum = "Shoes";

			factory.Save();

			line1.JI_PartNo = part.OP_PartNum;
			line2.JI_PartNo = part.OP_PartNum;
			line3.JI_PartNo = part.OP_PartNum;

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One cus entry header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("Three cus entry lines", 3, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		/// <summary>
		/// A declaration has two invoice headers that have the same supplier, but have different incoterms
		/// All lines have the same details
		/// Expected Result : We end up with two cus entry headers, each of which has one merged line.
		/// </summary>
		public void TestDoMergeWithSameSupplierButDifferentIncoTermsForTwoHeaders()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testJobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_OH_Supplier = GetValidSupplier();
			header1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			line3 = header1.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header1);

			header2 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header2.JZ_OH_Supplier = GetValidSupplier();
			header2.JZ_IncoTerm = Core.Constants.IncoTerms.ExWorks;
			header2.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			header2.JobComInvoiceLines.AddNew();
			header2.JobComInvoiceLines.AddNew();
			header2.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header2);

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("Two Customs Entry Header", 2, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("Merged Line Count", 1, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("Merged Line Count", 1, testJobDeclaration.CustomsEntryHeaders[1].MergedLines.Count);
		}

		public void TestMergeTwoCurrencyInvoices()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testJobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_OH_Supplier = GetValidSupplier();
			header1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			line3 = header1.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header1);

			header2 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header2.JZ_OH_Supplier = GetValidSupplier();
			header2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			header2.JZ_RX_NKInvoice_Currency = uSDCurrency.RX_Code;
			header2.JobComInvoiceLines.AddNew();
			header2.JobComInvoiceLines.AddNew();
			header2.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header2);

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One Custom Entry Header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("Merged Line Count", 1, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestGetOrderedAddInfo()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			SetSpecificValueToInvoiceLines(line1, "JI_AddInfo", addInfo1);
			SetSpecificValueToInvoiceLines(line2, "JI_AddInfo", addInfo2);

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();

			AssertEquals("One Customs Entry header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("One Customs Entry line", 1, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		/// <summary>
		/// A declaration has two invoice headers. One is nature 20. The other is nature 30.
		/// Each has three lines which have the same details.
		/// Expected Result: Two Cus Entry Headers and each header has one merged line
		/// </summary>
		public void TestCreateCusEntryHeaderWithDifferentNatures()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testJobDeclaration.WarehouseDocAddress.E2_OA_Address = GetValidWarehouseAddress().PK;
			testJobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_OH_Supplier = GetValidSupplier();
			header1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			header1.JZ_BondPackCount = 20;//Nature20
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			line3 = header1.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header1);

			header2 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header2.JZ_OH_Supplier = GetValidSupplier();
			header2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			header2.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			header2.JZ_Nature10PackCount = 10;//Nature10
			header2.JobComInvoiceLines.AddNew();
			header2.JobComInvoiceLines.AddNew();
			header2.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header2);

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("Two Customs Entry Headers created", 2, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("One Merged Line for the first header", 1, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("One Merged Line for the first header", 1, testJobDeclaration.CustomsEntryHeaders[1].MergedLines.Count);
		}

		/// <summary>
		/// A declaration has two invoice headers. One is valuation basis 'UT'. The other is 'RT'.
		/// Each has three lines which have the same details.
		/// Expected Result: Two Cus Entry Headers and each header has one merged line
		/// </summary>
		public void TestCreateCusEntryHeaderWithDifferentValuationBasis()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testJobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_OH_Supplier = GetValidSupplier();
			header1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			header1.AddInfo.ZA_ValuationBasis_Hidden = "UT";
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			line3 = header1.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header1);

			header2 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header2.JZ_OH_Supplier = GetValidSupplier();
			header2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			header2.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			header2.AddInfo.ZA_ValuationBasis_Hidden = "RT";
			header2.JobComInvoiceLines.AddNew();
			header2.JobComInvoiceLines.AddNew();
			header2.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header2);

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("Two Customs Entry Headers created", 2, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("One Merged Line for the first header", 1, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("One Merged Line for the first header", 1, testJobDeclaration.CustomsEntryHeaders[1].MergedLines.Count);
		}

		/// <summary>
		/// A declaration has one invoice header which has three invoice lines.
		/// Those lines have different part numbers
		/// Expected Result : One cus entry header and three cus entry lines
		/// </summary>
		public void TestMergeByPartNumWithDifferentPartNumbers()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			line3 = header1.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header1);
			SetSpecificValueToInvoiceLines(line1, JobComInvoiceLine.Schema.JI_PartNo, "Part1");
			SetSpecificValueToInvoiceLines(line2, JobComInvoiceLine.Schema.JI_PartNo, "Part2");
			SetSpecificValueToInvoiceLines(line3, JobComInvoiceLine.Schema.JI_PartNo, "Part3");

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One Customs Entry Header created", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("Three Lines for the header", 3, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		/// <summary>
		/// A declaration has one invoice header which has three invoice lines.
		/// Those lines have the same part number
		/// Expected Result : One cus entry header and one cus entry lines
		/// </summary>
		public void TestMergByPartNumWithSamePartNumber()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			line3 = header1.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header1);

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One Customs Entry Header created", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("Three Lines for the header", 1, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestCreateHeaderAndLineForExportDeclaration()
		{
			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			line3 = header1.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header1);

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One Customs Entry Header created", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("Three Lines for the header", 1, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		/// <summary>
		/// A declaration has one invoice header that has three invoice lines
		/// Three lines have the same lookup code, but different part numbers.
		/// Mergeby Lookup Codes should result in one merged line
		/// </summary>
		public void TestMergeByLookupCodeWithSameLookupCodesButWithDifferentPartNums()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			line3 = header1.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header1);
			SetSpecificValueToInvoiceLines(line1, JobComInvoiceLine.Schema.JI_PartNo, "Part1");
			SetSpecificValueToInvoiceLines(line2, JobComInvoiceLine.Schema.JI_PartNo, "Part2");
			SetSpecificValueToInvoiceLines(line3, JobComInvoiceLine.Schema.JI_PartNo, "Part3");

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One Customs Entry Header created", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("One line for the header", 1, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		/// <summary>
		/// A declaration has one invoice header that has three invoice lines
		/// Three lines have the different lookup code.
		/// Mergeby Lookup Codes should result in three merged lines
		/// </summary>
		public void TestMergeByLookupCodeWithDifferentLookupCodes()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			line3 = header1.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header1);

			SetClassificationToInvoiceLine(line1);
			SetClassificationToInvoiceLine(line2);
			SetClassificationToInvoiceLine(line3);

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One Customs Entry Header created", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("Three Lines for the header", 3, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestMergeByTariffWithDifferentTariffButWithSamePartNUms()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			line3 = header1.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header1);
			SetSpecificValueToInvoiceLines(line1, JobComInvoiceLine.Schema.JI_Tariff, "0000.00.00");
			SetSpecificValueToInvoiceLines(line2, JobComInvoiceLine.Schema.JI_Tariff, "1111.11.11");
			SetSpecificValueToInvoiceLines(line3, JobComInvoiceLine.Schema.JI_Tariff, "2222.22.22");

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One Customs Entry Header created", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("Three Lines for the header", 3, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestMergeByTariffWithSameTariffButWithDifferentPartNums()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			line3 = header1.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header1);//the same tariff set here
			SetSpecificValueToInvoiceLines(line1, JobComInvoiceLine.Schema.JI_PartNo, "Part1");
			SetSpecificValueToInvoiceLines(line2, JobComInvoiceLine.Schema.JI_PartNo, "Part2");
			SetSpecificValueToInvoiceLines(line3, JobComInvoiceLine.Schema.JI_PartNo, "Part3");

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One Customs Entry Header created", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("Three Lines for the header", 1, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		/// <summary>
		/// All invoice lines are to be merged into one cus entry line.
		/// </summary>
		public void TestJI_CLSetWhenEntryLineIdentified()
		{
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			line3 = header1.JobComInvoiceLines.AddNew();

			header1.JZ_OH_Supplier = GetValidSupplier();
			SetDefaultValuesToInvoiceLines(header1);

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();

			AssertEquals("Customs Entry Header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("Merged Line Count", 1, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);

			ZGuid entryLineGuid = testJobDeclaration.CustomsEntryHeaders[0].MergedLines[0].PK;
			AssertEquals("Invoice Line1", entryLineGuid, line1.JI_CL);
			AssertEquals("Invoice Line2", entryLineGuid, line2.JI_CL);
			AssertEquals("Invoice Line3", entryLineGuid, line3.JI_CL);
		}

		public void TestMergeWithDifferentNatureOfInvoiceLine()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testJobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			header1.JZ_InvoiceAmount = 200m;
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header1);
			line1.JI_IsPackToBondForLine = true; //Nature20
			line2.JI_IsPackToBondForLine = false; //Nature10

			testJobDeclaration.DoMerge();
			AssertEquals("Created two CusEntryHeader", 1, testJobDeclaration.CustomsEntryHeaders.Count);
		}

		#endregion

		public void TestAssignPrimeEnclosureHeadersIfNeeded1()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testJobDeclaration.JE_TotalNoOfPacks = 20;
			testJobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			testJobDeclaration.JE_ForcePrimeEnclosure = false;

			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_OH_Supplier = GetValidSupplier();
			header1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			header1.AddInfo.ZA_ValuationBasis_Hidden = "UT";
			header1.JZ_Nature10PackCount = 10;
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			line3 = header1.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header1);

			header2 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header2.JZ_OH_Supplier = GetValidSupplier();
			header2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			header2.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			header2.AddInfo.ZA_ValuationBasis_Hidden = "RT";
			header2.JZ_Nature10PackCount = 10;
			header2.JobComInvoiceLines.AddNew();
			header2.JobComInvoiceLines.AddNew();
			header2.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header2);

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("Two Customs Entry Headers created", 2, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("Normal Entry", true, testJobDeclaration.CustomsEntryHeaders[0].IsNormalEntry);
			AssertEquals("Normal Entry", true, testJobDeclaration.CustomsEntryHeaders[1].IsNormalEntry);
		}

		public void TestDifferentOriginsMerge()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_OH_Supplier = GetValidSupplier();

			header2 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header2.JZ_OH_Supplier = header1.JZ_OH_Supplier;

			header1.AddInfo.ZA_ORG = "NZ";
			header2.AddInfo.ZA_ORG = "US";

			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header2.JobComInvoiceLines.AddNew();

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One Customs Entry Headers created", 1, testJobDeclaration.CustomsEntryHeaders.Count);
		}

		public void TestDifferentPreferencesMerge()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_OH_Supplier = GetValidSupplier();

			header2 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header2.JZ_OH_Supplier = header1.JZ_OH_Supplier;

			header1.AddInfo.ZA_ORG = "US";
			header1.AddInfo.ZA_PRF = "S";
			header2.AddInfo.ZA_ORG = "US";

			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header2.JobComInvoiceLines.AddNew();

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One Customs Entry Headers created", 1, testJobDeclaration.CustomsEntryHeaders.Count);
		}

		/// <summary>
		/// One header with three invoice lines that have the same TILV amounts
		/// Expected Result : One entry header and one entry line with aggregated values
		/// </summary>
		public void TestMergeSameTILV()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testJobDeclaration.JE_TotalNoOfPacks = 20;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_OH_Supplier = GetValidSupplier();
			header1.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			header1.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 600);

			header1.AddInfo.ZA_ValuationBasis_Hidden = "UT";
			header1.JZ_Nature10PackCount = 10;
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			line3 = header1.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header1);

			line1.JI_LinePrice = 100m;
			line1.AddInfo.ZA_TILV = "200AUD";
			line2.JI_LinePrice = 200m;
			line2.AddInfo.ZA_TILV = "200AUD";
			line3.JI_LinePrice = 300m;
			line3.AddInfo.ZA_TILV = "200AUD";

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("One entry line", 1, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("CL_ParentTrailer", CusEntryLine.ParentTrailer.Normal, testJobDeclaration.CustomsEntryHeaders[0].MergedLines[0].CL_ParentTrailer);
			AssertEquals("Transport and insurance", 600m, testJobDeclaration.CustomsEntryHeaders[0].TAndI);
		}

		/// <summary>
		/// One header with three invoice lines that have the different TILV amounts. Eveything else is the same
		/// Expected Result : One entry Header and one entry line with the aggregated TILV
		/// </summary>
		public void TestMergeDifferentTILV()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testJobDeclaration.JE_TotalNoOfPacks = 20;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_OH_Supplier = GetValidSupplier();
			header1.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			header1.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 600);

			header1.AddInfo.ZA_ValuationBasis_Hidden = "UT";
			header1.JZ_Nature10PackCount = 10;
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			line3 = header1.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header1);

			line1.JI_LinePrice = 200m;
			line1.AddInfo.ZA_TILV = "100AUD";
			line2.JI_LinePrice = 200m;
			line2.AddInfo.ZA_TILV = "200AUD";
			line3.JI_LinePrice = 200m;
			line3.AddInfo.ZA_TILV = "300AUD";

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("One entry line", 1, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("Transport and insurance", 600m, testJobDeclaration.CustomsEntryHeaders[0].TAndI);
		}

		/// <summary>
		/// One invoice header with three invoice lines with differenct QT2s
		/// Expected Result : One entry header with one entry line with aggregated QT2
		/// </summary>
		public void TestMergeQT2()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testJobDeclaration.JE_TotalNoOfPacks = 20;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_OH_Supplier = GetValidSupplier();
			header1.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			header1.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 600);

			header1.AddInfo.ZA_ValuationBasis_Hidden = "UT";
			header1.JZ_Nature10PackCount = 10;
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			line3 = header1.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header1);

			line1.AddInfo.ZA_UQ2 = "SR";
			line1.AddInfo.ZA_QT2 = 2m;
			line2.AddInfo.ZA_UQ2 = "SR";
			line2.AddInfo.ZA_QT2 = 3m;
			line3.AddInfo.ZA_UQ2 = "SR";
			line3.AddInfo.ZA_QT2 = 4m;

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("One entry line", 1, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("QT2", 9m, testJobDeclaration.CustomsEntryHeaders[0].MergedLines[0].SecondQuantity);
		}

		/// <summary>
		/// One invoice header with three invoice lines with differenct ODF's
		/// Expected Result : One entry header with one entry line with aggregated ODF
		/// </summary>
		public void TestMergeODF()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testJobDeclaration.JE_TotalNoOfPacks = 20;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_OH_Supplier = GetValidSupplier();
			header1.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			header1.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 600);

			header1.AddInfo.ZA_ValuationBasis_Hidden = "UT";
			header1.JZ_Nature10PackCount = 10;
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			line3 = header1.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header1);

			line1.AddInfo.ZA_ODF = 2.5M;
			line2.AddInfo.ZA_ODF = 5.5M;
			line3.AddInfo.ZA_ODF = 1.4M;

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("One entry line", 1, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("ODF", 9.4m, testJobDeclaration.CustomsEntryHeaders[0].MergedLines[0].OtherDutyFactor.Amount);
		}

		/// <summary>
		/// Two invoice header with different origins and two invoice lines
		/// Expected Result : One entry header with two entry lines with different origins
		/// </summary>
		public void TestMergeOrigins()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testJobDeclaration.JE_TotalNoOfPacks = 20;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.AddInfo.ZA_ORG = "AU";

			line1 = header1.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header1);

			JobComInvoiceHeader header2 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header2.AddInfo.ZA_ORG = "IT";

			JobComInvoiceLine line2 = header2.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header2);

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("Two entry lines", 2, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		/// <summary>
		/// One invoice header with three invoice lines with the same Warehouse Unit Value
		/// Expected Result : One entry line with the sum of the rest
		/// </summary>
		public void TestMergeWUV()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testJobDeclaration.JE_TotalNoOfPacks = 20;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_OH_Supplier = GetValidSupplier();
			header1.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			header1.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 600);

			header1.AddInfo.ZA_ValuationBasis_Hidden = "UT";
			header1.JZ_Nature10PackCount = 10;
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			line3 = header1.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header1);

			line1.JI_LinePrice = 100;
			line2.JI_LinePrice = 100;
			line3.JI_LinePrice = 100;
			line1.AddInfo.ZA_WUV = 200m;
			line2.AddInfo.ZA_WUV = 200m;
			line3.AddInfo.ZA_WUV = 200m;

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("One entry line", 1, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
			CusEntryLine line = testJobDeclaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("Warehouse Unit Value", 200m, line.CL_WarehouseUnitValue);
			AssertEquals(300m, line.CustomsValue.Amount);
		}

		/// <summary>
		/// One invoice header with two invoice lines. Two have the same LCT amount.
		/// Expected result : One entry header and two entry lines
		/// </summary>
		public void TestLCTInAddInfoAndMerge()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testJobDeclaration.JE_TotalNoOfPacks = 20;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_OH_Supplier = GetValidSupplier();
			header1.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			header1.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 600);

			header1.AddInfo.ZA_ValuationBasis_Hidden = "UT";
			header1.JZ_Nature10PackCount = 10;
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header1);

			line1.AddInfo.ZA_LCT = 200m;
			line2.AddInfo.ZA_LCT = 200m;

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("One entry line", 2, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		/// <summary>
		/// One invoice header with two invoice lines. Both have the same percentage uplifts
		/// Expected Result : One entry header and one entry line
		/// </summary>
		public void TestADJWithPercentageSameUpliftMerge()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testJobDeclaration.JE_TotalNoOfPacks = 20;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_OH_Supplier = GetValidSupplier();
			header1.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			header1.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 600);

			header1.AddInfo.ZA_ValuationBasis_Hidden = "UT";
			header1.JZ_Nature10PackCount = 10;
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header1);

			line1.AddInfo.ZA_ADJ = "10%";
			line2.AddInfo.ZA_ADJ = "10%";

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("One entry line", 1, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		/// <summary>
		/// One invoice header with two invoice lines. Both have the different percentage uplifts
		/// Expected Result : One entry header and one entry lines
		/// This behaviour changed from two entry lines for Coles Myer : W00036236
		/// </summary>
		public void TestADJWithPercentageDifferentUpliftMerge()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testJobDeclaration.JE_TotalNoOfPacks = 20;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_OH_Supplier = GetValidSupplier();
			header1.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			header1.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 600);

			header1.AddInfo.ZA_ValuationBasis_Hidden = "UT";
			header1.JZ_Nature10PackCount = 10;
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header1);

			line1.AddInfo.ZA_ADJ = "10%";
			line2.AddInfo.ZA_ADJ = "15%";

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("One entry line", 1, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		/// <summary>
		/// One invoice header with two invoice lines that have the same price uplift
		/// Expected Result : One entry header with one entry lines
		/// </summary>
		public void TestADJWithPriceUpliftMerge()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testJobDeclaration.JE_TotalNoOfPacks = 20;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_OH_Supplier = GetValidSupplier();
			header1.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			header1.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 600);

			header1.AddInfo.ZA_ValuationBasis_Hidden = "UT";
			header1.JZ_Nature10PackCount = 10;
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header1);

			line1.AddInfo.ZA_ADJ = "10USD";
			line2.AddInfo.ZA_ADJ = "10USD";

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("Entry Lins", 1, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals(20m, testJobDeclaration.CustomsEntryHeaders[0].MergedLines[0].PriceAdjustment.Amount);
		}

		/// <summary>
		/// Merge By Lookup : Part Number has default CPDec answers
		/// One invoice header with two invoice lines that have the same lookup with different parts that have the same default answers
		/// Expected Result : One entry header with one entry line
		/// </summary>
		public void TestDifferentPartsWithTheSameDefaultCPDecAnswers()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			testJobDeclaration.JE_TotalNoOfPacks = 20;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_OH_Supplier = GetValidSupplier();
			header1.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			header1.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 600);

			header1.AddInfo.ZA_ValuationBasis_Hidden = "UT";
			header1.JZ_Nature10PackCount = 10;
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header1);

			AUOrgSupplierPart part1 = factory.New<AUOrgSupplierPart>();
			part1.OP_PartNum = "Part1";
			OrgPartRelation relation1 = part1.RelatedOrganisations.AddNew();
			relation1.OU_OH = header1.JZ_OH_Supplier;
			part1.AddNewImportPivotWithClassification(importClass.PK).AddInfo.ZA_CPDecDefaultAnswerTrue_Hidden = "1|2|3";

			AUOrgSupplierPart part2 = factory.New<AUOrgSupplierPart>();
			part2.OP_PartNum = "Part2";
			OrgPartRelation relation2 = part2.RelatedOrganisations.AddNew();
			relation2.OU_OH = header1.JZ_OH_Supplier;
			part2.AddNewImportPivotWithClassification(importClass.PK).AddInfo.ZA_CPDecDefaultAnswerTrue_Hidden = "1|2|3";

			//these two have the same lookups
			factory.Save();

			line1.JI_PartNo = part1.OP_PartNum;
			line2.JI_PartNo = part2.OP_PartNum;

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("One entry line", 1, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		/// <summary>
		/// Merge By Tariff : Part Number has default different CPDec answers
		/// One invoice header with two invoice lines that have the same tariff with different parts that have the different default answers
		/// Expected Result : One entry header with two entry line
		/// </summary>
		public void TestPartsWithDifferentDefaultCPDecAnswers()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			testJobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			testJobDeclaration.JE_TotalNoOfPacks = 20;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_OH_Supplier = GetValidSupplier();
			header1.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			header1.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 600);

			header1.AddInfo.ZA_ValuationBasis_Hidden = "UT";
			header1.JZ_Nature10PackCount = 10;
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header1);

			AUOrgSupplierPart part1 = factory.New<AUOrgSupplierPart>();
			part1.OP_PartNum = "Part1";
			OrgPartRelation relation1 = part1.RelatedOrganisations.AddNew();
			relation1.OU_OH = header1.JZ_OH_Supplier;
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			part1.AddNewImportPivotWithClassification(importClass.PK).AddInfo.ZA_CPDecDefaultAnswerTrue_Hidden = "1|2|3";

			AUOrgSupplierPart part2 = factory.New<AUOrgSupplierPart>();
			part2.OP_PartNum = "Part2";
			OrgPartRelation relation2 = part2.RelatedOrganisations.AddNew();
			relation2.OU_OH = header1.JZ_OH_Supplier;
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			part2.AddNewImportPivotWithClassification(importClass.PK).AddInfo.ZA_CPDecDefaultAnswerTrue_Hidden = "1|2|3|4";

			//these two have the same tariff
			factory.Save();

			line1.JI_PartNo = part1.OP_PartNum;
			line2.JI_PartNo = part2.OP_PartNum;

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("Two entry line", 2, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestCountervailingSecurityAmountMerge()
		{
			CreateEntryForMerge(testJobDeclaration);
			JobComInvoiceLine line1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0];
			JobComInvoiceLine line2 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[1];

			line1.AddInfo.ZA_CSA = 100m;
			line2.AddInfo.ZA_CSA = 200m;

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("One entry line", 1, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestCountervailingDutyMerge()
		{
			CreateEntryForMerge(testJobDeclaration);
			JobComInvoiceLine line1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0];
			JobComInvoiceLine line2 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[1];

			line1.AddInfo.ZA_CVD = 100m;
			line2.AddInfo.ZA_CVD = 200m;

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("One entry line", 1, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestDumpingDutyMerge()
		{
			CreateEntryForMerge(testJobDeclaration);
			JobComInvoiceLine line1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0];
			JobComInvoiceLine line2 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[1];

			line1.AddInfo.ZA_DMP = 100m;
			line2.AddInfo.ZA_DMP = 200m;

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("One entry line", 1, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestDumpingSecurityAmountMerge()
		{
			CreateEntryForMerge(testJobDeclaration);
			JobComInvoiceLine line1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0];
			JobComInvoiceLine line2 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[1];

			line1.AddInfo.ZA_DSA = 100m;
			line2.AddInfo.ZA_DSA = 200m;

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("One entry line", 1, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestDutyMerge()
		{
			CreateEntryForMerge(testJobDeclaration);
			JobComInvoiceLine line1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0];
			JobComInvoiceLine line2 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[1];

			line1.AddInfo.ZA_DTY = 100m;
			line2.AddInfo.ZA_DTY = 200m;

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("One entry line", 1, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestInterimCountervailingDutyMerge()
		{
			CreateEntryForMerge(testJobDeclaration);
			JobComInvoiceLine line1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0];
			JobComInvoiceLine line2 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[1];

			line1.AddInfo.ZA_ICV = 100m;
			line2.AddInfo.ZA_ICV = 200m;

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("One entry line", 1, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestInterimDumpingDutyMerge()
		{
			CreateEntryForMerge(testJobDeclaration);
			JobComInvoiceLine line1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0];
			JobComInvoiceLine line2 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[1];

			line1.AddInfo.ZA_IDP = 100m;
			line2.AddInfo.ZA_IDP = 200m;

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("One entry line", 1, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		/// <summary>
		/// One invoice header with two invoice lines that have the same CONs.
		/// Expected Result : One entry header with two entry lines
		/// </summary>
		public void TestSecurityConcessionOverrideMerge()
		{
			CreateEntryForMerge(testJobDeclaration);
			JobComInvoiceLine line1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0];
			JobComInvoiceLine line2 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[1];

			line1.AddInfo.ZA_CON = 200m;
			line2.AddInfo.ZA_CON = 200m;

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("TWO entry line", 2, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		/// <summary>
		/// One invoice header with two invoice lines that have the same WRUs
		/// Expected Result : One entry header with one entry line
		/// </summary>
		public void TestWarehouseReferenceQuantityAndUnitMerge()
		{
			CreateEntryForMerge(testJobDeclaration);
			JobComInvoiceLine line1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0];
			JobComInvoiceLine line2 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[1];

			line1.AddInfo.ZA_WRQ = 10m;
			line2.AddInfo.ZA_WRQ = 20m;

			line1.AddInfo.ZA_WRU = "KG";
			line2.AddInfo.ZA_WRU = "KG";

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("One entry line", 1, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("Summed WRQ", 30m, testJobDeclaration.CustomsEntryHeaders[0].MergedLines[0].WRQ);
			AssertEquals("Merged WRU", "KG", testJobDeclaration.CustomsEntryHeaders[0].MergedLines[0].WRU);
		}

		public void TestTwoWarehouseReferenceUnitMerge()
		{
			CreateEntryForMerge(testJobDeclaration);
			JobComInvoiceLine line1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0];
			JobComInvoiceLine line2 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[1];

			line1.AddInfo.ZA_WRQ = 10m;
			line2.AddInfo.ZA_WRQ = 20m;

			line1.AddInfo.ZA_WRU = "KG";
			line2.AddInfo.ZA_WRU = "ML";

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("two entry lines", 2, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("WRQ", 10m, line1.CusEntryLine.WRQ);
			AssertEquals("WRU", "KG", line1.CusEntryLine.WRU);
			AssertEquals("WRQ", 20m, line2.CusEntryLine.WRQ);
			AssertEquals("WRU", "ML", line2.CusEntryLine.WRU);
		}

		/// <summary>
		/// One invoice header with two invoice lines that have the same WUVs
		/// Expected Result : One entry header with one entry line
		/// </summary>
		public void TestWarehouseUnitValueMerge()
		{
			CreateEntryForMerge(testJobDeclaration);
			JobComInvoiceLine line1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0];
			JobComInvoiceLine line2 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[1];

			line1.AddInfo.ZA_WUV = 100m;
			line2.AddInfo.ZA_WUV = 100m;

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("One entry line", 1, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		/// <summary>
		/// One invoice header with two invoice lines that have the different ISSs
		/// Expected Result : One entry header with two entry lines
		/// </summary>
		public void TestInvoiceSpritStrength()
		{
			CreateEntryForMerge(testJobDeclaration);
			JobComInvoiceLine line1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0];
			JobComInvoiceLine line2 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[1];

			line1.AddInfo.ZA_ISS = 0.5m;
			line2.AddInfo.ZA_ISS = 0.25m;

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, testJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("TWO entry line", 2, testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestWillThereBeMultipleEntryHeaders()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testJobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			testJobDeclaration.JE_TotalNoOfPacks = 20;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_OH_Supplier = GetValidSupplier();
			header1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			header1.AddInfo.ZA_ValuationBasis_Hidden = "UT";
			header1.JZ_Nature10PackCount = 10;
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			line3 = header1.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header1);

			LineMerger merger = new LineMerger(testJobDeclaration);

			Assert("Not Multiple Entry Headers", !merger.WillThereBeMultipleEntryHeaders);

			header2 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header2.JZ_OH_Supplier = GetValidSupplier();
			header2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			header2.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			header2.AddInfo.ZA_ValuationBasis_Hidden = "RT";
			header2.JZ_Nature10PackCount = 10;
			header2.JobComInvoiceLines.AddNew();
			header2.JobComInvoiceLines.AddNew();
			header2.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header2);

			Assert("Multiple Entry Headers", merger.WillThereBeMultipleEntryHeaders);
		}

		public void TestMergeWithNoMergeWithTwoInvoicesThatShouldBeMerged()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			testJobDeclaration.JE_TotalNoOfPacks = 20;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_OH_Supplier = GetValidSupplier();
			header1.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;

			header1.AddInfo.ZA_ValuationBasis_Hidden = "UT";
			header1.JZ_Nature10PackCount = 10;

			header2 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header2.JZ_OH_Supplier = header1.JZ_OH_Supplier;
			header2.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			header2.JZ_RX_NKInvoice_Currency = header1.JZ_RX_NKInvoice_Currency;

			header2.AddInfo.ZA_ValuationBasis_Hidden = "UT";
			header2.JZ_Nature10PackCount = 10;

			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header2.JobComInvoiceLines.AddNew();

			SetDefaultValuesToInvoiceLines(header1);
			SetDefaultValuesToInvoiceLines(header2);

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			Assert("HeaderCount==1", testJobDeclaration.CustomsEntryHeaders.Count == 1);
			Assert("LineCount==2", testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count == 2);
		}

		public void TestMergeLinesWithDifferentHeaderPackCounts()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testJobDeclaration.JE_TotalNoOfPacks = 20;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_OH_Supplier = GetValidSupplier();
			header1.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;

			header1.AddInfo.ZA_ValuationBasis_Hidden = "UT";
			header1.JZ_Nature10PackCount = 10;

			header2 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header2.JZ_OH_Supplier = header1.JZ_OH_Supplier;
			header2.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			header2.JZ_RX_NKInvoice_Currency = header1.JZ_RX_NKInvoice_Currency;

			header2.AddInfo.ZA_ValuationBasis_Hidden = "UT";
			header2.JZ_Nature10PackCount = 15;

			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header2.JobComInvoiceLines.AddNew();

			SetDefaultValuesToInvoiceLines(header1);
			SetDefaultValuesToInvoiceLines(header2);

			LineMerger merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();
			Assert("HeaderCount==1", testJobDeclaration.CustomsEntryHeaders.Count == 1);
			Assert("LineCount==1", testJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count == 1);
		}

		public void TestRecyclingProblemWithPreferenceFields()
		{
			JobDeclaration declaration = JobDeclaration.New(factory);
			declaration.JE_MessageType = "IMP";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_TotalNoOfPacks = 20;

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_OH_Supplier = GetValidSupplier();
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoiceHeader.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			invoiceHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 600);

			JobComInvoiceLine line1 = invoiceHeader.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "1902.19.00 38";
			line1.AddInfo.ZA_PST = "GEN";

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);

			AssertEquals("One entry header", 1, declaration.CustomsEntryHeaders.Count);

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Clear.Code;

			AssertEquals("One entry line", 1, entryHeader.MergedLines.Count);
			CusEntryLine entryLine1 = entryHeader.MergedLines[0];
			AssertEquals("Entry line number is 1", (ZShort)1, entryLine1.CL_LineNumber);

			factory.Save();

			line1.AddInfo.ZA_PST = "DCS";
			line1.AddInfo.ZA_POC = "CN";
			line1.AddInfo.ZA_PRT = "P50";
			merger.DoMerge();
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);

			AssertEquals("One entry header", 1, declaration.CustomsEntryHeaders.Count);

			CusEntryHeader entryHeader2 = declaration.CustomsEntryHeaders[0];
			AssertEquals("Same entry header", true, entryHeader.PK == entryHeader2.PK);
			AssertEquals("One entry line", 1, entryHeader2.MergedLines.Count);

			CusEntryLine entryLine2 = entryHeader2.MergedLines[0];
			AssertEquals("Same entry line", true, entryLine1.PK == entryLine2.PK);
			AssertEquals("Entry line number is 1", (ZShort)1, entryLine2.CL_LineNumber);
		}

		public void TestAssignLineNumberWhenLinesAreDeletedAndInserted()
		{
			JobDeclaration declaration = JobDeclaration.New(factory);
			declaration.JE_MessageType = "IMP";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_TotalNoOfPacks = 20;

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_OH_Supplier = GetValidSupplier();
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoiceHeader.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			invoiceHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 600);

			JobComInvoiceLine line1 = invoiceHeader.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "1.1.1";
			JobComInvoiceLine line2 = invoiceHeader.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "2.2.2";
			JobComInvoiceLine line3 = invoiceHeader.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "3.3.3";

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);//should be called if merge is done through MergeManager.DoMerge();

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Clear.Code;
			entryHeader.EntryNumber = "Test";

			AssertEquals("Three entry lines", 3, entryHeader.MergedLines.Count);
			AssertEquals("Entry line number is 1", (ZShort)1, entryHeader.MergedLines[0].CL_LineNumber);
			AssertEquals("Entry line number is 2", (ZShort)2, entryHeader.MergedLines[1].CL_LineNumber);
			AssertEquals("Entry line number is 3", (ZShort)3, entryHeader.MergedLines[2].CL_LineNumber);
			factory.Save();

			invoiceHeader.JobComInvoiceLines.RemoveAndDelete(line2);
			merger.DoMerge();
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);//should be called if merge is done through MergeManager.DoMerge();

			AssertEquals("Two entry lines", 2, entryHeader.MergedLines.Count);
			AssertEquals("Entry line number is 1", (ZShort)1, entryHeader.MergedLines[0].CL_LineNumber);
			AssertEquals("Entry line number is 3", (ZShort)3, entryHeader.MergedLines[1].CL_LineNumber);
			factory.Save();

			JobComInvoiceLine line4 = invoiceHeader.JobComInvoiceLines.AddNew();
			line4.JI_Tariff = "4.4.4";
			merger.DoMerge();
			AssertEquals("Two entry lines", 3, entryHeader.MergedLines.Count);
			AssertEquals("Entry line number is 1", (ZShort)1, entryHeader.MergedLines[0].CL_LineNumber);
			AssertEquals("Entry line number is 3", (ZShort)3, entryHeader.MergedLines[1].CL_LineNumber);
			AssertEquals("Entry line number is 4", (ZShort)4, entryHeader.MergedLines[2].CL_LineNumber);
		}

		public void TestMergeForSubmitWeeklyNilReturnN30()
		{
			JobDeclaration testDec = JobDeclaration.New(factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			//SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			testDec.JE_SettlementPeriodType = "SW";
			testDec.NilReturnInd = true;
			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(testDec.MergeManager);
			AssertEquals(1, testDec.CustomsEntryHeaders.Count);
		}

		public void TestRefundReasonDefaultFordeletedLine()
		{
			JobDeclaration declaration = factory.New<JobDeclaration>();
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals("there should be one entry header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("There should be 2 entry lines", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			CusEntryLine entryLine1 = line1.CusEntryLine;
			CusEntryLine entryLine2 = line2.CusEntryLine;
			entryHeader.AddInfo.ZA_IsPAYRECAck_Hidden = true;

			entryHeader.CH_HighestLineNumber = 2;
			line1.Delete();
			merger.DoMerge();

			AssertEquals("EntryLine1 should not be deleted", false, entryLine1.IsDeleted);
			AssertEquals("EntryLine1 status should be changed", EntryLineStatusList.Codes.DeletePending, entryLine1.CL_CustomsPostedStatus);
			AssertEquals("EntryLine1 should have Refund Reason 126A", "126A", entryLine1.RefundReasonCode);
		}

		#region Implementation

		protected void SetupPrimeEnclosureEntry()
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testJobDeclaration.JE_TotalNoOfPacks = 20;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_OH_Supplier = GetValidSupplier();
			header1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			header1.AddInfo.ZA_ValuationBasis_Hidden = "UT";
			header1.JZ_Nature10PackCount = 0;
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();
			line3 = header1.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header1);

			header2 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header2.JZ_OH_Supplier = GetValidSupplier();
			header2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			header2.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			header2.AddInfo.ZA_ValuationBasis_Hidden = "RT";
			header2.JZ_Nature10PackCount = 0;
			header2.JobComInvoiceLines.AddNew();
			header2.JobComInvoiceLines.AddNew();
			header2.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header2);
		}

		BusinessObjectFactory factory;
		JobDeclaration testJobDeclaration;
		JobComInvoiceHeader header1;
		JobComInvoiceLine line1;
		JobComInvoiceLine line2;
		JobComInvoiceLine line3;
		JobComInvoiceHeader header2;
		Classification importClass;
		string treatmentCode1;
		string treatmentCode2;
		string addInfo1;
		string addInfo2;
		string addInfo;
		string part;
		string lookupCode;
		RefCurrency aUDCurrency;
		RefCurrency uSDCurrency;

		protected override void SetUp()
		{
			base.SetUp();
			treatmentCode1 = "TT1";
			treatmentCode2 = "TT2";
			addInfo = "PMT=123456";
			addInfo1 = "ORG=AU*PMT=1111";
			addInfo2 = "PMT=1111*ORG=AU";
			lookupCode = "LookupCode";
			part = "Part";
			factory = new BusinessObjectFactory();
			testJobDeclaration = factory.New<JobDeclaration>();
			testJobDeclaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testJobDeclaration.JE_ExportDate = new ZDateTime(2003, 11, 15);
			testJobDeclaration.JE_DateOfFirstArrival = new ZDateTime(2003, 11, 16);
			importClass = factory.New<Classification>();
			importClass.CC_ClassificationType = Classification.ClassificationType.IMP;
			importClass.CC_Description = "Description";
			importClass.CC_LookupCode = lookupCode;
			aUDCurrency = RefCurrency.LoadFromCurrencyCode(factory, "AUD");
			uSDCurrency = RefCurrency.LoadFromCurrencyCode(factory, "USD");
			TaxOrFeeTestHelper.SetUp();
		}

		protected void CreateEntryForMerge(JobDeclaration testJobDeclaration)
		{
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testJobDeclaration.JE_TotalNoOfPacks = 20;
			header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_OH_Supplier = GetValidSupplier();
			header1.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			header1.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 600);

			header1.AddInfo.ZA_ValuationBasis_Hidden = "UT";
			header1.JZ_Nature10PackCount = 10;
			line1 = header1.JobComInvoiceLines.AddNew();
			line2 = header1.JobComInvoiceLines.AddNew();

			SetDefaultValuesToInvoiceLines(header1);
		}

		protected void SetDefaultValuesToInvoiceLines(JobComInvoiceHeader header)
		{
			foreach (JobComInvoiceLine line in header.JobComInvoiceLines)
			{
				importClass.CC_TariffNum = "2203.00.31 15";
				line.JI_CC = importClass.PK;
				line.AddInfo.ZA_TreatmentCode_Hidden = treatmentCode1;
				line.JI_AddInfo = addInfo;
				line.JI_PartNo = part;
				line.JI_CustomsQuantity = 1.0m;
				line.JI_LinePrice = 100.0m;
			}
		}

		protected void SetSpecificValueToInvoiceLines(JobComInvoiceLine line, string lineColumn, object lineValue)
		{
			line.JI_Tariff = "2203.00.31";
			line.AddInfo.ZA_TreatmentCode_Hidden = treatmentCode1;
			line.JI_AddInfo = addInfo;
			line[lineColumn] = lineValue;
		}

		protected void SetClassificationToInvoiceLine(JobComInvoiceLine line)
		{
			Classification @class = factory.New<Classification>();
			@class.CC_ClassificationType = Classification.ClassificationType.EXP;
			@class.CC_LookupCode = line.PK.ToString().Substring(0, 10);
			@class.CC_Description = "Description";
			line.JI_CC = @class.PK;
		}

		ZGuid GetValidSupplier()
		{
			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_IsConsignor, true);
			var supplier = factory.LoadTop1<OrgHeader>(filter);
			return supplier.PK;
		}

		ZGuid GetValidSupplier2(ZGuid existingGUID)
		{
			ZQuery filter1 = new ZQuery(OrgHeaderSchema.OH_IsConsignor, true);
			ZQuery filter2 = new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, existingGUID);
			var supplier = factory.LoadTop1<OrgHeader>(new ZQuery(filter1, filter2));
			return supplier.PK;
		}

		OrgAddress GetValidWarehouseAddress()
		{
			OrgHeader header = factory.New<OrgHeader>();
			header.OH_IsWarehouseClient = true;
			return header.Addresses.AddNew();
		}
		#endregion
	}
}
