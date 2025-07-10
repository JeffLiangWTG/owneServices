using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.ProfitShare;
using Enterprise.Accounting.GUI.JobInvoicing.AgentPostingOptionSelection;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.ExcelTemplates;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.JobInvoicing.BatchPosting
{
	public partial class ConsolBatchInvoicingPostManagerGUIWrapper : BaseBatchInvoicingPostManagerGUIWrapper
	{
		public ConsolBatchInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption postingOption,
			IJobCostingPlugIn[] consolCollection,
			ZString selectedPostingOptionName)
			: base(postingOption, selectedPostingOptionName)
		{
			this.ConsolCollection = consolCollection;
		}

		readonly IJobCostingPlugIn[] ConsolCollection;

		#region Posting

		protected override BasePostManager GetNewPostManager()
		{
			return new ConsolInvoicingPostManager(TransactionFactory, Jobs, CurrentConsol, GetConsolCostListing(ConsolDataRetriever), APInvoiceApprovalGUIProvider);
		}

		protected override void SetUpEvents()
		{
			base.SetUpEvents();
			((ConsolInvoicingPostManager)PostManager).ExportAgentPosting += new ExportAgentPostingEventHandler(ConsolInvoicingPostManagerGUIWrapper_ExportAgentPosting);
			((ConsolInvoicingPostManager)PostManager).ProfitShareConfirmation += new ProfitShareConfirmationEventHandler(ConsolInvoicingPostManagerGUIWrapper_ProfitShareConfirmation);
		}

		void ConsolInvoicingPostManagerGUIWrapper_ExportAgentPosting(object sender, ExportAgentPostingEventArgs e)
		{
			DialogResult result = ZFormModaliser.ShowDialogAndDispose(new AgentPostingOptionSelectionForm(e.OptionSelector));

			if (result == DialogResult.Cancel)
			{
				e.IsPostingCancelled = true;
			}
			else
			{
				e.OptionSelector.UpdateAllChargesPostingStyleAccordingToSelectedCurrency();
				e.OptionSelector.UpdateAllChargesAddressAndContactAccordingToSelectedAddressAndContact();
			}
		}

		protected override PostManagerValidation GetPostManagerValidation(IEnumerable<Job> jobs, JobInvoicingPostingOption postingOption, IEnumerable<Job> originalJobs)
		{
			return new ConsolPostManagerValidation(jobs, CurrentConsol, postingOption, originalJobs, isBulkPosting: true);
		}

		public override void Post()
		{
			foreach (IJobCostingPlugIn consol in ConsolCollection)
			{
				RenewTransactionFactory();
				CurrentConsol = (IJobCostingPlugIn)TransactionFactory.Load(consol.GetType(), consol.PK);
				ConsolDataRetriever = new JobCostingPlugInDataRetriever(CurrentConsol, TransactionFactory);
				PostSingleObject(ConsolDataRetriever.Jobs);
				if (ShouldStopPostingOnNextObject)
				{
					break;
				}
			}

			RaiseOnPostingFinished();
		}

		IJobCostingPlugIn CurrentConsol;

		JobCostingPlugInDataRetriever ConsolDataRetriever;

		ApportionmentListing GetConsolCostListing(JobCostingPlugInDataRetriever consolDataRetriever)
		{
			ApportionmentListing fApportionments = consolDataRetriever.GetApportionments();

			SecurityOverrideProviderSource.Get(fApportionments).Provider = new ApportionmentSecurityOverrideProvider(fApportionments);
			return fApportionments;
		}

		protected override IJobInvoicingPlugIn JobInvoicingPlugIn
		{
			get { return CurrentConsol as IJobInvoicingPlugIn; }
		}

		#region BackDateARInvoices

		protected override ChangeTransactionDatesBusinessObject GetNewChangeTransactionDatesBusinessObject(SecurityCheckpoint pluginSecurity, BusinessObjectFactory factory)
		{
			return new ChangeTransactionDatesForConsolBatchPostingBusinessObject(pluginSecurity, factory);
		}

		protected override ChangeTransactionDatesBusinessObject GetNewChangeTransactionDatesBusinessObject(SecurityCheckpoint pluginSecurity, BusinessObjectFactory factory, OperationsJobConfigurationCodes codes)
		{
			return new ChangeTransactionDatesForConsolBatchPostingBusinessObject(pluginSecurity, factory, codes);
		}

		#endregion

		#endregion

		#region Implementation

		protected override ZString CurrentObjectCode
		{
			get { return CurrentConsol != null ? CurrentConsol.JK_UniqueConsignRef : (ZString)""; }
		}

		public override ZInt NumberOfObjectsToPost
		{
			get { return ConsolCollection.Length; }
		}

		protected override ZString DefaultPostedObjectName
		{
			get { return Res.GetString("Accounting|ConsolBatchInvoicingPostManager|DefaultPostedObjectName", "Consol"); }
		}

		protected override ParentInfo GetParentInfoForPostingAction()
		{
			return new ParentInfo() { Id = CurrentConsol.CostSupporter.PK, Name = CurrentConsol.JK_UniqueConsignRef, TableCode = CurrentConsol.CostSupporter.Type };
		}

		#endregion

		#region Message

		protected override string GetNothingPostedMessage()
		{
			var zeroBalanceWarningMsg = AccountingConfigurationRegistry.Instance.AllowZeroValueARInvoices.Value
				? string.Empty
				: System.Environment.NewLine + Res.GetString("1cc2de7c-69f3-42c4-8ba8-c5cbc9be25b7", "* Total of the charges being posted is zero and your system is configured to disallow zero value invoices.");

			return Res.GetString("f10d3400-23cc-4ef1-92b4-cef59c4eeed8", @"No appropriate charges were found for posting. This may be because:{0}
* All appropriate charges were to be appended to an existing transaction, and you elected to skip them.
* Charges pertaining to existing transactions contain differing AP details or currencies, and so cannot be appended.
* You are trying to post revenue but a shipment has invoicing on hold.
* You are trying to post revenue / cost but a shipment has Ready For Financial Closure status.
You may want to check the data you entered on Job Invoicing tabs on each shipment attached to this consol, and on the Costing tab of this form. Make sure that amounts are not zero and all appropriate information is entered for AP Invoices (if applicable).",
				zeroBalanceWarningMsg);
		}

		#endregion

		#region Printing

		PrintTask ProfitSharePrintTask
		{
			get
			{
				if (fProfitSharePrintTask == null)
				{
					fProfitSharePrintTask = new PrintTask(ProfitShareMenuItem);
				}

				return fProfitSharePrintTask;
			}
		}

		PrintTask fProfitSharePrintTask;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Hard coded constant")]
		ExcelTemplate ProfitShareExcelTemplate
		{
			get
			{
				if (fProfitShareExcelTemplate == null)
				{
					StmTemplate template = TransactionFactory.LoadTop1<StmTemplate>(new ZQuery(StmTemplateSchema.SO_Name, "Profit Share Calculation Worksheet"));
					fProfitShareExcelTemplate = new ExcelTemplateReadFromStmTemplateTable(template);
				}

				return fProfitShareExcelTemplate;
			}
		}

		ExcelTemplate fProfitShareExcelTemplate;

		DocumentPack GetDocumentPackForOrg(PrintTask task, DocumentCommand menuItem, OrgHeader organisation)
		{
			DocumentPack pack = (DocumentPack)DocPackOrgsHash[organisation];
			if (pack == null)
			{
				pack = new DocumentPack(menuItem);
				pack.DocumentSupporter = ((ForwardingConsol)CurrentConsol).DocumentSupporter;
				pack.Organisation = organisation;
				pack.OrgHeaderContact = new OrgHeaderContact(organisation, null);
				pack.ForceBusinessObjectToLogAgainst((ForwardingConsol)CurrentConsol);

				DocPackOrgsHash[organisation] = pack;
				task.Add(pack);
			}

			return pack;
		}

		Hashtable DocPackOrgsHash = new Hashtable();

		bool ConsolInvoicingPostManagerGUIWrapper_ProfitShareConfirmation(object sender, ProfitShareConfirmationEventArgs e)
		{
			foreach (ProfitShareDetail profitShare in e.CalculatedProfitShares)
			{
				DocumentPack pack = GetDocumentPackForOrg(ProfitSharePrintTask, ProfitShareMenuItem, profitShare.ProfitShareParty);
				DocumentWrapper wrapper = DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.ProfitShareDetail, profitShare);
				Report document = new Report(pack, ProfitShareExcelTemplate, wrapper, ProfitShareExcelTemplate.TemplateName, null, DocumentDirection.ANY, false);
				pack.Add(document);
			}

			DeliveryInstructionDestination destination = ProfitSharePrintTask.Run(AllowedDeliveryOptions.All, Env.Security.None);
			fProfitSharePrintTask = null;
			DocPackOrgsHash = new Hashtable();

			return destination != DeliveryInstructionDestination.UserCancelled;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Hard coded menu name")]
		DocumentCommand ProfitShareMenuItem
		{
			get
			{
				if (fProfitShareMenuItem == null)
				{
					fProfitShareMenuItem = new BusinessObjectFactory().New<DocumentCommand>();
					fProfitShareMenuItem.SU_MenuName = "Profit Share Calculation";
					fProfitShareMenuItem.SU_ContactType = ContactType.Receivables.Code;
					fProfitShareMenuItem.SU_PreventAutoDelivery = false;
					fProfitShareMenuItem.SU_IsSystemDefined = true;
					fProfitShareMenuItem.SU_SupportsVisualisation = false;
					fProfitShareMenuItem.SU_IsModifiable = false;
				}

				return fProfitShareMenuItem;
			}
		}

		DocumentCommand fProfitShareMenuItem;

		#endregion
	}
}
