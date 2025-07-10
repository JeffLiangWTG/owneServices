using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSREP;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CMRCUSREPMessageBuilder : CMRMessageBuilder
	{
		protected internal override UNHSegment UNH => cUSREP.UNH[0];

		protected internal override BGMSegment BGM => cUSREP.BGM[0];

		protected internal override UNTSegment UNT => cUSREP.UNT[0];

		protected internal override Edifact.Auto.SegmentGroup EdifactMessage => cUSREP;

		protected internal override MessageTypeList UNHMessageType => MessageTypeList.CustomsConveyanceReportMessage;

		protected CUSREPMessage cUSREP;
	}
}
