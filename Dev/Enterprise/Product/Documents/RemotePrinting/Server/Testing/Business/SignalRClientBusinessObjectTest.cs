using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.RemotePrinting.Server.Business;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Server.Testing.Business
{
	[TestedType(typeof(SignalRClientBusinessObject))]
	class SignalRClientBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SignalRClientBusinessObject();
		}
	}
}
