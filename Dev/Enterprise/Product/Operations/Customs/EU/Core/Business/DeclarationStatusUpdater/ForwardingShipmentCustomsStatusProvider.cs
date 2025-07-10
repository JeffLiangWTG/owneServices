using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business
{
	public class ForwardingShipmentCustomsStatusProvider : Forwarding.Business.ForwardingShipmentCustomsStatusProvider,
															Integration.Customs.EU.IForwardingShipmentCustomsStatusProvider
	{
		public ForwardingShipmentCustomsStatusProvider(ForwardingShipment shipment)
			: base(shipment)
		{ }

		public override ZString CustomsCargoStatus() => ZString.Empty;

		public override ZString CustomsMessageStatus() => Factory.GetValue(ref decCached, GetDec)?.JE_MessageStatus ?? ZString.Empty;
		CachedProperty<JobDeclaration> decCached;

		JobDeclaration GetDec()
		{
			JobDeclaration likelyEuDec = null;
			foreach (object declarationAsObject in Shipment.Declarations)
			{
				// Shipment might have several decs, for different countries
				likelyEuDec = declarationAsObject as JobDeclaration;
				if (likelyEuDec != null)
				{
					if (likelyEuDec.Country.Code == GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
					{
						return likelyEuDec; // this dec is defo good for us, so stop iterating over decs
					}
				}
			}
			return likelyEuDec;
		}
	}
}
