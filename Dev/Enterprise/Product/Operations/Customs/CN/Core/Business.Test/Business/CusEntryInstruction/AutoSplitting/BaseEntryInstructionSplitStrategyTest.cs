using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using StrategyProvider = Enterprise.Customs.CN.Business.EntryInstructionAutoSplitStrategyProvider;

namespace Enterprise.Customs.CN.Business.Testing
{
	class BaseEntryInstructionSplitStrategyTest : TestCaseWithFactory
	{
		public void TestSplitEntryInstruction()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.China, "HSN");
			Factory.Save();
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportCUSRequirement, "A", tariff1);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Style = "0110";
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			instruction2.CEI_Style = "0130";
			var invoiceHeader = declaration.Invoices.AddNew();
			for (var line = 0; line < 51; line++)
			{
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction1.PK;
				invoiceLine.JI_Tariff = "10010" + line.ToString().PadLeft(2, '0');
			}

			declaration.DoMerge();
			StrategyProvider.GetStrategy(instruction1, StrategyProvider.SplitBy.LineCount50).AutoSplit();
			AssertEquals(4, declaration.CustomsEntryInstructions.Count);
			var newInstruction1 = declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().FirstOrDefault(ins => ins.PK != instruction1.PK && ins.CEI_Style == "0110");
			var newInstruction2 = declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().FirstOrDefault(ins => ins.PK != instruction2.PK && ins.CEI_Style == "0130");
			AssertNotNull(newInstruction1);
			AssertNotNull(newInstruction2);
			AssertEquals(instruction1.PK, instruction2.CEI_CEI_Parent);
			AssertEquals(newInstruction1.PK, newInstruction2.CEI_CEI_Parent);
			AssertEquals(1, declaration.InvoiceLines.Cast<JobComInvoiceLine>().Count(line => line.JI_CEI == newInstruction1.PK));
			AssertEquals("-2", newInstruction1.CEI_Description);
			AssertEquals("-2", newInstruction2.CEI_Description);
			AssertEquals(50, declaration.InvoiceLines.Cast<JobComInvoiceLine>().Count(line => line.JI_CEI == instruction1.PK));
			AssertEquals("-1", instruction1.CEI_Description);
			AssertEquals("-1", instruction2.CEI_Description);
			var longDescription = "QuiteLongDescriptionQuiteLongDescriptionQuiteLongD";
			var instruction3 = declaration.CustomsEntryInstructions.AddNew();
			instruction3.CEI_Style = "0130";
			instruction3.CEI_Description = longDescription;
			var invoiceHeader2 = declaration.Invoices.AddNew();
			for (var line = 0; line < 51; line++)
			{
				var invoiceLine = invoiceHeader2.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction3.PK;
				invoiceLine.JI_Tariff = "10010" + line.ToString().PadLeft(2, '0');
			}

			declaration.DoMerge();
			StrategyProvider.GetStrategy(instruction3, StrategyProvider.SplitBy.LineCount50).AutoSplit();
			AssertEquals(6, declaration.CustomsEntryInstructions.Count);
			AssertEquals(2, declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().Count(ins => ins.CEI_Description == longDescription));
			var instruction4 = declaration.CustomsEntryInstructions.AddNew();
			instruction4.CEI_Style = "0214";
			instruction4.CEI_Description = "INS";
			var ins4inv1 = invoiceHeader2.JobComInvoiceLines.AddNew();
			ins4inv1.JI_CEI = instruction4.PK;
			ins4inv1.JI_Tariff = "2713200001";
			var ins4inv2 = invoiceHeader2.JobComInvoiceLines.AddNew();
			ins4inv2.JI_CEI = instruction4.PK;
			ins4inv2.JI_Tariff = "1003000";
			declaration.DoMerge();
			StrategyProvider.GetStrategy(instruction4, StrategyProvider.SplitBy.LegalInspection).AutoSplit();
			var legalInspectionInstructions = declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().Where(ins => ins.CEI_Style == "0214");
			AssertEquals("Split by RequiresLegalInspection", 2, legalInspectionInstructions.Count());
			Assert("LIR", legalInspectionInstructions.Any(ins => ins.CEI_Description == "INS-LIR" && ins4inv1.JI_CEI == ins.PK && ins.CEI_CIQRequires));
			Assert("LIN", legalInspectionInstructions.Any(ins => ins.CEI_Description == "INS-LIN" && ins4inv2.JI_CEI == ins.PK && !ins.CEI_CIQRequires));
		}

		public void TestCloneEntryInstruction()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var invoiceHeader = declaration.Invoices.AddNew();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Style = "0110";
			instruction1.BillOfLading = "BL001";
			instruction1.BillOfLadingDate = new ZDateTime(2019, 7, 29);
			var operationMatter1 = Factory.New<OperationMatter>();
			operationMatter1.CY_ParentTableCode = instruction1.TablePrefix;
			operationMatter1.CY_Code = OperationMatterList.Codes.AssuredInspectClearance;
			operationMatter1.CY_Type = Constants.CusCodeDataTypes.Codes.OperationMatter;
			operationMatter1.CY_ParentID = instruction1.PK;
			var operationMatter2 = Factory.New<OperationMatter>();
			operationMatter2.CY_ParentTableCode = instruction1.TablePrefix;
			operationMatter2.CY_Code = OperationMatterList.Codes.ConsolidatedDutyCollection;
			operationMatter2.CY_Type = Constants.CusCodeDataTypes.Codes.OperationMatter;
			operationMatter2.CY_ParentID = instruction1.PK;
			var otherPackage1 = Factory.New<OtherPackage>();
			otherPackage1.CY_ParentTableCode = instruction1.TablePrefix;
			otherPackage1.CY_Code = "00";
			otherPackage1.CY_Type = Constants.CusCodeDataTypes.Codes.Package;
			otherPackage1.CY_ParentID = instruction1.PK;
			var otherPackage2 = Factory.New<OtherPackage>();
			otherPackage2.CY_ParentTableCode = instruction1.TablePrefix;
			otherPackage2.CY_Code = "01";
			otherPackage2.CY_Type = Constants.CusCodeDataTypes.Codes.Package;
			otherPackage2.CY_ParentID = instruction1.PK;
			instruction1.CustomsMessageRemarks = "Remarks 1";
			var special1 = Factory.New<SpecialBusinessIdentifier>();
			special1.CY_ParentTableCode = instruction1.TablePrefix;
			special1.CY_Code = "B01";
			special1.CY_Type = "SBI";
			special1.CY_ParentID = instruction1.PK;
			var special2 = Factory.New<SpecialBusinessIdentifier>();
			special2.CY_ParentTableCode = instruction1.TablePrefix;
			special2.CY_Code = "B04";
			special2.CY_Type = "SBI";
			special2.CY_ParentID = instruction1.PK;
			var cusCodeData1 = instruction1.EnterpriseQualifications.AddNew();
			cusCodeData1.CY_ParentID = instruction1.PK;
			cusCodeData1.CY_ParentTableCode = "CEI";
			cusCodeData1.CY_Data = "100";
			var cusCodeData2 = instruction1.EnterpriseQualifications.AddNew();
			cusCodeData2.CY_ParentID = instruction1.PK;
			cusCodeData2.CY_ParentTableCode = "CEI";
			cusCodeData2.CY_Data = "101";
			var ciq1 = instruction1.CIQRequiredDocuments.AddNew();
			ciq1.XC_DocumentType = "11";
			ciq1.XC_NumberOfCopies = 2;
			ciq1.XC_NumberOfOriginals = 3;
			var ciq2 = instruction1.CIQRequiredDocuments.AddNew();
			ciq2.XC_DocumentType = "12";
			ciq2.XC_NumberOfCopies = 4;
			ciq2.XC_NumberOfOriginals = 5;
			for (var line = 0; line < 21; line++)
			{
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction1.PK;
				invoiceLine.JI_Tariff = "10010" + line.ToString().PadLeft(2, '0');
			}

			declaration.DoMerge();
			Factory.Save();
			StrategyProvider.GetStrategy(instruction1, StrategyProvider.SplitBy.LineCount20).AutoSplit();
			var cloneResult = declaration.CustomsEntryInstructions.FirstOrDefault<CusEntryInstruction>(ins => ins != instruction1);
			Assert("No. of packages should have been cleared.", instruction1.CEI_Packages.IsEmpty && cloneResult.CEI_Packages.IsEmpty);
			AssertEquals("B/L No.", "BL001", cloneResult.BillOfLading);
			AssertEquals("B/L Date", new ZDateTime(2019, 7, 29), cloneResult.BillOfLadingDate);
			AssertEquals("OperationMatters", "担保验放,汇总征税", cloneResult.OperationMattersAsString);
			AssertEquals("OtherPackages", "散装,裸装", cloneResult.OtherPackagesAsString);
			AssertEquals("CustomsMessageRemarks", "Remarks 1", cloneResult.CustomsMessageRemarks);
			AssertEquals("SpecialBusinessIdentifiers", "国际赛事,国际会议", cloneResult.SpecialBusinessIdentifiersAsString);
			AssertEquals("EnterpriseQualifications", 2, cloneResult.EnterpriseQualifications.Count);
			Assert("EnterpriseQualifications", cloneResult.EnterpriseQualifications.OfType<EnterpriseQualification>().Any(x => x.CY_Data == "100"));
			Assert("EnterpriseQualifications", cloneResult.EnterpriseQualifications.OfType<EnterpriseQualification>().Any(x => x.CY_Data == "101"));
			AssertEquals("CIQRequiredDocuments", 2, cloneResult.CIQRequiredDocuments.Count);
			Assert("CIQRequiredDocuments", cloneResult.CIQRequiredDocuments.OfType<CIQRequiredDocument>().Any(x => x.XC_DocumentType == "11" && x.XC_NumberOfCopies == 2 && x.XC_NumberOfOriginals == 3));
			Assert("CIQRequiredDocuments", cloneResult.CIQRequiredDocuments.OfType<CIQRequiredDocument>().Any(x => x.XC_DocumentType == "12" && x.XC_NumberOfCopies == 4 && x.XC_NumberOfOriginals == 5));
		}
	}
}
