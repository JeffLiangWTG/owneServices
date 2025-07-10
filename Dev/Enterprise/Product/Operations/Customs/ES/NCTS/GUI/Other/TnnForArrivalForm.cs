using System;
using System.Drawing;
using CargoWise.Common;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public partial class TnnForArrivalForm : ZChildForm
	{
		public TnnForArrivalForm(TnnDataCodeInfo tnnForArrivalInfo)
			: base(tnnForArrivalInfo)
		{
			this.tnnForArrivalInfo = tnnForArrivalInfo;
			MessageLabel.Text = messageLabel;
			InitializeAcceptanceDateFormLayout();
			InitializeClearanceDateFormLayout();
		}
		readonly TnnDataCodeInfo tnnForArrivalInfo;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		void InitializeClearanceDateFormLayout()
		{
			ClearanceDate.DateTimeValue = tnnForArrivalInfo.ClearanceDate;
			ClearanceDate.DateTimeValueChanged += new EventHandler(ClearanceDateDateEditChanged);
			EnableOkButton();
		}

		void InitializeAcceptanceDateFormLayout()
		{
			AcceptanceDate.DateTimeValue = tnnForArrivalInfo.AcceptanceDate;
			AcceptanceDate.DateTimeValueChanged += new EventHandler(AcceptanceDateDateEditChanged);
			EnableOkButton();
		}

		void ClearanceDateDateEditChanged(object sender, EventArgs e)
		{
			EnableOkButton();
			ClearanceDateForeColor();
			SetCommandStatusClearanceDate();
			MainStatusBar.Refresh();
		}

		void AcceptanceDateDateEditChanged(object sender, EventArgs e)
		{
			EnableOkButton();
			AcceptanceDate.DateTextBox.ForeColor = IsAcceptanceDateInFuture || acceptanceDateIsOlder30daysFromCurrentDate ? Color.Red : base.ForeColor;
			ClearanceDateForeColor();
			SetCommandStatusAcceptanceDate();
			MainStatusBar.Refresh();
		}

		void ClearanceDateForeColor() => ClearanceDate.DateTextBox.ForeColor = (clearanceDateAndAcceptanceDateIsValid && !clearanceDateIsGreaterThanOrEqualToAcceptanceDate) || IsClearanceDateInFuture ? Color.Red : base.ForeColor;

		bool IsAcceptanceDateInFuture => AcceptanceDate.DateTimeValue.IsInTheFuture();
		bool IsClearanceDateInFuture => ClearanceDate.DateTimeValue.IsInTheFuture();
		bool acceptanceDateIsOlder30daysFromCurrentDate => AcceptanceDate.DateTimeValue.IsValid && AcceptanceDate.DateTimeValue < ZDateTime.Now.AddDays(-30);
		bool clearanceDateIsGreaterThanOrEqualToAcceptanceDate => AcceptanceDate.DateTimeValue.IsValid && ClearanceDate.DateTimeValue.IsValid && ClearanceDate.DateTimeValue >= AcceptanceDate.DateTimeValue;
		bool clearanceDateAndAcceptanceDateIsValid => AcceptanceDate.DateTimeValue.IsValid && ClearanceDate.DateTimeValue.IsValid;

		void EnableOkButton()
		{
			ClearanceDate.CaptionResourceString = clearanceDateIsGreaterThanOrEqualToAcceptanceDate ? ClearanceDateFullDescriptionOrigin : ClearanceDateFullDescriptionChange;
			AcceptanceDate.CaptionResourceString = acceptanceDateIsOlder30daysFromCurrentDate ? AcceptanceDateFullDescriptionChange : AcceptanceDateFullDescriptionOrigin;
			OKButton.Enabled = clearanceDateAndAcceptanceDateIsValid && clearanceDateIsGreaterThanOrEqualToAcceptanceDate && !IsAcceptanceDateInFuture && !IsClearanceDateInFuture;
		}

		void SetCommandStatusClearanceDate() => MessageStatusBarPanel.Text = ClearanceDate.CaptionResourceString.FullDescription.IsNullOrEmpty() ? ClearanceDate.CaptionResourceString.Caption : ClearanceDate.CaptionResourceString.FullDescription;
		void SetCommandStatusAcceptanceDate() => MessageStatusBarPanel.Text = AcceptanceDate.CaptionResourceString.FullDescription.IsNullOrEmpty() ? AcceptanceDate.CaptionResourceString.Caption : AcceptanceDate.CaptionResourceString.FullDescription;

		ResourceStringData ClearanceDateFullDescriptionChange => Res.GetData("DD265FE5-4C8E-424C-980D-5D38EE8953F6", "Clearance Date", "Clearance Date cannot be older than Acceptance Date");

		ResourceStringData ClearanceDateFullDescriptionOrigin => Res.GetData("689ACF3B-9933-42C9-A50A-97C41E9704CF", "Clearance Date");

		ResourceStringData AcceptanceDateFullDescriptionChange => Res.GetData("8C68368D-7701-4921-B22F-536DC6B32529", "Acceptance Date", "Acceptance date is older than 30 days from current date");

		ResourceStringData AcceptanceDateFullDescriptionOrigin => Res.GetData("BEB47513-6FDB-448C-8E45-1EF609563AC7", "Acceptance Date");

		public override string FormVerb => string.Empty;

		protected string messageLabel = ResString.GetMultilingualString("C972C127-2219-4D22-A0F9-9E77E20BFBDB", "Acceptance Date and Clearance Date are mandatory to create a TNN declaration:");
	}
}
