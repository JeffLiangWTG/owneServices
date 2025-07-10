using CargoWise.Customs.CL.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public class BLOriginalMessageSender : BaseMessageSender
	{
		public BLOriginalMessageSender(AsycudaBill bill, IBLRequest dataProvider) : base(bill)
		{
			this.dataProvider = dataProvider;
		}
		readonly IBLRequest dataProvider;

		protected override ZString MessageType => MessageTypes.Codes.CHB;

		protected override ZString MessageSubType => MessageSubTypeCodes.Codes.Original;

		protected override ZString MessageText => BuildMessageText(new BLRequestMessageBuilder(dataProvider));
	}
}
