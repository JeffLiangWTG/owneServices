using System;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM414DeclarationTypeProvider : IIM414DeclarationType
	{
		readonly CusEntryHeader entryHeader;
		readonly DateTime preparationDateAndTime;
		readonly AISUCC5MessageSendingAction sendingAction;
		readonly JobDeclaration declaration;

		public IM414DeclarationTypeProvider(EntryHeaderWrapper entryHeaderWrapper, DateTime preparationDateAndTime, AISUCC5MessageSendingAction sendingAction)
		{
			entryHeader = entryHeaderWrapper.EntryHeader;
			this.preparationDateAndTime = preparationDateAndTime;
			this.sendingAction = sendingAction;
			declaration = entryHeader.Declaration;
		}

		public string MRN => entryHeader.MovementReferenceNumber;

		public DateTime DateOfInvalidationRequest => preparationDateAndTime;

		public string InvalidationReason => sendingAction.Annotation;

		public IDeclarationTypeCustomsOffices CustomsOffices => CachedValueHelper.GetValue(ref customsOfficesCached, () => new DeclarationTypeCustomsOfficesProvider(declaration));
		CachedValue<IDeclarationTypeCustomsOffices> customsOfficesCached;

		public IIM414DeclarationTypeParties Parties => CachedValueHelper.GetValue(ref partiesCached, () => new IM414DeclarationTypePartiesProvider(declaration));
		CachedValue<IIM414DeclarationTypeParties> partiesCached;
	}
}
