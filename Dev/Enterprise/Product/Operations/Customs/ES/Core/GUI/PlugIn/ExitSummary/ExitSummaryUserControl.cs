using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.GUI.WarehouseExtensions;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.GUI
{
	public partial class ExitSummaryUserControl : EU.GUI.PlugIn.ExitSummaryUserControl
	{
		public ExitSummaryUserControl()
		{
			InitializeComponent();
			RemoveArrivalNotificationPlaceTextBox();
		}

		protected override void InitializeLayoutMovementsGrid()
		{
			base.InitializeLayoutMovementsGrid();
			var arrivalNotifPlaceColumnStyle = MovementsGrid.GetColumnStyle(CusExitDetail.Schema.CED_ArrivalNotificationPlace);
			MovementsGrid.ColumnStyles.Remove(arrivalNotifPlaceColumnStyle);

			MovementsGrid.ColumnStyles.AddRange(new Core.Forms.ZGridColumnInfo[]
			{
				new ZArchitecture.ZDateEditColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("A19A586F-AF33-4FE0-A6B9-58672FFBAAB9", "Acceptance Date"),
					ColumnName = CusExitDetail.Schema.ZG_AcceptanceDate,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Long,
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("521E8487-F886-4964-87C0-5307B2AE169E", "Circuit"),
					ColumnName = CusExitDetail.Schema.FormattedCircuit,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60),
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("AD1D605C-F278-4599-9BCD-C0A728ACA2B5", "CSV Clearance"),
					ColumnName = CusExitDetail.Schema.ZG_CSVClearance,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
				},
				new ZDropEditColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("DDAAD4EA-2F32-4204-AC6B-0682C668BE0D", "Arrival Notification Place"),
					ColumnName = CusExitDetail.Schema.CED_ArrivalNotificationPlace,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140),
				}
			});

			MovementsGrid.ContextMenu.MenuItems.Add(Res.GetString("22203e46-9b24-4d1b-bc16-200ebb392aef", "Update CSV Clearance"), UpdateCSVClearanceClick);
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			RegisterControlsForLock((CusExitControlHeader)CurrentDataItem);
		}

		#region Control Lock

		void RegisterControlsForLock(CusExitControlHeader exitHeader)
		{
			if (exitHeader != null && Env.Security.LockOrUnlockFileForEdit.IsAllowed)
			{
				if (((ICustomsFileParent)exitHeader).IsLocked)
				{
					foreach (CusExitDetail exitDetail in exitHeader.CusExitDetails)
					{
						if (exitHeader.CodesForUnlocking.Contains(exitDetail.CED_Status))
						{
							exitDetail.SetReadOnlyIncludingChildren(true);
						}
					}
				}
				else if (exitHeader.Logs.MostRecentLogByEventTime(AutoEvents.UnlockForEdit) != null)
				{
					foreach (CusExitDetail exitDetail in exitHeader.CusExitDetails)
					{
						if (exitHeader.CodesForUnlocking.Contains(exitDetail.CED_Status))
						{
							exitDetail.SetReadOnlyIncludingChildren(false);
						}
					}
				}
			}
		}
		#endregion

		protected override ZBool DynamicLayoutApplied => true;

		protected override IPanelLayoutProvider GetNewExitSummaryMainPanelLayout() => new ExitSummaryMainPanelLayout();

		protected override Type GetMessagesTabUserControlType() => typeof(MessagesTabUserControl);

		void RemoveArrivalNotificationPlaceTextBox()
		{
			MovementDetailsPanel.Controls.Remove(ArrivalNotificationPlaceTextBox);
		}

		void UpdateCSVClearanceClick(object sender, EventArgs ev)
		{
			var selectedElements = MovementsGrid.SelectedElements;
			if (selectedElements.Length != 1)
			{
				Globals.Message.Show(Res.GetString("5cc314a3-c797-4bfa-a8aa-c56b65e5937b", "Please select a single row first"));
			}
			else
			{
				var exitDetailSelected = (CusExitDetail)selectedElements[0];
				if (exitDetailSelected.CED_Status != EntryStatusCodes.CustomsDeclarationAccepted)
				{
					Globals.Message.Show(Res.GetString("80b8abb6-c772-4544-a90d-ccd08dffe313", "Please select only a movement with status CDA"));
				}
				else if (PromptUserToSaveIfHasChanges())
				{
					ZString csvCodeFromUser = Globals.Message.QueryUserResponse(
					new UserResponseArgument
					{
						Message = string.Format(Res.GetString("cc49bbcb-998d-499a-bbe4-9a59784ae59e", @"You are about to change the clearance number (CSV Clearance) of movement {0}.
Please make sure that the number that you are entering is the right clearance number.
New Clearance Number:"), exitDetailSelected.CED_MovementReferenceNumber),
						Caption = Res.GetString("00f94b92-1f13-44fd-bb35-4920d04bfe2c", "Update CSV Clearance"),
						Buttons = ZMessageBoxButtons.OKCancel,
						Icon = ZMessageBoxIcon.Question,
						DefaultButton = ZMessageBoxDefaultButton.Button1,
						MinimumResponseLength = 1,
						MaximumResponseLength = 16,
						UserResponseTextBoxCharactersCasing = ZCharacterCasing.Upper
					});

					UpdateAndSaveCSVClearance(exitDetailSelected, csvCodeFromUser);
				}
			}
		}

		void UpdateAndSaveCSVClearance(CusExitDetail exitDetail, ZString csvCodeFromUser)
		{
			try
			{
				var factory = new BusinessObjectFactory();
				var newFactoryExitDetail = factory.Load<CusExitDetail>(exitDetail.PK);
				newFactoryExitDetail.UpdateCSVClearance(csvCodeFromUser);
				factory.Save();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		bool PromptUserToSaveIfHasChanges()
		{
			bool okToContinue = true;
			var exitHeader = (CusExitControlHeader)CurrentDataItem;
			var topLevelBusinessObject = exitHeader.CEH_Parent;
			if (exitHeader.HasChanges || topLevelBusinessObject.HasChanges)
			{
				var confirmedToSave = Globals.Message.Show(
										Res.GetString("22486BAD-3002-48B1-983C-0FD2318BEC92", "The Job has not yet been saved. Do you want to save and proceed?"),
										Res.GetString("6E4A2482-69A3-4F86-BEFA-64DEB79EDF73", "Save Job"),
										MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) == DialogResult.Yes;

				okToContinue = confirmedToSave && (this.FireSaveButton() == ContinueWithSave.Yes);
			}

			return okToContinue;
		}
	}
}
