using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	sealed partial class AdditionalWagonNumbersForm : ZChildForm
	{
		public AdditionalWagonNumbersForm(IDepartureTransportMeansProvider transportMeans)
			: base(transportMeans)
		{
			additionalWagons = transportMeans.AdditionalWagons;
			oldAdditionalWagons = new ReadOnlyCollection<(ZString, ZString)>(transportMeans.AdditionalWagons
				.Select(x => (x.WagonNumber, x.WagonNationality))
				.ToArray());
			InitializeComponent();
		}
		readonly IBusinessObjectCollection<IAdditionalWagonProvider> additionalWagons;
		readonly ReadOnlyCollection<(ZString, ZString)> oldAdditionalWagons;

		protected override void OnClosed(EventArgs e)
		{
			if (additionalWagons.HasChanges && DialogResult != System.Windows.Forms.DialogResult.OK)
			{
				additionalWagons.RemoveAndDeleteAll();
				foreach (var (wagonNumber, wagonNationality) in oldAdditionalWagons)
				{
					var item = additionalWagons.AddNew();
					item.WagonNumber = wagonNumber;
					item.WagonNationality = wagonNationality;
				}
			}
		}

		void OnOKButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();
			if (additionalWagons is INotificationProvider provider && provider.HasNotifications(CargoWise.EntityFramework.NotificationType.Error))
			{
				Globals.Message.ShowError(Res.GetString("AEAB8CC0-CC3A-49A9-92D1-4E1B0A811D30", "The form has errors. Please fix them before continuing."));
				DialogResult = System.Windows.Forms.DialogResult.None;
			}
			else
			{
				Close();
			}
		}

		void OnCloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
