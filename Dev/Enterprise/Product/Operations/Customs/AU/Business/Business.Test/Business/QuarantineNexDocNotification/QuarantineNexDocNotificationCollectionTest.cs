using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(QuarantineNexDocNotificationCollection))]
	sealed class QuarantineNexDocNotificationCollectionTest : ActiveBusinessObjectCollectionTestCase<QuarantineNexDocNotificationCollection>
	{
		protected override QuarantineNexDocNotificationCollection GetCollectionToTest() => new QuarantineNexDocNotificationCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => (QuarantineNexDocNotification)base.GetNewElementToAddToTheCollection();
	}
}
