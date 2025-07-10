using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(ICS2AmendedItem))]
	sealed class ICS2AmendedItemTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidation()
		{
			var item = (ICS2AmendedItem)GetNewBusinessObject();
			AssertType<ICS2AmendedItemValidation>(item.Validation);
		}

		protected override BusinessObject GetNewBusinessObject() => new ICS2AmendedItem(amendedItemsHeader, amendedItemsHeader.ManifestHeader.RequestHeaders.AddNew());

		protected override void SetUp()
		{
			base.SetUp();

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			amendedItemsHeader = new ICS2AmendedItemsHeader(manifestHeader, EUICS2ReferralRequestTypeList.Codes.CL735_AMD);
		}

		ICS2AmendedItemsHeader amendedItemsHeader;
	}
}
