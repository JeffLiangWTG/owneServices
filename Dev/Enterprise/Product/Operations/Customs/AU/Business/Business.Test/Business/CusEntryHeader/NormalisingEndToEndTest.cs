using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class NormalisingEndToEndTest : TestCaseWithFactory
	{
		public void TestITOTIncotermWhenNature30()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			AssertEquals("Is N30", true, entryHeader.IsCMRNature30);

			EntryHeaderNormalisedChargesProvider invoiceNormaliser = new EntryHeaderNormalisedChargesProvider(entryHeader, JobDeclaration.GetLocalCurrency());
			AssertEquals("Incoterm for N30 should be empty", "", invoiceNormaliser.ITOTIncoTerm);
		}

		public void TestInvoiceTotal()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			JobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			InvoiceCharge oFT = invoice2.Charges.AddNew();
			oFT.J7_ChargeType = AUChargeCodeList.Codes.OverseasFreight;
			oFT.J7_Amount = 1000m;
			oFT.J7_RX_NKCurrency = JobDeclaration.LocalCurrencyConstantCode;
			oFT.J7_IsIncludedInITOT = true;

			InvoiceCharge oNS = invoice2.Charges.AddNew();
			oNS.J7_ChargeType = AUChargeCodeList.Codes.OverseasInsurance;
			oNS.J7_Amount = 500m;
			oNS.J7_RX_NKCurrency = JobDeclaration.LocalCurrencyConstantCode;
			oNS.J7_IsIncludedInITOT = true;

			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoice2.JZ_InvoiceAmount = 20000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			GroupInvoiceCharge groupOFT = testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			groupOFT.J7_ChargeType = AUChargeCodeList.Codes.OverseasFreight;
			groupOFT.J7_Amount = 1500m;
			groupOFT.J7_RX_NKCurrency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 10000m;

			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 20000m;

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();

			AssertEquals("PreCondition : One CusEntryHeader is created", 1, testDec.CustomsEntryHeaders.Count);
			Assert("PreCondition:ITOT incoterm is different", invoice1.ITOTIncoTerm != invoice2.ITOTIncoTerm);
			AssertEquals("PreCondition: OFT for invoice1 is apportioned 1500 - 1000m", 500m, invoice1.GroupCharges.GetCharge(oFT.ChargeKey).Amount);

			EntryHeaderNormalisedChargesProvider invoiceNormaliser = new EntryHeaderNormalisedChargesProvider(testDec.CustomsEntryHeaders[0], JobDeclaration.GetLocalCurrency());

			AssertEquals("InvoiceTotal for invoice1", 10000m, invoice1.InvoiceLineTotal);
			AssertEquals("InvoiceTotal for Invoice2", 20000m, invoice2.InvoiceLineTotal);
			AssertEquals("Normalised InvoiceTotal: 10000 + (20000 - 1500)", 28500m, invoiceNormaliser.InvoiceTotal.Amount);
		}

		public void TestInvoiceTotalNormalisedForDifferentCurrencies()
		{
			ZTestHelper helper = new ZTestHelper(Factory);

			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_ExportDate = new ZDateTime(2005, 3, 15);
			helper.SetExchangeRate(testDec.JE_ExportDate, testDec.JE_ExportDate.AddDays(1), 0.7500m, helper.USDCurrency);

			JobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoice2.JZ_InvoiceAmount = 20000m;
			invoice2.JZ_RX_NKInvoice_Currency = helper.USDCurrency.RX_Code;

			InvoiceCharge oFT = invoice2.Charges.AddNew();
			oFT.J7_ChargeType = AUChargeCodeList.Codes.OverseasFreight;
			oFT.J7_Amount = 1000m;
			oFT.J7_IsIncludedInITOT = true;

			InvoiceCharge oNS = invoice2.Charges.AddNew();
			oNS.J7_ChargeType = AUChargeCodeList.Codes.OverseasInsurance;
			oNS.J7_Amount = 500m;
			oNS.J7_IsIncludedInITOT = true;

			GroupInvoiceCharge groupOFT = testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			groupOFT.J7_ChargeType = AUChargeCodeList.Codes.OverseasFreight;
			groupOFT.J7_Amount = 1500m;
			groupOFT.J7_RX_NKCurrency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 10000m;

			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 20000m;

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];
			AssertEquals("InvoiceTotal for invoice1", 10000m, invoice1.InvoiceLineTotal);
			AssertEquals("InvoiceTotal for Invoice2 in Invoice Currency", 20000m, invoice2.InvoiceLineTotal);
			AssertEquals("Normalised invoice total currency", "AUD", entryHeader.InvoiceTotal.Currency.Code);
			AssertEquals("Normalised invoice total Amount", 34666.67m, entryHeader.InvoiceTotal.Amount);
		}

		public void TestCFRInvoicesWithInvoiceOFTRefS00040362()
		{
			ZTestHelper helper = new ZTestHelper(Factory);

			JobDeclaration testDec = JobDeclaration.New(Factory);//Rohlig DB RefNo:S00040362
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_ExportDate = new ZDateTime(2005, 3, 29);
			testDec.JE_MergeBy = "TRF";

			helper.SetExchangeRate(testDec.JE_ExportDate, testDec.JE_ExportDate.AddDays(1), 0.5826m, helper.EURCurrency);
			GroupInvoiceCharge oNS = testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			oNS.J7_ChargeType = AUChargeCodeList.Codes.OverseasInsurance;
			oNS.J7_Amount = 80.74m;
			oNS.J7_RX_NKCurrency = helper.EURCurrency.RX_Code;

			JobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 1249.80m;
			invoice1.JZ_RX_NKInvoice_Currency = helper.EURCurrency.RX_Code;
			InvoiceCharge oFT1 = invoice1.Charges.AddNew();
			oFT1.J7_ChargeType = AUChargeCodeList.Codes.OverseasFreight;
			oFT1.J7_Amount = 85m;
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;

			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 1164.80m;
			line1.JI_Tariff = "9506.99.90 32";
			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 31599.16m;
			invoice2.JZ_RX_NKInvoice_Currency = helper.EURCurrency.RX_Code;
			InvoiceCharge oFT2 = invoice2.Charges.AddNew();
			oFT2.J7_ChargeType = AUChargeCodeList.Codes.OverseasFreight;
			oFT2.J7_Amount = 469m;
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;

			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 31130.16m;
			line2.JI_Tariff = "7326.90.90 58";
			testDec.ResumeApportionment();
			AssertEquals("T&I for invoice1 in invoice currency", 87.91m, invoice1.JZ_Calc_TNI);
			AssertEquals("T&I for invoice2 in invoice currency", 546.83m, invoice2.JZ_Calc_TNI);

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();

			AssertEquals("Only One CusEntryHeader is created", 1, testDec.CustomsEntryHeaders.Count);
			AssertEquals("Two entry lines", 2, testDec.CustomsEntryHeaders[0].MergedLines.Count);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];
			var entryLine1 = Factory.Load<CusEntryLine>(line1.JI_CL);
			var entryLine2 = Factory.Load<CusEntryLine>(line2.JI_CL);

			AssertEquals("ITOT incoterm", Core.Constants.IncoTerms.FreeOnBoard, entryHeader.ITOTIncoTerm);
			AssertEquals("Invoice total for EntryHeader", 32294.96m, entryHeader.InvoiceTotal.Amount);
			AssertEquals("Invoice total for EntryHeader", helper.EURCurrency, entryHeader.InvoiceTotal.Currency);
			AssertEquals("Invoice total for EntryLine1", 1164.80m, entryLine1.Price.Amount);
			AssertEquals("Invoice Total for EntryLine2", 31130.16m, entryLine2.Price.Amount);
			AssertEquals("TranportInsurance for EntryLine1", 150.89m, entryLine1.TransportAndInsuranceInLocalCurrency.Amount);
			AssertEquals("TranportInsurance for EntryLine2", 938.60m, entryLine2.TransportAndInsuranceInLocalCurrency.Amount);
			AssertEquals("FOB For entry header", 32294.96m, entryHeader.FOB.Amount);
			AssertEquals("CIF for entry header", 32929.70m, entryHeader.CIF.Amount);
		}

		public void TestFourEXWInvoicesWithADDInvoiceChargesRefS00040365()
		{
			ZTestHelper helper = new ZTestHelper(Factory);

			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_ExportDate = new ZDateTime(2005, 3, 2);
			helper.SetExchangeRate(testDec.JE_ExportDate, testDec.JE_ExportDate.AddDays(1), 0.783700m, helper.USDCurrency);

			testDec.JE_MasterBill = "08177777770";
			testDec.JE_MergeBy = "NON";
			testDec.JE_MessageSubType = "FRM";
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_OwnerRef = "40719-F,T334919,CASP";
			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			testDec.JE_RL_NKFinalDestination = "AUSYD";
			testDec.JE_RL_NKOrigin = "USKCK";
			testDec.JE_RL_NKPortOfArrival = "AUSYD";
			testDec.JE_RL_NKPortOfFirstArrival = "AUSYD";
			testDec.JE_RL_NKPortOfLoading = "USKCK";
			testDec.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			testDec.JE_SystemCreateTimeUtc = new ZDateTime(2005, 3, 31);
			testDec.JE_SystemCreateUser = "C";
			testDec.JE_SystemLastEditTimeUtc = new ZDateTime(2005, 4, 1);
			testDec.JE_SystemLastEditUser = "C";
			testDec.JE_TotalNoOfPacks = 32;
			testDec.JE_TotalNoOfPacksPackType = "PLT";
			testDec.JE_TotalVolume = 4.740m;
			testDec.JE_TotalVolumeUnit = "M3";
			testDec.JE_TotalWeight = 591.000m;
			testDec.JE_TotalWeightUnit = "KG";
			testDec.JE_TransportMode = Core.Constants.TransportModes.Air;
			testDec.JE_VoyageFlightNo = "QF108";

			JobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 333m, "USD");
			groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 2550.63m, "USD");
			groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 74.96m, "USD");

			JobComInvoiceHeader testHeader0309682 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testHeader0309682.JZ_AddInfo = "ORG=US*PackCountForNature10_Hidden=30*ValuationBasis_Hidden=UT";
			testHeader0309682.JZ_IncoTerm = Core.Constants.IncoTerms.ExWorks;
			testHeader0309682.JZ_InvoiceAmount = 691.2000m;
			testHeader0309682.JZ_InvoiceCurrExRate = 0.783700000m;
			testHeader0309682.JZ_InvoiceDate = new ZDateTime(2004, 8, 10);
			testHeader0309682.JZ_InvoiceNumber = "0309682";
			testHeader0309682.JZ_RX_NKInvoice_Currency = "USD";
			testHeader0309682.JZ_Weight = 16.000m;
			testHeader0309682.JZ_WeightUQ = "KG";

			JobComInvoiceHeader testHeaderIT45148 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testHeaderIT45148.JZ_AddInfo = "ORG=US*PackCountForNature10_Hidden=10*ValuationBasis_Hidden=UT";
			testHeaderIT45148.JZ_IncoTerm = Core.Constants.IncoTerms.ExWorks;
			testHeaderIT45148.JZ_InvoiceAmount = 2617.9000m;
			testHeaderIT45148.JZ_InvoiceCurrExRate = 0.783700000m;
			testHeaderIT45148.JZ_InvoiceDate = new ZDateTime(2004, 8, 10);
			testHeaderIT45148.JZ_InvoiceNumber = "IT45148";
			testHeaderIT45148.JZ_RX_NKInvoice_Currency = "USD";
			testHeaderIT45148.JZ_Weight = 41.000m;
			testHeaderIT45148.JZ_WeightUQ = "KG";

			testHeaderIT45148.Charges.AddNew(CustomsChargeTypeList.Codes.AdditionCharge, 28.75m, "USD");

			JobComInvoiceHeader testHeader299299 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testHeader299299.JZ_AddInfo = "ORG=US*PackCountForNature10_Hidden=32*ValuationBasis_Hidden=UT";
			testHeader299299.JZ_IncoTerm = Core.Constants.IncoTerms.ExWorks;
			testHeader299299.JZ_InvoiceAmount = 25665.3100m;
			testHeader299299.JZ_InvoiceCurrExRate = 0.783700000m;
			testHeader299299.JZ_InvoiceDate = new ZDateTime(2004, 8, 10);
			testHeader299299.JZ_InvoiceNumber = "299299";
			testHeader299299.JZ_RX_NKInvoice_Currency = "USD";
			testHeader299299.JZ_Weight = 500.000m;
			testHeader299299.JZ_WeightUQ = "KG";

			JobComInvoiceHeader testHeader5927 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testHeader5927.JZ_AddInfo = "ORG=US*PackCountForNature10_Hidden=10*ValuationBasis_Hidden=UT";
			testHeader5927.JZ_IncoTerm = Core.Constants.IncoTerms.ExWorks;
			testHeader5927.JZ_InvoiceAmount = 676.0000m;
			testHeader5927.JZ_InvoiceCurrExRate = 0.783700000m;
			testHeader5927.JZ_InvoiceDate = new ZDateTime(2004, 8, 10);
			testHeader5927.JZ_InvoiceNumber = "5927";
			testHeader5927.JZ_RX_NKInvoice_Currency = "USD";
			testHeader5927.JZ_Weight = 34.000m;
			testHeader5927.JZ_WeightUQ = "KG";

			testHeader5927.Charges.AddNew(CustomsChargeTypeList.Codes.AdditionCharge, 34.00m, "USD");

			JobComInvoiceLine testLine1 = testHeader0309682.JobComInvoiceLines.AddNew();
			testLine1.JI_AddInfo = "PRF=U*ORG=US*ValuationBasis_Hidden=TV";
			testLine1.JI_Description = "FOOTWEAR PARTS";
			testLine1.JI_LineNo = (short)1;
			testLine1.JI_LinePrice = 691.2000m;
			testLine1.JI_CountryOfOrigin = "US";
			testLine1.JI_Tariff = "6406.99.99 66";
			testLine1.JI_WeightUQ = "KG";

			JobComInvoiceLine testLine2 = testHeader299299.JobComInvoiceLines.AddNew();
			testLine2.JI_AddInfo = "PRF=U*ORG=US*ValuationBasis_Hidden=TV";
			testLine2.JI_CustomsQuantity = 396.0000m;
			testLine2.JI_CustomsUnitQty = "PR";
			testLine2.JI_Description = "LEATHER FOOTWEAR";
			testLine2.JI_LineNo = (short)1;
			testLine2.JI_LinePrice = 25665.3100m;
			testLine2.JI_CountryOfOrigin = "US";
			testLine2.JI_Tariff = "6403.99.00 03";
			testLine2.JI_WeightUQ = "KG";

			JobComInvoiceLine testLine3 = testHeader5927.JobComInvoiceLines.AddNew();
			testLine3.JI_AddInfo = "PRF=U*ORG=US*GSTE=B41*ValuationBasis_Hidden=TV";
			testLine3.JI_Description = "POST OPERATIVE FOOTWEAR";
			testLine3.JI_LineNo = (short)1;
			testLine3.JI_LinePrice = 642.0000m;
			testLine3.JI_CountryOfOrigin = "US";
			testLine3.JI_Tariff = "9021.10.10 65";
			testLine3.JI_WeightUQ = "KG";

			JobComInvoiceLine testLine4 = testHeaderIT45148.JobComInvoiceLines.AddNew();
			testLine4.JI_AddInfo = "ORG=US*ValuationBasis_Hidden=TV";
			testLine4.JI_CustomsQuantity = 47.0000m;
			testLine4.JI_CustomsUnitQty = "PR";
			testLine4.JI_Description = "LEATHER FOOTWEAR";
			testLine4.JI_LineNo = (short)1;
			testLine4.JI_LinePrice = 2589.1500m;
			testLine4.JI_CountryOfOrigin = "US";
			testLine4.JI_Tariff = "6403.99.00 03";
			testLine4.JI_WeightUQ = "KG";

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();

			var entryLine1 = Factory.Load<CusEntryLine>(testLine1.JI_CL);
			var entryLine2 = Factory.Load<CusEntryLine>(testLine2.JI_CL);
			var entryLine3 = Factory.Load<CusEntryLine>(testLine3.JI_CL);
			var entryLine4 = Factory.Load<CusEntryLine>(testLine4.JI_CL);

			AssertEquals("One entry header is created", 1, testDec.CustomsEntryHeaders.Count);
			AssertEquals("Four entry lines are created", 4, testDec.CustomsEntryHeaders[0].MergedLines.Count);
			AssertNotNull("EntryLine1", entryLine1);
			AssertNotNull("EntryLine2", entryLine2);
			AssertNotNull("EntryLine3", entryLine3);
			AssertNotNull("EntryLine4", entryLine4);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];
			AssertEquals("ITOT incoterm", Core.Constants.IncoTerms.FreeOnBoard, entryHeader.ITOTIncoTerm);
			AssertEquals("Invoice total for EntryHeader in USD", 29983.41m, entryHeader.InvoiceTotal.Amount);
			AssertEquals("Invoice total for EntryHeader in USD", helper.USDCurrency, entryHeader.InvoiceTotal.Currency);
			AssertEquals("FOB For entry header", 29983.41m, entryHeader.FOB.Amount);
			AssertEquals("CIF for entry header", 32609m, entryHeader.CIF.Amount);
			AssertEquals("FIFT for entry header", 0m, entryHeader.ForeignInlandFreight.Amount);
			AssertEquals("OFT for entry header", 2550.63m, entryHeader.OverseasFreight.Amount);
			AssertEquals("ONS for entry header", 74.96m, entryHeader.OverseasInsurance.Amount);
			AssertEquals("Customs Value for entry header", 38258.79m, entryHeader.CustomsValue);

			AssertEquals("Invoice total for EntryLine1", 698.98m, entryLine1.Price.Amount);
			AssertEquals("Invoice Total for EntryLine2", 25954.16m, entryLine2.Price.Amount);
			AssertEquals("Invoice total for EntryLine3", 683.23m, entryLine3.Price.Amount);
			AssertEquals("Invoice Total for EntryLine4", 2647.04m, entryLine4.Price.Amount);

			AssertEquals("Customs Value for EntryLine1", 891.90m, entryLine1.CustomsValue.Amount);
			AssertEquals("Customs Value for EntryLine2", 33117.47m, entryLine2.CustomsValue.Amount);
			AssertEquals("Customs Value for EntryLine3", 871.80m, entryLine3.CustomsValue.Amount);
			AssertEquals("Customs Value for EntryLine4", 3377.62m, entryLine4.CustomsValue.Amount);

			AssertEquals("TranportInsurance for EntryLine1", 78.27m, entryLine1.TransportAndInsuranceInLocalCurrency.Amount);
			AssertEquals("TranportInsurance for EntryLine2", 2906.11m, entryLine2.TransportAndInsuranceInLocalCurrency.Amount);
			AssertEquals("TranportInsurance for EntryLine3", 72.69m, entryLine3.TransportAndInsuranceInLocalCurrency.Amount);
			AssertEquals("TranportInsurance for EntryLine4", 293.17m, entryLine4.TransportAndInsuranceInLocalCurrency.Amount);
		}

		public void TestInvoiceTotalToReportInMessageWhenChargeIsInDifferentCurrency()
		{
			ZTestHelper helper = new ZTestHelper(Factory);

			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_ExportDate = new ZDateTime(2005, 3, 2);
			helper.SetExchangeRate(testDec.JE_ExportDate, testDec.JE_ExportDate.AddDays(1), 0.783700m, helper.USDCurrency);

			testDec.JE_MasterBill = "08177777770";
			testDec.JE_MergeBy = "NON";
			testDec.JE_MessageSubType = "FRM";
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_OwnerRef = "40719-F,T334919,CASP";
			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			testDec.JE_RL_NKFinalDestination = "AUSYD";
			testDec.JE_RL_NKOrigin = "USKCK";
			testDec.JE_RL_NKPortOfArrival = "AUSYD";
			testDec.JE_RL_NKPortOfFirstArrival = "AUSYD";
			testDec.JE_RL_NKPortOfLoading = "USKCK";
			testDec.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			testDec.JE_SystemCreateTimeUtc = new ZDateTime(2005, 3, 31);
			testDec.JE_SystemCreateUser = "C";
			testDec.JE_SystemLastEditTimeUtc = new ZDateTime(2005, 4, 1);
			testDec.JE_SystemLastEditUser = "C";
			testDec.JE_TotalNoOfPacks = 32;
			testDec.JE_TotalNoOfPacksPackType = "PLT";
			testDec.JE_TotalVolume = 4.740m;
			testDec.JE_TotalVolumeUnit = "M3";
			testDec.JE_TotalWeight = 591.000m;
			testDec.JE_TotalWeightUnit = "KG";
			testDec.JE_TransportMode = Core.Constants.TransportModes.Air;
			testDec.JE_VoyageFlightNo = "QF108";

			JobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 333m, "AUD");
			groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 2550.63m, "USD");
			groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 74.96m, "USD");

			JobComInvoiceHeader testHeader0309682 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testHeader0309682.JZ_AddInfo = "ORG=US*PackCountForNature10_Hidden=30*ValuationBasis_Hidden=UT";
			testHeader0309682.JZ_IncoTerm = Core.Constants.IncoTerms.ExWorks;
			testHeader0309682.JZ_InvoiceAmount = 691.2000m;
			testHeader0309682.JZ_InvoiceCurrExRate = 0.783700000m;
			testHeader0309682.JZ_InvoiceDate = new ZDateTime(2004, 8, 10);
			testHeader0309682.JZ_InvoiceNumber = "0309682";
			testHeader0309682.JZ_RX_NKInvoice_Currency = "USD";
			testHeader0309682.JZ_Weight = 16.000m;
			testHeader0309682.JZ_WeightUQ = "KG";

			JobComInvoiceHeader testHeaderIT45148 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testHeaderIT45148.JZ_AddInfo = "ORG=US*PackCountForNature10_Hidden=10*ValuationBasis_Hidden=UT";
			testHeaderIT45148.JZ_IncoTerm = Core.Constants.IncoTerms.ExWorks;
			testHeaderIT45148.JZ_InvoiceAmount = 2617.9000m;
			testHeaderIT45148.JZ_InvoiceCurrExRate = 0.783700000m;
			testHeaderIT45148.JZ_InvoiceDate = new ZDateTime(2004, 8, 10);
			testHeaderIT45148.JZ_InvoiceNumber = "IT45148";
			testHeaderIT45148.JZ_RX_NKInvoice_Currency = "USD";
			testHeaderIT45148.JZ_Weight = 41.000m;
			testHeaderIT45148.JZ_WeightUQ = "KG";
			testHeaderIT45148.Charges.AddNew(CustomsChargeTypeList.Codes.AdditionCharge, 28.75m, "USD");//should be normalised...

			JobComInvoiceLine testLine1 = testHeader0309682.JobComInvoiceLines.AddNew();
			testLine1.JI_AddInfo = "PRF=U*ORG=US*ValuationBasis_Hidden=TV";
			testLine1.JI_Description = "FOOTWEAR PARTS";
			testLine1.JI_LineNo = (short)1;
			testLine1.JI_LinePrice = 691.2000m;
			testLine1.JI_CountryOfOrigin = "US";
			testLine1.JI_Tariff = "6406.99.99 66";
			testLine1.JI_WeightUQ = "KG";

			JobComInvoiceLine testLine4 = testHeaderIT45148.JobComInvoiceLines.AddNew();
			testLine4.JI_AddInfo = "ORG=US*ValuationBasis_Hidden=TV";
			testLine4.JI_CustomsQuantity = 47.0000m;
			testLine4.JI_CustomsUnitQty = "PR";
			testLine4.JI_Description = "LEATHER FOOTWEAR";
			testLine4.JI_LineNo = (short)1;
			testLine4.JI_LinePrice = 2589.1500m;
			testLine4.JI_CountryOfOrigin = "US";
			testLine4.JI_Tariff = "6403.99.00 03";
			testLine4.JI_WeightUQ = "KG";

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();

			var entryLine1 = Factory.Load<CusEntryLine>(testLine1.JI_CL);
			var entryLine4 = Factory.Load<CusEntryLine>(testLine4.JI_CL);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];
			AssertEquals("ITOT incoterm", Core.Constants.IncoTerms.FreeOnBoard, entryHeader.ITOTIncoTerm);
			AssertEquals("Invoice total for EntryHeader in AUD", 4555.41m, entryHeader.InvoiceTotal.Amount);
			AssertEquals("Invoice total for EntryHeader in AUD. FIFT is in a different currency to invoice currencies", helper.AUDCurrency, entryHeader.InvoiceTotal.Currency);

			AssertEquals("Invoice total for EntryLine1", 952.14m, entryLine1.Price.Amount);
			AssertEquals("Invoice Total for EntryLine4", 3603.27m, entryLine4.Price.Amount);
		}

		public void TestTwoCFRInvoicesWithGroupOFTRefB00148206()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_AddInfo = "ForcePrimeEnclosureIfMultipleEntries_Hidden=Y*NumberOfEntryPrints_Hidden=2*EFTReceiptPrinter_Hidden=06976*PrinterNumber_Hidden=06976*ClearanceAdvicePrinter_Hidden=06976";
			testDec.JE_ContainerMode = Core.Constants.ContainerModes.AIR;
			testDec.JE_DateAtFinalDestination = new ZDateTime(2005, 3, 3);
			testDec.JE_DateAtOrigin = new ZDateTime(2005, 3, 2);
			testDec.JE_DateOfArrival = new ZDateTime(2005, 3, 3);
			testDec.JE_DateOfFirstArrival = new ZDateTime(2005, 3, 3);
			testDec.JE_DeclarationReference = "B00148206";
			testDec.JE_EntrySubmittedDate = new ZDateTime(2005, 4, 1);
			testDec.JE_ExportDate = new ZDateTime(2005, 3, 2);
			testDec.JE_ExportGoodsType = "OT";
			testDec.JE_GB = GlbBranch.CurrentBranch.PK;
			testDec.JE_GoodsDescription = "PARTS FOR CRANE";
			testDec.JE_LandedCostByWeight = 100;
			testDec.JE_MasterBill = "08165154784";
			testDec.JE_MergeBy = "NON";
			testDec.JE_MessageSubType = "FRM";
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_OwnerRef = "PRO040263";
			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			testDec.JE_RL_NKFinalDestination = "AUSYD";
			testDec.JE_RL_NKOrigin = "USKCK";
			testDec.JE_RL_NKPortOfArrival = "AUSYD";
			testDec.JE_RL_NKPortOfFirstArrival = "AUSYD";
			testDec.JE_RL_NKPortOfLoading = "USKCK";
			testDec.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			testDec.JE_SystemCreateTimeUtc = new ZDateTime(2005, 4, 1);
			testDec.JE_SystemCreateUser = "C";
			testDec.JE_SystemLastEditTimeUtc = new ZDateTime(2005, 4, 1);
			testDec.JE_SystemLastEditUser = "C";
			testDec.JE_TotalNoOfPacks = 1;
			testDec.JE_TotalNoOfPacksPackType = "PCE";
			testDec.JE_TotalVolumeUnit = "M3";
			testDec.JE_TotalWeight = 13.000m;
			testDec.JE_TotalWeightUnit = "KG";
			testDec.JE_TransportMode = Core.Constants.TransportModes.Air;
			testDec.JE_VoyageFlightNo = "QF23";

			testDec.JobComInvoiceGroupHeaders[0].JZ_GroupInvoice = true;
			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 350m, "AUD");
			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 31.13m, "AUD");

			JobComInvoiceHeader testHeader1 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testHeader1.JZ_AddInfo = "ORG=FR*PackCountForNature10_Hidden=1*ValuationBasis_Hidden=RT";
			testHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			testHeader1.JZ_InvoiceAmount = 316.5800m;
			testHeader1.JZ_InvoiceCurrExRate = 1.000000000m;
			testHeader1.JZ_InvoiceDate = new ZDateTime(2004, 10, 11);
			testHeader1.JZ_InvoiceNumber = "4X00092";
			testHeader1.JZ_PaymentExRate = 1.000000000m;
			testHeader1.JZ_RX_NKInvoice_Currency = "AUD";
			testHeader1.JZ_Weight = 6.000m;
			testHeader1.JZ_WeightUQ = "KG";

			JobComInvoiceHeader testHeader2 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testHeader2.JZ_AddInfo = "PRF=X*ORG=SG*PackCountForNature10_Hidden=1*ValuationBasis_Hidden=RT";
			testHeader2.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			testHeader2.JZ_InvoiceAmount = 12484.2200m;
			testHeader2.JZ_InvoiceCurrExRate = 1.000000000m;
			testHeader2.JZ_InvoiceDate = new ZDateTime(2004, 10, 11);
			testHeader2.JZ_InvoiceNumber = "30/09/04";
			testHeader2.JZ_PaymentExRate = 1.000000000m;
			testHeader2.JZ_RX_NKInvoice_Currency = "AUD";
			testHeader2.JZ_Weight = 7.000m;
			testHeader2.JZ_WeightUQ = "KG";

			JobComInvoiceLine testLine1 = testHeader2.JobComInvoiceLines.AddNew();
			testLine1.JI_AddInfo = "PRF=X*ORG=SG*ValuationBasis_Hidden=TV";
			testLine1.JI_Description = "RADIO TRANSMISSION/RECEPTION APPARATUS";
			testLine1.JI_LineNo = (short)1;
			testLine1.JI_LinePrice = 11148.0000m;
			testLine1.JI_CountryOfOrigin = "SG";
			testLine1.JI_Tariff = "8525.20.00 89";
			testLine1.JI_WeightUQ = "KG";

			JobComInvoiceLine testLine2 = testHeader2.JobComInvoiceLines.AddNew();
			testLine2.JI_AddInfo = "PRF=X*ORG=SG*ValuationBasis_Hidden=TV";
			testLine2.JI_Description = "PARTS FOR TOWER CRANES";
			testLine2.JI_LineNo = (short)2;
			testLine2.JI_LinePrice = 1208.0600m;
			testLine2.JI_CountryOfOrigin = "SG";
			testLine2.JI_Tariff = "8431.49.90 33";
			testLine2.JI_WeightUQ = "KG";

			JobComInvoiceLine testLine3 = testHeader2.JobComInvoiceLines.AddNew();
			testLine3.JI_AddInfo = "PRF=X*ORG=SG*ValuationBasis_Hidden=TV";
			testLine3.JI_CustomsQuantity = 12.0000m;
			testLine3.JI_CustomsUnitQty = "NO";
			testLine3.JI_Description = "SWITCHES";
			testLine3.JI_InvoiceQuantity = 12.00000m;
			testLine3.JI_InvoiceUQ = "no";
			testLine3.JI_LineNo = (short)3;
			testLine3.JI_LinePrice = 128.1600m;
			testLine3.JI_CountryOfOrigin = "SG";
			testLine3.JI_Tariff = "8536.50.99 61";
			testLine3.JI_WeightUQ = "KG";

			JobComInvoiceLine testLine4 = testHeader1.JobComInvoiceLines.AddNew();
			testLine4.JI_AddInfo = "ORG=FR*ValuationBasis_Hidden=TV";
			testLine4.JI_CustomsQuantity = 2.0000m;
			testLine4.JI_CustomsUnitQty = "KG";
			testLine4.JI_Description = "CABLES FOR ELECTRONIC EQUIPMENT (INCLUDING RADIO & T.V. HOOK UP WIRES)";
			testLine4.JI_InvoiceQuantity = 2.00000m;
			testLine4.JI_InvoiceUQ = "KG";
			testLine4.JI_LineNo = (short)1;
			testLine4.JI_LinePrice = 142.3200m;
			testLine4.JI_CountryOfOrigin = "FR";
			testLine4.JI_Tariff = "8544.41.90 23";
			testLine4.JI_Weight = 2.000m;
			testLine4.JI_WeightUQ = "KG";

			JobComInvoiceLine testLine5 = testHeader1.JobComInvoiceLines.AddNew();
			testLine5.JI_AddInfo = "ORG=FR*ValuationBasis_Hidden=TV";
			testLine5.JI_CustomsQuantity = 1.0000m;
			testLine5.JI_CustomsUnitQty = "NO";
			testLine5.JI_Description = "CHARGER/TRANSFORMER";
			testLine5.JI_InvoiceQuantity = 1.00000m;
			testLine5.JI_InvoiceUQ = "no";
			testLine5.JI_LineNo = (short)2;
			testLine5.JI_LinePrice = 174.2600m;
			testLine5.JI_CountryOfOrigin = "FR";
			testLine5.JI_Tariff = "8504.40.90 80";
			testLine5.JI_WeightUQ = "KG";
			testLine5.AddInfo.ZA_PRF = "X";
			testLine5.AddInfo.ZA_AMB = "Y";

			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			testDec.JE_MergeBy = "NON";
			testDec.DoMerge();

			var entryLine1 = Factory.Load<CusEntryLine>(testLine1.JI_CL);
			var entryLine2 = Factory.Load<CusEntryLine>(testLine2.JI_CL);
			var entryLine3 = Factory.Load<CusEntryLine>(testLine3.JI_CL);
			var entryLine4 = Factory.Load<CusEntryLine>(testLine4.JI_CL);
			var entryLine5 = Factory.Load<CusEntryLine>(testLine5.JI_CL);

			AssertEquals("One entry header is created", 1, testDec.CustomsEntryHeaders.Count);
			AssertEquals("Four entry lines are created", 5, testDec.CustomsEntryHeaders[0].MergedLines.Count);
			AssertNotNull("EntryLine1", entryLine1);
			AssertNotNull("EntryLine2", entryLine2);
			AssertNotNull("EntryLine3", entryLine3);
			AssertNotNull("EntryLine4", entryLine4);
			AssertNotNull("EntryLine5", entryLine5);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];
			AssertEquals("ITOT incoterm", Core.Constants.IncoTerms.CostAndFreight, entryHeader.ITOTIncoTerm);
			AssertEquals("Invoice total for EntryHeader", 12800.80m, entryHeader.InvoiceTotal.Amount);
			AssertEquals("FOB For entry header", 12450.80m, entryHeader.FOB.Amount);
			AssertEquals("CIF for entry header", 12831.93m, entryHeader.CIF.Amount);
			AssertEquals("OFT for entry header", 350m, entryHeader.OverseasFreight.Amount);
			AssertEquals("ONS for entry header", 31.13m, entryHeader.OverseasInsurance.Amount);
			AssertEquals("Customs Value for entry header", 12450.80m, entryHeader.CustomsValue);

			AssertEquals("Invoice total for EntryLine1", 11148m, entryLine1.Price.Amount);
			AssertEquals("Invoice Total for EntryLine2", 1208.06m, entryLine2.Price.Amount);
			AssertEquals("Invoice total for EntryLine3", 128.16m, entryLine3.Price.Amount);
			AssertEquals("Invoice Total for EntryLine4", 142.32m, entryLine4.Price.Amount);
			AssertEquals("Invoice Total for EntryLine5", 174.26m, entryLine5.Price.Amount);

			AssertEquals("Customs Value for EntryLine1", 10843.18m, entryLine1.CustomsValue.Amount);
			AssertEquals("Customs Value for EntryLine2", 1175.03m, entryLine2.CustomsValue.Amount);
			AssertEquals("Customs Value for EntryLine3", 124.66m, entryLine3.CustomsValue.Amount);
			AssertEquals("Customs Value for EntryLine4", 138.43m, entryLine4.CustomsValue.Amount);
			AssertEquals("Customs Value for EntryLine5", 169.50m, entryLine5.CustomsValue.Amount);

			AssertEquals("TranportInsurance for EntryLine1", 331.93m, entryLine1.TransportAndInsuranceInLocalCurrency.Amount);
			AssertEquals("TranportInsurance for EntryLine2", 35.97m, entryLine2.TransportAndInsuranceInLocalCurrency.Amount);
			AssertEquals("TranportInsurance for EntryLine3", 3.81m, entryLine3.TransportAndInsuranceInLocalCurrency.Amount);
			AssertEquals("TranportInsurance for EntryLine4", 4.24m, entryLine4.TransportAndInsuranceInLocalCurrency.Amount);
			AssertEquals("TranportInsurance for EntryLine5", 5.18m, entryLine5.TransportAndInsuranceInLocalCurrency.Amount);
		}
	}
}
