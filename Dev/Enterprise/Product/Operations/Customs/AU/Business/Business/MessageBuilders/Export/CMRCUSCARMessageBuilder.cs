using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSCAR;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CMRCUSCARMessageBuilder : CMRMessageBuilder
	{
		public CMRCUSCARMessageBuilder()
			: this(ZString.Empty)
		{
		}

		public CMRCUSCARMessageBuilder(ZString messageOwnerSiteID)
			: base(messageOwnerSiteID)
		{
		}

		protected internal override UNHSegment UNH => CUSCAR.UNH[0];

		protected internal override BGMSegment BGM => CUSCAR.BGM[0];

		protected internal override UNTSegment UNT => CUSCAR.UNT[0];

		protected internal override Edifact.Auto.SegmentGroup EdifactMessage => CUSCAR;

		protected internal override MessageTypeList UNHMessageType => MessageTypeList.CustomsCargoReportMessage;

		protected CUSCARMessage fCUSCAR;

		internal CUSCARMessage CUSCAR
		{
			get { return fCUSCAR; }
			set { fCUSCAR = value; }
		}
	}
}
