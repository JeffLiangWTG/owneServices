using System;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class RNSRequestForm : ZChildForm
	{
		public RNSRequestForm()
		{
		}

		public RNSRequestForm(RNSRequestBO bizObj)
			: base(bizObj)
		{
			AutoSendMessage = true;
		}

		RNSRequestBO RNSRequestBO
		{
			get { return (RNSRequestBO)BusinessEntity; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			DateOfArrivalZDateEdit.Visible = RNSRequestBO != null && RNSRequestBO.IsArrivalCertification;
			OfficeCodeFindBox.Visible = RNSRequestBO != null && RNSRequestBO.IsArrivalCertification;
			SubLocationCodeFindBox.Visible = RNSRequestBO != null && RNSRequestBO.IsArrivalCertification;
			TransactionNumberTextBox.Visible = RNSRequestBO.IsTransactionNumberApplicable;

			if (RNSRequestBO != null && RNSRequestBO.IsArrivalCertification && !RNSRequestBO.IsTransactionNumberApplicable)
			{
				DateOfArrivalZDateEdit.Location = ControlDpiScalingHelper.NewScaledPoint(DateOfArrivalZDateEdit.Location.X, DateOfArrivalZDateEdit.Location.Y - ControlDpiScalingHelper.ScaleToCurrentDpiY(33), false);
				OfficeCodeFindBox.Location = ControlDpiScalingHelper.NewScaledPoint(OfficeCodeFindBox.Location.X, OfficeCodeFindBox.Location.Y - ControlDpiScalingHelper.ScaleToCurrentDpiY(33), false);
				SubLocationCodeFindBox.Location = ControlDpiScalingHelper.NewScaledPoint(SubLocationCodeFindBox.Location.X, SubLocationCodeFindBox.Location.Y - ControlDpiScalingHelper.ScaleToCurrentDpiY(33), false);
			}
		}

		public override string FormHeading
		{
			get { return Res.GetString("9ec55ed4-93fe-45b1-9caf-00f73f659134", "RNS Request"); }
		}

		public bool AutoSendMessage { get; set; }

		void SendButton_Click(object sender, EventArgs e)
		{
			if (AutoSendMessage)
			{
				var manager = new RNSMessageManager(RNSRequestBO, GetNewUserNotification(), !RNSRequestBO.IsArrivalCertification);
				if (manager.SendMessage(MessageSubTypes.Request))
				{
					Close();
				}
			}
			else
			{
				DialogResult = System.Windows.Forms.DialogResult.OK;
				Close();
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		protected virtual IUserNotification GetNewUserNotification()
		{
			return new MessageInstructionUserNotification();
		}
	}
}
