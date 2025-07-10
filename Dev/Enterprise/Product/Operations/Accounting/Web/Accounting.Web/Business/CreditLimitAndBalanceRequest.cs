using System;
using System.Text;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Web.Business
{
	[Serializable]
	public class CreditLimitAndBalanceRequest
	{
		public CreditLimitAndBalanceRequest()
		{
			OverdueAgingPeriod = 30;
		}

		public string OrgCode { get; set; }
		public string CompanyCode { get; set; }
		public string AccLedger { get; set; }
		public int OverdueAgingPeriod { get; set; }
		public string BalanceOverdueAgingOption { get; set; }

		internal string BalanceOverdueAgingOptionPossibleValues
		{
			get
			{
				return string.Format((NoResString)"Possible values: {0}, {1}, {2}, or {3}.",
					Constants.BalanceOverdueAgingOption.Balance,
					Constants.BalanceOverdueAgingOption.RecognisedAndUnrecognisedRevenue,
					Constants.BalanceOverdueAgingOption.Overdue,
					Constants.BalanceOverdueAgingOption.Aging);
			}
		}

		internal string Validate()
		{
			StringBuilder errors = new StringBuilder();

			if (string.IsNullOrEmpty(OrgCode))
			{
				errors.AppendLine((NoResString)"OrgCode cannot be empty. Use " + Enterprise.Core.Constants.ProductName + (NoResString)" Organization Code or Legacy system code.");
			}

			if (OverdueAgingPeriod <= 0)
			{
				errors.AppendLine((NoResString)"OverdueAgingPeriod must be greater than zero.");
			}

			if (string.IsNullOrEmpty(CompanyCode))
			{
				errors.AppendLine((NoResString)"CompanyCode cannot be empty. Use " + Enterprise.Core.Constants.ProductName + (NoResString)" Company Code.");
			}

			if (string.IsNullOrEmpty(BalanceOverdueAgingOption))
			{
				errors.AppendLine(string.Format((NoResString)"BalanceOverdueAgingOption cannot be empty.  {0}", BalanceOverdueAgingOptionPossibleValues));
			}
			else if (BalanceOverdueAgingOption != Constants.BalanceOverdueAgingOption.Balance
				&& BalanceOverdueAgingOption != Constants.BalanceOverdueAgingOption.RecognisedAndUnrecognisedRevenue
				&& BalanceOverdueAgingOption != Constants.BalanceOverdueAgingOption.Overdue
				&& BalanceOverdueAgingOption != Constants.BalanceOverdueAgingOption.Aging)
			{
				errors.AppendLine(string.Format((NoResString)"Invalid value provided for BalanceOverdueAgingOption.  {0}", BalanceOverdueAgingOptionPossibleValues));
			}

			return errors.Length > 0 ? errors.ToString().Trim() : null;
		}
	}
}
