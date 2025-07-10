using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	[TestedType(typeof(AsycudaPackedItem))]
	sealed class AsycudaPackedItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAdditionalInfos()
		{
			var packedItem = GetNewBusinessObject() as AsycudaPackedItem;
			AssertType<AdditionalInfoCollection<AdditionalInfo>>(packedItem.AdditionalInfos);
		}

		public void TestSupportingDocuments()
		{
			var packedItem = GetNewBusinessObject() as AsycudaPackedItem;
			AssertType<SupportingDocumentCollection<SupportingDocument>>(packedItem.SupportingDocuments);
		}

		public void TestPreviousDocuments()
		{
			var packedItem = GetNewBusinessObject() as AsycudaPackedItem;

			AssertType<PreviousDocumentCollection<PreviousDocument>>(packedItem.PreviousDocuments);
		}

		public void TestTariffShouldNotContainsDotsAfterSet()
		{
			var packedItem = GetNewBusinessObject() as AsycudaPackedItem;
			packedItem.API_Tariff = "123.456";

			AssertEquals("123456", packedItem.API_Tariff);
		}

		public void TestAPIFormattedTariffDescription()
		{
			var packedItem = GetNewBusinessObject() as AsycudaPackedItem;
			var resourceStringDataAttribute = packedItem.API_FormattedTariffInfo.GetAttribute<ResourceStringDataAttribute>();

			AssertEquals("Commodity Code (Combined Nomenclature Code) associated with the item.", resourceStringDataAttribute.FullDescription);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return bill.PackedItems.AddNew();
		}
	}
}
