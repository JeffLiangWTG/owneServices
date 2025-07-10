using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.ProfitShare;
using Enterprise.Accounting.GUI.JobInvoicing.AgentPostingOptionSelection;
using Enterprise.Accounting.Integration;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.ExcelTemplates;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class ConsolInvoicingPostManagerGUIWrapper : PostManagerGUIWrapper
	{
		public ConsolInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption postingOption, BusinessObjectFactory plugInFactory, IEnumerable<Job> jobs, IJobCostingPlugIn consol, Form parentForm, ApportionmentListing consolCostListing)
			: base(null, postingOption, plugInFactory, parentForm)
		{
			this.Consol = (IJobCostingPlugIn)TransactionFactory.Load(consol.GetType(), consol.PK);
			this.Jobs = LoadJobsInNewFactory(jobs);
			this.ConsolCosts = consolCostListing.CostsCollection;
			this.ConsolCostListing = consolCostListing;
		}

		#region Posting

		readonly IJobCostingPlugIn Consol;
#pragma warning disable IDE0052 // Remove unread private members
		readonly JobConsolCostCollection ConsolCosts;
#pragma warning restore IDE0052 // Remove unread private members
		readonly ApportionmentListing ConsolCostListing;

		public ZBool SupressProfitSharePrintTask
		{
			get;
			set;
		}

		protected override BasePostManager GetNewPostManager()
		{
			return new ConsolInvoicingPostManager(TransactionFactory, Jobs, Consol, ConsolCostListing, APInvoiceApprovalGUIProvider);
		}

		protected override void SetUpEvents()
		{
			base.SetUpEvents();
			((ConsolInvoicingPostManager)PostManager).ExportAgentPosting += new ExportAgentPostingEventHandler(ConsolInvoicingPostManagerGUIWrapper_ExportAgentPosting);
			((ConsolInvoicingPostManager)PostManager).ProfitShareConfirmation += new ProfitShareConfirmationEventHandler(ConsolInvoicingPostManagerGUIWrapper_ProfitShareConfirmation);
			((ConsolInvoicingPostManager)PostManager).IncorrectRegistrySetup += new IncorrectRegistrySetupEventHandler(ConsolInvoicingPostManagerGUIWrapper_IncorrectRegistrySetup);
		}

		protected override PostManagerValidation GetPostManagerValidation(IEnumerable<Job> jobs, JobInvoicingPostingOption postingOption, IEnumerable<Job> originalJobs)
		{
			return new ConsolPostManagerValidation(jobs, Consol, postingOption, originalJobs);
		}

		protected override string GetNothingPostedMessage()
		{
			if (OverriddenNothingPostedMessage?.IsEmpty ?? true)
			{
				var extraSuggestions = PostingOption == JobInvoicingPostingOption.Gateway
					? ResString.GetMultilingualString("2ff2c211-66a6-422a-befb-9f6b973265f9", "* There are no charges where debtor is a gateway agent.")
					: null;
				return ConsolInvoicingPostManager.NoChargesToPostMessage(extraSuggestions);
			}
			else
			{
				return OverriddenNothingPostedMessage;
			}
		}

		public MultilingualString OverriddenNothingPostedMessage
		{
			get;
			set;
		}

		protected override string GetJobOnHoldMessage(IEnumerable<Job> jobs)
		{
			StringBuilder builder = new StringBuilder(ConsolInvoicingPostManager.JobOnHoldMessage);
			foreach (Job job in jobs)
			{
				builder.Append(" " + job.JH_JobNum + "\r\n");
			}
			return builder.ToString();
		}

		protected override ParentInfo GetParentInfoForPostingAction()
		{
			return new ParentInfo() { Id = Consol.CostSupporter.PK, Name = Consol.JK_UniqueConsignRef, TableCode = Consol.CostSupporter.Type };
		}

		protected override IJobInvoicingPlugIn JobInvoicingPlugIn
		{
			get { return Consol as IJobInvoicingPlugIn; }
		}

		public override void Post()
		{
			var consol = Consol as IBusiness;
			if (consol == null || (consol != null && consol.HasChanges))
			{
				Globals.Message.ShowError(Res.GetString("1693F575-38A1-4F32-907C-914F5479CA34", "Please save before posting"));
			}
			else
			{
				try
				{
					if (PostingOption == JobInvoicingPostingOption.Agent || PostingOption == JobInvoicingPostingOption.All)
					{
						TransactionFactory.SetContext(BusinessContext.PostingChargesFromConsol);
					}
					base.Post();
				}
				finally
				{
					TransactionFactory.RemoveContext(BusinessContext.PostingChargesFromConsol);
				}
			}
		}

		#region BackDateARInvoices

		protected override ChangeTransactionDatesBusinessObject GetNewChangeTransactionDatesBusinessObject(Security.SecurityCheckpoint pluginSecurity, BusinessObjectFactory factory)
		{
			return new ChangeTransactionDatesForConsolBusinessObject(pluginSecurity, factory);
		}

		protected override ChangeTransactionDatesBusinessObject GetNewChangeTransactionDatesBusinessObject(Security.SecurityCheckpoint pluginSecurity, BusinessObjectFactory factory, OperationsJobConfigurationCodes codes)
		{
			return new ChangeTransactionDatesForConsolBusinessObject(pluginSecurity, factory, codes);
		}

		#endregion

		#endregion

		#region Event Handlers

		void ConsolInvoicingPostManagerGUIWrapper_ExportAgentPosting(object sender, ExportAgentPostingEventArgs e)
		{
			if (ZFormModaliser.ShowDialogAndDispose(new AgentPostingOptionSelectionForm(e.OptionSelector)) == DialogResult.Cancel)
			{
				e.IsPostingCancelled = true;
			}
			else
			{
				e.OptionSelector.UpdateAllChargesPostingStyleAccordingToSelectedCurrency();
				e.OptionSelector.UpdateAllChargesAddressAndContactAccordingToSelectedAddressAndContact();
			}
		}

