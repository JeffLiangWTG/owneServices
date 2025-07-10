using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using static Enterprise.ComplianceRisk.GUI.ComplianceRiskPresentationHelper;
using static Enterprise.ComplianceRisk.Integration.ComplianceRiskStatusCodeList;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.ComplianceRisk.GUI
{
	public class ComplianceRiskPlugIn : ZAlwaysLoadPlugIn
	{
		readonly ComplianceRiskPlugInBusinessObject pluginBizO;

		/// <summary>
		/// This is the entry point to apply compliance risk plugin, by default it will be consumed by controller.
		/// Please see "<see cref="https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/4042/ComplianceWise-Plug-in"/>" for more details.
		/// </summary>
		/// <param name="hostBusinessEntity">The host business object which implements IComplianceItemRiskStatusProvider and one or more its sub interfaces.</param>
		public ComplianceRiskPlugIn(IBusiness hostBusinessEntity) : base(hostBusinessEntity)
		{
			var hostBusinessEntityHasChanges = hostBusinessEntity.HasChanges;

			using ((hostBusinessEntity as BusinessObject).SuspendSettingHasChanges())
			{
				pluginBizO = new ComplianceRiskPlugInBusinessObject(HostBusinessEntity);

				cancellationTokenSource = new CancellationTokenSource();
				pluginBizO.CommodityRiskStatusChecker = new CommodityRiskStatusBorderWiseCheckerForGui(pluginBizO, cancellationTokenSource.Token);
				CreateHelperRegisterInteractEventIfNeeded(pluginBizO);

				if (!hostBusinessEntityHasChanges && pluginBizO.HostBusinessEntity.IsInDatabase)
				{
					if (IsCurrent)
					{
						ComplianceRiskStatusSynchronizer.SynchronizeAndSaveIfNeeded(pluginBizO, synchronizeEnforceOnFormLoad: true);
					}
				}
				else
				{
					ComplianceRiskStatusSynchronizer.SynchronizeForConvertShipmentFirstLoadIfNeeded(pluginBizO, true);
				}
			}
		}

		void CreateHelperRegisterInteractEventIfNeeded(ComplianceRiskPlugInBusinessObject pluginBizO)
		{
			pluginBizO.CreateHelperRegisterInteractEventIfNeeded(new AssessmentHelper(async () =>
			{
				await ComplianceRiskStatusFormSynchronizer.InitializeComplianceAssessmentWhenMenuClick(pluginBizO, null);
			}));
		}

		bool IsCurrent => (HostBusinessEntity as IComplianceItemRiskStatusProvider)?.JobTime.IsCurrent ?? false;

		public string GetJobUniqueRef() => CodePropertyAttribute.CodeFromBusinessObject((BusinessObject)HostBusinessEntity);

		public override string Name => ComplianceWiseConstants.ComplianceRiskPlugInName;

		public override async void OnUserControlShown()
		{
			if (TopLevelTabControl != null)
			{
				TopLevelTabControl.GetTabPage(ComplianceWiseConstants.ComplianceRiskTabPageName).CheckForChildrenControlsVisibilityChange = false;
			}

			UpdateCommodityRiskGridForExportJob();

			base.OnUserControlShown();

			ComplianceRiskStatusSynchronizer.SynchronizeOnUserControlShownAndSaveIfNeeded(pluginBizO);

			UpdateUserControlsVisibilityIfNeeded();

			await TriggerCommodityRiskStatusCheckIfNeeded(true);
		}

		public override void OnSaving()
		{
			ConcurrencyChangedInDBSynchronizeDataIfNeeded();

			ComplianceRiskStatusSynchronizer.Synchronize(pluginBizO);

			base.OnSaving();
		}

		void ConcurrencyChangedInDBSynchronizeDataIfNeeded()
		{
			if (HostBusinessEntity is IForwardingShipment shipment)
			{
				ConcurrencyChangedInDBSynchronizeDataIfNeeded(shipment.PK, (HostBusinessEntity as IScreeningStatusProvider));
			}
			else if (HostBusinessEntity is IQuotedBooking booking && booking.ForwardingShipment != null && booking.ForwardingShipment.IsInDatabase)
			{
				ConcurrencyChangedInDBSynchronizeDataIfNeeded(booking.ForwardingShipment.PK, (booking.ForwardingShipment as IScreeningStatusProvider));
			}
		}

		void ConcurrencyChangedInDBSynchronizeDataIfNeeded(ZGuid businessEntityPK, IScreeningStatusProvider businessEntityStatusProvider)
		{
			if (HostBusinessEntity.IsInDatabase && (HostBusinessEntity as BusinessObject).HasChanges && businessEntityStatusProvider != null)
			{
				var dbScreeningStatus = (new BusinessObjectFactory()).Load<IForwardingShipment>(businessEntityPK);
				if (dbScreeningStatus != null && dbScreeningStatus is IScreeningStatusProvider statusProvider &&
					statusProvider.ScreeningStatus != businessEntityStatusProvider.ScreeningStatus)
				{
					businessEntityStatusProvider.ScreeningStatus = statusProvider.ScreeningStatus;
				}
			}
		}

		protected override ZBool HasUserControl => true;

		protected override Control GetNewUserControl() => new ComplianceRiskPlugInUserControl(pluginBizO) { Dock = DockStyle.Fill };

		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.Core;

		#region Menu Item

		protected override MenuItem GetNewTopLevelMenu()
		{
			var complianceRiskMenu = new ZMenuItem(ResString.GetMultilingualString("d775b5fa-51ed-4cd7-973f-a3aa61d154cb", "Compliance Risk"));
			var overrideComplianceRiskStatusMenuItem = new ZMenuItem(ResString.GetMultilingualString("c274eba4-3964-4bae-9f94-b64f7368b8f8", "Override Compliance Risk"));
			overrideComplianceRiskStatusMenuItem.Click += OverrideComplianceRiskStatusMenuItem_Click;
			complianceRiskMenu.MenuItems.Add(overrideComplianceRiskStatusMenuItem);

			var resynchronizeComplianceRiskStatusMenuItem = new ZMenuItem(ResString.GetMultilingualString("41ed6be0-38b8-429b-bc21-66451994536e", "Resynchronize Compliance Risk Status"));
			resynchronizeComplianceRiskStatusMenuItem.Click += ResynchronizeComplianceRiskStatusMenuItem_Click;
			complianceRiskMenu.MenuItems.Add(resynchronizeComplianceRiskStatusMenuItem);

			if (pluginBizO.ComplianceItemRiskStatusProvider.ComplianceRiskSupport.IsSupportInitialization())
			{
				initializeComplianceAssessmentMenuItem = new ZMenuItem(ResString.GetMultilingualString("ed2f9b14-987b-491b-9b21-b1d916492e12", "Initialize Compliance Assessment"));
				initializeComplianceAssessmentMenuItem.Click += ComplianceAssessmentMenuItem_Click;
				complianceRiskMenu.MenuItems.Add(initializeComplianceAssessmentMenuItem);
				initializeComplianceAssessmentMenuItem.Visible = pluginBizO.ComplianceCommodityRiskStatusProvider != null;

				declineComplianceAssessmentMenuItem = new ZMenuItem(ResString.GetMultilingualString("81d8f0b7-cb4e-4164-a20b-b467bbbb692d", "Decline Compliance Assessment"));
				declineComplianceAssessmentMenuItem.Click += ComplianceAssessmentMenuItem_Click;
				complianceRiskMenu.MenuItems.Add(declineComplianceAssessmentMenuItem);
				declineComplianceAssessmentMenuItem.Visible = pluginBizO.ComplianceCommodityRiskStatusProvider != null;

				UpdateAssessmentMenuItemsVisibilityIfNeeded();
			}

			var viewPartyRiskMenuItem = new ZMenuItem(ResString.GetMultilingualString("73F3A042-E567-4093-8B5A-A978F6765EDD", "View Party Risk"));
			viewPartyRiskMenuItem.Click += ViewPartyRiskMenuItem_Click;
			complianceRiskMenu.MenuItems.Add(viewPartyRiskMenuItem);
			viewPartyRiskMenuItem.Visible = pluginBizO.CompliancePartyRiskStatusProvider != null;

			var legacyDPSMenuItem = new ZMenuItem(ResString.GetMultilingualString("67e16d74-fe05-436d-99ef-9567faf23617", "Legacy Screening Status"));
			legacyDPSMenuItem.Click += LegacyDPSMenuItem_Click;
			complianceRiskMenu.MenuItems.Add(legacyDPSMenuItem);

			var actionLevelMenu = Form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
			actionLevelMenu.MenuItems.Add(complianceRiskMenu);

			return actionLevelMenu;
		}

		ZMenuItem initializeComplianceAssessmentMenuItem;
		ZMenuItem declineComplianceAssessmentMenuItem;

		public override bool ShouldHideTopLevelMenuWithTab => false;

		void OverrideComplianceRiskStatusMenuItem_Click(object sender, EventArgs e)
		{
			if (ComplianceRiskSecurityRights.IsAllowedOverrideOverallRiskStatusWithShowError(HostBusinessEntity, showErrorWhenNotAllowed: true))
			{
				if (pluginBizO.ComplianceRiskStatus.COR_OverallRisk == Codes.Clear || pluginBizO.ComplianceRiskStatus.COR_OverallRisk == Codes.OverrideClear)
				{
					Globals.Message.Show(Res.GetString("24052a6a-f18c-4e1c-85fd-74ade7a13173", "There is no need to override job compliance status when it is Clear (CLR) or Override Clear (OVR)."));
				}
				else if (HostBusinessEntity.HasChanges)
				{
					Globals.Message.Show(Res.GetString("06048576-bca5-4960-a146-51576b1b5d28", "Please save the form before overriding job compliance status."));
				}
				else
				{
					var confirmationModel = new OverrideComplianceRiskConfirmationModel();
					if (ZFormModaliser.ShowDialogAndDispose(new OverrideComplianceRiskConfirmationForm(GetJobUniqueRef(), confirmationModel)) == DialogResult.OK)
					{
						OverrideComplianceRiskStatus(confirmationModel);
					}
				}
			}
		}

		protected void OverrideComplianceRiskStatus(OverrideComplianceRiskConfirmationModel model)
		{
			pluginBizO.ComplianceRiskStatus.TakeSnapshotAndSetStatusToOverrideClear(pluginBizO, (model.Code, model.JobClearingReasonList[model.Code]?.Description, model.Reason));
			UpdateUserControlsVisibilityIfNeeded();
		}

		void ResynchronizeComplianceRiskStatusMenuItem_Click(object sender, EventArgs e)
		{
			if (ComplianceRiskSecurityRights.IsAllowedResynchronizeComplianceRiskStatusWithShowError(HostBusinessEntity, showErrorWhenNotAllowed: true))
			{
				if (HostBusinessEntity.HasChanges)
				{
					Globals.Message.ShowInformation(Res.GetString("8A708D04-72CA-4CE2-897D-4272F6E2E1C9", "Please save form before resynchronizing Compliance Risk statuses"));
				}
				else
				{
					if (ComplianceRiskStatusSynchronizer.SynchronizeAndSaveIfNeeded(pluginBizO, synchronizeEnforceOnFormLoad: false))
					{
						Globals.Message.ShowInformation(Res.GetString("654DDD39-E08E-4AB4-8AF6-67DD163C92C7", "Resynchronizing completed."));
					}
					else
					{
						Globals.Message.ShowInformation(Res.GetString("7b0116e3-c6f3-49ed-92d3-4de4e7249738", "{0} is synchronized, no need for resynchronization.", GetJobUniqueRef()));
					}
				}
			}
		}

		async void ViewPartyRiskMenuItem_Click(object sender, EventArgs e)
		{
			pluginBizO.ReloadSafeComplianceRiskStatusWithRelatedParties();
			if (pluginBizO.CompliancePartyRiskStatusProvider != null)
			{
				var presentationManager = ObjectFactory.Get<IDeniedPartyScreeningPresentationManager>();
				var parties = pluginBizO.CompliancePartyRiskStatusProvider.Parties.Select(o => ScreeningParty.ConvertFromComplianceParty(o.Parent, o.Description, o.Party, o.NaturalPerson)).ToArray();

				await presentationManager.PerformScreening(
					Form,
					DpsSourceWithParties.GetSingleSourceList(pluginBizO.ComplianceRiskStatus.Parent, parties),
					parties,
					false,
					!DeniedPartyScreenerAsync.HasExcludedList(HostBusinessEntity.Factory),
					parentEntityHasChanges: () => HostBusinessEntity.HasChanges,
					complianceRiskAction: new ComplianceRiskAction(
						() => ComplianceRiskStatusSynchronizer.SynchronizeAndSaveIfNeeded(pluginBizO, synchronizeEnforceOnFormLoad: false),
						ShowMessageIfNeeded, UpdateUserControlsVisibilityIfNeeded, pluginBizO.ComplianceRiskStatus.COR_OverallRisk));
			}
		}

		async void ComplianceAssessmentMenuItem_Click(object sender, EventArgs e)
		{
			if (sender == initializeComplianceAssessmentMenuItem)
			{
				var jobForm = (sender as ZMenuItem)?.GetMainMenu()?.GetForm() as ISupportSwitchTabPage;
				await ComplianceRiskStatusFormSynchronizer.InitializeComplianceAssessmentWhenMenuClick(pluginBizO, jobForm);
			}
			else if (sender == declineComplianceAssessmentMenuItem)
			{
				ComplianceRiskStatusFormSynchronizer.DeclineComplianceAssessmentWhenMenuClick(pluginBizO);
			}

			UpdateUserControlsVisibilityIfNeeded();
		}

		void UpdateAssessmentMenuItemsVisibilityIfNeeded()
		{
			var isCommodityRiskAssessable = pluginBizO.IsCommodityRiskAssessable();
			var initializeEnabled = isCommodityRiskAssessable;
			var declineEnabled = isCommodityRiskAssessable;

			if (isCommodityRiskAssessable)
			{
				if (pluginBizO.ComplianceRiskStatus.IsAssessmentInitialized)
				{
					initializeEnabled = false;
					declineEnabled = false;
				}
				else if (pluginBizO.ComplianceRiskStatus.IsAssessmentDeclined)
				{
					declineEnabled = false;
				}
			}

			if (declineComplianceAssessmentMenuItem != null)
			{
				declineComplianceAssessmentMenuItem.Enabled = declineEnabled;
			}

			if (initializeComplianceAssessmentMenuItem != null)
			{
				initializeComplianceAssessmentMenuItem.Enabled = initializeEnabled;
			}
		}

		void UpdateAssessmentAndCommodityControlsVisibilityIfNeeded()
		{
			var isCommodityPanelVisible = pluginBizO.IsCommodityPanelVisible();
			var complianceRiskPlugInUserControl = (ComplianceRiskPlugInUserControl)UserControl;
			var hasPointPairChanged = complianceRiskPlugInUserControl.SplitContainer.Panel2Collapsed != !isCommodityPanelVisible;

			complianceRiskPlugInUserControl.SplitContainer.Panel2Collapsed = !isCommodityPanelVisible;
			complianceRiskPlugInUserControl.AssessmentInitializeButton.Enabled = !pluginBizO.ComplianceRiskStatus.IsAssessmentInitialized;

			if (hasPointPairChanged)
			{
				complianceRiskPlugInUserControl.PartyHideCheckBox.Checked = false;
				complianceRiskPlugInUserControl.LocationHideCheckBox.Checked = false;
				complianceRiskPlugInUserControl.CommodityHideCheckBox.Checked = false;
				complianceRiskPlugInUserControl.UpdateRiskFactorControlsVisibility();
			}
		}

		void LegacyDPSMenuItem_Click(object sender, EventArgs e)
		{
			string statusCode = null;

			if (pluginBizO.HostBusinessEntity is IQuotedBooking quotedBooking)
			{
				statusCode = (quotedBooking.ForwardingShipment as IScreeningStatusProvider)?.ScreeningStatus;
			}
			else if (pluginBizO.HostBusinessEntity is IScreeningStatusProvider screeningStatusProvider)
			{
				statusCode = screeningStatusProvider.ScreeningStatus;
			}

			var statusList = new ScreeningStatusesList();
			Globals.Message.ShowInformation(Res.GetString("bbc2561c-0c2d-4ca6-bfda-ff4d26a7942d", "Screening Status of {0} is {1} - {2}", GetJobUniqueRef(), statusCode, statusList.GetDescriptionFromCode(statusCode ?? ScreeningStatusesList.Codes.Unknown)));
		}

		void ShowMessageIfNeeded(List<ZString> statusList)
		{
			if (statusList.Count != 1)
			{
				return;
			}

			if (statusList[0] != pluginBizO.ComplianceRiskStatus.COR_OverallRisk)
			{
				Globals.Message.Show(Res.GetString("3E23B9B2-89C3-48D6-9626-1AC4C16F886D", "The job will be set to '{0}'.", pluginBizO.OverallRiskDescription));
			}
		}

		#endregion

		protected override ZTabPagePlugIn GetTabPage()
		{
			var tabPage = base.GetTabPage();
			UpdateTabPageImageIfNeeded(tabPage);
			return tabPage;
		}

		protected override void HookFormEventsCore()
		{
			base.HookFormEventsCore();
			pluginBizO.ComplianceRiskStatus.COR_OverallRiskInfo.ValueChanged += HookEventsUpdateRelatedControls;
			pluginBizO.ComplianceRiskStatus.Factory.Saved += HookEventsUpdateRelatedControls;
			pluginBizO.ComplianceRiskStatus.Factory.Saved += TriggerAssessmentIfNeeded;
			pluginBizO.ComplianceRiskStatus.Factory.Saved += HookEventsProcessMaterialChangeIfNeeded;

			if (!pluginBizO.HostBusinessEntity.IsInDatabase)
			{
				pluginBizO.HostBusinessEntity.Factory.Saved += UpdateCollectionJobSourceNumberIfNeeded;
			}

			if (Form != null && !Form.IsDisposed)
			{
				Form.Load += FormLoad_TriggerCommodityRiskStatusCheck;
			}
		}

		async void TriggerAssessmentIfNeeded(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (pluginBizO.ComplianceRiskStatus.AssessmentInitializedByRule)
			{
				pluginBizO.ComplianceRiskStatus.AssessmentInitializedByRule = false;
				await TriggerCommodityRiskStatusCheckIfNeeded(true);
			}
		}

		#region BorderWise

		async Task TriggerCommodityRiskStatusCheckIfNeeded(bool needValidation)
		{
			if (Form != null
				&& Form.DisplayMode != ODisplayMode.ReadOnly
				&& Form.DisplayMode != ODisplayMode.Delete)
			{
				if (pluginBizO.CommodityRiskStatusChecker != null)
				{
					if (pluginBizO.ComplianceRiskStatus.IsAssessmentInitialized)
					{
						await pluginBizO.CommodityRiskStatusChecker.CheckAllCommoditiesRiskStatus(needValidation);
					}
					else
					{
						await pluginBizO.CommodityRiskStatusChecker.GetSupportedCountriesAndAssignStatusIfNeeded();
					}
				}
			}
		}

		async void FormLoad_TriggerCommodityRiskStatusCheck(object sender, EventArgs e)
		{
			if (IsCurrent)
			{
				await TriggerCommodityRiskStatusCheckIfNeeded(false);
			}
		}

		CancellationTokenSource cancellationTokenSource;

		#endregion

		protected override void UnHookFormEventsCore()
		{
			base.UnHookFormEventsCore();
			pluginBizO.ComplianceRiskStatus.COR_OverallRiskInfo.ValueChanged -= HookEventsUpdateRelatedControls;
			pluginBizO.ComplianceRiskStatus.Factory.Saved -= HookEventsUpdateRelatedControls;
			pluginBizO.ComplianceRiskStatus.Factory.Saved -= TriggerAssessmentIfNeeded;
			pluginBizO.HostBusinessEntity.Factory.Saved -= UpdateCollectionJobSourceNumberIfNeeded;
			pluginBizO.ComplianceRiskStatus.Factory.Saved -= HookEventsProcessMaterialChangeIfNeeded;

			if (Form != null)
			{
				Form.Load -= FormLoad_TriggerCommodityRiskStatusCheck;
			}

			if (cancellationTokenSource != null)
			{
				cancellationTokenSource.Cancel();
				cancellationTokenSource.Dispose();
				cancellationTokenSource = null;
			}
		}

		async void HookEventsProcessMaterialChangeIfNeeded(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (ComplianceCheckRequestModelBuilder.GetApplicableCommodityDetails(
				pluginBizO.ComplianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>())
				.Any(c => c.CCD_RiskStatus == ComplianceRiskStatusCodeList.Codes.NotChecked))
			{
				await TriggerCommodityRiskStatusCheckIfNeeded(true);
			}
		}

		void HookEventsUpdateRelatedControls(object sender, EventArgs e) => UpdateUserControlsVisibilityIfNeeded();

		void HookEventsUpdateRelatedControls(BusinessObjectFactory factory, bool savedSuccessfully) => UpdateUserControlsVisibilityIfNeeded();

		void UpdateTabPageImageIfNeeded(ZTabPage tabPage)
		{
			if (tabPage != null)
			{
				tabPage.ImageIndex = IsRiskStatusNotClear(pluginBizO.ComplianceRiskStatus.COR_OverallRisk)
					? Icons.GetImageIndex(IconTypes.RedAlarmBell) : -1;
			}
		}

		void UpdateCollectionJobSourceNumberIfNeeded(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			pluginBizO.ComplianceRiskStatus.CommodityDetailCollection.UpdateJobSourceNumberIfNeeded();
			pluginBizO.HostBusinessEntity.Factory.Saved -= UpdateCollectionJobSourceNumberIfNeeded;
		}

		/// <summary>
		/// Update user controls visibility when compliance risk status is Potential Risk.
		/// </summary>
		void UpdateUserControlsVisibilityIfNeeded()
		{
			UpdateAssessmentMenuItemsVisibilityIfNeeded();
			UpdateAssessmentAndCommodityControlsVisibilityIfNeeded();

			if (TopLevelTabControl != null)
			{
				UpdateTabPageImageIfNeeded(TopLevelTabControl.GetTabPage(ComplianceWiseConstants.ComplianceRiskTabPageName));
			}

			var messageBanner = Form?.MainStatusBar.FindSingleOrDefault<ZLabel>(c => c.Name == "CompliancePotentialRiskMessageBanner");
			if (messageBanner != null)
			{
				messageBanner.Visible = IsRiskStatusNotClear(pluginBizO.ComplianceRiskStatus.COR_OverallRisk);
			}
		}

		void UpdateCommodityRiskGridForExportJob()
		{
			var commodityRiskUserControl = UserControl.FindSingle<ZUserControl>(u => u.Name == "CommodityRiskUserControl") as ICommodityRiskUserControl;
			if (commodityRiskUserControl != null)
			{
				if (ComplianceRiskHelper.IsCustomsEnabledManageRiskStatusOnCommercialInvoice
					&& OrganisationsDataRegistry.Instance.CustomsShowImportAlertsOnExportDeclarations.Value
					&& pluginBizO.ComplianceCommodityRiskStatusProvider.RiskCalculateFactor == CommodityRiskCalculateFactor.Export)
				{
					commodityRiskUserControl.AddOrRemoveComplianceAlertsForExportDeclaration(true);
				}
				else
				{
					commodityRiskUserControl.AddOrRemoveComplianceAlertsForExportDeclaration(false);
				}
			}
		}

		public override void OnSaveCompletedOrAborted(bool saved)
		{
			base.OnSaveCompletedOrAborted(saved);
			UpdateUserControlsVisibilityIfNeeded();
		}

#if DEBUG
		public ComplianceRiskPlugInBusinessObject GetBusinessObjectForPlugin => pluginBizO;
#endif
	}
}
