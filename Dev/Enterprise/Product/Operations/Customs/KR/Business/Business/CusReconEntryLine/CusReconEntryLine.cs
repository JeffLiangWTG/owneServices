using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class CusReconEntryLine : Customs.Business.CusReconEntryLine,
		Integration.Customs.KR.ICusReconEntryLine,
		Integration.Customs.ICusSupportingInfoTypeSupporter
	{
		public CusReconEntryLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoCusReconEntryLine.Schema
		{
			public const int CRL_LineNumberMaxLength = 4;
			public const int CRL_OriginalEntryLineNumberMaxLength = 3;
		}
		public CusReconEntry Header
		{
			get
			{
				if (fHeader?.PK != CRL_CRE)
				{
					fHeader = Factory.Load<CusReconEntry>(CRL_CRE);
				}

				return fHeader;
			}
		}
		CusReconEntry fHeader;

		[ChildEditable(true)]
		public CusReconCustomsChargeCollection CusReconCharges
		{
			get
			{
				if (cusReconCharges == null)
				{
					cusReconCharges = new CusReconCustomsChargeCollection(this);
					RegisterEditableChildObject(cusReconCharges);
				}
				return cusReconCharges;
			}
		}
		CusReconCustomsChargeCollection cusReconCharges;

		[ChildEditable(true)]
		public ContractRevocation5ULCollection ContractRevocations
		{
			get
			{
				if (contractRevocations == null)
				{
					contractRevocations = new ContractRevocation5ULCollection(this);
					contractRevocations.Load();
					RegisterEditableChildObject(contractRevocations);
				}
				return contractRevocations;
			}
		}
		ContractRevocation5ULCollection contractRevocations;

		public ContractRevocation5UL ContractRevocation
		{
			get
			{
				if (contractRevocation?.IsDeleted ?? true)
				{
					contractRevocation = ContractRevocations.FirstOrDefault();
				}
				return contractRevocation;
			}
		}
		ContractRevocation5UL contractRevocation;

		[ChildEditable(true)]
		public RefundInvoiceLineCollection RefundInvoiceLines
		{
			get
			{
				if (refundInvoiceLines == null)
				{
					refundInvoiceLines = new RefundInvoiceLineCollection(this);
					refundInvoiceLines.Load();
					RegisterEditableChildObject(refundInvoiceLines);
				}
				return refundInvoiceLines;
			}
		}
		RefundInvoiceLineCollection refundInvoiceLines;

		ZDecimal GetReconChargeAmount(string type)
		{
			return CusReconCharges.FirstOrDefault(x => x.CRC_ChargeType == type)?.CRC_Amount ?? 0m;
		}

		CusReconCustomsCharge GetOrCreateCusReconCharge(string type)
		{
			var charge = CusReconCharges.FirstOrDefault(x => x.CRC_ChargeType == type);
			if (charge == null)
			{
				charge = CusReconCharges.AddNew();
				charge.CRC_ChargeType = type;
			}
			return charge;
		}

		[MaxLength(Schema.CRL_LineNumberMaxLength)]
		[ResourceStringData("7C17C5BB-749E-4D5F-9F46-08C81E1398E9", Caption = "Seq #")]
		[ReadOnly(true)]
		public override ZShort CRL_LineNumber { get => base.CRL_LineNumber; set => base.CRL_LineNumber = value; }

		public ZBool HasImportDetails => (Header?.CRE_OriginalEntryNumber ?? ZString.Empty) == ZString.Empty || RefundInvoiceLines.Count > 0;
		[ReadOnlyMember(nameof(HasImportDetails))]
		[MaxLength(Schema.CRL_OriginalEntryLineNumberMaxLength)]
		public override ZShort CRL_OriginalEntryLineNumber { get => base.CRL_OriginalEntryLineNumber; set => base.CRL_OriginalEntryLineNumber = value; }

		[ReadOnlyMember(nameof(HasImportDetails))]
		[BusinessObjectMaxLengthTestExclude]
		[MaxLength(Schema.CRL_OriginalEntryLineNumberMaxLength)]
		[List(nameof(Lookups) + "." + nameof(CusReconEntryLineLookups.ImportEntryLineNumbers))]
		[ResourceStringData("114B4FA9-FBE9-40D9-B8F4-B0807CF5D76F", Caption = "IMP Entry Line No.")]
		public ZString FormattedOriginalEntryLineNumber
		{
			get => base.CRL_OriginalEntryLineNumber == ZShort.Zero ? ZString.Empty : base.CRL_OriginalEntryLineNumber.ToString();
			set
			{
				if(string.IsNullOrWhiteSpace(value))
				{
					base.CRL_OriginalEntryLineNumber = ZShort.Zero;
				}
				if (short.TryParse(value.ToString(), out short parsed))
				{
					base.CRL_OriginalEntryLineNumber = parsed;
				}
				CheckMaximumLength(FormattedOriginalEntryLineNumberInfo, value);
				FormattedOriginalEntryLineNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo FormattedOriginalEntryLineNumberInfo => GetZPropertyInfo(nameof(FormattedOriginalEntryLineNumber));

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("9062A7EE-A6DA-44EE-A64B-27F9F5589DE9", Caption = "Duty Amount (Refund)")]
		public ZDecimal DutyToRefund
		{
			get => GetReconChargeAmount(ChargeTypeList.Codes.Duty);
			set
			{
				GetOrCreateCusReconCharge(ChargeTypeList.Codes.Duty).CRC_Amount = value;
				DutyToRefundInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateDutyToRefund();
				}
			}
		}
		public ZPropertyInfo DutyToRefundInfo => GetZPropertyInfo(nameof(DutyToRefund));

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("2E80728B-581C-4BE3-BC96-95C92460259B", Caption = "Duty Amount Penalty (Refund)")]
		public ZDecimal DutyPenaltyToRefund
		{
			get => GetReconChargeAmount(EntryTaxTypeList.Codes._5AD);
			set
			{
				GetOrCreateCusReconCharge(EntryTaxTypeList.Codes._5AD).CRC_Amount = value;
				DutyPenaltyToRefundInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateDutyPenaltyToRefund();
				}
			}
		}
		public ZPropertyInfo DutyPenaltyToRefundInfo => GetZPropertyInfo(nameof(DutyPenaltyToRefund));

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("E0B2F476-C192-4B60-A2F5-07BC339503F6", Caption = "Special Consumption Tax (Refund)")]
		public ZDecimal SCTToRefund
		{
			get => GetReconChargeAmount(ChargeTypeList.Codes.SpecialConsumptionTax);
			set
			{
				GetOrCreateCusReconCharge(ChargeTypeList.Codes.SpecialConsumptionTax).CRC_Amount = value;
				SCTToRefundInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateSCTToRefund();
				}
			}
		}
		public ZPropertyInfo SCTToRefundInfo => GetZPropertyInfo(nameof(SCTToRefund));

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("6D0A05F8-A10A-4002-8221-C75BE12A690D", Caption = "Special Consumption Penalty (Refund)")]
		public ZDecimal SCTPenaltyToRefund
		{
			get => GetReconChargeAmount(EntryTaxTypeList.Codes._5AE);
			set
			{
				GetOrCreateCusReconCharge(EntryTaxTypeList.Codes._5AE).CRC_Amount = value;
				SCTPenaltyToRefundInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateSCTPenaltyToRefund();
				}
			}
		}
		public ZPropertyInfo SCTPenaltyToRefundInfo => GetZPropertyInfo(nameof(SCTPenaltyToRefund));

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("3D295E56-48B6-40C4-B19B-BE490D7E09A9", Caption = "Transportation Tax (Refund)")]
		public ZDecimal TRTToRefund
		{
			get => GetReconChargeAmount(ChargeTypeList.Codes.TransportationTax);
			set
			{
				GetOrCreateCusReconCharge(ChargeTypeList.Codes.TransportationTax).CRC_Amount = value;
				TRTToRefundInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateTRTToRefund();
				}
			}
		}
		public ZPropertyInfo TRTToRefundInfo => GetZPropertyInfo(nameof(TRTToRefund));

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("1DBF2AC8-1C6D-4091-980F-3C909234AB02", Caption = "Transportation Penalty (Refund)")]
		public ZDecimal TRTPenaltyToRefund
		{
			get => GetReconChargeAmount(EntryTaxTypeList.Codes._5AG);
			set
			{
				GetOrCreateCusReconCharge(EntryTaxTypeList.Codes._5AG).CRC_Amount = value;
				TRTPenaltyToRefundInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateTRTPenaltyToRefund();
				}
			}
		}
		public ZPropertyInfo TRTPenaltyToRefundInfo => GetZPropertyInfo(nameof(TRTPenaltyToRefund));

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("5989FE98-53F6-4F38-9BF2-342F883305DB", Caption = "Liquor Tax (Refund)")]
		public ZDecimal LQTToRefund
		{
			get => GetReconChargeAmount(ChargeTypeList.Codes.LiquorTax);
			set
			{
				GetOrCreateCusReconCharge(ChargeTypeList.Codes.LiquorTax).CRC_Amount = value;
				LQTToRefundInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateLQTToRefund();
				}
			}
		}
		public ZPropertyInfo LQTToRefundInfo => GetZPropertyInfo(nameof(LQTToRefund));

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("05128DE9-10D5-4600-8B56-EB5B2C3182EF", Caption = "Liquor Tax Penalty (Refund)")]
		public ZDecimal LQTPenaltyToRefund
		{
			get => GetReconChargeAmount(EntryTaxTypeList.Codes._5AF);
			set
			{
				GetOrCreateCusReconCharge(EntryTaxTypeList.Codes._5AF).CRC_Amount = value;
				LQTPenaltyToRefundInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateLQTPenaltyToRefund();
				}
			}
		}
		public ZPropertyInfo LQTPenaltyToRefundInfo => GetZPropertyInfo(nameof(LQTPenaltyToRefund));

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("4C58D47F-4965-4C07-A5F4-F1707875B879", Caption = "Education Tax (Refund)")]
		public ZDecimal EDTToRefund
		{
			get => GetReconChargeAmount(ChargeTypeList.Codes.EducationTax);
			set
			{
				GetOrCreateCusReconCharge(ChargeTypeList.Codes.EducationTax).CRC_Amount = value;
				EDTToRefundInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateEDTToRefund();
				}
			}
		}
		public ZPropertyInfo EDTToRefundInfo => GetZPropertyInfo(nameof(EDTToRefund));

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("CC0EB01B-3A2D-42B8-875F-1D08DF37F44A", Caption = "Education Tax Penalty (Refund)")]
		public ZDecimal EDTPenaltyToRefund
		{
			get => GetReconChargeAmount(EntryTaxTypeList.Codes._5AI);
			set
			{
				GetOrCreateCusReconCharge(EntryTaxTypeList.Codes._5AI).CRC_Amount = value;
				EDTPenaltyToRefundInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateEDTPenaltyToRefund();
				}
			}
		}
		public ZPropertyInfo EDTPenaltyToRefundInfo => GetZPropertyInfo(nameof(EDTPenaltyToRefund));

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("B98EEFE8-5808-470E-9C96-76B57813016B", Caption = "Agriculture Tax (Refund)")]
		public ZDecimal AGTToRefund
		{
			get => GetReconChargeAmount(ChargeTypeList.Codes.AgricultureTax);
			set
			{
				GetOrCreateCusReconCharge(ChargeTypeList.Codes.AgricultureTax).CRC_Amount = value;
				AGTToRefundInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateAGTToRefund();
				}
			}
		}
		public ZPropertyInfo AGTToRefundInfo => GetZPropertyInfo(nameof(AGTToRefund));

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("EA8F6895-A04B-4105-9ADD-1719480CA965", Caption = "Agriculture Tax Penalty (Refund)")]
		public ZDecimal AGTPenaltyToRefund
		{
			get => GetReconChargeAmount(EntryTaxTypeList.Codes._5AJ);
			set
			{
				GetOrCreateCusReconCharge(EntryTaxTypeList.Codes._5AJ).CRC_Amount = value;
				AGTPenaltyToRefundInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateAGTPenaltyToRefund();
				}
			}
		}
		public ZPropertyInfo AGTPenaltyToRefundInfo => GetZPropertyInfo(nameof(AGTPenaltyToRefund));

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("6BC05150-0DA7-41C3-BC97-B00729CB0A4F", Caption = "VAT (Refund)")]
		public ZDecimal VATToRefund
		{
			get => GetReconChargeAmount(ChargeTypeList.Codes.VAT);
			set
			{
				GetOrCreateCusReconCharge(ChargeTypeList.Codes.VAT).CRC_Amount = value;
				VATToRefundInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateVATToRefund();
				}
			}
		}
		public ZPropertyInfo VATToRefundInfo => GetZPropertyInfo(nameof(VATToRefund));

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("ADFFDF34-CCDB-45A5-83B7-95A723076DFF", Caption = "Value For VAT (Refund)")]
		public ZDecimal ValueForVAT
		{
			get
			{
				var result = ZDecimal.Zero;
				if (Header != null)
				{
					var effectiveDate = Header.CRE_EntryDate.IsValid ? Header.CRE_EntryDate : ZDateTime.Today;
					var rateForVTA = new RefCusTaxOrFee.Loader(Factory).LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.KoreaSouth, Messaging.Constants.ZZ.RefCusTaxOrFeeCodes.VATRateA, effectiveDate)?.ZZF_Value ?? ZDecimal.Zero;

					if (rateForVTA != 0)
					{
						result = VATToRefund / rateForVTA;
					}
				}
				return result;
			}
		}

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("2624C2A5-553B-4DFA-9E86-AF883A5391F8", Caption = "VAT Exemption Value (Refund)")]
		public ZDecimal VATExemptionValue => 0m;

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("B986645E-B6F4-466C-A678-D6963A81BAB5", Caption = "VAT Penalty (Refund)")]
		public ZDecimal VATPenaltyToRefund
		{
			get => GetReconChargeAmount(EntryTaxTypeList.Codes._5AH);
			set
			{
				GetOrCreateCusReconCharge(EntryTaxTypeList.Codes._5AH).CRC_Amount = value;
				VATPenaltyToRefundInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateVATPenaltyToRefund();
				}
			}
		}
		public ZPropertyInfo VATPenaltyToRefundInfo => GetZPropertyInfo(nameof(VATPenaltyToRefund));

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("57D2B5CC-11E9-45F1-802F-7BE83063B65D", Caption = "Penalty For Late Declaration (Refund)")]
		public ZDecimal PenaltyLateDecToRefund
		{
			get => GetReconChargeAmount(ChargeTypeList.Codes.PenaltyForLateDeclaration);
			set
			{
				GetOrCreateCusReconCharge(ChargeTypeList.Codes.PenaltyForLateDeclaration).CRC_Amount = value;
				PenaltyLateDecToRefundInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidatePenaltyLateDecToRefund();
				}
			}
		}
		public ZPropertyInfo PenaltyLateDecToRefundInfo => GetZPropertyInfo(nameof(PenaltyLateDecToRefund));

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("A7BA338C-316E-45F8-ACEE-D4E7490BB51B", Caption = "Penalty For Missed Declaration (Refund)")]
		public ZDecimal PenaltyMissedDecToRefund
		{
			get => GetReconChargeAmount(ChargeTypeList.Codes.PenaltyForMissedDeclaration);
			set
			{
				GetOrCreateCusReconCharge(ChargeTypeList.Codes.PenaltyForMissedDeclaration).CRC_Amount = value;
				PenaltyMissedDecToRefundInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidatePenaltyMissedDecToRefund();
				}
			}
		}
		public ZPropertyInfo PenaltyMissedDecToRefundInfo => GetZPropertyInfo(nameof(PenaltyMissedDecToRefund));

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("D662A62B-C39C-4A2A-95D1-73B96D5B86A2", Caption = "Penalty For Late Payment (Refund)")]
		public ZDecimal PenaltyLatePaymentToRefund
		{
			get => GetReconChargeAmount(ChargeTypeList.Codes.PenaltyForLatePayment);
			set
			{
				GetOrCreateCusReconCharge(ChargeTypeList.Codes.PenaltyForLatePayment).CRC_Amount = value;
				PenaltyLatePaymentToRefundInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidatePenaltyLatePaymentToRefund();
				}
			}
		}
		public ZPropertyInfo PenaltyLatePaymentToRefundInfo => GetZPropertyInfo(nameof(PenaltyLatePaymentToRefund));

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("3AB62F7F-82CE-4E17-B7D3-B212F1FC417A", Caption = "Non-Duty Tax Revenue (Refund)")]
		public ZDecimal NonDutyTaxRevenueToRefund
		{
			get => GetReconChargeAmount(ChargeTypeList.Codes.NonDutyTaxRevenue);
			set
			{
				GetOrCreateCusReconCharge(ChargeTypeList.Codes.NonDutyTaxRevenue).CRC_Amount = value;
				NonDutyTaxRevenueToRefundInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateNonDutyTaxRevenueToRefund();
				}
			}
		}
		public ZPropertyInfo NonDutyTaxRevenueToRefundInfo => GetZPropertyInfo(nameof(NonDutyTaxRevenueToRefund));

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("6FA5C7D9-9165-4546-A8F5-3D90458279AB", Caption = "Total Refund Tax")]
		public ZDecimal TotalLateRefundAmount => PenaltyLateDecToRefund + PenaltyMissedDecToRefund + PenaltyLatePaymentToRefund + NonDutyTaxRevenueToRefund;

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		public ZDecimal TotalPenaltyToRefund => DutyPenaltyToRefund + LQTPenaltyToRefund + SCTPenaltyToRefund
											  + TRTPenaltyToRefund + EDTPenaltyToRefund + AGTPenaltyToRefund + VATPenaltyToRefund;

		public override void Delete()
		{
			var header = Header;//access before being deleted
								//when base.Delete() is done, this object will be removed from all collections.
								//CusReconEntry.Delete deleting all rows of CusReconEntryLine will not cause further problems. 

			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				CusReconCharges.DeleteAll();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}
			base.Delete();
			if (header != null && !header.IsDeleted)
			{
				header.Delete();
			}
		}

		public new CusReconEntryLineLookups Lookups => (CusReconEntryLineLookups)base.Lookups;
		protected override Customs.Business.CusReconEntryLineLookups GetNewLookups() => new CusReconEntryLineLookups(this);
		protected override Customs.Business.CusReconEntryLineValidation GetNewValidation() => new CusReconEntryLineValidation(this);
		public new CusReconEntryLineValidation Validation => (CusReconEntryLineValidation)base.Validation;
		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new CusReconEntryLineFetchStrategy(this);
		public CusReconSnapshot FirstSnapShot
		{
			get
			{
				if (firstSnapShot == null)
				{
					firstSnapShot = CusReconSnapshots.Cast<CusReconSnapshot>().FirstOrDefault();
				}
				return firstSnapShot;
			}
		}
		CusReconSnapshot firstSnapShot;

		ImportEntryOrEntryLineSerializable ImportEntryOrEntryLine => Header?.FirstSnapShot?.ImportEntryOrEntryLine ?? FirstSnapShot?.ImportEntryOrEntryLine;

		public void DefaultAmountsToRefund()
		{
			var amountsOfDifference = ImportEntryOrEntryLine?.RefundAmounts.FirstOrDefault(x => x.VersionNumber == Header.CRE_Amendment5WNVersionNumber);
			if (amountsOfDifference != null)
			{
				DutyToRefund = amountsOfDifference.RefundAmounts.DutyAmount;
				LQTToRefund = amountsOfDifference.RefundAmounts.LiquorTaxAmount;
				SCTToRefund = amountsOfDifference.RefundAmounts.SpecialConsumptionTaxAmount;
				TRTToRefund = amountsOfDifference.RefundAmounts.TransportTaxAmount;
				EDTToRefund = amountsOfDifference.RefundAmounts.EducationTaxAmount;
				AGTToRefund = amountsOfDifference.RefundAmounts.AgricultureTaxAmount;
				VATToRefund = amountsOfDifference.RefundAmounts.VATAmount;
				PenaltyLateDecToRefund = amountsOfDifference.RefundAmounts.LateDeclarationPenalty;
				PenaltyMissedDecToRefund = amountsOfDifference.RefundAmounts.MissedDeclarationPenalty;
				PenaltyLatePaymentToRefund = amountsOfDifference.RefundAmounts.LatePaymentPenalty;
				NonDutyTaxRevenueToRefund = amountsOfDifference.RefundAmounts.NonDutyTaxPayment;
			}
		}
		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		public ZDecimal TotalRefundAmount => CusReconCharges.Sum(x => x.CRC_Amount);

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		public ZDecimal PaidDutyAmount => ImportEntryOrEntryLine?.PaidAmounts?.DutyAmount ?? ZDecimal.Zero;

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		public ZDecimal PaidLiquorTaxAmount => ImportEntryOrEntryLine?.PaidAmounts?.LiquorTaxAmount ?? ZDecimal.Zero;

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		public ZDecimal PaidSpecialConsumptionTaxAmount => ImportEntryOrEntryLine?.PaidAmounts?.SpecialConsumptionTaxAmount ?? ZDecimal.Zero;

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		public ZDecimal PaidTransportTaxAmount => ImportEntryOrEntryLine?.PaidAmounts?.TransportTaxAmount ?? ZDecimal.Zero;

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		public ZDecimal PaidEducationTaxAmount => ImportEntryOrEntryLine?.PaidAmounts?.EducationTaxAmount ?? ZDecimal.Zero;

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		public ZDecimal PaidAgricultureTaxAmount => ImportEntryOrEntryLine?.PaidAmounts?.AgricultureTaxAmount ?? ZDecimal.Zero;

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		public ZDecimal PaidVATAmount => ImportEntryOrEntryLine?.PaidAmounts?.VATAmount ?? ZDecimal.Zero;

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		public ZDecimal PaidTotalPenalty => ImportEntryOrEntryLine?.PaidAmounts?.TotalPenalty ?? ZDecimal.Zero;

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		public ZDecimal TotalPaid => ImportEntryOrEntryLine?.PaidAmounts?.TotalPaid ?? ZDecimal.Zero;

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		public ZDecimal PaidLatePaymentPenalty => ImportEntryOrEntryLine?.PaidAmounts?.LatePaymentPenalty ?? ZDecimal.Zero;

		public void PopulateRefundInvoiceLinesFromEntryLine(KREntryLineDetailsView view)
		{
			CRL_OriginalEntryLineNumber = view.KEL_LineNumber;

			var invoiceLineViews = Factory.Load<KRInvoiceLineDetailsView>(new ZQuery(KRInvoiceLineDetailsViewSchema.KIL_KEL, view.PK));
			foreach (var invoiceLine in invoiceLineViews)
			{
				var reconInvoiceLine = RefundInvoiceLines.AddNew();
				reconInvoiceLine.CSI_LineNo = invoiceLine.KIL_SequenceNumber;
				reconInvoiceLine.CSI_Description = invoiceLine.KIL_Model;
				reconInvoiceLine.CSI_Quantity2 = invoiceLine.KIL_InvoiceQuantity;
				reconInvoiceLine.CSI_Value = invoiceLine.KIL_LinePrice == 0m || invoiceLine.KIL_InvoiceQuantity == 0 ? 0m : decimal.Round(invoiceLine.KIL_LinePrice / invoiceLine.KIL_InvoiceQuantity, DecimalPlacesConstants.UnitPrice);
				var tariffView = new TariffView.Loader(base.Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem, invoiceLine.KIL_Tariff, Header.CRE_EntryDate);
				reconInvoiceLine.CSI_AdditionalDescription = tariffView?.ZZ1_Description ?? ZString.Empty;
			}
		}

		public IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = new Dictionary<ZString, Type> {
				{ CusSupportingInfoTypeList.Codes.ContractRevocation5UL, typeof(ContractRevocation5UL) },
				{ CusSupportingInfoTypeList.Codes.RefundInvoiceLine, typeof(RefundInvoiceLine) }
			};
			return result;
		}

		public IEnumerable<IBusinessObjectFetchStrategy> GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusSupportingInfoTypeSupporterFetchStrategy(this);
		}
	}
}
