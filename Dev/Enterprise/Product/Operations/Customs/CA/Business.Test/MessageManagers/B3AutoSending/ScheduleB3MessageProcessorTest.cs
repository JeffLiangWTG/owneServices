using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ScheduleB3MessageProcessorTest : TestCaseWithFactory
	{
		public void TestSaveWithConcurrentConflict_CAD()
		{
			var messageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var alternativeBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK) { OrderBy = GlbBranchSchema.GB_Code.Name });
			var data = (RegistryItemSet)CargoWise.Application.ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
			var customsDefaultToCurrentLoginDeptRegistry = (BooleanRegistryItem)data.FindByName("CustomsDefaultToCurrentLoginDept");
			customsDefaultToCurrentLoginDeptRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			ScheduleCADMessageProcessor processor;
			JobDeclaration declaration;
			Notifications notifications;
			CusEntryHeader entryHeader;

			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest(companyPk: alternativeBranch.Company.PK.ToGuid()))
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
				CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDXX");
				var importer = Factory.New<OrgHeader>();
				importer.OH_Code = "XXXXX";
				importer.OH_RL_NKClosestPort = "AUBNE";
				importer.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = "BBB";

				declaration = Factory.New<JobDeclaration>();
				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
				declaration.JE_EntryAuthorisationDate = new DateTime(2015, 5, 13, 12, 0, 0);
				declaration.JE_GB = GlbBranch.CurrentBranch.PK;
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_MessageType = messageType;
				entryHeader.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted;

				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2015, 5, 1);
				var line = entryHeader.MergedLines.AddNew();
				line.CL_CustomsValue = 3000m;
				Assert("Precondition: Is High Value Declaration", !declaration.IsLowValueNormalReleaseJob);

				var delayFactorRegistryBO = new DelayFactorRegistryBusinessObject();
				delayFactorRegistryBO.HVSDelayIntervalType = DelayIntervalTypeCodes.Codes.DAR;
				delayFactorRegistryBO.HVSDelayInterval = 2;
				CACustomsDataRegistry.Instance.EntryAutomaticSendingDelayThresholds.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, delayFactorRegistryBO);
				AssertEquals("Precondition: 2 days after Release", new DateTime(2015, 5, 15, 12, 0, 0), declaration.CalculatedB3SendingDate);
				Factory.Save();

				processor = new ScheduleCADMessageProcessor(declaration);
				notifications = new Notifications();
			}

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var reloadDec = newFactory.Load<JobDeclaration>(declaration.PK);
			reloadDec.JE_MessageStatus = MessageStatusList.Codes.ClearOriginal;
			newFactory.Save();

			using (DisposableEnvironment.ForBranch(alternativeBranch.PK.ToGuid()))
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest(companyPk: alternativeBranch.Company.PK.ToGuid()))
			{
				processor.Process(notifications);
				AssertContains("CONCURRENCY Error Saving Record", notifications.ToString());

				var reloadEntry = newFactory.Load<CusEntryHeader>(entryHeader.PK);
				AssertEquals("Message was not saved.", 0, reloadEntry.Messages.Count);
				AssertEquals(ZDateTime.Empty, reloadEntry.DeferredB3MessageTime);
			}
		}

		public void TestProcess_CAD()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				var alternativeBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK) { OrderBy = GlbBranchSchema.GB_Code.Name });
				var data = (RegistryItemSet)CargoWise.Application.ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
				var customsDefaultToCurrentLoginDeptRegistry = (BooleanRegistryItem)data.FindByName("CustomsDefaultToCurrentLoginDept");
				customsDefaultToCurrentLoginDeptRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				using (DisposableEnvironment.ForBranch(alternativeBranch.PK.ToGuid()))
				using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest(companyPk: alternativeBranch.Company.PK.ToGuid()))
				{
					TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
					CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDXX");
					var importer = Factory.New<OrgHeader>();
					importer.OH_Code = "XXXXX";
					importer.OH_RL_NKClosestPort = "AUBNE";
					importer.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = "BBB";

					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_OH_Importer = importer.PK;
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
					declaration.JE_EntryAuthorisationDate = new DateTime(2015, 5, 13, 12, 0, 0);
					declaration.JE_GB = GlbBranch.CurrentBranch.PK;
					var entryHeader = declaration.CustomsEntryHeaders.AddNew();
					entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
					entryHeader.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted;

					var invoiceHeader = declaration.Invoices.AddNew();
					invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2015, 5, 1);
					var line = entryHeader.MergedLines.AddNew();
					line.CL_CustomsValue = 3000m;
					Assert("Precondition: Is High Value Declaration", !declaration.IsLowValueNormalReleaseJob);

					var delayFactorRegistryBO = new DelayFactorRegistryBusinessObject();
					delayFactorRegistryBO.HVSDelayIntervalType = DelayIntervalTypeCodes.Codes.DAR;
					delayFactorRegistryBO.HVSDelayInterval = 2;
					CACustomsDataRegistry.Instance.EntryAutomaticSendingDelayThresholds.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, delayFactorRegistryBO);
					AssertEquals("Precondition: 2 days after Release", new DateTime(2015, 5, 15, 12, 0, 0), declaration.CalculatedB3SendingDate);
					Factory.Save();

					var processor = new ScheduleCADMessageProcessor(declaration);
					var notifications = new Notifications();
					processor.Process(notifications);
					AssertStartsWith("Schedule B3", "CAD message scheduled successfully at", notifications.ToString());
					AssertEquals("1 message was added", 1, entryHeader.Messages.Count);
					AssertEquals(EDIMessage.Status.Queued, entryHeader.Messages[0].EM_Status);
					var deferredMessasgeTime = new ZDateTime(2015, 5, 15, 08, 00, 00);
					AssertEquals(deferredMessasgeTime, entryHeader.Messages[0].EM_HeldUntilDate);
					AssertEquals(deferredMessasgeTime, entryHeader.DeferredB3MessageTime);
					AssertEquals(declaration.JE_GB, entryHeader.Messages[0].EM_GB);
				}
			}
		}

		public void TestProcessThroughAutoSendCustomsMessagingBatchProcessor()
		{
			var alternativeBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK) { OrderBy = GlbBranchSchema.GB_Code.Name });
			var data = (RegistryItemSet)CargoWise.Application.ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
			var customsDefaultToCurrentLoginDeptRegistry = (BooleanRegistryItem)data.FindByName("CustomsDefaultToCurrentLoginDept");
			customsDefaultToCurrentLoginDeptRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (DisposableEnvironment.ForBranch(alternativeBranch.PK.ToGuid()))
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest(companyPk: alternativeBranch.Company.PK.ToGuid()))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
				CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDXX");
				var importer = Factory.New<OrgHeader>();
				importer.OH_Code = "XXXXX";
				importer.OH_RL_NKClosestPort = "AUBNE";
				importer.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = "BBB";

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
				declaration.JE_EntryAuthorisationDate = new DateTime(2015, 5, 13, 12, 0, 0);
				declaration.JE_GB = GlbBranch.CurrentBranch.PK;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
				entryHeader.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted;

				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2015, 5, 1);
				var line = entryHeader.MergedLines.AddNew();
				line.CL_CustomsValue = 3000m;
				Assert("Precondition: Is High Value Declaration", !declaration.IsLowValueNormalReleaseJob);

				var delayFactorRegistryBO = new DelayFactorRegistryBusinessObject();
				delayFactorRegistryBO.HVSDelayIntervalType = DelayIntervalTypeCodes.Codes.DAR;
				delayFactorRegistryBO.HVSDelayInterval = 2;
				CACustomsDataRegistry.Instance.EntryAutomaticSendingDelayThresholds.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, delayFactorRegistryBO);
				AssertEquals("Precondition: 2 days after Release", new DateTime(2015, 5, 15, 12, 0, 0), declaration.CalculatedB3SendingDate);
				Factory.Save();

				CustomsStmProcessQueueLoader.New(declaration, CustomsStmProcessQueueLoader.Constants.AutoSendCustomsMessaging, "SB3");

				var srmProcessQueue = Factory.New<StmProcessQueue>();
				srmProcessQueue.SW_ApplicationCode = "ASC";
				srmProcessQueue.SW_JobTypeCode = "CUS";
				srmProcessQueue.SW_ActionCode = "SB3";
				srmProcessQueue.SW_ReferenceID = declaration.PK;
				srmProcessQueue.SW_ReferenceTableCode = "JE";
				srmProcessQueue.SW_PostedTimeUtc = ZDateTime.Today;

				Factory.Save();

				var processor = new AutoSendCustomsMessagingBatchProcessor(new LoggingInformation());
				processor.ExecuteBatch();

				entryHeader.Messages.Reload(true);
				AssertEquals("1 message was added", 1, entryHeader.Messages.Count);
				AssertEquals(EDIMessage.Status.Queued, entryHeader.Messages[0].EM_Status);
				AssertEquals(new ZDateTime(2015, 5, 15, 08, 00, 00), entryHeader.Messages[0].EM_HeldUntilDate);
				AssertEquals(new ZDateTime(2015, 5, 15, 08, 00, 00), entryHeader.DeferredB3MessageTime);
				AssertEquals(declaration.JE_GB, entryHeader.Messages[0].EM_GB);
			}
		}

		public void TestNoMessageIsGeneratedWhenServiceProviderInterfaceIsConfiguredInRegistry()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.CurrentFallbackLevel = new FallbackLevel(GlbCompany.CurrentCompany, null, null);
			customsInterface.RecipientID = "123";
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Interfaced;

			var alternativeBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK) { OrderBy = GlbBranchSchema.GB_Code.Name });
			var data = (RegistryItemSet)CargoWise.Application.ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
			var customsDefaultToCurrentLoginDeptRegistry = (BooleanRegistryItem)data.FindByName("CustomsDefaultToCurrentLoginDept");
			customsDefaultToCurrentLoginDeptRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (DisposableEnvironment.ForBranch(alternativeBranch.PK.ToGuid()))
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest(companyPk: alternativeBranch.Company.PK.ToGuid()))
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
				CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDXX");
				var importer = Factory.New<OrgHeader>();
				importer.OH_Code = "XXXXX";
				importer.OH_RL_NKClosestPort = "AUBNE";
				importer.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = "BBB";

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
				declaration.JE_EntryAuthorisationDate = new DateTime(2015, 5, 13, 12, 0, 0);
				declaration.JE_GB = GlbBranch.CurrentBranch.PK;

				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2015, 5, 1);
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 1000m;
				Factory.Save();

				var processor = new ScheduleCADMessageProcessor(declaration);
				var notifications = new Notifications();
				processor.Process(notifications);
				AssertContains("because this job is configured to submit through a designated service provider interface", notifications.ToString());
			}
		}

		public void TestNoExceptionThrownWhenAutoSendMessage()
		{
			var alternativeBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK) { OrderBy = GlbBranchSchema.GB_Code.Name });
			var data = (RegistryItemSet)CargoWise.Application.ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
			var customsDefaultToCurrentLoginDeptRegistry = (BooleanRegistryItem)data.FindByName("CustomsDefaultToCurrentLoginDept");
			customsDefaultToCurrentLoginDeptRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (DisposableEnvironment.ForBranch(alternativeBranch.PK.ToGuid()))
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest(companyPk: alternativeBranch.Company.PK.ToGuid()))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
				CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDXX");
				var importer = Factory.New<OrgHeader>();
				importer.OH_Code = "XXXXX";
				importer.OH_RL_NKClosestPort = "AUBNE";
				importer.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = "BBB";

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
				declaration.JE_EntryAuthorisationDate = new DateTime(2015, 5, 13, 12, 0, 0);
				declaration.JE_GB = GlbBranch.CurrentBranch.PK;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
				entryHeader.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted;

				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2015, 5, 1);

				var line = entryHeader.MergedLines.AddNew();
				line.CL_CustomsValue = 3000m;

				var delayFactorRegistryBO = new DelayFactorRegistryBusinessObject();
				delayFactorRegistryBO.HVSDelayIntervalType = DelayIntervalTypeCodes.Codes.DAR;
				delayFactorRegistryBO.HVSDelayInterval = 2;
				CACustomsDataRegistry.Instance.EntryAutomaticSendingDelayThresholds.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, delayFactorRegistryBO);
				Factory.Save();

				CustomsStmProcessQueueLoader.New(declaration, CustomsStmProcessQueueLoader.Constants.AutoSendCustomsMessaging, WorkflowTriggerActionTypeConstants.Codes.ScheduleB3Message);

				var srmProcessQueue = Factory.New<StmProcessQueue>();
				srmProcessQueue.SW_ApplicationCode = "ASC";
				srmProcessQueue.SW_JobTypeCode = "CUS";
				srmProcessQueue.SW_ActionCode = "SB3";
				srmProcessQueue.SW_ReferenceID = declaration.PK;
				srmProcessQueue.SW_ReferenceTableCode = "JE";
				srmProcessQueue.SW_PostedTimeUtc = ZDateTime.Today;

				Factory.Save();

				invoiceHeader.JZ_InvoiceCurrExRate = 1.2m;
				Factory.Save();

				var processor = new AutoSendCustomsMessagingBatchProcessor(new LoggingInformation());

				var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var declarationLoaded = newFactory.Load<JobDeclaration>(declaration.PK);

				using (var mutex = declarationLoaded.DoMergeMutex)
				{
					Assert(mutex.Lock());
					AssertEquals("Is locked", true, declarationLoaded.DoMergeMutex.IsLocked);
					AssertNoExceptionThrown(() => processor.ExecuteBatch());
				}

				var log = string.Join("\r\n", processor.Logger.UserLogStrings.Cast<string>());
				AssertContains("is in the process of merging this job; system cannot merge this data as it will result in a different entry details.", log);
				AssertContains("Please retry merging when the other user has finished.", log);
			}
		}
	}
}
