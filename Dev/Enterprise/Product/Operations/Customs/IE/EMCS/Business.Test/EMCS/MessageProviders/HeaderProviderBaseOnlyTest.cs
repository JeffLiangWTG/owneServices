using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	[TestedType(typeof(HeaderProviderBase))]
	sealed class HeaderProviderBaseOnlyTest : HeaderProviderAbstractTest<HeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new HeaderProviderBase(null));
		}

		public void TestAdministrativeReferenceCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty by Default", ZString.Empty, HeaderProvider.AdministrativeReferenceCode);
				emcsDeclaration.EADNumber = "EADNUM1234";
				AssertEquals("Same as entered", "EADNUM1234", HeaderProvider.AdministrativeReferenceCode);
			});
		}

		protected override HeaderProvider GetHeaderProvider() => new HeaderProviderBase(emcsDeclaration);
	}

	public class HeaderProviderBase : HeaderProvider
	{
		public HeaderProviderBase(EMCSJobDeclaration emcsJobDeclaration)
			: base(emcsJobDeclaration)
		{
		}
	}
}
