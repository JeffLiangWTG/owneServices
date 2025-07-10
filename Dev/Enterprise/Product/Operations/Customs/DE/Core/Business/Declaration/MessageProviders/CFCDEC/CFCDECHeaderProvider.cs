using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.DE.Business.Declaration.CusEntryHeader;
using CusEntryLine = Enterprise.Customs.DE.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.DE.Business
{
	public sealed class CFCDECHeaderProvider : SingleDecHeaderProvider, ICFCDECHeader
	{
		public CFCDECHeaderProvider(CusEntryHeader entryHeader)
		: base(entryHeader)
		{
		}

		public bool InputTaxDeductionFlag
		{
			get
			{
				var vatClaimBack = Declaration.JE_VATClaimBack;
				return vatClaimBack.IsEmpty ? (bool)Declaration.IsDeclarantEntitledToClaimBackVAT : YesNoList.IsYes(vatClaimBack);
			}
		}

		public string PaymentMethod => Declaration.ZG_MethodOfPayment;

		public string TaxOffice => DeclarationSender?.Header.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.TaxOffice);

		public string ArrivalTransportMeansIdentity => Declaration.JE_TransportMode == TransportTypeList.Codes.FixedTransportInstallations ? null : (string)Declaration.ZG_Box18TransportID;

		public ICustomsValue CustomsValue => CachedValueHelper.GetValue(ref customsValue, () => Declaration.ZG_IsHighValueOvrd ? new CustomsValueProvider(EntryHeader) : null);
		CachedValue<ICustomsValue> customsValue;

		public string ForeignTradeStatisticsTransactionType => RandomInvoiceHeader?.JZ_ValuationCode;

		public IImportParty Consignor => CachedValueHelper.GetValue(ref consignorCached, () => ImportPartyProvider.NewOrNull(Declaration.SupplierDocumentaryAddress.Address));
		CachedValue<IImportParty> consignorCached;

		public IReadOnlyCollection<IImportAdditionalDutyReference> AdditionalDutyReferences => additionalDutyReferences ?? (additionalDutyReferences = EntryInstruction.FiscalReferences.Cast<EU.Business.Declaration.CusFiscalReference>().Select(x => new ImportAdditionalDutyReferenceProvider(x)).ToArray());
		IReadOnlyCollection<IImportAdditionalDutyReference> additionalDutyReferences;

		public IReadOnlyCollection<ICFCDECLine> Lines => lines ?? (lines = EntryHeader.MergedLines.Cast<CusEntryLine>().Select(l => new CFCDECLineProvider(l)).ToArray());
		IReadOnlyCollection<ICFCDECLine> lines;

		public IReadOnlyCollection<IDutyDefermentApproval> DutyDefermentApprovals => dutyDefermentApprovals ??= EntryHeader.GetDutyDefermentAccounts().Select(p => new DutyDefermentApprovalProvider(p)).ToArray();
		IReadOnlyCollection<IDutyDefermentApproval> dutyDefermentApprovals;

		OrgAddress DeclarationSender => CachedValueHelper.GetValue(ref declarationSender, () => Declaration.JE_DeclarantType == RepresentationTypeList.Codes._2Direct ? Declaration.Representative : Declaration.Declarant);
		CachedValue<OrgAddress> declarationSender;
	}
}
