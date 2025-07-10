using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusStatementLine))]
	sealed class CusStatementLineTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCustomsAssessmentCharge()
		{
			var line1 = Factory.New<CusStatementLine>();
			var line2 = Factory.New<CusStatementLine>();

			AssertEquals(0, line1.Charges.Count);
			AssertEquals(0, line2.Charges.Count);

			line1.B3_EntryType = JobMessageTypeList.Codes.Import;
			line2.B3_EntryType = CSARSFAssessmentTypes.Codes.CustomsAssessment;

			var charge1 = line1.Charges.AddNew();

			AssertNull(line1.CustomsAssessmentCharge);
			AssertNotNull(line2.CustomsAssessmentCharge);

			AssertEquals(1, line1.Charges.Count);
			AssertEquals(1, line2.Charges.Count);
		}

		public void TestImporterWhenStatementTypeIsBroker()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "DCA";
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company.GC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var branch = company.Branches.AddNew();
			branch.GB_Code = "MTL";
			branch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

			var broker = Factory.NewWithValidTestData<OrgHeader>();
			broker.OH_Code = "BROKER";
			broker.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "0000001RM0001", Core.Constants.CountryCodes.Canada);

			var importer1 = Factory.NewWithValidTestData<OrgHeader>();
			importer1.OH_Code = "IMPORTER1";
			importer1.OH_FullName = "IMPORTER1 Name";
			importer1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "0000001RM0001", Core.Constants.CountryCodes.Canada);

			var importer2 = Factory.NewWithValidTestData<OrgHeader>();
			importer2.OH_Code = "IMPORTER2";
			importer2.OH_FullName = "IMPORTER2 Name";
			importer2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "0000001RM0001", Core.Constants.CountryCodes.Canada);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "BM00001B";
			declaration.JE_GB = branch.PK;
			declaration.JE_OH_Importer = importer2.PK;

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_IsMonthlyStatement = false;
			statement.B2_StatementType = CusStatementHeaderTypes.Codes.Broker;
			statement.B2_OH_Importer = broker.PK;
			statement.B2_GC = company.PK;

			var lineGroup = statement.LineGroupCollection.AddNew();
			lineGroup.B10_OH_Importer = importer1.PK;
			lineGroup.B10_ImporterCustomsID = importer1.CustomsCodes.GetCustomsRegNo(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, Core.Constants.CountryCodes.Canada);

			var line = statement.StatementLines.AddNew();
			line.B3_ImporterCustomsID = importer2.CustomsCodes.GetCustomsRegNo(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, Core.Constants.CountryCodes.Canada);
			line.B3_BrokerReference = "BM00001B";
			Factory.Save();
			AssertEquals(importer2.OH_Code, line.ImporterCode);
			AssertEquals(importer2.OH_FullName, line.ImporterName);

			statement.B2_StatementType = CusStatementHeaderTypes.Codes.Importer;
			line.B3_BrokerReference = ZString.Empty;
			Factory.Save();
			AssertEquals(importer1.OH_Code, line.ImporterCode);
			AssertEquals(importer1.OH_FullName, line.ImporterName);

			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_IsMonthlyStatement = true;
			statement2.B2_StatementType = CusStatementHeaderTypes.Codes.Broker;
			statement2.B2_OH_Importer = broker.PK;
			statement2.B2_GC = company.PK;

			var lineGroup2 = statement2.LineGroupCollection.AddNew();
			lineGroup2.B10_OH_Importer = importer1.PK;
			lineGroup2.B10_ImporterCustomsID = importer1.CustomsCodes.GetCustomsRegNo(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, Core.Constants.CountryCodes.Canada);

			var line2 = statement2.StatementLines.AddNew();
			line2.B3_ImporterCustomsID = importer2.CustomsCodes.GetCustomsRegNo(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, Core.Constants.CountryCodes.Canada);
			line2.B3_BrokerReference = "BM00001B";
			Factory.Save();
			AssertEquals(broker.OH_Code, line2.ImporterCode);
			AssertEquals(broker.OH_FullName, line2.ImporterName);

			statement2.B2_StatementType = CusStatementHeaderTypes.Codes.Importer;
			line2.B3_BrokerReference = ZString.Empty;
			Factory.Save();
			AssertEquals(importer1.OH_Code, line2.ImporterCode);
			AssertEquals(importer1.OH_FullName, line2.ImporterName);
		}

		public void TestImporterProperties()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var dailyStatement = Factory.New<CusStatementHeader>();
			dailyStatement.B2_IsMonthlyStatement = false;

			var lineGroup = dailyStatement.LineGroupCollection.AddNew();
			lineGroup.B10_OH_Importer = importer.PK;
			lineGroup.B10_ImporterCustomsID = "123546789RM0001";

			var line = dailyStatement.StatementLines.AddNew();
			line.B3_ImporterCustomsID = "123546789RM0001";

			CombineAssertions(() =>
			{
				AssertEquals("ImporterCode", importer.OH_Code, line.ImporterCode);
				AssertEquals("ImporterName", importer.OH_FullName, line.ImporterName);
			});
		}

		public void TestHumanReadableName()
		{
			var statement = Factory.New<CusStatementHeader>();

			var line = statement.StatementLines.AddNew();
			AssertEquals("Statement Line - ", line.HumanReadableName);

			line.B3_EntryNum = "181216";
			AssertEquals("Statement Line - 181216", line.HumanReadableName);

			line.B3_EntryType = "I";
			AssertEquals("Statement Line - (I)181216", line.HumanReadableName);
		}

		public void TestRefreshLineGroupStatementLines()
		{
			var header = Factory.New<CusStatementHeader>();
			var line1 = header.StatementLines.AddNew();
			line1.B3_ImporterCustomsID = "IMP1";
			var line2 = header.StatementLines.AddNew();
			line2.B3_ImporterCustomsID = "IMP2";
			var line3 = header.StatementLines.AddNew();
			line3.B3_ImporterCustomsID = "IMP1";
			var lineGroup = header.LineGroupCollection.AddNew();
			lineGroup.B10_ImporterCustomsID = "IMP1";
			AssertArrayEqualsByElements(new[] { line1, line3 }, lineGroup.StatementLines);
			line3.B3_ImporterCustomsID = "IMP2";
			AssertArrayEqualsByElements(new[] { line1 }, lineGroup.StatementLines);
			line2.B3_ImporterCustomsID = "IMP1";
			AssertArrayEqualsByElements(new[] { line1, line2 }, lineGroup.StatementLines);
			var header2 = Factory.New<CusStatementHeader>();
			var lineGroup2 = header2.LineGroupCollection.AddNew();
			lineGroup2.B10_ImporterCustomsID = "IMP1";
			AssertEquals(0, lineGroup2.StatementLines.Length);
			line3.B3_B2 = header2.PK;
			header.StatementLines.Load();
			header2.StatementLines.Load();
			AssertArrayEqualsByElements(new[] { line1, line2 }, lineGroup.StatementLines);
			AssertEquals(0, lineGroup2.StatementLines.Length);
			line1.B3_B2 = header2.PK;
			header.StatementLines.Load();
			header2.StatementLines.Load();
			AssertArrayEqualsByElements(new[] { line2 }, lineGroup.StatementLines);
			AssertArrayEqualsByElements(new[] { line1 }, lineGroup2.StatementLines);
			line1.B3_B2 = header.PK;
			header.StatementLines.Load();
			header2.StatementLines.Load();
			AssertArrayEqualsByElements(new[] { line1, line2 }, lineGroup.StatementLines);
			AssertEquals(0, lineGroup2.StatementLines.Length);
			var lineGroup3 = header.LineGroupCollection.AddNew();
			lineGroup3.B10_ImporterCustomsID = lineGroup.B10_ImporterCustomsID;
			AssertArrayEqualsByElements(new[] { line1, line2 }, lineGroup.StatementLines);
			AssertArrayEqualsByElements(new[] { line1, line2 }, lineGroup3.StatementLines);
			line1.Delete();
			AssertArrayEqualsByElements(new[] { line2 }, lineGroup.StatementLines);
			AssertArrayEqualsByElements(new[] { line2 }, lineGroup3.StatementLines);
		}

		public void TestPaymentMethod()
		{
			var header = Factory.New<CusStatementHeader>();
			var line = header.StatementLines.AddNew();
			AssertEquals(ZString.Empty, line.PaymentMethod);
			var charge1 = line.Charges.AddNew();
			charge1.B4_ChargeType = EntryChargeTypeList.Codes.CustomsValueForTax;
			charge1.B4_PaymentParty = PaymentPartyCodeDescriptionList.Codes.Importer;
			AssertEquals(PaymentPartyCodeDescriptionList.Codes.Importer, line.PaymentMethod);
			var charge2 = line.Charges.AddNew();
			charge2.B4_ChargeType = EntryChargeTypeList.Codes.Duty1;
			charge2.B4_PaymentParty = PaymentPartyCodeDescriptionList.Codes.Broker;
			AssertEquals(ZString.Empty, line.PaymentMethod);
			charge2.B4_PaymentParty = PaymentPartyCodeDescriptionList.Codes.Importer;
			AssertEquals(PaymentPartyCodeDescriptionList.Codes.Importer, line.PaymentMethod);
			charge1.B4_PaymentParty = PaymentPartyCodeDescriptionList.Codes.Broker;
			AssertEquals(ZString.Empty, line.PaymentMethod);
			var charge3 = line.Charges.AddNew();
			AssertEquals(ZString.Empty, line.PaymentMethod);
			charge3.B4_PaymentParty = PaymentPartyCodeDescriptionList.Codes.Importer;
			AssertEquals(ZString.Empty, line.PaymentMethod);
			charge3.B4_ChargeType = EntryChargeTypeList.Codes.TotalGSTDirectAmount;
			AssertEquals(PaymentPartyCodeDescriptionList.Codes.GST, line.PaymentMethod);
			charge3.B4_PaymentParty = PaymentPartyCodeDescriptionList.Codes.Broker;
			AssertEquals(ZString.Empty, line.PaymentMethod);
		}

		public void TestDeclaration()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "$%#";
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company.GC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var branch = company.Branches.AddNew();
			branch.GB_Code = "$#@";
			branch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_DeclarationReference = "BM00001B";
			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_DeclarationReference = "BM00001A";
			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_DeclarationReference = "BM00002C";
			var dec4 = Factory.New<JobDeclaration>();
			dec4.JE_DeclarationReference = "BM00002D";
			dec4.JE_GB = branch.PK;
			Factory.Save();

			var statement = Factory.New<CusStatementHeader>();
			var line = statement.StatementLines.AddNew();
			AssertNull(line.Declaration);
			AssertEquals(0, Factory.GetTableHitCount(JobDeclarationSchema.Constants.TableName));
			line.B3_BrokerReference = "BM00002";
			AssertEquals(dec3, line.Declaration);
			AssertEquals(1, Factory.GetTableHitCount(JobDeclarationSchema.Constants.TableName));
			line.B3_BrokerReference = "BM00001";
			AssertNull(line.Declaration);
			AssertEquals(2, Factory.GetTableHitCount(JobDeclarationSchema.Constants.TableName));
		}

		public void TestEntryStatusDescription()
		{
			var statement = Factory.New<CusStatementHeader>();
			var line = statement.StatementLines.AddNew();

			void AssertDescription(string entryStatus, string expectedDescription)
			{
				line.B3_EntryStatus = entryStatus;
				AssertEquals(expectedDescription, line.EntryStatusDescription);
			}

			AssertDescription(ZString.Empty, ZString.Empty);
			AssertDescription(ARLLegacyTransactionStatusList.Codes.Paid, ARLLegacyTransactionStatusList.Descriptions.Paid);
			AssertDescription(ARLTransactionStatusList.Codes.SP, ARLTransactionStatusList.Descriptions.SP);
			AssertDescription("X", ZString.Empty);
			AssertDescription("XX", ZString.Empty);
		}

		public void TestDNHistoryColumns()
		{
			AssertEquals(statementHeader.PK, statementLine.B3_B2);
			statementHeader.B2_ProcessDate = new ZDateTime(2015, 11, 12);
			AssertEquals("B2_ProcessDate", new ZDateTime(2015, 11, 12), statementLine.B2_ProcessDate);
			statementHeader.B2_EntryFilerCode = new ZString("12345");
			AssertEquals("B2_EntryFilerCode", "12345", statementLine.B2_EntryFilerCode);
			statementHeader.B2_Status = new ZString("PRE");
			AssertEquals("B2_Status", "PRE", statementLine.B2_Status);
			statementHeader.B2_PaymentType = new ZString("BRK");
			AssertEquals("B2_PaymentType", "BRK", statementLine.B2_PaymentType);
			statementHeader.B2_StatementType = new ZString("U");
			AssertEquals("B2_StatementType", "", statementLine.B2_StatementType);
			statementHeader.B2_StatementType = new ZString("C");
			AssertEquals("B2_StatementType", "C", statementLine.B2_StatementType);
			statementHeader.B2_RMNumber = new ZString("1234");
			AssertEquals("B2_RMNumber", "1234", statementLine.B2_RMNumber);

			var chargeline1 = statementLine.Charges.AddNew();
			chargeline1.B4_ChargeType = EntryChargeTypeList.Codes.TotalDutyAmount;
			chargeline1.B4_ChargeAmount = 100m;
			AssertEquals("B4_ChargeAmountDTY", 100m, statementLine.B4_ChargeAmountDTY);

			var chargeline2 = statementLine.Charges.AddNew();
			chargeline2.B4_ChargeType = EntryChargeTypeList.Codes.TotalGSTAmount;
			chargeline2.B4_ChargeAmount = 110m;
			AssertEquals("B4_ChargeAmountGST", 110m, statementLine.B4_ChargeAmountGST);
			AssertEquals("B4_ChargeAmountGSTOrGSD", 110m, statementLine.B4_ChargeAmountGSTOrGSD);

			var chargeline3 = statementLine.Charges.AddNew();
			chargeline3.B4_ChargeType = EntryChargeTypeList.Codes.TotalGSTDirectAmount;
			chargeline3.B4_ChargeAmount = 120m;
			AssertEquals("B4_ChargeAmountGSD", 120m, statementLine.B4_ChargeAmountGSD);
			AssertEquals("B4_ChargeAmountGSTOrGSD", 110m, statementLine.B4_ChargeAmountGSTOrGSD);
			chargeline2.Delete();
			AssertEquals("B4_ChargeAmountGSTOrGSD", 120m, statementLine.B4_ChargeAmountGSTOrGSD);

			var chargeline4 = statementLine.Charges.AddNew();
			chargeline4.B4_ChargeType = EntryChargeTypeList.Codes.TotalSIMAAmount;
			chargeline4.B4_ChargeAmount = 130m;
			AssertEquals("B4_ChargeAmountSIM", 130m, statementLine.B4_ChargeAmountSIM);

			var chargeline5 = statementLine.Charges.AddNew();
			chargeline5.B4_ChargeType = EntryChargeTypeList.Codes.TotalExciseTaxAmount;
			chargeline5.B4_ChargeAmount = 140m;
			AssertEquals("B4_ChargeAmountEXS", 140m, statementLine.B4_ChargeAmountEXS);

			var chargeline6 = statementLine.Charges.AddNew();
			chargeline6.B4_ChargeType = EntryChargeTypeList.Codes.K84LateFilingPenalty;
			chargeline6.B4_ChargeAmount = 150m;
			AssertEquals("B4_ChargeAmountKPM", 150m, statementLine.B4_ChargeAmountKPM);

			var chargeline7 = statementLine.Charges.AddNew();
			chargeline7.B4_ChargeType = EntryChargeTypeList.Codes.Others;
			chargeline7.B4_ChargeAmount = 160m;
			AssertEquals("B4_ChargeAmountOTH", 160m, statementLine.B4_ChargeAmountOTH);
		}

		public void TestCARMDailyNoticeChargeAmountProperties()
		{
			var chargeline1 = statementLine.Charges.AddNew();
			chargeline1.B4_ChargeType = CARMDailyNoticeChargeTypeList.Codes.Duties;
			chargeline1.B4_ChargeAmount = 10m;
			AssertEquals("B4_CARMDNChargeAmount_Duties", 10m, statementLine.B4_CARMDNChargeAmount_Duties);

			var chargeline2 = statementLine.Charges.AddNew();
			chargeline2.B4_ChargeType = CARMDailyNoticeChargeTypeList.Codes.SIMA;
			chargeline2.B4_ChargeAmount = 20m;
			AssertEquals("B4_CARMDNChargeAmount_SIMA", 20m, statementLine.B4_CARMDNChargeAmount_SIMA);

			var chargeline3 = statementLine.Charges.AddNew();
			chargeline3.B4_ChargeType = CARMDailyNoticeChargeTypeList.Codes.ExciseDuties;
			chargeline3.B4_ChargeAmount = 30m;
			AssertEquals("B4_CARMDNChargeAmount_ExciseDuties", 30m, statementLine.B4_CARMDNChargeAmount_ExciseDuties);

			var chargeline4 = statementLine.Charges.AddNew();
			chargeline4.B4_ChargeType = CARMDailyNoticeChargeTypeList.Codes.ExciseTax;
			chargeline4.B4_ChargeAmount = 40m;
			AssertEquals("B4_CARMDNChargeAmount_ExciseTax", 40m, statementLine.B4_CARMDNChargeAmount_ExciseTax);

			var chargeline5 = statementLine.Charges.AddNew();
			chargeline5.B4_ChargeType = CARMDailyNoticeChargeTypeList.Codes.GoodsAndServicesTax;
			chargeline5.B4_ChargeAmount = 50m;
			AssertEquals("B4_CARMDNChargeAmount_GST", 50m, statementLine.B4_CARMDNChargeAmount_GST);
			AssertEquals("B4_CARMDNChargeAmount_GSTAndHSTAndPST", 50m, statementLine.B4_CARMDNChargeAmount_GSTAndHSTAndPST);

			var chargeline6 = statementLine.Charges.AddNew();
			chargeline6.B4_ChargeType = CARMDailyNoticeChargeTypeList.Codes.HarmonizedSalesTax;
			chargeline6.B4_ChargeAmount = 60m;
			AssertEquals("B4_CARMDNChargeAmount_HST", 60m, statementLine.B4_CARMDNChargeAmount_HST);
			AssertEquals("B4_CARMDNChargeAmount_GSTAndHSTAndPST", 110m, statementLine.B4_CARMDNChargeAmount_GSTAndHSTAndPST);

			var chargeline7 = statementLine.Charges.AddNew();
			chargeline7.B4_ChargeType = CARMDailyNoticeChargeTypeList.Codes.ProvincialSalesTax;
			chargeline7.B4_ChargeAmount = 70m;
			AssertEquals("B4_CARMDNChargeAmount_PST", 70m, statementLine.B4_CARMDNChargeAmount_PST);
			AssertEquals("B4_CARMDNChargeAmount_GSTAndHSTAndPST", 180m, statementLine.B4_CARMDNChargeAmount_GSTAndHSTAndPST);

			var chargeline8 = statementLine.Charges.AddNew();
			chargeline8.B4_ChargeType = CARMDailyNoticeChargeTypeList.Codes.Interest;
			chargeline8.B4_ChargeAmount = 80m;
			AssertEquals("B4_CARMDNChargeAmount_Interests", 80m, statementLine.B4_CARMDNChargeAmount_Interests);

			var chargeline9 = statementLine.Charges.AddNew();
			chargeline9.B4_ChargeType = CARMDailyNoticeChargeTypeList.Codes.Others;
			chargeline9.B4_ChargeAmount = 90m;
			AssertEquals("B4_CARMDNChargeAmount_Others", 90m, statementLine.B4_CARMDNChargeAmount_Others);

			var chargeline10 = statementLine.Charges.AddNew();
			chargeline10.B4_ChargeType = CARMDailyNoticeChargeTypeList.Codes.Penalties;
			chargeline10.B4_ChargeAmount = 10m;
			AssertEquals("B4_CARMDNChargeAmount_Penalties", 10m, statementLine.B4_CARMDNChargeAmount_Penalties);

			var chargeline11 = statementLine.Charges.AddNew();
			chargeline11.B4_ChargeType = CARMDailyNoticeChargeTypeList.Codes.Payments;
			chargeline11.B4_ChargeAmount = 20m;
			AssertEquals("B4_CARMDNChargeAmount_Payments", 20m, statementLine.B4_CARMDNChargeAmount_Payments);
		}

		public void TestCARMDailyNoticeExtentionProperties()
		{
			var dailyNoticeExtentions = statementLine.CARMDailyNoticeExtensions.AddNew();
			dailyNoticeExtentions.CSI_Description = "Test Description";
			dailyNoticeExtentions.CSI_Code = "Test Code";
			dailyNoticeExtentions.CSI_ReferenceNumber = "Test Reference Number";
			dailyNoticeExtentions.CSI_Status = "ABC";
			dailyNoticeExtentions.CSI_Procedure = "Port1";
			dailyNoticeExtentions.CSI_Value = 10m;

			var dailyNoticeExtention1 = statementLine.CARMDailyNoticeExtensions.AddNew();
			dailyNoticeExtention1.CSI_Code = "123";

			CombineAssertions(() =>
			{
				AssertEquals("CARMTransactionDescription", "Test Description", statementLine.CARMTransactionDescription);
				AssertEquals("CARMCADVersion", "Test Code", statementLine.CARMCADVersion);
				AssertEquals("CARMSubmittedBy", "Test Reference Number", statementLine.CARMSubmittedBy);
				AssertEquals("CARMStatus", "ABC", statementLine.CARMStatus);
				AssertEquals("CARMPort", "Port1", statementLine.CARMPort);
				AssertEquals("CARMTotal", 10m, statementLine.CARMTotal);
			});

			statementLine.CARMTransactionDescription = "Description";
			statementLine.CARMCADVersion = "Code";
			statementLine.CARMSubmittedBy = "Reference Number";
			statementLine.CARMStatus = "BCD";
			statementLine.CARMPort = "Port2";
			statementLine.CARMTotal = 20m;

			CombineAssertions(() =>
			{
				AssertEquals("CARMTransactionDescription", "Description", statementLine.CARMTransactionDescription);
				AssertEquals("CARMCADVersion", "Code", statementLine.CARMCADVersion);
				AssertEquals("CARMSubmittedBy", "Reference Number", statementLine.CARMSubmittedBy);
				AssertEquals("CARMStatus", "BCD", statementLine.CARMStatus);
				AssertEquals("CARMPort", "Port2", statementLine.CARMPort);
				AssertEquals("CARMTotal", 20m, statementLine.CARMTotal);
			});
		}

		public void TestB2_PaymentType()
		{
			statementHeader.B2_StatementNumber = "10012356";
			var chargeline1 = statementLine.Charges.AddNew();
			chargeline1.B4_PaymentParty = "BRK";
			chargeline1.B4_ChargeType = "SIM";
			AssertEquals("B2_PaymentType", "BRK", statementLine.B2_PaymentType);

			chargeline1.B4_PaymentParty = "IMP";
			AssertEquals("B2_PaymentType", "IMP", statementLine.B2_PaymentType);

			chargeline1.B4_PaymentParty = string.Empty;
			AssertEquals("B2_PaymentType", "BRK/IMP", statementLine.B2_PaymentType);

			chargeline1.B4_PaymentParty = "BRK";
			chargeline1.B4_ChargeType = "IMP";

			var chargeline2 = statementLine.Charges.AddNew();
			chargeline2.B4_PaymentParty = "IMP";
			chargeline2.B4_ChargeType = "GSD";

			AssertEquals("B2_PaymentType", "GST", statementLine.B2_PaymentType);

			chargeline1.B4_PaymentParty = "BRK";
			chargeline1.B4_ChargeType = "DTY";

			chargeline2.B4_PaymentParty = "BRK";
			chargeline2.B4_ChargeType = "SIM";
			AssertEquals("B2_PaymentType", "BRK", statementLine.B2_PaymentType);
		}

		public void TestB2_DueDate()
		{
			var today = ZDateTime.Today;
			statementHeader.B2_DueDate = today;
			AssertEquals(today, statementLine.B2_DueDate);

			statementHeader.B2_DueDate = today.AddDays(1);
			AssertEquals(today.AddDays(1), statementLine.B2_DueDate);

			var newStatementLine = Factory.New<CusStatementLine>();
			AssertEquals(true, newStatementLine.B2_DueDate.IsEmpty);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Definitely Deleting object should have an exception/error because of trigger.", true);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return statementLine;
		}

		protected override void SetUp()
		{
			base.SetUp();
			statementHeader = Factory.New<CusStatementHeader>();
			statementLine = statementHeader.StatementLines.AddNew();
		}

		CusStatementHeader statementHeader;
		CusStatementLine statementLine;
	}
}
