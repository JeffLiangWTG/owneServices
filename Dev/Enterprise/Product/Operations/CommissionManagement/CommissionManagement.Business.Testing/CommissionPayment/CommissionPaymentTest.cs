using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(CommissionPayment))]
	public class CommissionPaymentTest : NonPersistentBusinessObjectTestCase
	{
		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CommissionPayment(Factory, Enumerable.Empty<ViewCommissionLine>());
		}

		#endregion
	}
}