#if DEBUG
		internal
#endif
		bool ConsolInvoicingPostManagerGUIWrapper_ProfitShareConfirmation(object sender, ProfitShareConfirmationEventArgs e)
		{
			if (SupressProfitSharePrintTask)
			{
				return false;
			}
			else
			{
				foreach (ProfitShareDetail profitShare in e.CalculatedProfitShares)
				{
					DocumentPack pack = GetProfitShareDocumentPackForOrg(ProfitSharePrintTask, profitShare.ProfitShareParty);
					DocumentWrapper wrapper = DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.ProfitShareDetail, profitShare);
					((IBODocDataProvider)wrapper).SetDocWrapperContext(new Dictionary<string, object>
																	{
																		{ DocumentEngineIntegration.Constants.TemplateDefined.ContactType, ProfitShareMenuItem.SU_ContactType },
																		{ DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, ProfitShareMenuItem.SU_MenuName },
																	});
					Report document = new Report(pack, ProfitShareExcelTemplate, wrapper, ProfitShareExcelTemplate.TemplateName, null, DocumentDirection.ANY, false);
					pack.Add(document);
				}
				DeliveryInstructionDestination destination = ProfitSharePrintTask.Run(AllowedDeliveryOptions.All, Env.Security.None);
				fProfitSharePrintTask = null;
				DocPackOrgDictionary.Clear();

				return destination != DeliveryInstructionDestination.UserCancelled;
			}
		}

		void ConsolInvoicingPostManagerGUIWrapper_IncorrectRegistrySetup(object sender, IncorrectRegistrySetupEventArgs e)
		{
			Globals.Message.ShowError(Res.GetString("584a0d2a-e7cf-4180-b065-61720fbd679c", "Incorrect a Registry Item value.\r\nPlease set up a correct value of the Registry Item: '{0}'.", e.RegistyItemLocation));
		}

		#endregion

		#region Printing

		protected override bool AllowDeliveryDuringPreviewInvoice
		{
			get { return false; }
		}

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

		DocumentPack GetProfitShareDocumentPackForOrg(PrintTask task, OrgHeader organisation)
		{
			DocumentPack pack = null;
			if (!DocPackOrgDictionary.TryGetValue(organisation, out pack))
			{
				pack = new DocumentPack(ProfitShareMenuItem);
				pack.DocumentSupporter = Consol.CostSupporter.DocumentSupporter;
				pack.Organisation = organisation;
				pack.OrgHeaderContact = new OrgHeaderContact(organisation, null);
				pack.ForceBusinessObjectToLogAgainst(Consol as BusinessObject);

				DocPackOrgDictionary.Add(organisation, pack);
				task.Add(pack);
			}

			return pack;
		}

		readonly Dictionary<OrgHeader, DocumentPack> DocPackOrgDictionary = new Dictionary<OrgHeader, DocumentPack>();

		DocumentCommand ProfitShareMenuItem
		{
			get
			{
				if (fProfitShareMenuItem == null)
				{
					fProfitShareMenuItem = new BusinessObjectFactory().New<DocumentCommand>();
					fProfitShareMenuItem.SU_MenuName = Res.GetString("Accounting|ConsolInvoicingPostManagerGUIWrapper|ProfitShareCalculationMenu", "Profit Share Calculation");
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
