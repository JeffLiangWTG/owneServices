using CargoWise.Customs.JP.MessageContracts;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Customs.JP.MessageDefinitions.Inbound;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.JP.Common;

public sealed class HBLCargoRegistrationInformationWrapper(IJPInboundMessageParseResult parseResult, BusinessObjectFactory factory)
	: InboundMessageIncludingItemsDocumentWrapper<IHBLRegistrationInformationProvider, HBLCargoRegistrationInformationItemWrapper, HBLRegistrationInformationItemProvider>(parseResult, factory)
{
	#region Header Fields
	public ZString H_2 => messageProvider?.MasterBillNumber ?? ZString.Empty;

	public ZString H_3 => messageProvider?.BondedLocationCode ?? ZString.Empty;

	public ZString H_4 => messageProvider?.ConsolidatedCargoInformationRegistrant ?? ZString.Empty;

	#endregion

	protected override DocumentWrapperCollection<HBLCargoRegistrationInformationItemWrapper, HBLRegistrationInformationItemProvider> GetItemsCore() => new(messageProvider.HBLInformationItems, Factory, includingIndex: true);
}
