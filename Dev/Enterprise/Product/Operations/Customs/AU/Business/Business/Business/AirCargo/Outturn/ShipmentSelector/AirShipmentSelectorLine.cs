using System;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AirShipmentSelectorLine : ShipmentSelectorLine
	{
		public void CreateStandardLine()
		{
			this.Shipment = Res.GetString("0B762C7D-B365-47C1-AD49-16A29D73D0A8", "All Standards");
			this.Type = Core.Constants.ShipmentTypes.StandardHouse;
			this.Consignee = Res.GetString("0B762C7D-B365-47C1-AD49-16A29D73D0A9", "Various");
		}

		public void CreateStandAloneLine(IScanHouseBillProvider houseBill)
		{
			if (houseBill == null)
			{
				throw new ArgumentNullException(nameof(houseBill), "houseBill cannot be a null reference.");
			}

			if (houseBill.ShipmentType == Core.Constants.ShipmentTypes.StandardHouse)
			{
				throw new InvalidOperationException("Use default constructor instead.");
			}

			this.Shipment = houseBill.HouseBill;
			this.Type = houseBill.ShipmentType;
			this.Consignee = houseBill.ConsigneeName;
			this.Consignor = houseBill.ConsignorName;
			this.GoodsDescription = houseBill.GoodsDescription;
			houseBills.Add(houseBill);
		}

		public IEnumerable<CusHAWB> CusHAWBs
		{
			get
			{
				return HouseBills.Cast<CusHAWB>();
			}
		}

		public override void AddStandardHouseBill(IScanHouseBillProvider houseBill)
		{
			if (Type == Core.Constants.ShipmentTypes.StandardHouse && houseBill.ShipmentType != Core.Constants.ShipmentTypes.StandardHouse)
			{
				throw new InvalidOperationException();
			}
			base.AddStandardHouseBill(houseBill);
		}

		public void UpdateStatuses(CusUnderbond selectedUnderbond)
		{
			if (selectedUnderbond == null)
			{
				throw new ArgumentNullException(nameof(selectedUnderbond));
			}

			if (this.Type != Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy)
			{
				if (!CusHAWBs.Any())
				{
					return;
				}

				OutturnStatus = selectedUnderbond.GetOutturnStatus();
				UnderbondStatus = selectedUnderbond.GetUnderbondStatus();
			}
			else
			{
				if (!CusHAWBs.Any())
				{
					return;
				}

				OutturnStatus = OutturnStatus.ReadyForScanning;
				UnderbondStatus = UnderbondStatus.NotSend;
				var cusHAWB = CusHAWBs.ElementAt(0);
				var standAloneAirCargo = cusHAWB.MAWB.FindStandAloneAirCargo(cusHAWB.CS_HAWB);

				if (standAloneAirCargo != null)
				{
					var matchedUnderbond = standAloneAirCargo.GetMatchedToSelectedUnderbond(selectedUnderbond);
					if (matchedUnderbond != null)
					{
						OutturnStatus = matchedUnderbond.GetOutturnStatus();
						UnderbondStatus = matchedUnderbond.GetUnderbondStatus();
					}
				}
			}

			this.ReadOnly = UnderbondStatus != UnderbondStatus.NotSend;
		}

		protected override int Shipment_MaxLength { get { return CusHAWB.Schema.CS_HAWBMaxLength; } }
		protected override int Consignee_MaxLength { get { return CusHAWB.Schema.CS_ConsigneeNameMaxLength; } }
	}
}
