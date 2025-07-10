#if NETFRAMEWORK
using CargoWise.Common;
#endif
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using static System.FormattableString;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusEntryLine : TypeSafeCusEntryLine, Integration.Customs.EU.ICusEntryLine, ICanBeImportOrExport
	{
		public CusEntryLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusEntryLine.Schema
		{
			public const string ProcedureCodeWithoutConcession = nameof(CusEntryLine.ProcedureCodeWithoutConcession);
			public const string CountryOfOriginCode = nameof(CusEntryLine.CountryOfOriginCode);
			public const string PreferenceCode = nameof(CusEntryLine.PreferenceCode);
		}

		public new static readonly CusEntryLineTypeDecider TypeDecider = new CusEntryLineTypeDecider();

		public override ZGuid CL_CH
		{
			get => base.CL_CH;
			set
			{
				var oldValue = CL_CH;
				base.CL_CH = value;
				if (oldValue != CL_CH)
				{
					Fees.MarkAsNeedingValidation();
				}
			}
		}

		public ZDecimal CL_Calc_StatisticalBasisExcludingSTACharge
		{
			get { return InvoiceLines?.Cast<JobComInvoiceLine>().Sum(x => x.JI_Calc_StatisticalBasisExcludingSTACharge) ?? 0m; }
		}

		public ZPropertyInfo CL_Calc_StatisticalBasisExcludingSTAChargeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(CL_Calc_StatisticalBasisExcludingSTACharge)); }
		}

		/// <summary>
		/// Flag to show whether or not we're showing the EffectiveGrossWeight.  If this is TRUE, it means 
		/// that the true EffectiveGrossWeight will be returned from YourObject.EffectiveGrossWeight.
		/// If this is FALSE, then it means that we are HIDING the EffectiveGrossWeight, and that YourObject.EffectiveGrossWeight
		/// will return ZWeight.Empty.  We hide based on the rules of SAD-H, version 1.3a, pages 19 and 45.
		/// We use this flag to let the template have an easy easy of seeing whether to render the zero or whether to show it as blank.
		/// </summary>
		/// <returns></returns>
		public bool EffectiveGrossWeightIsApplicable
		{
			get
			{
				if (Declaration == null)
				{
					return true; // we have not declaration so there can be no reason for the gross weight to be hidden.
				}

				return EffectiveGrossWeightIsApplicableCore;
			}
		}
		protected virtual bool EffectiveGrossWeightIsApplicableCore => Declaration.WarehouseAddress != null;

		public ZWeight GrossWeight => Factory.GetValue(ref fGrossWeight, () =>
		{
			var weight = ZWeight.Empty;
			foreach (JobComInvoiceLine item in InvoiceLines)
			{
				weight += new ZWeight(item.JI_Weight, item.JI_WeightUQ);
			}
			return weight;
		});
		CachedProperty<ZWeight> fGrossWeight;

		public ZString ProcedureCodeWithoutConcession => ProcedureCode.Left(4);

		protected override ZString GetDutyRateDescription()
		{
			return GetDutyLinesAndFormatWithThisRunner(Fees, (entryLineFee) => entryLineFee.CF_ChargeType + ":" + entryLineFee.CF_Rate.Round(2).ToStringTrimZeros(2) + "%");
		}

		ZString GetDutyLinesAndFormatWithThisRunner(ICusEntryLineFeeCollection<Customs.Business.CusEntryLineFee, Customs.Business.CusEntryLine> fees, FormatOneDutyLineRunner runner)
		{
			var feeLines = new List<ZString>();
			var groupedFees = fees.Cast<CusEntryLineFee>().GroupBy(x => x.CF_ChargeType).Where(group => group.Count() == 1).Select(group => new
			{
				ChargeType = group.Key,
				Fees = group.ToList()
			});

			foreach (var fee in (from CusEntryLineFee f in groupedFees.SelectMany(x => x.Fees) where f.CF_ChargeType != UniversalReferenceConstants.RefCusRateCodes.Vat select f))
			{
				feeLines.Add(runner(fee));
			}
			var array = feeLines.ToArray();
			Array.Sort(array);
			return ZString.Join(System.Environment.NewLine, array);
		}

		internal delegate ZString FormatOneDutyLineRunner(CusEntryLineFee fee);

		protected override ZDecimal GetGSTRate()
		{
			var groupedFees = Fees.Cast<CusEntryLineFee>().GroupBy(x => x.CF_ChargeType).Where(group => group.Count() == 1).Select(group => new
			{
				ChargeType = group.Key,
				Fees = group.ToList()
			});

			var fee = (from CusEntryLineFee f in groupedFees.SelectMany(x => x.Fees) where f.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.Vat || f.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.VatOnAdditionalDutiesForNorthernIreland select f).FirstOrDefault();
			return fee != null ? fee.CF_Rate.Round(2) : ZDecimal.Zero;
		}

		public override ZWeight EffectiveGrossWeight => EffectiveGrossWeightIsApplicable ? base.EffectiveGrossWeight : ZWeight.Empty;

		public ZWeight GrossWeightForCommericalPurpose
		{
			get
			{
				return base.EffectiveGrossWeight;
			}
		}

		public OrgAddress Consignor => Factory.GetValue(ref consignorCached, () => ConsignorCore);
		CachedProperty<OrgAddress> consignorCached;

		protected virtual OrgAddress ConsignorCore => RandomLine.ExporterAddress;

		public OrgAddress Consignee => Factory.GetValue(ref consigneeCached, () => ConsigneeCore);
		CachedProperty<OrgAddress> consigneeCached;

		protected virtual OrgAddress ConsigneeCore => RandomLine.ConsigneeAddress;

		/// <summary>
		/// This collection is used to display readonly confirmed fees in grid without validation
		/// </summary>
		public CusEntryLineConfirmedFeeWrapperCollection ConfirmedFeesReadOnly => Factory.GetValue(ref confirmedFeesReadOnly, () => GetConfirmedFeesReadOnlyCore());
		CachedProperty<CusEntryLineConfirmedFeeWrapperCollection> confirmedFeesReadOnly;
		protected virtual CusEntryLineConfirmedFeeWrapperCollection GetConfirmedFeesReadOnlyCore() => new CusEntryLineConfirmedFeeWrapperCollection(this);

		#region CusAuthorizationUsages

		public IEnumerable<CusAuthorizationUsage> CusAuthorizationUsages => Factory.GetCachedAggregatedData(ref cusAuthorizationUsagesCached, Declaration.CreateEntryCreationStrategy().GetCusAuthorizationUsageKeys, () => InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.CusAuthorizationUsages));
		CachedProperty<IEnumerable<CusAuthorizationUsage>> cusAuthorizationUsagesCached;

		#endregion

		#region CusSupplyChainActorReferences

		public IEnumerable<CusSupplyChainActorReference> CusSupplyChainActorReferences => Factory.GetCachedAggregatedData(ref cusSupplyChainActorReferencesCached, Declaration.CreateEntryCreationStrategy().GetCusSupplyChainActorReferenceKeys, () => InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.CusSupplyChainActorReferences.Cast<CusSupplyChainActorReference>()));
		CachedProperty<IEnumerable<CusSupplyChainActorReference>> cusSupplyChainActorReferencesCached;

		#endregion

		#region AdditionalInfos

		public IEnumerable<AdditionalInfo> AdditionalInfos => Factory.GetValue(ref additionalInfosCached, () => AdditionalInfosCore);
		CachedProperty<IEnumerable<AdditionalInfo>> additionalInfosCached;

		protected virtual IEnumerable<AdditionalInfo> AdditionalInfosCore
		{
			get
			{
				var addInfosToReturn = InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.EffectiveAdditionalInfos());
				return addInfosToReturn.DistinctBy(GetAdditionalInfoDistinctKey);
			}
		}

		protected virtual object GetAdditionalInfoDistinctKey(AdditionalInfo ai)
		{
			return new
			{
				ai.CSI_Code,
				ai.CSI_Description
			};
		}

		#endregion

		// im suspecting that the helper routines in the following 3 regions might be able to be merged into a "generic" form?
		#region PreviousDocuments
		public IEnumerable<PreviousDocument> PreviousDocuments => Factory.GetCachedAggregatedData(ref previousDocumentsCached, () => Declaration?.PreviousDocumentKeys.ToArray() ?? Array.Empty<string>(), GetPreviousDocumentsToProcess);
		CachedProperty<IEnumerable<PreviousDocument>> previousDocumentsCached;

		protected virtual IEnumerable<PreviousDocument> GetPreviousDocumentsToProcess()
		{
			var supportsPreviousDocumentsAtEntryHeaderLevel = Header.SupportsPreviousDocumentsAtEntryHeaderLevel;
			var declaration = Declaration;
			foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
			{
				if (invoiceLine.InvoiceHeader is JobComInvoiceHeader invoice)
				{
					if (!supportsPreviousDocumentsAtEntryHeaderLevel && invoice.GroupHeader != null)
					{
						foreach (PreviousDocument document in declaration.PreviousDocuments)
						{
							yield return document;
						}
					}
					foreach (PreviousDocument document in invoice.PreviousDocuments)
					{
						yield return document;
					}
				}
				foreach (PreviousDocument document in invoiceLine.PreviousDocuments)
				{
					yield return document;
				}
			}
		}

		#endregion

		#region SupportingDocuments
		void SuppDocsDictionaryBuildingHelper(string[] keys, IEnumerable<SupportingDocument> supportingDocumentsToAdd, Dictionary<MergeKey, SupportingDocument> dictionary)
		{
			foreach (var supportingDocument in supportingDocumentsToAdd)
			{
				var mergeKey = GetSupportingDocumentMergeKey(keys, supportingDocument);

				if (dictionary.ContainsKey(mergeKey))
				{
					// Aggregate values
					dictionary[mergeKey].CSI_Quantity += supportingDocument.CSI_Quantity;
					dictionary[mergeKey].CSI_Quantity2 += supportingDocument.CSI_Quantity2;
					dictionary[mergeKey].CSI_Quantity3 += supportingDocument.CSI_Quantity3;
					dictionary[mergeKey].CSI_Value += supportingDocument.CSI_Value;
					UpdateSystemCreateTimeForAggregatedDocuments(dictionary[mergeKey], supportingDocument);
				}
				else
				{
					dictionary[mergeKey] = (SupportingDocument)supportingDocument.Clone();
					CopySupportingDocumentPropertiesExcludedFromCloning(dictionary[mergeKey], supportingDocument);
				}
			}
		}

		protected virtual void UpdateSystemCreateTimeForAggregatedDocuments(SupportingDocument copiedDocument, SupportingDocument originalDocument)
		{
		}

		static MergeKey GetSupportingDocumentMergeKey(string[] keys, SupportingDocument supportingDocument)
		{
			var mergeKey = new MergeKey();
			foreach (var key in keys)
			{
				mergeKey.Add((IZType)supportingDocument[key]);
			}
			return mergeKey;
		}

		protected virtual void CopySupportingDocumentPropertiesExcludedFromCloning(SupportingDocument copiedDocument, SupportingDocument originalDocument)
		{
			copiedDocument.CSI_ParentID = originalDocument.CSI_ParentID;
			copiedDocument.CSI_ParentTableCode = originalDocument.CSI_ParentTableCode;
			copiedDocument.IsUsedForTemporaryAggregation = true;
		}

		public IEnumerable<SupportingDocument> SupportingDocuments => Factory.GetValue(ref supportingDocumentsCached, GetSupportingDocumentsToProcess);
		CachedProperty<IEnumerable<SupportingDocument>> supportingDocumentsCached;

		protected virtual IEnumerable<SupportingDocument> GetSupportingDocumentsToProcess()
		{
			var declaration = Declaration;
			var keys = declaration.CreateEntryCreationStrategy().GetSupportingDocumentKeys();
			var suppDocsToReturn = new Dictionary<MergeKey, SupportingDocument>();

			foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
			{
				SuppDocsDictionaryBuildingHelper(keys, invoiceLine.InvoiceHeader.EffectiveSupportingDocumentsForLine().ToArray(), suppDocsToReturn);
				SuppDocsDictionaryBuildingHelper(keys, invoiceLine.EffectiveSupportingDocuments().ToArray(), suppDocsToReturn);
			}

			if (Configuration?.ShouldFilterSupportingDocumentsByMergeKeys() ?? true)
			{
				var headerSupportingDocumentMergeKeys = Header.SupportingDocuments.Select(x => GetSupportingDocumentMergeKey(keys, x)).ToHashSet();
				return suppDocsToReturn.Where(x => !headerSupportingDocumentMergeKeys.Contains(x.Key)).Select(x => x.Value);
			}

			return suppDocsToReturn.Values;
		}

		public ReadOnlySupportingDocumentCollection ReadOnlySupportingDocuments
		{
			get
			{
				if (readOnlySupportingDocuments == null)
				{
					readOnlySupportingDocuments = GetNewReadOnlySupportingDocuments();
				}
				if (!IsDeleted && !readOnlySupportingDocuments.IsLoaded && !IsInProcessOfMerging)
				{
					readOnlySupportingDocuments.LoadNew();
				}
				return readOnlySupportingDocuments;
			}
		}
		ReadOnlySupportingDocumentCollection readOnlySupportingDocuments;

		protected virtual ReadOnlySupportingDocumentCollection GetNewReadOnlySupportingDocuments() => new ReadOnlySupportingDocumentCollection(this);

		public void ResetReadOnlySupportingDocuments()
		{
			readOnlySupportingDocuments = null;
		}

		#endregion

		protected override Customs.Business.TariffFormatter GetTariffFormatter() => TariffFormatter.New(Header?.Declaration?.CountryCode);

		public ZDecimal StatisticalValue => Factory.GetValue(ref statisticalValueCached, () => InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.ZG_StatisticalValue));
		CachedProperty<ZDecimal> statisticalValueCached;

		public ZDecimal ThirdQuantity => Factory.GetValue(ref customsThirdQuantityCached, () => InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.JI_CustomsThirdQuantity));
		CachedProperty<ZDecimal> customsThirdQuantityCached;

		public ZString ThirdUQ
		{
			get
			{
				return RandomLine.JI_CustomsThirdUnitQty; // we know this is ok and common to all lines, otherwise the above (ThrirdQuantity) would have barfed
			}
		}

		public ZDecimal SupplementaryQuantity => Factory.GetValue(ref supplementaryQuantityCached, () =>
		{
			var result = ZDecimal.Zero;
			var defaultSupplementaryUQ = InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault()?.JI_CustomsSecondUnitQty ?? ZString.Empty;
			var defaultSupplementaryUQIsNotEmpty = !defaultSupplementaryUQ.IsEmpty;
			foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
			{
				if (defaultSupplementaryUQIsNotEmpty)
				{
					if (invoiceLine.JI_CustomsSecondUnitQty == defaultSupplementaryUQ)
					{
						result += invoiceLine.JI_CustomsSecondQuantity;
					}
					else
					{
						// #################	If you remove this throw, check the logic of SupplementaryUQ too please.		########################################################################
						throw new InvalidOperationException("Supplementary quantity cannot be aggregated as multiple units of quantity exist. Supplementary UQ should be in the merge key");
						// #################	If you remove this throw, check the logic of SupplementaryUQ too please.		########################################################################						
					}
				}
				else
				{
					result += invoiceLine.JI_CustomsSecondQuantity;
				}
			}
			return result;
		});
		CachedProperty<ZDecimal> supplementaryQuantityCached;

		public ZString SupplementaryUQ
		{
			get
			{
				_ = SupplementaryQuantity;
				return RandomLine.JI_CustomsSecondUnitQty; // we know this is ok and common to all lines, otherwise the above (SupplementaryQuantity) would have barfed
			}
		}

		public ZString CusNumber => RandomLine.ZG_CusNumber;

		public TaxStructCollection Taxes => Factory.GetValue(ref taxesCached, GetTaxes);
		CachedProperty<TaxStructCollection> taxesCached;

		protected virtual TaxStructCollection GetTaxes()
		{
			//HACK!
			var keys = Declaration.CreateEntryCreationStrategy().GetTaxKeys();
			var result = new TaxStructCollection();

			var taxes = new Dictionary<MergeKey, TaxStruct>();
			foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
			{
				foreach (JobComInvoiceLineTax jlt in invoiceLine.Taxes)
				{
					var mergeKey = new MergeKey();
					foreach (string key in keys)
					{
						mergeKey.Add((IZType)jlt[key]);
					}
					TaxStruct tax;
					if (!taxes.TryGetValue(mergeKey, out tax))
					{
						tax = result.AddNew();
						taxes[mergeKey] = tax;
					}
					tax.AddTax(jlt);
				}
			}
			return result;
		}

		public IEnumerable<InvoiceLinePackagePivot> PackagingDetails => Factory.GetValue(ref packagingDetailsCached, () =>
		{
			var coll = new List<InvoiceLinePackagePivot>();
			foreach (JobComInvoiceLine line in this.InvoiceLines)
			{
				coll.AddRange(line.PackagesPivot.OfType<InvoiceLinePackagePivot>());
			}
			return coll;
		});
		CachedProperty<IEnumerable<InvoiceLinePackagePivot>> packagingDetailsCached;

		public NonPersistentCusContainerCollection ContainersForInvoiceLines => Factory.GetValue(ref containersForInvoiceLinesCached, () =>
		{
			var coll = new NonPersistentCusContainerCollection(RandomLine);
			coll.RemoveAll();

			foreach (JobComInvoiceLine line in InvoiceLines)
			{
				coll.AddRange(line.ContainersForInvoiceLinesForBindingOnly);
			}
			coll.Sort(NonPersistentCusContainer.Schema.ContainerNumber);
			return coll;
		});
		CachedProperty<NonPersistentCusContainerCollection> containersForInvoiceLinesCached;

		public IEnumerable<ZString> Containers => Factory.GetValue(ref containersCached, () =>
		{
			var containers = InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.ContainersPivot).Cast<CusContainerInvoiceLinePivot>();
			return containers.Select(x => x.ContainerNumber).Where(x => !x.IsEmpty).Distinct();
		});
		CachedProperty<IEnumerable<ZString>> containersCached;

		public ZString Tariff => RandomLine.JI_Tariff;

		public ZString SupplementaryCode1 => RandomLine.JI_SupplementaryCode1;

		public ZString SupplementaryCode2 => RandomLine.JI_SupplementaryCode2;

		public ZString CountryOfOriginCode => RandomLine.JI_CountryOfOrigin;

		public ZString PreferenceCode => RandomLine.JI_PrimaryPreference;

		public ZString ProcedureCode => RandomLine.JI_Procedure;

		public ZString QuotaOrderNumber => RandomLine.JI_ConcessionOrder;

		public ZString ValuationMethod => RandomLine.JI_ValuationCode;

		public OrgAddress SupervisingOffice => RandomLine.SupervisingOffice;

		public ZString ValueAdjustmentCode => RandomLine.ZG_ValueAdjustmentCode;

		public ZDecimal ValueAdjustmentAmount => RandomLine.JI_ValuationMarkup;

		public ZString PrincipalsRepresentativeName => RandomLine.ZG_PrincipalsRepresentativeName;

		public ZString PrincipalsRepresentativeCity => RandomLine.ZG_RL_NKPrincipalsRepresentativeCity;

		public ZString TransportChargesMethodOfPayment => RandomLine.InvoiceHeader.ZG_TransportChargesMethodOfPayment;

		public ZString CountryOfExport => Configuration?.MergeJI_RN_NKCountryOfExport(Declaration) ?? false ? RandomLine.JI_RN_NKCountryOfExport : ZString.Empty;

		public ZString CountryOfSupply => RandomLine.ZG_CountryOfSupply;

		public ZString CountryOfDestination => RandomLine.ZG_CountryOfDestination;

		public RefCusProcedure CusProcedure => RandomLine.CusProcedure;

		public EntryLineConfiguration Configuration => configuration ?? (configuration = Declaration?.Configuration.EntryLineConfiguration);
		EntryLineConfiguration configuration;

		public ZString AllOtherFeeDetails => Factory.GetValue(ref allOtherFeeDetailsCached, () =>
		{
			var sb = new ZStringBuilder();
			foreach (var fee in Fees.OfType<CusEntryLineFee>().Where(f => !Header.ExcludedFeeCodesForAllOtherFees.Contains(f.CF_ChargeType)))
			{
				sb.Append(Invariant($"{fee.CF_ChargeType}: {fee.CF_ChargeAmount.ToString("C2", CultureInfo.CurrentCulture)} {fee.CF_MethodOfPayment} {fee.CF_MethodOfCalculation}").Trim());
			}
			return sb.ToStringWithNewLineBetweenAppends();
		});
		CachedProperty<ZString> allOtherFeeDetailsCached;

		public ZDecimal DutyDetails => Factory.GetValue(ref dutyDetailsCached, GetDutyAmountCore);
		CachedProperty<ZDecimal> dutyDetailsCached;
		protected override ZDecimal GetDutyAmountCore() => GetDutyTotalAmount(Fees.AllLineFees.ToList().AsReadOnly());

		protected ZDecimal GetDutyTotalAmount(IReadOnlyCollection<CusEntryLineFee> effectiveFees)
		{
			var result = ZDecimal.Zero;
			if (ShouldCalculateDutyAmount)
			{
				var dataGrouping = Declaration?.GetDefaultDataGroupingCode(DefaultDataGroupingType.DutyRateCodes);

				if (dataGrouping.HasValue)
				{
					var dutyRateCodes = GetAllDutyTypeRateCodes(dataGrouping.Value);

					foreach (var rateCode in dutyRateCodes)
					{
						result += effectiveFees.CalculateTotalFeeAmount(rateCode, includeLandedCostOnly: false);
					}
				}
			}

			return result;
		}
		protected bool ShouldCalculateDutyAmount => RandomLine.CusProcedure?.ZZ6_CalculateDuty ?? true;

		public ZDecimal DutyDetailsForVAT => Factory.GetValue(ref dutyDetailsForVATCached, () => GetDutyTotalAmount(Fees.AllLineFees.Where(f => f.IncludeForVatCalculation).ToList().AsReadOnly()));
		CachedProperty<ZDecimal> dutyDetailsForVATCached;

		IEnumerable<string> GetAllDutyTypeRateCodes(ZString dataGrouping)
		{
			var rateTypes = RandomLine.NationalRateSelectionCriteria.Select(x => x.RateType)
				.Union(new ZString[] { Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.CountervailingDuty, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.AntiDumpingDuty })
				.Distinct();

			var refCusRates = Factory.GetCachedRatesByType(
				dataGrouping,
				rateTypes.ToArray()
			);

			var rateCodes = new HashSet<string>();

			foreach (var refCusRate in refCusRates)
			{
				rateCodes.Add(GetFeeCodeFromRateCode(refCusRate.ZY1_RateCode));
			}

			return rateCodes;
		}

		public string GetFeeCodeFromRateCode(string rateCode) => GetFeeCodeFromRateCodeCore(rateCode);

		protected virtual string GetFeeCodeFromRateCodeCore(string rateCode) => rateCode;

		public ZDecimal VATDetails => Factory.GetValue(ref vatDetailsCached, GetGSTVATAmountCore);
		CachedProperty<ZDecimal> vatDetailsCached;

		protected override ZDecimal GetGSTVATAmountCore()
		{
			var result = ZDecimal.Zero;
			foreach (var vatFee in Fees.OfType<CusEntryLineFee>().Where(f => (f.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.Vat || f.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.VatOnAdditionalDutiesForNorthernIreland) && !f.CF_IsLandedCostOnly))
			{
				result += vatFee.CF_ChargeAmount;
			}
			return result;
		}

		protected override ZDecimal GetGSTVATDeferredCore()
		{
			var result = ZDecimal.Zero;
			foreach (var vatFee in Fees.OfType<CusEntryLineFee>().Where(f => (f.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.Vat || f.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.VatOnAdditionalDutiesForNorthernIreland) && !f.CF_IsLandedCostOnly && PaymentIsDeferred(f.CF_MethodOfPayment)))
			{
				result += vatFee.CF_ChargeAmount;
			}
			return result;
		}

		bool PaymentIsDeferred(ZString cF_MethodOfPayment) => Header.TaxFeePaymentCodeIsDeferred(cF_MethodOfPayment);

		public override void ResetTotalsAndCachedValues()
		{
			base.ResetTotalsAndCachedValues();

			CL_StatisticalValue = ZDecimal.Zero;
		}

		protected override void DoMergeInvoiceLine(BaseJobComInvoiceLine baseInvoiceLine)
		{
			base.DoMergeInvoiceLine(baseInvoiceLine);

			CL_StatisticalValue += ((JobComInvoiceLine)baseInvoiceLine).JI_Calc_StatisticalValue;
		}

		#region ICanBeImportOrExport Members

		ZBool ICanBeImportOrExport.IsImport => RandomLine.IsImport;

		ZBool ICanBeImportOrExport.IsExport => RandomLine.IsExport;

		string ICanBeImportOrExport.Level => UniversalReferenceConstants.RefCusCodeListLevelType.Item;

		string ICanBeImportOrExport.TrueCountryCode => Declaration?.CountryCode;
		string ICanBeImportOrExport.DataGroupingCode => Declaration?.GetDefaultDataGroupingCode();

		void ICanBeImportOrExport.ValidatePreviousDocuments() { }

		#endregion

		public ZString SadBox31PackagesPremable => SadBox31PackagesPremableCore;

		protected virtual ZString SadBox31PackagesPremableCore => ZString.Empty;

		public IEnumerable<IDocSADHLineTaxBoxSupporter> GetTaxBoxSupporterList() => GetTaxBoxSupporterListCore();

		protected virtual IEnumerable<IDocSADHLineTaxBoxSupporter> GetTaxBoxSupporterListCore() => Fees.Cast<IDocSADHLineTaxBoxSupporter>();

		#region Guarantee

		protected virtual IEnumerable<ZString> DeferredMethodsOfPayment => Array.Empty<ZString>();

		protected virtual IEnumerable<ZString> GuaranteeDeferredMethodsOfPayment => Array.Empty<ZString>();

		public enum MoPLevel
		{
			Unknown, InvoiceLine, EntryLineFee, InvoiceLineTaxes, Declaration
		}

		protected virtual MoPLevel MoPDetailsLevel => MoPLevel.EntryLineFee;

		protected override bool IsGuaranteeDeferredPaymentUsedCore() => MoPDetailsLevel == MoPLevel.InvoiceLine && GuaranteeDeferredMethodsOfPayment.Contains(RandomLine.ZG_MethodOfPayment)
																	 || MoPDetailsLevel == MoPLevel.InvoiceLineTaxes && Taxes.Cast<TaxStruct>().Any(t => GuaranteeDeferredMethodsOfPayment.Contains(t.G4_MethodOfPayment))
																	 || MoPDetailsLevel == MoPLevel.Declaration && GuaranteeDeferredMethodsOfPayment.Contains(Declaration.ZG_MethodOfPayment)
																	 || MoPDetailsLevel == MoPLevel.EntryLineFee && Fees.Cast<CusEntryLineFee>().Any(f => GuaranteeDeferredMethodsOfPayment.Contains(f.CF_MethodOfPayment));
		#endregion

		public IEnumerable<AmountAndTypeToBeGuaranteed> AmountAndTypeToBeGuaranteeds => AmountAndTypeToBeGuaranteedsCore;

		protected virtual IEnumerable<AmountAndTypeToBeGuaranteed> AmountAndTypeToBeGuaranteedsCore
		{
			get
			{
				return Factory.GetCached(ref amountAndTypeToBeGuaranteedsCache, () =>
				{
					var list = new List<AmountAndTypeToBeGuaranteed>();
					if (Header.HasConsumingGuaranteeProcedure)
					{
						var chargeTypes = Header.TaxCodeSet.Union(Header.DutyTotalUnionList());

						list.Add(new AmountAndTypeToBeGuaranteed
						{
							DebitType = GuaranteeDebitType.NORMAL,
							AmountInDeclarationCurrency = Fees.Cast<CusEntryLineFee>().Where(x => chargeTypes.Contains(x.CF_ChargeType)).Sum(x => x.CF_ChargeAmount)
						});
					}

					return list;
				});
			}
		}
		CachedProperty<IEnumerable<AmountAndTypeToBeGuaranteed>> amountAndTypeToBeGuaranteedsCache;

		public EntryLineVatCalculator GetEntryLineVatCalculator() => GetEntryLineVatCalculatorCore();

		protected virtual EntryLineVatCalculator GetEntryLineVatCalculatorCore() => new EntryLineVatCalculator(this);

		public override void Delete()
		{
			readOnlySupportingDocuments?.RemoveAndDeleteAll();
			base.Delete();
		}
	}
}
