using System.Linq;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Customs.JP.MessageDefinitions.Outbound;
using CargoWise.EntityFramework;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Common.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing;

[TestedType(typeof(MessageContentProvider))]
sealed class MessageContentProviderTest : TestCase
{
	public void TestProcedureCode()
	{
		var sendingObject = new MessageSendingObject(EntryHeader);
		sendingObject.MessageType = JPProcedureCodeList.Codes.EDA;

		IMessageContentProvider contentProvider = new MessageContentProvider(EntryHeader.Factory, sendingObject);
		AssertEquals(JPProcedureCodeList.Codes.EDA, contentProvider.ProcedureCode);

		var msxSendingObject = new MSXMessageSendingObject(EntryHeader);
		msxSendingObject.MessageType = JPProcedureCodeList.Codes.Traxon;

		contentProvider = new MessageContentProvider(EntryHeader.Factory, msxSendingObject);
		AssertEquals(JPProcedureCodeList.Codes.MSX, contentProvider.ProcedureCode);
	}

	public void TestGetMessageData()
	{
		var factory = EntryHeader.Factory;
		var mockWriter = new Mock<IJPOutboundMessageWriter>();

		using (NACCSFactoryServiceTestHelper.SetOutboundMessageWriter(factory, mockWriter.Object))
		{
			var parent = new DeclarationMessageSendingObjectParent(EntryHeader.Declaration);

			parent.SendingObjectsCollection[0].ProcedureCode = JPProcedureCodeList.Codes.IDC;
			mockWriter.Setup(m => m.Write<IEntrySubmission>(It.Is<IJPOutboundMessageHeader>(h => h.ProcedureCode == JPProcedureCodeList.Codes.IDC), It.Is<CusEntryHeaderMessageProvider>(p => p.DeclarationNumber == entryHeader.EntryNumber))).Returns(new byte[] { 40 });

			IMessageContentProvider contentProvider = new MessageContentProvider(factory, parent.SendingObjectsCollection.Cast<MessageSendingObject>().FirstOrDefault());
			AssertContainsExactElementsInExactOrder("Should output the message data.", new byte[] { 40 }, contentProvider.GetMessageData());

			mockWriter.Verify();
		}
	}

	CusEntryHeader EntryHeader
	{
		get
		{
			if (entryHeader == null)
			{
				var factory = new BusinessObjectFactory();

				var declaration = factory.New<JobDeclaration>();
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
			}

			return entryHeader;
		}
	}
	CusEntryHeader entryHeader;
}
