using System;
using System.Collections.ObjectModel;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	sealed partial class AdditionalWagonNumbersForm : ZChildForm
	{
		public AdditionalWagonNumbersForm(InlandTransportCollection inlandTransports)
			: base(inlandTransports)
		{
			this.inlandTransports = inlandTransports;
			oldDataAndCodeList = inlandTransports.DataAndCodeList;
			InitializeComponent();
		}
		readonly InlandTransportCollection inlandTransports;
		readonly ReadOnlyCollection<(ZString, ZString)> oldDataAndCodeList;

		protected override void OnClosed(EventArgs e)
		{
			if (inlandTransports.HasChanges && DialogResult != System.Windows.Forms.DialogResult.OK)
			{
				inlandTransports.DataAndCodeList = oldDataAndCodeList;
			}
			base.OnClosed(e);
		}

		void OnOKButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();
			if (inlandTransports is INotificationProvider provider && provider.HasNotifications(NotificationType.Error))
			{
				Globals.Message.ShowError(Res.GetString("0B7BF690-82C9-4DF2-9600-030FAE27E3D5", "The form has errors. Please fix them before continuing."));
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
