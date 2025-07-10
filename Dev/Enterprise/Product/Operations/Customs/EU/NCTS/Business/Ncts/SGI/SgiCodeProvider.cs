using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class SgiCodeProvider : List<ISgiCode>
	{
		public SgiCodeProvider(NctsCommonCargoDesc line)
		{
			this.line = line;
			Initialise();
		}

		void Initialise()
		{
			foreach (NctsAdditionalInfo ai in line.AdditionalInfos)
			{
				if (ai.CSI_Code.StartsWith("SGI", System.StringComparison.Ordinal))  // e.g. SGIXX
				{
					ZDecimal.TryParse(ai.CSI_Description, out var qty);
					var sgi = new SgiCode(ai.CSI_Code.SubstringSafe(3, 2), qty);  // XX
					Add(sgi);
				}
			}
		}

		readonly NctsCommonCargoDesc line;
	}
}
