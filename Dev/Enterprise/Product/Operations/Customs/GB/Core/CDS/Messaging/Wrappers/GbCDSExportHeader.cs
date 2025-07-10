using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GB.Business.Messaging;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.GB.CDS.Messaging.Wrappers
{
	public class GbCDSExportHeader : GbHeader
	{
		public GbCDSExportHeader(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		public override ZString JobType => "E";

		public ZBool HasDeclarationUCRPartSuffix => !EntryHeader.DeclarationUCRPartSuffix.IsEmpty;

		public ZString MasterOpt => ZString.Empty;

		public ZString MovementReference
		{
			get
			{
				var result = ZString.Empty;
				var arrivalDateTime = DateAndTimeTheGoodsWillBeAvailableForInspectionAtLCPPremises;
				if (!arrivalDateTime.IsEmpty)
				{
					result = arrivalDateTime.ToString("ddMMMHHmm", CultureInfo.InvariantCulture);
				}
				return result;
			}
		}

		protected override IDocAddress Customer => ShipperDocAddress;

		protected override GbLine GetNewGbLine(CusEntryLine entryLine) => new GbCDSExportLine(this, (Business.Declaration.CusEntryLine)entryLine);
	}
}
