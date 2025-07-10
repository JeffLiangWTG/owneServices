using System.Linq;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Customs.JP.MessageDefinitions.Outbound;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Common.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(ManifestNACCSMessageBuilder))]
	sealed class ManifestNACCSMessageBuilderTest : TestCaseWithFactory
	{
		public void TestBuildNACCSMessage()
		{
			var mockWriter = new Mock<IJPOutboundMessageWriter>();
			using (NACCSFactoryServiceTestHelper.SetOutboundMessageWriter(Factory, mockWriter.Object))
			{
				var manifestHeader = Factory.New<AsycudaManifestHeader>();
				var bill = manifestHeader.Bills.AddNew();
				var parent = new ManifestMessageSendingObjectParent(manifestHeader);

				parent.SendingObjectsCollection[0].MessageType = JPProcedureCodeList.Codes.HCH01;
				mockWriter.Setup(m => m.Write<IHCH01>(It.Is<IJPOutboundMessageHeader>(h => h.ProcedureCode == JPProcedureCodeList.Codes.HCH01), It.Is<ManifestHeaderMesssageProvider>(p => typeof(ManifestHeaderMesssageProvider) == p.GetType()))).Returns(new byte[] { 20 });
				AssertContainsExactElementsInExactOrder("HCH01", new byte[] { 20 }, ManifestNACCSMessageBuilder.BuildNACCSMessage(parent.SendingObjectsCollection.ToArray().Cast<ManifestMessageSendingObject>()));

				mockWriter.Verify();
				mockWriter.Reset();

				parent.SendingObjectsCollection[0].MessageType = JPProcedureCodeList.Codes.HDF01;
				mockWriter.Setup(m => m.Write<IHDF01>(It.Is<IJPOutboundMessageHeader>(h => h.ProcedureCode == JPProcedureCodeList.Codes.HDF01), It.Is<ManifestHeaderMesssageProvider>(p => typeof(ManifestHeaderMesssageProvider) == p.GetType()))).Returns(new byte[] { 30 });
				AssertContainsExactElementsInExactOrder("HDF01", new byte[] { 30 }, ManifestNACCSMessageBuilder.BuildNACCSMessage(parent.SendingObjectsCollection.ToArray().Cast<ManifestMessageSendingObject>()));
				mockWriter.Verify();
			}
		}
	}
}
