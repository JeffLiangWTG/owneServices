using System;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(HeaderProvider))]
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

		public void TestIsValidationAttributeAllowed()
		{
			AssertEquals("No FUNCS added so should return false", false, HeaderProvider.IsValidationAttributeAllowed);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.EMCSGB_ValidationAttributeAllowed, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, true))
			{
				AssertEquals("FUNCS added with FUNCS set to true, should return true", true, HeaderProvider.IsValidationAttributeAllowed);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.EMCSGB_ValidationAttributeAllowed, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, false))
			{
				AssertEquals("FUNCS added with FUNCS set to false, should return false", false, HeaderProvider.IsValidationAttributeAllowed);
			}
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
