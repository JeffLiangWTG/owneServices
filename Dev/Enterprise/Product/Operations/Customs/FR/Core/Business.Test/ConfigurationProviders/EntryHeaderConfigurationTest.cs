using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Testing
{
	[TestedType(typeof(EntryHeaderConfiguration))]
	class EntryHeaderConfigurationTest : EntryHeaderConfigurationAbstractTest<EntryHeaderConfiguration>
	{
		public void TestGetValidationDecider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				AssertNull(configuration.GetValidationDecider(entryHeader));
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertType<UCC6ImportEntryHeaderValidationDecider>(configuration.GetValidationDecider(entryHeader));
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				AssertNull(configuration.GetValidationDecider(entryHeader));
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertNull(configuration.GetValidationDecider(entryHeader));
			}
		}
	}
}
