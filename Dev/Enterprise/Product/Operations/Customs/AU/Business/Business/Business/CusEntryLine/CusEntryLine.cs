using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusEntryLine : TypeSafeCusEntryLine, IDutyData, ICPQALineAttachee, ICMRDutyData, ICusEntryLine, IAddInfo, IAddInfoManager, Integration.Customs.AU.ICusEntryLine, IDrawbackEntryLine
	{
		#region Constants
		public static class ParentTrailer
		{
			public const string Normal = "N";
			public const string Parent = "P";
			public const string Trailer = "T";
		}

		public static class TransportModeAbbreviation
		{
			public const string Air = "A";
			public const string Sea = "S";
			public const string Post = "P";
			public const string Other = "O";
		}
		#endregion

		public CusEntryLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override void RoundCustomsValue()
		{
			base.RoundCustomsValue();

			if (CL_CustomsValue < 0m)
			{
				CL_CustomsValue = 0m;
				if (Header != null && Header.AddInfo.ZA_NegativeCVAdjusted_Hidden.IsEmpty)
				{
					Header.AddInfo.ZA_NegativeCVAdjusted_Hidden = "Y";
				}
			}

			CL_CustomsValue = ZArchitecture.Core.Utilities.Round(CL_CustomsValue, 2);
		}

		public string Prefix
		{
			get { return IsParent ? "P" : (IsTrailer ? "T" : "N"); }
		}

		protected override ZDecimal GetGSTRate()
		{
			ZDecimal result = 0m;
			if (RandomLine != null && RandomLine.AddInfo.ZA_GSTE.IsEmpty && !IsNature20)
			{
				result = DutyCalculator.GSTRate;
			}
			return result;
		}

		#region Parent/Trailer

		public virtual bool IsParent
		{
			get { return ParentLine == null && ChildLine != null; }
		}

		public virtual bool IsTrailer
		{
			get { return ParentLine != null && ChildLine == null; }
		}

		public bool IsNormal
		{
			get { return ParentLine == null && ChildLine == null; }
		}

		public CusEntryLine ParentLine
		{
			get
			{
				CusEntryLine result = null;

				if (InvoiceLines.Count > 0 && RandomLine.ParentLine != null)
				{
					result = (CusEntryLine)Factory.Load(typeof(CusEntryLine), RandomLine.ParentLine.JI_CL);
				}
				return result;
			}
		}

		public CusEntryLine ChildLine
		{
			get
			{
				CusEntryLine result = null;
				if (InvoiceLines.Count > 0 && RandomLine.ChildLine != null)
				{
					result = (CusEntryLine)Factory.Load(typeof(CusEntryLine), RandomLine.ChildLine.JI_CL);
				}
				return result;
			}
		}

		#endregion

		#region Overrides

		protected override ICusEntryLineFeeCollection<Customs.Business.CusEntryLineFee, Customs.Business.CusEntryLine> GetCusEntryLineFeeCollection()
		{
			return new CusEntryLineFeeCollection(this, Factory);
		}

		protected override Customs.Business.InvoiceLinesForEntryLineCollection CreateInvoiceLinesForEntryLineCollection()
		{
			return new InvoiceLinesForEntryLineCollection(this);
		}

		protected override Customs.Business.BondedWarehouseTransactionLine GetNewBondedWarehouseTransactionLine()
		{
			return new BondedWarehouseTransactionLine(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CL_ParentTrailer = ParentTrailer.Normal;
		}

		public override void Delete()
		{
			Questions.RemoveAndDeleteAll();
			base.Delete();
		}

		public override void ResetTotalsAndCachedValues()
		{
			base.ResetTotalsAndCachedValues();
			fTransportAndInsurance = null;
			fTILV = null;
			AddInfoLineConstructor.RefreshCalculation();
			EntryLineAddInfo.ZA_TILV = "";
		}

		public override ZString CL_CustomsPostedStatus
		{
			get { return base.CL_CustomsPostedStatus; }
			set
			{
				base.CL_CustomsPostedStatus = value;
				if (CL_CustomsPostedStatus == Customs.Business.EntryLineStatusList.Codes.DeletePending && Header.IsCustomsChargePaid)
				{
					RefundReasonCode = DeletedLineAmendment.RefundReasonForDeletedLines;
				}
			}
		}

		[DecimalPlaces(4)]
		public override ZDecimal CL_DutyPercent
		{
			get { return base.CL_DutyPercent; }
			set { base.CL_DutyPercent = value; }
		}

		public override ZShort EffectiveLineNumber
		{
			get
			{
				return Declaration.IsConsolidated
					? ZA_AggregateEntryLineNumber
					: base.EffectiveLineNumber;
			}
		}

		public override ZString UniqueKey
		{
			get
			{
				return Declaration.IsConsolidated
					? UniqueKeyPrefix + Header.EntryNumber + "-" + EffectiveLineNumber
					: base.UniqueKey;
			}
		}

		#endregion

		#region Money Fields

		protected override Money PriceCore
		{
			get { return ChargesProvider.InvoiceTotal; }
		}

		public
#if DEBUG
 virtual
#endif
 Money PriceAdjustment
		{
			get
			{
				Money result = Money.Empty;
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					result = CurrencyConverter.Add(result, invoiceLine.JI_PriceAdjustment);
				}
				return result;
			}
		}

		protected override Money InvoiceLineAmountToAddForPrice(BaseJobComInvoiceLine invoiceLine)
		{
			Money result = base.InvoiceLineAmountToAddForPrice(invoiceLine);
			if (Header.ShouldEntryBeNormalised)
			{
				result = invoiceLine.JI_FOB;
			}
			return result;
		}

		public Money PriceIncludingAdjustment
		{
			get { return CurrencyConverter.Add(PriceAdjustment, Price); }
		}

		public bool DoesTILVExist
		{
			get { return TILV > 0m || RandomLine != null && !RandomLine.AddInfo.EffectiveTILVString.IsEmpty; }
		}

		public ZDecimal TILV => Factory.GetValue(ref fTILV, GetTILV);

		CachedProperty<ZDecimal> fTILV;

		ZDecimal GetTILV()
		{
			ZDecimal result = EntryLineAddInfo.TILVInAUD;
			if (result.IsEmpty)
			{
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					result += invoiceLine.AddInfo.TILVInAUD;
				}
			}
			return result;
		}

		public void UpdateTILV(string tilv)
		{
			EntryLineAddInfo.ZA_TILV = tilv;
			fTILV = null;
		}

		public Money TILVMoney
		{
			get { return new Money(TILV, JobDeclaration.GetLocalCurrency()); }
		}

		internal void UpdateLineTILV()
		{
			ZString tilvString = ZString.Empty;

			if (Header != null && Header.TAndITransmitConditionChecker.ShouldTransmitTAndIForLine)
			{
				tilvString = TransportAndInsurance.ToString();
			}

			EntryLineAddInfo.ZA_TILV = tilvString;
		}

		/// <summary>
		/// An aggregated amount of Transport & Insurance. If TILV is there (even for 0.00), it returns the amount. Otherwise Freight and Insurance in Charges.
		/// </summary>
		public Money TransportAndInsurance => Factory.GetValue(ref fTransportAndInsurance, GetTransportAndInsurance);
		CachedProperty<Money> fTransportAndInsurance;

		Money GetTransportAndInsurance()
		{
			Money result = Money.Empty;
			foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
			{
				result = CurrencyConverter.Add(result, invoiceLine.TransportAndInsurance);
			}
			return result;
		}

		/// <summary>
		/// System should send MOA+68 (TILV) only if it should. (Users have overriden transport and insurance in AddInfo or Invoice/Line level OFT/ONS)
		/// </summary>
		public Money TransportAndInsuranceForMessage
		{
			get
			{
				Money result = Money.Invalid;

				if (Header != null && Header.TAndITransmitConditionChecker.ShouldTransmitTAndIForLine && Header.TransportAndInsuranceForMessage != null)
				{
					result = CurrencyConverter.ConvertExact(TransportAndInsurance, Header.TransportAndInsuranceCurrencyForMessage);
				}

				return result;
			}
		}

		/// <summary>
		/// It looks at TILV amount that is returned from Customs first and then falls back to Transport and Insurance.
		/// </summary>
		public Money CustomsCalculatedTransportAndInsuranceInLocalCurrency
		{
			get
			{
				if (!EntryLineAddInfo.ZA_TILV.IsEmpty)
				{
					return CurrencyConverter.ConvertExact(EntryLineAddInfo.TILVMoney, JobDeclaration.GetLocalCurrency());
				}
				else
				{
					return TransportAndInsuranceInLocalCurrency;
				}
			}
		}

		public Money TransportAndInsuranceInLocalCurrency
		{
			get { return CurrencyConverter.ConvertExact(TransportAndInsurance, JobDeclaration.GetLocalCurrency()); }
		}

		public ZDecimal TransportAndInsuranceUnitValueInLocalCurrency
		{
			get
			{
				ZDecimal quantity = (WRQ == 0m) ? this.Quantity : WRQ;
				ZDecimal result = 0m;
				if (quantity != 0m)
				{
					result = CustomsCalculatedTransportAndInsuranceInLocalCurrency.Amount / quantity;
				}
				return result;
			}
		}

		public Money WarehouseUnitValue
		{
			get { return new Money(this.CL_WarehouseUnitValue, LocalCurrency); }
		}

		public Money StandardDuty
		{
			get
			{
				Money result = Money.Empty;
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					result = CurrencyConverter.Add(result, invoiceLine.JI_StandardDuty);
				}
				return result;
			}
		}

		public Money DumpingExportPrice
		{
			get
			{
				Money result = Money.Empty;
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					result = CurrencyConverter.Add(result, invoiceLine.JI_DumpingExportPrice);
				}
				return result;
			}
		}

		// this is pre CMR legacy
		public Money SecurityConcession
		{
			get
			{
				Money result = Money.Empty;
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					result = CurrencyConverter.Add(result, invoiceLine.JI_SecurityConcession);
				}
				return result;
			}
		}

		public Money OtherDutyFactor
		{
			get
			{
				Money result = Money.Empty;
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					result = CurrencyConverter.Add(result, invoiceLine.JI_OtherDutyFactor);
				}
				return result;
			}
		}

		//Luxury Car Tax
		public Money LCTPayable
		{
			get
			{
				Money result = Money.Empty;
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					result = CurrencyConverter.Add(result, invoiceLine.JI_LCT);
				}
				return result;
			}
		}

		public ZInt WarehouseRelatedLine
		{
			get
			{
				if (RandomLine != null && IsNature30)
				{
					return RandomLine.JI_WarehouseRelatedLine;
				}
				return 0;
			}
		}

		public ZDecimal Quantity
		{
			get
			{
				decimal result = 0m;
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					result += invoiceLine.JI_CustomsQuantity;
				}
				return result;
			}
		}

		public ZDecimal SecondQuantity
		{
			get
			{
				decimal result = 0m;
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					result += invoiceLine.JI_SecondQuantity;
				}
				return result;
			}
		}

		public Money BuyingCommission
		{
			get
			{
				Money result = Money.Empty;
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					result = CurrencyConverter.Add(result, invoiceLine.JI_BuyingCommission);
				}
				return result;
			}
		}

		public Money OtherCommission
		{
			get
			{
				Money result = Money.Empty;
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					result = CurrencyConverter.Add(result, invoiceLine.JI_OtherCommission);
				}
				return result;
			}
		}

		public Money Commission
		{
			get { return ChargesProvider.Commission; }
		}

		public Money ExWorksAmount
		{
			get
			{
				Money result = Money.Empty;
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					result = CurrencyConverter.Add(result, invoiceLine.JI_ExWorksAmount);
				}
				return result;
			}
		}

		public Money OtherCharge1
		{
			get { return ChargesProvider.OtherCharges1; }
		}

		public Money OtherCharge2
		{
			get { return ChargesProvider.OtherCharges2; }
		}

		public Money LandingCharge
		{
			get { return ChargesProvider.LandingCharges; }
		}

		public Money Discount
		{
			get { return ChargesProvider.Discount; }
		}

		public Money PackingCosts
		{
			get { return ChargesProvider.PackingCosts; }
		}

		public Money ForeignInlandFreight
		{
			get { return ChargesProvider.ForeignInlandFreight; }
		}

		public ICommercialChargesProvider ChargesProvider
		{
			get
			{
				return Header.ShouldEntryBeNormalised ? NormalisedProvider : CommercialChargesProvider;
			}
		}

		EntryLineNormalisedChargesProvider fNormalisedProvider;
		protected EntryLineNormalisedChargesProvider NormalisedProvider
		{
			get
			{
				if (fNormalisedProvider == null)
				{
					fNormalisedProvider = new EntryLineNormalisedChargesProvider(this, Header.NormalisedInvoiceTotalCurrency);
				}
				return fNormalisedProvider;
			}
		}

		EntryLineCommercialChargesProvider fCommercialChargesProvider;
		protected EntryLineCommercialChargesProvider CommercialChargesProvider
		{
			get
			{
				if (fCommercialChargesProvider == null)
				{
					fCommercialChargesProvider = new EntryLineCommercialChargesProvider(this);
				}
				return fCommercialChargesProvider;
			}
		}

		public ZDecimal Weight
		{
			get
			{
				decimal result = 0m;
				foreach (var invoiceLine in InvoiceLines.Cast<JobComInvoiceLine>())
				{
					result += invoiceLine.JI_Weight;
				}
				return result;
			}
		}

		#endregion

		#region New properties

		public bool IsLessDutyAndTax
		{
			get { return CurrentTotalDutyTax < TotalDutyTaxAdvisedInLastClearanceMessage; }
		}

		public virtual ZDecimal TotalDutyTaxAdvisedInLastClearanceMessage
		{
			get { return Fees.GetAmount(CusEntryChargeTypeList.Codes.TotalDutyTaxForLine); }
		}

		public ZPropertyInfo TotalDutyTaxAdvisedInLastClearanceMessageInfo
		{
			get { return GetZPropertyInfo(nameof(TotalDutyTaxAdvisedInLastClearanceMessage)); }
		}

		public virtual ZDecimal CurrentTotalDutyTax
		{
			get { return Fees.GetTotalPayableDutyTax(); }
		}

		public ZPropertyInfo CurrentTotalDutyTaxInfo
		{
			get { return GetZPropertyInfo(nameof(CurrentTotalDutyTax)); }
		}

		public ZDecimal ZA_QT2
		{
			get
			{
				ZDecimal result = 0m;
				foreach (JobComInvoiceLine invLine in this.InvoiceLines)
				{
					result += invLine.AddInfo.ZA_QT2;
				}
				return result;
			}
		}

		public bool IsLodgedAtCustoms
		{
			get { return Header.HasBeenLodgedAtCustoms && CL_LineNumber > 0 && CL_LineNumber <= Header.CH_HighestLineNumber; }
		}

		public ZString ZA_DetailsNotToBeAmended
		{
			get { return EntryLineAddInfo.ZA_DetailsNotToBeAmended_Hidden; }
			set { EntryLineAddInfo.ZA_DetailsNotToBeAmended_Hidden = value; }
		}

		public ZShort ZA_AggregateEntryLineNumber
		{
			get { return EntryLineAddInfo.ZA_AggregateEntryLineNumber_Hidden; }
			set { EntryLineAddInfo.ZA_AggregateEntryLineNumber_Hidden = value; }
		}

		ZString CurrentValueForNonAmendableDetails
		{
			get { return NatureType; }
		}

		public void UpdateDetailsNotToBeAmendedAsEntryIsCleared()
		{
			//once this column is filled in, it stays forever as it reflects what Customs has at their site for this line.
			if (ZA_DetailsNotToBeAmended.IsEmpty)
			{
				ZA_DetailsNotToBeAmended = CurrentValueForNonAmendableDetails;
			}
		}

		protected override string NonAmendableDetailsCore
		{
			get { return ZA_DetailsNotToBeAmended; }
		}

		protected override bool HasNonAmendableChangesCore
		{
			get { return !ZA_DetailsNotToBeAmended.IsEmpty && !CurrentValueForNonAmendableDetails.EqualsIgnoringCase(ZA_DetailsNotToBeAmended); }
		}

		[DecimalPlaces(5)]
		public ZDecimal DrawbackClaimQuantity
		{
			get { return drawbackClaimQuantity; }
			set { SetNonPersistentPropertyValue(DrawbackClaimQuantityInfo, ref drawbackClaimQuantity, value); }
		}

		ZDecimal drawbackClaimQuantity;

		public ZPropertyInfo DrawbackClaimQuantityInfo
		{
			get { return GetZPropertyInfo(nameof(DrawbackClaimQuantity)); }
		}

		public ZBool IsNonAQISAEPLine
		{
			get { return RandomLine.AddInfo.ZA_IsNonAQISAEPLine_Hidden; }
		}

		#endregion

		#region Related Objects

		public JobComInvoiceLine MinimumLine
		{
			get
			{
				JobComInvoiceLine.LineComparer comparer = new JobComInvoiceLine.LineComparer();
				JobComInvoiceLine result = null;
				foreach (JobComInvoiceLine invLine in InvoiceLines)
				{
					if (result == null || comparer.Compare(invLine, result) < 0)
					{
						result = invLine;
					}
				}
				return result;
			}
		}

		protected internal Classification CommonClassification
		{
			get
			{
				Classification lastClass = null;
				foreach (JobComInvoiceLine line in InvoiceLines)
				{
					if (lastClass == null)
					{
						if (line.Classification == null)
						{
							break;
						}
						else
						{
							lastClass = line.Classification;
						}
					}
					else
					{
						if (line.Classification != lastClass)
						{
							lastClass = null;
							break;
						}
					}
				}
				return lastClass;
			}
		}

		public AUOrgSupplierPart CommonPart
		{
			get
			{
				AUOrgSupplierPart lastPart = null;
				foreach (JobComInvoiceLine line in InvoiceLines)
				{
					if (lastPart == null)
					{
						if (line.Part == null)
						{
							break;
						}
						else
						{
							lastPart = line.Part;
						}
					}
					else
					{
						if (line.Part != lastPart)
						{
							lastPart = null;
							break;
						}
					}
				}
				return lastPart;
			}
		}

		public AUAddInfo InvoiceLineAddInfo
		{
			get { return RandomLine.AddInfo; }
		}

		AUAddInfo IDutyData.AddInfo
		{
			get { return InvoiceLineAddInfo; }
		}

		public ZString AddInfoLine
		{
			get { return AddInfoLineConstructor.AddInfoLine; }
		}

		EntryLineAddInfoLineConstructor AddInfoLineConstructor
		{
			get
			{
				if (fAddInfoLineConstructor == null)
				{
					fAddInfoLineConstructor = new EntryLineAddInfoLineConstructor(this);
				}
				return fAddInfoLineConstructor;
			}
		}
		EntryLineAddInfoLineConstructor fAddInfoLineConstructor;

		#endregion

		#region Amount in AUD

		public ZDecimal DutyAmountIncludingWHEstimate
		{
			get { return Fees.GetTotalAmount(CusEntryChargeTypeList.Codes.DutyAmount, IncludeLandedCostsOnly); }
		}

		public ZDecimal GSTVATAmountIncludingWHEstimate
		{
			get { return Fees.GetTotalAmount(CusEntryChargeTypeList.Codes.GSTAmount, IncludeLandedCostsOnly); }
		}

		public ZDecimal WETAmountIncludingWHEstimate
		{
			get { return Fees.GetTotalAmount(CusEntryChargeTypeList.Codes.WetAmount, IncludeLandedCostsOnly); }
		}

		public ZDecimal LCTAmountIncludingWHEstimate
		{
			get { return Fees.GetTotalAmount(CusEntryChargeTypeList.Codes.LCTAmount, IncludeLandedCostsOnly); }
		}

		public ZDecimal WoodLevyIncludingWHEstimate
		{
			get { return GetAmountApportionedFromCusEntryHeader(Header.Charges.GetTotalAmount(CusEntryChargeTypeList.Codes.Woodlevy, IncludeLandedCostsOnly)); }
		}

		public ZDecimal SecurityConcessionAmount
		{
			get { return Fees.GetAmount(CusEntryChargeTypeList.Codes.SecurityConcession); }
		}

		public ZDecimal SecurityLiabilityAmount
		{
			get { return Fees.GetAmount(CusEntryChargeTypeList.Codes.SecurityLiability); }
		}

		public ZDecimal VOTI
		{
			get { return CL_CustomsValue + CustomsCalculatedTransportAndInsuranceInLocalCurrency.Amount + DutyAmountIncludingWHEstimate + DumpingDuty + CountervailingDuty; }
		}

		public ZDecimal CountervailingSecurityAmount
		{
			get { return Fees.GetAmount(CusEntryChargeTypeList.Codes.CountervailingSecurityAmount); }
		}

		public ZDecimal DumpingSecurityAmount
		{
			get { return Fees.GetAmount(CusEntryChargeTypeList.Codes.DumpingSecurityAmount); }
		}

		public ZDecimal AllOtherDuty
		{
			get { return CountervailingDuty + DumpingDuty + InterimDumpingDuty + InterimAntiDumpingDuty + InterimCountervailingDuty; }
		}

		public ZDecimal CountervailingDuty
		{
			get { return Fees.GetAmount(CusEntryChargeTypeList.Codes.CountervailingDuty); }
		}

		public ZDecimal DumpingDuty
		{
			get { return Fees.GetAmount(CusEntryChargeTypeList.Codes.DumpingDuty); }
		}

		public ZDecimal InterimDumpingDuty
		{
			get { return Fees.GetAmount(CusEntryChargeTypeList.Codes.InterimDumpingDuty); }
		}

		public ZDecimal DutyOverriden
		{
			get { return Fees.GetAmount(CusEntryChargeTypeList.Codes.DutyOverride); }
		}

		public ZDecimal InterimAntiDumpingDuty
		{
			get { return Fees.GetAmount(CusEntryChargeTypeList.Codes.InterimAntiDumpingDuty); }
		}

		public ZDecimal InterimCountervailingDuty
		{
			get { return Fees.GetAmount(CusEntryChargeTypeList.Codes.InterimCountervailingDuty); }
		}

		public ZDecimal WETAmount
		{
			get { return Fees.GetAmount(CusEntryChargeTypeList.Codes.WetAmount); }
		}

		public ZDecimal LCTAmount
		{
			get { return Fees.GetAmount(CusEntryChargeTypeList.Codes.LCTAmount); }
		}

		public ZDecimal FlatDutyPortion
		{
			get { return Fees.GetAmount(CusEntryChargeTypeList.Codes.FlatDutyPortion); }
		}

		public ZDecimal EntryFee
		{
			get { return GetAmountApportionedFromCusEntryHeader(Header.Charges.GetAmount(CusEntryChargeTypeList.Codes.EntryFee)); }
		}

		public ZDecimal AQISContainerCharges
		{
			get { return GetAmountApportionedFromCusEntryHeader(Header.Charges.GetAmount(CusEntryChargeTypeList.Codes.AQISContainerCharges)); }
		}

		public ZDecimal AQISProcessingCharge
		{
			get { return GetAmountApportionedFromCusEntryHeader(Header.Charges.GetAmount(CusEntryChargeTypeList.Codes.AQISProcessingCharge)); }
		}

		public ZDecimal AQISServiceAmount
		{
			get { return GetAmountApportionedFromCusEntryHeader(Header.Charges.GetAmount(CusEntryChargeTypeList.Codes.AQISServicePaymentAmount)); }
		}

		public ZDecimal AllEntryFees
		{
			get { return GetAmountApportionedFromCusEntryHeader(Header.AllEntryFees); }
		}

		public ZDecimal TradegateGST
		{
			get { return GetAmountApportionedFromCusEntryHeader(Header.Charges.GetAmount(CusEntryChargeTypeList.Codes.TradegateGST)); }
		}

		public ZDecimal Woodlevy
		{
			get { return GetAmountApportionedFromCusEntryHeader(Header.Charges.GetAmount(CusEntryChargeTypeList.Codes.Woodlevy)); }
		}
		#endregion

		#region Implementation
		protected Money fPriceAdjustment;
		protected Money fPrice;
		protected Money fCustomsValue;
		protected Money fTAndI;
		protected Money fWarehouseUnitValue;
		protected Money fStandardDuty;
		protected Money fDumpingExportPrice;
		protected Money fSecurityConcession;
		protected Money fOtherDutyFactor;
		protected Money fLCTPayable;
		protected ZDecimal fQuantity;
		protected ZString fQuantityUQ;
		protected ZDecimal fSecondQuantity;
		protected ZString fSecondQuantityUQ;

		protected JobComInvoiceLine fRandomLine;
		protected RefCurrency LocalCurrency
		{
			get
			{
				return RefCurrency.LoadFromCurrencyCode(Factory, JobDeclaration.LocalCurrencyConstantCode);
			}
		}

		protected override void DoMergeInvoiceLine(BaseJobComInvoiceLine baseInvoiceLine)
		{
			base.DoMergeInvoiceLine(baseInvoiceLine);
			JobComInvoiceLine line = baseInvoiceLine as JobComInvoiceLine;

			CusEntryLineFee interimAntiDumpingDuty = Fees.GetOrAddFeeByFeeType(CusEntryChargeTypeList.Codes.InterimAntiDumpingDuty);
			interimAntiDumpingDuty.CF_ChargeAmount += line.AddInfo.ZA_IDP;

			CusEntryLineFee interimCountervailingDuty = Fees.GetOrAddFeeByFeeType(CusEntryChargeTypeList.Codes.InterimCountervailingDuty);
			interimCountervailingDuty.CF_ChargeAmount += line.AddInfo.ZA_ICV;

			if (CL_WarehouseUnitValue != 0 && CL_WarehouseUnitValue != line.WUV)
			{
				ErrorReporter.ReportOnce("WUVMerge", "Warehouse unit value may not be accumulated - only merge lines with the same wuv. Job: " + baseInvoiceLine.Declaration.JE_DeclarationReference);
			}
			CL_WarehouseUnitValue = line.WUV;
		}

		#endregion

		#region IDutyData Members

		public ZDecimal CustomsFactor
		{
			get
			{
				return Header.CustomsFactor;
			}
		}

		public ZDateTime DateOfValuation
		{
			get
			{
				return RandomLine.DateOfValuation;
			}
		}

		public ZDateTime EffectiveDutyDate
		{
			get
			{
				return Header.EffectiveDutyDate;
			}
		}

		public ZString TariffAndStatCodeFormatted
		{
			get
			{
				return RandomLine == null ? ZString.Empty : RandomLine.JI_Tariff;
			}
		}

		public ZString TariffNumber
		{
			get
			{
				ZString[] split = RandomLine.JI_Tariff.Split(' ');
				return split.Length >= 1 ? split[0].Replace(".", "") : ZString.Empty;
			}
		}

		public ZString StatCode
		{
			get
			{
				ZString[] split = RandomLine.JI_Tariff.Split(' ');
				return split.Length >= 2 ? split[1] : ZString.Empty;
			}
		}

		public ZString TreatmentCode
		{
			get { return RandomLine != null ? RandomLine.AddInfo.ZA_TreatmentCode_Hidden : ZString.Empty; }
		}

		public ZPropertyInfo TreatmentCodeInfo
		{
			get { return GetZPropertyInfo(nameof(TreatmentCode)); }
		}

		public ZString UnitOfQuantity
		{
			get { return RandomLine != null ? RandomLine.JI_CustomsUnitQty : ZString.Empty; }
		}

		public ZString UnitOfWeight => RandomLine != null ? RandomLine.JI_WeightUQ : ZString.Empty;

		public List<ZString> Permits => RandomLine != null ? RandomLine.PermitsIncludingHeader : new List<ZString>();

		public ZString TemporaryImportNumber => RandomLine != null ? RandomLine.JI_TempImportNum : ZString.Empty;

		public ZString GoodsOriginCode => RandomLine != null ? RandomLine.JI_GoodsOriginCode : ZString.Empty;

		#endregion

		#region Excise Equivalent Goods (EEG)

		public bool IsExciseEquivalentGoods => CMRTariffClassificationCharacteristic.IsTariffExciseEquivalentGoods(Factory, TariffNumber);

		#endregion

		#region Nature Type

		public ZBool IsNature10 => !IsNature20 && !IsNature30;

		public ZBool IsNature20 => RandomLine?.JI_IsPackToBondForLine ?? ZBool.False; //Different nature invoices are not to be merged

		public ZBool IsNature30 => RandomLine?.Declaration?.IsExWarehouse ?? ZBool.False;

		public ZString NatureType
		{
			get { return Declaration.IsImportEdifice ? NatureTypeForLegacy : NatureTypeForCMR; }
		}

		public ZString NatureTypeForCMR
		{
			get
			{
				ZString result = CusEntryHeader.NatureTypesForImportCMR.Nature10;

				if (IsNature30)
				{
					result = CusEntryHeader.NatureTypesForImportCMR.Nature30;
				}
				else if (IsNature20)
				{
					result = CusEntryHeader.NatureTypesForImportCMR.Nature20;
				}

				return result;
			}
		}

		public ZString NatureTypeForLegacy
		{
			get
			{
				ZString result = CusEntryHeader.NatureTypes.Nature10;

				if (IsNature30)
				{
					result = CusEntryHeader.NatureTypes.Nature30;
				}
				else if (IsNature20)
				{
					result = CusEntryHeader.NatureTypes.Nature20;
				}

				return result;
			}
		}

		bool IncludeLandedCostsOnly
		{
			get { return !IsNature20 || RandomLine.IsDutyAndTaxEstimatedForWH; }
		}

		#endregion

		#region AddInfo Money Amount
		public Money CSA
		{
			get
			{
				Money result = Money.Empty;
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					result = CurrencyConverter.Add(result, new Money(invoiceLine.AddInfo.ZA_CSA, JobDeclaration.GetLocalCurrency()));
				}
				return result;
			}
		}

		public Money CVD
		{
			get
			{
				Money result = Money.Empty;
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					result = CurrencyConverter.Add(result, new Money(invoiceLine.AddInfo.ZA_CVD, JobDeclaration.GetLocalCurrency()));
				}
				return result;
			}
		}

		public Money DMP
		{
			get
			{
				Money result = Money.Empty;
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					result = CurrencyConverter.Add(result, new Money(invoiceLine.AddInfo.ZA_DMP, JobDeclaration.GetLocalCurrency()));
				}
				return result;
			}
		}

		public Money DSA
		{
			get
			{
				Money result = Money.Empty;
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					result = CurrencyConverter.Add(result, new Money(invoiceLine.AddInfo.ZA_DSA, JobDeclaration.GetLocalCurrency()));
				}
				return result;
			}
		}

		public
