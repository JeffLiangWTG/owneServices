namespace Enterprise.Customs.ES.Business.Declaration.Testing;

sealed class JobDeclarationAutoSendingMessageSupporterTest : Customs.Business.Testing.JobDeclarationMessageSupporterTest<JobDeclaration>
{
	public override void TestIJobDeclarationMessageSupporterMembers()
	{
		var declaration = Factory.New<JobDeclaration>();
		var supporter = (Integration.Customs.IJobDeclarationAutoSendingMessageSupporter)declaration;
		CombineAssertions(() =>
		{
			AssertEquals("SupportEntryDeclarationMessage should be true for ES declarations.", true, supporter.SupportEntryDeclarationMessage);
			AssertEquals("SupportReleaseMessage should be false for ES declarations.", false, supporter.SupportReleaseMessage);
			var processor = supporter.CreateEntryDeclarationMessageProcessor();
			AssertNotNull("CreateEntryDeclarationMessageProcessor should be implemented in ES.", processor);
			AssertType<AutoSendCustomsMessageProcessor>("The processor should be of type ES.Business.Declaration.AutoSendCustomsMessageProcessor.", processor);
		});
	}
}
