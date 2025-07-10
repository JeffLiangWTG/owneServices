//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobExRateValidation
//
//    This class should be used for overriding validation in AutoJobExRateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobExRateValidation : AutoJobExRateValidation
	{
		public JobExRateValidation(AutoJobExRate parent)
			: base(parent)
		{
		}

		protected override void CheckJF_RX_NKRateCurrency()
		{
			base.CheckJF_RX_NKRateCurrency();
			MandatoryValidation.CheckEntered(Parent.JF_RX_NKRateCurrencyInfo);
			if (Parent.JF_RX_NKRateCurrency == (Parent?.Job?.Company ?? GlbCompany.CurrentCompany).GC_RX_NKLocalCurrency) //Somehow in some tests Company == null
			{
				Parent.JF_RX_NKRateCurrencyInfo.AddError(Res.GetString("048e17d0-a3b3-46aa-82e9-e69c20e84cc6", "This list is for foreign currency only. Please remove local currency from the list."));
			}
			CheckItIsNotDuplicate();
			ValidateJF_TodayRate();
		}

		protected override void CheckJF_BaseRate()
		{
			base.CheckJF_BaseRate();
			if (Parent.JF_BaseRate != Parent.JF_TodayRate)
			{
				Parent.JF_BaseRateInfo.AddWarning(Res.GetString("47f9e541-f3ea-4a2f-9dd8-d9e075db6fcd", "The base rate on this job is not the same as today's rate."));
			}

			if (Parent.JF_BaseRate < ZDecimal.Zero)
			{
				Parent.JF_BaseRateInfo.AddError(Res.GetString("6e918a7f-2adf-4efa-a20d-c0e8e02b8063", "Base Rate must be greater than zero."));
			}
			else if (Parent.JF_BaseRate.IsEmpty)
			{
				Parent.JF_BaseRateInfo.AddWarning(Res.GetString("6e918a7f-2adf-4efa-a20d-c0e8e02b8063", "Base Rate must be greater than zero."));
			}

			ZString rateType = ZString.Empty;
			if (Parent.ParentJob != null && AccExchangeRateConfigurationRateFinder.IsExchangeRateTypeNotEqualToSpecified(Parent.ParentJob.ExchangeRateConfigurationRateConsumer, Parent.RateCurrency, ref rateType))
			{
				Parent.JF_BaseRateInfo.AddWarning(Res.GetString("6d027774-3f3f-4034-a69f-b01df6719d5e", "No {0} rate is available for this client. Standard exchange rate will be used.", rateType));
			}
		}

		public void ValidateJF_TodayRate()
		{
			ValidateCalculatedProperty(Parent.JF_TodayRateInfo);
		}

		protected void CheckJF_TodayRate()
		{
			TypeValidation.CheckValidDecimal(Parent.JF_TodayRateInfo, 18, 9);
			if (Parent.JF_TodayRate == 0m)
			{
				Parent.JF_TodayRateInfo.AddWarning(Res.GetString("477f0544-7a48-4f53-8440-2a6da7774d67", "Could not find Today's exchange rate."));
			}
		}

		protected override void CheckJF_JH()
		{
			base.CheckJF_JH();
			CheckItIsNotDuplicate();
		}

		protected override void CheckJF_OrgType()
		{
			base.CheckJF_OrgType();
			ListValidation.ErrorIfInvalidCode(Parent.JF_OrgTypeInfo);
			CheckItHasEmptyOrgTypeWithNonEmptyOrg();
			CheckItIsNotDuplicate();
		}

		protected override void CheckJF_CFXPercent()
		{
			base.CheckJF_CFXPercent();
			CompareValidation.CheckLessThanOrEqualTo(Parent.JF_CFXPercentInfo, 100.000m);
			if (Parent.JF_CFXPercent < ZDecimal.Zero)
			{
				Parent.JF_CFXPercentInfo.AddError(Res.GetString("5B2CAB04-9B10-4D90-AC73-E6D90ED1EBF4", "CFX Percent must be greater than or equal to zero."));
			}
		}

		protected override void CheckJF_CFXMinimum()
		{
			base.CheckJF_CFXMinimum();
			if (Parent.JF_CFXMinimum < ZDecimal.Zero)
			{
				Parent.JF_CFXMinimumInfo.AddError(Res.GetString("46636AD0-DE4A-4C9D-A039-7E1B0BC0F85E", "CFX Minimum must be greater than or equal to zero."));
			}
		}

		protected override void CheckJF_InvoiceCurrencyType()
		{
			base.CheckJF_InvoiceCurrencyType();
			ListValidation.ErrorIfInvalidCode(Parent.JF_InvoiceCurrencyTypeInfo);
		}

		new ExchangeRate Parent
		{
			get { return (ExchangeRate)base.Parent; }
		}

		void CheckItIsNotDuplicate()
		{
			Parent.RemoveRowError(IsDuplicateErrorString);

			var parentCollection = (ExchangeRatesCollection)((IBusinessObjectInternals)Parent).ParentCollections.FirstOrDefault(pc => pc is ExchangeRatesCollection);

			if (parentCollection == null)
			{
				return;
			}

			if (parentCollection.Cast<ExchangeRate>().Any(c => c != Parent && Parent.IsDuplicateOf(c)))
			{
				Parent.AddRowError(IsDuplicateErrorString);
			}
		}

		void CheckItHasEmptyOrgTypeWithNonEmptyOrg()
		{
			if (!Parent.JF_OH_Org.IsEmpty && Parent.JF_OrgType.IsEmpty)
			{
				Parent.JF_OrgTypeInfo.AddError(IsEmptyOrgTypeWithNonEmptyOrg);
			}
		}

		static string IsDuplicateErrorString => Res.GetString("aa1cf448-5d4c-4849-8e42-e3cd4b63dac9", "At least one more record already sets exchange rate for the same currency, organization and role.");

		static string IsEmptyOrgTypeWithNonEmptyOrg => Res.GetString("9ccef650-1023-4c74-a012-44cfbd5c4ba5", "Please select a valid Organization Role for this exchange rate. Organization Role 'All' can only be used when Organization is blank.");
	}
}
