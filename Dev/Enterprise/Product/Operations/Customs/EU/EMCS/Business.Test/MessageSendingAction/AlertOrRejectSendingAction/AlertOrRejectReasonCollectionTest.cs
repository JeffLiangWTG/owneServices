using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(AlertOrRejectReasonCollection))]
	sealed class AlertOrRejectReasonCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AlertOrRejectReasonCollection>
	{
		protected override AlertOrRejectReasonCollection GetCollectionToTest() => new AlertOrRejectReasonCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new AlertOrRejectReason(Factory);
	}
}
