using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(GroupCompanyCharge))]
	public class GroupCompanyChargeTest : NonPersistentBusinessObjectTestCase
	{
		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			var job = Factory.NewJobForTesting<Job>();
			var charge = TestObjectCreator.CreateChargeWithPaymentBasis(job, TestObjectCreator.FRT, GlbCompany.CurrentCompany.OrgProxy, 123m);

			return new GroupCompanyCharge(charge, null, GroupCompanyCharge.AcceptAction.Create, Factory);
		}

		#endregion

		#region Implementation

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		#endregion
	}
}
