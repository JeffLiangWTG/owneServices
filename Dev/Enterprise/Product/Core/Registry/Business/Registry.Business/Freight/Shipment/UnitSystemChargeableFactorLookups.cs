
namespace Enterprise.Registry.Business
{
	using System.Linq;
	using Enterprise.Core;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.Core;

	public class UnitSystemChargeableFactorLookups : ConversionFactorLookups
	{
		public UnitSystemChargeableFactorLookups(ConversionFactorViewModel parent, UnitsSystem unitsSystem)
			: base(parent)
		{
			this.unitsSystem = unitsSystem;
		}

		protected override CodeDescriptionPairList GetConversionFactors()
		{
			var list = new CodeDescriptionPairList();

			foreach (var factor in ConversionFactor.Standard.All.Where(f => f.UnitsSystem == unitsSystem))
			{
				list.AddPair(factor.ToString(), factor.Description);
			}

			return list;
		}

		readonly UnitsSystem unitsSystem;
	}
}