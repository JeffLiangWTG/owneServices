using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using static Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.CustomsMessageSending.IT;
using CustomsMessageBuilder = CargoWise.Customs.IT.MessageContracts.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

sealed class H2ItemWrapper : IH2Item
{
	public H2ItemWrapper(CusEntryLine entryLine, IMessageSendingWrapperFactory messageSendingWrapperFactory)
	{
		Argument.NotNull(entryLine, nameof(entryLine));
		Argument.NotNull(entryLine.Declaration, nameof(entryLine.Declaration));
		Argument.NotNull(messageSendingWrapperFactory, nameof(messageSendingWrapperFactory));

		entryLineWrapper = new Lazy<ICusEntryLineCustomsMessageWrapper>(() => messageSendingWrapperFactory.GetNewCusEntryLineCustomsMessageWrapper(entryLine));
	}

	readonly Lazy<ICusEntryLineCustomsMessageWrapper> entryLineWrapper;

	#region IH2Item

	int IH2Item.ItemNumber => EntryLineWrapper.ItemNumber;

	ICustomsProcedure IH2Item.Procedure => EntryLineWrapper.Procedure;

	IReadOnlyCollection<IPreviousDocument> IH2Item.PreviousDocuments => EntryLineWrapper.PreviousDocuments ?? Array.Empty<IPreviousDocument>();

	IReadOnlyCollection<IAdditionalInformation> IH2Item.AdditionalInformation => EntryLineWrapper.AdditionalInformation ?? Array.Empty<IAdditionalInformation>();

	IReadOnlyCollection<CustomsMessageBuilder.ISupportingDocument> IH2Item.SupportingDocuments => EntryLineWrapper.SupportingDocuments ?? Array.Empty<CustomsMessageBuilder.ISupportingDocument>();

	IReadOnlyCollection<IAdditionalSupplyChainActor> IH2Item.AdditionalSupplyChainActors => EntryLineWrapper.AdditionalSupplyChainActors ?? Array.Empty<IAdditionalSupplyChainActor>();

	IReadOnlyCollection<IBaseAmount> IH2Item.BaseAmounts => EntryLineWrapper.BaseAmounts ?? Array.Empty<IBaseAmount>();

	int? IH2Item.Preferences => EntryLineWrapper.Preferences;

	string IH2Item.DestinationCountryCode => EntryLineWrapper.DestinationCountryCode;

	string IH2Item.DestinationStateCode => EntryLineWrapper.DestinationStateCode;

	string IH2Item.DispatchCountryCode => EntryLineWrapper.DispatchCountryCode;

	string IH2Item.OriginCountryCode => EntryLineWrapper.OriginCountryCode;

	string IH2Item.PreferredOriginCountryCode => EntryLineWrapper.PreferredOriginCountryCode;

	decimal? IH2Item.SupplementaryUnit => EntryLineWrapper.SupplementaryUnit;

	decimal IH2Item.GrossMass => EntryLineWrapper.GrossMass;

	string IH2Item.GoodsDescription => EntryLineWrapper.GoodsDescription;

	IReadOnlyCollection<IPackage> IH2Item.Packages => EntryLineWrapper.Packages ?? Array.Empty<IPackage>();

	string IH2Item.CusCode => EntryLineWrapper.CusCode;

	string IH2Item.NcCode => EntryLineWrapper.NcCode;

	string IH2Item.TaricCode => EntryLineWrapper.TaricCode;

	IReadOnlyCollection<string> IH2Item.AdditionalCodes => EntryLineWrapper.AdditionalCodes ?? Array.Empty<string>();

	IReadOnlyCollection<string> IH2Item.NationalAdditionalCodes => EntryLineWrapper.NationalAdditionalCodes ?? Array.Empty<string>();

	IReadOnlyCollection<string> IH2Item.Containers => EntryLineWrapper.Containers ?? Array.Empty<string>();

	int IH2Item.TransactionNature => EntryLineWrapper.TransactionNature;

	decimal IH2Item.StatisticalValue => EntryLineWrapper.StatisticalValue;

	#endregion

	#region Implementation

	ICusEntryLineCustomsMessageWrapper EntryLineWrapper => entryLineWrapper.Value;

	#endregion
}
