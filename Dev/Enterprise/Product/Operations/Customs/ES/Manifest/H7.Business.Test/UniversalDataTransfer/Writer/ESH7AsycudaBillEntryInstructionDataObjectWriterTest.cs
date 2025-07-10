using System.Reflection;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Manifest.H7.Business.UniversalDataTransfer;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Moq;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	sealed class ESH7AsycudaBillEntryInstructionDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestCreateEntryInstruction()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			Mock<IDataWritingManager> mockManager = new Mock<IDataWritingManager>();
			Mock<AsycudaManifestHeaderDataObjectWriterHelper> mockHelper = new Mock<AsycudaManifestHeaderDataObjectWriterHelper>(header);
			ESH7AsycudaBillEntryInstructionDataObjectWriter<ASYCUDA.Business.AsycudaBill> writer = new ESH7AsycudaBillEntryInstructionDataObjectWriter<ASYCUDA.Business.AsycudaBill>(mockManager.Object, mockHelper.Object);

			var methodInfo = typeof(ESH7AsycudaBillEntryInstructionDataObjectWriter<ASYCUDA.Business.AsycudaBill>).GetMethod("CreateEntryInstruction", BindingFlags.NonPublic | BindingFlags.Instance);
			var entryInstruction = (EntryInstruction)methodInfo.Invoke(writer, new object[] { bill });

			CombineAssertions("EntryInstruction", () =>
			{
				AssertEquals("Style should be IM", IMPDeclarationTypeList.Codes.IM, entryInstruction.Style);
				AssertEquals("Substyle should be A", EntrySubStyleList.Codes.A, entryInstruction.SubStyle.Code);
				AssertEquals("Code description matches the code", EntrySubStyleList.Descriptions.A, entryInstruction.SubStyle.Description);
			});
		}
	}
}
