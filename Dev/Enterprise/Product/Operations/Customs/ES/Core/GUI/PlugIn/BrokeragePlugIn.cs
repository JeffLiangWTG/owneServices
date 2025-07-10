using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.ES.GUI
{
	public class BrokeragePlugIn : EU.GUI.BrokeragePlugIn
	{
		public BrokeragePlugIn(ForwardingShipment shipment) : base(shipment)
		{
		}

		protected override BaseCustomsBrokerageUserControl CreateBrokerageUserControl() => new CustomsBrokerageUserControl();

		protected override MenuItem GetNewTopLevelMenuCore() => new EDIMenu();

		protected override IEnumerable<PreSaveDialogStrategy> GetPreSaveDialogStrategies()
		{
			var preSaveStrategies = new List<PreSaveDialogStrategy>(base.GetPreSaveDialogStrategies());

			var declaration = JobDeclaration as JobDeclaration;
			if (declaration != null)
			{
				preSaveStrategies.Add(new AEODocumentPreSaveDialogStrategy(declaration));
				preSaveStrategies.Add(new Document9015PreSaveDialogStrategy(declaration));
			}
			return preSaveStrategies;
		}
	}
}
