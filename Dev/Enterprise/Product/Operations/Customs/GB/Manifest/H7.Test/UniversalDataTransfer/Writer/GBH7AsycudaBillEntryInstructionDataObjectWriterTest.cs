using System.Reflection;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.H7.Business.UniversalDataTransfer;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Moq;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	sealed class GBH7AsycudaBillEntryInstructionDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestCreateEntryInstruction()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			Mock<IDataWritingManager> mockManager = new Mock<IDataWritingManager>();
			Mock<AsycudaManifestHeaderDataObjectWriterHelper> mockHelper = new Mock<AsycudaManifestHeaderDataObjectWriterHelper>(header);
			GBH7AsycudaBillEntryInstructionDataObjectWriter<ASYCUDA.Business.AsycudaBill> writer = new GBH7AsycudaBillEntryInstructionDataObjectWriter<ASYCUDA.Business.AsycudaBill>(mockManager.Object, mockHelper.Object);

			var methodInfo = typeof(GBH7AsycudaBillEntryInstructionDataObjectWriter<ASYCUDA.Business.AsycudaBill>).GetMethod("CreateEntryInstruction", BindingFlags.NonPublic | BindingFlags.Instance);
			var entryInstruction = (EntryInstruction)methodInfo.Invoke(writer, new object[] { bill });

			AssertEquals(AdditionalCustomsReferenceTypeList.Codes.H1, entryInstruction.Style);
		}
	}
}
