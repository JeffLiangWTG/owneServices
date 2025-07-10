using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EFTPaymentInformation : NonPersistentBusinessObject, IObsoleteValidation
	{
		public EFTPaymentInformation(CusEntryHeader entryHeader, bool initialiseFromLastClearance = true)
			: base(entryHeader.Factory)
		{
			EntryHeader = entryHeader;
			totalAmountDueAdvisedByCustoms = entryHeader.TotalPayableDueAdvisedInLastClearanceMessage;

			if (initialiseFromLastClearance)
			{
				CustomsChargeAmountPayableNow = totalAmountDueAdvisedByCustoms;
				AQISServicePaymentAmountPayableNow = 0m;
			}
		}

		#region Entry Header BGMReferenceNumber

		public bool HasAmountsToPay
		{
			get { return CustomsChargeAmountPayableNow > 0 || AQISServicePaymentAmountPayableNow > 0; }
		}

		public ZString BGMReferenceNumber
		{
			get { return EntryHeader.CH_BGMReference; }
		}

		public ZPropertyInfo BGMReferenceNumberInfo
		{
			get { return GetZPropertyInfo(nameof(BGMReferenceNumber)); }
		}

		#endregion

		#region Entry Header Entry Number

		public ZString EntryNumber
		{
			get { return EntryHeader.EntryNumber; }
		}

		public ZPropertyInfo EntryNumberInfo
		{
			get { return GetZPropertyInfo(nameof(EntryNumber)); }
		}

		#endregion

		#region AQISServicePaymentAmountPayableNow

		public ZDecimal AQISServicePaymentAmountPayableNow
		{
			get { return EntryHeader.AQISServicePaymentAmountPayableNow; }
			set
			{
				EntryHeader.AQISServicePaymentAmountPayableNow = value;
				if (!IsValidationSuspended)
				{
					ValidateAQISServicePaymentAmountPayableNow();
				}
				AQISServicePaymentAmountPayableNowInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AQISServicePaymentAmountPayableNowInfo
		{
			get { return GetZPropertyInfo(nameof(AQISServicePaymentAmountPayableNow)); }
		}

		public const string AQISForSACMessage = "You are attempting to PAY AQIS charges on a SAC entry. AU Customs and AQIS have a known system anomaly which STOPS such payments and is not expected to be fixed promptly. Payments must be made over the counter at an AQIS office as electronic payment is not available. If you proceed, the following error message will be responded: AQIS SERVICES PAYMENTS ARE NOT ALLOWED FOR A LONG FORM SELF ASSESSED CLEARANCE";
		public void ValidateAQISServicePaymentAmountPayableNow()
		{
			if (!IsValidationSuspended)
			{
				AQISServicePaymentAmountPayableNowInfo.ClearAllNotifications();
				if (AQISServicePaymentAmountPayableNow > 0m && EntryHeader.IsSAC)
				{
					AQISServicePaymentAmountPayableNowInfo.AddMessageError(AQISForSACMessage);
				}
			}
		}

		#endregion

		#region CustomsChargeAmountPayableNow

		public ZDecimal CustomsChargeAmountPayableNow
		{
			get { return EntryHeader.CustomsChargeAmountPayableNow; }
			set
			{
				EntryHeader.CustomsChargeAmountPayableNow = value;
				if (!IsValidationSuspended)
				{
					ValidateCustomsChargeAmountPayableNow();
				}
				CustomsChargeAmountPayableNowInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CustomsChargeAmountPayableNowInfo
		{
			get { return GetZPropertyInfo(nameof(CustomsChargeAmountPayableNow)); }
		}

		public void ValidateCustomsChargeAmountPayableNow()
		{
			if (!IsValidationSuspended)
			{
				CustomsChargeAmountPayableNowInfo.ClearAllNotifications();
				if (!CustomsChargeAmountPayableNow.IsEmpty)
				{
					if (CustomsChargeAmountPayableNow != totalAmountDueAdvisedByCustoms)
					{
						CustomsChargeAmountPayableNowInfo.AddMessageError("Customs advised total amount due is " + totalAmountDueAdvisedByCustoms + ", which is different to what you indicated here.");
					}

					if (EntryHeader.IsEntryHeld)
					{
						CustomsChargeAmountPayableNowInfo.AddMessageError(EntryIsHeldAndAmountIsGreatThanZero);
					}
				}
			}
		}
		internal const string EntryIsHeldAndAmountIsGreatThanZero = "The Entry is HELD.  Customs Entry Payments while the Entry is HELD may prevent further changes.";

		#endregion

		#region ScheduledPaymentDate

		[ResourceStringData("ED30BACC-C616-4322-9370-59A6776D084C", Caption = "Scheduled Payment Date", FullDescription = "Setting a date here will schedule the payment message to be sent at this date and time.")]
		public ZDateTime ScheduledPaymentDate
		{
			get => EntryHeader.ScheduledPaymentDate;
			set
			{
				EntryHeader.ScheduledPaymentDate = value;
				if (!IsValidationSuspended)
				{
					ValidateScheduledPaymentDate();
				}
				ScheduledPaymentDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ScheduledPaymentDateInfo => GetZPropertyInfo(nameof(EFTPaymentInformation.ScheduledPaymentDate));

		void ValidateScheduledPaymentDate()
		{
			ScheduledPaymentDateInfo.ClearAllNotifications();
			if (ScheduledPaymentDate < ZDateTime.Now)
			{
				ScheduledPaymentDateInfo.AddWarning("Scheduled date is earlier than the current date so the payment message will be sent immediately.");
			}
		}

		#endregion

		#region RunPreSaveValidationCore

		public readonly CusEntryHeader EntryHeader;
		readonly ZDecimal totalAmountDueAdvisedByCustoms;

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateAQISServicePaymentAmountPayableNow();
			ValidateCustomsChargeAmountPayableNow();
			ValidateScheduledPaymentDate();
		}

		#endregion
	}
}
