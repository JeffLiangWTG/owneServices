using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.RemotePrinting.Server.Business;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Server.Testing.Business
{
	[TestedType(typeof(SignalRClientCollection))]
	class SignalRClientCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SignalRClientCollection>
	{
		protected override SignalRClientCollection GetCollectionToTest()
		{
			return new SignalRClientCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SignalRClientBusinessObject();
		}
	}
}
