using CargoWise.Customs.JP.MessageContracts;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Customs.JP.MessageDefinitions.Outbound;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Common.Testing;
using Moq;

namespace Enterprise.Customs.JP.Business.Testing
{
	sealed class NACCSMessageBuilderTest : TestCaseWithFactory
	{
		public void TestBuildNACCSMessageWithEntryHeader()
		{
			CombineAssertions(() =>
			{
				var mockWriter = new Mock<IJPOutboundMessageWriter>();
				using (NACCSFactoryServiceTestHelper.SetOutboundMessageWriter(Factory, mockWriter.Object))
				{
					var entryHeader = Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
					entryHeader.EntryNumber = "123";
					Factory.Save();

					mockWriter.Setup(m => m.Write<IIDAEntry>(It.Is<IJPOutboundMessageHeader>(h => h.ProcedureCode == JPProcedureCodeList.Codes.IDA), It.Is<CusEntryHeaderMessageProvider>(p => p.DeclarationNumber == entryHeader.EntryNumber))).Returns(new byte[] { 20 });
					AssertContainsExactElementsInExactOrder("IDA", new byte[] { 20 }, NACCSMessageBuilder.BuildNACCSMessage(new MessageSendingObject(entryHeader) { ProcedureCode = JPProcedureCodeList.Codes.IDA }));
					mockWriter.Verify();
					mockWriter.Reset();

					mockWriter.Setup(m => m.Write<IEDAEntry>(It.Is<IJPOutboundMessageHeader>(h => h.ProcedureCode == JPProcedureCodeList.Codes.EDA), It.Is<CusEntryHeaderMessageProvider>(p => p.DeclarationNumber == entryHeader.EntryNumber))).Returns(new byte[] { 30 });
					AssertContainsExactElementsInExactOrder("EDA", new byte[] { 30 }, NACCSMessageBuilder.BuildNACCSMessage(new MessageSendingObject(entryHeader) { ProcedureCode = JPProcedureCodeList.Codes.EDA }));
					mockWriter.Verify();
					mockWriter.Reset();

					mockWriter.Setup(m => m.Write<IEntrySubmission>(It.Is<IJPOutboundMessageHeader>(h => h.ProcedureCode == JPProcedureCodeList.Codes.IDC), It.Is<CusEntryHeaderMessageProvider>(p => p.DeclarationNumber == entryHeader.EntryNumber))).Returns(new byte[] { 40 });
					AssertContainsExactElementsInExactOrder("IDC", new byte[] { 40 }, NACCSMessageBuilder.BuildNACCSMessage(new MessageSendingObject(entryHeader) { ProcedureCode = JPProcedureCodeList.Codes.IDC }));
					mockWriter.Verify();
					mockWriter.Reset();

					mockWriter.Setup(m => m.Write<IEAC>(It.Is<IJPOutboundMessageHeader>(h => h.ProcedureCode == JPProcedureCodeList.Codes.EAC), It.Is<CusEntryHeaderMessageProvider>(p => p.DeclarationNumber == entryHeader.EntryNumber))).Returns(new byte[] { 50 });
					AssertContainsExactElementsInExactOrder("EAC", new byte[] { 50 }, NACCSMessageBuilder.BuildNACCSMessage(new MessageSendingObject(entryHeader) { ProcedureCode = JPProcedureCodeList.Codes.EAC }));
					mockWriter.Verify();
				}
			});
		}
	}
}
