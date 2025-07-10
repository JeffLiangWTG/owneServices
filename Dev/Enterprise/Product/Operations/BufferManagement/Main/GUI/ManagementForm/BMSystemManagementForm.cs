using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.GUI
{
	public partial class BMSystemManagementForm : ZTemplateForm, IFilterPreviewableWithSubObject, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public BMSystemManagementForm(BMSystem system)
			: base(system)
		{
			InitializeComponent();
			ConfigureLivenessControls();
			ConfigureUpdateRelatedWorkflowsButton();

			SystemIsLiveToggleButton.Click += ToggleSystemIsLive; // the winforms designer likes to remove this event :)
		}

		#region Properties

		public BMSystem BMSystem => (BMSystem)BusinessEntity;

		#endregion

		#region Events

		void ToggleSystemIsLive(object sender, EventArgs e)
		{
			if (!BMSystem.FS_IsLive)
			{
				BMSystem.RunPreSaveValidation();

				if (BMSystem.HasErrors)
				{
					Globals.Message.Show(OnToggleWithErrorMessageText, OnToggleWithErrorCaptionText, MessageBoxButtons.OK, MessageBoxIcon.Error);

					return;
				}
			}

			var promptMessage = BMSystem.FS_IsLive ? OnToggleToNotLiveText : OnToggleToLiveText;
			var promptCaption = BMSystem.FS_IsLive ? ToggleMessageCaptionToNotLive : ToggleMessageCaptionToLive;
			var userResponse = Globals.Message.Show(promptMessage, promptCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

			if (userResponse != DialogResult.Yes)
			{
				return;
			}

			var oldValue = BMSystem.FS_IsLive;
			BMSystem.FS_IsLive = !oldValue;

			try
			{
				BMSystem.Factory.Save();
			}
			catch (ZSaveException)
			{
				BMSystem.FS_IsLive = oldValue;
				throw;
			}

			if (BMSystem.FS_IsLive)
			{
				BMSRegistry.Instance.ProcessAllTransferRulesLinksOnNextBMSRun.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			}

			ConfigureLivenessControls();
			ConfigureUpdateRelatedWorkflowsButton();
		}

		#endregion

		#region Is Live

		#region Constant Strings

		static readonly MultilingualString ToggleMessageCaptionToLive = ResString.GetMultilingualString("FC0B4319-34ED-46C0-AD1B-38122015D41B", "Attempting to set System to Live");
		static readonly MultilingualString ToggleMessageCaptionToNotLive = ResString.GetMultilingualString("A59FA8FD-1DA4-454C-879B-1D271FB33B96", "Attempting to set System to Not Live");

		static readonly MultilingualString OnToggleToLiveText = ResString.GetMultilingualString("176F0A68-DC26-4B62-AFB1-0A070F3DA6D4", @"You are trying to set this Buffer Management System to Live.
This means:
 " + BMConstants.BulletPointCharacter + @" You will be unable to edit components or component links for this System.
 " + BMConstants.BulletPointCharacter + @" Service tasks will start to operate on this System.
 " + BMConstants.BulletPointCharacter + @" All currently-pending changes will be saved.
Are you sure you wish to continue?");

		static readonly MultilingualString OnToggleToNotLiveText = ResString.GetMultilingualString("9DF29544-1E5C-4BA8-8993-41A89D95F9FA", @"You are trying to set this Buffer Management System to Not Live.
This means:
 " + BMConstants.BulletPointCharacter + @" You will be able to edit components and component links for this System.
 " + BMConstants.BulletPointCharacter + @" Service tasks will not operate on this System.
 " + BMConstants.BulletPointCharacter + @" All currently-pending changes will be saved.
Are you sure you wish to continue?");

		static readonly MultilingualString OnToggleWithErrorMessageText = ResString.GetMultilingualString("8FE052A0-4952-4225-8655-9A8F5CDB85D8", "Please resolve errors before performing this action.");
		static readonly MultilingualString OnToggleWithErrorCaptionText = ResString.GetMultilingualString("D686B41F-DA95-4CAD-AD9A-E6646B6A1E68", "Error");

		static readonly MultilingualString IsLiveToggleText = ResString.GetMultilingualString("EAB7385F-97A7-4983-AD19-555E1C0DE34B", "Configure Components");
		static readonly MultilingualString IsNotLiveToggleText = ResString.GetMultilingualString("8CF71AB5-0B63-40A1-9CF4-FC7EA4B30945", "Activate System");

		#endregion

		#region Liveness-swapping functionality

		void ConfigureLivenessControls()
		{
			SetupLivenessIndicatorControls();

			if (BMSystem.FS_IsLive)
			{
				SetCriticalBizosToReadOnly();
			}
			else
			{
				SetCriticalBizosToWriteable();
			}
		}

		void SetupLivenessIndicatorControls()
		{
			SystemIsLiveToggleButton.Text = GetCurrentLivenessText();
			SystemIsLiveToggleButton.ToolTipCaption = GetCurrentLivenessText();
		}

		MultilingualString GetCurrentLivenessText()
		{
			return BMSystem.FS_IsLive ? IsLiveToggleText : IsNotLiveToggleText;
		}

		void SetCriticalBizosToReadOnly()
		{
			SetCriticalBizos(true);
		}

		void SetCriticalBizosToWriteable()
		{
			SetCriticalBizos(false);
		}

		void SetCriticalBizos(bool readOnly)
		{
			BMSystem.Components.SetReadOnlyIncludingChildren(readOnly);
			foreach (var component in BMSystem.Components)
			{
				foreach (var link in component.FromMeToOthersLinks)
				{
					link.ReadOnly = readOnly;
					link.FilterRule.ReadOnly = readOnly;
				}
			}

			// repaint things
			ComponentsUserControl.FilterRulesControl.ReadOnly = readOnly;
			ComponentsUserControl.UpdateBindingForSelectedLinkChange();
			ComponentsUserControl.FilterRulesControl.Refresh();
		}

		#endregion

		#endregion

		#region Update Related Workflows

		void ConfigureUpdateRelatedWorkflowsButton()
		{
			UpdateRelatedWorkflowsButton.Enabled = BMSystem.FS_IsLive;
		}

		void UpdateRelatedWorkflowsButton_Click(object sender, EventArgs e)
		{
			if (!BMSRegistry.Instance.EnableResponsivePAVEDataProcessing.Value
				|| !BMSRegistry.Instance.EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges.Value)
			{
				Globals.Message.Show(message: ResString.GetMultilingualString("B1549EF4-77D1-4DC3-AA1E-20985D89444A", "This button is only available when both Workflow Manager > Buffer Management > Responsive PAVE Data Processing > Enable Responsive PAVE Data Processing and Enable Responsive Workflow Updates On Related Object Changes are set in the registry"),
					caption: ResString.GetMultilingualString("0B537366-B2AC-463E-B4DA-F4B691542F32", "Responsive Workflow Updates Are Disabled In The Registry"),
					MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}

			var userResponse = Globals.Message.Show(message: ResString.GetMultilingualString("8CEFA24A-5FB8-40B4-971B-66553BC15036", @"This operation will force the workflows related to this buffer management system to update their properties related to transfer and release such as Current Component, Dedicated Buffer, Effective Branch and Department and so on.
Normally these properties are updated responsively when workflows or their related objects change.
This operation maybe useful for troubleshooting. However, it will require performing multiple operations on each of the system related workflows and thus will increase a load on the system for a period of time.
Are you sure you wish to continue?"),
				caption: ResString.GetMultilingualString("6DAF95AB-A96A-4126-A723-D524B0052948", "Updating Related Workflows"),
				MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

			if (userResponse != DialogResult.Yes)
			{
				return;
			}

			BMSystem.UpdateRelatedWorkflows();

			var caption = ResString.GetMultilingualString("D4517B5B-E084-4B09-9501-A88392C8F14D", "Updating Related Workflows Initiated");
			ResourceString message = null;
			var delay = BMSRegistry.Instance.DelayForResponsiveWorkflowUpdatesOnRelatedObjectChanges.Value;

			if (delay == 0)
			{
				message = ResString.GetMultilingualString("ED9BB243-8AB4-4496-9F5C-A3FD4886BE2A", "Updating related workflows has been initiated. It will take some time to process every related workflow.");
			}
			else if (delay == 1)
			{
				message = ResString.GetMultilingualString("646A59EF-1EEE-4E24-B09F-E852336D0000", "Updating related workflows will be initiated in 1 minute (defined by the Delay For Responsive Workflow Updates On Related Object Changes registry item). It will take some time to process every related workflow.");
			}
			else
			{
				message = ResString.GetMultilingualString("A8C5D98B-110F-4C23-858D-58AFD0BA0AB2", "Updating related workflows will be initiated in {0} minutes (defined by the Delay For Responsive Workflow Updates On Related Object Changes registry item). It will take some time to process every related workflow.", delay);
			}
			Globals.Message.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		#endregion

		#region Fetch Hints

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			var system = dataSource as BMSystem;
			if (system != null)
			{
				var factory = system.Factory;
				SetBMSystemFetchHints(factory, system);
			}
		}

		void SetBMSystemFetchHints(BusinessObjectFactory factory, BMSystem system)
		{
			foreach (var component in system.Components)
			{
				factory.AddFetchHint(BMComponentLinkSchema.FL_FC_ComponentTo, component.PK);
				factory.AddFetchHint(BMComponentLinkSchema.FL_FC_ComponentFrom, component.PK);
				factory.AddFetchHint(BMComponentResourceLinkSchema.FD_FC_Component, component.PK);
			}

			SetFilterRuleFetchHints(factory, system);
		}

		void SetFilterRuleFetchHints(BusinessObjectFactory factory, BMSystem system)
		{
			foreach (var component in system.Components)
			{
				foreach (var link in component.FromMeToOthersLinks)
				{
					factory.AddFetchHint(StmModuleFilterSchema.S9_ParentID, link.PK);
				}
			}

			foreach (var component in system.Components)
			{
				foreach (var link in component.FromMeToOthersLinks)
				{
					factory.AddFetchHint(StmModuleFilterUserDataSchema.S0_S9, link.FilterRule.PK);
				}
			}
		}

		#endregion

		#region ZTemplateForm Overrides

		protected override bool SupportsEDocs => true;

		protected override bool ShowAuditTab => true;

		#endregion

		#region IFilterPreviewableWithSubObject Members

		public BusinessObject GetObjectForPreview(string filterControlIdentifier)
		{
			if (filterControlIdentifier == BMComponentsUserControl.ComponentLinkFilterIdentifier)
			{
				return ComponentsUserControl.ComponentLinkGrid.GetCurrent();
			}

			throw new UnidentifiedFilterControlException(BMFilterStripWrapperControl.UnidentifiedFilterControlExceptionMessage);
		}

		#endregion

		#region IAllowTabBackwardBetweenSomeOfMyChildren Members

		bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
		{
			return control == IsLiveTextBox && previousControl == ResourceCountdownTimeEdit;
		}

		#endregion

		void ExperimentalSettingsButton_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new ExperimentalSettingsForm(
				new ExperimentalSettingsProvider(((BMSystem)BusinessEntity).PK, new BusinessObjectFactory())));
		}

#if DEBUG
		#region Test Accessability

		public static string ToggledToLiveMessage => OnToggleToLiveText;
		public static string ToggledToDeadMessage => OnToggleToNotLiveText;

		public static string ToggleWithErrorMessage => OnToggleWithErrorMessageText;

		#endregion
#endif
	}
}
