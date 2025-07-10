using System.Reflection;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.IE.H7.Business.UniversalDataTransfer;
using Enterprise.UniversalDataBuss.Integration;
using Moq;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	sealed class IEH7AsycudaForCustomsDeclarationDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestCreateNewAsycudaBillEntryInstructionDataObjectWriter()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			Mock<IDataWritingManager> mockManager = new Mock<IDataWritingManager>();
			Mock<AsycudaManifestHeaderDataObjectWriterHelper> mockHelper = new Mock<AsycudaManifestHeaderDataObjectWriterHelper>(header);
			IEH7AsycudaForCustomsDeclarationDataObjectWriter writer = new IEH7AsycudaForCustomsDeclarationDataObjectWriter(mockManager.Object, mockHelper.Object);

			var methodInfo = typeof(IEH7AsycudaForCustomsDeclarationDataObjectWriter).GetMethod("CreateNewAsycudaBillEntryInstructionDataObjectWriter", BindingFlags.NonPublic | BindingFlags.Instance);
			var dataObjectWriter = methodInfo.Invoke(writer, null);

			AssertType<IEH7AsycudaBillEntryInstructionDataObjectWriter<EU.H7.Business.AsycudaBill>>(dataObjectWriter);
		}
	}
}
