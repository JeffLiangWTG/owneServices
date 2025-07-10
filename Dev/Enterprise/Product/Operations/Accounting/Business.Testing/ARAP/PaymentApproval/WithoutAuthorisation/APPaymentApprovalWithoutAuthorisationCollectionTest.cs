using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	[TestedType(typeof(APPaymentApprovalWithoutAuthorisationCollection))]
	public class APPaymentApprovalWithoutAuthorisationCollectionTest : PaymentApprovalBaseCollectionTest
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new APPaymentApprovalWithoutAuthorisationCollection(Factory);
		}

		#endregion
	}
}
