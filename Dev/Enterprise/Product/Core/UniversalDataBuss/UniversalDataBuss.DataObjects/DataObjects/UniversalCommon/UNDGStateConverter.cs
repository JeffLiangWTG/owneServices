using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	public class UNDGStateConverter : EnumConverter<UNDGState>
	{
		protected override ZString[] GetCodes()
		{
			return new ZString[]
			{
				"L",
				"G",
				"S",
				"E",
				"A",
			};
		}

		protected override UNDGState[] GetEnumValues()
		{
			return new UNDGState[]
			{
				UNDGState.Liquid,
				UNDGState.Gas,
				UNDGState.Solid,
				UNDGState.ExplosiveSubstance,
				UNDGState.ExplosiveArticle,
			};
		}
	}
}
