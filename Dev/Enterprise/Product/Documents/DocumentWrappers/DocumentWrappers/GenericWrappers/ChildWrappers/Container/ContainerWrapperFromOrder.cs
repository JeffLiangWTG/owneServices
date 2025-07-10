using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class ContainerWrapperFromOrder : ContainerWrapperEmpty
	{
		public ContainerWrapperFromOrder(OrderContainer containerBO, BusinessObjectFactory factory)
			: base(containerBO, factory)
		{
			Argument.NotNull(factory, "factory");
			ContainerBO = containerBO ?? factory.GetNull<OrderContainer>();
		}
		readonly OrderContainer ContainerBO;

		protected override FreightWrapper GetFreightJob()
		{
			if (ContainerBO.Order != null)
			{
				var freightWrappers = FreightWrapper.New(ContainerBO.Order, Factory);
				if (freightWrappers.Length > 0)
				{
					return freightWrappers[0];
				}
			}
			return null;
		}

		protected override ContainerTypeWrapper GetTypeWrapper()
		{
			return new ContainerTypeWrapper(ContainerBO.Container, Factory);
		}

		protected override WeightWrapper GetWeightTare()
		{
			return new WeightWrapper(ContainerBO.Container != null ? ContainerBO.Container.RC_TareWeight : ZDecimal.Zero, Core.Constants.Weight.Kilograms, 3, new CodeDescriptionPairList(OLookUpEditType.Weight), Factory);
		}

		protected override ZBool GetIsReefer()
		{
			RefContainer type = ContainerBO.Container;
			return type != null && type.RC_ContainerType == Constants.ContainerTypes.Refrigerated;
		}

		protected override ZString GetContainerNo()
		{
			return ContainerBO.J1_ContainerNumber;
		}

		protected override ZString GetContainerNumberOrTypeCount()
		{
			return ContainerNo.IsEmpty ? ZString.Format("{0} ({1})", Type.Code, ContainerBO.J1_ContainerCount) : ContainerNo;
		}

		protected override ZInt GetContainerCount()
		{
			return ContainerBO.J1_ContainerCount;
		}

		protected override ZString GetSealNo()
		{
			return ContainerBO.J1_SealNum;
		}

		protected override ZString GetSealNo2()
		{
			return ContainerBO.J1_AdditionalSealNum;
		}

		protected override ZString GetSealNo3()
		{
			return ContainerBO.J1_Additional2SealNum;
		}
	}
}
