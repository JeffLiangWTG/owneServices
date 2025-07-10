using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Messaging.Business.Testing
{
	[TestedType(typeof(EDIInterchangeCollection))]
	public class EDIInterchangeCollectionTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new EDIInterchangeCollection(Factory);
		}
	}
}
