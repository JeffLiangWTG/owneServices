using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.ConsolRevenue;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.ARAP.UnapprovedAPTransaction;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI.RateSelector.Services;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting
{
	public partial class ApportionmentPlugin : ZAlwaysLoadPlugIn, IPluginForGatewaySellApportionments
	{
		public ApportionmentPlugin(IBusiness hostEntity)
			: base(hostEntity)
		{
			Consol = hostEntity as IJobCostingPlugIn;
			if (Consol == null)
			{
				throw new NotSupportedException("Host entity must implement IJobCostingPlugin");
			}

			InitializePreviousConsolDates();
			costsWithSuspendedValidation = new HashSet<JobConsolCost>();
		}

		protected readonly IJobCostingPlugIn Consol;
		readonly HashSet<JobConsolCost> costsWithSuspendedValidation;

		#region Setup

		public override ZString PlugInNotDisplayedMessage
		{
			get { return !ApportionmentPlugInNotDisplayedMessage.IsEmpty ? ApportionmentPlugInNotDisplayedMessage : base.PlugInNotDisplayedMessage; }
		}

		ZString ApportionmentPlugInNotDisplayedMessage;

		public static string LabelMessage
		{
			get { return Res.GetString("7f75b322-dbb3-4a97-816a-28693e195fe0", "Some shipments for costs on this consol have apportioned costs, but are no longer attached to this consol.\r\n\r\nReview the following list of shipments. You will need to reattach those shipments indicated and review the costs:") + "\r\n"; }
		}

		public override string Name
		{
			get { return Res.GetString("e3ac1c74-6ebd-4dab-8b8f-f6ed55cc5b32", "Consol Costing"); }
		}

		public virtual ResourceString MenuName => ResString.GetMultilingualString("Accounting.JobInvoicing", "&Job Invoicing");

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Accountant; }
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return Apportionments;
		}

		protected override void HookFormEventsCore()
		{
			base.HookFormEventsCore();

			Factory.Saved += new BusinessObjectFactory.SavedEventHandler(ConsolFactory_Saved);
		}

		protected override void UnHookFormEventsCore()
		{
			base.UnHookFormEventsCore();

			Factory.Saved -= new BusinessObjectFactory.SavedEventHandler(ConsolFactory_Saved);
		}

		public override void OnSaving()
		{
			base.OnSaving();

			HandleExRateUpdate();
		}

		void HandleExRateUpdate()
		{
			CommonConsol commonConsol = Consol as CommonConsol;
			if (commonConsol != null)
			{
				var arrivalTransport = commonConsol.Transports.ArrivalTransport;
				var departureTransport = commonConsol.Transports.DepartureTransport;
				if (PreviousConsolATA != arrivalTransport?.JW_ATA ||
					PreviousConsolETA != arrivalTransport?.JW_ETA ||
					PreviousConsolATD != departureTransport?.JW_ATD ||
					PreviousConsolETD != departureTransport?.JW_ETD)
				{
					bool shouldPropmtToUpdateExRate = false;
					bool isAnyExRateExist = false;
					foreach (Job job in Jobs)
					{
						if (!shouldPropmtToUpdateExRate)
						{
							shouldPropmtToUpdateExRate = AccExchangeRateConfigurationRateFinder.GetExchangeRateConfigurationPromptAtCompanyLevel(job.ExchangeRateConfigurationRateConsumer);
						}
						if (job.ExchangeRates.Count > 0)
						{
							isAnyExRateExist = true;
						}
						if (shouldPropmtToUpdateExRate && isAnyExRateExist)
						{
							break;
						}
					}
					if (shouldPropmtToUpdateExRate && isAnyExRateExist &&
						Globals.Message.Show(Res.GetString("3267E075-C80F-44bb-B763-73A8077FAA65", @"Departure or Arrival dates have been changed.
Do you want to re-default the exchange rates on the shipment billing tabs?"), Res.GetString("A6C05283-3040-4e48-AB8F-CA050C9F56E7", "Confirm Exchange Rates re-defaulting"),
							MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						MenuItemRedefaultExRate_Click(null, EventArgs.Empty);
					}
				}
			}
		}

		void ConsolFactory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			UpdatePreviousConsolDates();
		}

		void InitializePreviousConsolDates()
		{
			var commonConsol = Consol as CommonConsol;
			if (commonConsol != null)
			{
				var arrivalTransport = commonConsol.Transports.ArrivalTransport;
				var departureTransport = commonConsol.Transports.DepartureTransport;
				if (arrivalTransport != null)
				{
					PreviousConsolETA = arrivalTransport.PreviousJW_ETA;
					PreviousConsolATA = arrivalTransport.PreviousJW_ATA;
				}

				if (departureTransport != null)
				{
					PreviousConsolETD = departureTransport.PreviousJW_ETD;
					PreviousConsolATD = departureTransport.PreviousJW_ATD;
				}
			}
		}

		void UpdatePreviousConsolDates()
		{
			var commonConsol = Consol as CommonConsol;
			if (commonConsol != null)
			{
				var arrivalTransport = commonConsol.Transports.ArrivalTransport;
				var departureTransport = commonConsol.Transports.DepartureTransport;
				if (arrivalTransport != null)
				{
					PreviousConsolETA = arrivalTransport.JW_ETA;
					PreviousConsolATA = arrivalTransport.JW_ATA;
				}

				if (departureTransport != null)
				{
					PreviousConsolETD = departureTransport.JW_ETD;
					PreviousConsolATD = departureTransport.JW_ATD;
				}
			}
		}

		ZDateTime PreviousConsolETA;
		ZDateTime PreviousConsolETD;
		ZDateTime PreviousConsolATA;
		ZDateTime PreviousConsolATD;

		#endregion

		#region Security

		SecurityCheckpoint PluginSecurity
		{
			get
			{
				return PlugInParent != null
					? PlugInParent.InvoicingSupporter.JobInvoicingSecurity
					: Env.Security.MaintainConsolJobInvoicing;
			}
		}

		JobInvoicingSecurityHelper SecurityHelper
		{
			get { return fSecurityHelper ?? (fSecurityHelper = new JobInvoicingSecurityHelper(() => PluginSecurity)); }
		}
		JobInvoicingSecurityHelper fSecurityHelper;

		JobInvoicingSecurityHelper MenuSecurityHelper
		{
			get { return fmenuSecurityHelper ?? (fmenuSecurityHelper = new JobInvoicingSecurityHelper(() => MenuSecurity)); }
		}
		JobInvoicingSecurityHelper fmenuSecurityHelper;

		SecurityCheckpoint MenuSecurity
		{
			get
			{
				return Env.Security.MaintainConsolJobInvoicing;
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (fApportionments != null)
				{
					if (Apportionments.IsActivated)
					{
						Apportionments.ReleaseMutexes();
					}

					Apportionments.OnReplaceShipmentExchangeRate -= new EventHandler<ReplaceShipmentExchangeRateEventArgs>(fApportionments_OnReplaceShipmentExchangeRate);

					SecurityOverrideProviderSource.Get(Apportionments).Provider = null;
				}
			}

			base.Dispose(disposing);
		}

		#endregion

		#region Apportionments

		ApportionmentListing fApportionments;
		public ApportionmentListing Apportionments
		{
			get
			{
				var isForGatewayApportionments = CheckIsUsedForGatewayApportionments();
				if (fApportionments == null)
				{
					fApportionments = Consol.GetApportionments(isForGatewayApportionments, new JobCreationErrorHandlerForApportionmentPluginThatDoesNothing());
					SecurityOverrideProviderSource.Get(fApportionments).Provider = new ApportionmentSecurityOverrideProvider(Apportionments);
					fApportionments.OnReplaceShipmentExchangeRate += new EventHandler<ReplaceShipmentExchangeRateEventArgs>(fApportionments_OnReplaceShipmentExchangeRate);
					fApportionments.IsAllowOverrideBaseExchangeRate = SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.AllowOverrideBaseExchangeRate);
				}
				return fApportionments;
			}
		}

		public virtual bool CheckIsUsedForGatewayApportionments()
		{
			return false;
		}

		void fApportionments_OnReplaceShipmentExchangeRate(object sender, ReplaceShipmentExchangeRateEventArgs e)
		{
			ZString sipmentExchangeRateReplaceQuestion = Res.GetString("2fbdb63e-4c6c-475b-a3dd-3605cf7bc1e0", "Do you want to update all exchange rates on the consol jobs with the exchange rate you have entered?");
			DialogResult result = Globals.Message.Show(sipmentExchangeRateReplaceQuestion, Res.GetString("fa12f206-c5a0-47c4-a4d5-c9c5530f29d9", "Shipment Exchange Rate Replacing"), MessageBoxButtons.YesNo,
				MessageBoxIcon.Question, DialogResult.No);

			e.DoesReplaceShipmentExchangeRate = result == DialogResult.Yes;
		}

		#endregion

		#region GUI

		public override void OnGUIShown()
		{
			base.OnGUIShown();
			DoWarningOnlyValidationOnCosts();
		}

		void DoWarningOnlyValidationOnCosts()
		{
			if (!IsGUIShown)
			{
				Apportionments.Factory.SetContext(BusinessContext.WarningOnlyValidation);
				try
				{
					foreach (var cost in Apportionments.CostsCollection.ToArray<JobConsolCost>())
					{
						cost.Validation.ValidateE6_OSGSTAmount_Calc();
					}
					IsGUIShown = true;
				}
				finally
				{
					Apportionments.Factory.RemoveContext(BusinessContext.WarningOnlyValidation);
				}
			}
		}

		public override void OnBusinessObjectIsCancelledChanged(ZBool isCancelled)
		{
			base.OnBusinessObjectIsCancelledChanged(isCancelled);

			if (isCancelled)
			{
				DeleteUnpostedCosts();
			}
		}

		void DeleteUnpostedCosts()
		{
			var toBeRemoved = new ArrayList();

			foreach (JobConsolCost consolCost in Apportionments.CostsCollection)
			{
				if (!consolCost.ApportionmentCharges.Any())
				{
					toBeRemoved.Add(consolCost);
				}
			}

			using (Apportionments.CostsCollection.SuspendListChanged())
			{
				foreach (JobConsolCost consolCost in toBeRemoved)
				{
					Apportionments.CostsCollection.RemoveAndDelete(consolCost);
				}
			}
		}

		bool IsGUIShown { get; set; }

		protected override bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			bool result = true;
			if (Globals.IsUserInteractive)
			{
				try
				{
					Apportionments.PrepareForConsolCosting();
				}
				catch (JobCreationException e)
				{
					ApportionmentPlugInNotDisplayedMessage = e.Message;
					result = false;
				}
				catch (InvalidOperationException ex)
				{
					ApportionmentPlugInNotDisplayedMessage = ex.Message;
					result = false;
				}

				if (result)
				{
					if (!AccountingConfigurationRegistry.Instance.EnableLightValidationForChargeAndConsolCost.Value)
					{
						foreach (var cost in costsWithSuspendedValidation)
						{
							cost.ResumeValidation();
						}
						costsWithSuspendedValidation.Clear();
					}
					else
					{
						Apportionments.MarkAsNeedingValidationIncludingChildren();
					}

					var currentCostCollectionReadOnly = Apportionments.CostsCollection.ReadOnly;

					if (previousCostCollectionReadOnly != currentCostCollectionReadOnly)
					{
						previousCostCollectionReadOnly = currentCostCollectionReadOnly;
						Apportionments.CostsCollection.RefreshBinding();
					}
				}
				else if (!AccountingConfigurationRegistry.Instance.EnableLightValidationForChargeAndConsolCost.Value)
				{
					foreach (JobConsolCost cost in Apportionments.CostsCollection)
					{
						cost.SuspendValidation();
						costsWithSuspendedValidation.Add(cost);
					}
				}
			}

			return result;
		}

		bool previousCostCollectionReadOnly;

		protected override bool ShouldPlugInGUIAndBusinessEntityBeCreatedCore()
		{
			return false;
		}

		public override ContinueWithSave ShowPreSaveDialogsCore()
		{
			ContinueWithSave saveResult = base.ShowPreSaveDialogsCore();
			if (saveResult == ContinueWithSave.Yes)
			{
				try
				{
					if ((Apportionments.IsActivated || Apportionments.HasChanges) && !Apportionments.ReopenClosedJobs())
					{
						saveResult = ContinueWithSave.No;
					}
				}
				catch (JobCreationException ex)
				{
					Globals.Message.ShowError(ex.Message, Res.GetString("19e0266e-70ec-44ec-b897-b9417af47593", "Unable to save"));
					saveResult = ContinueWithSave.No;
				}
			}
			return saveResult;
		}

		protected override MenuItem GetNewTopLevelMenu()
		{
			if (MainMenuItem == null)
			{
				MainMenuItem = new ZMenuItem(MenuName);

				var defaultARInvoiceDateMenuCaption = ARDefaultInvoiceAndPostDateCalculator.GetDefaultDateMenuCaption();
				if (defaultARInvoiceDateMenuCaption != null)
				{
					MainMenuItem.MenuItems.Add(new ZMenuItem(defaultARInvoiceDateMenuCaption));
				}

				if (IsFormEditable)
				{
					MenuItem menuItemAutoRateCosts = new ZMenuItem(ResString.GetMultilingualString("Accounting.JobInvoicing.AutorateCosts", "Autorate Costs"));
					menuItemAutoRateCosts.Click += MenuItemAutoRateCosts_Click;
					MainMenuItem.MenuItems.Add(menuItemAutoRateCosts);

					MenuItem menuItemAutoRateCostsAndRevenue = new ZMenuItem(ResString.GetMultilingualString("Accounting.JobInvoicing.AutorateCostsRevenue", "Autorate Costs and Revenue"));
					menuItemAutoRateCostsAndRevenue.Click += MenuItemAutoRateCostsAndRevenue_Click;
					MainMenuItem.MenuItems.Add(menuItemAutoRateCostsAndRevenue);

					MainMenuItem.MenuItems.Add(new ZMenuItem("-"));
				}

				var menuItemPostGateway = new ZMenuItem(Constants.MenuNameConstants.PostGatewayAgentCharges);
				menuItemPostGateway.Visible = false;
				menuItemPostGateway.Click += new EventHandler(MenuItemPostGateway_Click);
				MainMenuItem.MenuItems.Add(menuItemPostGateway);

				if (!IsSpecifiedNotToAddMenuItem)
				{
					MenuItem menuItemPostAgent = new ZMenuItem(Core.Constants.MenuNameConstants.PostOverseasAgentCharges);
					menuItemPostAgent.Click += new EventHandler(MenuItemPostAgent_Click);
					MainMenuItem.MenuItems.Add(menuItemPostAgent);
				}

				MenuItem menuItemPostAllCosts = new ZMenuItem(Core.Constants.MenuNameConstants.PostAllCosts);
				menuItemPostAllCosts.Click += new EventHandler(MenuItemPostConsolCosts_Click);
				MainMenuItem.MenuItems.Add(menuItemPostAllCosts);

				MenuItem menuItemPostConsolCostsOnly = new ZMenuItem(Core.Constants.MenuNameConstants.PostConsolCostsOnly);
				menuItemPostConsolCostsOnly.Click += new EventHandler(MenuItemPostConsolCostsOnly_Click);
				MainMenuItem.MenuItems.Add(menuItemPostConsolCostsOnly);

				MenuItem menuItemPostBoth = new ZMenuItem(Core.Constants.MenuNameConstants.PostWholeConsol);
				menuItemPostBoth.Click += new EventHandler(MenuItemPostConsol_Click);
				MainMenuItem.MenuItems.Add(menuItemPostBoth);

				if (!IsSpecifiedNotToAddMenuItem)
				{
					MenuItem menuItemApportionRevenue = new ZMenuItem(Core.Constants.MenuNameConstants.ApportionRevenueToShipments);
					menuItemApportionRevenue.Click += delegate { ApportionRevenueToShipments(); };
					MainMenuItem.MenuItems.Add(menuItemApportionRevenue);

					MainMenuItem.MenuItems.Add(new ZMenuItem("-"));
					MenuItem previewInvoices = new ZMenuItem(Core.Constants.MenuNameConstants.PreviewInvoices);
					previewInvoices.Click += new EventHandler(PreviewInvoices_Click);
					MainMenuItem.MenuItems.Add(previewInvoices);
				}

				MenuItem previewCosts = new ZMenuItem(Core.Constants.MenuNameConstants.PreviewCosts);
				previewCosts.Click += new EventHandler(PreviewCosts_Click);
				MainMenuItem.MenuItems.Add(previewCosts);

				if (!IsSpecifiedNotToAddMenuItem)
				{
					MainMenuItem.MenuItems.Add(new ZMenuItem("-"));
					MenuItem menuItemImportAPInvoices = new ZMenuItem(Core.Constants.MenuNameConstants.ImportAPInvoicesIssuedByOtherGroupCompanies);
					menuItemImportAPInvoices.Click += new EventHandler(MenuItemImportAPInvoices_Click);
					MainMenuItem.MenuItems.Add(menuItemImportAPInvoices);

					MainMenuItem.MenuItems.Add(new ZMenuItem("-"));

					MenuItem menuItemRedefaultExRate = new ZMenuItem(Core.Constants.MenuNameConstants.RedefaultJobBillingExchangeRate);
					menuItemRedefaultExRate.Click += new EventHandler(MenuItemRedefaultExRate_Click);
					MainMenuItem.MenuItems.Add(menuItemRedefaultExRate);

					var menuItemResetTaxDefaults = new ZMenuItem(Constants.MenuNameConstants.ResetUnpostedLinesTaxDefault);
					menuItemResetTaxDefaults.Click += new EventHandler(MenuItemResetTaxDefaults_Click);
					MainMenuItem.MenuItems.Add(menuItemResetTaxDefaults);
				}

				MenuItem menuItemSynchroniseInvoiceDetails = new ZMenuItem(ResString.GetMultilingualString("Accounting.JobInvoicing.SynchroniseInvoiceDetails", "Synchronize Cost Invoice Details"));
				menuItemSynchroniseInvoiceDetails.Click += new EventHandler(MenuItemSynchroniseInvoiceDetails_Click);
				MainMenuItem.MenuItems.Add(menuItemSynchroniseInvoiceDetails);

				var eInvoicingGUIActionHelper = new EInvoicingGUIActionHelper(() => null);
				MainMenuItem.MenuItems.AddIfNotNull(eInvoicingGUIActionHelper.GetPenaltyTaxInfoActionMenuItem());
			}
			return MainMenuItem;
		}

		public virtual bool IsSpecifiedNotToAddMenuItem => false;

		void ApportionRevenueToShipments()
		{
			if (((IBusiness)Consol).HasChanges || !((IBusiness)Consol).IsInDatabase)
			{
				Globals.Message.ShowError(Res.GetString("317ad56e-f832-473d-a267-fc4af85bd8d7", "Please save before apportioning."));
			}
			else
			{
				if (!MenuSecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.ApportionRevenueToShipment))
				{
					MenuSecurityHelper.ShowError(SecurityCore.ApportionRevenueToShipment);
				}
				else
				{
					var newFactory = new BusinessObjectFactory();
					var newConsol = newFactory.Load(BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(Consol.CostSupporter.Type), Consol.PK.ToGuid());
					using (var master = new ConsolRevenueMaster(newConsol as IJobCostingPlugIn, newFactory))
					{
						ZFormModaliser.ShowDialogAndDispose(new ConsolRevenueApportionForm(master));
					}
				}
			}
		}

		void MenuItemImportAPInvoices_Click(object sender, EventArgs e)
		{
			string consolSaved = string.Empty;
			if (Consol != null && (Consol.CostSupporter.HasChanges || !Consol.CostSupporter.IsInDatabase))
			{
				consolSaved = Res.GetString("92625959-fcc4-4d3f-a128-ed45f7a38ee6", "Please save this form before posting costs and/or charges.") + "\n";
			}

			if (consolSaved.Length == 0)
			{
				var jobUniqueRefQuery = new ZQuery(AccTransactionHeaderSchema.AH_JobNumber, Consol.JK_UniqueConsignRef);

				var gatewayConsolInvoiceQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));
				gatewayConsolInvoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_JH, null);
				var jobHeaderSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.PK);
				jobHeaderSubQuery.AddToFilter(JobHeaderSchema.JH_ParentID, SQLComparisonOperator.Equal, Consol.PK);
				gatewayConsolInvoiceQuery.AddSubQuery(AccTransactionHeaderSchema.AH_JH, jobHeaderSubQuery, JoinCondition.Or);

				jobUniqueRefQuery.AddToFilter(gatewayConsolInvoiceQuery, JoinCondition.And);

				var factory = new BusinessObjectFactory();
				var additionalFiltersForConverter = new ZQuery(jobUniqueRefQuery);
				ZDBOnlyQuery gatewayTargetJobQuery;
				if (PlugInParent is ForwardingConsol forwardingConsol
					&& (gatewayTargetJobQuery = GatewayTargetJobQueryCreator.CreateQueryForInvoiceTargetJob(factory, forwardingConsol)) != null)
				{
					additionalFiltersForConverter.AddToFilter(gatewayTargetJobQuery, JoinCondition.Or);
				}

				var converter = new UnapprovedTransactionConverter(factory, additionalFiltersForConverter);
				ZFormModaliser.ShowDialogAndDispose(new UnapprovedTransactionAuthorisationForm(converter));
			}
			else
			{
				Globals.Message.ShowError(consolSaved);
			}
		}

		MenuItem MainMenuItem;

		void MenuItemAutoRateCostsAndRevenue_Click(object sender, EventArgs e)
		{
			AutoRate(true);
		}

		void MenuItemAutoRateCosts_Click(object sender, EventArgs e)
		{
			AutoRate(false);
		}

		void AutoRate(bool autorateRevenue)
		{
			if (!MenuSecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.ConsolInvAutoRate))
			{
				MenuSecurityHelper.ShowError(SecurityCore.ConsolInvAutoRate);
				return;
			}

			var autoRateOptions = new AutoRateOptions
			(
				autoRateRevenue: autorateRevenue,
				autoRateCost: true,
				triggerSource: AutoRateTriggerSource.Menu,
				billingType: BillingType.Apportionment
			);

			var additionalJobsAction = autorateRevenue
				? AdditionalJobsAction.AutoRateAdditionalInvoicingJobs
				: AdditionalJobsAction.LoadOrCreateInvoicingJobs;

