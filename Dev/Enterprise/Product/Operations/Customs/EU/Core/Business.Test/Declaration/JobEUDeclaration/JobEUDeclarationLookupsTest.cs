using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class JobEUDeclarationLookupsTest : TestCaseWithFactory
	{
		public void TestAgreedPlaceCodeList_Type()
		{
			var declaration = Factory.New<JobDeclaration>();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				CombineAssertions(() =>
				{
					declaration.EUD_AgreedPlaceCode = ZString.Empty;
					AssertType<RefUNLOCOCollection>("Type is RefUNLOCOCollection for empty EUD_AgreedPlaceCode", declaration.AddInfoChildLookups.AgreedPlaceCodeList);

					declaration.EUD_AgreedPlaceCode = "1";
					AssertType<RefUNLOCOCollection>("Type is RefUNLOCOCollection for EUD_AgreedPlaceCode Length = 1", declaration.AddInfoChildLookups.AgreedPlaceCodeList);

					declaration.EUD_AgreedPlaceCode = "XX";
					AssertType<RefCountryCollection>("Type is RefCountryCollection for EUD_AgreedPlaceCode Length = 2", declaration.AddInfoChildLookups.AgreedPlaceCodeList);

					declaration.EUD_AgreedPlaceCode = "123";
					AssertType<RefUNLOCOCollection>("Type is RefUNLOCOCollection for EUD_AgreedPlaceCode Length > 2", declaration.AddInfoChildLookups.AgreedPlaceCodeList);
				});
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				declaration.EUD_AgreedPlaceCode = ZString.Empty;
				AssertType<CodeDescriptionPairList>("Type is CodeDescriptionPairList for empty AgreedPlaceCodeSupport disabled", declaration.AddInfoChildLookups.AgreedPlaceCodeList);
			}
		}
	}
}
