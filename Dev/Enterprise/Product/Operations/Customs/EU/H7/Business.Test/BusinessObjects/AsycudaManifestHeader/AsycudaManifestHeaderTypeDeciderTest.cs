using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	sealed class AsycudaManifestHeaderTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForBinding()
		{
			var typeDecider = new AsycudaManifestHeaderTypeDecider();
			AssertEquals(typeof(AsycudaManifestHeader), typeDecider.GetTypeForBinding());
		}

		public void TestGetTypeForNew()
		{
			AssertGetTypeForNew(Core.Constants.CountryCodes.Denmark, typeof(AsycudaManifestHeader).FullName);

			AssertGetTypeForNew(Core.Constants.CountryCodes.Ireland, "Enterprise.Customs.IE.H7.Business.AsycudaManifestHeader");
			AssertGetTypeForNew(Core.Constants.CountryCodes.UnitedKingdom, "Enterprise.Customs.GB.H7.Business.AsycudaManifestHeader");
			AssertGetTypeForNew(Core.Constants.CountryCodes.Spain, "Enterprise.Customs.ES.Manifest.H7.Business.AsycudaManifestHeader");
			AssertGetTypeForNew(Core.Constants.CountryCodes.France, "Enterprise.Customs.FR.H7.Business.H7ManifestHeader");
			AssertGetTypeForNew(Core.Constants.CountryCodes.Italy, "Enterprise.Customs.IT.H7.Business.AsycudaManifestHeader");

			void AssertGetTypeForNew(string countryCode, string expected)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					var typeDecider = new AsycudaManifestHeaderTypeDecider();
					AssertEquals(expected, typeDecider.GetTypeForNew().FullName);
				}
			}
		}

		public void TestGetTypeForLoad()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "456";
			header.AMA_ApplicationCode = "NVC";

			AssertGetTypeForLoad(Core.Constants.CountryCodes.Denmark, typeof(AsycudaManifestHeader).FullName);

			AssertGetTypeForLoad(Core.Constants.CountryCodes.Ireland, "Enterprise.Customs.IE.H7.Business.AsycudaManifestHeader");
			AssertGetTypeForLoad(Core.Constants.CountryCodes.UnitedKingdom, "Enterprise.Customs.GB.H7.Business.AsycudaManifestHeader");
			AssertGetTypeForLoad(Core.Constants.CountryCodes.Spain, "Enterprise.Customs.ES.Manifest.H7.Business.AsycudaManifestHeader");
			AssertGetTypeForLoad(Core.Constants.CountryCodes.France, "Enterprise.Customs.FR.H7.Business.H7ManifestHeader");
			AssertGetTypeForLoad(Core.Constants.CountryCodes.Italy, "Enterprise.Customs.IT.H7.Business.AsycudaManifestHeader");

			void AssertGetTypeForLoad(string countryCode, string expected)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					var typeDecider = new AsycudaManifestHeaderTypeDecider();
					var row = ((INeedRow)header).Row;
					var typeForLoad = typeDecider.GetTypeForLoad(row, Factory);
					AssertEquals(expected, typeForLoad.FullName);
				}
			}
		}
	}
}
