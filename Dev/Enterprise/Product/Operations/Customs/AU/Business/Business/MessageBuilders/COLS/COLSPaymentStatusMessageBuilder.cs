using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class COLSPaymentStatusMessageBuilder : COLSMessageBuilder<COLSPaymentStatusMessage>
	{
		public COLSPaymentStatusMessageBuilder(QuarantineColsHeader colsHeader)
			: base(colsHeader)
		{
		}

		protected override ZString MessageType => AUCOLSMessageTypeList.Codes.PaymentStatus;

		protected override BusinessObject MessageParent => colsHeader;

		protected override COLSPaymentStatusMessage GetMessageData()
		{
			return new COLSPaymentStatusMessage();
		}
	}
}
