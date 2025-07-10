//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccCommissionLineGroupValidation
//
//    This class should be used for overriding validation in AutoAccCommissionLineGroupValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using CargoWise.EntityFramework;

namespace Enterprise.CommissionManagement.Business
{
	public class CommissionLineGroupingValidation : AutoCommissionLineGroupingValidation
	{
		public CommissionLineGroupingValidation(AutoCommissionLineGrouping parent)
			: base(parent)
		{
		}
	}

	public class CommissionLineGroupingValidation<TGrouping, TLine> : CommissionLineGroupingValidation
		where TGrouping : CommissionLineGrouping<TGrouping, TLine>
		where TLine : BusinessObject, IViewCommissionLineProvider
	{
		public CommissionLineGroupingValidation(CommissionLineGrouping<TGrouping, TLine> parent)
			: base(parent)
		{
			this.ZValidationInternals = this;
		}

		new TGrouping Parent
		{
			get { return (TGrouping)base.Parent; }
		}

		#region PreferredPaymentCompanyPk

		protected override void CheckPreferredPaymentCompanyPk()
		{
			AddWarningIfNoPreferredPaymentCompanyEntered(Parent.PreferredPaymentCompanyPkInfo);
		}

		#endregion

		#region PreferredCurrencyCode

		protected override void CheckPreferredCurrencyCode()
		{
			AddWarningIfNoPreferredPaymentCompanyEntered(Parent.PreferredCurrencyCodeInfo);
		}

		#endregion

		#region TotalCommissionableAmountInLocalCurrency

		public void ValidateTotalCommissionableAmountInLocalCurrency()
		{
			ZValidationInternals.Validate(Parent.TotalCommissionableAmountInLocalCurrencyInfo, GetTotalCommissionableAmountInLocalCurrencyValidationInvoker());
		}

		RunValidationInvoker GetTotalCommissionableAmountInLocalCurrencyValidationInvoker()
		{
			return delegate
			{
				CheckTotalCommissionableAmountInLocalCurrency();
			};
		}

		protected virtual void CheckTotalCommissionableAmountInLocalCurrency()
		{
			if (Parent.ShouldRollupCommissionLineNotifications)
			{
				RollupCommissionLineNotifications(Parent.TotalCommissionableAmountInLocalCurrencyInfo, x => x.VCL_TotalCommissionableAmountInLocalCurrencyInfo);
			}
		}

		#endregion

		#region ShareCommissionAmountInLocalCurrency

		public void ValidateShareCommissionAmountInLocalCurrency()
		{
			ZValidationInternals.Validate(Parent.ShareCommissionAmountInLocalCurrencyInfo, GetShareCommissionAmountInLocalCurrencyValidationInvoker());
		}

		RunValidationInvoker GetShareCommissionAmountInLocalCurrencyValidationInvoker()
		{
			return delegate
			{
				CheckShareCommissionAmountInLocalCurrency();
			};
		}

		protected virtual void CheckShareCommissionAmountInLocalCurrency()
		{
			if (Parent.ShouldRollupCommissionLineNotifications)
			{
				RollupCommissionLineNotifications(Parent.ShareCommissionAmountInLocalCurrencyInfo, x => x.VCL_ShareCommissionAmountInLocalCurrencyInfo);
			}
		}

		#endregion

		#region EntityCommissionAmountInLocalCurrency

		public void ValidateEntityCommissionAmountInLocalCurrency()
		{
			ZValidationInternals.Validate(Parent.EntityCommissionAmountInLocalCurrencyInfo, GetEntityCommissionAmountInLocalCurrencyValidationInvoker());
		}

		RunValidationInvoker GetEntityCommissionAmountInLocalCurrencyValidationInvoker()
		{
			return delegate
			{
				CheckEntityCommissionAmountInLocalCurrency();
			};
		}

		protected virtual void CheckEntityCommissionAmountInLocalCurrency()
		{
			if (Parent.ShouldRollupCommissionLineNotifications)
			{
				RollupCommissionLineNotifications(Parent.EntityCommissionAmountInLocalCurrencyInfo, x => x.VCL_EntityCommissionAmountInLocalCurrencyInfo);
			}
		}

		#endregion

		#region TotalCommissionableAmountInPreferredCurrency

		public void ValidateTotalCommissionableAmountInPreferredCurrency()
		{
			ZValidationInternals.Validate(Parent.TotalCommissionableAmountInPreferredCurrencyInfo, GetTotalCommissionableAmountInPreferredCurrencyValidationInvoker());
		}

		RunValidationInvoker GetTotalCommissionableAmountInPreferredCurrencyValidationInvoker()
		{
			return delegate
			{
				CheckTotalCommissionableAmountInPreferredCurrency();
			};
		}

		protected virtual void CheckTotalCommissionableAmountInPreferredCurrency()
		{
			if (Parent.ShouldRollupCommissionLineNotifications)
			{
				RollupCommissionLineNotifications(Parent.TotalCommissionableAmountInPreferredCurrencyInfo, x => x.VCL_TotalCommissionableAmountInPreferredCurrencyInfo);
			}
			else
			{
				AddWarningIfNoPreferredPaymentCompanyEntered(Parent.TotalCommissionableAmountInPreferredCurrencyInfo);
			}
		}

		#endregion

		#region ShareCommissionAmountInPreferredCurrency

		public void ValidateShareCommissionAmountInPreferredCurrency()
		{
			ZValidationInternals.Validate(Parent.ShareCommissionAmountInPreferredCurrencyInfo, GetShareCommissionAmountInPreferredCurrencyValidationInvoker());
		}

		RunValidationInvoker GetShareCommissionAmountInPreferredCurrencyValidationInvoker()
		{
			return delegate
			{
				CheckShareCommissionAmountInPreferredCurrency();
			};
		}

		protected virtual void CheckShareCommissionAmountInPreferredCurrency()
		{
			if (Parent.ShouldRollupCommissionLineNotifications)
			{
				RollupCommissionLineNotifications(Parent.ShareCommissionAmountInPreferredCurrencyInfo, x => x.VCL_ShareCommissionAmountInPreferredCurrencyInfo);
			}
			else
			{
				AddWarningIfNoPreferredPaymentCompanyEntered(Parent.ShareCommissionAmountInPreferredCurrencyInfo);
			}
		}

		#endregion

		#region EntityCommissionAmountInPreferredCurrency

		public void ValidateEntityCommissionAmountInPreferredCurrency()
		{
			ZValidationInternals.Validate(Parent.EntityCommissionAmountInPreferredCurrencyInfo, GetEntityCommissionAmountInPreferredCurrencyValidationInvoker());
		}

		RunValidationInvoker GetEntityCommissionAmountInPreferredCurrencyValidationInvoker()
		{
			return delegate
			{
				CheckEntityCommissionAmountInPreferredCurrency();
			};
		}

		protected virtual void CheckEntityCommissionAmountInPreferredCurrency()
		{
			if (Parent.ShouldRollupCommissionLineNotifications)
			{
				RollupCommissionLineNotifications(Parent.EntityCommissionAmountInPreferredCurrencyInfo, x => x.VCL_EntityCommissionAmountInPreferredCurrencyInfo);
			}
			else
			{
				AddWarningIfNoPreferredPaymentCompanyEntered(Parent.EntityCommissionAmountInPreferredCurrencyInfo);
			}
		}

		#endregion

		#region PaidEntityCommissionAmountInPreferredCurrency

		public void ValidatePaidEntityCommissionAmountInPreferredCurrency()
		{
			ZValidationInternals.Validate(Parent.PaidEntityCommissionAmountInPreferredCurrencyInfo, GetPaidEntityCommissionAmountInPreferredCurrencyValidationInvoker());
		}

		RunValidationInvoker GetPaidEntityCommissionAmountInPreferredCurrencyValidationInvoker()
		{
			return delegate
			{
				CheckPaidEntityCommissionAmountInPreferredCurrency();
			};
		}

		protected virtual void CheckPaidEntityCommissionAmountInPreferredCurrency()
		{
			AddWarningIfNoPreferredPaymentCompanyEntered(Parent.PaidEntityCommissionAmountInPreferredCurrencyInfo);
		}

		#endregion

		#region OutstandingEntityCommissionAmountInPreferredCurrency

		public void ValidateOutstandingEntityCommissionAmountInPreferredCurrency()
		{
			ZValidationInternals.Validate(Parent.OutstandingEntityCommissionAmountInPreferredCurrencyInfo, GetOutstandingEntityCommissionAmountInPreferredCurrencyValidationInvoker());
		}

		RunValidationInvoker GetOutstandingEntityCommissionAmountInPreferredCurrencyValidationInvoker()
		{
			return delegate
			{
				CheckOutstandingEntityCommissionAmountInPreferredCurrency();
			};
		}

		protected virtual void CheckOutstandingEntityCommissionAmountInPreferredCurrency()
		{
			AddWarningIfNoPreferredPaymentCompanyEntered(Parent.OutstandingEntityCommissionAmountInPreferredCurrencyInfo);
		}

		#endregion

		#region RollupCommissionLineNotifications

		void RollupCommissionLineNotifications(ZPropertyInfo targetPropertyInfo, Func<ViewCommissionLine, ZPropertyInfo> commissionLinePropertyInfoGetter)
		{
			foreach (var line in Parent.CommissionLines)
			{
				var commissionLinePropertyInfo = commissionLinePropertyInfoGetter(line);
				foreach (var notification in commissionLinePropertyInfo.Notifications)
				{
					targetPropertyInfo.AddNotification(notification.Type, notification.Message);
				}
			}
		}

		#endregion

		#region ValidateAll

		protected override void ValidateAllCore()
		{
			base.ValidateAllCore();

			ValidateTotalCommissionableAmountInLocalCurrency();
			ValidateShareCommissionAmountInLocalCurrency();
			ValidateEntityCommissionAmountInLocalCurrency();

			ValidatePreferredPaymentCompanyPk();
			ValidatePreferredCurrencyCode();
			ValidateTotalCommissionableAmountInPreferredCurrency();
			ValidateShareCommissionAmountInPreferredCurrency();
			ValidateEntityCommissionAmountInPreferredCurrency();
			ValidatePaidEntityCommissionAmountInPreferredCurrency();
			ValidateOutstandingEntityCommissionAmountInPreferredCurrency();
		}

		#endregion

		#region Overrides

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
		{
			return false;
		}

		#endregion

		#region Implementation

		protected void AddWarningIfNoPreferredPaymentCompanyEntered(ZPropertyInfo propertyInfo)
		{
			if (Parent.PreferredPaymentCompanyPk.IsEmpty)
			{
				propertyInfo.AddWarning(Res.GetString("9f511983-9c27-4a3b-bcd5-27747df10cd3", "Amounts can not be calculated until a preferred payment company is entered on the entity"));
			}
		}

		protected IValidationInternals ZValidationInternals;

		#endregion
	}
}
