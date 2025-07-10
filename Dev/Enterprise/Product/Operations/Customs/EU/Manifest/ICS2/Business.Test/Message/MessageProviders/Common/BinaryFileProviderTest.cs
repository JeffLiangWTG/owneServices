using System;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class BinaryFileProviderTest : DataProviderTestCase<BinaryFileProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("CusStorageDocPivot missing", () => GenerateProvider(null));
				AssertNoExceptionThrown(() => GenerateProvider(pivot));
			});
		}

		public void TestFilename()
		{
			AssertEquals("Filename", "Test1.pdf", Provider.Filename);
		}

		public void TestDescription()
		{
			AssertEquals("Description", "Description", Provider.Description);
		}

		public void TestIdentification()
		{
			AssertEquals("Identification", $"cid:{pivot.CSD_StorageDocReference}", Provider.Identification);
		}

		public void TestMIME()
		{
			AssertEquals("MIME", "application/pdf", Provider.MIME);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var eDoc = manifestHeader.DocManagerInfo.AddFileOrDocument(new byte[1], "Test1.pdf", "PDF");
			pivot = manifestHeader.EDocPivotCollection.AddNew();
			pivot.CSD_DocType = "TY1";
			pivot.CSD_StorageDocReference = eDoc.UniqueKey;
			pivot.CSD_Description = "Description";
		}
		CusStorageDocPivot pivot;

		BinaryFileProvider GenerateProvider(CusStorageDocPivot pivot) => new BinaryFileProvider(pivot);

		protected sealed override BinaryFileProvider GetProvider()
		{
			return GenerateProvider(pivot);
		}
	}
}
