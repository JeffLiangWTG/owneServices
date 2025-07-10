namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	sealed class JobDeclarationAutoSendingMessageSupporterTest : Customs.Business.Testing.JobDeclarationMessageSupporterTest<JobDeclaration>
	{
		public override void TestIJobDeclarationMessageSupporterMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				var supporter = (Integration.Customs.IJobDeclarationAutoSendingMessageSupporter)declaration;
				AssertEquals(true, supporter.SupportEntryDeclarationMessage);
				AssertEquals(false, supporter.SupportReleaseMessage);
				AssertNotNull(supporter.CreateEntryDeclarationMessageProcessor());
				AssertType<DeltaIEAutoSendCustomsMessageProcessor>(supporter.CreateEntryDeclarationMessageProcessor());
			}
			using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC5Configuration(declaration, true))
			{
				var supporter = (Integration.Customs.IJobDeclarationAutoSendingMessageSupporter)declaration;
				AssertEquals(true, supporter.SupportEntryDeclarationMessage);
				AssertEquals(false, supporter.SupportReleaseMessage);
				AssertNotNull(supporter.CreateEntryDeclarationMessageProcessor());
				AssertType<DeltaGAutoSendCustomsMessageProcessor>(supporter.CreateEntryDeclarationMessageProcessor());
			}
		}
	}
}
