using CargoWise.Customs.JP.MessageContracts;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Customs.JP.MessageDefinitions.Inbound;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.JP.Common;

public class MismatchInformationMessageDocumentWrapper(IJPInboundMessageParseResult parseResult, BusinessObjectFactory factory)
	: InboundMessageIncludingItemsDocumentWrapper<IMismatchInformation, MismatchInformationMessageItemWrapper, MismatchInformationHouseBillProvider>(parseResult, factory)
{
	#region Header Fields

	public ZString H_2 => messageProvider?.CrossIndicator ?? ZString.Empty;

	public ZString H_3 => messageProvider?.HAWBInformationInputter ?? ZString.Empty;

	public ZString H_4 => messageProvider?.HAWBInformationInputterAgent ?? ZString.Empty;

	public ZString H_5 => messageProvider?.ConsolidationInformationInputter ?? ZString.Empty;

	public ZString H_6 => messageProvider?.ArrivalFlightNumber ?? ZString.Empty;

	public ZString H_7 => messageProvider?.ArrivalFlightDate ?? ZString.Empty;

	public ZString H_8 => messageProvider?.ArrivalAirport ?? ZString.Empty;

	public ZString H_9 => messageProvider?.MasterBillNumber ?? ZString.Empty;

	public ZString H_10 => messageProvider?.ArrivalDate?.ToNACCSDate() ?? ZString.Empty;

	#endregion

	#region Items

	protected override DocumentWrapperCollection<MismatchInformationMessageItemWrapper, MismatchInformationHouseBillProvider> GetItemsCore() => new(messageProvider.HouseBillItems, Factory);

	#endregion
}
