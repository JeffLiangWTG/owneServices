using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	[TestedType(typeof(JobConsolCostAttribCollection))]
	public class JobConsolCostAttribCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new JobConsolCostAttribCollection(Factory.New<JobConsolCost>());
		}
	}
}
