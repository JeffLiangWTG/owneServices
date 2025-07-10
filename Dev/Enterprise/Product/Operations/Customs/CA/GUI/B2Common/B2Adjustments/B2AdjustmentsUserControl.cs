using CargoWise.Types;
using CargoWise.Windows.UI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class B2AdjustmentsUserControl : BaseB2UserControl
	{
		public B2AdjustmentsUserControl() : base()
		{
			InitializeComponent();
		}

		protected override ZBool SupportTransactionNumberControl() => JobDeclaration?.IsImportIncludingB2 ?? ZBool.False;

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			var isB3X = JobDeclaration?.IsB3X ?? ZBool.False;
			this.B2TypeDropEdit.GetExtension<ILabelCaptionRenderer>().Caption =
				isB3X ? Enterprise.Customs.CA.GUI.Res.GetString("d9a50c7b-7f8e-4f53-bca5-f833d61a57c2", "B3X Type")
								: Enterprise.Customs.CA.GUI.Res.GetString("a48739c9-c9e0-447e-9f08-66921859daf6", "B2 Type");
			this.LongTextDetailsGroupBox.Visible = !isB3X;
			this.PaymentCodeDropEdit.Visible = isB3X;
			this.DocsAttachedCheckBox.Visible = !isB3X;
			this.IsOurFaultCheckBox.Visible = !isB3X;
			this.ClaimedInterestAmountCalcEdit.Visible = !isB3X;
			this.AnySightDepositAmountCalcEdit.Visible = !isB3X;
			this.AcceptedOverrideCheckBox.Visible = isB3X;
			this.SubmittedOverrideCheckBox.Visible = isB3X;
			this.TransportModeDropEdit.Visible = isB3X;
			this.CarrierCodeFindBox.Visible = isB3X;

			if (isB3X)
			{
				this.MailToOrganisationControlWithMiscellaneous.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("3A13753B-D8C9-4725-84FA-F85D25F58799", "Vendor");
				this.OriginalAccountingDateDateEdit.Location = ControlDpiScalingHelper.NewScaledPoint(148, 135, true);
				this.AuthorisationDateDateEdit1.Location = ControlDpiScalingHelper.NewScaledPoint(325, 135, true);
				this.SecurityNoTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(148, 161, true);
				this.AmendmentToDropEdit.Location = ControlDpiScalingHelper.NewScaledPoint(148, 187, true);
			}
			else
			{
				this.MailToOrganisationControlWithMiscellaneous.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("119425c5-889e-4ff2-abf3-ce54c4e0cf9b", "Mail To");
				this.OriginalAccountingDateDateEdit.Location = ControlDpiScalingHelper.NewScaledPoint(148, 89, true);
				this.AuthorisationDateDateEdit1.Location = ControlDpiScalingHelper.NewScaledPoint(325, 89, true);
				this.SecurityNoTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(148, 115, true);
				this.AmendmentToDropEdit.Location = ControlDpiScalingHelper.NewScaledPoint(148, 223, true);
			}
		}
	}
}
