using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	[TestedType(typeof(ViewMatchGroupCollection))]
	public class ViewMatchGroupCollectionTestCase : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ViewMatchGroupCollection(Factory);
		}
	}
}
