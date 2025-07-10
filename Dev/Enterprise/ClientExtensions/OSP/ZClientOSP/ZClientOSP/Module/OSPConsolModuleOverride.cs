using Enterprise.Billing.Integration;
using Enterprise.Client.OSP.Data_Import;
using Enterprise.DataTransfer.GUI;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.OSP.Module
{
	public class OSPConsolModuleOverride : JobConsolModule
	{
		public OSPConsolModuleOverride()
		{
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();
			AddImportDataMenuItem("From Combiline &File", delegate
			{
				using (DataImporterForm form = DataImporterForm.Create(BillingInterfaceName.ClientSpecifiedImport))
				{
					form.Importer = new OSPCombilineImporter(Factory);
					ZFormModaliser.ShowDialogWithoutDispose(form);
				}
			});
		}
	}
}

#region Implementation
#endregion
