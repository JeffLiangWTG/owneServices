using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Web.Business
{
	[Serializable]
	public class TransactionPaymentStatusRequest : ITransactionNaturalKeys
	{
		public string OrgCode { get; set; }
		public string CompanyCode { get; set; }
		public string AccLedger { get; set; }
		public string TransactionType { get; set; }
		public string TransactionNumber { get; set; }
		public string JobTransactionNumber { get; set; }
		public string InternalReference { get; set; }

		List<string> ITransactionNaturalKeys.ValidTransactionTypes
		{
			get { return new List<string>(new string[] { "INV", "CRD", "ADJ", "PAY", "REC", "TRF", "CTR", "JNL", "EXX", "OVP", "DSC" }); }
		}

		string ITransactionNaturalKeys.TransactionTypeHint
		{
			get
			{
				return (NoResString)@"A valid TransactionType should be provided. Please, use 
	INV for Invoice,
	CRD for Credit Note,
	ADJ for Ajustment Note,
	PAY for Payment,
	REC for Receipt,
	TRF for Transfer,
	CTR for Contra,
	JNL for Journal,
	EXX for Exchange Difference,
	OVP for Overpayment and
	DSC for Discount.";
			}
		}

		public string Validate()
		{
			return this.ValidateCore();
		}
	}
}
