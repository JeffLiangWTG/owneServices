using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.EMCS.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	class EMCSInboundBranchMessageProcessorTests : TestCaseWithFactory
	{
		public void TestResolveEMCSMessageProcessors()
		{
			var message = Factory.New<EMCSInboundEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCustomsEMCS;
			var messages = EMCSResponseMessageDetails.Instance.ResponseMessages.Keys;
			CombineAssertions(() =>
			{
				foreach (var messageCode in messages)
				{
					message.EM_MessageType = messageCode;
					var prc = processor.GetApplicationTypeProcessorCore(message);
					Assert($"Should resolve processor for type {messageCode}", prc is BranchCustomsApplicationTypeMessageProcessor);
				}
			});
		}

		public void TestProcessAllBranchesOfCompany()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_BranchName = "Branch1";
			company.Branches.Add(branch1);
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_BranchName = "Branch2";
			company.Branches.Add(branch2);

			var message1 = Factory.New<EMCSInboundEDIMessage>();
			message1.EM_Status = EDIMessageStatusList.Codes.Queued;
			message1.EM_GB = branch1.PK;
			message1.EM_MessageType = EMCSGBIncomingMessageTypeList.Codes.IE704;
			var message2 = Factory.New<EMCSInboundEDIMessage>();
			message2.EM_Status = EDIMessageStatusList.Codes.Queued;
			message2.EM_GB = branch2.PK;
			message2.EM_MessageType = EMCSGBIncomingMessageTypeList.Codes.IE704;
			Factory.Save();

			using (DisposableEnvironment.ForBranch(branch1.PK.ToGuid()))
			{
				processor.ExecuteBatch();
			}

			message1.Reload();
			message2.Reload();

			CombineAssertions(() =>
			{
				AssertNotEquals("message1.EM_Status", EDIMessageStatusList.Codes.Queued, message1.EM_Status);
				AssertNotEquals("message2.EM_Status", EDIMessageStatusList.Codes.Queued, message2.EM_Status);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			processor = new EMCSInboundBranchMessageProcessor(new LoggingInformation());
		}

		EMCSInboundBranchMessageProcessor processor;
	}
}
