using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class ContainerWrapperFromOneOffContainer : ContainerWrapperEmpty
	{
		public ContainerWrapperFromOneOffContainer(RateOneOffContainers container, BusinessObjectFactory factory)
			: base(container, factory)
		{
			Argument.NotNull(factory, "factory");
			this.container = container ?? factory.GetNull<RateOneOffContainers>();
		}

		protected override FreightWrapper GetFreightJob()
		{
			var freightWrappers = FreightWrapper.New(container.Parent.ParentQuote, Factory);
			if (freightWrappers.Length > 0)
			{
				return freightWrappers[0];
			}
			return null;
		}

		protected override ContainerTypeWrapper GetTypeWrapper()
		{
			return new ContainerTypeWrapper(container.Container, Factory);
		}

		protected override WeightWrapper GetWeightTare()
		{
			return new WeightWrapper(CalculateTare(), Constants.Weight.Kilograms, 3, new CodeDescriptionPairList(OLookUpEditType.Weight), Factory);
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
			return new WeightWrapper(CalculateTare(), Constants.Weight.Kilograms, 3, new CodeDescriptionPairList(OLookUpEditType.Weight), Factory);
		}

		protected override VolumeWrapper GetVolumeGoods()
		{
			return new VolumeWrapper(0, Constants.Volume.CubicMetres, new CodeDescriptionPairList(OLookUpEditType.Volume), Factory);
		}

		protected override CommodityWrapperCollection GetCommodities()
		{
			return new CommodityWrapperCollection(container, Factory);
		}

		protected override ZString GetContainerNumberOrTypeCount()
		{
			RefContainer type = container.Container;

			if (type != null)
			{
				return string.Format("{0} ({1})", type.RC_Code, container.TC_ContainerCount);
			}
			else
			{
				return ZString.Empty;
			}
		}

		protected override ZInt GetContainerCount()
		{
			return container.TC_ContainerCount;
		}

		protected override ServiceWrapperCollection GetServices()
		{
			return new ServiceWrapperCollection(this, Factory);
		}

		ZDecimal CalculateTare()
		{
			ZDecimal tare;
			RefContainer type = container.Container;

			if (type == null)
			{
				tare = 0m;
			}
			else
			{
				tare = type.RC_TareWeight * container.TC_ContainerCount;
			}

			return tare;
		}
		readonly RateOneOffContainers container;
	}
}
