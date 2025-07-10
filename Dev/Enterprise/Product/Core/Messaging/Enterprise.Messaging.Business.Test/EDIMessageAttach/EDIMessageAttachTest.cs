using System.IO;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Messaging.Testing
{
	[TestedType(typeof(EDIMessageAttach))]
	sealed class EDIMessageAttachTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetAttachment_DeclarationInShipment()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingShipment)));
			var docManagerSupport = (IDocManagerSupport)shipment;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(shipment, "SHP");
			((BusinessObject)storageMain)["SM_DB"] = 1;
			Factory.Save();
			docManagerSupport.DocManagerInfo.MasterFactory.Save();

			var declaration = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.US.IJobDeclaration)));
			declaration["JE_JS"] = shipment.PK;
			docManagerSupport = (IDocManagerSupport)declaration;
			storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(declaration, "DEC");
			((BusinessObject)storageMain)["SM_DB"] = 1;
			var eDocs = storageMain.AddFileOrDocument((CargoWise.IO.SubStreamableStream)new MemoryStream(new byte[] { 1, 2, 3 }), "ABC.pdf", "ABC", false);

			var requiredDoc = ((IDocsAndCartageParent)shipment).RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var requiredDocAddInfo = requiredDoc.AddInfos.AddNew();
			requiredDocAddInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;

			var message = (EDIMessage)Factory.New<Enterprise.Integration.Customs.US.DIS.IEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = "SUB";
			message.EM_LinkedObject = requiredDocAddInfo;

			var messageAttachment = message.MessageAttachments.AddNew();
			messageAttachment.EG_StorageDocsGuid = eDocs.UniqueKey;

			Factory.Save();
			docManagerSupport.DocManagerInfo.MasterFactory.Save();

			var factory2 = new BusinessObjectFactory();
			var messageLoaded = factory2.Load<EDIMessage>(message.PK);
			AssertEquals("AfterNewObjectIsLinked should be called from the getter of EM_LinkedObject", requiredDocAddInfo.PK, messageLoaded.EM_LinkedObject.PK);
			var attachment = messageLoaded.MessageAttachments[0].GetAttachment();
			AssertNotNull("attachment should not be null", attachment);
			AssertEquals("get the correct eDocs", eDocs.UniqueKey, attachment.UniqueKey);
		}

		public void TestGetAttachment_CusEntryHeaderInShipment()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "USC";
			company.GC_RN_NKCountryCode = "US";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "USB";
			Factory.Save();

			ZGuid messagePK;
			ZGuid requiredDocAddInfoPK;
			ZGuid eDocsUniqueKey;

			using (DisposableEnvironment.ForBranch("USB"))
			{
				var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingShipment)));
				var declaration = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.US.IJobDeclaration)));
				declaration["JE_JS"] = shipment.PK;
				var entryHeader = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.US.ICusEntryHeader)));
				entryHeader["CH_JE"] = declaration.PK;
				var docManagerSupport = (IDocManagerSupport)entryHeader;
				var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(entryHeader, "CEH");
				var eDocs = storageMain.AddFileOrDocument((CargoWise.IO.SubStreamableStream)new MemoryStream(new byte[] { 1, 2, 3 }), "ABC.pdf", "ABC", false);
				eDocsUniqueKey = eDocs.UniqueKey;

				var requiredDoc = ((IDocsAndCartageParent)shipment).RequiredDocumentsProvider.RequiredDocuments.AddNew();
				var requiredDocAddInfo = requiredDoc.AddInfos.AddNew();
				requiredDocAddInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
				requiredDocAddInfoPK = requiredDocAddInfo.PK;

				var message = (EDIMessage)Factory.New<Enterprise.Integration.Customs.US.DIS.IEDIMessage>();
				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message.EM_MessageType = "SUB";
				message.EM_LinkedObject = requiredDocAddInfo;
				messagePK = message.PK;

				var messageAttachment = message.MessageAttachments.AddNew();
				messageAttachment.EG_StorageDocsGuid = eDocs.UniqueKey;

				Factory.Save();
				docManagerSupport.DocManagerInfo.MasterFactory.Save();
			}

			var factory2 = new BusinessObjectFactory();
			var messageLoaded = factory2.Load<EDIMessage>(messagePK);
			AssertEquals("AfterNewObjectIsLinked should be called from the getter of EM_LinkedObject", requiredDocAddInfoPK, messageLoaded.EM_LinkedObject.PK);
			var attachment = messageLoaded.MessageAttachments[0].GetAttachment();
			AssertNotNull("attachment should not be null", attachment);
			AssertEquals("get the correct eDocs", eDocsUniqueKey, attachment.UniqueKey);
		}

		public void TestGetAttachment_CusEntryHeader()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingShipment)));
			var declaration = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			declaration["JE_JS"] = shipment.PK;
			var entryHeader = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.ICusEntryHeader)));
			entryHeader["CH_JE"] = declaration.PK;
			var docManagerSupportEntryHeader = (IDocManagerSupport)entryHeader;
			var storageMainEntryHeader = docManagerSupportEntryHeader.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(entryHeader, "CEH");
			var eDocOnEntry = storageMainEntryHeader.AddFileOrDocument((CargoWise.IO.SubStreamableStream)new MemoryStream(new byte[] { 1, 2, 3 }), "ABC.pdf", "ABC", false);
			var docManagerSupportDeclaration = (IDocManagerSupport)entryHeader;
			var storageMainDeclaration = docManagerSupportDeclaration.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(entryHeader, "DEC");
			var eDocOnDeclaration = storageMainDeclaration.AddFileOrDocument((CargoWise.IO.SubStreamableStream)new MemoryStream(new byte[] { 1, 2, 3 }), "DEF.pdf", "ABC", false);
			var docManagerSupportShipment = (IDocManagerSupport)shipment;
			var storageMainShipment = docManagerSupportEntryHeader.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(shipment, "SHP");
			var eDocOnShipment = storageMainShipment.AddFileOrDocument((CargoWise.IO.SubStreamableStream)new MemoryStream(new byte[] { 1, 2, 3 }), "GHI.pdf", "GHI", false);

			var message = (EDIMessage)Factory.New<Enterprise.Integration.Customs.CH.IEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = "SUB";
			message.EM_LinkedObject = entryHeader;

			var messageAttachment1 = message.MessageAttachments.AddNew();
			messageAttachment1.EG_StorageDocsGuid = eDocOnEntry.UniqueKey;
			var messageAttachment2 = message.MessageAttachments.AddNew();
			messageAttachment2.EG_StorageDocsGuid = eDocOnDeclaration.UniqueKey;
			var messageAttachment3 = message.MessageAttachments.AddNew();
			messageAttachment3.EG_StorageDocsGuid = eDocOnShipment.UniqueKey;

			Factory.Save();
			docManagerSupportEntryHeader.DocManagerInfo.MasterFactory.Save();
			docManagerSupportShipment.DocManagerInfo.MasterFactory.Save();

			var factory2 = new BusinessObjectFactory();
			var messageLoaded = factory2.Load<EDIMessage>(message.PK);
			AssertEquals("AfterNewObjectIsLinked should be called from the getter of EM_LinkedObject", entryHeader.PK, messageLoaded.EM_LinkedObject.PK);
			var attachment1 = (messageLoaded.MessageAttachments.FindByPK(messageAttachment1.PK) as EDIMessageAttach).GetAttachment();
			AssertNotNull("attachment should not be null", attachment1);
			AssertEquals("get the correct eDoc on Entry", eDocOnEntry.UniqueKey, attachment1.UniqueKey);
			var attachment2 = (messageLoaded.MessageAttachments.FindByPK(messageAttachment2.PK) as EDIMessageAttach).GetAttachment();
			AssertNotNull("attachment should not be null", attachment2);
			AssertEquals("get the correct eDoc on Declaration", eDocOnDeclaration.UniqueKey, attachment2.UniqueKey);
			var attachment3 = (messageLoaded.MessageAttachments.FindByPK(messageAttachment3.PK) as EDIMessageAttach).GetAttachment();
			AssertNotNull("attachment should not be null", attachment3);
			AssertEquals("get the correct eDoc on Shipment", eDocOnShipment.UniqueKey, attachment3.UniqueKey);
		}
	}
}
