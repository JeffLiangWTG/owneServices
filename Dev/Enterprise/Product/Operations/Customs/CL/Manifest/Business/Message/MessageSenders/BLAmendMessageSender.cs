using CargoWise.Customs.CL.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public class BLAmendMessageSender : BaseMessageSender
	{
		public BLAmendMessageSender(AsycudaBill bill, IBLRequest dataProvider) : base(bill)
		{
			this.dataProvider = dataProvider;
		}
		readonly IBLRequest dataProvider;

		protected override ZString MessageType => MessageTypes.Codes.CHA;

		protected override ZString MessageSubType => MessageSubTypeCodes.Codes.Change;

		protected override ZString MessageText => BuildMessageText(new BLRequestMessageBuilder(dataProvider));
	}
}
