using System;
using Enterprise.Accounting.Business.Base.Transaction.Testing;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	public class ARExchangeDifferenceValidationTest : TransactionHeaderValidationTest
	{
		protected override bool ShouldTestAH_AG
		{
			get { return true; }
		}

		protected override Type HeaderType
		{
			get { return typeof(ARExchangeDifference); }
		}
	}
}
