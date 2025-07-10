using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	sealed partial class AdditionalTransportBorderForm : ZChildForm
	{
		public AdditionalTransportBorderForm(IAdditionalTransportMeansProvider additionalTransportMeansProvider)
			: base(additionalTransportMeansProvider.AdditionalTransportAtBorderList)
		{
			this.additionalTransportMeansProvider = additionalTransportMeansProvider;
			additionalTransportAtBorderList = additionalTransportMeansProvider.AdditionalTransportAtBorderList;

			oldAdditionalTransportAtBorderList = new ReadOnlyCollection<(ZString, ZString, ZString, ZString, ZString)>(additionalTransportAtBorderList
				.Cast<DepartureCusTransportMeans>().OrderBy(x => x.TPM_SequenceNumber)
				.Select(x => (x.TPM_CustomsOffice, x.TPM_IdentificationNumber, x.TPM_ReferenceNumber, x.TPM_RN_NKTransportNationality, x.TPM_TypeOfIdentification))
				.ToArray());

			InitializeComponent();
		}
		readonly IAdditionalTransportMeansProvider additionalTransportMeansProvider;
		readonly IDepartureCusTransportMeansCollection<DepartureCusTransportMeans> additionalTransportAtBorderList;
		readonly ReadOnlyCollection<(ZString, ZString, ZString, ZString, ZString)> oldAdditionalTransportAtBorderList;

		protected override void OnClosed(EventArgs e)
		{
			if (additionalTransportAtBorderList.HasChanges && DialogResult != System.Windows.Forms.DialogResult.OK)
			{
				additionalTransportAtBorderList.RemoveAndDeleteAll();
				foreach (var (customsOffice, identificationNumber, referenceNumber, transportNationality, typeOfIdentification) in oldAdditionalTransportAtBorderList)
				{
					var item = additionalTransportAtBorderList.AddNew();
					item.TPM_CustomsOffice = customsOffice;
					item.TPM_IdentificationNumber = identificationNumber;
					item.TPM_ReferenceNumber = referenceNumber;
					item.TPM_RN_NKTransportNationality = transportNationality;
					item.TPM_TypeOfIdentification = typeOfIdentification;
				}
			}
			base.OnClosed(e);
		}

		void OnOKButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();
			if (additionalTransportAtBorderList is INotificationProvider provider && provider.HasNotifications(NotificationType.Error))
			{
				Globals.Message.ShowError(Res.GetString("AEAB8CC0-CC3A-49A9-92D1-4E1B0A811D30", "The form has errors. Please fix them before continuing."));
				DialogResult = System.Windows.Forms.DialogResult.None;
			}
			else
			{
				additionalTransportMeansProvider.ValidateAdditionalTransportAtBorderListCount();
				Close();
			}
		}

		void OnCloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
