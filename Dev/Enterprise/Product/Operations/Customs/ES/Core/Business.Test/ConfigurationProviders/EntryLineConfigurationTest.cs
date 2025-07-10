using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(EntryLineConfiguration))]
	public class EntryLineConfigurationTest : EU.Business.Testing.EntryLineConfigurationAbstractTest<EntryLineConfiguration>
	{
		protected override bool ExpectedSupportingDocumentsSupportResult => true;
	}
}
