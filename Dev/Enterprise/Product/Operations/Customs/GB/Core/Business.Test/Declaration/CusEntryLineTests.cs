using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration.Testing;
using Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;
using UniversalReferenceConstants = Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryLine))]
	class CusEntryLineTests : CusEntryLineTest<CusEntryLine, JobComInvoiceLine>
	{
		public new void TestPreviousDocuments()
		{
			Assert(true);
		}

		public void TestIsGrossMassMandatoryDueToCpc()
		{
			var declaration = (JobDeclaration)GetJobDeclarationForTest();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGroupCode = declaration.GetDefaultDataGroupingCode();
			var fullDecCpc = helper.CreateOrFindExistingRefCusProcedure(dataGroupCode, "00", "10", "00", "056", "Test", "EXP", "EFD");
			var otherCpc = helper.CreateOrFindExistingRefCusProcedure(dataGroupCode, "00", "10", "00", "046", "Test", "EXP", "EFD");
			fullDecCpc.Attributes.AddNew(Universal.AttributeNames.Codes.GrossMassMandatory, "anything");
			Factory.Save();

			declaration.JE_MessageType = "EXP";
			declaration.JE_DeclarationType = "EFD";
			var invHeader = declaration.Invoices.AddNew();
			var invLne1 = invHeader.InvoiceLines.AddNew();
			var invLine2 = invHeader.InvoiceLines.AddNew();
			invLne1.JI_Procedure = fullDecCpc.FullCodeCurrentPlusPreviousPlusConcession;
			invLine2.JI_Procedure = otherCpc.FullCodeCurrentPlusPreviousPlusConcession;

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			var entryLine1 = invLne1.CusEntryLine;
			var entryLine2 = invLine2.CusEntryLine;
			AssertEquals(true, entryLine1.IsGrossMassMandatoryDueToCpc);
			AssertEquals(false, entryLine2.IsGrossMassMandatoryDueToCpc);
		}

		public void TestPreviousDocumentsForUccCompliant() // Box40
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.CustomsEntryHeaders.AddNew();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var aPrevDocOnInvHeader = invoice.PreviousDocuments.AddNew();
			aPrevDocOnInvHeader.CSI_Code = "380";
			aPrevDocOnInvHeader.CSI_SubType = "X";
			aPrevDocOnInvHeader.CSI_ReferenceNumber = "PREVDOC INVHDR";

			var aPrevDocOnInvLine1 = invoiceLine.PreviousDocuments.AddNew();
			aPrevDocOnInvLine1.CSI_Code = "123";
			aPrevDocOnInvLine1.CSI_SubType = "Y";
			aPrevDocOnInvLine1.CSI_ReferenceNumber = "PREVDOC INVLINE";

			var aPrevDocOnInvLine2 = invoiceLine.PreviousDocuments.AddNew();
			aPrevDocOnInvLine2.CSI_Code = "456";
			aPrevDocOnInvLine2.CSI_SubType = "K";
			aPrevDocOnInvLine2.CSI_ReferenceNumber = "PREVDOC INVLINE";

			var aPrevDocOnGroupHeader = declaration.PreviousDocuments.AddNew();
			aPrevDocOnGroupHeader.CSI_Code = "987";
			aPrevDocOnGroupHeader.CSI_SubType = "Z";
			aPrevDocOnGroupHeader.CSI_ReferenceNumber = "PREVDOC GRPHDR";

			base.DoMerge(declaration);

			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			var aPrevDocToTest = entryLine.PreviousDocuments.GetEnumerator();

			CombineAssertions(() =>
			{
				aPrevDocToTest.MoveNext();
				AssertEquals("aPrevDoc.CSI_Code = 380", aPrevDocToTest.Current.CSI_Code, aPrevDocOnInvHeader.CSI_Code);

				aPrevDocToTest.MoveNext();
				AssertEquals("aPrevDocOnInvLine.CSI_Code = 123", aPrevDocToTest.Current.CSI_Code, aPrevDocOnInvLine1.CSI_Code);
				AssertEquals("aPrevDocOnInvLine.CSI_SubType = Y", aPrevDocToTest.Current.CSI_SubType, aPrevDocOnInvLine1.CSI_SubType);

				aPrevDocToTest.MoveNext();
				AssertEquals("aPrevDocOnInvLine.CSI_Code = 456", aPrevDocToTest.Current.CSI_Code, aPrevDocOnInvLine2.CSI_Code);
				AssertEquals("aPrevDocOnInvLine.CSI_SubType = K", aPrevDocToTest.Current.CSI_SubType, aPrevDocOnInvLine2.CSI_SubType);
			});
		}

		public void TestIsNorthernIrelandDomestic()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var entry = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var additionalInfo1 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo1.CSI_Code = GBCommonConstants.AdditonalInfoCodes.NIAID;
			Assert(!entryLine.IsNorthernIrelandDomestic);

			var additionalInfo2 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo2.CSI_Code = GBCommonConstants.AdditonalInfoCodes.NIDOM;
			Assert(entryLine.IsNorthernIrelandDomestic);
		}

		public void TestIsNorthernIrelandImportFromRow()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var entry = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var additionalInfo1 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo1.CSI_Code = GBCommonConstants.AdditonalInfoCodes.NIAID;
			Assert(!entryLine.IsNorthernIrelandImportFromRow);

			var additionalInfo2 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo2.CSI_Code = GBCommonConstants.AdditonalInfoCodes.NIIMP;
			Assert(entryLine.IsNorthernIrelandImportFromRow);
		}

		public void TestIsAtRisk()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var entry = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var additionalInfo1 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo1.CSI_Code = GBCommonConstants.AdditonalInfoCodes.NIAID;
			Assert(entryLine.IsAtRisk);

			var additionalInfo2 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo2.CSI_Code = GBCommonConstants.AdditonalInfoCodes.NIREM;
			Assert(!entryLine.IsAtRisk);
		}

		public void TestIsEuTariffToBeUsedForNorthernIreland()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var entry = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var additionalInfo1 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo1.CSI_Code = GBCommonConstants.AdditonalInfoCodes.NIAID;
			Assert(!entryLine.IsEuTariffToBeUsedForNorthernIreland);

			var additionalInfo2 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo2.CSI_Code = GBCommonConstants.AdditonalInfoCodes.NIDOM;
			Assert(entryLine.IsEuTariffToBeUsedForNorthernIreland);

			var additionalInfo3 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo3.CSI_Code = GBCommonConstants.AdditonalInfoCodes.NIREM;
			Assert(entryLine.IsEuTariffToBeUsedForNorthernIreland);

			additionalInfo2.CSI_Code = GBCommonConstants.AdditonalInfoCodes.NIIMP;
			Assert(!entryLine.IsEuTariffToBeUsedForNorthernIreland);
		}

		public void TestGetCDSChargeDeductions()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = "CDS";
			var entry = (CusEntryHeader)dec.ActiveEntryHeaders.AddNew();
			var entryLine1 = entry.AllEntryLines.AddNew();

			var invoice1 = dec.Invoices.AddNew();
			var invoice1Line1 = invoice1.InvoiceLines.AddNew();
			invoice1Line1.JI_CL = entryLine1.PK;

			var invoice2 = dec.Invoices.AddNew();
			var invoice2Line1 = invoice2.InvoiceLines.AddNew();
			invoice2Line1.JI_CL = entryLine1.PK;

			var charge1 = invoice1Line1.Charges.AddNew();
			var charge2 = invoice1Line1.Charges.AddNew();
			var charge3 = invoice1Line1.Charges.AddNew();
			var charge4 = invoice1Line1.Charges.AddNew();
			var charge5 = invoice2Line1.Charges.AddNew();
			var charge6 = invoice2Line1.Charges.AddNew();
			var charge7 = invoice2Line1.Charges.AddNew();
			var charge8 = invoice2Line1.Charges.AddNew();
			var charge9 = invoice2Line1.ApportionedCharges.AddNew();
			var charge10 = invoice2Line1.ApportionedCharges.AddNew();
			InvChargeTestHelper.SetUpCharge(charge1, "CBR", false, 1.1m, string.Empty, true, 1m);//AB Item
			InvChargeTestHelper.SetUpCharge(charge2, "CBR", false, 1.1m, string.Empty, false, 2m);//AB Item
			InvChargeTestHelper.SetUpCharge(charge3, "OFT", true, 0m, "VAL", false, 3m);//AP Header
			InvChargeTestHelper.SetUpCharge(charge4, "OFT", true, 0m, "VAL", true, 4m);//AP Header
			InvChargeTestHelper.SetUpCharge(charge5, "OFT", true, 0m, "VAL", false, 5m);//AP Header
			InvChargeTestHelper.SetUpCharge(charge6, "CEA", false, 0m, string.Empty, false, 6m);//BB Item
			charge6.J7_IsGSTApplicable = true;
			InvChargeTestHelper.SetUpCharge(charge7, "CEA", false, 0m, string.Empty, false, 7m);//BB Item
			charge7.J7_IsStatisticalValueApplicable = true;

			InvChargeTestHelper.SetUpCharge(charge8, "OFT", true, 0m, "VAL", false, 5m);//AP Header
			charge8.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.China;
			InvChargeTestHelper.SetUpCharge(charge9, "OFT", true, 0m, "VAL", false, 6m);//AP Header
			charge9.J7_IsGSTApplicable = false;
			charge9.J7_Calc_IsIncludedInInvoiceAmount = true;
			InvChargeTestHelper.SetUpCharge(charge10, "OFT", true, 0m, "VAL", false, 7m);//AP Header
			charge10.J7_IsStatisticalValueApplicable = false;
			charge10.J7_Calc_IsIncludedInInvoiceAmount = true;

			var deductions = entryLine1.CDSChargeDeductions;
			AssertEquals(2, deductions.Count);
			AssertEquals(3m, deductions["AB.GBP"].Amount);
			AssertEquals(13m, deductions["BB.GBP"].Amount);
		}

		protected override string OverseasFreightCode => ChargesProvider.AirFreightCode;

		public override void TestMultiInvoiceMergeShowsInvoicesOnBothLines()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";
			var invoice1 = dec.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV1";
			invoice1.JobComInvoiceLines.AddNew().JI_Tariff = "22030010";
			invoice1.JobComInvoiceLines.AddNew().JI_Tariff = "22030020";
			var invoice2 = dec.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "INV2";
			invoice2.JobComInvoiceLines.AddNew().JI_Tariff = "22030010";
			invoice2.JobComInvoiceLines.AddNew().JI_Tariff = "22030020";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals(2, dec.CustomsEntryHeaders[0].MergedLines.Count);
			var entryLine1 = dec.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals(2, entryLine1.PreviousDocuments.Count());
			var entryLine2 = dec.CustomsEntryHeaders[0].MergedLines[1];
			AssertEquals(2, entryLine2.PreviousDocuments.Count());
		}

		public void TestLineShouldHideGrossMassForImportUnlessWarehouseIsDefinedOrUnlessWeightApportionmentIsInUseImport()
		{
			JobDeclaration dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = "CDS";
			dec.JE_MessageType = "IMP";

			var invoiceLine = dec.Invoices.AddNew().JobComInvoiceLines.AddNew();
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			CusEntryLine entryLine = dec.CustomsEntryHeaders[0].MergedLines[0];

			ZWeight soRealWeight = new ZWeight(123.45, "KG");
			entryLine.RandomLine.JI_Weight = soRealWeight.InKilograms;

			// Check that it shows Empty for the gross weight
			entryLine.Declaration.ZG_ApportionByWeight = false;
			AssertEquals(true, entryLine.EffectiveGrossWeightIsApplicable);

			// Check that is we are apportioning by weight, we show the real weight
			entryLine.Declaration.ZG_ApportionByWeight = true;
			AssertEquals(true, entryLine.EffectiveGrossWeightIsApplicable);

			// Check that if we have a warehouse, we show the real weight
			entryLine.Declaration.ZG_ApportionByWeight = false;
			entryLine.Declaration.WarehouseDocAddress.E2_OA_Address = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;
			AssertEquals(true, entryLine.EffectiveGrossWeightIsApplicable);
		}

		public override void TestAdditionalInfos()
		{
			CusEntryHeaderTestHelper.CreateAdditionalInfos(Factory);

			var dec = GetJobDeclarationForTest();
			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var inv = dec.Invoices.AddNew();
			var invLine1 = inv.JobComInvoiceLines.AddNew();
			invLine1.JI_Tariff = "2203001010";
			var invLine2 = inv.JobComInvoiceLines.AddNew();
			invLine2.JI_Tariff = "2203001010";

			var addInfo1 = invLine1.AdditionalInfos.AddNew();
			addInfo1.CSI_Code = "9001";
			addInfo1.CSI_Description = "9001 Desc";

			var addInfo2 = invLine2.AdditionalInfos.AddNew();
			addInfo2.CSI_Code = "9001";
			addInfo2.CSI_Description = "9001 Desc";

			var addInfo3 = inv.AdditionalInfos.AddNew();
			addInfo3.CSI_Code = "9003";
			addInfo3.CSI_Description = "9003 Desc";

			DoMerge(dec);

			AssertEquals(1, dec.ActiveEntryHeaders.Count);
			AssertEquals(1, dec.ActiveEntryHeaders[0].MergedLines.Count);
			AssertEquals(2, ((CusEntryLine)dec.ActiveEntryHeaders[0].MergedLines[0]).AdditionalInfos.Count());

			addInfo2.CSI_Code = "12345";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(2, dec.ActiveEntryHeaders[0].MergedLines.Count);
		}

		public void TestAdditionalInfosSomeMore()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			AdditionalInfo addInfo = invoice.AdditionalInfos.AddNew();
			addInfo.CSI_Code = "123";

			addInfo = invoiceLine.AdditionalInfos.AddNew();
			addInfo.CSI_Code = "380";

			addInfo = invoiceLine.AdditionalInfos.AddNew();
			addInfo.CSI_Code = "456";

			addInfo = declaration.AdditionalInfos.AddNew();
			addInfo.CSI_Code = "789";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			CusEntryLine entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			IEnumerator<AdditionalInfo> addInfoToTest = entryLine.AdditionalInfos.GetEnumerator();

			addInfoToTest.MoveNext();
			AssertEquals("addInfoToTest.Current.CSI_Code = 380", addInfoToTest.Current.CSI_Code, "380");
			addInfoToTest.MoveNext();
			AssertEquals("addInfoToTest.Current.CSI_Code = 456", addInfoToTest.Current.CSI_Code, "456");

			addInfoToTest.Dispose();
		}

		public void TestAdditonalInfosForCDS()
		{
			var refCusCodeCDS = Factory.New<ZZRefCusCodeListCombined>();
			refCusCodeCDS.ZZD_CodeType = "ADDIN";
			refCusCodeCDS.ZZD_Code = "00500";
			refCusCodeCDS.ZZD_CountryOrGrouping = "CDS";
			var attribute = refCusCodeCDS.Attributes.AddNew();
			attribute.ZZE_ZXE_NKName = RefCusCodeListAttributeTypes.Codes.Level;
			attribute.ZZE_Value = RefCusCodeListLevelType.Item;

			var refCusCodeCDS2 = Factory.New<ZZRefCusCodeListCombined>();
			refCusCodeCDS2.ZZD_CodeType = "ADDIN";
			refCusCodeCDS2.ZZD_Code = "00501";
			refCusCodeCDS2.ZZD_CountryOrGrouping = "CDS";
			var attribute2 = refCusCodeCDS2.Attributes.AddNew();
			attribute2.ZZE_ZXE_NKName = RefCusCodeListAttributeTypes.Codes.Level;
			attribute2.ZZE_Value = RefCusCodeListLevelType.Item;

			var refCusCodeCHF = Factory.New<ZZRefCusCodeListCombined>();
			refCusCodeCHF.ZZD_CodeType = "ADDIN";
			refCusCodeCHF.ZZD_Code = "00500";
			refCusCodeCHF.ZZD_CountryOrGrouping = "GB";
			var attribute3 = refCusCodeCHF.Attributes.AddNew();
			attribute3.ZZE_ZXE_NKName = RefCusCodeListAttributeTypes.Codes.Level;
			attribute3.ZZE_Value = RefCusCodeListLevelType.Header;

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "CDS";
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var entryInstruction = declaration.CustomsEntryInstructions.FirstOrDefault();
			var addInfoCEI = entryInstruction.AdditionalInfos.AddNew();
			addInfoCEI.CSI_Code = "00500";

			AdditionalInfo addInfo = invoice.AdditionalInfos.AddNew();
			addInfo.CSI_Code = "123";

			addInfo = invoiceLine.AdditionalInfos.AddNew();
			addInfo.CSI_Code = "380";

			addInfo = invoiceLine.AdditionalInfos.AddNew();
			addInfo.CSI_Code = "456";

			addInfo = declaration.AdditionalInfos.AddNew();
			addInfo.CSI_Code = "00501";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			CusEntryLine entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			var addInfoToTest = entryLine.AdditionalInfos;

			AssertEquals("380 from invoice line should be present", true, addInfoToTest.Any(x => x.CSI_Code == "380"));
			AssertEquals("456 from invoice line should be present", true, addInfoToTest.Any(x => x.CSI_Code == "456"));
			AssertEquals("00500 from entry instruction should be present", true, addInfoToTest.Any(x => x.CSI_Code == "00500"));
			AssertEquals("00501 from declaration should be present", true, addInfoToTest.Any(x => x.CSI_Code == "00501"));
		}

		public void TestSupportingDocs()
		{
			var declaration = (JobDeclaration)GetJobDeclarationForTest();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var dataGroupCode = declaration.GetDefaultDataGroupingCode();
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var gbDataGrouping = helper.CreateNewOrGetExistingDataGrouping("GB", "United Kingdom");
			var cdsDataGrouping = helper.CreateNewOrGetExistingDataGrouping(dataGroupCode, "GB Customs Declaration Services (CDS)", parent: gbDataGrouping);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>();
			attributeNameValuePairs.Add("Level", new string[] { "ITEM", "HEADER" });
			attributeNameValuePairs.Add("ACTAV", new string[] { "AC" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(gbDataGrouping.ZZZ_DataGrouping,
				new string[] { importCodeType, exportCodeType }, "9999", "9999 DES", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			attributeNameValuePairs.Clear();
			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			attributeNameValuePairs.Add("ACTAV", new string[] { "AC" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(cdsDataGrouping.ZZZ_DataGrouping,
				new string[] { importCodeType, exportCodeType }, "9999", "9999 DES", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue.AddDays(1), ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			declaration.JE_MessageType = "IMP";
			var invHeader1 = declaration.Invoices.AddNew();
			var invLine1 = invHeader1.InvoiceLines.AddNew();
			var suppDocsForHeader = invHeader1.SupportingDocuments.AddNew("9999", "12345");
			var collection = (ZZRefCusCodeListCombinedCollection)suppDocsForHeader.Lookups.CodeList;
			collection.Load();
			var invHeader2 = declaration.Invoices.AddNew();
			var invLine2 = invHeader2.InvoiceLines.AddNew();

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			var entryLine1 = invLine1.CusEntryLine;
			var entryLine2 = invLine2.CusEntryLine;
			AssertEquals(1, entryLine1.ReadOnlySupportingDocuments.Count);
			AssertEquals(0, entryLine2.ReadOnlySupportingDocuments.Count);
		}

		public void TestCDSEntryLineValidation()
		{
			var declarationCDS = Factory.New<JobDeclaration>();
			declarationCDS.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var invoiceCDS = declarationCDS.Invoices.AddNew();
			var invoiceLine1 = invoiceCDS.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Weight = 5;
			declarationCDS.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNoNotifications(declarationCDS.CustomsEntryHeaders[0].MergedLines[0].CL_CustomsPostedStatusInfo);
			declarationCDS.CustomsEntryHeaders[0].MergedLines[0].CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.DeletePending;
			AssertHasNotifications("CDS Entry Line with status DeletePending should have a notification", declarationCDS.CustomsEntryHeaders[0].MergedLines[0].CL_CustomsPostedStatusInfo);

			var declarationCHIEF = Factory.New<JobDeclaration>();
			declarationCHIEF.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			declarationCHIEF.CustomsEntryHeaders.AddNew();
			var invoiceCHIEF = declarationCHIEF.Invoices.AddNew();
			invoiceCHIEF.JobComInvoiceLines.AddNew();
			declarationCHIEF.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNoNotifications(declarationCHIEF.CustomsEntryHeaders[0].MergedLines[0].CL_CustomsPostedStatusInfo);
			declarationCHIEF.CustomsEntryHeaders[0].MergedLines[0].CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.DeletePending;
			AssertNoNotifications(declarationCHIEF.CustomsEntryHeaders[0].MergedLines[0].CL_CustomsPostedStatusInfo);
		}

		public void TestProcedureCodeWithoutConcession()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoiceLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			var entryHeader = dec.ActiveEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.AllEntryLines.AddNew() as CusEntryLine;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Procedure = "4000123";
			AssertEquals("4000", entryLine.ProcedureCodeWithoutConcession);
		}

		public void TestFiscalReferences()
		{
			var declaration = Factory.New<JobDeclaration>();

			CommonTestData.CreateFiscalReferenceData(declaration);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals(3, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			var entryLineA = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals(0, entryLineA.FiscalReferences.Count());

			var entryLineB = declaration.CustomsEntryHeaders[0].MergedLines[1];
			AssertEquals(2, entryLineB.FiscalReferences.Count());
			AssertNotNull(entryLineB.FiscalReferences.FirstOrDefault(x => x.CFR_Code == "FR1" && x.CFR_Reference == "GB11111111"));
			AssertNotNull(entryLineB.FiscalReferences.FirstOrDefault(x => x.CFR_Code == "FR2" && x.CFR_Reference == "GB22222222"));

			var entryLineC = declaration.CustomsEntryHeaders[0].MergedLines[2];
			AssertEquals(3, entryLineC.FiscalReferences.Count());
			AssertNotNull(entryLineC.FiscalReferences.FirstOrDefault(x => x.CFR_Code == "FR1" && x.CFR_Reference == "GB11111111"));
			AssertNotNull(entryLineC.FiscalReferences.FirstOrDefault(x => x.CFR_Code == "FR2" && x.CFR_Reference == "GB33333333"));
			AssertNotNull(entryLineC.FiscalReferences.FirstOrDefault(x => x.CFR_Code == "FR4" && x.CFR_Reference == "GB44444444"));
		}

		void CreateRefDataForNIDutyAndVat()
		{
			var refDataHelper = new UniversalReferenceTestDataHelper(Factory);

			var dtyRateType = refDataHelper.CreateNewOrGetExistingRateType(dataGroupingCode: "CDS", rateType: Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, description: "Customs duties on industrial products");
			refDataHelper.CreateCusRateCode(Factory, zy1RateCode: GBCommonConstants.NorthernIrelandDutyCodes.CustomsDuty, dtyRateType.PK);
			var gbDataGrouping = refDataHelper.CreateNewOrGetExistingDataGrouping("GB", "United Kingdom");
			refDataHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, dtyRateType.PK);
			refDataHelper.CreateNewOrGetExistingDataGrouping("CDS", "", parent: gbDataGrouping);

			Factory.Save();
		}

		CusEntryLine CreateEntryLineForNIDutyAndVat()
		{
			CreateRefDataForNIDutyAndVat();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.CustomsEntryHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			return declaration.CustomsEntryHeaders[0].MergedLines[0];
		}

		public void TestNIDutyAndTaxIncluded()
		{
			var entryLine = CreateEntryLineForNIDutyAndVat();

			var fee1 = entryLine.Fees.AddOrUpdate(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, 111m);

			var fee2 = entryLine.Fees.AddNew();
			fee2.CF_MethodOfPayment = "A";
			fee2.CF_ChargeType = "A50";
			fee2.CF_ChargeAmount = 333m;

			var fee3 = entryLine.Fees.AddNew();
			fee3.CF_MethodOfPayment = "A";
			fee3.CF_ChargeType = "B00";
			fee3.CF_ChargeAmount = 666m;

			var fee4 = entryLine.Fees.AddNew();
			fee4.CF_MethodOfPayment = "E";
			fee4.CF_ChargeType = "B05";
			fee4.CF_ChargeAmount = 777m;

			CombineAssertions(() =>
			{
				AssertEquals("CustomsEntryLine.GSTVATDeferred", 777m, entryLine.GSTVATDeferred);
				AssertEquals("CustomsEntryLine.DutyAmount", 444m, entryLine.DutyAmount);
				AssertEquals("CustomsEntryLine.GSTVATAmount", 1443m, entryLine.GSTVATAmount);
			});
		}

		public void TestDutyAndVatConfirmedFees()
		{
			var entryLine = CreateEntryLineForNIDutyAndVat();
			entryLine.Fees.AddOrUpdate(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, 111m);
			entryLine.Fees.AddOrUpdate(GBCommonConstants.NorthernIrelandDutyCodes.CustomsDuty, 333m);
			entryLine.Fees.AddOrUpdate(UniversalReferenceConstants.RefCusRateCodes.Vat, 666m);
			entryLine.Fees.AddOrUpdate(UniversalReferenceConstants.RefCusRateCodes.VatOnAdditionalDutiesForNorthernIreland, 777m).CF_MethodOfPayment = "E";

			entryLine.ConfirmedFees.AddOrUpdate(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, 111.11m);
			entryLine.ConfirmedFees.AddOrUpdate(GBCommonConstants.NorthernIrelandDutyCodes.CustomsDuty, 333.33m);
			entryLine.ConfirmedFees.AddOrUpdate(UniversalReferenceConstants.RefCusRateCodes.Vat, 666.66m);
			entryLine.ConfirmedFees.AddOrUpdate(UniversalReferenceConstants.RefCusRateCodes.VatOnAdditionalDutiesForNorthernIreland, 777.77m).CF_MethodOfPayment = "E";

			CombineAssertions(() =>
			{
				AssertEquals("CustomsEntryLine.GSTVATDeferred", 777.77m, entryLine.GSTVATDeferred);
				AssertEquals("CustomsEntryLine.DutyAmount", 444.44m, entryLine.DutyAmount);
				AssertEquals("CustomsEntryLine.GSTVATAmount", 1444.43m, entryLine.GSTVATAmount);
			});
		}

		public void TestDutyAndVatConfirmedFeesTwoLines()
		{
			CreateRefDataForNIDutyAndVat();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var header = declaration.CustomsEntryHeaders.AddNew();
			var line1 = header.MergedLines.AddNew();
			var line2 = header.MergedLines.AddNew();

			line1.Fees.AddOrUpdate(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, 111m);
			line1.Fees.AddOrUpdate(GBCommonConstants.NorthernIrelandDutyCodes.CustomsDuty, 333m);
			line1.Fees.AddOrUpdate(UniversalReferenceConstants.RefCusRateCodes.Vat, 666m);
			line1.Fees.AddOrUpdate(UniversalReferenceConstants.RefCusRateCodes.VatOnAdditionalDutiesForNorthernIreland, 777m).CF_MethodOfPayment = "E";
			line1.ConfirmedFees.AddOrUpdate(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, 111.11m);
			line1.ConfirmedFees.AddOrUpdate(GBCommonConstants.NorthernIrelandDutyCodes.CustomsDuty, 333.33m);
			line1.ConfirmedFees.AddOrUpdate(UniversalReferenceConstants.RefCusRateCodes.Vat, 666.66m);
			line1.ConfirmedFees.AddOrUpdate(UniversalReferenceConstants.RefCusRateCodes.VatOnAdditionalDutiesForNorthernIreland, 777.77m).CF_MethodOfPayment = "E";

			line2.Fees.AddOrUpdate(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, 111m);
			line2.Fees.AddOrUpdate(GBCommonConstants.NorthernIrelandDutyCodes.CustomsDuty, 333m);
			line2.Fees.AddOrUpdate(UniversalReferenceConstants.RefCusRateCodes.Vat, 666m);
			line2.Fees.AddOrUpdate(UniversalReferenceConstants.RefCusRateCodes.VatOnAdditionalDutiesForNorthernIreland, 777m).CF_MethodOfPayment = "E";

			CombineAssertions(() =>
			{
				AssertEquals("CustomsEntryLine1.GSTVATDeferred", 777.77m, line1.GSTVATDeferred);
				AssertEquals("CustomsEntryLine1.DutyAmount", 444.44m, line1.DutyAmount);
				AssertEquals("CustomsEntryLine1.GSTVATAmount", 1444.43m, line1.GSTVATAmount);
				AssertEquals("CustomsEntryLine2.GSTVATDeferred", 0m, line2.GSTVATDeferred);
				AssertEquals("CustomsEntryLine2.DutyAmount", 0m, line2.DutyAmount);
				AssertEquals("CustomsEntryLine2.GSTVATAmount", 0m, line2.GSTVATAmount);
			});
		}

		protected override CusEntryLine GetCusEntryLineForTestGetTaxBoxSupporterList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.Taxes.AddNew();
			return entryLine;
		}

		public override void TestDutyRateDescription()
		{
			// Only testing CHIEF fucntionality as CDS is tested in the base test
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var cusEntryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = cusEntryLine.PK;
			AssertEquals(ZDecimal.Zero, cusEntryLine.GSTRate);
			AssertEquals(ZString.Empty, cusEntryLine.DutyRateDescription);
			AssertEquals(ZString.Empty, invoiceLine.DutyAmountsAsString);
			var b00Tax = invoiceLine.Taxes.AddNew();
			b00Tax.JLT_Amount = 175m;
			b00Tax.JLT_BaseValue = 1000m;
			b00Tax.JLT_Type = UniversalReferenceConstants.RefCusRateCodes.Vat;
			var a30Tax = invoiceLine.Taxes.AddNew();
			a30Tax.JLT_Type = UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty;
			a30Tax.JLT_Amount = 202m;
			a30Tax.JLT_BaseValue = 404m;
			var a00Tax = invoiceLine.Taxes.AddNew();
			a00Tax.JLT_Amount = 33m;
			a00Tax.JLT_BaseValue = 100m;
			a00Tax.JLT_Type = UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			AssertEquals("GetGSTRate():  GST/VAT rates pulls only the B00 & B05 tax lines and looks at its Calculated Percentage", 17.5m, cusEntryLine.GSTRate);
			AssertEquals("GetDutyRateDescription(): Pulls all but VAT, sorted by code, formatted % per line", "A00:33%\r\nA30:50%", cusEntryLine.DutyRateDescription);
			AssertEquals("DutyAmountsAsString(Core): Pulls all but VAT, sorted by code, formatted £ per line", "A00:33.00\r\nA30:202.00", invoiceLine.DutyAmountsAsString);

			b00Tax.JLT_Amount = 363.63m;
			b00Tax.JLT_BaseValue = 1818.17m;

			a30Tax.JLT_Amount = 170.96m;
			a30Tax.JLT_BaseValue = 569.87;

			a00Tax.JLT_Amount = 35.33m;
			a00Tax.JLT_BaseValue = 1766.82m;

			AssertEquals("GetGSTRate():  GST/VAT rates pulls only the B00 & B05 tax lines and looks at its Calculated Percentage and should be suitably rounded", 20.0m, cusEntryLine.GSTRate);
			AssertEquals("GetDutyRateDescription(): Pulls all but VAT, sorted by code, formatted % per line", "A00:2%\r\nA30:30%", cusEntryLine.DutyRateDescription);
			AssertEquals("DutyAmountsAsString(Core): Pulls all but VAT, sorted by code, formatted £ per line", "A00:35.33\r\nA30:170.96", invoiceLine.DutyAmountsAsString);
		}

		public void TestConsignorConsignee()
		{
			var dec = Factory.New<JobDeclaration>();
			var header = dec.CustomsEntryHeaders.AddNew();
			var sup = Factory.New<OrgHeader>();
			var imp = Factory.New<OrgHeader>();
			dec.JE_OH_Importer = imp.PK;
			dec.JE_OH_Supplier = sup.PK;
			var line = Factory.New<CusEntryLine>();
			line.CL_CH = header.PK;
			AssertEquals(sup.PK, line.Consignor.OA_OH);
			AssertEquals(imp.PK, line.Consignee.OA_OH);
		}

		protected override int ExpectedReadOnlySupportingDocumentsCount => 9;

		protected override Type GetExpectedTaxBoxSupporterType() => typeof(EU.Business.Declaration.MultiLineAddInfos.TaxStruct);

		protected override SupportingDocTestHelper GetSupportingDocTestHelper() => new GBSupportingDocsTestHelper(Factory);

		protected override Type ExpectedTypeOfFees => typeof(EU.Business.Declaration.CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>);

		public override EU.Business.Declaration.JobDeclaration GetJobDeclarationForTest()
		{
			var dec = base.GetJobDeclarationForTest();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			return dec;
		}

		public override EU.Business.Declaration.JobDeclaration GetJobDeclarationForTestWithValidTestData()
		{
			var dec = base.GetJobDeclarationForTestWithValidTestData();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			return dec;
		}

		public class GBSupportingDocsTestHelper : SupportingDocTestHelper
		{
			public GBSupportingDocsTestHelper(BusinessObjectFactory factory) : base(factory)
			{
			}

			protected override Type GetSupportingDocumentType() => typeof(SupportingDocument);

			public override List<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument> GetSupportingDocsForAggregationMergeTest()
			{
				var docs = base.GetSupportingDocsForAggregationMergeTest().ToList();

				docs.AddRange(new[]
				{
					GetSupportingDoc(9, "REF666", false, 10, 1),
					GetSupportingDoc(10, "REF666", false, 10, 2),
					GetSupportingDoc(11, "REF666", false, 10, 3),
					GetSupportingDoc(12, "REF666", false, 10, 4),
					GetSupportingDoc(9, "REF666", false, 10, 1),
					GetSupportingDoc(10, "REF666", false, 10, 2),
					GetSupportingDoc(11, "REF666", false, 10, 3),
					GetSupportingDoc(12, "REF666", false, 10, 4)
				});

				return docs;
			}

			protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocument GetSupportingDoc(int i, ZString refNumber, bool alternateSubType, decimal qty3 = 10.0M, int flag = 0)
			{
				var doc = (SupportingDocument)base.GetSupportingDoc(i, refNumber, alternateSubType, qty3);

				doc.CSI_Actions = "A";
				doc.CSI_Availability = "A";

				if (flag == 2 || flag == 4)
				{
					doc.CSI_Availability = "B";
				}

				if (flag == 3 || flag == 4)
				{
					doc.CSI_Actions = "B";
				}

				return doc;
			}

			public override EU.Business.Declaration.MultiLineAddInfos.SupportingDocument MatchExpected(EU.Business.Declaration.MultiLineAddInfos.SupportingDocument merged)
			{
				var m = (SupportingDocument)merged;

				return expectedSupportingDocs.Cast<SupportingDocument>().FirstOrDefault(x => x.CSI_ReferenceNumber == m.CSI_ReferenceNumber && x.CSI_SubType == m.CSI_SubType && x.CSI_Actions == m.CSI_Actions && x.CSI_Availability == m.CSI_Availability);
			}

			public override List<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument> GetExpectedDocsAfterAggregationMerge()
			{
				var results = base.GetExpectedDocsAfterAggregationMerge();

				results.AddRange(new[]
				{
					GetSupportingDoc(18, "REF666", false, 20, 1),
					GetSupportingDoc(20, "REF666", false, 20, 2),
					GetSupportingDoc(22, "REF666", false, 20, 3),
					GetSupportingDoc(24, "REF666", false, 20, 4)
				});

				return results.Cast<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument>().ToList();
			}

			public override string GetErrorMsg(EU.Business.Declaration.MultiLineAddInfos.SupportingDocument merged)
			{
				var m = (SupportingDocument)merged;

				var msg = $"GB Expected {SupportingDocument.Schema.CSI_ReferenceNumber}: {m.CSI_ReferenceNumber} {SupportingDocument.Schema.CSI_SubType}: {m.CSI_SubType} {SupportingDocument.Schema.CSI_Actions}: {m.CSI_Actions} {SupportingDocument.Schema.CSI_Availability}: {m.CSI_Availability}\nPossible Options:\n";

				foreach (var exp in expectedSupportingDocs.Cast<SupportingDocument>())
				{
					msg += $"{SupportingDocument.Schema.CSI_ReferenceNumber}: {exp.CSI_ReferenceNumber} {SupportingDocument.Schema.CSI_SubType}: {exp.CSI_SubType} {SupportingDocument.Schema.CSI_Actions}: {exp.CSI_Actions} {SupportingDocument.Schema.CSI_Availability}: {exp.CSI_Availability}\n";
				}

				return msg;
			}
		}
	}
}
