using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(ICS2AmendedItemCollection))]
	public sealed class ICS2AmendedItemCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ICS2AmendedItemCollection>
	{
		public void TestAllowNew()
		{
			var collection = GetCollectionToTest();
			Assert(!collection.AllowNew);
		}

		public override void TestAddNew()
		{
			var collection = GetCollectionToTest();
			AssertExceptionThrown<NotSupportedException>(() => collection.AddNew());
		}

		protected override ICS2AmendedItemCollection GetCollectionToTest()
		{
			return new ICS2AmendedItemCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var requestHeader = amendedItemsHeader.ManifestHeader.RequestHeaders.AddNew();
			return new ICS2AmendedItem(amendedItemsHeader, requestHeader);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			amendedItemsHeader = new ICS2AmendedItemsHeader(manifestHeader, EUICS2ReferralRequestTypeList.Codes.CL735_AMD);
		}

		ICS2AmendedItemsHeader amendedItemsHeader;
	}
}
