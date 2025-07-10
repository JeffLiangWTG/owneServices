using CargoWise.EntityFramework;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	[TestedType(typeof(AsycudaPackedItem))]
	sealed class AsycudaPackedItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAdditionalInfosDoesNotCauseExceptionWhenLoadChildEditableObjects()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var item = bill.PackedItems.AddNew();
			var info = item.AdditionalDocuments.AddNew();
			info.CSI_SubType = "TST";
			info.CSI_Code = "INF";
			info.CSI_ReferenceNumber = "RN001";
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var loadedHeader = anotherFactory.Load<AsycudaManifestHeader>(header.PK);

			AssertNoExceptionThrown(() => loadedHeader.LoadChildEditableObjects());
		}

		public void TestGetCusSupportingInfoTypes()
		{
			var packedItem = GetNewBusinessObject() as AsycudaPackedItem;
			CombineAssertions(() =>
			{
				AssertEquals(typeof(AdditionalInfo), ((ICusSupportingInfoTypeSupporter)packedItem).GetCusSupportingInfoTypes()[H7CusSupportingInfoTypeList.Codes.AdditionalInfo]);
				AssertEquals(typeof(AdditionalDocument), ((ICusSupportingInfoTypeSupporter)packedItem).GetCusSupportingInfoTypes()[H7CusSupportingInfoTypeList.Codes.AdditionalDocument]);
				AssertEquals(typeof(PreviousDocument), ((ICusSupportingInfoTypeSupporter)packedItem).GetCusSupportingInfoTypes()[H7CusSupportingInfoTypeList.Codes.PreviousDocument]);
				AssertEquals(typeof(SupportingDocument), ((ICusSupportingInfoTypeSupporter)packedItem).GetCusSupportingInfoTypes()[H7CusSupportingInfoTypeList.Codes.SupportingDocument]);
			});
		}

		public void TestAPI_GrossWeightUQ()
		{
			var packedItem = GetNewBusinessObject() as AsycudaPackedItem;
			Assert("Readonly", packedItem.API_GrossWeightUQInfo.ReadOnly);
		}

		public void TestAPI_NetWeightUQ()
		{
			var packedItem = GetNewBusinessObject() as AsycudaPackedItem;
			Assert("Readonly", packedItem.API_NetWeightUQInfo.ReadOnly);
		}

		public void TestAdditionalDocuments()
		{
			var packedItem = GetNewBusinessObject() as AsycudaPackedItem;
			AssertType<AdditionalDocumentCollection<AdditionalDocument>>(packedItem.AdditionalDocuments);
		}

		public void TestSupportingDocuments()
		{
			var packedItem = GetNewBusinessObject() as AsycudaPackedItem;
			AssertType<SupportingDocumentCollection<SupportingDocument>>(packedItem.SupportingDocuments);
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
