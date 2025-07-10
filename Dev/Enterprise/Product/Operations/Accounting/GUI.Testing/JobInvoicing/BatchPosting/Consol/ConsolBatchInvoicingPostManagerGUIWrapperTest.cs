using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.GUI.JobInvoicing.AgentPostingOptionSelection;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.BatchPosting.Testing
{
	public class ConsolBatchInvoicingPostManagerGUIWrapperTest : BaseBatchInvoicingPostManagerGUIWrapperTest
	{
		public override void TestConstructor()
		{
			JobInvoicingPostingOption expectedPostingOption = JobInvoicingPostingOption.All;
			IJobCostingPlugIn[] expectedConsols = new IJobCostingPlugIn[] { ConsolA, ConsolB };
			ZString expectedPostingOptionName = "Selected Posting Option Name";

			base.Wrapper = new ConsolBatchInvoicingPostManagerGUIWrapper(expectedPostingOption, expectedConsols, expectedPostingOptionName);

			AssertEquals(expectedPostingOption, Wrapper.PostingOption_ForTestOnly);
			AssertEquals("PlugInFactory_ForTestOnly.RefreshEnabled should be disabled for performance reasons.", false, Wrapper.PlugInFactory_ForTestOnly.RefreshEnabled);
			AssertEquals(expectedConsols.Length, Wrapper.ConsolCollection_ForTestOnly.Length);
			AssertEquals(expectedPostingOptionName, Wrapper.SelectedPostingOptionName);
		}

		public override void TestCurrentObjectCode()
		{
			Wrapper.Post();
			AssertEquals("Current Object Code", ConsolB.JK_UniqueConsignRef, Wrapper.CurrentObjectCode_ForTestOnly);
		}

		public override void TestDefaultPostedObjectName()
		{
			AssertEquals("Default Posted Object Name", "Consol", Wrapper.DefaultPostedObjectName_ForTestOnly);
		}

		protected override ZString GetExpectedMessageForJobOnHold()
		{
			return ZString.Empty;
		}

		protected override string GetExpectedMessageForNothingPosted()
		{
			var expectedZeroValueMessage = AccountingConfigurationRegistry.Instance.AllowZeroValueARInvoices.Value
				? string.Empty
				: System.Environment.NewLine + "* Total of the charges being posted is zero and your system is configured to disallow zero value invoices.";

			return $@"No appropriate charges were found for posting. This may be because:{expectedZeroValueMessage}
* All appropriate charges were to be appended to an existing transaction, and you elected to skip them.
* Charges pertaining to existing transactions contain differing AP details or currencies, and so cannot be appended.
* You are trying to post revenue but a shipment has invoicing on hold.
* You are trying to post revenue / cost but a shipment has Ready For Financial Closure status.
You may want to check the data you entered on Job Invoicing tabs on each shipment attached to this consol, and on the Costing tab of this form. Make sure that amounts are not zero and all appropriate information is entered for AP Invoices (if applicable).";
		}

		protected override BaseBatchInvoicingPostManagerGUIWrapper GetGuiWrapperWithOneObjectToPost(string postedBusinessObjectName)
		{
			Consols = new IJobCostingPlugIn[] { ConsolA };
			var wrapper = new ConsolBatchInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Consols, "Posting Option Name");
			wrapper.DoTestPostTransactions = true;
			return wrapper;
		}

		protected override IJobInvoicingPlugIn GUIWrapperPlugIn
		{
			get
			{
				return ConsolA;
			}
		}

		protected override BaseBatchInvoicingPostManagerGUIWrapper GetGuiWrapperWithTwoObjectsToPost(string postedBusinessObjectName, bool isDebtorValid = true)
		{
			IJobCostingPlugIn[] consols = (isDebtorValid) ? new IJobCostingPlugIn[] { ConsolA, ConsolB } : new IJobCostingPlugIn[] { Consol1, Consol2 };
			return new TestConsolBatchInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, consols, "Posting Option Name");
		}

		public void TestShowExportPostingPopup()
		{
			IReceivablesPostingChargeCollection charges = new IReceivablesPostingChargeCollection();
			charges.Key = new PostingChargeKey(TestObjectCreator.AALSHI.PK, "", ConsolA.JK_UniqueConsignRef, ZGuid.Empty, ZGuid.Empty, 0);
			Business.JobInvoicing.Posting.AgentPostingOptionSelection.AgentPostingOptionSelector optionSelector = new Business.JobInvoicing.Posting.AgentPostingOptionSelection.AgentPostingOptionSelector(Factory, ConsolA, charges);
			ExportAgentPostingEventArgs args = new ExportAgentPostingEventArgs(optionSelector);
			try
			{
				Wrapper.ConsolInvoicingPostManagerGUIWrapper_ExportAgentPosting_ForTestOnly(null, args);
				AssertEquals("Should show the agent posting currency selection form", typeof(AgentPostingOptionSelectionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
			finally
			{
				ZFormModaliser.LastFormShownDialogForTest.Dispose();
			}
		}

		public void TestProfitShareDocContactType()
		{
			IReceivablesPostingChargeCollection charges = new IReceivablesPostingChargeCollection();
			charges.Key = new PostingChargeKey(TestObjectCreator.AALSHI.PK, "", ConsolA.JK_UniqueConsignRef, ZGuid.Empty, ZGuid.Empty, 0);
			Business.JobInvoicing.Posting.AgentPostingOptionSelection.AgentPostingOptionSelector optionSelector = new Business.JobInvoicing.Posting.AgentPostingOptionSelection.AgentPostingOptionSelector(Factory, ConsolA, charges);
			ExportAgentPostingEventArgs args = new ExportAgentPostingEventArgs(optionSelector);
			AssertEquals("Should show the agent posting currency selection form", ContactType.Receivables.ToString(), Wrapper.ProfitShareMenuItem_ForTestOnly.SU_ContactType);
		}

		public void TestProfitShareMenuItem()
		{
			Assert("DocumentCommand not ReportCommand", Wrapper.ProfitShareMenuItem_ForTestOnly is DocumentCommand);
			AssertEquals("SU_MenuName", "Profit Share Calculation", Wrapper.ProfitShareMenuItem_ForTestOnly.SU_MenuName);
			AssertEquals("SU_ContactType", ContactType.Receivables.Code, Wrapper.ProfitShareMenuItem_ForTestOnly.SU_ContactType);
			Assert("SU_PreventAutoDelivery should be false (to allow delivery details to be found)", !Wrapper.ProfitShareMenuItem_ForTestOnly.SU_PreventAutoDelivery);
			Assert("SU_IsSystemDefined should be true", Wrapper.ProfitShareMenuItem_ForTestOnly.SU_IsSystemDefined);
			Assert("SU_SupportsVisualisation should be false", !Wrapper.ProfitShareMenuItem_ForTestOnly.SU_SupportsVisualisation);
			Assert("SU_IsModifiable should be false", !Wrapper.ProfitShareMenuItem_ForTestOnly.SU_IsModifiable);
		}

		public void TestGetPostManagerValidation()
		{
			IReceivablesPostingChargeCollection charges = new IReceivablesPostingChargeCollection();
			charges.Key = new PostingChargeKey(TestObjectCreator.AALSHI.PK, "", ConsolA.JK_UniqueConsignRef, ZGuid.Empty, ZGuid.Empty, 0);
			var validation = Wrapper.GetPostManagerValidation_ForTestOnly(Wrapper.Jobs_ForTestOnly, JobInvoicingPostingOption.All, Wrapper.OriginalJobs_ForTestOnly);
			Assert("Validation object should be of correct type", validation is ConsolPostManagerValidation);
			Assert("IsBulkPosting value should be true", validation.IsBulkPosting_ForTestOnly);
		}

		public void TestConsolHasNoChangesExceptLogAfterPosting()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKDestination = "JPAAM";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_UniqueConsignRef = "C1";

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C1";
			consol.Shipments.Add(shipment);
			Factory.Save();

			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			SetJobDetails(job, "Z00001000", LocalClient, 5M, Agent, 10M);

			CC1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			CreateCharge(job, CC1, "Charge Code 1", TestObjectCreator.AUD, 100M, Creditor1, TestObjectCreator.AUD, 150M, LocalClient);
			job.Charges[0].JR_InvoiceType = "FIN";

			Factory.Save();
			IJobCostingPlugIn[] consols = new IJobCostingPlugIn[] { consol };

			var wrapper = new ConsolBatchInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, consols, "Posting Option Name");
			Factory.Save();
			Assert(!consol.HasChanges);
			wrapper.Post();
			Assert("No changes should occur when posting a consol", !consol.HasChanges);

			ZQuery filter = new ZQuery(StmALogSchema.SL_Parent, consol.PK);
			StmALog[] retrievedLogs = Factory.Load<StmALog>(filter);

			bool isFound = false;
			ZString logReference = "FIN INV C1";
			foreach (StmALog log in retrievedLogs)
			{
				if (Events.ServiceInvoicePosted.Code == log.SL_SE_NKEvent)
				{
					isFound = true;
					AssertEquals(log.SL_Reference, logReference);
					break;
				}
			}
			Assert("LogReference should occur when posting", isFound);
		}

		public override void TestCreditLimitEmailsAreSentWhenPosting()
		{
			SetupForCreditLimitEmails();

			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();
			Factory.Save();

			ForwardingShipment shipment1 = SetupShipmentWithMultipleInvoices(TestObjectCreator.ABIGAS.PK);
			ForwardingShipment shipment2 = SetupShipmentWithMultipleInvoices(TestObjectCreator.ABIGAS.PK);

			Factory.Save();
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C1";
			consol.Shipments.Add(shipment1);
			Factory.Save();
			ForwardingConsol consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C2";
			consol.Shipments.Add(shipment2);
			Factory.Save();
			IJobCostingPlugIn[] consols = new IJobCostingPlugIn[] { consol, consol2 };

			var wrapper = new ConsolBatchInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, consols, "Posting Option Name");
			Factory.Save();
			wrapper.Post();

			AssertEquals("4 Emails should be sent", 4, Env.OutgoingMailManager.EmailsCreated.Count);
			var aPPostedInvoices = Factory.Load<TransactionHeader>(new ZQuery(AccTransactionHeaderSchema.PK, wrapper.BulkPostingDataCollector_ForTestOnly.AllAPInvoicesAndCreditNotesExcludingUAInvoicesAndUACreditNotePKs));
			var aRPostedInvoices = Factory.Load<TransactionHeader>(new ZQuery(AccTransactionHeaderSchema.PK, wrapper.BulkPostingDataCollector_ForTestOnly.AllPostedInvoicePKs));
			AssertEquals("Should have created 12 invoices", 12, aPPostedInvoices.Length + aRPostedInvoices.Length);
		}

		[TestDate(2015, 5, 10)]
		[DisableZeroExchangeRateOverriding]
		public override void TestPostWithInvoicePostingExchangeRateOption()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();

			SetupForInvoicePostingExchangeRateOption();
			var consolPK1 = CreateConsolForInvoicePostingExchangeRateOption();
			var consolPK2 = CreateConsolForInvoicePostingExchangeRateOption();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var consol1 = newFactory.Load<ForwardingConsol>(consolPK1);
			var consol2 = newFactory.Load<ForwardingConsol>(consolPK2);

			var consols = new IJobCostingPlugIn[] { consol1, consol2 };

			var wrapper = new ConsolBatchInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, consols, "Posting Option Name");
			wrapper.Post();

			AssertConsolAndAPInvoiceForInvoicePostingExchangeRateOption(consolPK1);
			AssertConsolAndAPInvoiceForInvoicePostingExchangeRateOption(consolPK2);

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		public override void TestBackDatingARAPInvoiceWithInvoicePostingExchangeRateOption()
		{
			Assert("N/a", true);
		}

		#region BackDateARInvoices

		protected override SecurityCheckpoint ModifyTransactionDateSecurity
		{
			get { return Env.Security.ConsolBulkModifyTransactionDate; }
		}

		protected override SecurityCheckpoint ModifyPostDateSecurity
		{
			get { return Env.Security.ConsolBulkModifyPostDate; }
		}

		protected override SecurityCheckpoint OverrideRequisitionDetailsSecurity
		{
			get { return Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainConsolJobInvoicing, SecurityCore.OverrideRequisitionDetails); }
		}

		#endregion

		#region Implementation

		IJobCostingPlugIn[] Consols;

		protected override void AssertParentIDIsSetToParentJob(ZString description, ZGuid parentId)
		{
			AssertNotNull(description, Consols.SingleOrDefault(x => ((ForwardingConsol)x).PK == parentId));
		}

		protected override void AssertParentTableCode(ZString description, ZString actualTableCode)
		{
			AssertEquals(description, JobConsolSchema.Constants.Prefix, actualTableCode);
		}

		new ConsolBatchInvoicingPostManagerGUIWrapper Wrapper
		{
			get { return base.Wrapper as ConsolBatchInvoicingPostManagerGUIWrapper; }
		}

		class TestConsolBatchInvoicingPostManagerGUIWrapper : ConsolBatchInvoicingPostManagerGUIWrapper
		{
			public TestConsolBatchInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption postingOption, IJobCostingPlugIn[] consolCollection, ZString selectedPostingOptionName)
				: base(postingOption, consolCollection, selectedPostingOptionName)
			{
				DoTestPostTransactions = true;
			}

			protected override void PrintInvoices(TransactionCreatorHashtable transactions)
			{
			}
		}

		#endregion
	}
}
