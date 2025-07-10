using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class AmendmentComplXExportSendMessageWrapper : AmendmentDUAExportSendMessageWrapper
	{
		public AmendmentComplXExportSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) : base(cusEntryHeader, certificateData) { }

		const string CustomsProcedureCat2Code = "X";

		protected override ZString CustomsProcedureCategory2Core => CustomsProcedureCat2Code;

		protected override IEnumerable<IDUAExportLine> LinesCore
		{
			get
			{
				if (exportLines == null)
				{
					exportLines = new List<AmendmentComplXExportLineWrapper>();

					exportLines.AddRange(entryHeader.MergedLines.Cast<CusEntryLine>().Select(entryLine => new AmendmentComplXExportLineWrapper(entryLine)));
				}
				return exportLines;
			}
		}
		List<AmendmentComplXExportLineWrapper> exportLines;
	}
}
