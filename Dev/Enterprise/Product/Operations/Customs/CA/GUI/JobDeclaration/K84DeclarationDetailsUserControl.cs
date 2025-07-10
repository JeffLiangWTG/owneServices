using System;
using System.Drawing;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class K84DeclarationDetailsUserControl : ZUserControl
	{
		public K84DeclarationDetailsUserControl()
		{
			InitializeComponent();
		}

		public void ChangeControlsVisibility(JobDeclaration declaration)
		{
			if (declaration != null)
			{
				var scheduledB3SendingDate = declaration.ScheduledB3SendingDate;
				var shouldDisplayB3ScheduleControl = scheduledB3SendingDate.IsValid;
				ScheduledB3DateEdit.Visible = shouldDisplayB3ScheduleControl;
				if (shouldDisplayB3ScheduleControl)
				{
					var isCADEnabled = declaration.IsCADEnabled;
					if (!declaration.HasScheduledB3Message || declaration.ScheduledB3AutoSendingDate == scheduledB3SendingDate)
					{
						if (isCADEnabled)
						{
							ScheduledB3DateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("bc349293-5c9e-42d9-ba89-106f5c53d758", "CAD Auto-Send Time", "Scheduled Auto-Sending Time For CAD Message", "");
						}
						else
						{
							ScheduledB3DateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("50f9674f-ae5b-47ec-b944-9e83aaae81d3", "Entry Auto-Send Time", "Scheduled Auto-Sending Time For Entry Message", "");
						}
						ScheduledB3DateEdit.DateTextBox.ColorChanger.ForceBackColor(SystemColors.Control);
					}
					else
					{
						if (isCADEnabled)
						{
							ScheduledB3DateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e4604ddf-1c12-4acc-80c2-f81300c3230a", "CAD Scheduled Time", "Scheduled Sending Time For CAD Message", "");
						}
						else
						{
							ScheduledB3DateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("555E907B-BC8E-4834-A112-AA6AC9D6153E", "Entry Scheduled Time", "Scheduled Sending Time For Entry Message", "");
						}
						ScheduledB3DateEdit.DateTextBox.ColorChanger.ForceBackColor(Color.Yellow);
					}
				}

				LowValueShipmentLabel.Visible = declaration.IsImport && declaration.IsLowValueNormalReleaseJob;
				CA_OGDStatusDescriptionTextBox.Visible = declaration.IsOGD || declaration.IsIID;
				if (!declaration.IsIID)
				{
					CA_OGDStatusDescriptionTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("41119b7d-e56a-4263-bcab-cceb7c80c54a", "OGD Status");
				}
				else
				{
					CA_OGDStatusDescriptionTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("25646a52-93db-46b0-a300-66b6bff57dd9", "PGA Status");
				}
				WarehouseTransactionStatusDescriptionTextBox.Visible = declaration.SupportsBondedWarehousing;

				B3AcceptedDateEdit.Visible = !shouldDisplayB3ScheduleControl && (declaration.IsB3Lodged || declaration.IsCADLodged);

				foreach (System.Windows.Forms.Control control in Controls)
				{
					control.UpdateCaption();
				}

				ChangeTransactionNumberControlVisibility(declaration);

				this.CADVersionTextBox.Visible = false;

				if (declaration.IsCADEnabled)
				{
					this.B3EntrySubmittedDateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("B5A920BF-234F-4F49-9FB5-38A66F670FDD", "CAD Submitted Date");
					this.B3AcceptedDateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("F6DA4684-8F74-40F0-8AB2-63A6BDB8FA11", "CAD Accepted Date");
					this.B3EntryStatusTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("601945F6-B91D-46FE-A390-D6146987A558", "CAD Entry Status");
					this.B3EntryMessageTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("1ECE3DC9-9823-4484-A51B-79EF51AE8800", "CAD Message Status");
					this.CADVersionTextBox.Visible = true;
				}
			}
		}

		void ChangeTransactionNumberControlVisibility(JobDeclaration declaration)
		{
			if (declaration.IsImportIncludingB2)
			{
				var effectiveBranch = declaration.EffectiveBranch;
				var isFormattedControlVisible = !CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.GetFallBackValueAtAllLevels(effectiveBranch.Company.PK.ToGuid(), effectiveBranch.PK.ToGuid(), Guid.Empty);
				FormattedTransactionNumberTextBox.Visible = isFormattedControlVisible;
				SecurityCodeTextBox.Visible = !isFormattedControlVisible;
				SequentialNumberTextBox.Visible = !isFormattedControlVisible;
				CheckDigitTextBox.Visible = !isFormattedControlVisible;
			}
		}
	}
}
