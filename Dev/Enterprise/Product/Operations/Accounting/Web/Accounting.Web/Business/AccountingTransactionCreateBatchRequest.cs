using System;
using System.Text;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Web.Business
{
	[Serializable]
	public class AccountingTransactionCreateBatchRequest
	{
		public AccountingTransactionCreateBatchRequest()
		{
		}

		public string CompanyCode { get; set; }

		public virtual string Validate()
		{
			StringBuilder errors = new StringBuilder();

			if (string.IsNullOrEmpty(CompanyCode))
			{
				errors.AppendLine((NoResString)"CompanyCode cannot be empty. Use " + Enterprise.Core.Constants.ProductName + (NoResString)" Company Code.");
			}

			return errors.Length > 0 ? errors.ToString().Trim() : null;
		}
	}
}
