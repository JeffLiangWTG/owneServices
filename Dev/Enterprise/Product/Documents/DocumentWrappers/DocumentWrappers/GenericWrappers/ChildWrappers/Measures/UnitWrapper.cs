
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class UnitWrapper : CodeAndDescriptionWrapper
	{
		public UnitWrapper(ZString unitCode, IBusinessObjectCollection unitList, BusinessObjectFactory factory) : base(unitCode, unitList, factory) { }
		public UnitWrapper(ZString unitCode, BusinessObjectCollection unitList, BusinessObjectFactory factory) : base(unitCode, unitList, factory) { }
		public UnitWrapper(ZString unitCode, CodeDescriptionPairList unitList, BusinessObjectFactory factory) : base(unitCode, unitList, factory) { }
		public UnitWrapper(ZString newUnitCode, UnitWrapper sourceUnit) : base(newUnitCode, sourceUnit) { }
	}
}
