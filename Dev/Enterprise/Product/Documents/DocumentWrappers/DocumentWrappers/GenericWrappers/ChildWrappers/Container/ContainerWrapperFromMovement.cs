using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class ContainerWrapperFromMovement : ContainerWrapperEmpty
	{
		public ContainerWrapperFromMovement(ContainerMovement movement, BusinessObjectFactory factory)
			: base(movement, factory)
		{
			Argument.NotNull(factory, "factory");
			this.movement = movement ?? factory.GetNull<ContainerMovement>();
		}

		readonly ContainerMovement movement;

		protected override FreightWrapper GetFreightJob()
		{
			var containers = movement.RelatedInfo.LoadContainers();
			var freightWrappers = containers.Length > 0 ? FreightWrapper.New(containers[0], Factory) : FreightWrapper.New(null, Factory);
			if (freightWrappers.Length > 0)
			{
				return freightWrappers[0];
			}
			return null;
		}

		protected override ContainerTypeWrapper GetTypeWrapper()
		{
			RefContainerStock stock = movement.Stock;
			RefContainer containerType;

			if (stock == null || (containerType = stock.Container) == null)
			{
				return new ContainerTypeWrapper(null, Factory);
			}
			else
			{
				return new ContainerTypeWrapper(containerType, Factory);
			}
		}

		protected override WeightWrapper GetWeightGoods()
		{
			return new WeightWrapper(0, Constants.Weight.Kilograms, 3, new CodeDescriptionPairList(OLookUpEditType.Weight), Factory);
		}

		protected override WeightWrapper GetWeightDunnage()
		{
			return new WeightWrapper(0, Constants.Weight.Kilograms, 3, new CodeDescriptionPairList(OLookUpEditType.Weight), Factory);
		}

		protected override WeightWrapper GetWeightGross()
		{
			return new WeightWrapper(0, Constants.Weight.Kilograms, 3, new CodeDescriptionPairList(OLookUpEditType.Weight), Factory);
		}

		protected override VolumeWrapper GetVolumeGoods()
		{
			return new VolumeWrapper(0, Constants.Volume.CubicMetres, new CodeDescriptionPairList(OLookUpEditType.Volume), Factory);
		}

		protected override ValueAndUnitWrapper GetPackCount()
		{
			return ValueAndUnitWrapper.Empty;
		}

		protected override CommodityWrapperCollection GetCommodities()
		{
			return new CommodityWrapperCollection(Factory);
		}

		protected override ValueAndUnitWrapper GetSetPointTemperature()
		{
			return ValueAndUnitWrapper.Empty;
		}

		protected override ValueAndUnitWrapper GetAirVentFlow()
		{
			return ValueAndUnitWrapper.Empty;
		}

		protected override ZBool GetIsReefer()
		{
			RefContainer type = movement.Stock == null ? null : movement.Stock.Container;
			return type != null && type.RC_ContainerType == Constants.ContainerTypes.Refrigerated;
		}

		protected override ZString GetContainerNo()
		{
			RefContainerStock stock = movement.Stock;
			return stock == null ? ZString.Empty : stock.R6_ContainerNum;
		}

		protected override ZString GetContainerNumberOrTypeCount()
		{
			return ContainerNo;
		}

		protected override ZDecimal GetLength()
		{
			RefContainerStock stock = movement.Stock;
			RefContainer container;

			if (stock == null || (container = stock.Container) == null)
			{
				return ZDecimal.Zero;
			}
			else
			{
				return container.RC_Length;
			}
		}

		protected override ZDecimal GetWidth()
		{
			RefContainerStock stock = movement.Stock;
			RefContainer container;

			if (stock == null || (container = stock.Container) == null)
			{
				return ZDecimal.Zero;
			}
			else
			{
				return container.RC_Width;
			}
		}

		protected override ZDecimal GetHeight()
		{
			RefContainerStock stock = movement.Stock;
			RefContainer container;

			if (stock == null || (container = stock.Container) == null)
			{
				return ZDecimal.Zero;
			}
			else
			{
				return container.RC_Height;
			}
		}

		protected override ServiceWrapperCollection GetServices()
		{
			return new ServiceWrapperCollection(this, Factory);
		}

		protected override DetentionWrapper GetImportDetention()
		{
			return GetDetentionWrapperOfType(DetentionInvoiceType.Codes.Import);
		}

		protected override DetentionWrapper GetExportDetention()
		{
			return GetDetentionWrapperOfType(DetentionInvoiceType.Codes.Export);
		}

		DetentionWrapper GetDetentionWrapperOfType(string detentionType)
		{
			string actualDetentionType = ContainerMovementTypes.GetDetentionCalculation(movement.E9_MovementType);

			if (actualDetentionType != detentionType)
			{
				return DetentionWrapper.Empty;
			}
			else
			{
				DetentionStrategy strategy = DetentionStrategy.New(detentionType);
				ZDateTime released = strategy.GetStartOfDetentionFreePeriod(movement);
				ZDateTime startOfDetention = strategy.GetStartOfDetentionPeriod(movement);
				ZDateTime lastFreeDay = startOfDetention.IsEmpty ? startOfDetention : startOfDetention.AddDays(-1);
				RefUNLOCO port = movement.Depot != null ? movement.Depot.EffectiveRelatedPortCode : null;

				return new DetentionWrapper(released, lastFreeDay, movement.E9_MovementDate, movement.E9_DetentionDays, port != null ? port.RL_Code : ZString.Empty, Factory);
			}
		}
	}
}
