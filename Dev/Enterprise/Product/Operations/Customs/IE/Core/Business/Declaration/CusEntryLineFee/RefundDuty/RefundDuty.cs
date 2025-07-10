using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using ICanBeImportOrExport = Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport;
using IEuTax = Enterprise.Customs.EU.Business.Declaration.IEuTax;
using RefundMethodOfCalculation = Enterprise.Customs.IE.Business.Constants.CusEntryLineFeeRefundDutyMethodOfCalculation;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class RefundDuty : NonPersistentBusinessObject, IEuTax
	{
		public RefundDuty(ZString chargeType, RefundDutyFeeCollection refundDutyFees) : base(refundDutyFees.Factory)
		{
			TaxType = chargeType;
			this.refundDutyFees = refundDutyFees;
			refundDutyFeesDict = refundDutyFees.Cast<CusEntryLineFee>()
				.Where(fee => fee.CF_ChargeType == chargeType)
				.ToDictionary(fee => fee.CF_MethodOfCalculation);

			taxAmountConfirmedRelease = refundDutyFeesDict.TryGetValue(RefundMethodOfCalculation.ConfirmedRelease, out var release) ? release.CF_ChargeAmount : ZDecimal.Zero;
			taxAmountConfirmedAmendment = refundDutyFeesDict.TryGetValue(RefundMethodOfCalculation.ConfirmedAmendment, out var amendment) ? amendment.CF_ChargeAmount : ZDecimal.Zero;
			taxAmountDifferenceForRefunds = refundDutyFeesDict.TryGetValue(RefundMethodOfCalculation.DifferenceForRefunds, out var diff) ? diff.CF_ChargeAmount : ZDecimal.Zero;
			taxAmountOfDutyToBeRepaid = refundDutyFeesDict.TryGetValue(RefundMethodOfCalculation.ToBeRepaid, out var repay) ? repay.CF_ChargeAmount : ZDecimal.Zero;
		}
		readonly RefundDutyFeeCollection refundDutyFees;
		readonly IDictionary<ZString, CusEntryLineFee> refundDutyFeesDict;

		public CusEntryLine CusEntryLine => refundDutyFees.CusEntryLine;

		public RefundDutyLookups Lookups => new RefundDutyLookups(this);

		[ResourceStringData("2485AF1D-7F43-43C4-8DEE-D4B55E380223", Caption = "Tax Type")]
		[List($"{nameof(Lookups)}.{nameof(RefundDutyLookups.TaxTypes)}")]
		[MaxLength(5)]
		public ZString TaxType { get; }
		public ZPropertyInfo TaxTypeInfo => GetZPropertyInfo(nameof(TaxType));

		[ReadOnly(true)]
		[ResourceStringData("C30CBD5C-7E85-4533-AFBE-74B70F0BD5FB", Caption = "Confirmed Release", FullDescription = "Tax Amount Confirmed Release")]
		public ZDecimal TaxAmountConfirmedRelease
		{
			get => taxAmountConfirmedRelease;
			set => SetPropertyValueAndUpdateFees(value, RefundMethodOfCalculation.ConfirmedRelease, TaxAmountConfirmedReleaseInfo, ref taxAmountConfirmedRelease);
		}
		ZDecimal taxAmountConfirmedRelease;
		public ZPropertyInfo TaxAmountConfirmedReleaseInfo => GetZPropertyInfo(nameof(TaxAmountConfirmedRelease));

		[ReadOnly(true)]
		[ResourceStringData("66E3BC61-BDCE-4EB7-BA5A-3329F13808B5", Caption = "Confirmed Amendment", FullDescription = "Tax Amount Confirmed Amendment")]
		public ZDecimal TaxAmountConfirmedAmendment
		{
			get => taxAmountConfirmedAmendment;
			set => SetPropertyValueAndUpdateFees(value, RefundMethodOfCalculation.ConfirmedAmendment, TaxAmountConfirmedAmendmentInfo, ref taxAmountConfirmedAmendment);
		}
		ZDecimal taxAmountConfirmedAmendment;
		public ZPropertyInfo TaxAmountConfirmedAmendmentInfo => GetZPropertyInfo(nameof(TaxAmountConfirmedAmendment));

		[ReadOnly(true)]
		[ResourceStringData("19646135-50EC-4EB6-99B6-E62E78ED184A", Caption = "Difference for Refunds", FullDescription = "Tax amount difference for Refunds")]
		public ZDecimal TaxAmountDifferenceForRefunds
		{
			get => taxAmountDifferenceForRefunds;
			set => SetPropertyValueAndUpdateFees(value, RefundMethodOfCalculation.DifferenceForRefunds, TaxAmountDifferenceForRefundsInfo, ref taxAmountDifferenceForRefunds);
		}
		ZDecimal taxAmountDifferenceForRefunds;
		public ZPropertyInfo TaxAmountDifferenceForRefundsInfo => GetZPropertyInfo(nameof(TaxAmountDifferenceForRefunds));

		[ReadOnly(true)]
		[ResourceStringData("29D55D7B-DB07-4FFD-BA22-6A8992115F8F", Caption = "Confirmed Refund", FullDescription = "Confirmed Refund Amount To-Be-Paid")]
		public ZDecimal TaxAmountOfDutyToBeRepaid
		{
			get => taxAmountOfDutyToBeRepaid;
			set => SetPropertyValueAndUpdateFees(value, RefundMethodOfCalculation.ToBeRepaid, TaxAmountOfDutyToBeRepaidInfo, ref taxAmountOfDutyToBeRepaid);
		}
		ZDecimal taxAmountOfDutyToBeRepaid;
		public ZPropertyInfo TaxAmountOfDutyToBeRepaidInfo => GetZPropertyInfo(nameof(TaxAmountOfDutyToBeRepaid));

		void SetPropertyValueAndUpdateFees(ZDecimal value, string refundMethodOfCalculation, ZPropertyInfo propertyInfo, ref ZDecimal privateProperty)
		{
			SetNonPersistentPropertyValue(propertyInfo, ref privateProperty, value);
			if (value.IsEmpty)
			{
				if (refundDutyFeesDict.TryGetValue(refundMethodOfCalculation, out var fee) && !fee.IsDeleted)
				{
					refundDutyFees.RemoveAndDelete(fee);
				}
			}
			else
			{
				CusEntryLineFee fee;
				if (!refundDutyFeesDict.TryGetValue(refundMethodOfCalculation, out fee))
				{
					fee = refundDutyFees.AddNew(TaxType, refundMethodOfCalculation);
					refundDutyFeesDict[refundMethodOfCalculation] = fee;
				}
				fee.CF_ChargeAmount = value;
			}
		}

		#region IEuTax Members

		ZBool IEuTax.IsCopying => throw new System.NotImplementedException();

		ZString IEuTax.CountryCode => refundDutyFees.CusEntryLine.Declaration.CountryCode;

		ICanBeImportOrExport IEuTax.ImportExportParent => refundDutyFees.CusEntryLine.Declaration;

		BusinessObjectFactory IEuTax.Factory => refundDutyFees.Factory;

		ZDecimal IEuTax.G4_CalculatedPercentage => throw new System.NotImplementedException();

		ZString IEuTax.G4_MethodOfPayment { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
		ZString IEuTax.G4_RateDuty { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
		ZString IEuTax.G4_RateOverride { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
		ZString IEuTax.G4_RateSuspension { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
		ZString IEuTax.G4_Type { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
		ZString IEuTax.G4_Amount { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
		ZDecimal IEuTax.G4_BaseAmount { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
		ZDecimal IEuTax.G4_BaseQuantity { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
		ZString IEuTax.G4_BaseQuantityUQ { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

		ZPropertyInfo IEuTax.G4_MethodOfPaymentInfo => throw new System.NotImplementedException();

		ZPropertyInfo IEuTax.G4_RateDutyInfo => throw new System.NotImplementedException();

		ZPropertyInfo IEuTax.G4_RateOverrideInfo => throw new System.NotImplementedException();

		ZPropertyInfo IEuTax.G4_RateSuspensionInfo => throw new System.NotImplementedException();

		ZPropertyInfo IEuTax.G4_TypeInfo => throw new System.NotImplementedException();

		ZPropertyInfo IEuTax.G4_AmountInfo => throw new System.NotImplementedException();

		ZPropertyInfo IEuTax.G4_BaseAmountInfo => throw new System.NotImplementedException();

		void IEuTax.Delete()
		{
			throw new System.NotImplementedException();
		}

		#endregion
	}
}
