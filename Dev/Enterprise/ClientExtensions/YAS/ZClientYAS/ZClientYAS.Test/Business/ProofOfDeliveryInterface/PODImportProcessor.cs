using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.YAS.Testing;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.YAS.Business.ProofOfDeliveryInterface.Testing
{
	public class PODImportProcessorTest : TestCaseWithFactory
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExecute()
		{
			testHelper.SetValidRegistryProofOfDeliveryInterface();
			var sampleFile = new FileInfo(Path.Combine(YASDataRegistry.Instance.PODImportFolder, "crap.000"));
			if (!sampleFile.Exists)
			{
				File.Copy(YASTestHelper.TestFiles.ProofOfDelivery.CorrectSample, sampleFile.FullName);
			}
			sampleFile.Attributes = FileAttributes.Normal;

			var processor = new PODImportProcessor();

			processor.ExecuteForTest(testHelper.Notifications);
			Assert("Import started", testHelper.Notifications.AsString.Contains("Starting import processing of file"));
			Assert("Notifications (" + testHelper.Notifications.AsString + ") should indicate the import finished.", testHelper.Notifications.AsString.Contains("Finished import"));
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExecuteWithError()
		{
			var logger = new TestServiceLogger();
			var notifications = logger.GetTaskNotificationSubscriber();

			var shipment = Factory.NewWithValidTestData<YASForwardingShipment>();
			shipment.JS_HouseBill = "YATA5685851";

			Factory.Save();

			testHelper.SetValidRegistryProofOfDeliveryInterface();
			var sampleFile = new FileInfo(Path.Combine(YASDataRegistry.Instance.PODImportFolder, "crap.000"));
			if (!sampleFile.Exists)
			{
				File.Copy(YASTestHelper.TestFiles.ProofOfDelivery.CorrectSample, sampleFile.FullName);
			}
			sampleFile.Attributes = FileAttributes.Normal;

			var processor = new PODImportProcessor();

			processor.ExecuteForTest(notifications);

			AssertEquals("1 email created because of error", 1, Environment.Env.OutgoingMailManager.EmailsCreated.Count);
			AssertContains("There was a problem importing", Environment.Env.OutgoingMailManager.EmailsCreated[0].Body);
			AssertContains("The confirmation is not created as the shipment (YATA5685851) has no packing line.", Environment.Env.OutgoingMailManager.EmailsCreated[0].Body);
		}

		protected override void SetUp()
		{
			base.SetUp();
			testHelper = new YASTestHelper(Factory);
		}

		protected override void TearDown()
		{
			testHelper.TidyUp();
			base.TearDown();
		}
		YASTestHelper testHelper;
	}
}
