using System;
using Enterprise.Client.UPE.Business.DataImport;
using Enterprise.Client.UPE.GUI.DataImport;
using Enterprise.Customs.SG.Access.GUI;

namespace Enterprise.Client.UPE.Module
{
	public class UPESGAccessModule : ManifestModule
	{
		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();
			AddImportDataMenuItem("Level 1", HandleLevel1FileImport);
		}

		void HandleLevel1FileImport(object sender, EventArgs e)
		{
			using (var form = new Level1DataImportForm(new Level1DataImport(Factory, isImportToManifest: true)))
			{
				form.ShowDialog(EmbeddedControl);
			}
		}
	}
}
