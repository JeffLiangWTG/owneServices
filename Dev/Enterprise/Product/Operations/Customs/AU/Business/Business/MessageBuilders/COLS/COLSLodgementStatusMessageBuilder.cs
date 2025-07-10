using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class COLSLodgementStatusMessageBuilder : COLSMessageBuilder<COLSLodgementStatusMessage>
	{
		public COLSLodgementStatusMessageBuilder(QuarantineColsHeader colsHeader)
		: base(colsHeader)
		{
		}

		protected override ZString MessageType => AUCOLSMessageTypeList.Codes.LodgementStatus;

		protected override BusinessObject MessageParent => colsHeader;

		protected override COLSLodgementStatusMessage GetMessageData()
		{
			return new COLSLodgementStatusMessage();
		}
	}
}
