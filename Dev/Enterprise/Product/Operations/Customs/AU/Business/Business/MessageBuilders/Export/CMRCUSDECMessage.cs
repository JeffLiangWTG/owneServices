using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSDEC;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CMRCUSDECMessageBuilder : CMRMessageBuilder
	{
		protected internal override UNHSegment UNH => cUSDEC.UNH[0];

		protected internal override BGMSegment BGM => cUSDEC.BGM[0];

		protected internal override UNTSegment UNT => cUSDEC.UNT[0];

		protected internal override Edifact.Auto.SegmentGroup EdifactMessage => cUSDEC;

		protected internal override MessageTypeList UNHMessageType => MessageTypeList.CustomsDeclarationMessage;

		protected internal CUSDECMessage cUSDEC;
	}
}
