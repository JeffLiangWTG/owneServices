using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class ContainerWrapperFromWhsDocket : ContainerWrapperEmpty
	{
		public ContainerWrapperFromWhsDocket(WhsDocketContainer containerBO, BusinessObjectFactory factory)
			: base(containerBO, factory)
		{
			Argument.NotNull(factory, "factory");
			ContainerBO = containerBO ?? factory.GetNull<WhsDocketContainer>();
		}
		readonly WhsDocketContainer ContainerBO;

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
			return ContainerBO.WC_ContainerNum;
		}

		protected override ZString GetContainerNumberOrTypeCount()
		{
			return ContainerNo.IsEmpty ? ZString.Format("{0} ({1})", Type.Code, ContainerBO.WC_ContainerNum) : ContainerNo;
		}

		protected override ZString GetSealNo()
		{
			return ContainerBO.WC_SealNum;
		}

		protected override bool? GetIsPalletized()
		{
			return ContainerBO.WC_IsPalletised;
		}

		protected override bool? GetIsIsChargeable()
		{
			return ContainerBO.WC_IsChargeable;
		}

		protected override ZString GetPackages()
		{
			return ContainerBO.WC_ItemCount.ToString();
		}

		protected override ZString GetPallets()
		{
			return ContainerBO.WC_PalletCount.ToString();
		}
	}
}
