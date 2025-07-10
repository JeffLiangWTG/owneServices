using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.MasterFiles;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	sealed class LineMergerTest : TestCaseWithFactory
	{
		public void TestByPassCodeNeverUpdates()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);

			var testTradeGroup1 = helper.CreateTradeGroup(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "STANDARD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.SouthAfrica, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));

			var rateType1 = helper.CreateCusRateType(Core.Constants.CountryCodes.France, "AMC");
			var rateCode1 = helper.CreateCusRateCode(Factory, "111", rateType1.PK);

			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.France, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.France, tariffType.PK, "10000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var rate1 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula: UniversalReferenceConstants.RefCusRateFormula.Precalcule);
			var testApplicability1 = helper.CreateCusApplicability(rate1, testTradeGroup1, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			Factory.Save();

			var (declaration, invoiceline11, invoiceline21, invoiceline22) = PrepareDeclarationForUpdateVATDeferType();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceline11.JI_Tariff = "10000001";
			invoiceline11.JI_CountryOfOrigin = Enterprise.Core.Constants.CountryCodes.SouthAfrica;

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals(ZString.Empty, invoiceline11.EntryInstruction.ZG_BypassCode);
		}

		public void TestPopulateGroupCharges()
		{
			var link = Factory.NewWithValidTestData<OrgSupplierBuyerLink>();
			link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.Australia;
			link.OL_InsuranceUplift = 10m;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = link.OL_OH_Supplier;
			declaration.JE_OH_Importer = link.OL_OH_Buyer;
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.JE_RL_NKFinalDestination = Core.Constants.CountryCodes.Australia;
			var groupInvoiceHeader = declaration.TopGroupInvoice;

			var invoice1 = groupInvoiceHeader.AllJobComInvoiceHeaders.AddNew() as JobComInvoiceHeader;
			invoice1.JZ_InvoiceAmount = 100m;
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			instruction.CEI_Style = "21P";

			var invoiceLine = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			new LineMerger(declaration).DoMerge();
			AssertEquals(1, groupInvoiceHeader.Charges.Count);
		}

		public void TestInvoiceAmountSetOnMerge()
		{
			TestJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var header1 = TestJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_OH_Supplier = GetValidSupplier();
			header1.JZ_IncoTerm = "FOB";
			header1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			var line1 = header1.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header1);
			line1.JI_LinePrice = 10.0m;

			var header2 = TestJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header2.JZ_OH_Supplier = GetValidSupplier();
			header2.JZ_IncoTerm = "FOB";
			header2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			var line2 = header2.JobComInvoiceLines.AddNew();
			var line3 = header2.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header2);
			line2.JI_LinePrice = 100.0m;
			line3.JI_LinePrice = 1000.0m;

			var merger = (LineMerger)GetNewLineMerger(TestJobDeclaration);
			merger.DoMerge();

			CombineAssertions(() =>
			{
				AssertEquals("One cus entry header", 1, TestJobDeclaration.CustomsEntryHeaders.Count);
				AssertContainsExactElementsInAnyOrder("3 entrylines",
					new[] { "10.0, AUD", "100.0, NZD", "1000.0, NZD" },
					TestJobDeclaration.CustomsEntryHeaders[0].MergedLines.Select(x => $"{x.CL_InvoiceAmount}, {x.CL_RX_NKInvoiceAmountCurrency}"));
			});
		}

		public void TestOnMerging()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var orgCusAccount = Factory.New<OrgCusAccount>();
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGI;
			orgCusAccount.CZ_Account = "TESTACC";
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount.CZ_OH = importer.PK;
			orgCusAccount.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G2;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_CustomsProfile = "TESTACC";
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;

			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			instruction.CEI_Style = "21P";

			var authorisationJeader = Factory.New<CusAuthorisationHeader>();
			authorisationJeader.CPH_Number = "12345678";
			authorisationJeader.CPH_IsSingleUse = true;
			authorisationJeader.CPH_Type = "OPO";
			authorisationJeader.CPH_OH_PermitHolder = importer.PK;
			authorisationJeader.CPH_RN_NKCountryCode = "FR";

			var usage = instruction.CusAuthorizationUsages.AddNew();
			usage.AGC_Code = "OPO";
			usage.AGC_OH_Owner = importer.PK;
			usage.AGC_Number = "12345678";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			new LineMerger(declaration).DoMerge();
			var specialMentions = invoiceLine.AdditionalInfos;
			AssertEquals("specialMentions should have 1 record", 1, specialMentions.Count);
			AssertEquals("specialMentions should have 1 record", LineMerger.SimplifiedAuthorizationCode, specialMentions[0].CSI_Code);
			authorisationJeader.CPH_IsSingleUse = false;
			new LineMerger(declaration).DoMerge();
			AssertEquals("specialMentions should have 0 record", 0, specialMentions.Count);
		}

		public void TestLineMergerCreateOneEntryHeader()
		{
			TestJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			BaseJobComInvoiceHeader header1 = TestJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_OH_Supplier = GetValidSupplier();
			header1.JZ_IncoTerm = "FOB";
			header1.JZ_RX_NKInvoice_Currency = "AUD";
			header1.JobComInvoiceLines.AddNew();
			header1.JobComInvoiceLines.AddNew();
			header1.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header1);

			BaseJobComInvoiceHeader header2 = TestJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header2.JZ_OH_Supplier = GetValidSupplier();
			header2.JZ_IncoTerm = "FOB";
			header2.JZ_RX_NKInvoice_Currency = "NZD";
			header2.JobComInvoiceLines.AddNew();
			header2.JobComInvoiceLines.AddNew();
			header2.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header2);

			LineMerger merger = (LineMerger)GetNewLineMerger(TestJobDeclaration);
			merger.DoMerge();
			AssertEquals("One cus entry header", 1, TestJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("Six cus entry lines", 6, TestJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestCalculateDuties()
		{
			TestJobDeclaration.JE_MergeBy = Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;

			var header = TestJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header.JZ_OH_Supplier = GetValidSupplier();
			header.JZ_IncoTerm = "FOB";
			header.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			var invLine1 = header.JobComInvoiceLines.AddNew();
			invLine1.JI_LinePrice = 107;
			invLine1.JI_Tariff = "1010";
			var invLine2 = header.JobComInvoiceLines.AddNew();
			invLine2.JI_LinePrice = 207;
			invLine2.JI_Tariff = "1010";
			var invLine3 = header.JobComInvoiceLines.AddNew();
			invLine3.JI_LinePrice = 307;
			invLine3.JI_Tariff = "1010";

			var merger = GetNewLineMerger(TestJobDeclaration);
			merger.DoMerge();

			AssertEquals("Entry Lines count", 1, TestJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("Entry Line Assiette TVA", 621m, TestJobDeclaration.CustomsEntryHeaders[0].MergedLines[0].CL_ValueForVAT);
		}

		(JobDeclaration, JobComInvoiceLine, JobComInvoiceLine, JobComInvoiceLine) PrepareDeclarationForUpdateVATDeferType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MergeBy = Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;
			declaration.ZG_VATDeferType = "2";

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "VATFR345", declaration.CountryCode);
			var frOrgImpAddInfo = FROrgImpAddInfo.Get(importer);
			frOrgImpAddInfo.ZO_VATDeferType = VATProcedureList.Codes._2;
			frOrgImpAddInfo.ZO_VATProcedureDateLimit = new ZDateTime(2019, 01, 01);
			declaration.JE_OH_Importer = importer.PK;

			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();

			var invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "INV1";
			invoiceHeader1.JZ_ValuationDateOverride = ZDateTime.Today;
			var invoiceline11 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceline11.JI_LinePrice = 107;
			invoiceline11.JI_Tariff = "1010";
			invoiceline11.JI_CEI = entryInstruction1.PK;

			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_InvoiceNumber = "INV2";
			invoiceHeader2.JZ_ValuationDateOverride = ZDateTime.Today.AddDays(1);
			var invoiceline21 = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoiceline21.JI_LinePrice = 108;
			invoiceline21.JI_Tariff = "1011";
			invoiceline21.JI_CEI = entryInstruction2.PK;

			var invoiceline22 = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoiceline22.JI_LinePrice = 109;
			invoiceline22.JI_Tariff = "1012";
			invoiceline22.JI_CEI = entryInstruction2.PK;

			return (declaration, invoiceline11, invoiceline21, invoiceline22);
		}

		public void TestUpdateVATDeferTypeForIsAllEntryLinesVatSuspended()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "", "Ye", "s", "   ", "", "EXP", "");
			procedure.ZZ6_CalculateVAT = false;
			Factory.Save();

			var (declaration, invoiceline11, invoiceline21, invoiceline22) = PrepareDeclarationForUpdateVATDeferType();
			invoiceline11.JI_Procedure = "Yes";
			invoiceline21.JI_Procedure = "Yes";
			invoiceline22.JI_Procedure = "";

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			Assert(declaration.ActiveEntryHeaders[0].IsAllEntryLinesVatSuspended);
			Assert(!declaration.ActiveEntryHeaders[1].IsAllEntryLinesVatSuspended);
			AssertEquals("2", declaration.ZG_VATDeferType);

			invoiceline22.JI_Procedure = "Yes";
			merger.DoMerge();
			AssertEquals("", declaration.ZG_VATDeferType);
		}

		public void TestDutyCalculatorStrategyType()
		{
			var merger = GetNewLineMerger(TestJobDeclaration);
			var getNewDutyCalculatorStrategyMethod = merger.GetType().GetMethod("GetNewDutyCalculatorStrategy", BindingFlags.Instance | BindingFlags.NonPublic);
			var strategy = getNewDutyCalculatorStrategyMethod.Invoke(merger, System.Array.Empty<object>());
			AssertEquals("DutyCalculatorStrategyType", typeof(DutyCalculatorStrategy), strategy.GetType());
		}

		public void TestGetEntryCreationStrategies()
		{
			var merger = GetNewLineMerger(TestJobDeclaration);
			var getEntryCreationStrategiesMethod = merger.GetType().GetMethod("GetEntryCreationStrategies", BindingFlags.Instance | BindingFlags.NonPublic);
			var strategies = getEntryCreationStrategiesMethod.Invoke(merger, System.Array.Empty<object>()) as Customs.Business.EntryCreationStrategy[];
			AssertContainsExactElementsInExactOrder("EntryCreationStrategiesType", new[] { typeof(EntryCreationStrategy) }, strategies.Select(x => x.GetType()));
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestJobDeclaration = Factory.New<JobDeclaration>();
			TestJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			classification = Factory.New<BaseCusClassification>();
			classification.CC_LookupCode = "TEST";
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			classification.CC_TariffNum = "00000000";
			classification.CC_Description = "TESTDESCRIPTION";
			classification.CC_RN_NKCountryCode = TestJobDeclaration.CountryCode;

			part = Enterprise.MasterFiles.Business.OrgSupplierPart.New(Factory);
			part.OP_PartNum = "PartNum";

			BaseCusClassPartPivot partClassPivot = Factory.New<BaseCusClassPartPivot>();
			partClassPivot.CI_OP = part.PK;
			partClassPivot.CI_CC = classification.PK;
			Factory.Save();
		}

		Customs.Business.LineMerger GetNewLineMerger(BaseJobDeclaration declaration) => new LineMerger((JobDeclaration)declaration);

		ZGuid GetValidSupplier()
		{
			var result = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, true));
			return result.PK;
		}

		void SetDefaultValuesToInvoiceLines(BaseJobComInvoiceHeader header)
		{
			foreach (BaseJobComInvoiceLine line in header.JobComInvoiceLines)
			{
				line.JI_PartNo = part.OP_PartNum;
				line.JI_CustomsQuantity = 1.0m;
				line.JI_LinePrice = 100.0m;
			}
		}

		JobDeclaration TestJobDeclaration { get; set; }
		BaseCusClassification classification;
		Enterprise.MasterFiles.Business.OrgSupplierPart part;
	}
}
