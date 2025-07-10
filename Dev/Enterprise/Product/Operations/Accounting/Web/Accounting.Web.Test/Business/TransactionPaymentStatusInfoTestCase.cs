using NUnit.Framework;

namespace Enterprise.Accounting.Web.Business.Testing
{
	[TestedType(typeof(TransactionPaymentStatusInfo))]
	public class TransactionPaymentStatusInfoTestCase : TransactionPaymentStatusRequestTestCase
	{
		#region Implementation

		protected override ITransactionNaturalKeys GetNewRequest()
		{
			return new TransactionPaymentStatusInfo();
		}

		#endregion
	}
}
