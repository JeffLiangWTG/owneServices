//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewCommissionLineValidation
//
//    This class should be used for overriding validation in AutoViewCommissionLineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.CommissionManagement.Business
{
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Registry.Business;

	public class ViewCommissionLineValidation : AutoViewCommissionLineValidation
	{
		public ViewCommissionLineValidation(AutoViewCommissionLine parent)
			: base(parent)
		{
			this.ZValidationInternals = this;
		}

		new ViewCommissionLine Parent
		{
			get { return (ViewCommissionLine)base.Parent; }
		}

		#region Properties

		#region CheckVCL_TransactionAmount

		protected override void CheckVCL_TransactionAmountIsNotEmpty()
		{
			// Allow 0 values
		}

		#endregion

		#region VCL_TotalCommissionableAmountInLocalCurrency

		public void ValidateVCL_TotalCommissionableAmountInLocalCurrency()
		{
			ZValidationInternals.Validate(Parent.VCL_TotalCommissionableAmountInLocalCurrencyInfo, GetVCL_TotalCommissionableAmountInLocalCurrencyInvoker());
		}

		RunValidationInvoker GetVCL_TotalCommissionableAmountInLocalCurrencyInvoker()
		{
			return delegate
			{
				CheckVCL_TotalCommissionableAmountInLocalCurrency();
			};
		}

		protected virtual void CheckVCL_TotalCommissionableAmountInLocalCurrency()
		{
			CheckLocalRatesEntered(Parent.VCL_TotalCommissionableAmountInLocalCurrencyInfo);
		}

		#endregion

		#region VCL_TotalCommissionableAmountInPreferredCurrency

		public void ValidateVCL_TotalCommissionableAmountInPreferredCurrency()
		{
			ZValidationInternals.Validate(Parent.VCL_TotalCommissionableAmountInPreferredCurrencyInfo, GetVCL_TotalCommissionableAmountInPreferredCurrencyInvoker());
		}

		RunValidationInvoker GetVCL_TotalCommissionableAmountInPreferredCurrencyInvoker()
		{
			return delegate
			{
				CheckVCL_TotalCommissionableAmountInPreferredCurrency();
			};
		}

		protected virtual void CheckVCL_TotalCommissionableAmountInPreferredCurrency()
		{
			CheckPreferredCompanyAndRatesEntered(Parent.VCL_TotalCommissionableAmountInPreferredCurrencyInfo);
		}

		#endregion

		#region VCL_ShareCommissionAmountInLocalCurrency

		public void ValidateVCL_ShareCommissionAmountInLocalCurrency()
		{
			ZValidationInternals.Validate(Parent.VCL_ShareCommissionAmountInLocalCurrencyInfo, GetVCL_ShareCommissionAmountInLocalCurrencyInvoker());
		}

		RunValidationInvoker GetVCL_ShareCommissionAmountInLocalCurrencyInvoker()
		{
			return delegate
			{
				CheckVCL_ShareCommissionAmountInLocalCurrency();
			};
		}

		protected virtual void CheckVCL_ShareCommissionAmountInLocalCurrency()
		{
			CheckLocalRatesEntered(Parent.VCL_ShareCommissionAmountInLocalCurrencyInfo);
		}

		#endregion

		#region VCL_ShareCommissionAmountInPreferredCurrency

		public void ValidateVCL_ShareCommissionAmountInPreferredCurrency()
		{
			ZValidationInternals.Validate(Parent.VCL_ShareCommissionAmountInPreferredCurrencyInfo, GetVCL_ShareCommissionAmountInPreferredCurrencyInvoker());
		}

		RunValidationInvoker GetVCL_ShareCommissionAmountInPreferredCurrencyInvoker()
		{
			return delegate
			{
				CheckVCL_ShareCommissionAmountInPreferredCurrency();
			};
		}

		protected virtual void CheckVCL_ShareCommissionAmountInPreferredCurrency()
		{
			CheckPreferredCompanyAndRatesEntered(Parent.VCL_ShareCommissionAmountInPreferredCurrencyInfo);
		}

		#endregion

		#region VCL_EntityCommissionAmountInLocalCurrency

		public void ValidateVCL_EntityCommissionAmountInLocalCurrency()
		{
			ZValidationInternals.Validate(Parent.VCL_EntityCommissionAmountInLocalCurrencyInfo, GetVCL_EntityCommissionAmountInLocalCurrencyInvoker());
		}

		RunValidationInvoker GetVCL_EntityCommissionAmountInLocalCurrencyInvoker()
		{
			return delegate
			{
				CheckVCL_EntityCommissionAmountInLocalCurrency();
			};
		}

		protected virtual void CheckVCL_EntityCommissionAmountInLocalCurrency()
		{
			CheckLocalRatesEntered(Parent.VCL_EntityCommissionAmountInLocalCurrencyInfo);
		}

		#endregion

		#region VCL_EntityCommissionAmountInPreferredCurrency

		public void ValidateVCL_EntityCommissionAmountInPreferredCurrency()
		{
			ZValidationInternals.Validate(Parent.VCL_EntityCommissionAmountInPreferredCurrencyInfo, GetVCL_EntityCommissionAmountInPreferredCurrencyInvoker());
		}

		RunValidationInvoker GetVCL_EntityCommissionAmountInPreferredCurrencyInvoker()
		{
			return delegate
			{
				CheckVCL_EntityCommissionAmountInPreferredCurrency();
			};
		}

		protected virtual void CheckVCL_EntityCommissionAmountInPreferredCurrency()
		{
			CheckPreferredCompanyAndRatesEntered(Parent.VCL_EntityCommissionAmountInPreferredCurrencyInfo);
		}

		#endregion

		public void ValidateFullyPaymentOfARInvoices()
		{
			ZValidationInternals.Validate(Parent.HasFullyPaidInfo, ValidateARInvoicesFullPaymentInvoker());
		}

		RunValidationInvoker ValidateARInvoicesFullPaymentInvoker()
		{
			return delegate
			{
				CheckFullPayment();
			};
		}

		protected virtual void CheckFullPayment()
		{
			AddWarningIfARInvoicesHavenotFullyPaid(Parent.HasFullyPaidInfo);
		}

		protected void AddWarningIfARInvoicesHavenotFullyPaid(ZPropertyInfo propertyInfo)
		{
			if (Parent.IsARInvoice && !Parent.HasFullyPaid)
			{
				if (OrganisationsDataRegistry.Instance.DisallowCommissionPaymentIfARInvoiceNotFullyPaid.Value)
				{
					propertyInfo.AddWarning(Res.GetString("A9FE12E8-3A47-47C7-8622-1338E69C63E1", "AR invoice(s) have to be fully paid prior to payment process action"));
				}
				else
				{
					propertyInfo.AddWarning(Res.GetString("5999A9D8-8E01-432C-9740-BB37A664AE48", "AR Invoice(s) are not fully paid"));
				}
			}
		}

		#endregion

		#region LocalAmounts

		public void ValidateAllLocalAmounts()
		{
			ValidateVCL_TotalCommissionableAmountInLocalCurrency();
			ValidateVCL_ShareCommissionAmountInLocalCurrency();
			ValidateVCL_EntityCommissionAmountInLocalCurrency();
		}

		protected void CheckLocalRatesEntered(ZPropertyInfo propertyInfo)
		{
			if (Parent.VCL_CommissionToLocalExchangeRate == 0m)
			{
				AddNoValidExchangeRateWarning(propertyInfo, Parent.VCL_RX_NKCommissionCurrency, Parent.VCL_CommissionDate);
			}
		}

		#endregion

		#region PreferredAmounts

		public void ValidateAllPreferredAmounts()
		{
			ValidateVCL_TotalCommissionableAmountInPreferredCurrency();
			ValidateVCL_ShareCommissionAmountInPreferredCurrency();
			ValidateVCL_EntityCommissionAmountInPreferredCurrency();
		}

		protected void CheckPreferredCompanyAndRatesEntered(ZPropertyInfo propertyInfo)
		{
			if (Parent.VCL_RX_NKPreferredPaymentCurrency.IsEmpty)
			{
				var errorMessage = Res.GetString("7859dd41-08f4-4296-9ef0-4be9bf2e7d74", "Preferred Payment Company has not been entered for this Entity");
				propertyInfo.AddWarning(errorMessage);
			}
			else if (Parent.VCL_CommissionToLocalExchangeRate == 0m)
			{
				AddNoValidExchangeRateWarning(propertyInfo, Parent.VCL_RX_NKCommissionCurrency, Parent.VCL_CommissionDate);
			}
			else if (Parent.VCL_LocalToPreferredExchangeRate == 0m)
			{
				AddNoValidExchangeRateWarning(propertyInfo, Parent.VCL_RX_NKPreferredPaymentCurrency, Parent.VCL_CommissionDate);
			}
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateAllLocalAmounts();
			ValidateAllPreferredAmounts();
			ValidateFullyPaymentOfARInvoices();
		}

		public bool ShouldStopPaymentForProcess
		{
			get
			{
				if (!Parent.HasNotifications())
				{
					return false;
				}

				if (Parent.PropertiesWithNotifications.Any(x => x.Name != ViewCommissionLine.Schema.HasFullyPaid && x.Name != ViewCommissionLine.Schema.VCL_AC) || Parent.HasRowNotifications)
				{
					return true;
				}

				return (Parent.IsARInvoice && !Parent.HasFullyPaid && OrganisationsDataRegistry.Instance.DisallowCommissionPaymentIfARInvoiceNotFullyPaid.Value);
			}
		}

		#endregion

		#region Implementation

		void AddNoValidExchangeRateWarning(ZPropertyInfo info, ZString currency, ZDateTime date)
		{
			var message = Res.GetString("376fe01a-e3e5-4094-8c5b-3c80ebe04ede", "No '{0}' exchange rate valid on the {1}", currency, date.ToShortDateString());
			info.AddWarning(message);
		}

		readonly IValidationInternals ZValidationInternals;

		#endregion
	}
}
