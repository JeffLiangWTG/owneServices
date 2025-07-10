using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.IT.Business;

sealed class LockerForEditingSendingObject
{
	public LockerForEditingSendingObject(JobDeclarationMessageSendingObjectParent sendingObjectParent, JobDeclaration declarationLoadedInMainFactory)
	{
		this.sendingObjectParent = Argument.NotNull(sendingObjectParent, nameof(sendingObjectParent));
		declaration = Argument.NotNull(sendingObjectParent.ParentDeclaration, nameof(sendingObjectParent.ParentDeclaration));
		this.declarationLoadedInMainFactory = Argument.NotNull(declarationLoadedInMainFactory, nameof(declarationLoadedInMainFactory));
	}

	readonly JobDeclarationMessageSendingObjectParent sendingObjectParent;
	readonly JobDeclaration declaration;
	readonly JobDeclaration declarationLoadedInMainFactory;

	public IEnumerable<IOutgoingCustomsMessageGeneratorValuesProvider> GetLockedOutgoingMessageGeneratorProviders()
	{
		return GetLockedOutgoingMessageGeneratorProvidersCore().ToList();
	}

	#region Implementation

	IEnumerable<IOutgoingCustomsMessageGeneratorValuesProvider> GetLockedOutgoingMessageGeneratorProvidersCore()
	{
		var entryPksToBeCompared = declarationLoadedInMainFactory.ActiveEntryHeaders
			.Cast<CusEntryHeader>()
			.Where(x => x.IsEntryLockedForEditing)
			.Select(x => x.PK)
			.ToHashSet();

		var entriesLockedForEditing = declaration.ActiveEntryHeaders
			.Cast<CusEntryHeader>()
			.Where(x => ShouldBeCompared(x) && x.IsEntryLockedForEditing);

		foreach (var entryHeader in entriesLockedForEditing)
		{
			yield return entryHeader.EntryInstruction is null
					? GetEmptyJobDeclarationMessageSendingObject(entryHeader)
					: GetOutgoingMessageGeneratorProvider(entryHeader);
		}

		bool ShouldBeCompared(CusEntryHeader x) => entryPksToBeCompared.Contains(x.PK);
	}

	IOutgoingCustomsMessageGeneratorValuesProvider GetEmptyJobDeclarationMessageSendingObject(CusEntryHeader entryHeader)
	{
		return new EmptyJobDeclarationMessageSendingObject(entryHeader, sendingObjectParent);
	}

	IOutgoingCustomsMessageGeneratorValuesProvider GetOutgoingMessageGeneratorProvider(CusEntryHeader entryHeader)
	{
		return declaration.IsUCC6
			? GetUcc6OutgoingMessageGeneratorProvider(entryHeader)
			: GetSadOutgoingMessageGeneratorProvider(entryHeader);
	}

	IOutgoingCustomsMessageGeneratorValuesProvider GetSadOutgoingMessageGeneratorProvider(CusEntryHeader entryHeader)
	{
		switch (declaration.JE_MessageType)
		{
			case Common.EU.EUJobMessageTypeList.Codes.Import:
				return new IMMessageSendingObject(entryHeader, sendingObjectParent, isForDeterminingMessageChangedStatus: true);

			case Common.EU.EUJobMessageTypeList.Codes.Export:
				return new ETMessageSendingObject(entryHeader, sendingObjectParent, isForDeterminingMessageChangedStatus: true);

			default:
				return null;
		}
	}

	IOutgoingCustomsMessageGeneratorValuesProvider GetUcc6OutgoingMessageGeneratorProvider(CusEntryHeader entryHeader)
	{
		return new Ucc6XmlJobDeclarationMessageSendingObjectFactory(entryHeader, sendingObjectParent)
			.TryGetNewMessageSendingObject() ?? GetEmptyJobDeclarationMessageSendingObject(entryHeader);
	}

	#endregion
}
