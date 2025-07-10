using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Messaging.Business.Testing
{
	[TestedType(typeof(EDICommunicationsModeCollection))]
	public class EDICommunicationsModeCollectionTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new EDICommunicationsModeCollection(Factory);
		}
	}
}
