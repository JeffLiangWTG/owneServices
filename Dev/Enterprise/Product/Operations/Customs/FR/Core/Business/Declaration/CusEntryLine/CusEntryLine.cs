using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.MessagesWrappers.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	[TestedAsNonPersistentBusinessObject]
	public class CusEntryLine : EU.Business.Declaration.CusEntryLine, Integration.Customs.FR.ICusEntryLine
	{
		public CusEntryLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : EU.Business.Declaration.CusEntryLine.Schema
		{
			public const string CL_BaseVATableValue = "CL_BaseVATableValue";
			public const string D48 = "D48";
		}

		protected override Customs.Business.CusEntryLineValidation GetNewValidation() => new CusEntryLineValidation(this);

		public new EU.Business.Declaration.CusEntryLineValidation Validation => (CusEntryLineValidation)base.Validation;

		protected override Customs.Business.ICusEntryLineFeeCollection<Customs.Business.CusEntryLineFee, Customs.Business.CusEntryLine> GetCusEntryLineFeeCollection()
		{
			return new CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>(this, Factory);
		}

		public new CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine> Fees => (CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>)base.Fees;

		protected override Customs.Business.ConfirmedCusEntryLineFeeCollection GetConfirmedCusEntryLineFeeCollection()
		{
			return new ConfirmedCusEntryLineFeeCollection(this);
		}

		public new ConfirmedCusEntryLineFeeCollection ConfirmedFees => (ConfirmedCusEntryLineFeeCollection)base.ConfirmedFees;

		protected override IEnumerable<IDocSADHLineTaxBoxSupporter> GetTaxBoxSupporterListCore()
			=> CL_LineNumber == 1 ? Header.GetTaxBoxSupporterList().Union(ConfirmedFees.Cast<CusEntryLineFee>()) : ConfirmedFees.Cast<CusEntryLineFee>();

		public new CusEntryLineConfirmedFeeWrapperCollection ConfirmedFeesReadOnly => base.Factory.GetValue(ref confirmedFeesReadOnly, () => GetConfirmedFeesReadOnlyCore());

		CachedProperty<CusEntryLineConfirmedFeeWrapperCollection> confirmedFeesReadOnly;

		protected new CusEntryLineConfirmedFeeWrapperCollection GetConfirmedFeesReadOnlyCore() => new CusEntryLineConfirmedFeeWrapperCollection(this);

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		public new CusEntryHeader Header => (CusEntryHeader)base.Header;

		public new JobComInvoiceLine RandomLine => (JobComInvoiceLine)base.RandomLine;
		protected override bool EffectiveGrossWeightIsApplicableCore => true;

		public ZString LocalCurrency => localCurrency ?? (localCurrency = RefCurrency.LoadFromCurrencyCode(GlbCompany.CurrentCompany.Factory, GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency)?.RX_Code ?? ZString.Empty);
		string localCurrency;

		public bool IsPromotionalProductToDROM => InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.IsPromotionalProductToDROM);

		public bool IsProductOfNegligibleValueToDROM => InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.IsProductOfNegligibleValueToDROM);

		[ReadOnlyMember(nameof(CL_ValueForVATReadOnly))]
		public override ZDecimal CL_ValueForVAT
		{
			get { return base.CL_ValueForVAT; }
			set { base.CL_ValueForVAT = value; }
		}

		public bool CL_ValueForVATReadOnly => true;

		public ZDecimal CL_ConfirmedCIFValue => AddInfo.ZG_ConfirmedCIFValue;

		public override ZString CL_CustomsPostedStatus
		{
			get { return base.CL_CustomsPostedStatus; }
			set
			{
				base.CL_CustomsPostedStatus = value;
				if (!IsValidationSuspended)
				{
					Header?.MarkAsNeedingValidation();
				}
			}
		}

		public CusEntryLineCalculatedFeeCollection CusEntryLineCalculatedFees =>  new CusEntryLineCalculatedFeeCollection(this);

		public CusEntryLineConfirmedFeeCollection CusEntryLineConfirmedFees => new CusEntryLineConfirmedFeeCollection(this);

		#region CL_BaseVATableValue

		public ZDecimal CL_BaseVATableValue
		{
			get
			{
				var result = ZDecimal.Zero;
				var valueForVat = CL_ValueForVAT;
				if (Header != null && valueForVat > ZDecimal.Zero)
				{
					result = valueForVat + DutyAmount;
				}
				return result;
			}
		}

		public ZString CL_BaseVATableValueUQ => LocalCurrency;

		public ZPropertyInfo CL_BaseVATableValueInfo => GetZPropertyInfo(Schema.CL_BaseVATableValue);

		protected override TaxStructCollection GetTaxes()
		{
			var taxes = base.GetTaxes();

			if (CL_LineNumber == 1)
			{
				foreach (var tax in Header.ChargesAsTaxes())
				{
					taxes.Add(tax);
				}
			}

			return taxes ?? new TaxStructCollection();
		}
		#endregion

		[ReadOnly(true)]
		public override ZDecimal CL_InvoiceAmount { get => base.CL_InvoiceAmount; set => base.CL_InvoiceAmount = value; }

		[ReadOnly(true)]
		public override ZString CL_RX_NKInvoiceAmountCurrency { get => base.CL_RX_NKInvoiceAmountCurrency; set => base.CL_RX_NKInvoiceAmountCurrency = value; }

		public Money CL_InvoiceMoney => new Money(CL_InvoiceAmount, InvoiceAmountCurrency);

		protected override ZString GetInvoicedDocumentaryAmountCurrencyCore() => IsMultiInvoiceCurrency ? (ZString)Core.Constants.CurrencyCodes.EuropeanUnion : (RandomLine.InvoiceHeader?.JZ_RX_NKInvoice_Currency ?? (ZString)Core.Constants.CurrencyCodes.EuropeanUnion);

		protected override ZDecimal GetInvoicedDocumentaryAmountCore() => IsMultiInvoiceCurrency ? CL_Calc_InvoicedDocumentaryAmountValueInLocalCurrency : CalculateInvoicedDocumentaryAmount().Amount;

		public ZDecimal CL_Calc_InvoicedDocumentaryAmountValueInLocalCurrency => ConvertToLocalAmountRounded(CalculateInvoicedDocumentaryAmount()).Amount;

		public ZDecimal CL_Calc_InvoicedDocumentaryAmountValueInInvoiceCurrency => CalculateInvoicedDocumentaryAmount().Amount;

		Money CalculateInvoicedDocumentaryAmount()
			=> CurrencyConverter != null ? InvoiceLines.Cast<JobComInvoiceLine>().Aggregate(Money.Empty, (m, x) => CurrencyConverter.Add(m, x.JI_Calc_InvoicedDocumentaryAmount)) : Money.Empty;

		bool IsMultiInvoiceCurrency
		{
			get
			{
				if (isMultiInvoiceCurrencyCache == null)
				{
					isMultiInvoiceCurrencyCache = new CachedProperty<bool>(Factory, () =>
					{
						return !InvoiceLines.Cast<JobComInvoiceLine>().AllSame(line => line.InvoiceHeader.JZ_RX_NKInvoice_Currency);
					});
				}
				return isMultiInvoiceCurrencyCache.Value;
			}
		}
		CachedProperty<bool> isMultiInvoiceCurrencyCache;

		#region properties
		protected virtual ZBool CusEntryLinesConfirmedValueHasBeenPopulated => Header.CusEntryLinesConfirmedValueHasBeenPopulated;

		public ZBool HasConfirmedValues
		{
			get
			{
				var result = false;

				if (CusEntryLinesConfirmedValueHasBeenPopulated)
				{
					var entryStatus = Header.CH_EntryStatus;
					if (Declaration.IsDeltaC && (entryStatus > EntryStatusDescriptionCodeList.Codes.ES050 || entryStatus == EntryStatusDescriptionCodeList.Codes.ES050))
					{
						result = true;
					}
					else if (Declaration.IsDeltaD && (entryStatus > EntryStatusDescriptionCodeList.Codes.ES130 || entryStatus == EntryStatusDescriptionCodeList.Codes.ES130))
					{
						result = true;
					}
				}

				return result;
			}
		}

		public ZDecimal CL_ConfirmedOrCalculatedStatisticalValue => HasConfirmedValues ? CL_ConfirmedStatisticalValue : CL_StatisticalValue;

		public ZDecimal CL_ConfirmedOrCalculatedCustomsValue => HasConfirmedValues ? CL_ConfirmedCustomsValue : CL_CustomsValue;

		public ZDecimal CL_ConfirmedOrCalculatedValueForVAT => HasConfirmedValues ? CL_ConfirmedValueForVAT : CL_ValueForVAT;

		#endregion

		#region Package

		public bool HasNonEmptyPackage
		{
			get
			{
				bool result = false;
				var package = Package;

				if (package != null)
				{
					result = (!package.IsUnpacked && package.Count > 0 || package.IsUnpacked && package.ItemsCount > 0);
				}

				return result;
			}
		}

		public CusEntryLinePack Package => GetPackage();

		CusEntryLinePack GetPackage()
		{
			CusEntryLinePack result = null;

			if (PackagingDetails.Any())
			{
				var firstPackagingDetail = PackagingDetails.First();
				var firstPackage = firstPackagingDetail.Package;
				var firstInvoiceLine = firstPackagingDetail.InvoiceLine;

				bool isfirstPackageUnpacked = firstPackage.CW_PackType == EU.Business.UniversalReferenceConstants.RefCusCodeUnPackedPackageUnitType.Unpacked
										   || firstPackage.CW_PackType == EU.Business.UniversalReferenceConstants.RefCusCodeUnPackedPackageUnitType.UnpackedMultiple
										   || firstPackage.CW_PackType == EU.Business.UniversalReferenceConstants.RefCusCodeUnPackedPackageUnitType.UnpackedSingle;

				result = new CusEntryLinePack(firstPackage.CW_PackType, 0, 0, firstPackage.CW_MarksAndNos, isfirstPackageUnpacked);

				foreach (var packagePivot in PackagingDetails)
				{
					if (packagePivot.Package.CW_PackType == firstPackage.CW_PackType)
					{
						result.Count += packagePivot.CHC_NumberOfPacks;

						if (packagePivot.PK == firstPackagingDetail.PK || packagePivot.InvoiceLine.PK != firstInvoiceLine.PK)
						{
							result.ItemsCount += packagePivot.InvoiceLine.JI_InvoiceQuantity;
						}

						if (!result.MarksAndNos.SplitIgnoringEscapedDelimiter(',', ' ').Contains(packagePivot.Package.CW_MarksAndNos))
						{
							result.MarksAndNos += ", " + packagePivot.Package.CW_MarksAndNos;
						}
					}
				}
			}

			return result;
		}

		#endregion

		protected override Type GetCusEntryHeaderType() => typeof(CusEntryHeader);

		protected override IEnumerable<ZString> DeferredMethodsOfPayment => Factory.GetCachedValue("FR.CusEntryLine.DeferredMethodsOfPayment", () => new ZString[] {
			MethodOfPaymentList.Codes.R });

		protected override IEnumerable<ZString> GuaranteeDeferredMethodsOfPayment => Factory.GetCachedValue("FR.CusEntryLine.GuaranteeDeferredMethodsOfPayment", () => new ZString[] {
			MethodOfPaymentList.Codes.R });

		protected override MoPLevel MoPDetailsLevel => MoPLevel.Declaration;

		protected override IEnumerable<EU.Business.Declaration.MultiLineAddInfos.PreviousDocument> GetPreviousDocumentsToProcess()
		{
			return Declaration.IsUCC6 ? InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.PreviousDocuments.Cast<PreviousDocument>()) : base.GetPreviousDocumentsToProcess();
		}

		public IEnumerable<SupportingDocument> SupportingDocumentsDTP => SupportingDocuments.OfType<SupportingDocument>().Where(x => x.CSI_IsDTP);

		public IEnumerable<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument> SupportingDocumentsToCustoms
		{
			get
			{
				List<SupportingDocument> supportingDocList = new List<SupportingDocument>();

				foreach (SupportingDocument supDoc in Header.SupportingDocuments)
				{
					if (!supDoc.CSI_IsDTP)
					{
						supportingDocList.Add(supDoc);
					}
				}

				var key = new List<string>();
				foreach (SupportingDocument supDoc in SupportingDocuments)
				{
					if (!supDoc.CSI_IsDTP && (!supDoc.IsCodeAPermitType || !key.Contains(supDoc.CSI_Code + supDoc.CSI_ReferenceNumber + supDoc.CSI_DateOfIssue)))
					{
						supportingDocList.Add(supDoc);
						key.Add(supDoc.CSI_Code + supDoc.CSI_ReferenceNumber + supDoc.CSI_DateOfIssue);
					}
				}

				return supportingDocList;
			}
		}

		public ZString EffectiveTransNature
		{
			get
			{
				var result = RandomLine?.ZG_TransNature ?? ZString.Empty;
				return result.IsEmpty ? Header.EntryInstruction?.ZG_TransNature ?? ZString.Empty : result;
			}
		}

		public ZBool IsT2LApplicable => Declaration.JE_MessageType == EU.Business.MessageTypeList.Codes.Export &&
										Declaration.TransportMode == Core.Constants.TransportModes.Sea &&
										(this.SupportingDocuments.Any(doc => doc.CSI_Code == UniversalReferenceConstants.RefCusCodeList.SupportingDocumentsCodes.T2LDocument) || this.Header.SupportingDocuments.Any(doc => doc.CSI_Code == UniversalReferenceConstants.RefCusCodeList.SupportingDocumentsCodes.T2LDocument));

		public ZBool IsT2LFApplicable => Declaration.JE_MessageType == EU.Business.MessageTypeList.Codes.Export &&
										(this.SupportingDocuments.Any(doc => doc.CSI_Code == UniversalReferenceConstants.RefCusCodeList.SupportingDocumentsCodes.T2LFDocument) || this.Header.SupportingDocuments.Any(doc => doc.CSI_Code == UniversalReferenceConstants.RefCusCodeList.SupportingDocumentsCodes.T2LFDocument));

		public ZString GetWarehouseType()
		{
			var warehouseType = ZString.Empty;
			var result = ZString.Empty;

			if (HasIntoRegimeProcedure)
			{
				warehouseType = Header.EntryInstruction?.ToWarehouseType ?? ZString.Empty;
			}
			else if (HasOutOfRegimeProcedure)
			{
				warehouseType = Header.EntryInstruction?.FromWarehouseType ?? ZString.Empty;
			}

			switch (warehouseType)
			{
				case CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1:
					result = WarehouseTypeList.Codes.Type_R;
					break;
				case CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2:
					result = WarehouseTypeList.Codes.Type_S;
					break;
				case CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP:
					result = WarehouseTypeList.Codes.Type_U;
					break;
			}

			return result;
		}

		public ZInt? SpecificRegimeNumberDaysOfDischarge
		{
			get
			{
				var stoValue = Header.EntryInstruction?.GetAuthorisationRuleValue(CusAuthorisationRuleTypeList.Codes.STO);
				if (!stoValue.HasValue || stoValue.Value.IsEmpty)
				{
					return null;
				}
				else
				{
					return (ZInt)Convert.ToInt32(stoValue);
				}
			}
		}

		public ZDecimal SpecificRegimeGuaranteeAmount
		{
			get
			{
				if (specificRegimeGuaranteeAmountCached == null)
				{
					specificRegimeGuaranteeAmountCached = new CachedProperty<ZDecimal>(Factory, () =>
					{
						var result = ZDecimal.Zero;
						var instruction = Header.EntryInstruction;
						if (instruction != null)
						{
							var pcd = instruction.GetAuthorisationRuleDecimalValue(CusAuthorisationRuleTypeList.Codes.PCD) * 0.01m * DutyAmount;
							var pcv = instruction.GetAuthorisationRuleDecimalValue(CusAuthorisationRuleTypeList.Codes.PCV) * 0.01m * GSTVATAmount;
							var pcp = instruction.GetAuthorisationRuleDecimalValue(CusAuthorisationRuleTypeList.Codes.PCP) * 0.01m * ParaFiscal;
							result = pcd + pcv + pcp;
						}

						return result;
					});
				}

				return specificRegimeGuaranteeAmountCached.Value;
			}
		}

		CachedProperty<ZDecimal> specificRegimeGuaranteeAmountCached;

		[ResourceStringData("Enterprise.Customs.FR.Business.Declaration.CusEntryLine|CL_Calc_GuaranteeAmount", Caption = "Guarantee Amount")]
		public ZDecimal CL_Calc_GuaranteeAmount => SpecificRegimeGuaranteeAmount.Round(0);

		internal ZDecimal ParaFiscal => Fees.OfType<CusEntryLineFee>()
			.Where(fee => FeeTypeCodeConverter.GetTaxCategory(fee.NationalFeeTypeCode) == TaxCategoryList.Codes.IndirectFees)
			.Sum(fee => fee.CF_ChargeAmount);

		public ZBool HasIntoRegimeProcedure => InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.HasIntoRegimeProcedure);

		public ZBool HasOutOfRegimeProcedure => InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.HasOutOfRegimeProcedure);

		public CusTempStorageJobHeader PreviousISTHeader => RandomLine.PreviousISTHeader;

		protected override string GetFeeCodeFromRateCodeCore(string rateCode)
		{
			return FeeTypeCodeConverter.GetEUFeeTypeCode(rateCode);
		}

		protected override object GetAdditionalInfoDistinctKey(EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo ai)
		{
			return new
			{
				ai.CSI_SubType,
				ai.CSI_Code,
				ai.CSI_ReferenceNumber
			};
		}

		public ZDecimal D48Amount
		{
			get
			{
				var wrapper = new ArticleWrapper(Header, this);
				return wrapper.SupportingDocuments.Sum(x => x.D48Amount);
			}
		}

		protected override IEnumerable<AmountAndTypeToBeGuaranteed> AmountAndTypeToBeGuaranteedsCore
		{
			get
			{
				var list = base.AmountAndTypeToBeGuaranteedsCore.ToList();
				var d48Amount = D48Amount;

				if (d48Amount != ZDecimal.Zero)
				{
					list.Add(new AmountAndTypeToBeGuaranteed
					{
						DebitType = GuaranteeDebitType.SUSPENDED,
						AmountInDeclarationCurrency = d48Amount,
						Procedure = Schema.D48
					});
				}

				return list;
			}
		}

		protected override ZString GetFallbackInvoiceLineDescription()
		{
			return FirstLine.JI_Description;
		}

		public bool HasNegligibleValueProcedure =>  InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.HasNegligibleValueProcedure);

		public bool HasC2CProcedure =>  InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.HasC2CProcedure);
	}
}
