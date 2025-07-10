using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class ApplicationBusinessProviderBaseTest : TestCaseWithFactory
	{
		public void TestGetAllApplicationBusinessProviders()
		{
			var selector = (ApplicationBusinessProvider p) => (string.Join(",", p.CountryCodes), string.Join(",", p.ManifestTypes), p.ApplicationCode);

			var providers = GetApplicationBusinessProviders();
			var expectedProviders = providers.Select(selector);

			using (ObjectFactory.Substitute("GlobalManifestApplicationBusinessProvider", providers))
			{
				AssertContainsExactElementsInAnyOrder(expectedProviders, ApplicationBusinessProvider.GetAllApplicationBusinessProviders(Factory).Select(selector));
			}
		}

		public void TestGlobalManifestApplicationBusinessProvider_EnableParallelInit()
		{
			AssertEquals(true, ObjectFactory.GetObjectDefinitions().Where(x => x.Name == "GlobalManifestApplicationBusinessProvider").Single().PropertyDefinitions[0].EnableParallelInit);
		}

		public void TestGetApplicationBusinessProvidersDictionary()
		{
			var expectedKeys = new List<(ZString, ZString, ZString)>
			{
				("C1", "T1", "NVC"),
				("C1", "T1", "VOC"),
				("C1", "T2", "NVC"),
				("C1", "T2", "VOC"),
				("C2", "T1", "NVC"),
				("C2", "T1", "VOC"),
				("C3", "T3", "NVC"),
				("C3", "T3", "VOC"),
				("C4", "T4", "A4"),
				("C5", "", "A5"),
			};

			using (ObjectFactory.Substitute("GlobalManifestApplicationBusinessProvider", GetApplicationBusinessProviders()))
			{
				AssertContainsExactElementsInAnyOrder(expectedKeys, ApplicationBusinessProvider.GetApplicationBusinessProvidersDictionary(Factory).Keys);
			}
		}

		public void TestGetApplicationBusinessProvidersDictionaryDuplicates() => CombineAssertions(() =>
		{
			const string expectedErrorKey = "Enterprise.Customs.ASYCUDA.Business.ApplicationBusinessProvider.DuplicateProvider";
			const string expectedReportedMessage = """
												   Ambigous Application Provider ignored. See debug information below:
												   CountryOrGrouping='C3' ManifestTypeCode='T3' ApplicationCode='NVC'
												     Provider1=Enterprise.Customs.ASYCUDA.Business.Testing.DummyApplicationBusinessProvider (AllManifestTypes.Count=1)
												     Provider2=Enterprise.Customs.ASYCUDA.Business.Testing.DummyApplicationBusinessProvider (AllManifestTypes.Count=1)
												   CountryOrGrouping='C3' ManifestTypeCode='T3' ApplicationCode='VOC'
												     Provider1=Enterprise.Customs.ASYCUDA.Business.Testing.DummyApplicationBusinessProvider (AllManifestTypes.Count=1)
												     Provider2=Enterprise.Customs.ASYCUDA.Business.Testing.DummyApplicationBusinessProvider (AllManifestTypes.Count=1)
												   CountryOrGrouping='C5' ManifestTypeCode='' ApplicationCode='A5'
												     Provider1=Enterprise.Customs.ASYCUDA.Business.Testing.DummyApplicationBusinessProvider (AllManifestTypes.Count=0)
												     Provider2=Enterprise.Customs.ASYCUDA.Business.Testing.DummyApplicationBusinessProvider (AllManifestTypes.Count=0)
												   """;

			var globalManifestApplicationBusinessProvider = new ApplicationBusinessProvider[]
			{
				new DummyApplicationBusinessProvider("T1", "C1"),
				new DummyApplicationBusinessProvider("T2", "C1"),
				new DummyApplicationBusinessProvider("T3", "C3"),
				new DummyApplicationBusinessProvider("T3", "C3"),
				new DummyApplicationBusinessProvider("T1", "C2"),
				new DummyApplicationBusinessProvider(null, "C4").WithApplicationCode("A4"),
				new DummyApplicationBusinessProvider(null, "C5").WithApplicationCode("A5"),
				new DummyApplicationBusinessProvider(null, "C5").WithApplicationCode("A5"),
				new DummyApplicationBusinessProvider(null, "C6").WithApplicationCode("A6"),
			};

			using (ObjectFactory.Substitute("GlobalManifestApplicationBusinessProvider", globalManifestApplicationBusinessProvider))
			{
				AssertEquals("# of items added", 11, ApplicationBusinessProvider.GetApplicationBusinessProvidersDictionary(Factory).Count);
			}
			AssertEquals("ErrorReporter.LastKeyReported", expectedErrorKey, ErrorReporter.LastKeyReported);
			AssertEquals("ErrorReporter.LastMessageReported", expectedReportedMessage.Trim(), ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		});

		public void TestGetManifestCountriesWithActiveManifestTypes()
		{
			using (ObjectFactory.Substitute("GlobalManifestApplicationBusinessProvider", GetApplicationBusinessProviders()))
			{
				AssertContainsExactElementsInAnyOrder(["C1", "C2", "C3"], ApplicationBusinessProvider.GetManifestCountriesWithActiveManifestTypes(Factory, ApplicationCodeTypeList.Codes.Consolidator));
				AssertContainsExactElementsInAnyOrder(["C1", "C2", "C3", "C4"], ApplicationBusinessProvider.GetManifestCountriesWithActiveManifestTypes(Factory));
			}
		}

		public void TestGetManifestApplicationBusinessProvidersForActiveManifestTypes()
		{
			var providers = GetApplicationBusinessProviders();
			using (ObjectFactory.Substitute("GlobalManifestApplicationBusinessProvider", providers))
			{
				AssertContainsExactElementsInAnyOrder([providers[0], providers[1], providers[2], providers[3]], ApplicationBusinessProvider.GetManifestApplicationBusinessProvidersForActiveManifestTypes(Factory, ApplicationCodeTypeList.Codes.Consolidator));
				AssertContainsExactElementsInAnyOrder([providers[0], providers[1], providers[2], providers[3], providers[4]], ApplicationBusinessProvider.GetManifestApplicationBusinessProvidersForActiveManifestTypes(Factory));
			}
		}

		public void TestGetManifestTypeListByCountry()
		{
			using (ObjectFactory.Substitute("GlobalManifestApplicationBusinessProvider", GetApplicationBusinessProviders()))
			{
				AssertCodeDescriptionPairList(ApplicationBusinessProvider.GetManifestTypeListByCountry(Factory, "C1"), ("T1", "description"), ("T2", "description"));
				AssertCodeDescriptionPairList(ApplicationBusinessProvider.GetManifestTypeListByCountry(Factory, "C2"), ("T1", "description"));
				AssertCodeDescriptionPairList(ApplicationBusinessProvider.GetManifestTypeListByCountry(Factory, "C3"), ("T3", "description"));
				AssertCodeDescriptionPairList(ApplicationBusinessProvider.GetManifestTypeListByCountry(Factory, "C4"), ("T4", "description"));
				AssertCodeDescriptionPairList(ApplicationBusinessProvider.GetManifestTypeListByCountry(Factory, "C5"));
			}
		}

		public void TestGetApplicationBusinessProviders()
		{
			var providers = GetApplicationBusinessProviders();
			using (ObjectFactory.Substitute("GlobalManifestApplicationBusinessProvider", providers))
			{
				AssertContainsExactElementsInAnyOrder([providers[0], providers[1]], ApplicationBusinessProvider.GetApplicationBusinessProviders(Factory, "C1"));
				AssertContainsExactElementsInAnyOrder([providers[3]], ApplicationBusinessProvider.GetApplicationBusinessProviders(Factory, "C2"));
				AssertContainsExactElementsInAnyOrder([providers[2]], ApplicationBusinessProvider.GetApplicationBusinessProviders(Factory, "C3"));
				AssertContainsExactElementsInAnyOrder([providers[4]], ApplicationBusinessProvider.GetApplicationBusinessProviders(Factory, "C4"));
				AssertContainsExactElementsInAnyOrder([providers[5]], ApplicationBusinessProvider.GetApplicationBusinessProviders(Factory, "C5"));
			}
		}

		public void TestGetApplicationBusinessProvider()
		{
			var providers = GetApplicationBusinessProviders();
			using (ObjectFactory.Substitute("GlobalManifestApplicationBusinessProvider", providers))
			{
				AssertSubscriberMapping("C1", "T1", "NVC", "C1-T1", providers[0].GetType().FullName);
				AssertSubscriberMapping("C1", "T1", "VOC", "C1-T1", providers[0].GetType().FullName);
			}
		}

		void AssertSubscriberMapping(string country, string manifestType, string applicationCode, string applicationName, string fullTypeName)
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, country, manifestType, applicationCode);
			var provider = ApplicationBusinessProvider.GetApplicationBusinessProvider(header);
			var provider2 = ApplicationBusinessProvider.GetApplicationBusinessProvider(Factory, header.GetApplicationProviderKey());
			AssertNotNull($"{applicationName} provider must exist", provider);
			AssertNotNull($"{applicationName} provider must exist", provider2);
			Assert($"{applicationName} provider must be of type {fullTypeName}", provider.GetType().FullName.Equals(fullTypeName));
		}

		ApplicationBusinessProvider[] GetApplicationBusinessProviders() =>
		[
			new DummyApplicationBusinessProvider("T1", "C1"),
			new DummyApplicationBusinessProvider("T2", "C1"),
			new DummyApplicationBusinessProvider("T3", "C3"),
			new DummyApplicationBusinessProvider("T1", "C2"),
			new DummyApplicationBusinessProvider("T4", "C4").WithApplicationCode("A4"),
			new DummyApplicationBusinessProvider(null, "C5").WithApplicationCode("A5")
		];
	}
}
