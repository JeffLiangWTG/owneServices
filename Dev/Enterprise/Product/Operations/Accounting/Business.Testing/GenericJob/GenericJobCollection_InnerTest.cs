using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GenericJob.Testing
{
	[TestedType(typeof(GenericJobCollection))]
	public class GenericJobCollection_InnerTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GenericJobCollection(Factory);
		}
	}
}
