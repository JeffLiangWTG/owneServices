using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Testing
{
	[TestedType(typeof(EntryLineConfiguration))]
	public class EntryLineConfigurationTest : EU.Business.Testing.EntryLineConfigurationAbstractTest<EntryLineConfiguration>
	{
		protected override bool ExpectedSupportingDocumentsSupportResult => true;

		protected override bool ExpectedMergeJI_RN_NKCountryOfExportResult => true;
	}
}
