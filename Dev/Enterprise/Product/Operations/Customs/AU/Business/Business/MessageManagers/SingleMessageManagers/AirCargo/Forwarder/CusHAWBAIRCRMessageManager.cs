using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusHAWBAIRCRMessageManager : CusHAWBBaseAIRCRManager
	{
		public CusHAWBAIRCRMessageManager(CusHAWB hAWB, ZString ownerCompanyABN)
			: base(hAWB)
		{
			OwnerCompanyABN = ownerCompanyABN;
		}

		public CusHAWBAIRCRMessageManager(CusHAWB hAWB, bool shouldDelaySending = false)
			: this(hAWB, ZString.Empty)
		{
			this.shouldDelaySending = shouldDelaySending;
		}

		public ZString OwnerCompanyABN { get; }
		readonly bool shouldDelaySending;

		internal override CMRAmendmentGenerator GetAmendmentManager(BusinessObject bizo) => new CusHAWBAIRCRAmendmentGenerator(bizo as CusHAWB);

		internal override CMRMessageBuilder[] GetBuilder(BusinessObject bizo) => new CMRMessageBuilder[] { new AIRCRMessageBuilder(bizo as CusHAWB, shouldDelaySending) };

		public override string MessageFriendlyName => "Air Cargo Report for HAWB: " + ((CusHAWB)BusinessObject).CS_HAWB;

		protected override ForwardingShipmentProcessTask OverdueCargoReportExceptionCore
		{
			get
			{
				ForwardingShipmentProcessTask result = null;
				ForwardingShipment cachedShipment = ((CusHAWB)BusinessObject).Shipment;
				if (cachedShipment != null)
				{
					result = cachedShipment.OverdueCargoReportException;
				}
				return result;
			}
		}
	}
}
