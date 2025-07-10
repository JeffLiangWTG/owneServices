using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(GroupCompanyChargeCollection))]
	public class GroupCompanyChargeCollectionTest : NonPersistentBusinessObjectCollectionTestCase<GroupCompanyChargeCollection>
	{
		#region Implementation

		protected override GroupCompanyChargeCollection GetCollectionToTest()
		{
			return new GroupCompanyChargeCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var charge = Factory.NewWithValidTestData<Charge>();

			return new GroupCompanyCharge(null, charge, GroupCompanyCharge.AcceptAction.Create, Factory);
		}

		#endregion
	}
}
