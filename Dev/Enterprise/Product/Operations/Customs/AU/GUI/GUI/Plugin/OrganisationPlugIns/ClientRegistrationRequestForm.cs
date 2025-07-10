using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.AU.GUI
{
	public partial class ClientRegistrationRequestForm : ZChildForm
	{
		public ClientRegistrationRequestForm(OrgHeaderWrapper organisation)
			: base(organisation)
		{
			this.organisation = organisation;
			HookMessageModeChangeEvent();
			ManageIndividualDataControlsVisibility();
			ManagePostalAddressControlsVisibility();

			MissingResourceStringChecker.ExcludeFromTest(contactPhGroupBox);
			MissingResourceStringChecker.ExcludeFromTest(ahTextBox);
			MissingResourceStringChecker.ExcludeFromTest(faxTextBox);
			MissingResourceStringChecker.ExcludeFromTest(phTextBox);
			MissingResourceStringChecker.ExcludeFromTest(detailsGroupBox);
		}

		readonly OrgHeaderWrapper organisation;

		public override string FormCaption
		{
			get { return "Customs Client Registration Request"; }
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		void HookMessageModeChangeEvent()
		{
			organisation.CLREGInfoProvider.ZA_IsIndivInfo.ValueChanged -= ZA_IsIndividualInfo_ValueChanged;
			organisation.CLREGInfoProvider.ZA_IsIndivInfo.ValueChanged += ZA_IsIndividualInfo_ValueChanged;

			organisation.CLREGInfoProvider.ZA_ABNInfo.ValueChanged -= ZA_IsIndividualInfo_ValueChanged;
			organisation.CLREGInfoProvider.ZA_ABNInfo.ValueChanged += ZA_IsIndividualInfo_ValueChanged;

			organisation.CLREGInfoProvider.ZA_ABNIndInfo.ValueChanged -= ZA_IsIndividualInfo_ValueChanged;
			organisation.CLREGInfoProvider.ZA_ABNIndInfo.ValueChanged += ZA_IsIndividualInfo_ValueChanged;

			organisation.CLREGInfoProvider.ZA_CACInfo.ValueChanged -= ZA_IsIndividualInfo_ValueChanged;
			organisation.CLREGInfoProvider.ZA_CACInfo.ValueChanged += ZA_IsIndividualInfo_ValueChanged;
		}

		void ZA_IsIndividualInfo_ValueChanged(object sender, EventArgs e)
		{
			ManageIndividualDataControlsVisibility();
			ManagePostalAddressControlsVisibility();
		}

		void ManageIndividualDataControlsVisibility()
		{
			var dataProvider = organisation.CLREGInfoProvider;
			TitleTextBox.Visible = dataProvider.ZA_IsIndiv;
			FirstNameTextBox.Visible = dataProvider.ZA_IsIndiv;
			SecondNameTextBox.Visible = dataProvider.ZA_IsIndiv;
			FamilyNameTextBox.Visible = dataProvider.ZA_IsIndiv;
			suffixTextBox.Visible = dataProvider.ZA_IsIndiv;
			orgNameTextBox.Visible = !dataProvider.ZA_IsIndiv;
			TravelDocsGroupBox.Visible = dataProvider.ZA_IsIndiv;

			var isIndividualModeForControlsVisibility = dataProvider.ZA_IsIndiv || dataProvider.IsABNIndividual;
			DOBDateEdit.Visible = isIndividualModeForControlsVisibility;
			GenderDropEdit.Visible = isIndividualModeForControlsVisibility;
		}

		void ManagePostalAddressControlsVisibility()
		{
			pATabPage.TabVisible = !organisation.CLREGInfoProvider.ZA_ABN.IsEmpty && !organisation.CLREGInfoProvider.ZA_CAC.IsEmpty;
		}

		internal void OKBoundButton_Click(object sender, EventArgs e)
		{
			var dataProvider = organisation.CLREGInfoProvider;
			dataProvider.RunPreSaveValidation();

			if (organisation.HasCCIDWithSameAddress)
			{
				Globals.Message.Show(CCIDExists, "Cannot send message", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			IsOKToSendMessage = true;
			if (dataProvider.HasMessageErrors || dataProvider.HasErrors)
			{
				var collector = new ZNotificationCollector(dataProvider, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetErrors();
				if (collector.HasErrors())
				{
					Globals.Message.Show("There are errors.\r\n\r\n" + collector.ToUniqueMessageListString() + "\r\nPlease fix errors first", "Errors", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				collector = new ZNotificationCollector(dataProvider, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors();

				if (GetConfirmationWithThisWarning(collector.ToUniqueMessageListString()) == DialogResult.No)
				{
					IsOKToSendMessage = false;
				}
			}

			if (IsOKToSendMessage)
			{
				Close();
			}
		}
		public bool IsOKToSendMessage;
		internal const string CCIDExists = "Customs Client ID with same address already exists for this organization. You cannot send Client Registration Request message.";

		DialogResult GetConfirmationWithThisWarning(string warningMessage)
		{
			return Globals.Message.Show("There are message errors.\r\n\r\n" + warningMessage + "\r\nAre you sure you wish to continue?", "Message Errors", MessageBoxButtons.YesNo, DialogResult.No);
		}

		internal void CancelBoundButton_Click(object sender, EventArgs e)
		{
			IsOKToSendMessage = false;
			if (BusinessEntity.HasChanges)
			{
				BusinessEntity.Factory.Save();
			}
			Close();
		}
	}
}
