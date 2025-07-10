using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public sealed class SCIDECHeaderProvider : SingleDecHeaderProvider, ISCIDECHeader
	{
		public SCIDECHeaderProvider(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		public string SimplifiedRequestAuthorisationFlag => EntryInstruction.CEI_SimplifiedGrantAuthorization;

		public IImportParty Consignor => CachedValueHelper.GetValue(ref consignorCached, () => ImportPartyProvider.NewOrNull(Declaration.SupplierDocumentaryAddress.Address));
		CachedValue<IImportParty> consignorCached;

		public int InwardProcessingCompletionLimitDate => EntryInstruction.CEI_CompletionDuration;

		public string InwardProcessingCriteriaType => EntryInstruction.CEI_CriteriaType;

		public string InwardProcessingAdditionalInformation => EntryInstruction.CEI_InwardProcessingAdditionalInformation;

		public string IntendedActivityDetailDescription => EntryInstruction.CEI_InwardProcessingDescription;

		public IImportPartyIdAddress MainAccounting => CachedValueHelper.GetValue(ref mainAccountingCached, () => ImportPartyIdAddressProvider.NewOrNull(EntryInstruction.MainAccountingAddress, false));
		CachedValue<IImportPartyIdAddress> mainAccountingCached;

		public IImportPartyIdAddress FirstInwardProcessingPlace => CachedValueHelper.GetValue(ref firstInwardProcessingPlaceCached, () =>
		{
			return ImportPartyIdAddressProvider.NewOrNull(EntryInstruction.InwardProcessingPlaces.Cast<InwardProcessingPlace>().OrderBy(x => x.E2_AddressSequence).FirstOrDefault(), false);
		});
		CachedValue<IImportPartyIdAddress> firstInwardProcessingPlaceCached;

		public IReadOnlyCollection<IImportPartyIdAddress> AdditionalInwardProcessingPlace => additionalInwardProcessingPlace ??= EntryInstruction.InwardProcessingPlaces
			.Cast<InwardProcessingPlace>()
			.OrderBy(x => x.E2_AddressSequence)
			.Skip(1)
			.Select(x => ImportPartyIdAddressProvider.NewOrNull(x, false))
			.ToArray();
		IReadOnlyCollection<IImportPartyIdAddress> additionalInwardProcessingPlace;

		public IReadOnlyCollection<string> CompletionCustomsOfficeReferenceNumbers => completionCustomsOfficeReferenceNumbers ??
			(completionCustomsOfficeReferenceNumbers = EntryInstruction.CompletionCustomsOffices
				.Cast<CompletionCustomsOffice>()
				.Select(x => x.CY_Data.ToString())
				.ToArray());
		IReadOnlyCollection<string> completionCustomsOfficeReferenceNumbers;

		public string ForeignTradeStatisticsTransactionType => RandomInvoiceHeader?.JZ_ValuationCode;

		public IReadOnlyCollection<ISCIDECLine> Lines => lines ?? (lines = EntryHeader.MergedLines.Select(l => new SCIDECLineProvider(l)).ToArray());
		IReadOnlyCollection<ISCIDECLine> lines;

		string IImportDecHeader.ProcedureAuthorisation => CachedValueHelper.GetValue(ref procedureAuthorisation, () => EntryInstruction.CEI_AuthorisationNumber);
		CachedValue<string> procedureAuthorisation;
	}
}
