using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[TestedType(typeof(DocCMRMessageChargeItemCollection))]
	sealed class DocCMRMessageChargeItemCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocCMRMessageChargeItemCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DocCMRMessageChargeItem("ChargeType", 0m);
		}

		protected override DocCMRMessageChargeItemCollection GetCollectionToTest()
		{
			return new DocCMRMessageChargeItemCollection(Factory);
		}
	}
}
