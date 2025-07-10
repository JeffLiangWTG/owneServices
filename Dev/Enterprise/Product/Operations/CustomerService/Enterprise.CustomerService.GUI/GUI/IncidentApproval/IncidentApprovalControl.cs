using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.CustomerService.Business;
using Enterprise.DocumentScanning.Integration;
using Enterprise.EConversation.GUI;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Balloons;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CustomerService.GUI
{
	public partial class IncidentApprovalControl : ZUserControl
	{
		public IncidentApprovalControl()
		{
			InitializeComponent();

			ServiceRequestLabel.Font = new System.Drawing.Font("Arial", 30F, System.Drawing.FontStyle.Bold);

			if (!DesignModeFinder.IsDesigning)
			{
				DisplayWarningMessageIfNonProductionSystem();
			}
		}

		void DisplayWarningMessageIfNonProductionSystem()
		{
			var key = ObjectFactory.Get<IProductRegistration>().Key;
			if (key.DatabaseType != DatabaseTypes.Codes.Production)
			{
				WarningMessgeLabel.Text = Res.GetString("b4e2b0a1-c0ea-4074-95cc-7d7e9e761f97", "WARNING: You are raising an eRequest from a test system. Information recorded in test systems is not retained long term. It is advised you only raise eRequests from your production system.");
				WarningMessgeLabel.Visible = true;
			}
		}

		IncidentApproval Incident
		{
			get { return (IncidentApproval)BindingSource.Current; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (Incident != null)
			{
				SetModuleDropEditVisibility();
				SetControlsVisibleAndEnable();
				Incident.IA_CriticalityInfo.ValueChanged += IA_CriticalityInfo_ValueChanged;
				CriticalityDropEdit.Validated += CriticalityDropEdit_Validated;

				if (EConversationGroupBox.Visible)
				{
					conversationControl = new EConversationMessageListUserControl(true);
					conversationControl.SetDataBinding(Incident, "EConversation");
					conversationControl.Dock = DockStyle.Fill;
					EConversationGroupBox.Controls.Add(conversationControl);
					Incident.EConversation.NewMessageAdded += (sender, eventArgs) => { conversationControl.RefreshMessages(); };
				}
			}
		}

		EConversationMessageListUserControl conversationControl;
		ZString lastCriticality;

		public void RefreshEConversationControl()
		{
			if (conversationControl != null)
			{
				conversationControl.RefreshMessages();
			}
		}

		void CriticalityDropEdit_Validated(object sender, EventArgs e)
		{
			if (Incident.IA_Criticality != lastCriticality)
			{
				if (Incident.IA_Criticality == Constants.CustomerService.CriticalityCodes.CR5_Training)
				{
					var parentForm = FindForm();
					var eLearningNoticeForm = new ELearningNoticeForm();
					ZFormModaliser.Show(eLearningNoticeForm, parentForm);

					var x = parentForm.Left + (parentForm.Width - eLearningNoticeForm.Width) / 2;
					var y = parentForm.Top + (parentForm.Height - eLearningNoticeForm.Height) / 2;
					eLearningNoticeForm.Location = ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(x), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(y));
				}

				lastCriticality = Incident.IA_Criticality;
			}
		}

		public void FocusOnIncidentCriticalityAndShowToolTip()
		{
			if (Incident != null)
			{
				Incident.Validation.ValidateIA_Criticality();
				ActiveControl = CriticalityDropEdit.CodeBox;
				var notificationExtension = CriticalityDropEdit.Extensions.Get<INotificationExtension>();
				if (notificationExtension != null)
				{
					Balloon.Instance.Show(notificationExtension.Notifications.GetUniqueNotifications(), CriticalityDropEdit, CriticalityDropEdit.CodeBox.ClientRectangle, false);
				}
			}
		}

		public void SetCriticality(string criticality)
		{
			this.CriticalityDropEdit.Text = criticality;
		}

		#region Event Handlers

		void IncidentApprovalControl_DragDrop(object sender, DragEventArgs e)
		{
			Incident.AttachedEDocs.Load();
		}

		void IA_CriticalityInfo_ValueChanged(object sender, EventArgs e)
		{
			SetModuleDropEditVisibility();
		}

		internal void SetModuleDropEditVisibility()
		{
			MenuSectionDropEdit.Visible = Incident != null && Incident.ModuleType == ModuleListType.MenuSection;
			Cr8ModuleDropEdit.Visible = Incident != null && Incident.ModuleType == ModuleListType.Cr8;
			Cr9ModuleDropEdit.Visible = Incident != null && Incident.ModuleType == ModuleListType.Cr9;
		}

		void SetControlsVisibleAndEnable()
		{
			EConversationGroupBox.Visible = Incident.HasBeenSent && !Incident.IA_IncidentNumber.IsEmpty;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
#if DEBUG
		protected
#endif
 void ReopenIncident(ZString newStatus)
		{
			string sql = string.Format("UPDATE {0} SET {1} = '{2}' WHERE {3} = '{4}'",
				IncidentApprovalSchema.Constants.TableName,
				IncidentApprovalSchema.Constants.IA_Status,
				newStatus,
				IncidentApprovalSchema.Constants.PK,
				Incident.PK);

			using (var command = Db.Connection.Command(sql))    // To bypass unnecessary save concurrency check because it only changes status
			{
				command.ExecuteNonQuery();
			}

			Incident.Reload();
		}

		void IncidentSummaryTextBox_Enter(object sender, EventArgs e)
		{
			IncidentSummaryAndDetailsTooltip.Show(IncidentSummaryAndDetailsToolTipHintMessage, IncidentSummaryTextBox, toolTipRelativePositionX, toolTipRelativePositionY);
		}

		void IncidentSummaryTextBox_Leave(object sender, EventArgs e)
		{
			IncidentSummaryAndDetailsTooltip.Hide(IncidentSummaryTextBox);
		}

		void IncidentSummaryTextBox_MouseLeave(object sender, EventArgs e)
		{
			IncidentSummaryAndDetailsTooltip.Hide(IncidentSummaryTextBox);
		}

		void IncidentDetailsTextBox_Enter(object sender, EventArgs e)
		{
			IncidentSummaryAndDetailsTooltip.Show(IncidentSummaryAndDetailsToolTipHintMessage, IncidentDetailsTextBox, toolTipRelativePositionX, toolTipRelativePositionY);
		}

		void IncidentDetailsTextBox_Leave(object sender, EventArgs e)
		{
			IncidentSummaryAndDetailsTooltip.Hide(IncidentDetailsTextBox);
		}

		void IncidentDetailsTextBox_MouseLeave(object sender, EventArgs e)
		{
			IncidentSummaryAndDetailsTooltip.Hide(IncidentDetailsTextBox);
		}

		ToolTip IncidentSummaryAndDetailsTooltip
		{
			get
			{
				if (toolTip == null)
				{
					toolTip = new ToolTip();
					toolTip.IsBalloon = true;
				}
				return toolTip;
			}
		}
		ToolTip toolTip;

		static string IncidentSummaryAndDetailsToolTipHintMessage
		{
			get { return Res.GetString("ec9f1bd5-b42a-4ea7-a4ef-4d9999acfd3c", "This must be explanatory.\r\ni.e. One-Stop ComTrac updated the consol but not the shipment. NOT \"Having problem with software\"."); }
		}

		const int toolTipRelativePositionX = 150;
		const int toolTipRelativePositionY = -70;

		#endregion

		void DocsGrid_DoubleClick(object sender, EventArgs e)
		{
			if (DocsGrid.SelectedElements.Length > 0 &&
				DocsGrid.List[DocsGrid.HitTest(DocsGrid.PointToClient(Cursor.Position)).Row] != null)
			{
				var eDoc = ((IncidentApprovalAttachment)DocsGrid.SelectedElements[0]).EDoc;
				if (eDoc != null && !eDoc.IsDeleted)
				{
					StorageDocsViewer.View(eDoc, false);
				}
			}
		}

		IStorageDocsViewer StorageDocsViewer
		{
			get
			{
				if (storageDocsViewer == null)
				{
					storageDocsViewer = ObjectFactory.Get<IStorageDocsViewer>();
					storageDocsViewer.Initialise(DocsGrid, EDocsPlugin);
				}
				return storageDocsViewer;
			}
		}
		IStorageDocsViewer storageDocsViewer;

		IEDocsPlugIn EDocsPlugin
		{
			get
			{
				if (eDocsPlugin == null)
				{
					var zform = FindForm() as ZForm;
					if (zform != null)
					{
						foreach (var plugin in zform.PlugIns.Instances)
						{
							if (plugin is IEDocsPlugIn)
							{
								eDocsPlugin = (IEDocsPlugIn)plugin;
								eDocsPlugin.ForceSetup();
								break;
							}
						}
					}
				}

				return eDocsPlugin;
			}
		}
		IEDocsPlugIn eDocsPlugin;
	}
}
