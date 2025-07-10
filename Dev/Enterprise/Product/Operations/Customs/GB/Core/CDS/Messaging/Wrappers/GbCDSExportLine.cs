using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Messaging;

namespace Enterprise.Customs.GB.CDS.Messaging.Wrappers
{
	class GbCDSExportLine : GbLine
	{
		public GbCDSExportLine(GbHeader header, CusEntryLine actualEntryLine) : base(header, actualEntryLine)
		{
		}
	}
}
