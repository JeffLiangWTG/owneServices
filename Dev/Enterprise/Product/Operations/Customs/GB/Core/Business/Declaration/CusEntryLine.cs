using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class CusEntryLine : EU.Business.Declaration.CusEntryLine
		, Integration.Customs.GB.ICusEntryLine
		, ICusCodeDataTypeSupporter
	{
		public CusEntryLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		ZBool IsCustomsProcedureDutySuspended => (CusProcedure?.ZZ6_CalculateDuty ?? ZBool.False) && (CusProcedure?.IsGuaranteeConsumed() ?? ZBool.False);

		ZBool IsCustomsProcedureDutyWaived => (!CusProcedure?.ZZ6_CalculateDuty ?? ZBool.False);

		ZBool IsCustomsProcedureVatSuspended => (CusProcedure?.ZZ6_CalculateVAT ?? ZBool.False) && (CusProcedure?.IsGuaranteeConsumed() ?? ZBool.False);

		ZBool IsCustomsProcedureVatWaived => (!CusProcedure?.ZZ6_CalculateVAT ?? ZBool.False);

		public ZBool IsCustomsProcedureDutySuspendedOrWaived => IsCustomsProcedureDutySuspended || IsCustomsProcedureDutyWaived;

		public ZBool IsCustomsProcedureVatSuspendedOrWaived => IsCustomsProcedureVatSuspended || IsCustomsProcedureVatWaived;

		protected override Type GetCusEntryHeaderType() => typeof(CusEntryHeader);

		protected override Customs.Business.CusEntryLineValidation GetNewValidation()
		{
			return Declaration?.ApplicationExtender?.GetCusEntryLineValidation(this) ?? base.GetNewValidation();
		}

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		public new CusEntryHeader Header => (CusEntryHeader)base.Header;

		public new JobComInvoiceLine RandomLine => (JobComInvoiceLine)base.RandomLine;

		protected override bool EffectiveGrossWeightIsApplicableCore => true;

		public bool IsGrossMassMandatoryDueToCpc => InvoiceLines.OfType<JobComInvoiceLine>().Any(il => il.CusProcedure?.HasAttribute(Universal.AttributeNames.Codes.GrossMassMandatory) ?? false);

		protected override OrgAddress ConsignorCore
		{
			get
			{
				if (RandomLine.InvoiceHeader is JobComInvoiceHeader invoice)
				{
					return invoice.SupplierAddress ?? invoice.Supplier?.MainAddress;
				}
				else if (Header.Declaration is JobDeclaration declaration)
				{
					return declaration.SupplierDocumentaryAddress.E2_AddressOverride ? null : declaration.SupplierDocumentaryAddress.Address;
				}
				return null;
			}
		}

		protected override OrgAddress ConsigneeCore
		{
			get
			{
				if (RandomLine.InvoiceHeader is JobComInvoiceHeader invoice)
				{
					return invoice.BuyerAddress ?? invoice.Buyer?.MainAddress;
				}
				else if (Header.Declaration is JobDeclaration declaration)
				{
					return declaration.ImporterDocumentaryAddress.E2_AddressOverride ? null : declaration.ImporterDocumentaryAddress.Address;
				}
				return null;
			}
		}

		public new IEnumerable<AdditionalInfo> AdditionalInfos => Enumerable.Cast<AdditionalInfo>(base.AdditionalInfos);

		protected override IEnumerable<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo> AdditionalInfosCore
		{
			get
			{
				if (Declaration.AreMultipleEntryInstructionsAllowed)
				{
					var addInfoList = GetDistinctAdditionalInfos();
					foreach (var add in addInfoList)
					{
						yield return add;
					}
				}
				else
				{
					foreach (var add in base.AdditionalInfosCore)
					{
						yield return (AdditionalInfo)add;
					}
				}
			}
		}

		IEnumerable<AdditionalInfo> GetDistinctAdditionalInfos()
		{
			var uniqueCol = new UniqueAdditionalInfoCollection();

			foreach (var add in base.AdditionalInfosCore)
			{
				uniqueCol.AddIfNotExist((AdditionalInfo)add);
			}

			foreach (var invoiceLine in InvoiceLines)
			{
				JobComInvoiceLine line = (JobComInvoiceLine)invoiceLine;
				foreach (var add in line.InvoiceHeader.AdditionalInfos)
				{
					uniqueCol.AddIfNotExist((AdditionalInfo)add);
				}
			}

			foreach (var add in Declaration.AdditionalInfos)
			{
				uniqueCol.AddIfNotExist((AdditionalInfo)add);
			}

			foreach (var add in Header.AdditionalInfos)
			{
				uniqueCol.AddIfNotExist(add);
			}

			var entryInstruction = Header.EntryInstruction;
			var ceiAddInfos = entryInstruction?.AdditionalInfos.OfType<AdditionalInfo>().Where(x => x.IsLine).GroupBy(ai => new { ai.CSI_Code, ai.CSI_Description }, (key, ai) => ai.FirstOrDefault()) ?? Array.Empty<AdditionalInfo>();
			foreach (var add in ceiAddInfos)
			{
				uniqueCol.AddIfNotExist(add);
			}

			return uniqueCol.GetUniqueAdditionalInfoList();
		}

		class UniqueAdditionalInfoCollection
		{
			public UniqueAdditionalInfoCollection()
			{
				dict = new Dictionary<string, AdditionalInfo>();
			}

			public void AddIfNotExist(AdditionalInfo addInfo)
			{
				var key = addInfo.KeyToDeterimeUniqueness;
				if (!dict.ContainsKey(key))
				{
					dict.Add(key, addInfo);
				}
			}

			public IEnumerable<AdditionalInfo> GetUniqueAdditionalInfoList() => dict.Values;

			readonly Dictionary<string, AdditionalInfo> dict;
		}

		#region ICusCodeDataTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.FEC, typeof(FECChallenge));
			return result;
		}

		#endregion

		public IEnumerable<CusFiscalReference> FiscalReferences
		{
			get
			{
				var fisRefs = InvoiceLines.OfType<JobComInvoiceLine>().SelectMany(x => x.FiscalReferences);
				return fisRefs.OfType<CusFiscalReference>().GroupBy(fr => new { fr.CFR_Code, fr.CFR_Reference }, (key, fr) => fr.FirstOrDefault());
			}
		}

		public Dictionary<ZString, Money> CDSChargeDeductions => Factory.GetValue(ref cdsChargeDeductionsCache, () =>
		{
			return JobComInvChargeHelper.GetCDSChargeDeductions(CDSLineChargeDeductions);
		});

		#region MOP
		protected override IEnumerable<ZString> GuaranteeDeferredMethodsOfPayment => Declaration.ApplicationExtender.GetGuaranteeDeferredMethodsOfPayment(this);
		protected override IEnumerable<ZString> DeferredMethodsOfPayment => Declaration.ApplicationExtender.GetDeferredMethodsOfPayment(this);
		protected override MoPLevel MoPDetailsLevel => Declaration.ApplicationExtender.GetMoPDetailsLevel();
		#endregion
		CachedProperty<Dictionary<ZString, Money>> cdsChargeDeductionsCache;

		public IEnumerable<BaseJobComInvHeaderCharge> CDSLineChargeDeductions => CDSChargeDeductionsAll.Where(x => Declaration.ApplicationExtender.IsCDSChargeTypeItemLevel(x));

		public IEnumerable<BaseJobComInvHeaderCharge> CDSChargeDeductionsAll => InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.Charges.Cast<BaseJobComInvHeaderCharge>()
																						.Union(x.ApportionedCharges.Cast<BaseJobComInvHeaderCharge>()));

		protected override IEnumerable<IDocSADHLineTaxBoxSupporter> GetTaxBoxSupporterListCore() => Taxes.Cast<IDocSADHLineTaxBoxSupporter>();

		protected override ICusEntryLineFeeCollection<Customs.Business.CusEntryLineFee, Customs.Business.CusEntryLine> GetCusEntryLineFeeCollection()
		{
			return new EU.Business.Declaration.CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>(this, Factory);
		}

		[ChildEditable]
		public new EU.Business.Declaration.CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine> Fees => (EU.Business.Declaration.CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>)base.Fees;

		public new ConfirmedCusEntryLineFeeCollection ConfirmedFees => (ConfirmedCusEntryLineFeeCollection)base.ConfirmedFees;

		public bool IsNorthernIrelandDomestic => RandomLine.IsNorthernIrelandDomestic;

		public bool IsNorthernIrelandImportFromRow => RandomLine.IsNorthernIrelandImportFromRow;

		public bool IsAtRisk => RandomLine.IsAtRisk;

		public bool IsEuTariffToBeUsedForNorthernIreland => RandomLine.IsEuTariffToBeUsedForNorthernIreland;

		protected override EU.Business.EntryLineVatCalculator GetEntryLineVatCalculatorCore() => Declaration?.ApplicationExtender.GetEntryLineVatCalculator(this, x => base.GetEntryLineVatCalculatorCore()) ?? new EU.Business.EntryLineVatCalculator(this);

		protected override ZDecimal GetGSTRate() => Declaration?.ApplicationExtender.GetGSTRate(this, base.GetGSTRate()) ?? base.GetGSTRate();

		protected override ZString GetDutyRateDescription() => Declaration?.ApplicationExtender.GetDutyRateDescription(this, base.GetDutyRateDescription()) ?? base.GetDutyRateDescription();

		protected override string GetFeeCodeFromRateCodeCore(string rateCode) => Declaration?.ApplicationExtender.GetFeeCodeFromRateCode(this, rateCode);

		protected override Customs.Business.ConfirmedCusEntryLineFeeCollection GetConfirmedCusEntryLineFeeCollection() => new ConfirmedCusEntryLineFeeCollection(this);

		protected override ZDecimal GetGSTVATAmountCore()
		{
			return Header.HasAnyConfirmedFeesOnAnyMergedLine ? GetConfirmedGSTVATAmount() : base.GetGSTVATAmountCore();
		}

		ZDecimal GetConfirmedGSTVATAmount()
		{
			return ConfirmedFees.Cast<CusEntryLineFee>()
				.Where(fee => !fee.CF_IsLandedCostOnly && Header.TaxCodeSet.Contains(fee.CF_ChargeType))
				.Select(fee => (decimal)fee.CF_ChargeAmount)
				.Sum();
		}

		protected override ZDecimal GetGSTVATDeferredCore()
		{
			return Header.HasAnyConfirmedFeesOnAnyMergedLine ? GetConfirmedGSTVATDeferred() : base.GetGSTVATDeferredCore();
		}

		ZDecimal GetConfirmedGSTVATDeferred()
		{
			return ConfirmedFees.Cast<CusEntryLineFee>()
				.Where(fee => !fee.CF_IsLandedCostOnly && Header.TaxCodeSet.Contains(fee.CF_ChargeType) && Header.TaxFeePaymentCodeIsDeferred(fee.CF_MethodOfPayment))
				.Select(fee => (decimal)fee.CF_ChargeAmount)
				.Sum();
		}

		protected override ZDecimal GetDutyAmountCore()
		{
			return Header.HasAnyConfirmedFeesOnAnyMergedLine ? GetDutyTotalAmount(ConfirmedFees.AllLineFees.ToList().AsReadOnly()) : base.GetDutyAmountCore();
		}

		protected override void CopySupportingDocumentPropertiesExcludedFromCloning(EU.Business.Declaration.MultiLineAddInfos.SupportingDocument copiedDocument, EU.Business.Declaration.MultiLineAddInfos.SupportingDocument originalDocument)
		{
			base.CopySupportingDocumentPropertiesExcludedFromCloning(copiedDocument, originalDocument);
			copiedDocument.CSI_SystemCreateTimeUtc = originalDocument.CSI_SystemCreateTimeUtc;
		}

		protected override void UpdateSystemCreateTimeForAggregatedDocuments(EU.Business.Declaration.MultiLineAddInfos.SupportingDocument copiedDocument, EU.Business.Declaration.MultiLineAddInfos.SupportingDocument originalDocument)
		{
			base.UpdateSystemCreateTimeForAggregatedDocuments(copiedDocument, originalDocument);
			if (copiedDocument.CSI_SystemCreateTimeUtc.IsEmpty || copiedDocument.CSI_SystemCreateTimeUtc >= originalDocument.CSI_SystemCreateTimeUtc)
			{
				copiedDocument.CSI_SystemCreateTimeUtc = originalDocument.CSI_SystemCreateTimeUtc;
			}
		}
	}
}
