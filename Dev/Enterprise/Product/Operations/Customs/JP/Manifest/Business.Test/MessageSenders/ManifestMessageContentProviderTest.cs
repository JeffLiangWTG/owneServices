using System.Linq;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Customs.JP.MessageDefinitions.Outbound;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Common.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.Business.Testing;

[TestedType(typeof(ManifestMessageContentProvider))]
sealed class ManifestMessageContentProviderTest : TestCaseWithFactory
{
	public void TestProcedureCode()
	{
		var sendingObject1 = new ManifestMessageSendingObject(Bill);
		var sendingObject2 = new ManifestMessageSendingObject(Bill);

		sendingObject1.MessageType = "HCH01";
		sendingObject2.MessageType = "HDF01";

		IMessageContentProvider contentProvider = new ManifestMessageContentProvider(Factory, [sendingObject1, sendingObject2]);
		AssertEquals(contentProvider.ProcedureCode, "HCH01");
	}

	public void TestGetMessageData()
	{
		var mockWriter = new Mock<IJPOutboundMessageWriter>();

		using (NACCSFactoryServiceTestHelper.SetOutboundMessageWriter(Factory, mockWriter.Object))
		{
			var parent = new ManifestMessageSendingObjectParent(Bill.Header);

			parent.SendingObjectsCollection[0].MessageType = JPProcedureCodeList.Codes.HCH01;
			mockWriter.Setup(m => m.Write<IHCH01>(It.Is<IJPOutboundMessageHeader>(h => h.ProcedureCode == JPProcedureCodeList.Codes.HCH01), It.Is<ManifestHeaderMesssageProvider>(p => typeof(ManifestHeaderMesssageProvider) == p.GetType()))).Returns(new byte[] { 20 });

			IMessageContentProvider contentProvider = new ManifestMessageContentProvider(Factory, parent.SendingObjectsCollection.Cast<ManifestMessageSendingObject>());
			AssertContainsExactElementsInExactOrder("Should output the message data.", new byte[] { 20 }, contentProvider.GetMessageData());

			mockWriter.Verify();
		}
	}

	AsycudaBill Bill
	{
		get
		{
			if (bill == null)
			{
				var header = Factory.New<AsycudaManifestHeader>();
				bill = header.Bills.AddNew();
			}

			return bill;
		}
	}
	AsycudaBill bill;
}
