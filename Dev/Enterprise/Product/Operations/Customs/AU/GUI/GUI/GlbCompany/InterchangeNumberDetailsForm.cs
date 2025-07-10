using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class InterchangeNumberDetailsForm : ZChildForm
	{
		public InterchangeNumberDetailsForm(GlbCompany company) : this(new InterchangeNumberDetails(company))
		{
		}

		public InterchangeNumberDetailsForm(InterchangeNumberDetails interchangeNumberDetails) : base(interchangeNumberDetails)
		{
			InterchangeNumberDetails = interchangeNumberDetails;
			InitializeComponent();
			CheckEnableForOKButton(null, null);
		}

		InterchangeNumberDetails InterchangeNumberDetails { get; }

		public static MultilingualString MessageCaption => ResString.GetMultilingualString("AU.InterchangeNumberDetailsForm|MessageCaption", "Interchange Number Details Form");

		public static MultilingualString SavedSuccessfully => ResString.GetMultilingualString("AU.InterchangeNumberDetailsForm|SavedSuccessful", "Interchange Number saved successfully.");

		public static MultilingualString FailedToSave => ResString.GetMultilingualString("AU.InterchangeNumberDetailsForm|FailedToSave", "Interchange Number failed to save.");

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (DataSource != dataSource && DataSource is InterchangeNumberDetails oldDetails)
			{
				oldDetails.NotificationsChanged -= CheckEnableForOKButton;
			}

			base.SetDataBinding(dataSource, dataMember);

			if (dataSource is InterchangeNumberDetails details)
			{
				details.NotificationsChanged -= CheckEnableForOKButton;
				details.NotificationsChanged += CheckEnableForOKButton;
			}
		}

		void CheckEnableForOKButton(object sender, NotificationsChangedEventArgs e)
		{
			ButtonOK.Enabled = !InterchangeNumberDetails.HasErrors();
		}

		void ButtonOK_Click(object sender, EventArgs e)
		{
			if (InterchangeNumberDetails.SaveNewInterchangeNumber())
			{
				Globals.Message.ShowInformation(SavedSuccessfully, MessageCaption);
				Close();
			}
			else
			{
				Globals.Message.ShowError(FailedToSave, MessageCaption);
			}
		}

		void ButtonIncrement_Click(object sender, EventArgs e)
		{
			InterchangeNumberDetails.SetNewInterchangeNumberAutomatically();
		}

		void ButtonCancel_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
