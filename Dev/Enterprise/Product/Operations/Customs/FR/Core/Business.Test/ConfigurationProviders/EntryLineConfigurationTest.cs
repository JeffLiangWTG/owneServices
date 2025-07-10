using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Testing
{
	[TestedType(typeof(EntryLineConfiguration))]
	class EntryLineConfigurationTest : EntryLineConfigurationAbstractTest<EntryLineConfiguration>
	{
		public void TestGetValidationDecider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				AssertNull(configuration.GetValidationDecider(entryLine));
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertType<UCC6ImportEntryLineValidationDecider>(configuration.GetValidationDecider(entryLine));
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				AssertNull(configuration.GetValidationDecider(entryLine));
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertNull(configuration.GetValidationDecider(entryLine));
			}
		}
	}
}