#if DEBUG
			var dialogService = DialogService_ForTestOnly;
			if (dialogService == null && Form != null)
			{
				dialogService = new DialogService(Form);
			}
#else
			var dialogService = (IDialogService)null;
			if (Form != null)
			{
				dialogService = new DialogService(Form);
			}
#endif

			var guiInteractor = new AutoRatingGUIInteractor(UserControl);
			var ratingContext = DataRegistryRating.Instance.RatesServiceRateSelector.IsAllowed()
				? RatingContext.CreateForManualSelect(guiInteractor, dialogService)
				: new RatingContext(guiInteractor, dialogService);

			var autoRatingStarter = new AutoRatingStarter(new[] { (IBusiness)Consol }, ratingContext, additionalJobsAction);

			try
			{
				autoRatingStarter.ExecuteAutorating(autoRateOptions, isSaveCalledManuallyAfterSession: true);
			}
			catch (AutoRater.RatingCancelledException)
			{
				// User cancelled. No need further action.
			}
		}

#if DEBUG
		public IDialogService DialogService_ForTestOnly { get; set; }
#endif

		void PreviewTransactions(JobInvoicingPostingOption previewOption)
		{
			IJobCostingPlugIn forwardingConsol = HostBusinessEntity as IJobCostingPlugIn;
			try
			{
				var wrapper = GetPostManagerGUIWrapper(previewOption, HostBusinessEntity.Factory, Jobs, forwardingConsol, TabPage.FindForm(), Apportionments);
				wrapper.SupressProfitSharePrintTask = true;
				wrapper.AllowedReportDeliveryOption = Enterprise.DocumentEngine.AllowedDeliveryOptions.PreviewOnly;
				wrapper.Preview();
			}
			catch (JobCreationException ex)
			{
				Globals.Message.ShowError(ex.Message, Res.GetString("e638131a-b968-4130-a3b6-001a56467b7b", "Unable to preview"));
			}
		}

		void PreviewInvoices_Click(object sender, EventArgs e)
		{
			if (!Consol.CostSupporter.IsInDatabase || Consol.CostSupporter.HasChanges)
			{
				Globals.Message.ShowError(Res.GetString("93d82c7e-b031-4191-8c93-bef087d244fb", "Please save this form before previewing Invoices."));
			}
			else if (!MenuSecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.ConsolInvPreviewInvoices))
			{
				MenuSecurityHelper.ShowError(SecurityCore.ConsolInvPreviewInvoices);
			}
			else
			{
				PreviewTransactions(JobInvoicingPostingOption.All);
			}
		}

		void PreviewCosts_Click(object sender, EventArgs e)
		{
			if (!Consol.CostSupporter.IsInDatabase || Consol.CostSupporter.HasChanges)
			{
				Globals.Message.ShowError(Res.GetString("a7d99b27-556a-42eb-b553-5d8661062244", "Please save this form before previewing Cost Confirmation Documents."));
			}
			else if (!MenuSecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.ConsolInvPreviewInvoices))
			{
				MenuSecurityHelper.ShowError(SecurityCore.ConsolInvPreviewInvoices);
			}
			else
			{
				PreviewTransactions(JobInvoicingPostingOption.ConsolCosts);
			}
		}

		void MenuItemPostGateway_Click(object sender, EventArgs e)
		{
			if (!MenuSecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.ConsolInvPostGateway))
			{
				MenuSecurityHelper.ShowError(SecurityCore.ConsolInvPostGateway);
			}
			else
			{
				PostTransactions(JobInvoicingPostingOption.Gateway);
			}
		}

		void MenuItemPostAgent_Click(object sender, EventArgs e)
		{
			if (!MenuSecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.ConsolInvPostAgent))
			{
				MenuSecurityHelper.ShowError(SecurityCore.ConsolInvPostAgent);
			}
			else
			{
				PostTransactions(JobInvoicingPostingOption.Agent);
			}
		}

		void MenuItemPostConsol_Click(object sender, EventArgs e)
		{
			if (!MenuSecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.ConsolInvPostWholeConsol))
			{
				MenuSecurityHelper.ShowError(SecurityCore.ConsolInvPostWholeConsol);
			}
			else
			{
				PostTransactions(JobInvoicingPostingOption.All);
			}
		}

		void MenuItemPostConsolCosts_Click(object sender, EventArgs e)
		{
			if (!MenuSecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.ConsolInvPostAllCosts))
			{
				MenuSecurityHelper.ShowError(SecurityCore.ConsolInvPostAllCosts);
			}
			else
			{
				PostTransactions(JobInvoicingPostingOption.Costs);
			}
		}

		void MenuItemPostConsolCostsOnly_Click(object sender, EventArgs e)
		{
			if (!MenuSecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.ConsolInvPostAllCosts))
			{
				MenuSecurityHelper.ShowError(SecurityCore.ConsolInvPostAllCosts);
			}
			else
			{
				PostTransactions(JobInvoicingPostingOption.ConsolCosts);
			}
		}

		void MenuItemRedefaultExRate_Click(object sender, EventArgs e)
		{
			foreach (JobConsolCost cost in Apportionments.CostsCollection)
			{
				cost.RedeaultExchangeRate();
			}
			foreach (Job job in Jobs)
			{
				job.UpdateExchangeRates(false);
			}
			HostBusinessEntity.HasChanges = true;
		}

		void MenuItemResetTaxDefaults_Click(object sender, EventArgs e)
		{
			var jobs_ReadyForFinancialClosureWithoutModifySecurity = Jobs.Where(job => job.IsReadyForFinancialClosureWithoutModifySecurity);

			if (jobs_ReadyForFinancialClosureWithoutModifySecurity.Any())
			{
				Globals.Message.ShowError(Res.GetString("BAACA90D-2C3C-4707-B97A-7B8017F371FD"
					, "Cannot reset tax defaults of unposted lines for this consol, because one or more jobs have Ready For Financial Closure status \r\n{0}"
					, string.Join(System.Environment.NewLine, jobs_ReadyForFinancialClosureWithoutModifySecurity.Select(job => job.JH_JobNum))
				));
				return;
			}

			var apportionedCharges = new List<ApportionSplitCharge>();

			foreach (JobConsolCost cost in Apportionments.CostsCollection)
			{
				if (!cost.IsPosted)
				{
					cost.ResetCostTaxInfo();
				}

				apportionedCharges.AddRange(cost.ApportionmentCharges.Cast<ApportionSplitCharge>());
			}

			foreach (var job in Jobs)
			{
				ResetJobTaxDefaults(job, apportionedCharges);
			}
		}

		void ResetJobTaxDefaults(Job job, IEnumerable<ApportionSplitCharge> apportionedCharges)
		{
			job.SetDefaultValueForTaxBranch();

			foreach (Charge charge in job.Charges)
			{
				var apportionedCharge = apportionedCharges.FirstOrDefault(x => x.PK == charge.PK);
				var processingCharge = (apportionedCharge == null || !apportionedCharge.JR_IsUsedForApportionment)
					? charge as BaseCharge
					: apportionedCharge;

				processingCharge.ResetUnpostedSellTaxDefault();

				if (processingCharge is Charge shipmentCharge)
				{
					shipmentCharge.ResetUnpostedCostTaxDefault();
				}
			}
		}

		void MenuItemSynchroniseInvoiceDetails_Click(object sender, EventArgs e)
		{
			var forwardingConsol = HostBusinessEntity as IJobCostingPlugIn;
			if (forwardingConsol != null)
			{
				IEnumerable<JobConsolCost> costsToFix = null;

				try
				{
					costsToFix = forwardingConsol.GetApportionments().CostsCollection.Where(x => !x.HasSynchronisedAPInvoiceDetails);
				}
				catch (JobCreationException ex)
				{
					Globals.Message.ShowError(ex.Message, Res.GetString("ede5900c-d981-4555-b5b4-22fae59b40d7", "Unable to Synchronize"));
				}

				if (costsToFix != null)
				{
					var syncDone = false;
					var failedToSyncMessage = new StringBuilder();

					foreach (var cost in costsToFix)
					{
						syncDone |= cost.SynchroniseUnpostedInvoiceDetailsIfNecessary();

						var failedToSyncCosts = string.Empty;
						syncDone |= cost.SynchronisePostedInvoiceDetailsIfPossible(out failedToSyncCosts);

						if (!string.IsNullOrEmpty(failedToSyncCosts))
						{
							failedToSyncMessage.AppendLine(failedToSyncCosts);
						}
					}

					var message = string.Empty;
					if (failedToSyncMessage.Length > 0)
					{
						var messageBuilder = new StringBuilder(Res.GetString("47fba26a-b947-424b-9138-146337422ca0", "Following Consol Cost(s) cannot be synchronized. You should try deleting them. Deleting will keep charges, only Consol Cost can be unlinked and removed."));
						messageBuilder = messageBuilder.AppendLine();
						messageBuilder = messageBuilder.AppendLine(failedToSyncMessage.ToString());
						message = messageBuilder.ToString();
					}

					if (string.IsNullOrEmpty(message))
					{
						message = syncDone ?
							Res.GetString("d97f0a9a-1da2-43e2-bc47-05711988dba7", "There were some adjustment made to the Invoice details on Apportionment Charges.") :
							Res.GetString("341a85cd-8391-459d-8e11-c76860dc1af7", "All Invoice details on Apportionment Charges are in sync with Consol Cost ones.");
					}

					Globals.Message.ShowInformation(message);
				}
			}
		}

		void PostTransactions(JobInvoicingPostingOption postingOption)
		{
			if (((IBusiness)Consol).HasChanges)
			{
				Globals.Message.ShowError(Res.GetString("3a23464c-eb18-44a6-979b-b992a60ca2d8", "Please save before posting"));
			}
			else
			{
				IJobCostingPlugIn forwardingConsol = HostBusinessEntity as IJobCostingPlugIn;
				try
				{
					var wrapper = GetPostManagerGUIWrapper(postingOption, HostBusinessEntity.Factory, Jobs, forwardingConsol, UserControl.FindForm(), Apportionments);
					wrapper.Post();
				}
				catch (JobCreationException ex)
				{
					Globals.Message.ShowError(ex.Message, Res.GetString("26de816e-71ac-4978-ad29-1aabb2a844f8", "Unable to post"));
				}
			}
		}

		protected virtual ConsolInvoicingPostManagerGUIWrapper GetPostManagerGUIWrapper(JobInvoicingPostingOption postingOption, BusinessObjectFactory plugInFactory, IEnumerable<Job> jobs, IJobCostingPlugIn consol, Form parentForm, ApportionmentListing consolCostListing)
		{
			return new ConsolInvoicingPostManagerGUIWrapper(postingOption, plugInFactory, jobs, consol, parentForm, consolCostListing);
		}

		protected IJobInvoicingPlugIn PlugInParent
		{
			get { return HostBusinessEntity as IJobInvoicingPlugIn; }
		}

		protected override Control GetNewUserControl()
		{
			return new NewApportionmentUserControl(Apportionments);
		}

		protected override ZTabPagePlugIn GetTabPage()
		{
			return new ZAutoSizedTabPagePlugIn(this);
		}

		public override void UpdateTabPageMinimumAutoSized()
		{
			const int ExtraWidthRequiredToSizeTabPageProperly = 10;
			TabPage.MinimumAutoSizedWidth = UserControl.MinimumSize.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(ExtraWidthRequiredToSizeTabPageProperly);
			TabPage.MinimumAutoSizedHeight = UserControl.MinimumSize.Height;
		}

		void IPluginShouldRefreshMenuForGateway.RefreshGatewayElements(bool isGatewayEnabled)
		{
			RefreshGatewayElementsCore(isGatewayEnabled);
		}

		protected virtual void RefreshGatewayElementsCore(bool isGatewayEnabled)
		{
			TabPage.Text = Name;
			if (Consol is ForwardingConsol commonConsol)
			{
				foreach (MenuItem menuItem in TopLevelMenu.MenuItems)
				{
					var text = menuItem.Text;
					menuItem.Visible = !text.Equals(Constants.MenuNameConstants.PostGatewayAgentCharges) || commonConsol.IsGatewayConsol;
				}
			}
		}

		void IPluginForGatewaySellApportionments.SyncGatewaySellToCostIfNecessary()
		{
			if (Consol.IsGatewayBillingEnabled())
			{
				GatewaySellToCostSynchroniser.Synchronise(new Job.Loader(Consol as IJobHeaderParent).Load());
			}
		}

		#endregion

		#region Jobs

		public IEnumerable<Job> Jobs
		{
			get
			{
				ZQuery jobsQuery = new ZQuery(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
				jobsQuery.AddToFilter(JobHeaderSchema.JH_ParentID, Consol.CostSupporter.ShipmentsListPKs);
				var jobs = HostBusinessEntity.Factory.Load<Job>(new JobCollection(HostBusinessEntity.Factory, jobsQuery).CompleteFilter);
				return jobs;
			}
		}

		#endregion
	}
}
