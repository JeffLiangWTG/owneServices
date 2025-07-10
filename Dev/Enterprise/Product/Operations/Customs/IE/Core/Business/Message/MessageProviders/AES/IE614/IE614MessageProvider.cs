using System;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.AES
{
	public class IE614MessageProvider : EntryHeaderMessageProvider, IIE614Header, IIE614ExportOperation, IDeclarantAndRepresentativeProvider
	{
		public IE614MessageProvider(AESMessageSendingAction sendingAction) : base(sendingAction.EntryHeader)
		{
			this.sendingAction = Argument.NotNull(sendingAction, nameof(sendingAction));
		}
		readonly AESMessageSendingAction sendingAction;

		#region IIE614ExportOperation
		public IIE614ExportOperation ExportOperation => this;
		public string InvalidationReason => sendingAction.Annotation;
		public DateTime InvalidationRequestDateAndTime => PreparationDateAndTime;
		public string MRN => entryHeader.MovementReferenceNumber;
		public string CustomsOfficeOfExitReferenceNumber => declaration.OfficeOfExitCustomsOffice;

		#endregion

		#region IDeclarantAndRepresentativeProvider

		public IParty Declarant => CachedValueHelper.GetValue(ref declarantCached, () => PartyProvider.New(declaration.Declarant));
		CachedValue<IParty> declarantCached;

		public IRepresentative Representative => CachedValueHelper.GetValue(ref representativeCached, () => RepresentativeProvider.New(declaration.Representative, declaration.JE_DeclarantType));
		CachedValue<IRepresentative> representativeCached;

		#endregion
	}
}
