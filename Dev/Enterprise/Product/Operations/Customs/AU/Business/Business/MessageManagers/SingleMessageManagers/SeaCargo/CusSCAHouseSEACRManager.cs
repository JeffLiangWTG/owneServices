using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAHouseSEACRManager : CMRMessageManager
	{
		public CusSCAHouseSEACRManager(CusSCAHouse house, ZString ownerCompanyABN)
		{
			this.house = house;
			OwnerCompanyABN = ownerCompanyABN;
		}
		public ZString OwnerCompanyABN { get; }

		public CusSCAHouseSEACRManager(CusSCAHouse house, bool shouldDelaySending = false)
			: this(house, ZString.Empty)
		{
			this.shouldDelaySending = shouldDelaySending;
		}

		readonly bool shouldDelaySending;

		protected override ForwardingShipmentProcessTask OverdueCargoReportExceptionCore
		{
			get
			{
				ForwardingShipmentProcessTask result = null;
				ForwardingShipment cachedShipment = house.Shipment;
				if (cachedShipment != null)
				{
					result = cachedShipment.OverdueCargoReportException;
				}
				return result;
			}
		}

		internal override ICalculatedCusStatusCalculator[] StatusCalculators
		{
			get
			{
				var result = new List<ICalculatedCusStatusCalculator>();
				result.Add(house.MessageStatusCalculator);
				result.AddRange(house.Pivot.Cast<CusSCAPivot>().Select(x => x.StatusCalculator));
				return result.ToArray();
			}
		}

		internal override string GetStatus() => house.CA_MessageStatus;

		public override BusinessObject BusinessObject => house;

		public override string MessageFriendlyName => "Sea Cargo Report for housebill: " + house.CA_HouseBill;

		internal override CMRMessageBuilder[] GetBuilder(BusinessObject bizo) => new CMRMessageBuilder[] { new SEACRMessageBuilder(bizo as CusSCAHouse, OwnerCompanyABN, shouldDelaySending) };

		internal override EDIMessageCollection GetMessages(BusinessObject bizo) => (bizo as CusSCAHouse).Messages;

		internal override CMRAmendmentGenerator GetAmendmentManager(BusinessObject bizo) => new CusSCAHouseSEACRAmendmentGenerator(bizo as CusSCAHouse);

		protected override MessageSendingNotificationCollection GetCommonNotificationsForSending()
		{
			MessageSendingNotificationCollection result = base.GetCommonNotificationsForSending();
			if (house.OceanBill.CB_MultiOBLUnpack)
			{
				result.AddError("You can't send this cargo report because the ocean bill unpack checkbox has been selected");
			}
			return result;
		}

		protected override bool ShouldValidateCurrentCompanyLocalBusNumCharacters => true;

		readonly CusSCAHouse house;
	}
}
