using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class CUSTAXLineProvider : ICUSTAXLine
	{
		public CUSTAXLineProvider(GCTAXMBodyGoodsItem line)
		{
			this.line = Argument.NotNull(line, nameof(line));
		}
		readonly GCTAXMBodyGoodsItem line;

		public string LineNumber => line.SequenceNumber;

		public string LineCompletionFlag => line.CompletionFlag;

		public decimal? CustomsValue => line.Assessment?.CustomsValueSpecified ?? false ? line.Assessment.CustomsValue : null;

		public IReadOnlyCollection<ICUSTAXLineDuty> Duties => duties ?? (duties = line.CustomsDuties?.CustomsDuty?.Select(x => new CUSTAXLineDutyProvider(x, line.Assessment?.TaxValue ?? 0, line.Assessment?.CustomsValue ?? 0)).ToArray() ?? Array.Empty<ICUSTAXLineDuty>());

		IReadOnlyCollection<ICUSTAXLineDuty> duties;

		public DateTime? ExportLimitDate => (line.CustomsTaxAssessment?.ExportLimitDateSpecified ?? false) ? line.CustomsTaxAssessment.ExportLimitDate : null;
	}
}
