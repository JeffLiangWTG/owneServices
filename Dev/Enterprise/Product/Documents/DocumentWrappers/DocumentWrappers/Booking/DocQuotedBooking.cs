using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.QuotedBookings.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocQuotedBooking : DocForwardingShipment
	{
		#region Constructors

		protected DocQuotedBooking(QuotedBooking quotedBooking, BusinessObjectFactory factoryToWrap)
			: base(quotedBooking.Booking, factoryToWrap)
		{
			this.quotedBooking = quotedBooking;
		}

		public static DocQuotedBooking New(QuotedBooking quotedBooking, BusinessObjectFactory factoryToWrap)
		{
			DocQuotedBooking result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(quotedBooking, factoryToWrap);
			}
			else if (quotedBooking != null)
			{
				result = new DocQuotedBooking(quotedBooking, factoryToWrap);
			}

			return result;
		}

		protected new delegate DocQuotedBooking NewDelegate(QuotedBooking quotedBooking, BusinessObjectFactory factoryToWrap);
		protected new static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#endregion

		#region  overrides

		public override ZString BookingETA
		{
			get { return (Sailing != null) ? FreightHelperClass.FormatETAETDDate(TransportModeIsSea, Sailing.ETA) : ZString.Empty; }
		}

		public override ZString DischargeETAString
		{
			get { return Sailing == null ? base.DischargeETAString : BookingETA; }
		}

		public override ZString LoadingETDString
		{
			get { return Sailing == null ? base.LoadingETDString : BookingETD; }
		}

		public override ZString BookingETD
		{
			get { return (Sailing != null) ? FreightHelperClass.FormatETAETDDate(TransportModeIsSea, Sailing.ETD) : ZString.Empty; }
		}

		public override DocSailing Sailing
		{
			get { return QuotedBooking.ScheduleChooser != null && QuotedBooking.ScheduleChooser.Sailing != null ? DocSailing.New(QuotedBooking.ScheduleChooser.Sailing, Factory) : null; }
		}

		#endregion

		#region Properties

		public override ZString Weight
		{
			get { return FormatNumber(QuotedBooking.Weight); }
		}

		public ZString VoyageNo
		{
			get { return Sailing != null ? Sailing.VoyageFlight : ZString.Empty; }
		}

		public ZString VesselName
		{
			get { return QuotedBooking.Booking.JS_Calc_CurrentVessel; }
		}

		public ZString ContainerCommodities
		{
			get
			{
				var allContainerCommodities = QuotedBooking.QuotedBookingContainers
					.Cast<CommonContainer>()
					.Where(c => c.JC_RH_NKContainerCommodityCode != "")
					.Select(c => c.JC_RH_NKContainerCommodityCode);

				var result = new ZStringBuilder(allContainerCommodities.Distinct());

				return result.ToStringWithDelimiterBetweenAppends(", ");
			}
		}

		#endregion

		#region Containers

		protected override IDocContainerCollection NewContainersCollection()
		{
			IDocContainerCollection result = new IDocContainerCollection(QuotedBooking.Factory);

			foreach (CommonContainer container in QuotedBooking.QuotedBookingContainers)
			{
				result.Add(DocContainer.New(container, QuotedBooking.Booking, Factory));
			}

			return result;
		}

		#endregion

		#region Implemenation

		QuotedBooking QuotedBooking
		{
			get { return quotedBooking; }
		}
		readonly QuotedBooking quotedBooking;

		#endregion
	}
}
