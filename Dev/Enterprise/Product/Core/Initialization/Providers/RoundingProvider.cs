using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Initialisation
{
	class RoundingProvider : IRoundingProvider
	{
		public decimal Round(decimal value, int decimalPlaces)
		{
			return Utilities.Round(value, decimalPlaces);
		}
	}
}
