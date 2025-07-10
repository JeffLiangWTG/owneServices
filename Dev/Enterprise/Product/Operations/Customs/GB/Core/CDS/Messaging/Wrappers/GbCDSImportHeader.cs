using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GB.Business.Messaging;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.GB.CDS.Messaging.Wrappers
{
	public class GbCDSImportHeader : GbHeader
	{
		public GbCDSImportHeader(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		public override ZString JobType => "I";

		public ZString OSAirTransportLoad => declaration.JE_IATALoadPort;

		protected override IDocAddress Customer => ConsigneeDocAddress;

		protected override GbLine GetNewGbLine(CusEntryLine entryLine) => new GbCDSImportLine(this, (Business.Declaration.CusEntryLine)entryLine);
	}
}
