using System;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.USSalesTax
{
	[Immutable]
	public sealed class CalculationResult : IEquatable<CalculationResult>
	{
		public CalculationResult(
			decimal totalSalesTaxAmount,
			decimal totalInvoiceAmountExcludingSalesTax = 0m,
			string warningMessage = "",
			string submissionStatus = "")
		{
			TotalSalesTaxAmount = totalSalesTaxAmount;
			TotalInvoiceAmountExcludingSalesTax = totalInvoiceAmountExcludingSalesTax;
			WarningMessage = warningMessage;
			SubmissionStatus = submissionStatus;
		}

		public static readonly CalculationResult Zero = new CalculationResult(0m);

		public decimal TotalInvoiceAmountExcludingSalesTax { get; }

		public decimal TotalSalesTaxAmount { get; }

		public string WarningMessage { get; }

		public string SubmissionStatus { get; }

		public CalculationResult WithInvoiceAmountExcludingSalesTax(decimal amount)
			=> new CalculationResult(
				TotalSalesTaxAmount,
				totalInvoiceAmountExcludingSalesTax: amount,
				warningMessage: WarningMessage,
				submissionStatus: SubmissionStatus
			);

		#region IEquatable

		public override bool Equals(object obj)
			=> obj is CalculationResult other && Equals(other);

		public bool Equals(CalculationResult other)
			=> this.TotalInvoiceAmountExcludingSalesTax == other.TotalInvoiceAmountExcludingSalesTax
			&& this.TotalSalesTaxAmount == other.TotalSalesTaxAmount
			&& this.WarningMessage == other.WarningMessage
			&& this.SubmissionStatus == other.SubmissionStatus;

		public override int GetHashCode()
			=> this.TotalInvoiceAmountExcludingSalesTax.GetHashCode()
			^ this.TotalSalesTaxAmount.GetHashCode()
			^ this.WarningMessage.GetHashCode()
			^ this.SubmissionStatus.GetHashCode();

		#endregion
	}
}
