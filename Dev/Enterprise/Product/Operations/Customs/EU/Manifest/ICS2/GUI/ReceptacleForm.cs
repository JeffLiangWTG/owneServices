using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	public partial class ReceptacleForm : ZChildForm
	{
		public ReceptacleForm(AsycudaManifestHeader manifestHeader)
		{
			this.manifestHeader = manifestHeader;
			SetFormDataBinding();
		}
		internal readonly AsycudaManifestHeader manifestHeader;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			fOldItems = manifestHeader.Receptacles.AsString;
		}
		string fOldItems;

		protected override void OnClosing(CancelEventArgs e)
		{
			if (DialogResult == DialogResult.Cancel)
			{
				var collection = manifestHeader.Receptacles;
				using (new DisposableAction(() => collection.SuspendValidation(), () => collection.ResumeValidation()))
				{
					collection.AsString = fOldItems;
				}
			}
			else
			{
				manifestHeader.Receptacles.RunPreSaveValidation();
				if (manifestHeader.Receptacles.Cast<Receptacle>().Any(code => code.NotificationsIncludingChildren.GetErrors().Any()))
				{
					Globals.Message.ShowError(Res.GetString("D39E04EF-44CB-4D48-9D9E-72843387EF89", "The form has errors. Please fix them before continuing."));
					e.Cancel = true;
				}
				manifestHeader.ReceptacleIdInfo.RefreshBinding();
			}

			base.OnClosing(e);
		}

		void SetFormDataBinding()
		{
			SuspendLayout();
			SetDataBinding(manifestHeader, "");
			ResumeLayout();
		}

		void OnOKButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void OnCloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
