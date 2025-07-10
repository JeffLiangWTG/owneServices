using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(CusStorageDocPivot))]
	sealed class CusStorageDocPivotTest : Customs.Business.Testing.BaseCusStorageDocPivotTest
	{
		public void TestValidation()
		{
			var pivot = manifestHeader.EDocPivotCollection.AddNew();
			AssertType<CusStorageDocPivotValidation>(pivot.Validation);
		}

		public override void TestParent()
		{
			var pivot = manifestHeader.EDocPivotCollection.AddNew();
			AssertEquals(manifestHeader, pivot.Parent);
		}

		public void TestCSD_DocType()
		{
			var pivot = manifestHeader.EDocPivotCollection.AddNew();
			Assert(pivot.CSD_DocTypeInfo.ReadOnly);
		}

		public void TestFieldCaption()
		{
			var pivot = manifestHeader.EDocPivotCollection.AddNew();

			var fileNameRESAttribute = pivot.CSD_StorageDocReferenceInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("File Name", fileNameRESAttribute.Caption);

			var descriptionRESAttribute = pivot.CSD_DescriptionInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("Description", descriptionRESAttribute.Caption);

			var fileTypeRESAttribute = pivot.CSD_DocTypeInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("File Type", fileTypeRESAttribute.Caption);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = manifestHeader.EDocPivotCollection.AddNew();
			result.CSD_DocType = "TY1";
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
		}

		AsycudaManifestHeader manifestHeader;
	}
}
