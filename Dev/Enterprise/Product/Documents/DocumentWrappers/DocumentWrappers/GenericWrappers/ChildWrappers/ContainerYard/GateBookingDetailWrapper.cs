using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.ContainerYard.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class GateBookingDetailWrapper : GenericWrapper
	{
		public GateBookingDetailWrapper(GateBookingDetail gateBookingDetailBO, BusinessObjectFactory factory)
			: base(gateBookingDetailBO, factory)
		{
			this.gateBookingDetailBO = gateBookingDetailBO ?? factory.GetNull<GateBookingDetail>();
		}
		readonly GateBookingDetail gateBookingDetailBO;

		public OrganisationWrapper BookingParty
		{
			get { return bookingParty ?? (bookingParty = new OrganisationWrapper(OrganisationUsageType.BookingParty, gateBookingDetailBO.BookingPartyAddress, ContactType.All, Factory)); }
		}
		OrganisationWrapper bookingParty;

		public GateYardUnitWrapper YardUnit
		{
			get { return yardUnit ?? (yardUnit = new GateYardUnitWrapper(gateBookingDetailBO.YardUnit, Factory)); }
		}
		GateYardUnitWrapper yardUnit;

		public CommodityWrapper CommodityCode
		{
			get { return commodityCode ?? (commodityCode = new CommodityWrapper(gateBookingDetailBO.CommodityCode, Factory)); }
		}
		CommodityWrapper commodityCode;

		public ZBool IsDangerousGoods => gateBookingDetailBO.GTD_IsDangerousGoods;

		public ZString BookingReference => gateBookingDetailBO.GTD_BookingReference;

		public ZBool IsContainer => gateBookingDetailBO.GTD_IsContainer;
	}
}
