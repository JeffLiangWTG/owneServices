using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.EU.NCTS.Business
{
	class SpecialMention : IStatement
	{
		public SpecialMention(NctsAdditionalInfo additionalInfo)
		{
			if (additionalInfo == null)
			{
				throw new ArgumentNullException(nameof(additionalInfo));
			}

			Statement = additionalInfo.CSI_Code;
			StatementText = additionalInfo.CSI_Description;
			ExportFromCountry = additionalInfo.CSI_RN_NKCountryCode;
			ExportFromEC = additionalInfo.CSI_NctsExportFromEC;
		}

		public ZString ExportFromCountry { get; }
		public ZBool ExportFromEC { get; }
		public ZString Statement { get; }
		public ZString StatementText { get; }
	}
}
