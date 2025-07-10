using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class RF415HeaderProvider : EntryHeaderMessageProvider, IRF415Header
	{
		readonly RefundApplicationMessageSendingAction sendingAction;

		public RF415HeaderProvider(RefundApplicationMessageSendingAction sendingAction) : base(sendingAction.EntryHeader)
		{
			this.sendingAction = sendingAction;
		}

		public IRF415HeaderType Header => CachedValueHelper.GetValue(ref header, () => new RF415HeaderTypeProvider(sendingAction));
		CachedValue<RF415HeaderTypeProvider> header;

		public IReadOnlyCollection<IAttachedDocument> AttachedDocuments => attachedDocuments ?? (attachedDocuments = sendingAction.DocumentSendingObjectCollection.Cast<RefundApplicationDocumentSendingObject>().Select(x => new AttachedDocumentProvider(x.DocumentType, x.DocumentIdentifier, x.DocumentDate)).ToArray());
		IReadOnlyCollection<IAttachedDocument> attachedDocuments;

		public IParties Parties => CachedValueHelper.GetValue(ref parties, () => new PartiesProvider(declaration));
		CachedValue<PartiesProvider> parties;

		public IDatesPlaces DatesPlaces => CachedValueHelper.GetValue(ref datesPlaces, () => new DatesPlacesProvider(sendingAction, instruction));
		CachedValue<DatesPlacesProvider> datesPlaces;

		public string MRN => entryHeader.MovementReferenceNumber;

		public string LegalBasisCodes => sendingAction.LegalBasis;

		public string DescriptionOfGrounds => sendingAction.DescriptionOfGrounds;

		public string BankDetails => sendingAction.BankDetails;

		public IReadOnlyCollection<IRF415GoodsInformation> GoodsInformation => goodsInformation ?? (goodsInformation = entryHeader.MergedLines.Select(x => new RF415GoodsInformationProvider(x, entryHeaderWrapper)).ToArray());
		IReadOnlyCollection<IRF415GoodsInformation> goodsInformation;

		public string CustomsProcedure => instruction.CEI_Style;

		public IMoney AmountOfDutiesToBeRepaid => new MoneyProvider(sendingAction.Amount, Core.Constants.CurrencyCodes.EuropeanUnion);

		public string AdditionalInformation => sendingAction.AdditionalInformation;

		public IFallbackProcedure FallbackProcedure => null;
	}
}
