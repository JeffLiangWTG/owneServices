using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(EntryLineConfiguration))]
sealed class EntryLineConfigurationTest : EntryLineConfigurationAbstractTest<EntryLineConfiguration>
{
	protected override bool ExpectedMergeJI_RN_NKCountryOfExportResult => false;

	public void TestMergeJI_RN_NKCountryOfExportForUCC6Export()
	{
		using (ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(Factory, "IsUCC6Core", true, null))
		{
			var dec = CreateDeclaration();
			dec.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals(true, configuration.MergeJI_RN_NKCountryOfExport(dec));
		}
	}
}
