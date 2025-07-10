using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(GOVCBR5TVLineMessageData))]
	sealed class GOVCBR5TVLineMessageDataTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new GOVCBR5TVLineMessageData(new GOVCBR5TVMessageData(Factory));
	}
}