#if DEBUG
 virtual
#endif
 Money DTY
		{
			get
			{
				Money result = Money.Empty;
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					result = CurrencyConverter.Add(result, new Money(invoiceLine.AddInfo.ZA_DTY, JobDeclaration.GetLocalCurrency()));
				}
				return result;
			}
		}

		public Money DXP
		{
			get
			{
				Money result = Money.Empty;
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					result = CurrencyConverter.Add(result, new Money(invoiceLine.AddInfo.DumpingExportAmount, invoiceLine.AddInfo.DumpingExportCurrency));
				}
				return result;
			}
		}

		public Money STD
		{
			get
			{
				Money result = Money.Empty;
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					result = CurrencyConverter.Add(result, new Money(invoiceLine.AddInfo.ZA_STD, JobDeclaration.GetLocalCurrency()));
				}
				return result;
			}
		}

		public Money WET
		{
			get
			{
				Money result = Money.Empty;
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					result = CurrencyConverter.Add(result, new Money(invoiceLine.AddInfo.ZA_WET, JobDeclaration.GetLocalCurrency()));
				}
				return result;
			}
		}
		#endregion

		#region AddInfo Assay Code

		public ZDecimal AssayAU_Hidden => RandomLine?.AddInfo.ZA_AssayAU_Hidden ?? ZDecimal.Zero;

		public ZDecimal AssayNI_Hidden => RandomLine?.AddInfo.ZA_AssayNI_Hidden ?? ZDecimal.Zero;

		public ZDecimal AssaySN_Hidden => RandomLine?.AddInfo.ZA_AssaySN_Hidden ?? ZDecimal.Zero;

		public ZDecimal AssayPT_Hidden => RandomLine?.AddInfo.ZA_AssayPT_Hidden ?? ZDecimal.Zero;

		public ZDecimal AssayPB_Hidden => RandomLine?.AddInfo.ZA_AssayPB_Hidden ?? ZDecimal.Zero;

		public ZDecimal AssayCU_Hidden => RandomLine?.AddInfo.ZA_AssayCU_Hidden ?? ZDecimal.Zero;

		public ZDecimal AssayAG_Hidden => RandomLine?.AddInfo.ZA_AssayAG_Hidden ?? ZDecimal.Zero;

		public ZDecimal AssayWO_Hidden => RandomLine?.AddInfo.ZA_AssayWO_Hidden ?? ZDecimal.Zero;

		public ZDecimal AssayZN_Hidden => RandomLine?.AddInfo.ZA_AssayZN_Hidden ?? ZDecimal.Zero;

		#endregion

		#region Validation

		protected override CusEntryLineValidation GetNewValidation()
		{
			return new CusEntryLineValidation(this);
		}

		public new CusEntryLineValidation Validation
		{
			get { return base.Validation; }
		}

		#endregion

		#region AQIS Package Number and Type

		public AQISPackageCollection OrderedAQISPackages => Factory.GetValue(ref cachedAQISPacakges, GetOrderedAQISPackages);

		CachedProperty<AQISPackageCollection> cachedAQISPacakges;

		AQISPackageCollection GetOrderedAQISPackages()
		{
			AQISPackageCollection result = new AQISPackageCollection(Factory, null);

			foreach (JobComInvoiceLine currentInvoiceLine in InvoiceLines)
			{
				foreach (AQISPackage package in currentInvoiceLine.AQISPackages)
				{
					AQISPackage packageInCollection = result.FindAQISPackageByType(package.Type);

					if (packageInCollection != null)
					{
						packageInCollection.Number += package.Number;
					}
					else
					{
						AQISPackage newPackage = result.AddNew();
						newPackage.Type = package.Type;
						newPackage.Number = package.Number;

						if (result.Count > 10)
						{
							ErrorReporter.ReportOnce("AQISPackageCollection", "Added more than 10 items to the list");
						}
					}
				}
			}
			result.SortByUniqueCode();
			return result;
		}

		#endregion

		#region ICPQALineAttachee Members

		CPQuestionKeys ICPQALineAttachee.CPQuestionKey
		{
			get
			{
				CPQuestionKeys result = new CPQuestionKeys();
				JobComInvoiceLine randomLine = this.RandomLine;
				if (randomLine != null)
				{
					result.TariffNumber = randomLine.TariffNumber;
					result.StatCode = randomLine.StatCode;
					result.OriginCode = randomLine.AggregatedZA_ORG;
				}
				result.ModeOfTransport = GetAbbreviatedTransportMode();
				result.Nature = NatureTypeForCMR;
				result.HasValidOriginOrNatureOrModeOfTransport = true;
				return result;
			}
		}

		ZString GetAbbreviatedTransportMode()
		{
			ZString result = "";
			JobDeclaration declaration = Header.Declaration;
			if (declaration != null)
			{
				if (declaration.IsAir)
				{
					result = TransportModeAbbreviation.Air;
				}
				else if (declaration.IsSea)
				{
					result = TransportModeAbbreviation.Sea;
				}
				else if (declaration.IsPost)
				{
					result = TransportModeAbbreviation.Post;
				}
				else
				{
					result = TransportModeAbbreviation.Other;
				}
			}
			return result;
		}

		SchemaGuidColumn ICPQAAttachee.FKColumnInCusEntryCPDecTable
		{
			get { return CusEntryCPDecSchema.ON_CL; }
		}

		ZDateTime ICPQAAttachee.SelectionDate
		{
			get { return ((ICPQAAttachee)Header).SelectionDate; }
		}

		[ChildEditable(true)]
		public CMRCusEntryCPDecCollection Questions
		{
			get
			{
				if (fQuestions == null)
				{
					fQuestions = new CMRCusEntryCPDecCollection(this);
					fQuestions.Load();
					RegisterEditableChildObject(fQuestions);
				}
				return fQuestions;
			}
		}
		CMRCusEntryCPDecCollection fQuestions;

		ICPQALineAttachee[] ICPQALineAttachee.SourcesToDefault
		{
			get
			{
				ArrayList result = new ArrayList();
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					var pivot = invoiceLine.Pivot;
					if (pivot != null && !result.Contains(pivot))
					{
						result.Add(pivot);
					}
					var classification = invoiceLine.Classification;
					if (classification != null && !result.Contains(classification))
					{
						result.Add(classification);
					}
					var importer = invoiceLine.Declaration.Importer;
					if (importer != null && !result.OfType<OrganisationCPQA>().Any(x => x.Organisation == importer))
					{
						result.Add(new OrganisationCPQA(invoiceLine.Declaration.Importer));
					}
				}
				return (ICPQALineAttachee[])result.ToArray(typeof(ICPQALineAttachee));
			}
		}

		LineDefaultQuestions ICPQALineAttachee.DefaultUniqueQuestions => Factory.GetValue(ref cachedDefaultUniqueQuestions, delegate
		{
			return DefaultQuestionGenerator.GenerateUniqueDefaultQuestions(this, RandomLine?.DateOfValuation ?? ZDateTime.Now);
		});

		CachedProperty<LineDefaultQuestions> cachedDefaultUniqueQuestions;

		LineDefaultQuestionGenerator DefaultQuestionGenerator
		{
			get
			{
				if (defaultQuestionGenerator == null)
				{
					defaultQuestionGenerator = new LineDefaultQuestionGenerator();
				}
				return defaultQuestionGenerator;
			}
		}
		LineDefaultQuestionGenerator defaultQuestionGenerator;

		ZString ICPQALineAttachee.TableCode
		{
			get { return CusEntryLineSchema.Constants.Prefix; }
		}

		ZBool ICPQALineAttachee.IsRiskCalculatedFromTariff
		{
			get { return true; }
		}

		ZBool ICPQALineAttachee.IsRiskHistorySupported
		{
			get { return false; }
		}

		#endregion

		#region ICMRDutyData Members

		ZDecimal ICMRDutyData.CustomsValue
		{
			get { return CL_CustomsValue; }
		}

		ZDecimal ICMRDutyData.TransportAndInsuranceInAUD
		{
			get
			{
				if (EntryLineAddInfo.ZA_TILV.IsEmpty)
				{
					return TransportAndInsuranceInLocalCurrency.Amount;
				}
				else
				{
					return EntryLineAddInfo.TILVInAUD;
				}
			}
		}

		ZDecimal ICMRDutyData.FirstQty
		{
			get { return Quantity; }
		}

		ZDecimal ICMRDutyData.SecondQty
		{
			get { return SecondQuantity; }
		}

		ZDecimal ICMRDutyData.OtherDutyFactor
		{
			get
			{
				ZDecimal result = 0m;
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					result += invoiceLine.AddInfo.ZA_ODF;
				}
				return result;
			}
		}

		public Money ManualDutyAmount
		{
			get
			{
				ZDecimal result = 0m;
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					result += invoiceLine.AddInfo.ZA_DTY;
				}
				return new Money(result, JobDeclaration.GetLocalCurrency());
			}
		}

		DutyDataFromInvoiceLine ICMRDutyData.RandomLineDutyData
		{
			get
			{
				JobComInvoiceLine invoiceLine = RandomLine;
				return invoiceLine != null ? new CMRDutyWrapperForInvoiceLine(invoiceLine, false).RandomLineDutyData : new DutyDataFromInvoiceLine();
			}
		}

		bool ICMRDutyData.IsSubjectToDutyAndTax
		{
			get { return Header != null && Header.IsSubjectToDutyAndTax && !IsSecurityClaimed; }
		}

		bool ICMRDutyData.IsNotLowValueShipment
		{
			get { return Header != null && Header.IsSubjectToDutyAndTax; }
		}

		public bool IsDutyAndTaxEstimatedForWH
		{
			get { return RandomLine != null && RandomLine.IsDutyAndTaxEstimatedForWH; }
		}

		internal bool IsSecurityClaimed
		{
			get { return RandomLine != null && Header != null && Header.SecurityRequiredTreatmentCodes.Contains((string)RandomLine.TreatmentCode); }
		}

		bool ICMRDutyData.IsGSTDeferred
		{
			get { return Declaration?.Importer?.MiscServ.IsGSTVATDeferred ?? false; }
		}

		public ZDecimal DeferrableFeesAndCharges
		{
			get
			{
				var result = ZDecimal.Zero;

				foreach (CusEntryLineFee fee in Fees)
				{
					if (fee.CF_ChargeType != CusEntryChargeTypeList.Codes.DutyAmount && fee.IsDeferrable)
					{
						result += fee.CF_ChargeAmount;
					}
				}

				return result;
			}
		}

		#endregion

		#region ILineDutyData Members

		void Common.ILineDutyData.SetDutyResult(Common.DutyResult dutyResult)
		{
			CL_DutyPercent = dutyResult.Percent;
			CL_FlatAmount = dutyResult.FlatRateAmount;
			CL_FlatAmountUQ = dutyResult.FlatRateUQ;

			Fees.SetAmount(CusEntryChargeTypeList.Codes.DutyAmount, dutyResult.Amount.Amount);
		}

		void Common.ILineDutyData.SetFeeResult(ZString feeType, ZDecimal feeAmount)
		{
			Fees.SetAmount(feeType, feeAmount);
		}

		#endregion

		#region ICusEntryLine Members

		public ZString StatCodeForCMR
		{
			get { return StatCode.IsEmpty ? ZString.Empty : StatCode.PadLeft(2, '0'); }
		}

		public bool IsGeneralRate
		{
			get { return RandomLine.IsGeneralRate; }
		}

		public ZDecimal SecondCustomsQuantity
		{
			get { return SecondQuantity; }
		}

		public ZString SecondCustomsUnitQty
		{
			get { return RandomLine.AddInfo.ZA_UQ2; }
		}

		public bool SendZeroManualDuty
		{
			get { return RandomLine.AddInfo.ZA_SendZeroDutyOverride_Hidden; }
		}

		public ZString GSTE
		{
			get
			{
				ZString result = "";
				if (!RandomLine.DoesTariffRateOrTreatmentCodeDeemGSTExemption)
				{
					result = (ZString)RandomLine.AggregatedValue(AUAddInfo.Schema.ZA_GSTE);
				}
				return result;
			}
		}

		public ZString WETE
		{
			get { return RandomLine.AddInfo.ZA_WETE; }
		}

		public ZString WETQ
		{
			get { return RandomLine.AddInfo.ZA_WETQ; }
		}

		public ZString WMC
		{
			get { return RandomLine.AddInfo.ZA_WMC; }
		}

		public char[] OrderedAMBs
		{
			get
			{
				string aggregatedAMB = (ZString)RandomLine.AddInfo.AggregatedValue(AUAddInfo.Schema.ZA_AMB);

				ArrayList orderedResult = new ArrayList();
				orderedResult.AddRange(aggregatedAMB.ToCharArray());
				orderedResult.Sort(new CaseInsensitiveComparer());

				return (char[])orderedResult.ToArray(typeof(char));
			}
		}

		public ZString ORG
		{
			get { return RandomLine.AggregatedZA_ORG; }
		}

		public ZString PRF
		{
			get { return RandomLine.AggregatedZA_PRF; }
		}

		public ZString POC
		{
			get { return (ZString)RandomLine.AddInfo.AggregatedValue(AUAddInfo.Schema.ZA_POC); }
		}

		public ZString DCX
		{
			get { return RandomLine.AddInfo.ZA_DCX; }
		}

		public ZDateTime FOD
		{
			get { return RandomLine.AddInfo.FOD; }
		}

		public ZDecimal WRQ
		{
			get
			{
				ZDecimal result = 0;
				foreach (JobComInvoiceLine line in InvoiceLines)
				{
					result += line.AddInfo.ZA_WRQ;
				}
				return result;
			}
		}

		public ZString WRU
		{
			get { return RandomLine.AddInfo.ZA_WRU; }
		}

		public Money StandardDutyOverriden
		{
			get
			{
				ZDecimal result = 0m;
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					result += invoiceLine.AddInfo.ZA_STD;
				}
				return new Money(result, JobDeclaration.GetLocalCurrency());
			}
		}

		public OrgHeader Supplier => RandomLine.Supplier;

		public OrgAddress SupplierAddress => RandomLine.InvoiceHeader?.SupplierAddress;

		public ZString SupplierCode => Supplier?.GetCustomsClientID(SupplierAddress) ?? ZString.Empty;

		public ZString ConsignorVendor => CargoHelper.GetConsignorVendor(Supplier);

		public ZString VAN
		{
			get { return (ZString)RandomLine.AddInfo.AggregatedValue(AUAddInfo.Schema.ZA_VAN); }
		}

		public ZDecimal LCP
		{
			get { return RandomLine.AddInfo.ZA_LCP; }
		}

		public ZDecimal ISS
		{
			get { return RandomLine.AddInfo.ZA_ISS; }
		}

		public ZString SCN
		{
			get { return (ZString)RandomLine.AddInfo.AggregatedValue(AUAddInfo.Schema.ZA_SCN); }
		}

		public ZString PST
		{
			get { return (ZString)RandomLine.AddInfo.AggregatedValue(AUAddInfo.Schema.ZA_PST); }
		}

		public ZString PRT
		{
			get { return (ZString)RandomLine.AddInfo.AggregatedValue(AUAddInfo.Schema.ZA_PRT); }
		}

		public ZString WRN
		{
			get { return (ZString)RandomLine.AddInfo.AggregatedValue(AUAddInfo.Schema.ZA_WRN); }
		}

		public ZString DSN
		{
			get { return RandomLine.AddInfo.ZA_DSN; }
		}

		public ZString LCTE
		{
			get { return RandomLine.AddInfo.ZA_LCTE; }
		}

		public ZString TR2
		{
			get { return RandomLine.AddInfo.ZA_TR2; }
		}

		public ZString CL2
		{
			get { return RandomLine.AddInfo.ZA_CL2; }
		}

		public ZString TRN
		{
			get { return RandomLine.AddInfo.ZA_TRN; }
		}

		public ZString ISC
		{
			get { return RandomLine.AddInfo.ZA_ISC; }
		}

		public ZString TAN
		{
			get { return RandomLine.AddInfo.ZA_TAN; }
		}

		public ZString RNO
		{
			get { return RandomLine.AddInfo.ZA_RNO; }
		}

		public ZString ICN
		{
			get { return RandomLine.AddInfo.ZA_ICN.Trim(); }
		}

		public ZString TCI_InstrumentType
		{
			get { return RandomLine.AddInfo.TCI_InstrumentType; }
		}

		public ZString TI2_InstrumentType
		{
			get { return RandomLine.AddInfo.TI2_InstrumentType; }
		}

		public ZString PRI_InstrumentType
		{
			get { return RandomLine.AddInfo.PRI_InstrumentType; }
		}

		public ZString DXT
		{
			get { return RandomLine.AddInfo.ZA_DXT; }
		}

		public ZString InstrumentCode
		{
			get { return RandomLine.InstrumentCode; }
		}

		public ZString InstrumentType
		{
			get { return RandomLine.InstrumentType; }
		}

		public ZString[] OrderedELAs
		{
			get
			{
				return SortByCaseInsensitiveComparer(RandomLine.AddInfo.ZA_ELA.Replace(" ", "").Split(','));
			}
		}

		public ZDecimal DRE
		{
			get { return RandomLine.AddInfo.ZA_DRE; }
		}

		public ZString REL
		{
			get
			{
				ZString result = ZString.Empty;
				if (Declaration != null && !Declaration.IsExWarehouse)
				{
					if ((RandomLine.AddInfo.ZA_REL_Hidden.IsEmpty || RandomLine.AddInfo.ZA_REL_Hidden == CMRRelatedTransaction.Default.Code) && RandomLine.InvoiceHeader != null)
					{
						result = RandomLine.InvoiceHeader.AddInfo.ZA_HeaderREL_Hidden == CMRRelatedTransaction.Yes.Code ? CMRRelatedTransaction.Yes.Code : CMRRelatedTransaction.No.Code;
					}
					else
					{
						result = RandomLine.AddInfo.ZA_REL_Hidden;
					}
				}
				return result;
			}
		}

		public ZString SEC
		{
			get { return RandomLine.AddInfo.ZA_SEC; }
		}

		public CusContainersInvoiceLinesCollection ContainersPivot
		{
			get { return RandomLine.ContainersPivot; }
		}

		public ZString ValuationBasisForCMR
		{
			get { return RandomLine.AddInfo.AggregatedZA_VALB_Hidden; }
		}

		public ZInt WRL
		{
			get { return (ZInt)RandomLine.AddInfo.AggregatedValue(AUAddInfo.Schema.ZA_WRL); }
		}

		public ZString[] OrderedVIDs
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					string oneVID = GetVIDFromInvoiceLine(invoiceLine);
					if (!string.IsNullOrEmpty(oneVID))
					{
						result.Append(oneVID + ",");
					}
				}

				ZString finalResult = new ZString(result.ToString().Replace(" ", "")).TrimEnd(',');
				return SortByCaseInsensitiveComparer(finalResult.Split(','));
			}
		}

		ZString GetVIDFromInvoiceLine(JobComInvoiceLine invoiceLine)
		{
			ZString result = invoiceLine.AddInfo.ZA_VID;
			var declaration = Declaration;
			var partAttribute = declaration?.Importer?.PartAttributeManager?.VINPartAttribute;
			if (partAttribute != null)
			{
				if (result.IsEmpty || (declaration.SupportsBondedWarehousing && invoiceLine.Part != null &&
					(declaration.IsWarehousedByExternalAgent || declaration.IsExWarehouse)))
				{
					int attributeIndex = partAttribute.Index;

					if (attributeIndex == 1)
					{
						result = invoiceLine.JI_PartAttrib1;
					}
					else if (attributeIndex == 2)
					{
						result = invoiceLine.JI_PartAttrib2;
					}
					else if (attributeIndex == 3)
					{
						result = invoiceLine.JI_PartAttrib3;
					}
				}
			}

			return result;
		}

		public ZString LCTI
		{
			get { return RandomLine.AddInfo.ZA_LCTI; }
		}

		public ZString LCTQ
		{
			get { return RandomLine.AddInfo.ZA_LCTQ; }
		}

		public ZString MLPI
		{
			get { return RandomLine.AddInfo.ZA_MLPI; }
		}

		public ZString PUP
		{
			get { return RandomLine.AddInfo.ZA_PUP; }
		}

		public bool IsExWarehouse
		{
			get { return Declaration.IsExWarehouse; }
		}

		public ZString TCI_InstrumentNo
		{
			get { return RandomLine.AddInfo.TCI_InstrumentNo; }
		}

		public ZString TI2_InstrumentNo
		{
			get { return RandomLine.AddInfo.TI2_InstrumentNo; }
		}

		public ZString PRI_InstrumentNo
		{
			get { return RandomLine.AddInfo.PRI_InstrumentNo; }
		}

		public ZString ActionCodeForMessage
		{
			get
			{
				if (Header != null && CL_LineNumber > Header.CH_HighestLineNumber)
				{
					return LineAction.Insert;
				}
				else
				{
					return LineAction.Amend;
				}
			}
		}

		public ZString RefundReasonCode
		{
			get { return EntryLineAddInfo.ZA_RRC_Hidden; }
			set
			{
				EntryLineAddInfo.ZA_RRC_Hidden = value;
			}
		}

		public ZPropertyInfo RefundReasonCodeInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(RefundReasonCode), x => EntryLineAddInfo.ZA_RRC_HiddenInfo); }
		}

		public AQISDocumentCollection OrderedAQISDocuments
		{
			get
			{
				AQISDocumentCollection result = null;

				if (RandomLine != null)
				{
					result = RandomLine.AggreatedAQISDocuments;
					result.SortByUniqueCode();
				}
				else
				{
					result = new AQISDocumentCollection(Factory, EntryLineAddInfo);
				}

				return result;
			}
		}

		public AQISPremisesIdAndProcessingTypeCollection OrderedAQISPremisesIdAndProcessingTypes
		{
			get
			{
				AQISPremisesIdAndProcessingTypeCollection result = null;

				if (RandomLine != null)
				{
					result = RandomLine.AggreatedAQISPremisesIdAndProcessingTypes;
					result.SortByUniqueCode();
				}
				else
				{
					result = new AQISPremisesIdAndProcessingTypeCollection(Factory, EntryLineAddInfo);
				}

				return result;
			}
		}

		public AQISCommodityCodeCollection OrderedAQISCommodityCodes
		{
			get
			{
				AQISCommodityCodeCollection result = null;

				if (RandomLine != null)
				{
					result = RandomLine.AggreatedAQISCommodityCodes;
					result.SortByUniqueCode();
				}
				else
				{
					result = new AQISCommodityCodeCollection(Factory);
				}

				return result;
			}
		}

		public AQISEntityIdCollection OrderedAQISEntityIds
		{
			get
			{
				AQISEntityIdCollection result = null;

				if (RandomLine != null)
				{
					result = RandomLine.AggreatedAQISEntityIds;
					result.SortByUniqueCode();
				}
				else
				{
					result = new AQISEntityIdCollection(Factory);
				}

				return result;
			}
		}

		public AQISProducerCodeCollection OrderedAQISProducerCodes
		{
			get
			{
				AQISProducerCodeCollection result = null;

				if (RandomLine != null)
				{
					result = RandomLine.AggreatedAQISProducerCodes;
					result.SortByUniqueCode();
				}
				else
				{
					result = new AQISProducerCodeCollection(Factory);
				}

				return result;
			}
		}

		public AQISPermitIdCollection OrderedAQISPermitIds
		{
			get
			{
				AQISPermitIdCollection result = null;

				if (RandomLine != null)
				{
					result = RandomLine.AggreatedAQISPermitIds;
					result.SortByUniqueCode();
				}
				else
				{
					result = new AQISPermitIdCollection(Factory);
				}

				return result;
			}
		}

		ZString[] SortByCaseInsensitiveComparer(ZString[] strings)
		{
			ArrayList orderedResult = new ArrayList();
			orderedResult.AddRange(strings);
			orderedResult.Sort(new CaseInsensitiveComparer());
			return (ZString[])orderedResult.ToArray(typeof(ZString));
		}

		ZString ICusEntryLine.RefundReasonCode
		{
			get { return RefundReasonCode.IsEmpty && Header != null ? Header.RefundReasonCode : RefundReasonCode; }
		}

		ZString ICusEntryLine.WAR
		{
			get
			{
				var result = ZString.Empty;
				var randomLine = RandomLine;
				if (randomLine != null && !randomLine.JI_IsPackToBondForLine)
				{
					result = randomLine.AddInfo.ZA_WAR;
				}

				return result;
			}
		}

		IEnumerable<ZString> ICusEntryLine.ImportPermitNumbers
		{
			get
			{
				IEnumerable<ZString> result = RandomLine?.ICSPermits.Select(x => x.CY_Data).Distinct().OrderBy(x => x);
				return result ?? Enumerable.Empty<ZString>();
			}
		}

		#endregion

		#region IAddInfo Members

		public AUAddInfo EntryLineAddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new CusEntryLineAddInfo(this, CL_AddInfoInfo);
					RegisterEditableChildObject(fAddInfo);
				}
				return fAddInfo;
			}
		}
		AUAddInfo fAddInfo;

		AUAddInfo IAddInfo.AddInfo
		{
			get { return EntryLineAddInfo; }
		}

		#endregion

		#region IAddInfoManager Members

		Customs.Business.IAddInfo IAddInfoManager.AddInfo
		{
			get { return EntryLineAddInfo; }
		}

		#endregion

		#region IAggregatedAddInfo Members

		ZString IAggregatedAddInfo.AggregatedZA_ORG
		{
			get
			{
				JobComInvoiceLine randomLine = this.RandomLine;
				return randomLine == null ? ZString.Empty : randomLine.AggregatedZA_ORG;
			}
		}

		ZString IAggregatedAddInfo.AggregatedZA_PRF
		{
			get
			{
				JobComInvoiceLine randomLine = this.RandomLine;
				return randomLine == null ? ZString.Empty : randomLine.AggregatedZA_PRF;
			}
		}

		IZType IAggregatedAddInfo.AggregatedValue(string propertyName)
		{
			return (IZType)EntryLineAddInfo[propertyName];
		}

		bool IAggregatedAddInfo.IsCopying
		{
			get { return IsCopying; }
		}

		#endregion

		#region IDrawbackEntryLine Members

		ZDateTime IDrawbackEntryLine.DeclarationDate
		{
			get { return Header?.DeclarationDate ?? ZDateTime.Empty; }
		}

		ZString IDrawbackEntryLine.EntryNumber
		{
			get { return Header?.EntryNumber ?? ZString.Empty; }
		}

		#endregion

		#region FlatRateDescription

		public ZString FlatRateDescription
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();
				if (CL_FlatAmount != 0m)
				{
					result.Append(CL_FlatAmount.ToString(5));
				}
				if (!result.IsEmpty)
				{
					result.Append("/");
				}
				result.Append(CL_FlatAmountUQ);
				return result.ToString();
			}
		}

		public ZPropertyInfo FlatRateDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(FlatRateDescription)); }
		}

		#endregion

		#region Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : Customs.Business.FetchStrategies.CusEntryLineFetchStrategy
		{
			public Strategy(CusEntryLine cusEntryLine)
				: base(cusEntryLine)
			{
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(CusEntryCPDecSchema.ON_CL, BusinessObject.PK);
			}

			protected override void FetchForDeleteCore()
			{
				base.FetchForDeleteCore();
				Factory.AddFetchHint(CusEntryCPDecSchema.ON_CL, BusinessObject.PK);
				Factory.AddFetchHint(CusEntryLineFeeSchema.CF_CL, BusinessObject.PK);
			}
		}

		#endregion
	}
}
