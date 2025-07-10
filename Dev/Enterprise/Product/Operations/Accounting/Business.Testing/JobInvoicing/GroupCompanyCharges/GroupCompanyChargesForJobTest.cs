using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(GroupCompanyChargesForJob))]
	public class GroupCompanyChargesForJobTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var creator = new TestObjectCreator(Factory);
			var bizO = new GroupCompanyChargesForJob(creator.Job1);

			return bizO;
		}
	}
}
