using System;
using System.Collections.Generic;

namespace Enterprise.Accounting.Web.Business
{
	[Serializable]
	public class CreditLimitAndBalanceResponse : ResponseBase
	{
		public List<CreditLimitAndBalanceInfo> CreditLimitAndBalanceDetails
		{
			get { return creditLimitAndBalanceDetails; }
			set { creditLimitAndBalanceDetails = value; }
		}
		List<CreditLimitAndBalanceInfo> creditLimitAndBalanceDetails = new List<CreditLimitAndBalanceInfo>();
	}
}
