using System;
using Enterprise.Billing.Integration;
using Enterprise.Client.TEL.Import;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.TEL.Modules
{
	internal class TELJobConsolModule : JobConsolModule
	{
		public TELJobConsolModule()
		{
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();
			AddImportDataMenuItem("Manifest <DEBUG ONLY>", new EventHandler(ImportTELManifest));
		}

		void ImportTELManifest(object sender, EventArgs e)
		{
			using (TELTempImportForm form =  new TELTempImportForm(new DataImporterBusinessObject(Factory), null))
			{
				form.Importer = new TELConsolShipXmlDataImporter();
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}

		internal class TELTempImportForm : DataImporterForm
		{
			public TELTempImportForm(DataImporterBusinessObject businessEntity, string formCaption)
				: base(businessEntity, formCaption, BillingInterfaceName.ClientSpecifiedImport)
			{
				InitializeComponent();
				SetDataBinding(businessEntity, "");
			}

			protected override string ImportFileFilter
			{
				get
				{
					return "Xml Files (*.xml)|*.xml";
				}
			}
		}
	}
}
