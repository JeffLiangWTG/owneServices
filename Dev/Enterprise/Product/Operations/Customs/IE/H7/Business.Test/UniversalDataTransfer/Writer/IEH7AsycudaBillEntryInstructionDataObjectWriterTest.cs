using System.Reflection;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.H7.Business.UniversalDataTransfer;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Moq;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	sealed class IEH7AsycudaBillEntryInstructionDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestCreateEntryInstruction()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			Mock<IDataWritingManager> mockManager = new Mock<IDataWritingManager>();
			Mock<AsycudaManifestHeaderDataObjectWriterHelper> mockHelper = new Mock<AsycudaManifestHeaderDataObjectWriterHelper>(header);
			IEH7AsycudaBillEntryInstructionDataObjectWriter<ASYCUDA.Business.AsycudaBill> writer = new IEH7AsycudaBillEntryInstructionDataObjectWriter<ASYCUDA.Business.AsycudaBill>(mockManager.Object, mockHelper.Object);

			var methodInfo = typeof(IEH7AsycudaBillEntryInstructionDataObjectWriter<ASYCUDA.Business.AsycudaBill>).GetMethod("CreateEntryInstruction", BindingFlags.NonPublic | BindingFlags.Instance);
			var entryInstruction = (EntryInstruction)methodInfo.Invoke(writer, new object[] { bill });

			CombineAssertions("EntryInstruction", () =>
			{
				AssertEquals("Style should be H1", ImportDeclarationTypeList.Codes.H1, entryInstruction.Style);
				AssertEquals("Substyle should be ABL_ShipmentType", bill.ABL_ShipmentType, entryInstruction.SubStyle.Code);
			});
		}
	}
}
