using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestedType(typeof(AdditionalDocument))]
	sealed class AdditionalDocumentTest : CusSupportingInfoTest<AdditionalDocument>
	{
		public void TestMaxLength()
		{
			var additionalDocument = Factory.New<AdditionalDocument>();
			CombineAssertions(() =>
			{
				AssertEquals(3, additionalDocument.CSI_SubTypeInfo.MaxLength);
				AssertEquals(5, additionalDocument.CSI_CodeInfo.MaxLength);
			});
		}

		public void TestCaptionResourceString()
		{
			var additionalDocument = Factory.New<AdditionalDocument>();
			CombineAssertions(() =>
			{
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(additionalDocument.CSI_SubTypeInfo, (string[])null, "Kind", "Kind", "Kind", "Additional document list type.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(additionalDocument.CSI_CodeInfo, (string[])null, "Full Type", "Type", "Full Type", "Code within relevant additional document list.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(additionalDocument.CSI_ReferenceNumberInfo, (string[])null, "Reference", "Ref.", "Reference", "Additional document reference number.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(additionalDocument.CSI_DescriptionInfo, (string[])null, "Description", "Desc.", "Description", "Additional document description.");
			});
		}

		public void TestCSI_CodeReadOnly()
		{
			var additionalDocument = Factory.New<AdditionalDocument>();

			Assert("CSI_Code should be readonly when CSI_SubType is not entered", additionalDocument.CSI_CodeInfo.ReadOnly);

			additionalDocument.CSI_SubType = "XXX";
			Assert("CSI_Code should be readonly when CSI_SubType is not in the list", additionalDocument.CSI_CodeInfo.ReadOnly);

			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			Assert("CSI_Code should be enabled when CSI_SubType is valid", !additionalDocument.CSI_CodeInfo.ReadOnly);
		}

		public void TestCSI_ReferenceNumberReadOnly()
		{
			var additionalDocument = Factory.New<AdditionalDocument>();

			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			Assert("CSI_ReferenceNumber should be readonly when CSI_SubType is INF", additionalDocument.CSI_ReferenceNumberInfo.ReadOnly);
			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			Assert("CSI_ReferenceNumber should be enabled when CSI_SubType is REF", !additionalDocument.CSI_ReferenceNumberInfo.ReadOnly);
			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			Assert("CSI_ReferenceNumber should be enabled when CSI_SubType is INF", !additionalDocument.CSI_ReferenceNumberInfo.ReadOnly);
		}

		public void TestCSI_DescriptionReadOnly()
		{
			var additionalDocument = Factory.New<AdditionalDocument>();

			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			Assert("CSI_DescriptionInfo should be enabled when CSI_SubType is INF", !additionalDocument.CSI_DescriptionInfo.ReadOnly);
			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			Assert("CSI_DescriptionInfo should be readonly when CSI_SubType is REF", additionalDocument.CSI_DescriptionInfo.ReadOnly);
			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			Assert("CSI_DescriptionInfo should be readonly when CSI_SubType is INF", additionalDocument.CSI_DescriptionInfo.ReadOnly);
		}

		public void TestLookups()
		{
			var additionalDocument = Factory.New<AdditionalDocument>();
			AssertType<AdditionalDocumentLookups>(additionalDocument.Lookups);
		}

		public void TestValidation()
		{
			var additionalDocument = Factory.New<AdditionalDocument>();
			AssertType<AdditionalDocumentValidation>(additionalDocument.Validation);
		}

		protected override IEnumerable<AdditionalDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			yield return header.AdditionalDocuments.AddNew();

			var bill = header.Bills.AddNew();
			yield return bill.AdditionalDocuments.AddNew();

			var packedItem = bill.PackedItems.AddNew();
			yield return packedItem.AdditionalDocuments.AddNew();
		}
	}
}
