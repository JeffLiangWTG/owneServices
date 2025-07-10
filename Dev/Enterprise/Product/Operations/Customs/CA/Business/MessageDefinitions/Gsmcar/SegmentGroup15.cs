using Enterprise.Edifact.Auto;

using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.CA.Edifact.GSMCAR
{
	public class SegmentGroup15 : SegmentGroup
	{
		public SegmentGroup15()
		{
			GID = new SegmentMessageSection<GIDSegment>(1);
			PAC = new SegmentMessageSection<PACSegment>(1);
			FTX = new SegmentMessageSection<FTXSegment>(1);
			MEA = new SegmentMessageSection<MEASegment>(1);
			SGP = new SegmentMessageSection<SGPSegment>(1);
			DGS = new SegmentMessageSection<DGSSegment>(1);
			PCI = new SegmentMessageSection<PCISegment>(1);
			CST = new SegmentMessageSection<CSTSegment>(1);
		}

		public readonly SegmentMessageSection<GIDSegment> GID;
		public readonly SegmentMessageSection<PACSegment> PAC;
		public readonly SegmentMessageSection<FTXSegment> FTX;
		public readonly SegmentMessageSection<MEASegment> MEA;
		public readonly SegmentMessageSection<SGPSegment> SGP;
		public readonly SegmentMessageSection<DGSSegment> DGS;
		public readonly SegmentMessageSection<PCISegment> PCI;
		public readonly SegmentMessageSection<CSTSegment> CST;

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { GID, PAC, FTX, MEA, SGP, DGS, PCI, CST };
		}
	}
}
