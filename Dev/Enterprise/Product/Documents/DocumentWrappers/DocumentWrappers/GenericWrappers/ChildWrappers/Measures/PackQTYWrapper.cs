using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class PackQTYWrapper : ValueAndUnitWrapper, ITotalValueAndUnits
	{
		public PackQTYWrapper(ZInt value, ZString unitCode, CodeDescriptionPairList unitList, BusinessObjectFactory factory)
			: base(value, unitCode, unitList, factory)
		{
		}

		internal new static PackQTYWrapper Empty
		{
			get { return new PackQTYWrapper(ZInt.Zero, ZString.Empty, new CodeDescriptionPairList(), null); }
		}

		public ValueAndUnitSelfTotaller GetNewForTotalling()
		{
			return new ValueAndUnitSelfTotaller(Value, Unit.Code);
		}

		public void AddSelfToResult(ValueAndUnitSelfTotaller result)
		{
			result.Value += Value;
			if (result.UnitCode != Unit.Code)
			{
				result.UnitCode = Constants.PkgUnit.Package;
			}
		}
	}
}
