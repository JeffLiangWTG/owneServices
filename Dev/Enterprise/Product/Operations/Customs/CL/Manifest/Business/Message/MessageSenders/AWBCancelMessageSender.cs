using CargoWise.Customs.CL.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public class AWBCancelMessageSender : BaseMessageSender
	{
		public AWBCancelMessageSender(AsycudaBill bill, IAWBCancelRequest dataProvider) : base(bill)
		{
			this.dataProvider = dataProvider;
		}
		readonly IAWBCancelRequest dataProvider;

		protected override ZString MessageType => MessageTypes.Codes.CHF;

		protected override ZString MessageSubType => MessageSubTypeCodes.Codes.Cancellation;

		protected override ZString MessageText => BuildMessageText(new AWBCancelRequestMessageBuilder(dataProvider));
	}
}

