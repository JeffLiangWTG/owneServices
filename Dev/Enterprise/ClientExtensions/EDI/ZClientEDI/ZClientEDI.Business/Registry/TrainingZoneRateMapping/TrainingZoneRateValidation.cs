using System;
using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class TrainingZoneRateValidation : ZValidation
	{
		#region Implementation

		public TrainingZoneRateValidation(TrainingZoneRate parent)
			: base(parent)
		{
			this.parent = parent;
			this.parentListInternals = parent;
		}

		public override void ValidateAll()
		{
			using (parentListInternals.SuspendListChanged())
			{
				ValidateZonePK();
				ValidateRateAmount();
				ValidateCurrencyCode();
			}
		}

		public override Type AutoValidationType
		{
			get { return typeof(TrainingZoneRateValidation); }
		}

		protected TrainingZoneRate parent;
		readonly ISingleElementListInternal parentListInternals;

		#endregion

		#region ZonePK

		public void ValidateZonePK()
		{
			ValidateCalculatedProperty(parent.ZonePKInfo);
		}

		protected void CheckZonePK()
		{
			const string propertyDescription = "Zone";

			MandatoryValidation.CheckEntered(parent.ZonePKInfo, propertyDescription);
			ListValidation.ErrorIfInvalidPK(parent.ZonePKInfo, parent.Lookups.Zones, ListValidation.GetNotificationMessage(propertyDescription));
			CheckDuplicateZonePK();
		}

		void CheckDuplicateZonePK()
		{
			if (parent.ParentCollection != null)
			{
				const string duplicateZoneErrorMessage = "The selected zone has already been chosen elsewhere.";
				foreach (TrainingZoneRate rate in parent.ParentCollection)
				{
					if (rate != parent && rate.ZonePK == parent.ZonePK)
					{
						parent.ZonePKInfo.AddError(duplicateZoneErrorMessage);
						break;
					}
				}
			}
		}

		#endregion

		#region Rate Amount

		public void ValidateRateAmount()
		{
			ValidateCalculatedProperty(parent.RateAmountInfo);
		}

		protected void CheckRateAmount()
		{
			CompareValidation.CheckNumberGreaterThanZero(parent.RateAmountInfo);
		}

		#endregion

		#region Currency

		public void ValidateCurrencyCode()
		{
			ValidateCalculatedProperty(parent.CurrencyCodeInfo);
		}

		protected void CheckCurrencyCode()
		{
			const string propertyDescription = "Currency";

			MandatoryValidation.CheckEntered(parent.CurrencyCodeInfo, propertyDescription);
			ListValidation.ErrorIfInvalidCode(parent.CurrencyCodeInfo, parent.Lookups.Currencies, ListValidation.GetNotificationMessage(propertyDescription));
		}

		#endregion
	}
}

