using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(KREntryLineDetailsViewCollection))]
	sealed class KREntryLineDetailsViewCollectionTest : ActiveBusinessObjectCollectionTestCase<KREntryLineDetailsViewCollection>
	{
		protected override KREntryLineDetailsViewCollection GetCollectionToTest() => new KREntryLineDetailsViewCollection(Factory);
	}
}
