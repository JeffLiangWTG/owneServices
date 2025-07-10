using System;

namespace Enterprise.Accounting.Business.ARAP.Overpayment.Testing
{
	public class APOverpaymentValidationTest : OverpaymentValidationTest
	{
		protected override bool ShouldTestAH_AG
		{
			get { return true; }
		}

		protected override Type HeaderType
		{
			get { return typeof(APOverpayment); }
		}
	}
}
