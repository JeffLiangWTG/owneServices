using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	[TestedType(typeof(CC044CAdditionalReferenceProvider))]
	class CC044CAdditionalReferenceProviderTest : DocumentProviderAbstractTest<CC044CAdditionalReferenceProvider>
	{
		protected override string SubType => "REF";
	}
}
