using System;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.AES
{
	public class IE514MessageProvider : EntryHeaderMessageProvider, IIE514Header, IIE514ExportOperation, IDeclarantAndRepresentativeProvider
	{
		public IE514MessageProvider(AESMessageSendingAction sendingAction) : base(sendingAction.EntryHeader)
		{
			this.sendingAction = Argument.NotNull(sendingAction, nameof(sendingAction));
		}
		readonly AESMessageSendingAction sendingAction;

		#region IIE514ExportOperation

		public IIE514ExportOperation ExportOperation => this;
		public string LRN => entryHeader.CH_BGMReference;
		public string MRN => entryHeader.MovementReferenceNumber;
		public DateTime InvalidationRequestDateAndTime => PreparationDateAndTime;
		public string InvalidationReason => sendingAction.Annotation;
		public string CustomsOfficeOfExportReferenceNumber => declaration.JE_CustomsOffice;

		#endregion

		#region IIE514Header

		public IParty Exporter => CachedValueHelper.GetValue(ref exporterCached, () => PartyProvider.New(declaration.SupplierDocumentaryAddress));
		CachedValue<IParty> exporterCached;

		public IParty Declarant => CachedValueHelper.GetValue(ref declarantCached, () => PartyProvider.New(declaration.Declarant));
		CachedValue<IParty> declarantCached;

		public IRepresentative Representative => CachedValueHelper.GetValue(ref representativeCached, () => RepresentativeProvider.New(declaration.Representative, declaration.JE_DeclarantType));
		CachedValue<IRepresentative> representativeCached;

		#endregion

	}
}
