using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.Customs.GB.H7.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
#if !WINZOR
using Enterprise.ZArchitecture.Schema;
#endif

namespace Enterprise.Customs.GB.H7.GUI.Testing.DocumentSending
{
#if !WINZOR
	public class CDSH7SupportingDocSendingManagerTest : TestCaseWithFactory
	{
		public void TestSendMessages()
		{
			using (Factory.AddDisposableService())
			{
				var header = Factory.New<AsycudaManifestHeader>();
				var branch = header.Branch.Company.Branches.AddNew();
				header.AMA_GB = branch.PK;
				var bill1 = header.Bills.AddNew();
				var bill2 = header.Bills.AddNew();
				var bill3 = header.Bills.AddNew();

				Factory.Save();

				var eDoc1 = bill1.DocManagerInfo().AddFileOrDocument(new byte[1], "AddInfo1.pdf", "CIV");
				var eDoc2 = bill1.DocManagerInfo().AddFileOrDocument(new byte[1], "AddInfo2.pdf", "CIV");
				bill1.DocManagerInfo().Save();

				var sendingObjectParent = new UploadDocumentsSendingActionParent(header);

				var sendingObject1 = sendingObjectParent.SendingObjectsCollection
					.Cast<UploadDocumentsSendingAction>().Single(x => x.Bill == bill1);
				var sendingDoc1 = sendingObject1.EDocsCollection.AddNew();
				sendingDoc1.EDoc = eDoc1.UniqueKey;
				var sendingDoc2 = sendingObject1.EDocsCollection.AddNew();
				sendingDoc2.EDoc = eDoc2.UniqueKey;
				sendingObject1.ShouldSend = true;

				Factory.Save();

				var zQuery = new ZDBOnlyQuery(typeof(StmNote));
				zQuery.AddToFilter(StmNoteSchema.ST_Description, "Message Interpretation");
				zQuery.AddToFilter(StmNoteSchema.ST_Table, EDIMessageSchema.Constants.TableName);

				CombineAssertions("Successful Send", () =>
				{
					AssertEquals(0, Factory.Load<StmNote>(zQuery).Length);
					var notification = new Customs.GUI.DocumentSending.MessageNotificationCollector();
					var sendingManager = new CDSH7SupportingDocSendingManager(sendingObjectParent, notification);

					sendingManager.SendMessages();
					UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);
					AssertEquals("Summary notification should be shown", "2 documents(s) queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Message interpretations are saved", 2, Factory.Load<StmNote>(zQuery).Length);

					UnitTestUserNotification.Instance.ClearMessages();
					sendingManager.SendMessages();
					AssertEquals("Summary notification should be shown", "2 documents(s) queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}
	}
#endif
}
