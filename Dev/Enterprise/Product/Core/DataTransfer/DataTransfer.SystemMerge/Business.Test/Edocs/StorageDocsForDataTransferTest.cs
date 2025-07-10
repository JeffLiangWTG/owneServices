using System;
using System.IO;
using System.Text;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.SystemMerge.Business.Testing
{
	[TestedType(typeof(StorageDocsForDataTransfer))]
	internal class StorageDocsForDataTransferTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParentOrgPk()
		{
			// Act
			var docFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var testDoc = docFactory.New<StorageDocsForDataTransfer>();

			// Assert
			AssertEquals("ParentOrgPk initial value", ZGuid.Empty, testDoc.ParentOrgPk);

			// Act
			testDoc.ParentOrgPk = ZGuid.NewZGuid();

			// Assert
			Assert("ParentOrgPk should have a value", !testDoc.ParentOrgPk.IsEmpty);
		}

		public void TestSC_ImageData_WhenExternalStorageIsEnabled()
		{
			// Arrange
			var docFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var testDoc = docFactory.New<StorageDocsForDataTransfer>();
			testDoc.SC_ImageData = ZBlob.Empty;
			testDoc.SC_Date = DateTime.Now;
			docFactory.Save();

			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			{
				var persisterProviderMock = new Mock<IExternalPersisterProvider>();
				var persisterMock = new Mock<IExternalPersister>(MockBehavior.Strict);
				var bytes = Encoding.ASCII.GetBytes("Seek first to understand, then to be understood.");

				persisterProviderMock.Setup(p => p.GetExternalPersister(Core.Constants.EDocsStorageProviders.Code.S3)).Returns(persisterMock.Object);
				persisterMock.Setup(x => x.RetrieveStream(testDoc.PK)).Returns((new MemoryStream(bytes), string.Empty));

				using (ObjectFactory.Substitute(persisterProviderMock.Object))
				{
					// Act
					var newFactory = new DocumentFactoryProvider().GetFactory(Factory);
					var sameDoc = newFactory.Load<StorageDocsForDataTransfer>(testDoc.PK);

					// Assert
					AssertNotNull(nameof(sameDoc), sameDoc);
					AssertEquals(bytes, sameDoc.SC_ImageData);
				}
			}
		}

		public void TestSC_ImageData_WhenDBIsStorageProvider()
		{
			// Arrange
			var docFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var testDoc = docFactory.New<StorageDocsForDataTransfer>();
			testDoc.SC_ImageData = new byte[] { 1, 2, 3 };
			testDoc.SC_Date = ZDateTime.Now;
			docFactory.Save();

			// Act
			var newFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var sameDoc = newFactory.Load<StorageDocsForDataTransfer>(testDoc.PK);

			// Assert
			AssertNotNull(nameof(sameDoc), sameDoc);
			AssertEquals(testDoc.SC_ImageData, sameDoc.SC_ImageData);
		}
	}
}
