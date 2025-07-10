using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.CommissionManagement.Business;
using Enterprise.CommissionManagement.GUI;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Module
{
	public class CommissionManagementModule : ZFilterGridModule, IImportCollectionInfoProvider
	{
		protected override int MaxRowsToLoad => base.MaxRowsToLoad * 4;

		#region ID

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Commission; }
		}

		#endregion

		#region Allowed Actions

		public override bool AllowEdit
		{
			get { return false; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		#endregion

		#region GridCollection

		public new ViewCommissionLineCollection GridCollection
		{
			get { return (ViewCommissionLineCollection)base.GridCollection; }
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ViewCommissionLineCollection(Factory);
		}

		#endregion

		#region FilterBusinessObject

		new CommissionManagementFilterBusinessObject FilterBusinessObject
		{
			get { return (CommissionManagementFilterBusinessObject)base.FilterBusinessObject; }
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CommissionManagementFilterBusinessObject();
		}

		#endregion

		#region FilterControl

		protected override IFilterControl GetNewFilterControl()
		{
			return new CommissionManagementFilterControl(GridCollection, FilterBusinessObject);
		}

		#endregion

		#region ZFilterGridModule

		protected override bool ShouldLoadTop1WhenGridEmpty => false;

		#endregion

		#region RecentItems

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}

		#endregion

		#region Sort

		protected override SortInfo DefaultSortOrder
		{
			get { return null; }
		}

		#endregion

		#region Controller

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Commission);
		}

		#endregion

		#region Module Decision Provider

		protected override IModuleDecisionProvider CreateDefaultModuleDecisionProvider()
		{
			return new CommissionManagementModuleDecisionProvider(this);
		}

		#endregion

		#region Menu Items

		protected override MenuItem[] GetNewAdditionalMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewAdditionalMenuItems());
			result.InsertRange(result.Count - 1,
				new[]
				{
					new ZMenuItem(ResString.GetMultilingualString("1908967d-d4db-4627-9f63-85118af2dcdc", "Undo Cancel"), OnUndoCancelMenuItemClick) { Tag = UndoCancelMenuItemTag },
					new ZMenuItem(ResString.GetMultilingualString("A0794CF0-28F5-464D-9E7F-FD35345939E4", "Regenerate Commissions"), OnRegenerateCommissionsMenuItemClick) { Tag = RegenerateCommissionsMenuItemTag },
					new ZMenuItem(ResString.GetMultilingualString("1483f5b7-69f1-4c93-a640-bfcbbdf809c9", "Agreement Approval"), OnApproveCommissionAgreementsMenuItemClick) { Tag = AgreementApprovalMenuItemTag },
					new ZMenuItem(ResString.GetMultilingualString("80ad4f25-f8be-459c-8605-81839cdd6dec", "Finalizer"), OnCommissionFinaliserMenuItemClick) { Tag = FinaliserMenuItemTag }
				});

			return result.ToArray();
		}

		public const string UndoCancelMenuItemTag = "UndoCancelMenuItemTag";
		public const string RegenerateCommissionsMenuItemTag = "RegenerateCommissionsMenuItemTag";
		public const string AgreementApprovalMenuItemTag = "AgreementApprovalMenuItemTag";
		public const string FinaliserMenuItemTag = "FinaliserMenuItemTag";

		protected override void SetupButtonDetailForItem(MenuItem item, ref IconTypes buttonImage, ref IconTypes buttonImageActive, ref string buttonToolTip)
		{
			base.SetupButtonDetailForItem(item, ref buttonImage, ref buttonImageActive, ref buttonToolTip);

			if (item.Tag == (object)AgreementApprovalMenuItemTag)
			{
				buttonImage = IconTypes.EditButtonRest;
				buttonImageActive = IconTypes.EditButtonActive;
				buttonToolTip = Res.GetString("30c56797-1f51-4107-9fdc-7996b961d023", "Open Commission Agreement Approval Form");
			}
			else if (item.Tag == (object)FinaliserMenuItemTag)
			{
				buttonImage = IconTypes.Dollar;
				buttonImageActive = IconTypes.None;
				buttonToolTip = Res.GetString("cdfd79d8-498d-4164-8e72-0441281a8af3", "Open Commission Finalizer");
			}
			else if (item.Tag == (object)UndoCancelMenuItemTag)
			{
				buttonImage = IconTypes.ResetButtonRest;
				buttonImageActive = IconTypes.ResetButtonActive;
				buttonToolTip = Res.GetString("781a0ebe-c536-4d47-b839-c43c48260ee5", "Open Bulk undo cancellation");
			}
			else if (item.Tag == (object)RegenerateCommissionsMenuItemTag)
			{
				buttonImage = IconTypes.Cog;
				buttonImageActive = IconTypes.Cog;
				buttonToolTip = Res.GetString("0A5C99F6-6C47-4C23-A683-A5ECFE5A18F6", "Regenerate All Commissions For This Job");
			}
		}

		void OnUndoCancelMenuItemClick(object sender, EventArgs e)
		{
			UndoCancel(SelectedBusinessObjects);
		}

		void OnRegenerateCommissionsMenuItemClick(object sender, EventArgs e)
		{
			var selectedBizOs = GetSelectedBusinessObjects().Cast<ViewCommissionLineGrouping>().DistinctBy(x => x.SourceNumber);

			if (selectedBizOs != null && !selectedBizOs.Any())
			{
				Globals.Message.Show(NoCommissionGroupsSelectedMessage, RegenerateCommissionsCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, DialogResult.OK);
				return;
			}

			var stringBuilderArchivedJobs = new ZStringBuilder();
			CargoWise.Common.IEnumerableExtensions.ForEach(selectedBizOs.Where(b => b.SourceTableCode == JobHeaderSchema.Constants.Prefix && b.SourceId.IsEmpty), lineGrouping => stringBuilderArchivedJobs.AppendLine($" - {lineGrouping.SourceNumber}"));
			if (!stringBuilderArchivedJobs.IsEmpty)
			{
				var archiveJobsMessage = Res.GetString("B6668BFB-3B22-423B-8CFF-FBB58484F665", "There are commissions with Jobs that have been archived. They cannot have their commissions regenerated. The archived Jobs are:");
				stringBuilderArchivedJobs.Prepend(archiveJobsMessage + System.Environment.NewLine);

				Globals.Message.Show(stringBuilderArchivedJobs.ToString(), RegenerateCommissionsCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			var stringBuilder = new ZStringBuilder();
			var infoMessage = Res.GetString("B1037095-6635-4FC2-9626-F7A20978A2B5", "Selecting Yes will queue the following Job(s) / Transaction(s) for regeneration, this action will cancel all existing commission transactions and regenerate their commission lines:");
			var confirmationMessage = Res.GetString("765FFAFF-74CC-4C39-8388-33A735CD8A71", "Select Yes to Continue, or No to Cancel this action.");

			stringBuilder.AppendLine(infoMessage + System.Environment.NewLine);
			CargoWise.Common.IEnumerableExtensions.ForEach(selectedBizOs, lineGrouping => stringBuilder.AppendLine($" - {lineGrouping.SourceNumber}"));
			stringBuilder.AppendLine(System.Environment.NewLine + confirmationMessage);

			if (Globals.Message.Show(stringBuilder.ToString(), RegenerateCommissionsCaption, MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
			{
				QueueSourceItemsForRegeneration(selectedBizOs);
				Globals.Message.Show(Res.GetString("B35C0F26-CC2B-427A-8AEB-54C790238687", "Successfully added to Calculation Queue."));
			}
		}

		protected virtual void QueueSourceItemsForRegeneration(IEnumerable<ViewCommissionLineGrouping> groupings)
		{
			var factoryForRegenerationQueue = new BusinessObjectFactory { NameForDebugging = "FactoryForAddingOrgCommissionCalculationQueue" };
			CommissionRegenerator.QueueSourceItemsForRegeneration(factoryForRegenerationQueue, groupings);
		}

		protected internal virtual void UndoCancel(BusinessObject[] selectedBusinessObjects)
		{
			if ((selectedBusinessObjects?.Length ?? 0) == 0)
			{
				Globals.Message.Show(NoCommissionGroupsSelectedMessage, Res.GetString("a0baa42-34a7-4a53-9d14-b60f86bb41d7", "Undo Cancellation"), MessageBoxButtons.OK, DialogResult.OK);
				return;
			}

			if (selectedBusinessObjects.Any(HasTypeErrorForSelectedBusinessObjects))
			{
				ShowIncorrectTypeErrorMessage();
			}
			else
			{
				var controller = (CommissionManagementController)GetNewController(selectedBusinessObjects[0]);
				controller.UndoCancel(selectedBusinessObjects);
			}
		}

		string NoCommissionGroupsSelectedMessage => Res.GetString("1443f8bf-d15e-42c1-ac23-ea3d84e064b6", "Please select a commission group");
		string RegenerateCommissionsCaption => Res.GetString("F751D76E-6D4E-438E-8D36-42C8839BB723", "Regenerate Commissions?");

		void OnApproveCommissionAgreementsMenuItemClick(object sender, EventArgs e)
		{
			if (!Env.Security.CommissionAgreementApproval.IsAllowed)
			{
				Env.Security.CommissionAgreementApproval.ShowError();
			}
			else
			{
				var wizard = new CommissionAgreementApprovalWizard(new BusinessObjectFactory());
				var approverForm = new CommissionAgreementApprovalWizardForm(wizard);
				approverForm.Show();
			}
		}

		protected virtual void OnCommissionFinaliserMenuItemClick(object sender, EventArgs e)
		{
			if (!Env.Security.CommissionFinaliser.IsAllowed)
			{
				Env.Security.CommissionFinaliser.ShowError();
			}
			else
			{
				var finalizer = new CommissionFinalizer();
				var finalizerForm = new CommissionFinalizerForm(finalizer);
				finalizerForm.Show();
			}
		}

		protected override ResourceStringData GetDeleteMenuItemText()
		{
			return Res.GetData("03218c02-df25-4e93-b547-3e7e1b77a4f0", "&Cancel", "Deletes the selected item after viewing its details read-only (shortcut Del)");
		}

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CommissionManager; }
		}

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.CommissionManager; }
		}

		#endregion

		#region Classes

		class CommissionManagementModuleDecisionProvider : DefaultModuleDecisionProvider
		{
			public CommissionManagementModuleDecisionProvider(ZFilterModule module)
				: base(module)
			{
			}

			public override bool ShouldDisplayNotifications
			{
				get { return true; }
			}
		}

		#endregion

		#region IImportCollectionInfoProvider Members

		string IImportCollectionInfoProvider.ContextKey
		{
			get { return "{5277B853-3DBC-48E9-A88D-8E3CE8287F06}"; }
		}

		IImportCollectionInfo IImportCollectionInfoProvider.ImportCollectionInfo
		{
			get
			{
				var collection = new CommissionFlattenedCollection(Factory);
				return new CommissionImportCollectionInfo(collection, OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.Value);
			}
		}

		#endregion

		#region ImportDataWizard

		protected override void RunImportDataWizard()
		{
			IImportCollectionInfoProvider importProvider = this;
			var flattenedCollection = new CommissionFlattenedCollection(Factory);
			var commissionLineCollection = new AccCommissionLineCollection(Factory);
			var flattenedCollectionInfo = new CommissionImportCollectionInfo(flattenedCollection, OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.Value);
			var flattenedProcessor = new CommissionFlattenedDataTransferProcessor(commissionLineCollection, flattenedCollectionInfo);
			var saveProcessor = new CommissionSaveProcessor(Factory);
			var importWizardForm = new CommissionImportWizardForm(flattenedCollectionInfo, importProvider.ContextKey, flattenedProcessor, saveProcessor);

			var isCancelled = false;
			importWizardForm.Cancelled += (sender, e) => { flattenedProcessor.Rollback(); isCancelled = true; };
			importWizardForm.Imported += (sender, e) => { DisplayImportDataWizardResult(flattenedProcessor, isCancelled); };
			importWizardForm.Show();
		}

		protected static void DisplayImportDataWizardResult(CommissionFlattenedDataTransferProcessor processor, bool isCancelled)
		{
			if (isCancelled)
			{
				Globals.Message.ShowInformation(Res.GetString("54404603-14f3-40fb-a1c9-45daa8b10eeb", "No entity commission was created."), Res.GetString("8d95f762-c502-460e-a41e-bdf32f2b4008", "Import Canceled"));
			}
			else
			{
				ZStringBuilder messageBuilder = new ZStringBuilder();
				messageBuilder.AppendLine(Res.GetString("864a95ac-36a4-4ca5-9672-727acb32fe97", "Entity Commissions to Import = {0}", processor.CommissionsToImport));
				messageBuilder.AppendLine();

				foreach (var log in processor.Logs)
				{
					messageBuilder.AppendLine(log);
				}

				messageBuilder.AppendLine();
				messageBuilder.AppendLine(Res.GetString("b4202dd0-4e8f-4998-80bd-7da01d79fda5", "TOTAL: New Commissions Created = {0}, Commissions Overridden = {1}, Commissions Ignored = {2}, Commissions Excluded = {3}", processor.NewCount, processor.OverriddenCount, processor.IgnoredCount, processor.ErrorCount));

				var caption = Res.GetString("3f1ce907-cd83-4cfe-95d8-00b16f99eccb", "Import Completed");
				using (ZMessageBox notification = new ZMessageBox(messageBuilder.ToString(), caption, MessageBoxButtons.OK, MessageBoxIcon.Information))
				{
					ZFormModaliser.ShowDialogAndDispose(notification);
				}
			}
		}

		#endregion
	}
}
