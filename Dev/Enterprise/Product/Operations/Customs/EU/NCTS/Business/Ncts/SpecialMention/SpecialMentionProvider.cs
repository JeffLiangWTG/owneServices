using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class SpecialMentionProvider : List<IStatement>
	{
		public SpecialMentionProvider(NctsCommonCargoDesc line)
		{
			this.line = line;
			Initialise();
		}

		void Initialise()
		{
			foreach (NctsAdditionalInfo ai in line.AdditionalInfos)
			{
				if (!ai.CSI_Code.StartsWith("SGI", System.StringComparison.Ordinal))  // e.g. not SGIXX
				{
					var specialMention = new SpecialMention(ai);
					Add(specialMention);
				}
			}
		}

		readonly NctsCommonCargoDesc line;
	}
}
