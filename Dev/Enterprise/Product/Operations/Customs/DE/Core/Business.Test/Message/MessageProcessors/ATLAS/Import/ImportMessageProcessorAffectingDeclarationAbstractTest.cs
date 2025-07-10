using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	abstract class ImportMessageProcessorAffectingDeclarationAbstractTest<T, TEDIMessage> : MessageProcessorAbstractTest<T, TEDIMessage>
		where T : DEBranchCustomsApplicationTypeMessageProcessor<TEDIMessage>
		where TEDIMessage : EDIMessage
	{
		public void TestSkipUpdateSnapshot()
		{
			CombineAssertions(() =>
			{
				AssertEquals("before message processing", false, declaration.SkipSnapshotUpdate);

				ProcessMessage(Message);
				AssertEquals("after message processing", true, declaration.SkipSnapshotUpdate);
			});
		}

		protected abstract TEDIMessage Message { get; }

		protected JobDeclaration declaration;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		}
	}
}
