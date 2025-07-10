using System.Linq;
using CargoWise.Types;

namespace Enterprise.Registry.Business
{
	public static class PrintChargesBilledToLocalClientAtDestAsCollectHelper
	{
		public static bool IsEnabled(ZString transportMode, ZString exportCountry, ZString importCountry, ZString fallbackExportCountry)
		{
			var settings = FreightDataRegistry.Instance.PrintChargesBilledToLocalClientAtDestAsCollect.Value;
			if (settings != null)
			{
				return settings.OfType<PrintChargesBilledToLocalClientAtDestAsCollect>().Any(s =>
					s.TransportMode == transportMode
					&& (s.ExportCountry == exportCountry || (!fallbackExportCountry.IsEmpty && s.ExportCountry == fallbackExportCountry) || s.ExportCountry == ZString.Empty)
					&& (s.ImportCountry == importCountry || s.ImportCountry == ZString.Empty)
				);
			}

			return false;
		}
	}
}

