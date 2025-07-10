using System.Reflection;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core.Writing;

namespace Enterprise.Customs.ASYCUDAManifest.Business.UniversalDataTransfer.Testing
{
	sealed class AsycudaManifestHeaderDataObjectWriterTest : DataObjectWriterTest
	{
		public void TestAsycudaBillDataObjectWriterType()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var headerHelper = new AsycudaManifestHeaderDataObjectWriterHelper(header);
			var writer = new AsycudaManifestHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, header)));
			var createNewAsycudaBillDataObjectWriter = typeof(AsycudaManifestHeaderDataObjectWriter).GetMethod("CreateNewAsycudaBillDataObjectWriter", BindingFlags.Instance | BindingFlags.NonPublic);
			var type = createNewAsycudaBillDataObjectWriter.Invoke(writer, new object[] { headerHelper }).GetType();
			AssertEquals("AsycudaBillDataObjectWriter", typeof(AsycudaBillDataObjectWriter), type);
		}
	}
}
