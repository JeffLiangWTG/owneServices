using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using CustomsChargeTypeList = Enterprise.Customs.Business.CustomsChargeTypeList;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryHeader))]
	sealed class CusEntryHeaderTest : EU.Business.Declaration.Testing.CusEntryHeaderTest<CusEntryHeader>
	{
		public void TestUpdateHightestLineNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			CombineAssertions(() =>
			{
				entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.AwaitingChange;
				AssertEquals("LockNumberOfEntryLines false", (ZShort)0, entryHeader.CH_HighestLineNumber);

				entryHeader.EntryNumber = "1234";
				entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.Sent;
				AssertEquals((ZShort)1, entryHeader.CH_HighestLineNumber);
			});
		}

		public void TestDutyAndTax()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			var entryLine2 = entryHeader.MergedLines.AddNew();
			var entryLine3 = entryHeader.MergedLines.AddNew();

			var fee1A00 = AddNewFee(entryLine1, "A00", "A", 124.9270m);
			var fee1B00 = AddNewFee(entryLine1, "B00", "A", 547.8270m);
			var fee2A00 = AddNewFee(entryLine2, "A00", "A", 129.1821m);
			var fee2B00 = AddNewFee(entryLine2, "B00", "A", 566.5021m);
			var fee3A00 = AddNewFee(entryLine3, "A00", "A", 87.6255m);
			var fee3B00 = AddNewFee(entryLine3, "B00", "A", 384.2655m);

			CombineAssertions(() =>
			{
				AssertEquals("CustomsEntryHeader.Duty", 341.74m, entryHeader.Duty);
				AssertEquals("CustomsEntryHeader.VAT", 1498.60m, entryHeader.VAT);
				AssertEquals("CustomsEntryHeader.DutyImmediate", 341.74m, entryHeader.DutyImmediate);
				AssertEquals("CustomsEntryHeader.VATImmediate", 1498.60m, entryHeader.VATImmediate);
			});
		}

		CusEntryLineFee AddNewFee(CusEntryLine entryLine, string tty, string mop, decimal amount)
		{
			var fee = entryLine.Fees.AddNew();
			fee.CF_MethodOfPayment = mop;
			fee.CF_ChargeType = tty;
			fee.CF_ChargeAmount = amount;
			return (CusEntryLineFee)fee;
		}

		public void TestLineTriggerWorkflowReportIntegration() => CombineAssertions(() =>
		{
			var refDocType = Factory.New<RefDocType>();
			refDocType.RT_ReferenceType = "SCL";
			refDocType.RT_DocType = "TTT";
			refDocType.RT_Desc = "Entry Print/ Customs Declaration Documents";
			refDocType.RT_AllowMultiplePeriodicDocs = false;

			var stmMenuItem = Factory.New<StmMenuItem>();
			stmMenuItem.SU_MenuName = "Any Doc";
			stmMenuItem.SU_BusinessContext = "CusEntryHeader";
			stmMenuItem.SU_ContactType = ContactType.Consignee.Code;
			stmMenuItem.SU_MenuType = "DOC";
			stmMenuItem.SU_PreventAutoDelivery = ZBool.False;
			stmMenuItem.SU_FilterList = ""; // make sure that DocumentCommand.IsApplicable returns true

			var template1 = Enterprise.DocumentEngine.Testing.DocumentEngineTestHelper.CreateTemplateFromString(
				@"{A}-[#Config]
{A}-[DataContext=CusEntryHeader]
{A}-[Name=<LocalReferenceNumber>]
{A}-[#EndOfReport]");

			var stmTemplate = Factory.New<StmTemplate>();
			stmTemplate.SO_Name = "UnitTest";
			stmTemplate.SO_DataContext = "CusEntryHeader";
			stmTemplate.SO_Template = template1;
			stmTemplate.SO_TemplateType = "DOC";

			var stmMenuTemplatePivot = Factory.New<StmMenuTemplatePivot>();
			stmMenuTemplatePivot.SI_SU = stmMenuItem.PK;
			stmMenuTemplatePivot.SI_SO = stmTemplate.PK;
			stmMenuTemplatePivot.SI_DocumentTitle = "Entry Print";
			stmMenuTemplatePivot.SI_PrintByDefault = ZBool.True;
			stmMenuTemplatePivot.SI_RT_DocType = refDocType.PK;

			var declaration = Factory.New<JobDeclaration>();
			var declarationWithWorkflow = (IWorkflowProvider)declaration;
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.LocalReferenceNumber = "LRN001";
			cusEntryHeader.CH_MessageType = Enterprise.Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			cusEntryHeader.CH_CEI_Instruction = Factory.NewWithValidTestData<CusEntryInstruction>().PK;
			var cusEntryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader2.LocalReferenceNumber = "LRN002";
			cusEntryHeader2.CH_MessageType = Enterprise.Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			cusEntryHeader2.CH_CEI_Instruction = Factory.NewWithValidTestData<CusEntryInstruction>().PK;

			var trigger = declarationWithWorkflow.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomsEntryStatusCode;
			trigger.P9_LineTriggerType = "CEH";

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs;
			action.PQ_SU_Document = stmMenuItem.PK;
			Factory.Save();
			cusEntryHeader.Logs.AddNew(Events.CustomsEntryStatus, cusEntryHeader.CH_EntryStatus, ZDateTime.Now.ToOffset());
			cusEntryHeader2.Logs.AddNew(Events.CustomsEntryStatus, cusEntryHeader2.CH_EntryStatus, ZDateTime.Now.ToOffset());
			cusEntryHeader2.Logs.AddNew(Events.CustomsEntryStatus, cusEntryHeader2.CH_EntryStatus, ZDateTime.Now.ToOffset());

			Factory.Save();
			MasterFilesTestHelper.RunLogWalker();
			Factory.Save();

			AssertEquals("nothing to print on decl", 0, GetPrintForBusinessObject(declaration).Length);

			var docsOnLRN001 = GetPrintForBusinessObject(cusEntryHeader);
			AssertEquals("1 doc on LRN001", 1, docsOnLRN001.Length);

			AssertEquals("2 docs on LRN002", 2, GetPrintForBusinessObject(cusEntryHeader2).Length);

			StmPrintJob[] GetPrintForBusinessObject(BusinessObject bo)
			{
				return Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentTableName, SQLComparisonOperator.Equal, bo.TableName).AddToFilter(StmPrintJobSchema.SP_ParentGuid, SQLComparisonOperator.Equal, bo.PK));
			}
		});

		public void TestGetEntryNumberFormatter()
		{
			var dec = (JobDeclaration)GetNewDeclaration();
			var entry = dec.CustomsEntryHeaders.AddNew();
			AssertType<DECommonGoodsItemsIntegrator>(entry.CommonGoodsItemsIntegrator);
		}

		public void TestSetDefaultWarehouseTransactionStatusFromParent()
		{
			var declaration = (JobDeclaration)GetNewDeclaration();
			declaration.WarehouseTransactionStatus = "OCW";
			var header = declaration.ActiveEntryHeaders.AddNew();
			AssertEquals(header.CH_WarehouseTransactionStatus, "OCW");
		}

		public override void TestOfficeOfEntry()
		{
			var declaration = (JobDeclaration)GetNewDeclaration();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsOffices.RemoveAndDeleteAll();
			declaration.JE_CustomsOffice = "LV001000";
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, "LV002000");
			AssertEquals("LV002000", declaration.OfficeOfEntry);
			AssertEquals("LV002000", entry.OfficeOfEntry);
		}

		public void TestOrganisationsProxiedFromDec()
		{
			JobDeclaration dec = (JobDeclaration)GetNewDeclaration();
			var entry = dec.CustomsEntryHeaders.AddNew();

			AssertNull(entry.Supplier.Organisation);
			AssertNull(entry.Importer.Organisation);
			AssertContains("EDI CUSTOMS BROKERS", entry.DeclarantOrganisation.OH_FullName);
			AssertNull(entry.SellerOrganisation);
			AssertNull(entry.Buyer.Organisation);
			AssertContains("EDI CUSTOMS BROKERS", entry.RepresentativeOrganisation.OH_FullName);

			OrgHeader supplier = Factory.New<OrgHeader>();
			OrgHeader import = Factory.New<OrgHeader>();
			OrgHeader declarant = Factory.New<OrgHeader>();
			declarant.OH_FullName = "Daniel's forwarder Ltd";
			OrgAddress declarantAddress = Factory.New<OrgAddress>();
			declarant.Addresses.Add(declarantAddress);
			var seller = Factory.New<OrgHeader>();
			var buyer = Factory.New<OrgHeader>();
			var representative = Factory.New<OrgHeader>();

			dec.JE_OH_Supplier = supplier.PK;
			dec.JE_OA_DeclarantAddress = declarantAddress.PK;
			dec.JE_OH_Importer = import.PK;
			dec.JE_OA_SellerAddress = seller.MainAddress.PK;
			dec.BuyerDocAddress.OrganisationPK = buyer.PK;
			dec.JE_OA_Representative = representative.MainAddress.PK;

			AssertEquals(supplier.PK, entry.Supplier.Organisation.PK);
			AssertEquals(import.PK, entry.Importer.Organisation.PK);
			AssertEquals(entry.DeclarantOrganisation.OH_FullName, "Daniel's forwarder Ltd");
			AssertEquals(seller.PK, entry.SellerOrganisation.PK);
			AssertEquals(buyer.PK, entry.Buyer.Organisation.PK);
			AssertEquals(representative.PK, entry.RepresentativeOrganisation.PK);
		}

		public void TestIsOriginAndDestinationRequiredInItinerary()
		{
			var dec = Factory.New<JobDeclaration>();
			var entryHeader = (CusEntryHeader)dec.ActiveEntryHeaders.AddNew();
			AssertEquals("Origin and dest should appear in itinerary, ja", true, entryHeader.IsOriginAndDestinationRequiredInItinerary_ForTest);
		}

		public void TestEntryLinesTypes()
		{
			CombineAssertions(() =>
			{
				var testItem = Factory.New<CusEntryHeader>();
				AssertType<CusEntryLineCollection<CusEntryLine>>("Merged Lines should be a DE CusEntryLineCollection", testItem.MergedLines);
				AssertType<AllCusEntryLineCollection<CusEntryLine>>("All Entry Lines should be a DE AllCusEntryLineCollection", testItem.AllEntryLines);
			});
		}

		public void TestCEI_Style()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.CustomsEntryInstructions.RemoveAndDeleteAll();
			var cei = dec.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "H1";

			var entryHeader = dec.ActiveEntryHeaders.AddNew() as CusEntryHeader;
			entryHeader.CH_CEI_Instruction = cei.PK;
			AssertEquals(cei.CEI_Style, entryHeader.Style);
		}

		public void TestCEI_SubStyle()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.CustomsEntryInstructions.RemoveAndDeleteAll();
			var cei = dec.CustomsEntryInstructions.AddNew();
			cei.CEI_SubStyle = "A";

			var entryHeader = dec.ActiveEntryHeaders.AddNew() as CusEntryHeader;
			entryHeader.CH_CEI_Instruction = cei.PK;
			AssertEquals(cei.CEI_SubStyle, entryHeader.SubStyle);
		}

		public void TestCEI_Description()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.CustomsEntryInstructions.RemoveAndDeleteAll();
			var cei = dec.CustomsEntryInstructions.AddNew();
			cei.CEI_Description = "Description";

			var entryHeader = dec.ActiveEntryHeaders.AddNew() as CusEntryHeader;
			entryHeader.CH_CEI_Instruction = cei.PK;
			AssertEquals(cei.CEI_Description, entryHeader.Description);
		}

		public override void TestEntriesOfHouseBillsRefreshed()
		{
			var declaration = ImportJobDeclaration;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.FillWithValidTestData();

			var invoice1 = declaration.Invoices.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			SetInvoicesToResultInTwoEntries(invoice1.JobComInvoiceLines.AddNew(), invoice2.JobComInvoiceLines.AddNew());

			var bill1 = declaration.Bills.AddNew();
			var bill2 = declaration.Bills.AddNew();

			invoice1.JZ_CU_RelatedHouseBill = bill1.PK;
			invoice2.JZ_CU_RelatedHouseBill = bill2.PK;

			DoMerge(declaration);

			var entry1 = (CusEntryHeader)invoice1.JobComInvoiceLines[0].CusEntryLine.Header;
			var entry2 = (CusEntryHeader)invoice2.JobComInvoiceLines[0].CusEntryLine.Header;
			AssertEquals(true, entry1 != entry2);
			AssertEquals(true, new List<Customs.Business.CusEntryHeader>(bill1.Entries).Contains(entry1));
			AssertEquals(true, new List<Customs.Business.CusEntryHeader>(bill2.Entries).Contains(entry2));

			invoice1.JZ_CU_RelatedHouseBill = bill2.PK;
			invoice2.JZ_CU_RelatedHouseBill = bill1.PK;

			DoMerge(declaration);

			entry1 = (CusEntryHeader)invoice1.JobComInvoiceLines[0].CusEntryLine.Header;
			entry2 = (CusEntryHeader)invoice2.JobComInvoiceLines[0].CusEntryLine.Header;
			AssertEquals(true, entry1 != entry2);
			AssertEquals(true, new List<Customs.Business.CusEntryHeader>(bill1.Entries).Contains(entry2));
			AssertEquals(true, new List<Customs.Business.CusEntryHeader>(bill2.Entries).Contains(entry1));
		}

		public void TestLoadCusEntryNumber_MRN()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var entryNum = CusEntryNumber.LoadOrCreate(entryHeader, "MRN", "DE");
			AssertEquals(entryNum, entryHeader.CusEntryNumber);
		}

		public void TestLoadCusEntryNumber_NotMRN()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			CusEntryNumber.LoadOrCreate(entryHeader, "IMP", "DE");
			AssertNull(entryHeader.CusEntryNumber);
		}

		public void TestCreateCusEntryNumber_MRN()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			CombineAssertions(() =>
			{
				AssertNull("No CusEntryNum", entryHeader.CusEntryNumber);

				entryHeader.EntryNumber = "ATC996151771020016389";
				var entryNum = entryHeader.CusEntryNumber;
				AssertNotNull("CusEntryNum created", entryNum);
				AssertEquals("EntryType MRN", CusEntryNumberTypes.Standard.MovementReferenceNumber, entryNum.CE_EntryType);
			});
		}

		public void TestEntryTypeFriendlyName_Export()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportNormal;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000000;
			entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._13;

			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			CombineAssertions(() =>
			{
				AssertEquals("ProcedureCodeWithoutConcession is empty", "(EX13) 000000", entryHeader.EntryTypeFriendlyName);

				invoiceLine.JI_Procedure = "4000456";
				AssertEquals("ProcedureCodeWithoutConcession isn't empty", "(EX13 / 4000) 000000", entryHeader.EntryTypeFriendlyName);
			});
		}

		public void TestLocalReferenceNumber_Getter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OwnerRef = "12345";
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("No LRNCusEntryNumber", "12345", entryHeader.LocalReferenceNumber);

				var cusEntryNumber = CusEntryNumber.New(entryHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Germany);
				cusEntryNumber.CE_EntryNum = "ABCDE";
				AssertEquals("Has LRNCusEntryNumber", "ABCDE", entryHeader.LocalReferenceNumber);
			});
		}

		public void TestLocalReferenceNumber_Getter_EmptyJE_OwnerRef_Import() => AssertLocalReferenceNumber_Getter_EmptyJE_OwnerRef(Common.EU.EUJobMessageTypeList.Codes.Import);

		public void TestLocalReferenceNumber_Getter_EmptyJE_OwnerRef_Export() => AssertLocalReferenceNumber_Getter_EmptyJE_OwnerRef(Common.EU.EUJobMessageTypeList.Codes.Export);

		public void TestLocalReferenceNumber_Setter_Exports()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.LocalReferenceNumber = string.Empty.PadRight(23, '1');
			AssertEquals(string.Empty.PadRight(22, '1'), CusEntryNumber.Load(entryHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Germany).CE_EntryNum);
		}

		public void TestLocalReferenceNumber_Setter_Imports()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = ATLASVersionNumberList.Codes._101 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
				entryHeader.LocalReferenceNumber = string.Empty.PadRight(23, '1');
				AssertEquals(string.Empty.PadRight(22, '1'), CusEntryNumber.Load(entryHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Germany).CE_EntryNum);
			}
		}

		public void TestLocalReferenceNumber_Caption()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			AssertEquals("LRN", DataBoundResourceStrings.GetDataForProperty(entryHeader.LocalReferenceNumberInfo).Caption);
		}

		public void TestLocalReferenceNumber_MaxLength_Exports()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			AssertEquals(22, entryHeader.LocalReferenceNumberInfo.MaxLength);
		}

		public void TestLocalReferenceNumber_MaxLength_Imports()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = ATLASVersionNumberList.Codes._101 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
				AssertEquals(22, entryHeader.LocalReferenceNumberInfo.MaxLength);
			}
		}

		public void TestOnSaving_FillInLocalReferenceNumber_Import() => AssertOnSaving_FillInLocalReferenceNumber(Common.EU.EUJobMessageTypeList.Codes.Import);

		public void TestOnSaving_FillInLocalReferenceNumber_Export() => AssertOnSaving_FillInLocalReferenceNumber(Common.EU.EUJobMessageTypeList.Codes.Export);

		public void TestOnSaving_LRNAndBGMReferenceSuffixesMatch_Import() => AssertOnSaving_LRNAndBGMReferenceSuffixesMatch(Common.EU.EUJobMessageTypeList.Codes.Import);

		public void TestOnSaving_LRNAndBGMReferenceSuffixesMatch_Export() => AssertOnSaving_LRNAndBGMReferenceSuffixesMatch(Common.EU.EUJobMessageTypeList.Codes.Export);

		public void TestOnSaving_FillInLocalReferenceNumber_JE_OwnerRefIsEmpty_Import() => AssertOnSaving_FillInLocalReferenceNumber_JE_OwnerRefIsEmpty(Common.EU.EUJobMessageTypeList.Codes.Import);

		public void TestOnSaving_FillInLocalReferenceNumber_JE_OwnerRefIsEmpty_Export() => AssertOnSaving_FillInLocalReferenceNumber_JE_OwnerRefIsEmpty(Common.EU.EUJobMessageTypeList.Codes.Export);

		public void TestShouldBeIncludedInCusEntryNumberFilterCore_Export()
		{
			var exportDeclaration = GetNewDeclaration();
			var entryHeader = exportDeclaration.CustomsEntryHeaders.AddNew();
			var entryNum = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			entryNum.CE_EntryNum = "MRNTest";
			CombineAssertions(() =>
			{
				foreach (var status in new[] { A0115DepartureStatusCodeList.Codes._191, A0115DepartureStatusCodeList.Codes._520 })
				{
					entryHeader.CH_EntryStatus = status;
					entryNum.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
					AssertEquals($"EntryType: MRN, CH_EntryStatus: {status}", expected: false, entryHeader.ShouldBeIncludedInCusEntryNumberFilter);

					entryNum.CE_EntryType = CusEntryNumberTypes.Standard.LocalReferenceNumber;
					AssertEquals($"EntryType: LRN, CH_EntryStatus: {status}", expected: true, entryHeader.ShouldBeIncludedInCusEntryNumberFilter);
				}
				entryHeader.CH_EntryStatus = EntryStatusList.Codes.Cancelled;
				entryNum.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
				AssertEquals($"Base Condition applied, CH_EntryStatus: CAN", expected: false, entryHeader.ShouldBeIncludedInCusEntryNumberFilter);

				entryHeader.CH_EntryStatus = A0115DepartureStatusCodeList.Codes._570;
				entryNum.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
				AssertEquals($"EntryType: MRN, CH_EntryStatus: 570", expected: true, entryHeader.ShouldBeIncludedInCusEntryNumberFilter);
			});
		}

		public void TestIsChangingToClearStatusForAccIntegration()
		{
			SetUpRefDataCustomsStatuses();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = Factory.New<CusEntryHeaderForTest>();
			entryHeader.CH_JE = declaration.PK;

			entryHeader.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.TRA;
			AssertEquals("TRA status should never trigger any billing.", false, entryHeader.IsChangingToClearStatusForAccIntegration);

			Factory.Save();
			entryHeader.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.TX5;
			AssertEquals("Change of status to TX5  should trigger a billing.", true, entryHeader.IsChangingToClearStatusForAccIntegration);
		}

		public void TestIsStatusChangingToCleared_WithNoCustomsValuesFromRegistry()
		{
			SetUpRefDataCustomsStatuses();

			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = Factory.New<CusEntryHeaderForTest>();
			entryHeader.CH_JE = declaration.PK;

			AssertEquals(false, entryHeader.IsStatusChangingToCleared(UniversalReferenceConstants.EntryStatus.TX6, UniversalReferenceConstants.EntryStatus.TRA));
			AssertEquals(true, entryHeader.IsStatusChangingToCleared(UniversalReferenceConstants.EntryStatus.TX6, UniversalReferenceConstants.EntryStatus.TX5));
			AssertEquals(false, entryHeader.IsStatusChangingToCleared("XYZ", UniversalReferenceConstants.EntryStatus.TX5));
		}

		public void TestIsStatusChangingToCleared_WithCustomsValuesFromRegistry()
		{
			SetUpRefDataCustomsStatuses();

			var options = CustomsDataRegistry.Instance.EnableAccountingIntegration.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			options.EUCustomsStatusCodes = UniversalReferenceConstants.EntryStatus.TX6;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, options);

			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = Factory.New<CusEntryHeaderForTest>();
			entryHeader.CH_JE = declaration.PK;

			AssertEquals(false, entryHeader.IsStatusChangingToCleared(UniversalReferenceConstants.EntryStatus.TRA, UniversalReferenceConstants.EntryStatus.TX5));
			AssertEquals(true, entryHeader.IsStatusChangingToCleared(UniversalReferenceConstants.EntryStatus.TRA, UniversalReferenceConstants.EntryStatus.TX6));
			AssertEquals(false, entryHeader.IsStatusChangingToCleared("XYZ", EntryStatusList.Codes.Clear));
		}

		void SetUpRefDataCustomsStatuses()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");
			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: grouping);

			AddCustomsStatusCodeList(UniversalReferenceConstants.EntryStatus.TRA, "TRA");
			AddCustomsStatusCodeList(UniversalReferenceConstants.EntryStatus.TX5, "TX5", hasExcecuteAutoBillingAttribute: true);
			AddCustomsStatusCodeList("XYZ", "Fake Customs Status XYZ with Excecute Auto Billing Attribute", hasExcecuteAutoBillingAttribute: true);
			Factory.Save();

			void AddCustomsStatusCodeList(string code, string description, bool hasExcecuteAutoBillingAttribute = false)
			{
				var cusCodeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, code, description, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
				if (hasExcecuteAutoBillingAttribute)
				{
					helper.CreateNewOrGetExistingRefCusCodeListAttributeName("IExecuteAutoBilling", ZString.Empty, cusCodeList.ZZD_ZZK_NKCodeType, cusCodeList.ZZD_ZZZ_NKDataGrouping);
					cusCodeList.Attributes.AddNew("IExecuteAutoBilling", ZString.Empty);
				}
			}
		}

		protected override BaseJobDeclaration GetNewDeclaration()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			return dec;
		}

		protected override void DoMerge(BaseJobDeclaration declaration)
		{
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			foreach (var invoiceLine in declaration.InvoiceLines.Cast<EU.Business.Declaration.JobComInvoiceLine>())
			{
				invoiceLine.JI_CEI = entryInstruction.PK;
			}
			base.DoMerge(declaration);
		}

		protected override BaseJobDeclaration ImportJobDeclaration
		{
			get
			{
				var declaration = base.ImportJobDeclaration;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				return declaration;
			}
		}

		protected override (ZString OverseasFreightChargeCode, ZString OverseasInsuranceChargeCode, ZString NotIncludedChargeCode) GetChargeCodesForTotalTAndI()
			=> (ImportChargeCodeList.Codes._011, ImportChargeCodeList.Codes._012, CustomsChargeTypeList.Codes.PackingCost);

		protected override IChargesCurrencyTestSetup GetChargesCurrencyTestSetup() => new CurrencyTestSetup();

		CusEntryNumber GetCusEntryNumber(ZGuid parentID)
		{
			var cusEntryNumberFilter = new ZQuery(CusEntryNumSchema.CE_ParentID, parentID);
			cusEntryNumberFilter.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.LocalReferenceNumber);
			return Factory.LoadTop1<CusEntryNumber>(cusEntryNumberFilter);
		}

		void AssertOnSaving_FillInLocalReferenceNumber(string messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			declaration.JE_OwnerRef = "TestOwnRef";

			CombineAssertions(() =>
			{
				var entry = declaration.CustomsEntryHeaders.AddNew();
				Factory.Save();
				AssertEquals("LocalReferenceNumber is from JE_OwnRef", "TestOwnRef", entry.LocalReferenceNumber);

				var entry2 = declaration.CustomsEntryHeaders.AddNew();
				Factory.Save();
				AssertEquals("Has CusEntryNum for entry2", "TestOwnRef/1", GetCusEntryNumber(entry2.PK).CE_EntryNum);

				var cusEntryNum = GetCusEntryNumber(entry.PK);
				AssertEquals("CusEntryNum for entry wasn't changed", "TestOwnRef", cusEntryNum.CE_EntryNum);
				cusEntryNum.CE_EntryNum = "TestOwnRefXX";
				Factory.Save();
				AssertEquals("LocalReferenceNumber changes with CE_EntryNum", "TestOwnRefXX", entry.LocalReferenceNumber);
			});
		}

		public void AssertOnSaving_LRNAndBGMReferenceSuffixesMatch(string messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			declaration.JE_OwnerRef = "TestOwnRef";

			Factory.Save();

			CombineAssertions(() =>
			{
				var entry1 = declaration.CustomsEntryHeaders.AddNew();
				var entry2 = declaration.CustomsEntryHeaders.AddNew();
				var entry3 = declaration.CustomsEntryHeaders.AddNew();

				Factory.Save();

				var ucr = declaration.JE_UCR;

				foreach (var entry in new[] { entry1, entry2, entry3 })
				{
					var entrySuffix = GetSuffixFromLRN(entry);
					var bgmReference = entry.CH_BGMReference;
					AssertEquals(ucr + entrySuffix, bgmReference);
				}
			});

			string GetSuffixFromLRN(CusEntryHeader entryHeader)
			{
				var suffix = string.Empty;
				var lrn = entryHeader.LocalReferenceNumber;
				if (lrn.Contains("/"))
				{
					var separatorIndex = lrn.IndexOf("/");
					suffix = lrn.Substring(separatorIndex);
				}

				return suffix;
			}
		}

		void AssertLocalReferenceNumber_Getter_EmptyJE_OwnerRef(string messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			declaration.JE_DeclarationReference = "12346";
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("No LRNCusEntryNumber, JE_DeclarationReference is used", "12346", entryHeader.LocalReferenceNumber);

				var cusEntryNumber = CusEntryNumber.New(entryHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Germany);
				cusEntryNumber.CE_EntryNum = "ABCDE";
				AssertEquals("Has LRNCusEntryNumber", "ABCDE", entryHeader.LocalReferenceNumber);
			});
		}

		void AssertOnSaving_FillInLocalReferenceNumber_JE_OwnerRefIsEmpty(string messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			declaration.JE_OwnerRef = string.Empty;
			declaration.JE_DeclarationReference = "12346";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			AssertEquals("DeclarationReference", "12346", GetCusEntryNumber(entry.PK).CE_EntryNum);
		}

		sealed class CurrencyTestSetup : IChargesCurrencyTestSetup
		{
			void IChargesCurrencyTestSetup.SetupJobDecWithOFTAndCIFCharges(BaseJobDeclaration declaration, ZString currencyCode)
			{
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				declaration.AutoCreateChargesBasedOnIncoTerm = false;

				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_InvoiceAmount = 10000m;
				invoiceHeader.JZ_RX_NKInvoice_Currency = currencyCode;
				invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 10000m;

				var nonDutiableCharge = invoiceHeader.Charges.AddNew();
				nonDutiableCharge.J7_ChargeType = CustomsChargeTypeList.Codes.ForeignInlandFreight;
				nonDutiableCharge.J7_Amount = 200m;
				nonDutiableCharge.J7_IsDutiable = false;
				nonDutiableCharge.J7_IsIncludedInITOT = true;

				var oft = invoiceHeader.Charges.AddNew();
				oft.J7_ChargeType = ImportChargeCodeList.Codes._011;
				oft.J7_Amount = 500m;
				oft.J7_RX_NKCurrency = invoiceHeader.Invoice_Currency.RX_Code;
			}

			ZDecimal IChargesCurrencyTestSetup.ExpectedFOB => 10500m;
			ZDecimal IChargesCurrencyTestSetup.ExpectedCIF => 11000m;
		}
	}

	sealed class CusEntryHeaderForTest : CusEntryHeader
	{
		public CusEntryHeaderForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		public new ZBool IsStatusChangingToCleared(ZString originalStatus, ZString newStatus) => base.IsStatusChangingToCleared(originalStatus, newStatus);

		public new ZBool IsChangingToClearStatusForAccIntegration => base.IsChangingToClearStatusForAccIntegration;
	}
}
