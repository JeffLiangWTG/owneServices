using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Messaging.Business.Testing
{
	[TestedType(typeof(NonDependentEDIMessageCollection))]
	public class NonDependentEDIMessageCollection_Test : CargoWise.EntityFramework.Testing.BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new NonDependentEDIMessageCollection(Factory);
		}
	}
}
