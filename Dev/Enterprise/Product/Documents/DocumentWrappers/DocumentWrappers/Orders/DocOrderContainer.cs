using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocOrderContainer : DocumentWrapper, IDocContainer, IDocSimpleContainer
	{
		DocOrderContainer(AutoJobOrderContainer orderContainer, BusinessObjectFactory factoryToWrap)
			: base(orderContainer, factoryToWrap)
		{
			QuantityCount = orderContainer.J1_ContainerCount;
		}

		public static DocOrderContainer New(AutoJobOrderContainer orderContainer, BusinessObjectFactory factoryToWrap)
		{
			if (orderContainer == null)
			{
				return null;
			}
			else
			{
				return new DocOrderContainer(orderContainer, factoryToWrap);
			}
		}

		#region Overrides

		public override string ToString()
		{
			return ContainerNumber;
		}

		#endregion

		#region ZShort Fields

		public ZShort ContainerCount
		{
			get { return OrderContainer.J1_ContainerCount; }
		}

		#endregion

		#region ZString Fields

		public ZString ContainerNumber
		{
			get { return OrderContainer.J1_ContainerNumber; }
		}

		public ZString Size
		{
			get { return (Container != null) ? Container.Code : ZString.Empty; }
		}

		#endregion

		#region Wrapper Fields

		public DocRefContainer Container
		{
			get { return DocRefContainer.New(OrderContainer.Container, Factory); }
		}

		#endregion

		#region IDocContainer Members

		#region Wrapper Fields
		public DocDocAddress DepartureContainerParkAddress
		{
			get { return null; }
		}

		#endregion

		#region ZString Fields

		public ZString ContainerCode
		{
			get { return ZString.Empty; }
		}

		public ZString SealNumber
		{
			get { return ContainerCount.ToString(); }
		}

		public ZString SealNumber2
		{
			get { return ZString.Empty; }
		}

		public ZString SealNumber3
		{
			get { return ZString.Empty; }
		}

		public ZString ContainerMode
		{
			get { return ZString.Empty; }
		}

		public ZString ReleaseNum
		{
			get { return ZString.Empty; }
		}

		public ZString SlotReference
		{
			get { return ZString.Empty; }
		}

		public ZBool IsControlledAtmosphere
		{
			get { return false; }
		}

		public ZDecimal SetPointTemp
		{
			get { return 0; }
		}

		public ZString SetPointTempUnit
		{
			get { return ZString.Empty; }
		}

		public ZString TempRecorderSerialNo
		{
			get { return ZString.Empty; }
		}

		public ZString HumidityPercent
		{
			get { return ZString.Empty; }
		}

		public ZString AirVent
		{
			get { return ZString.Empty; }
		}

		public ZString ForwardingInstructionPackages
		{
			get { return ZString.Empty; }
		}

		public ZString ForwardingInstructionWeightHeading
		{
			get { return ZString.Empty; }
		}

		public ZString ForwardingInstructionVolumeHeading
		{
			get { return ZString.Empty; }
		}

		public ZString ForwardingInstructionPackageHeading
		{
			get { return ZString.Empty; }
		}

		public ZString ForwardingInstructionWeight
		{
			get { return ZString.Empty; }
		}

		public ZString ForwardingInstructionVolume
		{
			get { return ZString.Empty; }
		}

		public ZString CommodityDescription
		{
			get { return ZString.Empty; }
		}

		public ZString DescriptionAndStatus
		{
			get { return ZString.Empty; }
		}

		public ZString DeliveryMode
		{
			get { return ZString.Empty; }
		}

		public ZString TareWeightWithUQ
		{
			get { return ZString.Empty; }
		}

		public ZString GrossWeightWithUQ
		{
			get { return ZString.Empty; }
		}

		public ZString TotalAllocatedShipmentPackagesPackType
		{
			get { return ZString.Empty; }
		}

		public ZString WeightUQ
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region ZDecimal Fields

		public ZDecimal TotalAllocatedShipmentWeight
		{
			get { return 0M; }
		}

		public ZDecimal TotalAllocatedShipmentVolume
		{
			get { return 0M; }
		}

		public ZString TotalAllocatedShipmentVolumeUQ
		{
			get { return ZString.Empty; }
		}

		public ZDecimal TotalPackLineVolume
		{
			get { return 0M; }
		}

		public ZDecimal TotalPackLineWeight
		{
			get { return 0M; }
		}

		#endregion

		#region ZDateTime Fields

		public ZDateTime ContainerAvailable
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime LCLAvailable
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime StorageCommences
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime LCLStorageCommences
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime EmptyRequired
		{
			get { return ZDateTime.Empty; }
		}
		public ZDateTime EmptyReturnedBy
		{
			get { return ZDateTime.Empty; }
		}
		public ZDateTime ContainerParkEmptyReturnGateIn
		{
			get { return ZDateTime.Empty; }
		}
		public ZDateTime FullPickDate
		{
			get { return ZDateTime.Empty; }
		}

		#endregion

		#region ZInt Fields

		public ZInt TotalPackLinePackages
		{
			get { return 0; }
		}

		public ZInt TotalAllocatedShipmentPackages
		{
			get { return 0; }
		}

		protected ZInt fQuantityCount;
		public ZInt QuantityCount
		{
			get { return fQuantityCount; }
			set { fQuantityCount = value; }
		}
		#endregion

		#region Extra CommonCartage Fields

		public DocCommonCartageLegCollection ContainerLegs
		{
			get
			{
				if (fContainerLegs == null)
				{
					fContainerLegs = new DocCommonCartageLegCollection(Factory);
				}

				return fContainerLegs;
			}
		}

		DocCommonCartageLegCollection fContainerLegs;

		public DocCommonCartage Cartage
		{
			get { return null; }
		}

		public ZString ContainerType
		{
			get { return ""; }
		}

		public ZDecimal TareWeight
		{
			get { return 0m; }
		}

		public ZDecimal GrossWeight
		{
			get { return 0m; }
		}

		public ZDecimal TotalVolume
		{
			get { return 0m; }
		}

		public ZString TotalVolumeUnit
		{
			get { return ZString.Empty; }
		}

		public ZString SlotArrivalReference
		{
			get { return ""; }
		}

		public ZString SlotDepartureReference
		{
			get { return ""; }
		}

		public ZDateTime SlotArrivalTime
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime SlotDepartureTime
		{
			get { return ZDateTime.Empty; }
		}

		public ZString SlotArrivalDetails
		{
			get { return ""; }
		}

		public ZString SlotDepartureDetails
		{
			get { return ""; }
		}

		public ZString SlotAsArrivalOrDeparture
		{
			get { return ""; }
		}

		#endregion

		#endregion

		#region IDocSimpleContainer Members

		public ZString ClientRef
		{
			get { return ZString.Empty; }
		}

		public ZInt TotalAllocatedJobPackages
		{
			get { return 0; }
		}

		public ZDecimal TotalAllocatedJobWeight
		{
			get { return 0; }
		}

		public ZDecimal TotalAllocatedJobVolume
		{
			get { return 0; }
		}

		public ZString Type
		{
			get { return (Container != null) ? Container.Code : ZString.Empty; }
		}

		#endregion

		#region Implementation

		AutoJobOrderContainer OrderContainer
		{
			get { return (AutoJobOrderContainer)WrappedObject; }
		}

		#endregion
	}
}
