using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	internal class LicenceModulesLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLicenceTypes()
		{
			licenceTypesTestedFor = new List<string>();

			LicenceHeader header = Factory.NewWithValidTestData<LicenceHeader>();
			header.Database.LD_Product = ProductTypes.Codes.Enterprise;
			AssertEquals("Sanity check", LicenceAdvStdOthList.Codes.Advanced, header.LA_LicenceAdvStdOth);
			AssertLicenceTypesList(header.Modules[0], LicenceTypes.Codes.CPT, LicenceTypes.Codes.NON, LicenceTypes.Codes.PUR, LicenceTypes.Codes.REN, LicenceTypes.Codes.TRI, LicenceTypes.Codes.SRU);

			header.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.Standard;
			AssertLicenceTypesList(header.Modules[0], LicenceTypes.Codes.CPT, LicenceTypes.Codes.NON, LicenceTypes.Codes.PUR, LicenceTypes.Codes.REN, LicenceTypes.Codes.TRI, LicenceTypes.Codes.SRU);

			header.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.Global;
			AssertLicenceTypesList(header.Modules[0], LicenceTypes.Codes.CPT, LicenceTypes.Codes.NON, LicenceTypes.Codes.PUR, LicenceTypes.Codes.REN, LicenceTypes.Codes.TRI);

			header.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.OnDemand;
			AssertLicenceTypesList(header.Modules[0], LicenceTypes.Codes.CPT, LicenceTypes.Codes.NON, LicenceTypes.Codes.ODM);

			header.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.OtherLegacyApplication;
			AssertLicenceTypesList(header.Modules[0], LicenceTypes.Codes.CPT, LicenceTypes.Codes.NON, LicenceTypes.Codes.PUR, LicenceTypes.Codes.REN, LicenceTypes.Codes.TRI);

			header.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.ConversionToODPL;
			AssertLicenceTypesList(header.Modules[0], LicenceTypes.Codes.CPT, LicenceTypes.Codes.NON, LicenceTypes.Codes.ODM);

			header.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.Hybrid;
			AssertLicenceTypesList(header.Modules[0], LicenceTypes.Codes.CPT, LicenceTypes.Codes.NON, LicenceTypes.Codes.ODM, LicenceTypes.Codes.OPN, LicenceTypes.Codes.OTM, LicenceTypes.Codes.SRU);

			header.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.ConcurrentExpress;
			AssertLicenceTypesList(header.Modules[0], LicenceTypes.Codes.CPT, LicenceTypes.Codes.NON, LicenceTypes.Codes.PUR, LicenceTypes.Codes.REN, LicenceTypes.Codes.TRI, LicenceTypes.Codes.OPN, LicenceTypes.Codes.SRU);

			header.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.ConcurrentCountry;
			AssertLicenceTypesList(header.Modules[0], LicenceTypes.Codes.CPT, LicenceTypes.Codes.NON, LicenceTypes.Codes.PUR, LicenceTypes.Codes.REN, LicenceTypes.Codes.TRI, LicenceTypes.Codes.OPN, LicenceTypes.Codes.SRU);

			header.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.ConcurrentRegional;
			AssertLicenceTypesList(header.Modules[0], LicenceTypes.Codes.CPT, LicenceTypes.Codes.NON, LicenceTypes.Codes.PUR, LicenceTypes.Codes.REN, LicenceTypes.Codes.TRI, LicenceTypes.Codes.OPN, LicenceTypes.Codes.SRU);

			header.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.ConcurrentUniversal;
			AssertLicenceTypesList(header.Modules[0], LicenceTypes.Codes.CPT, LicenceTypes.Codes.NON, LicenceTypes.Codes.PUR, LicenceTypes.Codes.REN, LicenceTypes.Codes.TRI, LicenceTypes.Codes.OPN, LicenceTypes.Codes.SRU);

			header.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			AssertLicenceTypesList(header.Modules[0], LicenceTypes.Codes.CPT, LicenceTypes.Codes.NON, LicenceTypes.Codes.ODM);

			foreach (CodeDescriptionPair licenceType in new LicenceTypes())
			{
				Assert("New Licence Type Code " + licenceType.Code + " has not been catered for in LicenceModules.Lookups.LicenceTypesList.", licenceTypesTestedFor.Contains(licenceType.Code));
			}
		}

		public void TestPurchasableLanguageLicenceTypes()
		{
			licenceTypesTestedFor = new List<string>();

			LicenceHeader header = Factory.NewWithValidTestData<LicenceHeader>();
			header.Database.LD_Product = ProductTypes.Codes.Enterprise;

			var licence = Env.Licence;

			foreach (var licenceEdition in new string[] {
							LicenceAdvStdOthList.Codes.Global,
							LicenceAdvStdOthList.Codes.OtherLegacyApplication,
							LicenceAdvStdOthList.Codes.Advanced,
							LicenceAdvStdOthList.Codes.Standard,
							LicenceAdvStdOthList.Codes.ConcurrentExpress,
							LicenceAdvStdOthList.Codes.ConcurrentRegional,
							LicenceAdvStdOthList.Codes.ConcurrentCountry,
							LicenceAdvStdOthList.Codes.ConcurrentUniversal })
			{
				header.LA_LicenceAdvStdOth = licenceEdition;

				foreach (LicenceCheckpoint languageCheckpoint in licence.LanguagePackLookup.Values)
				{
					var module = header.Modules.FindByCode(languageCheckpoint.Name);
					AssertLicenceTypesList(module, LicenceTypes.Codes.NON, LicenceTypes.Codes.ODM, LicenceTypes.Codes.PUR);
				}
			}

			foreach (var licenceEdition in new string[] {
							LicenceAdvStdOthList.Codes.OnDemand,
							LicenceAdvStdOthList.Codes.ConversionToODPL,
							LicenceAdvStdOthList.Codes.Hybrid })
			{
				header.LA_LicenceAdvStdOth = licenceEdition;

				foreach (LicenceCheckpoint languageCheckpoint in licence.LanguagePackLookup.Values)
				{
					var module = header.Modules.FindByCode(languageCheckpoint.Name);
					AssertLicenceTypesList(module, LicenceTypes.Codes.NON, LicenceTypes.Codes.ODM);
				}
			}
		}

		List<string> licenceTypesTestedFor;
		void AssertLicenceTypesList(LicenceModules module, params string[] expectedLicenceTypes)
		{
			LicenceTypes allLicenceTypes = new LicenceTypes();
			AssertEquals(expectedLicenceTypes.Length, module.Lookups.LicenceTypesList.Count);
			foreach (string licenceTypeCode in expectedLicenceTypes)
			{
				Assert(module.Lookups.LicenceTypesList.ContainsCode(licenceTypeCode));
				AssertEquals(allLicenceTypes.GetDescriptionFromCode(licenceTypeCode), module.Lookups.LicenceTypesList.GetDescriptionFromCode(licenceTypeCode));

				if (!licenceTypesTestedFor.Contains(licenceTypeCode))
				{
					licenceTypesTestedFor.Add(licenceTypeCode);
				}
			}
		}
	}
}