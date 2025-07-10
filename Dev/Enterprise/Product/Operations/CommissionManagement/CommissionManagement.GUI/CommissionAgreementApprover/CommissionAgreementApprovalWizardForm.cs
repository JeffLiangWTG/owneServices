using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.CommissionManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Module;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

[assembly: WTG.StaticAnalysis.Annotation.UsesConstants(typeof(CommissionAgreementFilterBusinessObject.FilterDescription))]

namespace Enterprise.CommissionManagement.GUI
{
	public partial class CommissionAgreementApprovalWizardForm : ZChildForm
	{
		public CommissionAgreementApprovalWizardForm(CommissionAgreementApprovalWizard wizard)
			: base(wizard)
		{
			if (!DesignModeFinder.IsDesigning)
			{
				this.BackColor = SystemDataRegistry.Instance.ColorTheme.TabBackgroundColor; // Set BackColor before calling InitializeComponent() so that child checkboxes inherit the BackColor
			}

			InitializeComponent();
			AddCommissionAgreementApprovalFilterControl();
			AddCommissionAgreementControl();
			SetEnabled();
			SubscribeOnIncluded();
		}

		void SubscribeOnIncluded()
		{
			if (BusinessEntity != null)
			{
				foreach (var item in BusinessEntity.CommissionAgreementApprovalItems)
				{
					item.IsIncludeInfo.ValueChanged -= OnIsIncludeInfoOnValueChanged;
					item.IsIncludeInfo.ValueChanged += OnIsIncludeInfoOnValueChanged;
				}
			}
		}

		void OnIsIncludeInfoOnValueChanged(object sender, EventArgs e)
		{
			SetEnabled();
		}

		void SetEnabled()
		{
			if (BusinessEntity != null)
			{
				var hasIncluded = BusinessEntity.HasIncludedItems;
				if (hasIncluded != ApproveButton.Enabled)
				{
					ApproveButton.Enabled = hasIncluded;
					DisapproveButton.Enabled = hasIncluded;
				}
			}
		}

		new CommissionAgreementApprovalWizard BusinessEntity
		{
			get { return (CommissionAgreementApprovalWizard)base.BusinessEntity; }
		}

		new CommissionAgreementApprovalWizard CurrentDataItem
		{
			get { return (CommissionAgreementApprovalWizard)base.CurrentDataItem; }
		}

		#region Load

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetupOptionsGroupBox();
			SetupCommissionAgreementApprovalFilterControlGrid();
			ViewQueueButton.Image = Icons.GetImage(IconTypes.ViewButtonRest);
		}

		#endregion

		#region Options

		void SetupOptionsGroupBox()
		{
			RefreshFromDateDateEdit();
			CurrentDataItem.FromTypeInfo.ValueChanged += FromTypeInfo_ValueChanged;
		}

