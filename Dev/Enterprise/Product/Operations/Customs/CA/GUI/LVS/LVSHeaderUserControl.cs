using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class LVSHeaderUserControl : BaseCustomsEntryUserControl
	{
		public LVSHeaderUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			JobDeclaration.JE_OH_ImporterInfo.ValueChanged += JE_OH_ImporterInfo_ValueChanged;
			if (!JobDeclaration.IsInDatabase && !JobDeclaration.JE_OH_Importer.IsEmpty)
			{
				JE_OH_ImporterInfo_ValueChanged(this, null);
				if (JobDeclaration.DisplaySequentialOfTransactionNumberSeparately)
				{
					JobDeclaration.TransactionNumber.SetAccountSecurityNo();
				}
			}

			JobDeclaration.CA_DeclarationExceptionInfo.ValueChanged += CA_DeclarationExceptionInfo_ValueChanged;
			ChangeTransactionNumberControlVisibility();
		}

		void ChangeTransactionNumberControlVisibility()
		{
			if (JobDeclaration != null && JobDeclaration.IsLVS)
			{
				var isFormattedControlVisible = !JobDeclaration.DisplaySequentialOfTransactionNumberSeparately;
				FormattedTransactionNumberTextBox.Visible = isFormattedControlVisible;
				SecurityCodeTextBox.Visible = !isFormattedControlVisible;
				SequentialNumberTextBox.Visible = !isFormattedControlVisible;
				CheckDigitTextBox.Visible = !isFormattedControlVisible;
			}
		}

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			base.HandleDeclarationControlVisibilityChangedCore();
			ChangeB3ScheduleControlVisibility();
			ChangeCADSubmittedDateEditAndCADStatusTextBoxVisibility();
			this.AllowOICCheckBox.Visible = JobDeclaration.JE_MessageSubType != LowValueShipmentsTypes.Codes.TotalConsolidation;
		}

		void ChangeB3ScheduleControlVisibility()
		{
			var declaration = JobDeclaration;
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
			CA_DeclarationExceptionInfo_ValueChanged(this, null);
		}

		void ChangeCADSubmittedDateEditAndCADStatusTextBoxVisibility()
		{
			var visible = JobDeclaration.IsCADEnabled;
			CADSubmittedDateEdit.Visible = visible;
			CADStatusTextBox.Visible = visible;
		}

		void CA_DeclarationExceptionInfo_ValueChanged(object sender, EventArgs e)
		{
			ExceptionDescriptionLabel.Visible = JobDeclaration != null && !JobDeclaration.CA_DeclarationException.IsEmpty;
		}

		void JE_OH_ImporterInfo_ValueChanged(object sender, EventArgs e)
		{
			if (JobDeclaration.TransactionNumber.CanChange)
			{
				var importerAddInfo = JobDeclaration.ImporterAddInfo;
				if (importerAddInfo != null && importerAddInfo.HasAccountSecurityNumber)
				{
					JobDeclaration.CA_UseImporterAccountSecurityNumber = CACustomsDataRegistry.Instance.AlwaysUseImporterAccountSecurity.Value ||
						Globals.Message.Show(
						Res.GetString("89185DC5-F471-4C0B-9503-B01EE0700669", "Do you want to use the Importer's Account Security Number instead of yours?"),
						Res.GetString("892FE048-2AC0-4FDD-9947-160E81FCDA18", "Importer Account Security Number"),
						MessageBoxButtons.YesNo,
						DialogResult.Yes) == DialogResult.Yes;
				}
			}
		}

		new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
		}
	}
}
