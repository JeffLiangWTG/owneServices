using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class TallyCustomsListener : IOutturnLink
	{
		public TallyCustomsListener(TallyOutturn tallyOutturn)
		{
			if (tallyOutturn == null)
			{
				throw new ArgumentNullException(nameof(tallyOutturn));
			}

			this.tallyOutturn = tallyOutturn;
			this.tallyOutturn.IsSynchronisedWithTallyContainer = true;
		}

		public IOutturn GetOutturnFor(PackUnpackShipment shipment)
		{
			CFSShipmentWrapper shipmentWrapper = CFSShipmentWrapper.Load(shipment);
			if (shipmentWrapper.Outturns.Count > 1)
			{
				foreach (DepotCusOutturn outturn in shipmentWrapper.Outturns.OrderBy(x => x.PK))
				{
					if (!outturn.C5_CustomsStatus.IsEmpty)
					{
						return GetTallyCustomsOutturnListener(outturn);
					}
				}
			}
			if (shipmentWrapper.Outturns.Count > 0)
			{
				DepotCusOutturn result = shipmentWrapper.Outturns[0];
				return GetTallyCustomsOutturnListener(result);
			}
			return null;
		}

		TallyCustomsOutturnListener GetTallyCustomsOutturnListener(DepotCusOutturn packUnPackShipmentOutturn)
		{
			var tallyHeader = tallyOutturn.Header;
			if (tallyHeader != null)
			{
				var outturn = tallyHeader.Outturns.Cast<TallyOutturn>().FirstOrDefault(x => x.PK == packUnPackShipmentOutturn.PK);
				if (outturn != null)
				{
					outturn.IsSynchronisedWithPackUnpachShipment = true;
				}
			}
			return new TallyCustomsOutturnListener(packUnPackShipmentOutturn);
		}

		public ZDateTime ReceiptDate
		{
			get
			{
				return tallyOutturn.C5_CargoReceiptDate;
			}
			set
			{
				tallyOutturn.C5_CargoReceiptDate = value;
			}
		}

		public void SetSealNumber(ZString sealNumber)
		{
			tallyOutturn.C5_ContainerSeal = sealNumber;
		}

		public void SetSealIntact(ZBool sealIntact)
		{
			tallyOutturn.C5_SealIntactIndicator = sealIntact;
		}

		readonly TallyOutturn tallyOutturn;
	}
}
