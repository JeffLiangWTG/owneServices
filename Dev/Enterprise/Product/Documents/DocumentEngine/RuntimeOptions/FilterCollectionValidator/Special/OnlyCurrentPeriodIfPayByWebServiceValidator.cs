using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	/// <summary>
	/// TODO: Update summary.
	/// </summary>
	public class OnlyCurrentPeriodIfPayByWebServiceValidator : FilterCollectionValidator
	{
		public OnlyCurrentPeriodIfPayByWebServiceValidator()
			: base()
		{
		}

		public const string NoAgeing = "NON";

		public override bool IsValid(FilterField filterToValidate)
		{
			if (!filterToValidate.IsEmpty && !filterToValidate.HasErrors &&
				IsPaymentWebServiceEnabled)
			{
				if (filterToValidate is SingleAccountingPeriodField)
				{
					var filter = (SingleAccountingPeriodField)filterToValidate;
					return PeriodCalculator.IsCurrentPeriod(filter.SinglePeriod);
				}
				else if (filterToValidate is MultipleChoice)
				{
					var filter = (MultipleChoice)filterToValidate;
					return filter.ZValue == NoAgeing;
				}
				else if (filterToValidate is DateRangeField)
				{
					var filter = (DateRangeField)filterToValidate;
					return filter.ValueHigh.Date >= ZDateTime.Today;
				}
				else if (filterToValidate is DateField)
				{
					var filter = (DateField)filterToValidate;
					return filter.Value.Date >= ZDateTime.Today;
				}
			}
			return base.IsValid(filterToValidate);
		}

		public override string GetErrorMessage(FilterField filterToValidate)
		{
			if (filterToValidate is SingleAccountingPeriodField)
			{
				return Res.GetString("027c6e1e-5d27-4c10-95a3-95ef0f51801a", "With the Invoice Payment Web Service enabled it must be only the current period");
			}
			else if (filterToValidate is MultipleChoice)
			{
				return Res.GetString("074f4003-dcf0-45ed-a119-e1489724ba88", "With the Invoice Payment Web Service enabled it must be No Aging");
			}
			else if (filterToValidate is DateRangeField)
			{
				return Res.GetString("64e381ec-11b9-483f-9c35-f31b5d53afec", "With the Invoice Payment Web Service enabled Date To must be Today or a day in the future");
			}
			else if (filterToValidate is DateField)
			{
				return Res.GetString("1f37d2a6-4eba-4b4f-91ae-8d16c0ce87dc", "With the Invoice Payment Web Service enabled Date must be Today or a day in the future");
			}
			else
			{
				return base.GetErrorMessage(filterToValidate);
			}
		}

		public bool IsPaymentWebServiceEnabled
		{
			get { return isPaymentWebServiceEnabled ?? (isPaymentWebServiceEnabled = ObjectFactory.Get<IAccounting>().IsInvoicePaymentWebServiceEnabled(GlbCompany.CurrentCompany.PK.ToGuid())).Value; }
		}
		bool? isPaymentWebServiceEnabled;

		public AccountingPeriodCalculator PeriodCalculator
		{
			get { return periodCalculator ?? (periodCalculator = new AccountingPeriodCalculator(Filters.Count > 0 ? Filters[0].Factory : new BusinessObjectFactory())); }
		}
		AccountingPeriodCalculator periodCalculator;
	}
}
