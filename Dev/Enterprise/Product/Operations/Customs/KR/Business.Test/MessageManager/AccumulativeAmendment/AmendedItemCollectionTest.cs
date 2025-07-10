using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.DataItemIDAttribute;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(AmendedItemCollection))]
	sealed class AmendedItemCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AmendedItemCollection>
	{
		protected override AmendedItemCollection GetCollectionToTest() => new AmendedItemCollection(new List<AmendedItem>() { new AmendedItem() }, new GOVCBR5BADataItemIDList());
		protected override BusinessObject GetNewElementToAddToTheCollection() => new AmendedItem();

		public void TestGetChangeType()
		{
			var amendedItem1 = new AmendedItem();
			var amendedItem2 = new AmendedItem();

			amendedItem1.ChangeType = ChangeType.DutyTaxRelatedAndNormal;
			amendedItem2.ChangeType = ChangeType.Normal;
			var dutyTaxRelatedAndNormalTypeList = new AmendedItemCollection(new List<AmendedItem>() { amendedItem1, amendedItem2 }, new GOVCBR5BADataItemIDList());
			AssertEquals(ChangeType.DutyTaxRelatedAndNormal, dutyTaxRelatedAndNormalTypeList.GetChangeType());

			amendedItem1.ChangeType = ChangeType.DutyTaxRelated;
			var mixTypeList = new AmendedItemCollection(new List<AmendedItem>() { amendedItem1, amendedItem2 }, new GOVCBR5BADataItemIDList());
			AssertEquals(ChangeType.DutyTaxRelatedAndNormal, mixTypeList.GetChangeType());

			amendedItem2.ChangeType = ChangeType.DutyTaxRelated;
			var dutyTaxRelatedTypeList = new AmendedItemCollection(new List<AmendedItem>() { amendedItem1, amendedItem2 }, new GOVCBR5BADataItemIDList());
			AssertEquals(ChangeType.DutyTaxRelated, dutyTaxRelatedTypeList.GetChangeType());

			amendedItem1.ChangeType = ChangeType.Normal;
			amendedItem2.ChangeType = ChangeType.Normal;
			var normalTypeList = new AmendedItemCollection(new List<AmendedItem>() { amendedItem1, amendedItem2 }, new GOVCBR5BADataItemIDList());
			AssertEquals(ChangeType.Normal, normalTypeList.GetChangeType());
		}
	}
}
