using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business
{
	public sealed class StlDiscountPreview : NonPersistentBusinessObject, IObsoleteValidation
	{
		public StlDiscountPreview()
			: base()
		{
		}

		#region Name

		[MaxLength(EdiPriceHeaderDiscount.Schema.PHD_NameMaxLength)]
		public ZString Name
		{
			get
			{
				return name;
			}
			set
			{
				CheckMaximumLength(NameInfo, value);
				SetNonPersistentPropertyValue(NameInfo, ref name, value);
			}
		}
		ZString name;

		public ZPropertyInfo NameInfo
		{
			get
			{
				return this.GetZPropertyInfo(nameof(Name));
			}
		}

		#endregion

		#region IsActive

		public ZBool IsActive
		{
			get
			{
				return isActive;
			}
			set
			{
				SetNonPersistentPropertyValue(IsActiveInfo, ref isActive, value);
			}
		}
		public ZPropertyInfo IsActiveInfo
		{
			get
			{
				return this.GetZPropertyInfo(nameof(IsActive));
			}
		}
		ZBool isActive;

		#endregion

		#region Percent

		public ZDecimal Percent
		{
			get
			{
				return percent;
			}
			set
			{
				SetNonPersistentPropertyValue(PercentInfo, ref percent, value);
				if (!IsValidationSuspended)
				{
					ValidatePercent();
				}
			}
		}
		ZDecimal percent;

		public ZPropertyInfo PercentInfo
		{
			get
			{
				return this.GetZPropertyInfo(nameof(Percent));
			}
		}

		public bool Percent_ReadOnly { get; set; }

		public void ValidatePercent()
		{
			PercentInfo.ClearAllNotifications();
		}

		public new StlDiscountPreview Clone()
		{
			return new StlDiscountPreview
			{
				Name = Name,
				Percent = Percent,
				isActive = IsActive,
				Percent_ReadOnly = Percent_ReadOnly
			};
		}

		#endregion
	}
}

