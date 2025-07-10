using CargoWise.Types;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRDepotShipmentOutturnSynchroniser : CMRDepotOutturnSynchroniser
	{
		public CMRDepotShipmentOutturnSynchroniser(CusOutturn destination, CFSShipment source)
			: base(destination, source)
		{
			this.Shipment = source;
		}

		public readonly CFSShipment Shipment;

		protected override void HookSynchronisers()
		{
			if (!Outturn.C5_ReceiptOnlyIndicator)
			{
				foreach (CFSPackLine packLine in Shipment.OuterPackLines)
				{
					if (Shipment.ArrivalConsol != null)
					{
						CFSContainer arrivalContainer = (CFSContainer)packLine.GetContainer(Shipment.ArrivalConsol);
						if (arrivalContainer != null)
						{
							if (arrivalContainer.JC_ContainerNum == Outturn.C5_ContainerNumber)
							{
								var packLineOutturnSynchroniser = new Customs.Business.FieldSynchroniser(Outturn.C5_PackagesOutturnedInfo, packLine.JL_OutturnInfo);
								packLineOutturnSynchroniser.Format += PackLineOutturnSynchroniser_Format;
								Synchronisers.Add(packLineOutturnSynchroniser);
							}
						}
					}
				}
			}
			else
			//			else if (Shipment.JS_PackingMode == Enterprise.Core.Constants.ContainerModes.Bulk 
			//				|| Shipment.JS_PackingMode == Enterprise.Core.Constants.ContainerModes.BreakBulk
			//				|| Shipment.JS_PackingMode == Enterprise.Core.Constants.ContainerModes.Liquid)
			{
				Synchronisers.Add(new Customs.Business.FieldSynchroniser(Outturn.C5_PackagesOutturnedInfo, Shipment.JS_OuterPacksInfo));
			}
		}

		void PackLineOutturnSynchroniser_Format(object sender, Customs.Business.FieldSynchroniser.ConvertEventArgs e)
		{
			ZInt totalPackageCountForContainer = 0;
			foreach (CFSPackLine packLine in Shipment.OuterPackLines)
			{
				if (Shipment.ArrivalConsol != null)
				{
					CFSContainer arrivalContainer = (CFSContainer)packLine.GetContainer(Shipment.ArrivalConsol);
					if (arrivalContainer.JC_ContainerNum == Outturn.C5_ContainerNumber)
					{
						totalPackageCountForContainer += packLine.JL_Outturn;
					}
				}
			}
			e.Value = totalPackageCountForContainer;
		}
	}
}
