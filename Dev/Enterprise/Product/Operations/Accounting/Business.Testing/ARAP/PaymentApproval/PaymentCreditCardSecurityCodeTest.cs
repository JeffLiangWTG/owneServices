using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	[TestedType(typeof(PaymentCreditCardSecurityCode))]
	internal sealed class PaymentCreditCardSecurityCodeTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PaymentCreditCardSecurityCode();
		}

		#endregion
	}
}
