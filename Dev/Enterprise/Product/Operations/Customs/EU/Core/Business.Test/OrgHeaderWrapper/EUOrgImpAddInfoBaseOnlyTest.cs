using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(EUOrgImpAddInfo))]
	sealed class EUOrgImpAddInfoBaseOnlyTest : EUOrgImpAddInfoAbstractTest
	{
		public void TestGetWhenInLoggedIntoNonEUCompany()
		{
			//JobDeclarations Loaded when Shipment attached to Consol cannot use Current Company Country
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			{
				AssertNoExceptionThrown(() => EUOrgImpAddInfo.Get(orgHeader, Core.Constants.CountryCodes.Italy));
			}
		}

		public void TestGetWhenGettingForNonEUCountry()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			AssertNoExceptionThrown(() => EUOrgImpAddInfo.Get(orgHeader, Core.Constants.CountryCodes.Brazil));

			var euOrgImpAddInfo = EUOrgImpAddInfo.Get(orgHeader, Core.Constants.CountryCodes.Brazil);

			AssertNull("No instance should be returned", euOrgImpAddInfo);
		}
	}
}
