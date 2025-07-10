using CargoWise.Customs.CL.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public class AWBAmendMessageSender : BaseMessageSender
	{
		public AWBAmendMessageSender(AsycudaBill bill, IAWBRequest dataProvider) : base(bill)
		{
			this.dataProvider = dataProvider;
		}
		readonly IAWBRequest dataProvider;

		protected override ZString MessageType => MessageTypes.Codes.CHD;

		protected override ZString MessageSubType => MessageSubTypeCodes.Codes.Change;

		protected override ZString MessageText => BuildMessageText(new AWBRequestMessageBuilder(dataProvider));
	}
}
