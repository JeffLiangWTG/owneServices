using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.ContainerYard.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class GateYardUnitWrapper : GenericWrapper
	{
		public GateYardUnitWrapper(YardUnit yardUnitBO, BusinessObjectFactory factory)
			: base(yardUnitBO, factory)
		{
			this.yardUnitBO = yardUnitBO ?? factory.GetNull<YardUnit>();
		}
		readonly YardUnit yardUnitBO;

		public CodeAndDescriptionWrapper Quality => quality ?? (quality = new CodeAndDescriptionWrapper(yardUnitBO.GTY_Quality, yardUnitBO.Lookups.Qualities, Factory));
		CodeAndDescriptionWrapper quality;

		public ContainerTypeWrapper TypeSize => typeSize ?? (typeSize = new ContainerTypeWrapper(yardUnitBO.TypeSize, Factory));
		ContainerTypeWrapper typeSize;

		public WeightWrapper WeightGross => weightGross ?? (weightGross = new WeightWrapper(yardUnitBO.GTY_GrossWeight, yardUnitBO.GTY_GrossWeightUQ, 3, new CodeDescriptionPairList(OLookUpEditType.Weight), Factory));
		WeightWrapper weightGross;

		public ZString SealNumber => yardUnitBO.GTY_Seal1;

		public ZBool IsEmptyContainer => yardUnitBO.GTY_IsEmpty;

		public ZString UnitNumber => yardUnitBO.GTY_UnitNumber;
	}
}
