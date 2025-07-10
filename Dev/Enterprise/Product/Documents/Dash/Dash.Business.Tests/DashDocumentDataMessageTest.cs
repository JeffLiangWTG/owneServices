using Enterprise.DocumentScanning.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Dash.Business.Tests
{
	[TestedType(typeof(DashDocumentDataMessage))]
	public class DashDocumentDataMessageTest : EDIMessageTest
	{
		public void TestDefaultValues()
		{
			var message = Factory.New<DashDocumentDataMessage>();

			AssertEquals("EM_ApplicationCode should be DDP", EDIInterchange.ApplicationCodes.DashDocumentDataProcessing, message.EM_ApplicationCode);
			AssertEquals("EM_ReceiveTransmit should be INT", ReceiveTransmitList.Codes.Internal, message.EM_ReceiveTransmit);
			AssertEquals("EM_Status should be QUE", EDIMessageStatusList.Codes.Queued, message.EM_Status);
		}

		public void TestMessageData()
		{
			var message = Factory.New<DashDocumentDataMessage>();

			AssertEquals(message.EM_MessageData.Length, 0);
			AssertNull(message.MessageData);

			var dataContext = new DashDocumentDataMessageData
			{
				DataProcessingSteps = ["ORM", "UNM", "MNC"],
				CurrentDataProcessingStep = 1
			};

			message.MessageData = dataContext;

			AssertNotNull(message.MessageData);
			AssertEquals(true, message.EM_MessageData.Length > 0);

			var messageData = message.MessageData;

			AssertNotNull(messageData);

			AssertNotNull(messageData.DataProcessingSteps);
			AssertEquals(3, messageData.DataProcessingSteps.Length);
			AssertEquals("ORM", messageData.DataProcessingSteps[0]);
			AssertEquals("UNM", messageData.DataProcessingSteps[1]);
			AssertEquals("MNC", messageData.DataProcessingSteps[2]);

			AssertEquals(1, messageData.CurrentDataProcessingStep);
		}

		protected override void MasterSetUp()
		{
			base.MasterSetUp();

			if (!dbHelper.DatabaseExists(1))
			{
				dbHelper.CreateDatabase(1);
			}
		}

		protected override void FinalTearDown()
		{
			base.FinalTearDown();

			if (dbHelper.DatabaseExists(1))
			{
				var dbName = dbHelper.GetDatabaseName(1);
				dbHelper.DropDatabase(dbName);
			}
		}

		readonly DocManagerDBHelperTestClass dbHelper = new ();
	}
}
