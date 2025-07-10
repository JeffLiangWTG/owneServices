using System;
using System.Collections.Generic;
using System.Text;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Web.Business
{
	[Serializable]
	public class UpdateInvoicePaymentDetailsRequest : ITransactionNaturalKeys
	{
		public string AccLedger { get; set; }
		public string TransactionType { get; set; }
		public string TransactionNumber { get; set; }
		public string JobTransactionNumber { get; set; }
		public string InternalReference { get; set; }
		public string PaymentReference { get; set; }
		public string CompanyCode { get; set; }
		public string OrgCode { get; set; }
		public DateTime? PaymentDate { get; set; }
		public Decimal AmountPaidInCompanyCurrency { get; set; }

		List<string> ITransactionNaturalKeys.ValidTransactionTypes
		{
			get { return new List<string>(new string[] { "INV", "CRD" }); }
		}

		string ITransactionNaturalKeys.TransactionTypeHint
		{
			get
			{
				return (NoResString)@"A valid TransactionType should be provided. Please, use 
	INV for Invoice or
	CRD for Credit Note.";
			}
		}

		public string ValidateAll(TransactionPaymentDataAccess dataAccess)
		{
			var baseErrors = Validate();
			var errors = new StringBuilder();

			if (!string.IsNullOrEmpty(baseErrors))
			{
				errors.AppendLine(baseErrors);
			}

			if (errors.Length == 0)
			{
				var companyCurrencySubUnitRatio = dataAccess.TryGetCompanyCurrencySubUnitRatio(CompanyCode);
				if (companyCurrencySubUnitRatio is null)
				{
					errors.AppendLine($"Cannot get a valid company currency subunit ratio based on the specific company code: {CompanyCode}.");
				}
				else
				{
					var amountErrors = ValidateAmountPaidInCompanyCurrency(companyCurrencySubUnitRatio.Value);

					if (!string.IsNullOrEmpty(amountErrors))
					{
						errors.AppendLine(amountErrors);
					}
				}
			}

			return errors.Length > 0 ? errors.ToString().Trim() : null;
		}

		public string Validate()
		{
			StringBuilder errors = new StringBuilder();
			string coreErrors = this.ValidateCore();
			if (!string.IsNullOrEmpty(coreErrors))
			{
				errors.AppendLine(coreErrors);
			}
			return errors.Length > 0 ? errors.ToString().Trim() : null;
		}

		string ValidateAmountPaidInCompanyCurrency(int companyCurrencySubUnitRatio)
		{
			var errors = new StringBuilder();
			if (AmountPaidInCompanyCurrency == Decimal.Zero)
			{
				errors.AppendLine((NoResString)"AmountPaidInCompanyCurrency cannot be zero.");
			}
			else if (((AmountPaidInCompanyCurrency * companyCurrencySubUnitRatio) % 1) != Decimal.Zero)
			{
				errors.AppendLine((NoResString)"AmountPaidInCompanyCurrency exceed the number of decimals that are allowed by the company's currency.");
			}
			return errors.Length > 0 ? errors.ToString().Trim() : null;
		}
	}
}
