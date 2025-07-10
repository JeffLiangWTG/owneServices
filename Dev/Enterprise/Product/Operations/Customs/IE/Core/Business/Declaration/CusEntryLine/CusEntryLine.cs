using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.IE.Business.Constants;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class CusEntryLine : EU.Business.Declaration.CusEntryLine
		, Integration.Customs.IE.ICusEntryLine
		, IAdditionalInfoCollectionProvider
	{
		public CusEntryLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new CusEntryHeader Header => (CusEntryHeader)base.Header;

		public new JobComInvoiceLine RandomLine => (JobComInvoiceLine)base.RandomLine;

		public JobComInvoiceLine RandomMainPackLineOrRandomLine => Factory.GetValue(ref randomMainPackLineOrRandomLineCached, () => InvoiceLines.Cast<JobComInvoiceLine>().Where(x => x.ZG_IsMainPack).OrderBy(x => x.JI_SystemCreateTimeUtc).ThenBy(x => x.InvoiceAndLineReference).FirstOrDefault() ?? RandomLine);
		CachedProperty<JobComInvoiceLine> randomMainPackLineOrRandomLineCached;

		protected override CusEntryLineValidation GetNewValidation() => Declaration?.IsExport ?? false ? new ExportCusEntryLineValidation(this) : base.GetNewValidation();

		protected override System.Type GetCusEntryHeaderType() => typeof(CusEntryHeader);

		protected override ICusEntryLineFeeCollection<Customs.Business.CusEntryLineFee, Customs.Business.CusEntryLine> GetCusEntryLineFeeCollection()
		{
			return new CusEntryLineFeeCollection(this, Factory);
		}

		public new CusEntryLineFeeCollection Fees => (CusEntryLineFeeCollection)base.Fees;

		public new IEnumerable<PreviousDocument> PreviousDocuments => base.PreviousDocuments.Cast<PreviousDocument>();
		protected override IEnumerable<EU.Business.Declaration.MultiLineAddInfos.PreviousDocument> GetPreviousDocumentsToProcess()
		{
			foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
			{
				foreach (PreviousDocument document in invoiceLine.PreviousDocuments)
				{
					yield return document;
				}
			}
		}

		protected override bool EffectiveGrossWeightIsApplicableCore => true;

		public new IEnumerable<AdditionalInfo> AdditionalInfos => base.AdditionalInfos.Cast<AdditionalInfo>();
		protected override IEnumerable<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo> AdditionalInfosCore
		{
			get
			{
				var additionalInfos = InvoiceLines.OfType<JobComInvoiceLine>().SelectMany(x => x.AdditionalInfos.Cast<AdditionalInfo>());

				return additionalInfos.GroupBy((AdditionalInfo ai) => new { ai.CSI_Code, ai.CSI_Description }, (key, ai) => ai.FirstOrDefault());
			}
		}

		public new IEnumerable<SupportingDocument> SupportingDocuments => base.SupportingDocuments.Cast<SupportingDocument>();
		protected override IEnumerable<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument> GetSupportingDocumentsToProcess()
		{
			foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
			{
				foreach (SupportingDocument document in invoiceLine.SupportingDocuments)
				{
					yield return document;
				}
			}
		}

		public IEnumerable<BaseJobComInvHeaderCharge> EntryLineRelatedChargeDeductions => Factory.GetValue(ref entryLineRelatedChargeDeductionsCached, () =>
		{
			return InvoiceLines.Cast<JobComInvoiceLine>().SelectMany((JobComInvoiceLine x) => x.Charges.Cast<BaseJobComInvHeaderCharge>().Union(x.ApportionedCharges.Cast<BaseJobComInvHeaderCharge>()));
		});
		CachedProperty<IEnumerable<BaseJobComInvHeaderCharge>> entryLineRelatedChargeDeductionsCached;

		public RefundDutyCollection RefundDuties
		{
			get
			{
				if (refundDuties == null)
				{
					refundDuties = new RefundDutyCollection(this);
					refundDuties.Load();
				}
				return refundDuties;
			}
		}
		RefundDutyCollection refundDuties;

		public IEnumerable<CusFiscalReference> FiscalReferences => Factory.GetValue(ref fiscalReferencesCached, () =>
		{
			var source = InvoiceLines.OfType<JobComInvoiceLine>().SelectMany((JobComInvoiceLine x) => x.FiscalReferences);
			return source.OfType<CusFiscalReference>().GroupBy((CusFiscalReference fr) => new { fr.CFR_Code, fr.CFR_Reference }, (key, fr) => fr.FirstOrDefault());
		});
		CachedProperty<IEnumerable<CusFiscalReference>> fiscalReferencesCached;

		public Dictionary<ZString, Money> AISChargeDeductions => Factory.GetValue(ref aisChargeDeductionsCache, () =>
		{
			return JobComInvoiceChargeHelper.GetChargeDeductions(EntryLineRelatedChargeDeductions);
		});
		CachedProperty<Dictionary<ZString, Money>> aisChargeDeductionsCache;

		protected override ZDecimal GetGSTVATDeferredCore()
		{
			if (UseDeferredVATInsteadOfVATAmount)
			{
				var result = ZDecimal.Zero;
				foreach (var vatFee in Fees.OfType<CusEntryLineFee>().Where(f =>
				(f.CF_ChargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat
				|| f.CF_ChargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.VatOnAdditionalDutiesForNorthernIreland
				|| f.CF_ChargeType == UniversalReferenceConstants.IERefCusRateCodes.DeferredVAT
				|| f.CF_ChargeType == UniversalReferenceConstants.IERefCusRateCodes.SpecialArrangementsVAT) && !f.CF_IsLandedCostOnly))
				{
					result += vatFee.CF_ChargeAmount;
				}
				return result;
			}
			return base.GetGSTVATDeferredCore();
		}

		protected override ZDecimal GetGSTVATAmountCore()
		{
			if (UseDeferredVATInsteadOfVATAmount)
			{
				return 0m;
			}
			return base.GetGSTVATAmountCore();
		}

		IEnumerable<JobComInvoiceHeader> InvoiceHeaders => Factory.GetValue(ref invoiceHeadersCached, () =>
		{
			var invoiceHeaders = new Dictionary<ZGuid, JobComInvoiceHeader>();
			foreach (JobComInvoiceLine line in InvoiceLines)
			{
				var header = line.InvoiceHeader;
				if (header?.PK is ZGuid key)
				{
					invoiceHeaders.GetOrAdd(key, () => header);
				}
			}
			return invoiceHeaders.Values;
		});
		CachedProperty<IEnumerable<JobComInvoiceHeader>> invoiceHeadersCached;

		IEnumerable<SupportingDocument> InvoiceHeaderSupportingDocuments => Factory.GetValue(ref invoiceHeaderSupportingDocumentsCached, () =>
		{
			return InvoiceHeaders.SelectMany(header => header.SupportingDocuments.Cast<SupportingDocument>());
		});
		CachedProperty<IEnumerable<SupportingDocument>> invoiceHeaderSupportingDocumentsCached;

		bool UseDeferredVATInsteadOfVATAmount => Factory.GetValue(ref useDeferredVATInsteadOfVATAmountCached, GetShouldUseDeferredVATInsteadOfVATAmount);
		CachedProperty<bool> useDeferredVATInsteadOfVATAmountCached;

		bool GetShouldUseDeferredVATInsteadOfVATAmount() => RandomLine.EntryInstruction is CusEntryInstruction instruction &&
			(instruction.IsH1 || instruction.IsH5) && instruction.IsImport &&
			 (RandomLine.EntryInstruction.SupportingDocuments.Cast<SupportingDocument>().Any(doc => supportingDocumentTypeUsedDeferredVATInsteadOfVATAmount.Contains(doc.CSI_Code)) ||
			InvoiceHeaderSupportingDocuments.Any(doc => supportingDocumentTypeUsedDeferredVATInsteadOfVATAmount.Contains(doc.CSI_Code)));

		readonly ZString[] supportingDocumentTypeUsedDeferredVATInsteadOfVATAmount = new ZString[]
		{
			SupportingDocumentCodes._1A05, SupportingDocumentCodes._1A06
		};

		CachedProperty<bool> isSecuritiesForEndUse;
		public bool IsSecuritiesForEndUse => Factory.GetValue(ref isSecuritiesForEndUse, GetIsSecuritiesForEndUse);
		bool GetIsSecuritiesForEndUse()
		{
			var randomLine = RandomLine;
			var instruction = randomLine.EntryInstruction;
			var invoiceHeader = randomLine.InvoiceHeader;
			return
				instruction != null
				&& randomLine.IsSecuritiesForEndUse
				&& (
					instruction.HasAuthorisationForSpecialProcedure
					|| invoiceHeader.AdditionalInfos.HasAuthorisationForSpecialProcedure()
					|| randomLine.AdditionalInfos.HasAuthorisationForSpecialProcedure()
				);
		}
	}
}

