using System.Globalization;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Customs.GB.GUI.Ccsuk;
using Enterprise.Customs.GUI;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.GB.GUI
{
	public class GbBrokeragePlugIn : EU.GUI.BrokeragePlugIn
	{
		public GbBrokeragePlugIn(ForwardingShipment shipment)
			: base(shipment)
		{
			UnHookConsolCountChangedHandler();
			HookConsolCountChangedHandler();
		}

		protected override BaseCustomsBrokerageUserControl CreateBrokerageUserControl()
		{
			return new CustomsBrokerageUserControl();
		}

		MenuItem topLevelMenu;
		protected override MenuItem GetNewTopLevelMenuCore()
		{
			if (topLevelMenu == null)
			{
				topLevelMenu = new EDIMenu();
			}
			return topLevelMenu;
		}

		void Consols_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			var collection = sender as ForwardingConsolManyToManyCollection;
			if (collection != null
				&& !collection.IsRefreshingByDataRefreshBus                 // Cos otherwise if you have both the consol form and the shipment form open and you detach the shipment from the consol using the consol form, you get this hit 
				&& !collection.IsNonCommittedCollectionElement(e.BizObject) // Cos there's a "feature" in Core whereby CountChanged events are fired for uncommitted rows upon removing them (open a consol-less shipment and press its packing tab, this method is struct)
				)
			{
				var shipment = collection.ParentShipment;
				var consol = e.BizObject as ForwardingConsol;
				if (consol != null)
				{
					var chiefExportConsolIntegrationWrapper = new CustomsExportConsolIntegrationWrapper(consol, new SendsMessagesToCustomsGUI());
					chiefExportConsolIntegrationWrapper.HandleConsolAddedToOrRemovedFromShipment(e, shipment);
				}
			}
		}

		void HookConsolCountChangedHandler()
		{
			if (Shipment != null && Shipment.Consols != null)
			{
				Shipment.Consols.CountChanged += new CollectionCountChangedEventHandler(Consols_CountChanged);
			}
		}

		void UnHookConsolCountChangedHandler()
		{
			if (Shipment != null && Shipment.Consols != null)
			{
				Shipment.Consols.CountChanged -= new CollectionCountChangedEventHandler(Consols_CountChanged);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnHookConsolCountChangedHandler();
			}

			base.Dispose(disposing);
		}

		protected override string GetQueryText(string[] messages, Customs.Business.CreateDeclarationHelper.QuestionType type)
		{
			var queryText = base.GetQueryText(messages, type);
			if (type != Customs.Business.CreateDeclarationHelper.QuestionType.ImportDeclarationQuery)
			{
				var ccsukWarningPrefix = CcsukNotLinkedWarningPrefix;
				return ccsukWarningPrefix.IsEmpty ? queryText : string.Format(CultureInfo.CurrentCulture, "{0}\r\n{1}", ccsukWarningPrefix, queryText);
			}
			return queryText;
		}

		ZString CcsukNotLinkedWarningPrefix
		{
			get
			{
				var result = ZString.Empty;
				using (var ccsukShipmentPlugin = new CcsukShipmentMultiHawbPlugin(Shipment))
				{
					if (ccsukShipmentPlugin.IsValidShipmentForCcsuk)
					{
						foreach (CusHAWB hawb in ccsukShipmentPlugin.HawbPluginHelper.Hawbs)
						{
							if (hawb.CS_JS != Shipment.PK)  // will happen if there's a NK link but no FK link, which means that the CCSUK tab has not yet been first selected.
							{
								result = "A CCS-UK HAWB was found but there is not an internal link to it. You should first press the CCS-UK tab to ensure the link exists.";
								break;
							}
						}
					}
				}
				return result;
			}
		}
	}
}
