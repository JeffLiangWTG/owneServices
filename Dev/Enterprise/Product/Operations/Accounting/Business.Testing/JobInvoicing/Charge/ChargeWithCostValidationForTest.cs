using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class ChargeWithCostValidationForTest : ChargeWithCostValidation
	{
		public ChargeWithCostValidationForTest(ChargeWithCostForTest parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		protected override bool HasChequeNumberBeenUsed
		{
			get { return false; }
		}

		protected override string ChequeNumberErrorMessage
		{
			get { return null; }
		}

		protected override string GetDifferenceBetweenChargesForSameTransactionErrorMessage(ZPropertyInfo field, string expected)
		{
			return null;
		}

		protected override string InvoiceNumberErrorMessage
		{
			get { return null; }
		}
	}
}