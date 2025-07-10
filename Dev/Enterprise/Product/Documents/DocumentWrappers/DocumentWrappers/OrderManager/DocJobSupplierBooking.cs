using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers
{
	public class DocJobSupplierBooking : DocumentWrapper
	{
		protected DocJobSupplierBooking(JobSupplierBooking supplierBooking, BusinessObjectFactory factoryToWrap)
			: base(supplierBooking, factoryToWrap)
		{
		}

		public static DocJobSupplierBooking New(BusinessObjectFactory factory, ZGuid pK)
		{
			return New(factory.Load<JobSupplierBooking>(pK), factory);
		}

		public static DocJobSupplierBooking New(JobSupplierBooking supplierBooking, BusinessObjectFactory factoryToWrap)
		{
			if (supplierBooking == null)
			{
				return null;
			}
			else
			{
				return new DocJobSupplierBooking(supplierBooking, factoryToWrap);
			}
		}

		#region Overrides

		public override string ToString()
		{
			return ZString.Empty;
		}

		#endregion

		protected JobSupplierBooking Booking
		{
			get { return (JobSupplierBooking)WrappedObject; }
		}

		public ZString BookingId => Booking.JSB_BookingId;

		public ZString Status => Booking.JSB_Status;

		public ZString LoadMode => Booking.JSB_LoadMode;

		public ZString TransportMode => Booking.JSB_TransportMode;

		public ZString IncoTerm => Booking.JSB_IncoTerm;

		public ZString LoadPort => Booking.JSB_RL_NKLoadPort;

		public ZString DischargePort => Booking.JSB_RL_NKDischargePort;

		public DocOrganisation BookingParty => bookingParty ?? (bookingParty = DocOrganisation.New(Booking.BookingParty, Factory));
		DocOrganisation bookingParty;

		public ZDate BookedOnDate => Booking.JSB_BookedOnDate;

		public ZDate CargoAvailableDate => Booking.JSB_CargoAvailableDate;

		public ZString ContainerMode => Booking.JSB_ContainerMode;

		public ZString GoodsDescription => Booking.JSB_GoodsDescription;

		public ZString MarksAndNumbers => Booking.JSB_MarksAndNumbers;

		public ZString DetailedGoodsDescription
		{
			get
			{
				if (!detailedGoodsDescription.HasValue)
				{
					detailedGoodsDescription = Booking.Notes.FindByDescription((NoResString)"Detailed Goods Description").FirstOrDefault()?.ST_NoteText ?? ZString.Empty;
				}

				return detailedGoodsDescription.Value;
			}
		}
		ZString? detailedGoodsDescription;

		public DocDocAddress CFSAddress => cfsAddress ?? (cfsAddress = DocDocAddress.New(Booking.CFSAddress, Factory));
		DocDocAddress cfsAddress;

		public ZString Destination => Booking.JSB_RL_NKDestination;

		public ZString Origin => Booking.JSB_RL_NKOrigin;

		public DocDocAddress SupplierAddress => supplierAddress ?? (supplierAddress = DocDocAddress.New(Booking.SupplierAddress, Factory));
		DocDocAddress supplierAddress;

		public DocDocAddress ControllingCustomerAddress => controllingCustomerAddress ?? (controllingCustomerAddress = DocDocAddress.New(Booking.ControllingCustomerAddress, Factory));
		DocDocAddress controllingCustomerAddress;

		public DocDocAddress ConsigneeDocumentaryAddress => consigneeDocumentaryAddress ?? (consigneeDocumentaryAddress = DocDocAddress.New(Booking.ConsigneeDocumentaryAddress, Factory));
		DocDocAddress consigneeDocumentaryAddress;

		public DocDocAddress NotifyPartyDocumentaryAddress => notifyPartyDocumentaryAddress ?? (notifyPartyDocumentaryAddress = DocDocAddress.New(Booking.DocAddresses.FindByDocAddressType(MasterFiles.Integration.DocAddressType.NotifyParty), Factory));
		DocDocAddress notifyPartyDocumentaryAddress;

		public DocDocAddress NotifyParty2DocumentaryAddress => notifyParty2DocumentaryAddress ?? (notifyParty2DocumentaryAddress = DocDocAddress.New(Booking.DocAddresses.FindByDocAddressType(MasterFiles.Integration.DocAddressType.NotifyParty2), Factory));
		DocDocAddress notifyParty2DocumentaryAddress;

		public DocDocAddress NotifyParty3DocumentaryAddress => notifyParty3DocumentaryAddress ?? (notifyParty3DocumentaryAddress = DocDocAddress.New(Booking.DocAddresses.FindByDocAddressType(MasterFiles.Integration.DocAddressType.NotifyParty3), Factory));
		DocDocAddress notifyParty3DocumentaryAddress;

		public DocJobSupplierBookingLineCollection BookingLines
		{
			get
			{
				if (bookingLines == null)
				{
					bookingLines = new DocJobSupplierBookingLineCollection(Factory);
					Booking.SupplierBookingLines.ForEach(bookingLine => bookingLines.Add(DocJobSupplierBookingLine.New(bookingLine, Factory)));
				}

				return bookingLines;
			}
		}
		DocJobSupplierBookingLineCollection bookingLines;

		public DocContainerCollection JobContainers
		{
			get
			{
				if (jobContainers == null)
				{
					jobContainers = new DocContainerCollection(Factory);
					Booking?.Containers?.ForEach(container => jobContainers.Add(DocContainer.New((ForwardingContainer)container, Factory)));
				}

				return jobContainers;
			}
		}
		DocContainerCollection jobContainers;

		public IDocContainerCollection PlannedContainers
		{
			get
			{
				if (plannedContainers == null)
				{
					plannedContainers = new IDocContainerCollection(Factory);
					Booking?.PlannedContainers?.ForEach(container => plannedContainers.Add(DocOrderContainer.New((JobSupplierBookingPlannedContainer)container, Factory)));
				}

				return plannedContainers;
			}
		}
		IDocContainerCollection plannedContainers;
	}
}
