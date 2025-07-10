using CargoWise.Customs.JP.MessageContracts;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.JP.Common;

public abstract class InboundMessageIncludingItemsDocumentWrapper<TParentProvider, TItemWrapper, TItemProvider>(IJPInboundMessageParseResult parseResult, BusinessObjectFactory factory) : InboundMessageDocumentWrapper<TParentProvider>(parseResult, factory)
	where TParentProvider : IJPInboundMessageDataProvider
	where TItemWrapper : DocumentWrapper
{
	public DocumentWrapperCollection<TItemWrapper, TItemProvider> Items => items ??= GetItemsCore();
	DocumentWrapperCollection<TItemWrapper, TItemProvider> items;

	protected abstract DocumentWrapperCollection<TItemWrapper, TItemProvider> GetItemsCore();
}
