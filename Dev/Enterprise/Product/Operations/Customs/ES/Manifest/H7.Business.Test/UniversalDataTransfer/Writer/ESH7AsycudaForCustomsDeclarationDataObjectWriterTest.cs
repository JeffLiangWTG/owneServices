using System.Reflection;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.ES.Manifest.H7.Business.UniversalDataTransfer;
using Enterprise.UniversalDataBuss.Integration;
using Moq;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	sealed class ESH7AsycudaForCustomsDeclarationDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestCreateNewAsycudaBillEntryInstructionDataObjectWriter()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			Mock<IDataWritingManager>  mockManager = new Mock<IDataWritingManager>();
			Mock<AsycudaManifestHeaderDataObjectWriterHelper>  mockHelper = new Mock<AsycudaManifestHeaderDataObjectWriterHelper>(header);
			ESH7AsycudaForCustomsDeclarationDataObjectWriter writer = new ESH7AsycudaForCustomsDeclarationDataObjectWriter(mockManager.Object, mockHelper.Object);

			var methodInfo = typeof(ESH7AsycudaForCustomsDeclarationDataObjectWriter).GetMethod("CreateNewAsycudaBillEntryInstructionDataObjectWriter", BindingFlags.NonPublic | BindingFlags.Instance);
			var dataObjectWriter = methodInfo.Invoke(writer, null);

			AssertType<ESH7AsycudaBillEntryInstructionDataObjectWriter<EU.H7.Business.AsycudaBill>>(dataObjectWriter);
		}
	}
}
