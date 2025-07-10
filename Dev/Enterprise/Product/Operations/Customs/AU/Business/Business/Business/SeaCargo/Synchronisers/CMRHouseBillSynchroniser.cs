using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRHouseBillSynchroniser : HouseBillSynchroniser
	{
		public CMRHouseBillSynchroniser(CusSCAHouse destination, CommonShipment source)
			: base(destination, source)
		{
			this.House = destination;
			this.Shipment = source;
		}

		public readonly CusSCAHouse House;
		public readonly CommonShipment Shipment;

		#region Implementation

		protected override PivotSynchroniser GetPivotSynchroniser(CusSCAPivot pivot, PackLine packLine)
			=> PivotSynchroniserFromPivot(pivot)
				?? new CMRPivotSynchroniser(this, pivot, packLine, Source);

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();

			var watchedProperties = new List<ZPropertyInfo>() { Shipment.JS_HouseBillInfo };
			if (Shipment.ArrivalConsol != null)
			{
				watchedProperties.Add(Shipment.JS_PaymentTermInfo);
			}

			var prepaidCollectSynchroniser = new FieldSynchroniser(Destination.CA_PrepaidCollectOtherInfo, GetPrepaidCollectOther, () => watchedProperties, true);
			prepaidCollectSynchroniser.Format += PrepaidCollectSynchroniser_Format;
			Synchronisers.Add(prepaidCollectSynchroniser);
		}

		IZType GetPrepaidCollectOther()
		{
			return !Destination.CA_PrepaidCollectOther.IsEmpty && Source.JS_PaymentTerm.IsEmpty ? Destination.CA_PrepaidCollectOther : Source.JS_PaymentTerm;
		}

		void PrepaidCollectSynchroniser_Format(object sender, FieldSynchroniser.ConvertEventArgs e)
		{
			ZString consolValue = new ZString(e.Value);
			if (!consolValue.IsEmpty)
			{
				switch (consolValue)
				{
					case Enterprise.Core.Constants.PaymentType.Collect:
						e.Value = new ZString(CMRMethodsOfPayment.Codes.Collect);
						break;
					default:
						e.Value = new ZString(CMRMethodsOfPayment.Codes.PrepaidOnly);
						break;
				}
			}
		}

		#endregion

	}
}
