using CargoWise.Customs.JP.MessageContracts;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.JP.Common
{
	public sealed class MoveInNoticeMessageWrapper(IJPInboundMessageParseResult parseResult, BusinessObjectFactory factory)
		: InboundMessageIncludingItemsDocumentWrapper<ICarryInDocument, MoveInNoticeMessageItemWrapper, CarryInDocumentItemProvider>(parseResult, factory)
	{
		#region Header Fields
		public ZString H_2 => messageProvider?.MoveInNoticeNumber ?? ZString.Empty;

		public ZString H_3 => messageProvider?.MoveInDestination ?? ZString.Empty;

		public ZString H_4 => messageProvider?.MoveInDate?.ToNACCSDate() ?? ZString.Empty;

		public ZString H_5 => messageProvider?.AirCargoAgentNACCSUserCode ?? ZString.Empty;
		#endregion

		protected override DocumentWrapperCollection<MoveInNoticeMessageItemWrapper, CarryInDocumentItemProvider> GetItemsCore() => new(messageProvider.CarryInDocumentItems, Factory);
	}
}
