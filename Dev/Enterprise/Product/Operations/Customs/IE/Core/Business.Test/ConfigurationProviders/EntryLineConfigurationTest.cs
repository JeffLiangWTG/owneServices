using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(EntryLineConfiguration))]
	sealed class EntryLineConfigurationTest : EU.Business.Testing.EntryLineConfigurationAbstractTest<EntryLineConfiguration>
	{
		public void TestMergeJI_RN_NKCountryOfExportForImport()
		{
			var dec = CreateDeclaration();
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals(false, configuration.MergeJI_RN_NKCountryOfExport(dec));
		}

		protected override bool ExpectedMergeJI_RN_NKCountryOfExportResult => true;

		protected override JobDeclaration CreateDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			return declaration;
		}
	}
}
