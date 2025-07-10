using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC044CAdditionalReferenceProvider))]
	sealed class CC044CAdditionalReferenceProviderTest : DocumentProviderAbstractTest<CC044CAdditionalReferenceProvider>
	{
		protected override string SubType => "REF";
	}
}
