using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class COLSSwitchAepLodgementMessageBuilder : COLSMessageBuilder<COLSSwitchAepLodgementMessage>
	{
		public COLSSwitchAepLodgementMessageBuilder(QuarantineColsHeader colsHeader)
			: base(colsHeader)
		{
		}

		protected override ZString MessageType => AUCOLSMessageTypeList.Codes.SwitchAepLodgement;

		protected override BusinessObject MessageParent => colsHeader;

		protected override COLSSwitchAepLodgementMessage GetMessageData()
		{
			return new COLSSwitchAepLodgementMessage();
		}
	}
}
