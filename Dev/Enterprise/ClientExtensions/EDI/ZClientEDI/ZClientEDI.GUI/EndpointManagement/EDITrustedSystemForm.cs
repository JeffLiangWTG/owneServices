using System;
using CargoWise.Types;
using Enterprise.Client.EDI.Modules;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.EndpointManagement.GUI
{
	public partial class EdiTrustedSystemForm : ZTemplateForm
	{
		public EdiTrustedSystemForm(EdiTrustedSystem config)
			: base(config)
		{
			if (!DesignModeFinder.IsDesigning)
			{
				DatabasesModuleButtonGrid.ParentTrustedSystem = config;
				DatabasesModuleButtonGrid.ModuleID = ClientModuleRegistration.LicenceDatabase;
			}
		}

		EdiTrustedSystem TrustedSystem => (EdiTrustedSystem)DataSource;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			CertificateControl.SetFileData(TrustedSystem?.CertificateConfig?.ETM_CertificateData ?? ZBlob.Empty);
		}

		void CertificateControl_DataChanged(object sender, EventArgs e)
		{
			if (TrustedSystem != null)
			{
				if (TrustedSystem.CertificateConfig == null)
				{
					TrustedSystem.GetOrCreateCertificateConfig();
				}

				TrustedSystem.CertificateConfig.ETM_CertificateData = CertificateControl.FileDataAsBinary();
				TrustedSystem.HasChanges = true;
			}
		}
	}
}
