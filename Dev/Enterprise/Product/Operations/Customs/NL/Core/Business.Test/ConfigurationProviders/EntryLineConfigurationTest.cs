using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing
{
	[TestedType(typeof(EntryLineConfiguration))]
	public class EntryLineConfigurationTest : EU.Business.Testing.EntryLineConfigurationAbstractTest<EntryLineConfiguration>
	{
		protected override bool ExpectedShouldFilterSupportingDocumentsByMergeKeys => ZBool.False;
	}
}
