using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SurplusOutturnLine : AutoSurplusOutturnLine
	{
		public SurplusOutturnLine(OutturnLine outturnLine, ShipmentSelectorLineCollection shipmentSelectorLineCollection, BusinessObjectFactory factory)
			: base(factory)
		{
			if (shipmentSelectorLineCollection != null)
			{
				this.selectedShipments = shipmentSelectorLineCollection.GetHLSShipmentIncludeInScan();
			}
			else
			{
				this.selectedShipments = new List<ShipmentSelectorLine>();
			}
			this.OutturnLine = outturnLine;
		}

		[List(nameof(ShipmentList))]
		public override ZString Shipment
		{
			get { return base.Shipment; }
			set
			{
				base.Shipment = value;
				foreach (ShipmentSelectorLine shipment in selectedShipments)
				{
					if (string.Equals(shipment.Shipment, value, System.StringComparison.OrdinalIgnoreCase))
					{
						Description = shipment.Consignor + " - " + shipment.GoodsDescription;
						SelectedShipmentLine = shipment;
						break;
					}
				}
			}
		}
		public readonly OutturnLine OutturnLine;

		readonly IEnumerable<ShipmentSelectorLine> selectedShipments;

		public override void ValidateShipment()
		{
			base.ValidateShipment();
			MandatoryValidation.MessageErrorIfNotEntered(ShipmentInfo);
			ListValidation.MessageErrorIfInvalidCode(ShipmentInfo, ShipmentList);
		}

		public CodeDescriptionPairList ShipmentList
		{
			get
			{
				return Factory.GetCachedValue("SurplusOutturnLine.ShipmentList",
				delegate
				{
					var result = new CodeDescriptionPairList();
					foreach (ShipmentSelectorLine shipment in selectedShipments)
					{
						result.AddPairIfNotExist(shipment.Shipment, shipment.Consignor);
					}
					return result;
				});
			}
		}

		public ShipmentSelectorLine SelectedShipmentLine { get; private set; }
	}
}
