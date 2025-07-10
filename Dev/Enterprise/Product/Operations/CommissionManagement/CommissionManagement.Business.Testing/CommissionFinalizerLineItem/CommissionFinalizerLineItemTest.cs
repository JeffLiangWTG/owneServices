using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(CommissionFinalizerLineItem))]
	public class CommissionFinalizerLineItemTest : NonPersistentBusinessObjectTestCase
	{
		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			var commissionLine = Factory.New<ViewCommissionLine>();
			return new CommissionFinalizerLineItem(commissionLine);
		}

		#endregion
	}
}
