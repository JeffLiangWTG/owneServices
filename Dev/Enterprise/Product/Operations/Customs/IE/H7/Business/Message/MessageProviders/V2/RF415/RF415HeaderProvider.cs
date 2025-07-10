using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.H7.Business
{
	class RF415HeaderProvider : IRF415Header
	{
		public RF415HeaderProvider(RF415MessageSendingObject messageSendingObject)
		{
			MessageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
			PreparationDateAndTime = DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(ZDateTime.UtcNow, true);
		}

		RF415MessageSendingObject MessageSendingObject { get; }

		public IRF415HeaderType Header => CachedValueHelper.GetValue(ref header, () => new RF415HeaderTypeProvider(MessageSendingObject));
		CachedValue<RF415HeaderTypeProvider> header;

		public IReadOnlyCollection<IAttachedDocument> AttachedDocuments => attachedDocuments ?? (attachedDocuments = MessageSendingObject.DocumentSendingObjectCollection.Cast<RF415DocumentSendingObject>().Select(x => new RF415AttachedDocumentProvider(x.DocumentType, x.DocumentIdentifier, x.DocumentDate)).ToArray());
		IReadOnlyCollection<IAttachedDocument> attachedDocuments;

		public IParties Parties => CachedValueHelper.GetValue(ref parties, () => new PartiesProvider(MessageSendingObject.Bill.Header));
		CachedValue<PartiesProvider> parties;

		public IDatesPlaces DatesPlaces => CachedValueHelper.GetValue(ref datePlaces, () => new DatesPlacesProvider(MessageSendingObject, PreparationDateAndTime));
		CachedValue<DatesPlacesProvider> datePlaces;

		public string MRN => MessageSendingObject.Bill.CustomsEntryNumber;

		public string LegalBasisCodes => MessageSendingObject.LegalBasis;

		public string DescriptionOfGrounds => MessageSendingObject.DescriptionOfGrounds;

		public string BankDetails => MessageSendingObject.BankDetails;

		public IReadOnlyCollection<IRF415GoodsInformation> GoodsInformation => goodsInformation ?? (goodsInformation = MessageSendingObject.Bill.PackedItems.Select(packedItem => new RF415GoodsInformationProvider(packedItem)).ToArray());
		IReadOnlyCollection<IRF415GoodsInformation> goodsInformation;

		public string CustomsProcedure => null;

		public IMoney AmountOfDutiesToBeRepaid => CachedValueHelper.GetValue(ref amountOfDutiesToBeRepaid, () => new RF415AmountOfDutiesToBeRepaidProvider(MessageSendingObject));
		CachedValue<RF415AmountOfDutiesToBeRepaidProvider> amountOfDutiesToBeRepaid;

		public string AdditionalInformation => MessageSendingObject.AdditionalInformation;

		public IFallbackProcedure FallbackProcedure => null;

		public DateTime PreparationDateAndTime { get; }
	}
}