		void FromTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			RefreshFromDateDateEdit();
		}

		void RefreshFromDateDateEdit()
		{
			FromDateDateEdit.Visible = CurrentDataItem != null && CurrentDataItem.FromTypeRequiresDate;
		}

		#endregion

		#region CommissionAgreementControl

		void AddCommissionAgreementControl()
		{
			commissionAgreementControl = CommissionAgreementControl.New();
			commissionAgreementControl.SuspendLayout();
			AgreementPreviewSplitContainer.Panel1.Controls.Add(commissionAgreementControl);
			commissionAgreementControl.AllowDrop = true;
			BindingSource.SetBindingMember(commissionAgreementControl, "");
			commissionAgreementControl.Dock = DockStyle.Fill;
			commissionAgreementControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
			commissionAgreementControl.Name = "CommissionAgreementControl";
			commissionAgreementControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 305);
			commissionAgreementControl.TabIndex = 0;
			commissionAgreementControl.ResumeLayout(true);
			commissionAgreementControl.PerformLayout();
		}

		CommissionAgreementControl commissionAgreementControl;

		#endregion

		#region CommissionAgreementApprovalFilterControlGrid

		void SetupCommissionAgreementApprovalFilterControlGrid()
		{
			RefreshStmALogFilterControl();
			commissionAgreementApprovalFilterControl.Grid.ListManager.CurrentChanged += CommissionAgreementApprovalFilterControlGrid_CurrentChanged;
		}

		void CommissionAgreementApprovalFilterControlGrid_CurrentChanged(object sender, EventArgs e)
		{
			RefreshStmALogFilterControl();
		}

		void CommissionAgreementApprovalFilterControlGrid_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Clicks == 2 && e.Button == MouseButtons.Left)
			{
				if (commissionAgreementApprovalFilterControl.Grid.HitTest(e.X, e.Y).Row > -1)
				{
					var current = commissionAgreementApprovalFilterControl.Grid.ListManager.GetCurrent() as CommissionAgreementApprovalItem;
					if (current != null)
					{
						ZControllerFactory.Create(ControllerIDs.OrgCommissionAgreement).ShowEditForm(current.CommissionAgreement);
					}
				}
			}
		}

		void SelectAllButton_Click(object sender, EventArgs e)
		{
			if (CurrentDataItem != null)
			{
				foreach (CommissionAgreementApprovalItem item in CurrentDataItem.CommissionAgreementApprovalItemCollection)
				{
					item.IsInclude = true;
				}
			}
		}

		void DeselectAllButton_Click(object sender, EventArgs e)
		{
			if (CurrentDataItem != null)
			{
				foreach (CommissionAgreementApprovalItem item in CurrentDataItem.CommissionAgreementApprovalItemCollection)
				{
					item.IsInclude = false;
				}
			}
		}

		#endregion

		#region StmALogFilterControl

		void RefreshStmALogFilterControl()
		{
			if (StmALogFilterControl != null)
			{
				AgreementPreviewSplitContainer.Panel2.Controls.Remove(StmALogFilterControl);
				StmALogFilterControl.Dispose();
				StmALogFilterControl = null;
			}

			if (LogsModule != null)
			{
				LogsModule.Dispose();
				LogsModule = null;
			}

			var current = commissionAgreementApprovalFilterControl.Grid.ListManager.GetCurrent() as CommissionAgreementApprovalItem;
			if (current != null && current.CommissionAgreement != null)
			{
				LogsModule = ZModuleFactory.Instance.Create((ModuleIDs.CommissionAgreementLogFilter)) as CommissionAgreementLogFilterModule;
				LogsModule.InitData(current.CommissionAgreement.GetMainVersion(), GetCommissionAgreementLogFilterBusinessObject);
				StmALogFilterControl = LogsModule.EmbeddedControl as CommissionAgreementLogFilterControl;
				StmALogFilterControl.Dock = DockStyle.Fill;
				AgreementPreviewSplitContainer.Panel2.Controls.Add(StmALogFilterControl);
			}
		}
		static CommissionAgreementLogFilterBusinessObject GetCommissionAgreementLogFilterBusinessObject(IStmALogParent master)
		{
			var masterAgreement = (OrgCommissionAgreement)master;
			return new CommissionAgreementLogFilterBusinessObject(masterAgreement.Draft ?? masterAgreement);
		}

		ZStmALogFilterControl StmALogFilterControl;

		CommissionAgreementLogFilterModule LogsModule;

		#endregion

		#region CommissionAgreementApprovalFilterControl

		void AddCommissionAgreementApprovalFilterControl()
		{
			commissionAgreementApprovalFilterControl = new CommissionAgreementApprovalFilterControl(BusinessEntity, new CommissionAgreementApprovalFilterBusinessObject()) { Dock = DockStyle.Fill };
			AgreementListSplitContainer.Panel1.Controls.Add(commissionAgreementApprovalFilterControl);
			commissionAgreementApprovalFilterControl.SearchPerformed += (o, e) => SubscribeOnIncluded();
			commissionAgreementApprovalFilterControl.Grid.MouseDown += CommissionAgreementApprovalFilterControlGrid_MouseDown;
			commissionAgreementApprovalFilterControl.Grid.AfterBind += (s, a) =>
			{
				commissionAgreementControl.SetDataBinding(commissionAgreementApprovalFilterControl.Grid.List as CommissionAgreementApprovalItemCollection, nameof(CommissionAgreementApprovalItem.CommissionAgreement));
			};
		}

		protected CommissionAgreementApprovalFilterControl commissionAgreementApprovalFilterControl;

		#endregion

		#region Approve Button

		void ApproveButton_Click(object sender, EventArgs e)
		{
			if (CurrentDataItem != null)
			{
				CurrentDataItem.Action = CommissionAgreementApprovalWizard.ActionType.Approve;
				CurrentDataItem.RunPreSaveValidation();
				if (CurrentDataItem.HasErrors())
				{
					var message = GetErrorMessageListIncludingChildren(CurrentDataItem);
					Globals.Message.Show(message, CannotApproveCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				var controller = new CommissionAgreementApproveProgressController();
				try
				{
					var lastApprovedSelectedAgreements = CurrentDataItem.CommissionAgreementApprovalItems.Where(x => x.IsInclude).Select(x => x.CommissionAgreement);
					var agreementsAndCancelledLines = new Dictionary<OrgCommissionAgreement, List<ViewCommissionLine>>();

					foreach (var agreement in lastApprovedSelectedAgreements)
					{
						var lines = CurrentDataItem.Factory.Load<ViewCommissionLine>(new ZQuery(ViewCommissionLineSchema.VCL_CA0, agreement.MainVersion.PK));
						lines.ForEach(line => line.Reload());

						var cancelledLines = lines.Where(x => x.IsCancelled && !x.IsOverriden && x.VCL_ShouldReinstate);

						if (cancelledLines.Any())
						{
							agreementsAndCancelledLines.Add(agreement, cancelledLines.ToList());
						}
					}

					if (agreementsAndCancelledLines.Any())
					{
						var excludeCancelledCommissionLinesForm = new ExcludeCancelledCommissionLinesForm(CurrentDataItem.Factory, agreementsAndCancelledLines);

						if (ZFormModaliser.ShowDialogAndDispose(excludeCancelledCommissionLinesForm, this) == DialogResult.OK)
						{
							ApproveAgreement(controller);
						}
					}
					else
					{
						ApproveAgreement(controller);
					}
				}
				catch (ZSaveException ex)
				{
					try
					{
						ZExceptionReporting.HandleSaveException(ex);
					}
					catch (ZSaveConcurrencyException)
					{
						Globals.Message.Show(Res.GetString("63ff619e-b7ae-4810-b1f3-fd412a2e0de9", "While you have been working with this form, another user has made changes which cannot be merged. Please re-open the form and try again."));
					}
					Close();
				}
			}
		}

		void ApproveAgreement(CommissionAgreementApproveProgressController controller)
		{
			controller.Show(CurrentDataItem.Approve, this);
			if (CurrentDataItem.ErrorMessages.Count > 0)
			{
				// If approval fails, state in the Factory can become invalid with partially merged agreements.
				// If something triggers saving of such Factory (e.g. Disapprove button), then this invalid state will be persisted to database.
				// To prevent saving of such corrupted state, this form is made readonly after approval error.
				SetReadOnlyIncludingChildren();

				Globals.Message.ShowError(string.Join(System.Environment.NewLine, CurrentDataItem.ErrorMessages));
			}
		}

		#endregion

		#region Disapprove Button

		void DisapproveButton_Click(object sender, EventArgs e)
		{
			if (CurrentDataItem != null)
			{
				CurrentDataItem.Action = CommissionAgreementApprovalWizard.ActionType.Disapprove;
				CurrentDataItem.RunPreSaveValidation();
				if (CurrentDataItem.HasErrors())
				{
					var message = GetErrorMessageListIncludingChildren(CurrentDataItem);
					Globals.Message.Show(message, CannotDisapproveCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				try
				{
					CurrentDataItem.Disapprove();
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
					Close();
				}
			}
		}

		#endregion

		#region Append Button

		void AppendButton_Click(object sender, EventArgs e)
		{
			var appendForm = GetNewAppendPopupForm();
			ZFormModaliser.ShowDialogAndDispose(appendForm);
			SubscribeOnIncluded();
		}

		EmbeddedModulePopup GetNewAppendPopupForm()
		{
			var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.OrgCommissionAgreement);
			var popup = new EmbeddedModulePopup(module);

			var moduleDecisionProvider = new AppendCommissionAgreementItemModuleDecisionProvider(popup, BusinessEntity);
			module.OverrideModuleDecisionProvider(moduleDecisionProvider);
			popup.EmbeddedModulePopupOKButtonStrategy = moduleDecisionProvider;

			return popup;
		}

		#endregion

		#region Form Captions

		public override string FormVerb
		{
			get { return ""; }
		}

		#endregion

		#region Dispose

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (CurrentDataItem != null)
			{
				CurrentDataItem.FromTypeInfo.ValueChanged -= FromTypeInfo_ValueChanged;
				if (commissionAgreementApprovalFilterControl.Grid.ListManager != null)
				{
					commissionAgreementApprovalFilterControl.Grid.ListManager.CurrentChanged -= CommissionAgreementApprovalFilterControlGrid_CurrentChanged;
				}

				foreach (var item in CurrentDataItem.CommissionAgreementApprovalItems)
				{
					item.IsIncludeInfo.ValueChanged -= OnIsIncludeInfoOnValueChanged;
				}
			}

			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}

				if (LogsModule != null)
				{
					LogsModule.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Messages

		static string GetErrorMessageListIncludingChildren(IBusiness business)
		{
			var notificationCollector = new ZNotificationCollector(business, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName)
				.Where(x => x.Type == CargoWise.EntityFramework.NotificationType.Error);
			return notificationCollector.ToMessageListString();
		}

		static string CannotApproveCaption
		{
			get { return Res.GetString("d2663195-8d3f-4158-bdad-fa8f4a01bf30", "Cannot Approve"); }
		}

		static string CannotDisapproveCaption
		{
			get { return Res.GetString("a5710d52-ca11-4bd0-8b01-5ff6884afdf9", "Cannot Disapprove"); }
		}

		#endregion

		void ViewQueueButton_Click(object sender, EventArgs e)
		{
			using (var module = (CommissionAgreementModule)ZModuleFactory.Instance.Create(ModuleIDs.OrgCommissionAgreement))
			{
				var defaults = new FilterBusinessObjectDefaults
				{
					new FilterBusinessObjectDefault(CommissionAgreementFilterBusinessObject.FilterDescription.IsInCalculationQueue,
						"Property0", ZBool.True, false)
				};

				module.FilterBusinessObject.SetExternalDefaults(defaults);
				module.ShouldShowCalculationQueueActionMenuItems = true;
				var popup = new EmbeddedModulePopup(module);
				popup.Closed += delegate
				{
					popup.Dispose();
				};

				ZFormModaliser.ShowDialogAndDispose(popup);
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
