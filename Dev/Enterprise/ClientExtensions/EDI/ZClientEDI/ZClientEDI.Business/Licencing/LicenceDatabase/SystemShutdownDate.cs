using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;

namespace Enterprise.Client.EDI.Licencing.Business
{
	public class SystemShutdownDate : NonPersistentBusinessObject, IObsoleteValidation
	{
		public SystemShutdownDate(LicenceDatabase db)
			: base(db != null ? db.Factory : null)
		{
			ParentDatabase = db;
			if (db != null)
			{
				newSystemShutdownDate = db.LD_ManualLicenceExpiry;
				expiredMessage = db.CustomExpiredNote.Text;
				expiryWeekMessage = db.CustomExpiryWeekNote.Text;
				expiryMonthMessage = db.CustomExpiryMonthNote.Text;
			}
		}

		public readonly LicenceDatabase ParentDatabase;

		public void ApplyTo(LicenceDatabase db)
		{
			db.LD_ManualLicenceExpiry = NewSystemShutdownDate;
			db.CustomExpiredNote.Text = ExpiredMessage;
			db.CustomExpiryWeekNote.Text = ExpiryWeekMessage;
			db.CustomExpiryMonthNote.Text = ExpiryMonthMessage;
		}

		#region Date

		public ZDateTime NewSystemShutdownDate
		{
			get { return newSystemShutdownDate; }
			set
			{
				SetNonPersistentPropertyValue(NewSystemShutdownDateInfo, ref newSystemShutdownDate, value);
				if (!IsValidationSuspended)
				{
					ValidateNewSystemShutdownDate();
				}
			}
		}

		ZDateTime newSystemShutdownDate;

		public ZPropertyInfo NewSystemShutdownDateInfo
		{
			get { return GetZPropertyInfo(nameof(NewSystemShutdownDate)); }
		}

		void ValidateNewSystemShutdownDate()
		{
			NewSystemShutdownDateInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeWithoutRange(NewSystemShutdownDateInfo);
			TypeValidation.CheckValidZDateTimeRange(NewSystemShutdownDateInfo, new TypeValidationLimits() { FutureYearsBeforeError = 10 });
		}

		#endregion

		#region Custom Messages

		[CargoWise.ComponentModel.MaxLength(2000)]
		public ZString ExpiredMessage
		{
			get { return expiredMessage; }
			set { SetNonPersistentPropertyValue(ExpiredMessageInfo, ref expiredMessage, value); }
		}
		ZString expiredMessage;

		public ZPropertyInfo ExpiredMessageInfo
		{
			get { return GetZPropertyInfo(nameof(ExpiredMessage)); }
		}

		[CargoWise.ComponentModel.MaxLength(2000)]
		public ZString ExpiryWeekMessage
		{
			get { return expiryWeekMessage; }
			set { SetNonPersistentPropertyValue(ExpiryWeekMessageInfo, ref expiryWeekMessage, value); }
		}
		ZString expiryWeekMessage;

		public ZPropertyInfo ExpiryWeekMessageInfo
		{
			get { return GetZPropertyInfo(nameof(ExpiryWeekMessage)); }
		}

		[CargoWise.ComponentModel.MaxLength(2000)]
		public ZString ExpiryMonthMessage
		{
			get { return expiryMonthMessage; }
			set { SetNonPersistentPropertyValue(ExpiryMonthMessageInfo, ref expiryMonthMessage, value); }
		}
		ZString expiryMonthMessage;

		public ZPropertyInfo ExpiryMonthMessageInfo
		{
			get { return GetZPropertyInfo(nameof(ExpiryMonthMessage)); }
		}

		#endregion

		#region Validation

		void ValidateMessages()
		{
			ExpiredMessageInfo.ClearAllNotifications();
			ExpiryWeekMessageInfo.ClearAllNotifications();
			ExpiryMonthMessageInfo.ClearAllNotifications();
			if (!ExpiredMessage.IsEmpty || !ExpiryWeekMessage.IsEmpty || !ExpiryMonthMessage.IsEmpty)
			{
				if (ExpiredMessage.IsEmpty)
				{
					ExpiredMessageInfo.AddError(AllMessagesRequiredError);
				}
				if (ExpiryWeekMessage.IsEmpty)
				{
					ExpiryWeekMessageInfo.AddError(AllMessagesRequiredError);
				}
				if (ExpiryMonthMessage.IsEmpty)
				{
					ExpiryMonthMessageInfo.AddError(AllMessagesRequiredError);
				}
			}
		}

		const string AllMessagesRequiredError = "If any message is customized then all messages must be customized";

		protected override void RunPreSaveValidationCore()
		{
			ValidateMessages();
			ValidateNewSystemShutdownDate();
			base.RunPreSaveValidationCore();
		}

		#endregion
	}
}

