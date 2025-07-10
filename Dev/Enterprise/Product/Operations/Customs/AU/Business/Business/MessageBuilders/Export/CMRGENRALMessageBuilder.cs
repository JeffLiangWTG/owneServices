using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.GENRAL;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CMRGENRALMessageBuilder : CMRMessageBuilder
	{
		protected internal override UNHSegment UNH => gENRAL.UNH[0];

		protected internal override BGMSegment BGM => gENRAL.BGM[0];

		protected internal override UNTSegment UNT => gENRAL.UNT[0];

		protected internal override Edifact.Auto.SegmentGroup EdifactMessage => gENRAL;

		protected internal override MessageTypeList UNHMessageType => MessageTypeList.GeneralPurposeMessage;

		protected GENRALMessage gENRAL;
	}
}
