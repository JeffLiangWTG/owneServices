using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Packing.Business;
using Enterprise.Rating.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class ContainerWrapperCollection : GenericWrapperCollection<ContainerWrapper>
	{
		public ContainerWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ContainerWrapperCollection(ReleaseInstance instance, BusinessObjectFactory factory)
			: base(factory)
		{
			foreach (ReleaseDetail detail in instance.Header.Details)
			{
				if (detail.ReleaseCount == 0)
				{
					continue;
				}

				// if one is null but the other isn't, then move on to the next instance.
				if ((detail.ContainerYard == null) != (instance.ContainerYard == null))
				{
					continue;
				}

				if (detail.ContainerYard == null || detail.ContainerYard.PK == instance.ContainerYard.OA_OH)
				{
					Add(new ContainerWrapperFromAgency(detail.Container, factory));
				}
			}
		}

		public ContainerWrapperCollection(ForwardingShipment shipmentBO, ZString documentDirection, BusinessObjectFactory factory)
			: base(factory)
		{
			if (shipmentBO != null)
			{
				ForwardingConsol consol = GetForwardingConsol(shipmentBO, documentDirection);

				if (consol == null)
				{
					foreach (ForwardingContainer containerBO in shipmentBO.Containers)
					{
						Add(new ContainerWrapperFromFreight(containerBO, shipmentBO, factory));
					}
				}
				else
				{
					foreach (ForwardingContainer containerBO in shipmentBO.ContainersOnConsol(consol))
					{
						Add(new ContainerWrapperFromFreight(containerBO, shipmentBO, factory));
					}
				}
			}
		}

		ForwardingConsol GetForwardingConsol(ForwardingShipment shipment, ZString documentDirection)
		{
			ForwardingConsol consol = null;

			if (shipment != null)
			{
				if (shipment.CurrentConsolForDocuments != null)
				{
					consol = shipment.CurrentConsolForDocuments;
				}
				else if (documentDirection == nameof(DocumentDirection.ARV))
				{
					consol = shipment.ArrivalConsolForDocuments as ForwardingConsol;
				}
				else if (documentDirection == nameof(DocumentDirection.DEP))
				{
					consol = shipment.DepartureConsolForDocuments as ForwardingConsol;
				}
			}

			return consol;
		}

		public ContainerWrapperCollection(CFSShipment shipmentBO, BusinessObjectFactory factory)
			: base(factory)
		{
			if (shipmentBO != null)
			{
				foreach (CFSContainer containerBO in shipmentBO.Containers)
				{
					Add(new ContainerWrapperFromFreight(containerBO, factory));
				}
			}
		}

		public ContainerWrapperCollection(AgencyShipment agencyShipmentBO, BusinessObjectFactory factory)
			: base(factory)
		{
			if (agencyShipmentBO != null && !agencyShipmentBO.IsTopLevelPacksMode)
			{
				AgencyShipmentContainerDependentCollection containers = agencyShipmentBO.IsBillOfLadingStage ? agencyShipmentBO.RealContainers : agencyShipmentBO.BookedContainers;

				foreach (AgencyShipmentContainer containerBO in containers)
				{
					Add(new ContainerWrapperFromAgency(containerBO, factory));
				}
			}
		}

		public ContainerWrapperCollection(AgencyShipmentContainer agencyShipmentContainerBO, BusinessObjectFactory factory)
			: base(factory)
		{
			if (agencyShipmentContainerBO != null)
			{
				Add(new ContainerWrapperFromAgency(agencyShipmentContainerBO, factory));
			}
		}

		public ContainerWrapperCollection(BaseJobDeclaration declarationBO, BusinessObjectFactory factory)
			: base(factory)
		{
			if (declarationBO != null)
			{
				foreach (BaseCusContainer containerBO in declarationBO.CusContainers)
				{
					Add(new ContainerWrapperFromCustoms(containerBO, factory));
				}
			}
		}

		public ContainerWrapperCollection(FreightWrapperFromCartage parentCartageWrapper, BusinessObjectFactory factory)
			: base(factory)
		{
			var cartageBO = parentCartageWrapper.Cartage;
			if (cartageBO != null)
			{
				foreach (CommonContainer containerBO in cartageBO.Containers)
				{
					Add(new ContainerWrapperFromCartage(parentCartageWrapper, containerBO, factory));
				}
			}
		}

		public ContainerWrapperCollection(CommonConsol consolBO, BusinessObjectFactory factory)
			: base(factory)
		{
			if (consolBO != null)
			{
				foreach (CommonContainer containerBO in consolBO.Containers)
				{
					Add(new ContainerWrapperFromFreight(containerBO, factory));
				}
			}
		}

		public ContainerWrapperCollection(CommonConsol consolBO, DeliveryAgentOrgHeader deliveryAgent, BusinessObjectFactory factory)
			: base(factory)
		{
			if (consolBO != null)
			{
				ArrayList containersForDeliveryAgent = new ArrayList();
				foreach (ForwardingShipment shipment in consolBO.Shipments)
				{
					if (shipment.JS_IsForwardRegistered && deliveryAgent != null && shipment.DeliveryAgent != null
						&& shipment.DeliveryAgent.PK.Equals(deliveryAgent.PK)
						&& !shipment.DeliveryAgent.PK.Equals(consolBO.ReceivingForwarderPK))
					{
						foreach (PackLine packLine in shipment.OuterPackLines)
						{
							var container = packLine.GetContainer(consolBO);
							if (container != null && !containersForDeliveryAgent.Contains(container))
							{
								containersForDeliveryAgent.Add(container);
							}
						}
					}
				}
				foreach (CommonContainer container in containersForDeliveryAgent)
				{
					Add(new ContainerWrapperFromFreight(container, factory));
				}
			}
		}

		public ContainerWrapperCollection(Order orderBO, BusinessObjectFactory factory)
			: base(factory)
		{
			if (orderBO != null)
			{
				foreach (OrderContainer containerBO in orderBO.PlannedContainers)
				{
					Add(new ContainerWrapperFromOrder(containerBO, factory));
				}
			}
		}

		public ContainerWrapperCollection(ContainerDetention detention, BusinessObjectFactory factory)
			: base(factory)
		{
			if (detention != null)
			{
				foreach (ContainerMovement movement in detention.Movements)
				{
					Add(new ContainerWrapperFromMovement(movement, factory));
				}
			}
		}

		public ContainerWrapperCollection(DetentionAdviceHeader detention, BusinessObjectFactory factory)
			: base(factory)
		{
			if (detention != null)
			{
				foreach (BillOfLadingContainer container in detention.Containers)
				{
					Add(new ContainerWrapperFromAgency(container, factory));
				}

				foreach (ContainerMovement movement in detention.Movements)
				{
					Add(new ContainerWrapperFromMovement(movement, factory));
				}
			}
		}

		public ContainerWrapperCollection(WhsDocket docket, BusinessObjectFactory factory)
			: base(factory)
		{
			if (docket != null)
			{
				foreach (WhsDocketContainer containerBO in docket.Containers)
				{
					Add(new ContainerWrapperFromWhsDocket(containerBO, factory));
				}
			}
		}

		public ContainerWrapperCollection(RateOneOffShipment oneOffShipment, BusinessObjectFactory factory)
			: base(factory)
		{
			if (oneOffShipment != null)
			{
				foreach (RateOneOffContainers container in oneOffShipment.Containers)
				{
					Add(new ContainerWrapperFromOneOffContainer(container, factory));
				}
			}
		}

		public ContainerWrapperCollection(QuotedBooking quotedBooking, BusinessObjectFactory factory)
			: base(factory)
		{
			if (quotedBooking != null)
			{
				foreach (ForwardingContainer container in quotedBooking.QuotedBookingContainers)
				{
					Add(new ContainerWrapperFromFreight(container, factory));
				}
			}
		}

		public ContainerWrapperCollection(CommonPickupDeliveryConfirm deliveryConfirm, BusinessObjectFactory factory)
			: base(factory)
		{
			List<CommonContainer> myListOfUniqueContainers = new List<CommonContainer>();

			if (deliveryConfirm != null)
			{
				foreach (CommonConfirmDivot divot in deliveryConfirm.Divots)
				{
					if (divot.J8_PackagesDelivered > 0 && divot.PackLine != null)
					{
						foreach (CommonContainer containerBO in divot.PackLine.Containers)
						{
							if (!myListOfUniqueContainers.Contains(containerBO))
							{
								myListOfUniqueContainers.Add(containerBO);
							}
						}
					}
				}

				foreach (var container in myListOfUniqueContainers)
				{
					this.Add(new ContainerWrapperFromFreight(container, factory));
				}
			}
		}

		public ContainerWrapperCollection(DtbTransport transport, BusinessObjectFactory factory)
			: base(factory)
		{
			if (transport != null)
			{
				AddContainersFromTransportBooking(new List<PkgPackage>(), transport);
			}
		}

		public ContainerWrapperCollection(DtbBooking booking, BusinessObjectFactory factory)
			: base(factory)
		{
			if (booking != null)
			{
				AddContainersFromTransportBooking(new List<PkgPackage>(), booking);
			}
		}

		public ContainerWrapperCollection(DtbBookingConsolidation bookingConsolidation, BusinessObjectFactory factory)
			: base(factory)
		{
			if (bookingConsolidation != null)
			{
				var listOfAddedPackages = new List<PkgPackage>();
				foreach (DtbBooking booking in bookingConsolidation.Bookings)
				{
					AddContainersFromTransportBooking(listOfAddedPackages, booking);
				}
			}
		}

		public ContainerWrapperCollection(CommonContainer container, BusinessObjectFactory factory)
			: base(factory)
		{
			if (container != null)
			{
				Add(new ContainerWrapperFromFreight(container, factory));
			}
		}

		public ContainerWrapperCollection(PkgPackage package, BusinessObjectFactory factory)
			: base(factory)
		{
			if (package != null)
			{
				Add(new ContainerWrapperFromPkgPackage(package, factory));
			}
		}

		void AddContainersFromTransportBooking(List<PkgPackage> listOfAddedPackages, DtbTransport booking)
		{
			foreach (DtbTransportInstruction instruction in booking.Instructions)
			{
				foreach (DtbTransportInstructionPkgDivot instructionPkgDivot in instruction.PackageDivots)
				{
					PkgPackage package = instructionPkgDivot.Package;
					if (package?.IsContainer == true && !listOfAddedPackages.Contains(package))
					{
						var containerWrapper = new ContainerWrapperFromPkgPackage(package, Factory);
						Add(containerWrapper);
						listOfAddedPackages.Add(package);
					}
				}
			}
		}

		void AddContainersFromTransportBooking(List<PkgPackage> listOfAddedPackages, DtbBooking booking)
		{
			foreach (DtbBookingInstruction instruction in booking.Instructions)
			{
				foreach (DtbBookingInstructionPkgDivot instructionPkgDivot in instruction.PackageDivots)
				{
					PkgPackage package = instructionPkgDivot.Package;
					if (package?.IsContainer == true && !listOfAddedPackages.Contains(package))
					{
						var containerWrapper = new ContainerWrapperFromPkgPackage(package, Factory);
						Add(containerWrapper);
						listOfAddedPackages.Add(package);
					}
				}
			}
		}

		#region Properties

		internal ZString ContainerSummary
		{
			get
			{
				ZString result = "";
				Dictionary<string, int> containerTypes = new Dictionary<string, int>();
				foreach (ContainerWrapper wrapper in this)
				{
					if (containerTypes.ContainsKey(wrapper.Type.Code))
					{
						containerTypes[wrapper.Type.Code] += wrapper.ContainerCount;
					}
					else
					{
						containerTypes.Add(wrapper.Type.Code, wrapper.ContainerCount);
					}
				}

				foreach (KeyValuePair<string, int> pair in containerTypes)
				{
					result += pair.Key + (NoResString)" x " + pair.Value.ToString() + (NoResString)", ";
				}

				return result.TrimEndIncludingWhiteSpace(',');
			}
		}

		internal ZInt ContainerCount
		{
			get { return this.Cast<ContainerWrapper>().Sum(x => x.ContainerCount); }
		}

		#endregion
	}
}
