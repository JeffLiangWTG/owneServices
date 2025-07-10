using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public class CcsukShipmentMultiHawbPluginForDirectConsols : CcsukShipmentMultiHawbPlugin
	{
		public CcsukShipmentMultiHawbPluginForDirectConsols(CommonShipment shipment)
			: base(shipment)
		{
			unableToCreateMessage = @"No CCS-UK Basic Air Waybill is linked to this direct shipment's consol. 
Neither a link nor a new record can be created from here. 
Instead open the consol and either:
 a) create/link the record there first by entering the consol's CCS-UK tab
 or
 b) change the consol type from DRT to AGT in order to add a HAWB";
		}

		protected override void SetupTopLevelMenu()
		{
			// No menu
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			if (MawbPluginHelper.Mawbs.Count > 0)
			{
				foreach (var m in MawbPluginHelper.Mawbs)
				{
					m.SetReadOnlyIncludingChildren(true);
				}

				MawbPluginHelper.Mawbs.SetReadOnlyIncludingChildren(true); // the collection itself needs to be read-only so users cannot remove rows from the grid
				MawbPluginHelper.SetReadOnlyIncludingChildren(true);
				return MawbPluginHelper;
			}

			return null;
		}

		protected override bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			return GetBusinessEntityForPlugIn() != null;
		}

		protected override Control GetNewUserControl()
		{
			return new CcsukAirConsignmentUserControlMawbMany();
		}

		protected override bool IsValidShipmentForCcsukCore
		{
			get { return CcsukUtilities.IsDirectShipmentValidForCcsuk(Shipment, Consol); }
		}
	}
}
