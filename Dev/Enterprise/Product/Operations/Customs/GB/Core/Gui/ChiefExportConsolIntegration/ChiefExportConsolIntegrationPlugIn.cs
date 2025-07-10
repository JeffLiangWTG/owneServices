using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Licensing;

namespace Enterprise.Customs.GB.GUI.ChiefExportConsolIntegration
{
	public class ChiefExportConsolIntegrationPlugIn : CustomsManifestPlugIn
	{
		public ChiefExportConsolIntegrationPlugIn(ForwardingConsol hostEntity)
			: base(hostEntity)
		{
			UnHookShipmentCountChangedHandler();
			HookShipmentCountChangedHandler();
		}

		protected override MenuItem GetNewTopLevelMenu()
		{
			return new ExportConsolIntegrationMenu(ChiefExportConsolIntegrationWrapper);
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return ChiefExportConsolIntegrationWrapper;
		}

		public override bool CanDelete
		{
			get { return true; }
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.ExportBroker; }
		}

		protected override Control GetNewUserControl()
		{
			return new ChiefExportConsolIntegrationUserControl(ChiefExportConsolIntegrationWrapper);
		}

		public override string Name
		{
			get { return "ChiefExportConsolIntegration"; }
		}

		internal CustomsExportConsolIntegrationWrapper ChiefExportConsolIntegrationWrapper
		{
			get
			{
				if (chiefExportConsolIntegrationWrapper == null && Consol != null)
				{
					chiefExportConsolIntegrationWrapper = new CustomsExportConsolIntegrationWrapper(Consol, new Customs.GUI.SendsMessagesToCustomsGUI());
				}
				return chiefExportConsolIntegrationWrapper;
			}
		}

		protected override void ChangeTheVisibilityCore()
		{
			this.Enabled = ChiefExportConsolIntegrationWrapper?.IsChiefCcsukEnabled ?? false;
			(this.TopLevelMenu as ChiefExportConsolIntegrationMenu)?.RefreshMenu();
		}

		ForwardingConsol Consol
		{
			get { return (ForwardingConsol)ManifestProvider; }
		}

		void HookShipmentCountChangedHandler()
		{
			if (Consol != null && Consol.Shipments != null)
			{
				Consol.Shipments.CountChanged += Shipments_CountChanged;
			}
		}

		void UnHookShipmentCountChangedHandler()
		{
			if (Consol != null && Consol.Shipments != null)
			{
				Consol.Shipments.CountChanged -= Shipments_CountChanged;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnHookShipmentCountChangedHandler();
			}

			base.Dispose(disposing);
		}

		void Shipments_CountChanged(object sender, CollectionCountChangedEventArgs collectionCountChangedEventArgs)
		{
			if (sender is BusinessObjectCollection collection
				&& !collection.IsRefreshingByDataRefreshBus // Cos otherwise if you have both the consol form and the shipment form open and you detach the shipment from the consol using the consol form, you get this hit 
				)
			{
				ChiefExportConsolIntegrationWrapper.HandleShipmentAddedToOrRemovedFromConsol(collectionCountChangedEventArgs);
			}
		}

		CustomsExportConsolIntegrationWrapper chiefExportConsolIntegrationWrapper;
	}
}
