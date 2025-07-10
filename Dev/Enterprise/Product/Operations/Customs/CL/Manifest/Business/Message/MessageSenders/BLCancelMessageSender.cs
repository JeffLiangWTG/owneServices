using CargoWise.Customs.CL.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public class BLCancelMessageSender : BaseMessageSender
	{
		public BLCancelMessageSender(AsycudaBill bill, IBLCancelRequest dataProvider) : base(bill)
		{
			this.dataProvider = dataProvider;
		}
		readonly IBLCancelRequest dataProvider;

		protected override ZString MessageType => MessageTypes.Codes.CHC;

		protected override ZString MessageSubType => MessageSubTypeCodes.Codes.Cancellation;

		protected override ZString MessageText => BuildMessageText(new BLCancelRequestMessageBuilder(dataProvider));
	}
}
