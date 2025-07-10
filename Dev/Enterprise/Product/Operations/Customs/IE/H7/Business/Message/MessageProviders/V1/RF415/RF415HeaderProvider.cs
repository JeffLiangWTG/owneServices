using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.AIS;
using Enterprise.ZArchitecture.Core;
using IMoney = CargoWise.Customs.IE.MessageContracts.AIS.Interfaces.IMoney;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	class RF415HeaderProvider : IRF415Header
	{
		public RF415HeaderProvider(RF415MessageSendingObject sendingObject)
		{
			this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
		}

		readonly RF415MessageSendingObject sendingObject;

		public IRF415HeaderType Header => CachedValueHelper.GetValue(ref header, () => new RF415HeaderTypeProvider(sendingObject));
		CachedValue<RF415HeaderTypeProvider> header;

		public IReadOnlyCollection<IAttachedDocument> AttachedDocuments => attachedDocuments ?? (attachedDocuments = sendingObject.DocumentSendingObjectCollection.Cast<RF415DocumentSendingObject>().Select(x => new AttachedDocumentProvider(x.DocumentType, x.DocumentIdentifier, x.DocumentDate)).ToArray());
		IReadOnlyCollection<IAttachedDocument> attachedDocuments;

		public IRF415PartiesType Parties => CachedValueHelper.GetValue(ref parties, () => new RF415PartiesProvider(sendingObject.Bill.Header));
		CachedValue<RF415PartiesProvider> parties;

		public IRF415DatesPlacesType DatesPlaces => CachedValueHelper.GetValue(ref datesPlaces, () => new RF415DatesPlacesTypeProvider(sendingObject));
		CachedValue<RF415DatesPlacesTypeProvider> datesPlaces;

		public string LegalBasisCodes => sendingObject.LegalBasis;

		public string DescriptionOfGrounds => sendingObject.DescriptionOfGrounds;

		public string BankDetails => sendingObject.BankDetails;

		public IReadOnlyCollection<IRF415GoodsInformationType> GoodsInformation => goodsInformation ?? (goodsInformation = sendingObject.Bill.PackedItems.Select(packedItem => new RF415GoodsInformationProvider(packedItem)).ToArray());
		IReadOnlyCollection<IRF415GoodsInformationType> goodsInformation;

		public ICustomsProcedureType CustomsProcedure => null;

		public IMoney AmountOfDutiesToBeRepaid => CachedValueHelper.GetValue(ref amountOfDutiesToBeRepaid, () => new MoneyProvider(Utilities.Round(sendingObject.Amount, 2), Core.Constants.CurrencyCodes.EuropeanUnion));
		CachedValue<MoneyProvider> amountOfDutiesToBeRepaid;

		public string AdditionalInformation => sendingObject.AdditionalInformation;

		public string MRN => sendingObject.MovementReferenceNumber;
	}
}
