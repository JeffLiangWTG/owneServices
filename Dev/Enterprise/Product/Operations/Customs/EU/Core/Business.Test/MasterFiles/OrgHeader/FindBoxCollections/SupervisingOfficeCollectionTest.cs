using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.MasterFiles.Testing
{
	[TestedType(typeof(SupervisingOfficeCollection))]
	class SupervisingOfficeCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetFilterBusinessObjectDefaults()
		{
			Assert("FilterBusinessObjectDefaults should contain default for \"Organisation Types\"", GetCollectionToTest().FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1"));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new SupervisingOfficeCollection(Factory);
		}
	}
}
