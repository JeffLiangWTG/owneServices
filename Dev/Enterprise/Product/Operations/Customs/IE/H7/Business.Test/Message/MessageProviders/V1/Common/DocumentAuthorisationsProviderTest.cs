using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.AIS;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	public class DocumentAuthorisationsProviderTest : DataProviderTestCase<DocumentAuthorisationsProvider>
	{
		public void TestPreviousDocuments()
		{
			SetUpTestData();
			var providerWithNoPreviousDocuments = GetProvider();
			AssertEquals(0, providerWithNoPreviousDocuments.PreviousDocuments.Count);

			var previousDocument1 = packedItem.PreviousDocuments.AddNew();
			var previousDocument2 = packedItem.PreviousDocuments.AddNew();

			var notPreviousDocument = packedItem.AdditionalDocuments.AddNew();

			var result = Provider.PreviousDocuments.ToList();
			AssertEquals(2, result.Count);
			AssertType<DocumentProvider>(result[0]);
		}

		public void TestSupportingDocuments()
		{
			SetUpTestData();
			var providerWithNoSupportingDocuments = GetProvider();
			AssertEquals(0, providerWithNoSupportingDocuments.SupportingDocuments.Count);

			var supportingDocument1 = packedItem.SupportingDocuments.AddNew();
			var supportingDocument2 = packedItem.SupportingDocuments.AddNew();

			var notSupportingDocument = packedItem.AdditionalDocuments.AddNew();

			var result = Provider.SupportingDocuments.ToList();
			AssertEquals(2, result.Count);
			AssertType<DocumentProvider>(result[0]);
		}

		public void TestAdditionalReference()
		{
			SetUpTestData();
			var notREFDocument = packedItem.AdditionalDocuments.AddNew();
			notREFDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;

			var providerWithNoAdditionalReference = GetProvider();
			AssertNull(providerWithNoAdditionalReference.AdditionalReference);

			var document = packedItem.AdditionalDocuments.AddNew();
			document.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			document.CSI_Code = "CODE1";
			document.CSI_ReferenceNumber = "NUMBER1";

			var document2 = packedItem.AdditionalDocuments.AddNew();
			document2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			document2.CSI_Code = "CODE2";
			document2.CSI_ReferenceNumber = "NUMBER2";

			CombineAssertions(() =>
			{
				AssertType<AdditionalReferenceProvider>(Provider.AdditionalReference);
				AssertEquals("Type", "CODE1", Provider.AdditionalReference.Type);
				AssertEquals("Number", "NUMBER1", Provider.AdditionalReference.Number);
			});
		}

		public void TestAdditionalInformations()
		{
			SetUpTestData();
			var providerWithNoAdditionalInformations = GetProvider();
			AssertEquals(0, providerWithNoAdditionalInformations.AdditionalInformations.Count);

			var additionalInfo1 = packedItem.AdditionalDocuments.AddNew();
			additionalInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			var additionalInfo2 = packedItem.AdditionalDocuments.AddNew();
			additionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;

			var notAdditionalInfo = packedItem.SupportingDocuments.AddNew();

			var result = Provider.AdditionalInformations.ToList();
			AssertEquals(2, result.Count);
			AssertType<CcQualifierAdditionalInformationProvider>(result[0]);
		}

		public void TestReferenceNumberUCR()
		{
			AssertEquals(string.Empty, Provider.ReferenceNumberUCR);
			bill.ABL_UCRNumber = "1111";
			AssertEquals("1111", Provider.ReferenceNumberUCR);
		}

		protected override DocumentAuthorisationsProvider GetProvider()
		{
			SetUpTestData();
			return new DocumentAuthorisationsProvider(packedItem);
		}

		void SetUpTestData()
		{
			if (packedItem == null)
			{
				var header = Factory.New<AsycudaManifestHeader>();
				bill = header.Bills.AddNew();
				pack = bill.Packs.AddNew();
				packedItem = bill.PackedItems.AddNew();
				packedItem.AsycudaPackPackedItemPivots.AddPivotFor(pack);
			}
		}
		AsycudaBill bill;
		AsycudaPack pack;
		AsycudaPackedItem packedItem;
	}
}
