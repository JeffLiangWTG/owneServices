using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Integration;
using Enterprise.Core;
using Enterprise.MasterFiles.Integration;
using Moq;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class CountrySpecificTypeDeciderTest_ForCoreFunctionality : CountrySpecificTestCase
	{
		public void TestGetTypeForCountryCode_IsInEuropeanCustomsUnionOrInheritsFromEU()
		{
			CombineAssertions(() =>
			{
				using (ObjectFactory.Substitute("Shared.IEuropeanUnionCustomsMembersProvider"
					, MockIEuropeanUnionCustomsMembersProvider(Enterprise.Core.Constants.CountryCodes.UnitedKingdom, true, true).Object))
				{
					var cType = TypeDecider.GetTypeForCountryCode(Enterprise.Core.Constants.CountryCodes.UnitedKingdom);
					AssertEquals("In EU", typeof(long), cType);
				}

				using (ObjectFactory.Substitute("Shared.IEuropeanUnionCustomsMembersProvider"
					, MockIEuropeanUnionCustomsMembersProvider(Enterprise.Core.Constants.CountryCodes.UnitedKingdom, false, true).Object))
				{
					AssertEquals("Inherits from EU", typeof(long), TypeDecider.GetTypeForCountryCode(Enterprise.Core.Constants.CountryCodes.UnitedKingdom));
				}

				using (ObjectFactory.Substitute("Shared.IEuropeanUnionCustomsMembersProvider"
					, MockIEuropeanUnionCustomsMembersProvider(Enterprise.Core.Constants.CountryCodes.UnitedKingdom, false, false).Object))
				{
					AssertEquals("Not in EU and not inherits from EU", typeof(char), TypeDecider.GetTypeForCountryCode(Enterprise.Core.Constants.CountryCodes.UnitedKingdom));
				}
			});
		}

		public void TestGetTypeFromTypeDecider()
		{
			SetCountryCode(Enterprise.Core.Constants.CountryCodes.Australia);
			AssertEquals(typeof(double), TypeDecider.GetTypeForBinding());
			AssertEquals(typeof(double), TypeDecider.GetTypeForNew());
			AssertEquals(typeof(double), TypeDecider.GetTypeForLoad(null, null));
			AssertEquals(typeof(double), TypeDecider.GetTypeForCountryCode(Enterprise.Core.Constants.CountryCodes.Australia));
			AssertEquals(typeof(float), TypeDecider.GetTypeForCountryCode(Enterprise.Core.Constants.CountryCodes.Singapore));
			AssertEquals(typeof(long), TypeDecider.GetTypeForCountryCode(Enterprise.Core.Constants.CountryCodes.Lithuania));

			SetCountryCode(Enterprise.Core.Constants.CountryCodes.Singapore);
			AssertEquals(typeof(float), TypeDecider.GetTypeForBinding());
			AssertEquals(typeof(float), TypeDecider.GetTypeForNew());
			AssertEquals(typeof(float), TypeDecider.GetTypeForLoad(null, null));
			AssertEquals(typeof(double), TypeDecider.GetTypeForCountryCode(Enterprise.Core.Constants.CountryCodes.Australia));
			AssertEquals(typeof(float), TypeDecider.GetTypeForCountryCode(Enterprise.Core.Constants.CountryCodes.Singapore));
			AssertEquals(typeof(long), TypeDecider.GetTypeForCountryCode(Enterprise.Core.Constants.CountryCodes.Lithuania));

			SetCountryCode(Enterprise.Core.Constants.CountryCodes.Senegal);
			AssertEquals(typeof(int), TypeDecider.GetTypeForBinding());
			AssertEquals(typeof(int), TypeDecider.GetTypeForNew());
			AssertEquals(typeof(int), TypeDecider.GetTypeForLoad(null, null));

			SetCountryCode(Enterprise.Core.Constants.CountryCodes.SouthAfrica);
			AssertEquals(typeof(string), TypeDecider.GetTypeForBinding());
			AssertEquals(typeof(string), TypeDecider.GetTypeForNew());
			AssertEquals(typeof(string), TypeDecider.GetTypeForLoad(null, null));

			SetCountryCode(Enterprise.Core.Constants.CountryCodes.SierraLeone);
			AssertEquals(typeof(char), TypeDecider.GetTypeForBinding());
			AssertEquals(typeof(char), TypeDecider.GetTypeForNew());
			AssertEquals(typeof(char), TypeDecider.GetTypeForLoad(null, null));

			SetCountryCode(Enterprise.Core.Constants.CountryCodes.SolomonIslands);
			AssertEquals(typeof(char), TypeDecider.GetTypeForBinding());
			AssertEquals(typeof(char), TypeDecider.GetTypeForNew());
			AssertEquals(typeof(char), TypeDecider.GetTypeForLoad(null, null));

			foreach (var euMember in new string[] { Enterprise.Core.Constants.CountryCodes.Lithuania, Enterprise.Core.Constants.CountryCodes.UnitedKingdom, Enterprise.Core.Constants.CountryCodes.Germany, Enterprise.Core.Constants.CountryCodes.Croatia })
			{
				SetCountryCode(euMember);
				AssertEquals(typeof(long), TypeDecider.GetTypeForBinding());
				AssertEquals(typeof(long), TypeDecider.GetTypeForNew());
				AssertEquals(typeof(long), TypeDecider.GetTypeForLoad(null, null));
				AssertEquals(typeof(double), TypeDecider.GetTypeForCountryCode(Enterprise.Core.Constants.CountryCodes.Australia));
				AssertEquals(typeof(float), TypeDecider.GetTypeForCountryCode(Enterprise.Core.Constants.CountryCodes.Singapore));
				AssertEquals(typeof(long), TypeDecider.GetTypeForCountryCode(euMember));
			}

			foreach (var frDrom in Enterprise.Core.Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
			{
				AssertEquals($"Precondition: {frDrom} should be under FR Customs Jurisdiction", Enterprise.Core.Constants.CountryCodes.France, Enterprise.Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(frDrom));
				SetCountryCode(frDrom);
				AssertEquals(typeof(long), TypeDecider.GetTypeForBinding());
				AssertEquals(typeof(long), TypeDecider.GetTypeForNew());
				AssertEquals(typeof(long), TypeDecider.GetTypeForLoad(null, null));
				AssertEquals(typeof(double), TypeDecider.GetTypeForCountryCode(Enterprise.Core.Constants.CountryCodes.Australia));
				AssertEquals(typeof(float), TypeDecider.GetTypeForCountryCode(Enterprise.Core.Constants.CountryCodes.Singapore));
				AssertEquals(typeof(long), TypeDecider.GetTypeForCountryCode(Enterprise.Core.Constants.CountryCodes.Lithuania));
			}

			SetCountryCode(Enterprise.Core.Constants.CountryCodes.Lithuania);
			AssertEquals(typeof(long), TypeDecider.GetTypeForBinding());
			AssertEquals(typeof(long), TypeDecider.GetTypeForNew());
			AssertEquals(typeof(long), TypeDecider.GetTypeForLoad(null, null));
			AssertEquals(typeof(double), TypeDecider.GetTypeForCountryCode(Enterprise.Core.Constants.CountryCodes.Australia));
			AssertEquals(typeof(float), TypeDecider.GetTypeForCountryCode(Enterprise.Core.Constants.CountryCodes.Singapore));
			AssertEquals(typeof(long), TypeDecider.GetTypeForCountryCode(Enterprise.Core.Constants.CountryCodes.Lithuania));

			AssertEquals("Precondition: Puerto Rico should be under US Customs Jurisdiction", Enterprise.Core.Constants.CountryCodes.UnitedStates, Enterprise.Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Enterprise.Core.Constants.CountryCodes.PuertoRico));
			SetCountryCode(Enterprise.Core.Constants.CountryCodes.UnitedStates);
			AssertEquals(typeof(bool), TypeDecider.GetTypeForBinding());
			AssertEquals(typeof(bool), TypeDecider.GetTypeForNew());
			AssertEquals(typeof(bool), TypeDecider.GetTypeForLoad(null, null));
			AssertEquals(typeof(double), TypeDecider.GetTypeForCountryCode(Enterprise.Core.Constants.CountryCodes.Australia));
			AssertEquals(typeof(float), TypeDecider.GetTypeForCountryCode(Enterprise.Core.Constants.CountryCodes.Singapore));
			AssertEquals(typeof(long), TypeDecider.GetTypeForCountryCode(Enterprise.Core.Constants.CountryCodes.Lithuania));
			AssertEquals(typeof(int), TypeDecider.GetTypeForCountryCode(Enterprise.Core.Constants.CountryCodes.Senegal));
			AssertEquals(typeof(string), TypeDecider.GetTypeForCountryCode(Enterprise.Core.Constants.CountryCodes.SouthAfrica));
			AssertEquals(typeof(bool), TypeDecider.GetTypeForCountryCode(Enterprise.Core.Constants.CountryCodes.UnitedStates));
			AssertEquals(typeof(bool), TypeDecider.GetTypeForCountryCode(Enterprise.Core.Constants.CountryCodes.PuertoRico));

			AssertEquals("Precondition: Liechtenstein should be under CH Customs Jurisdiction", Enterprise.Core.Constants.CountryCodes.Switzerland, Enterprise.Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Enterprise.Core.Constants.CountryCodes.Liechtenstein));
			SetCountryCode(Enterprise.Core.Constants.CountryCodes.Liechtenstein);
			AssertEquals(typeof(char), TypeDecider.GetTypeForBinding());
			AssertEquals(typeof(char), TypeDecider.GetTypeForNew());
			AssertEquals(typeof(char), TypeDecider.GetTypeForLoad(null, null));
			AssertEquals(typeof(double), TypeDecider.GetTypeForCountryCode(Enterprise.Core.Constants.CountryCodes.Australia));
			AssertEquals(typeof(float), TypeDecider.GetTypeForCountryCode(Enterprise.Core.Constants.CountryCodes.Singapore));
			AssertEquals(typeof(long), TypeDecider.GetTypeForCountryCode(Enterprise.Core.Constants.CountryCodes.Lithuania));
		}

		public void TestSimulateDotNetDesigner_NoCurrentCompany()
		{
			TypeDecider = new SimulateCountrySpecificTypeDeciderInDotNetDesigner();
			AssertNotNull(TypeDecider.GetTypeForBinding());
		}

		public void TestGetTypeForNew_ITypeDeciderContext()
		{
			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>(null);
			AssertEquals("Enterprise.Customs.AU.Declaration.Business.JobDeclaration", declaration.GetType().FullName);

			var typeDeciderContextMock = new Mock<ITypeDeciderContext>();
			typeDeciderContextMock.Setup(k => k.Country).Returns("NZ");
			declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>(typeDeciderContextMock.Object);
			AssertEquals("Enterprise.Customs.NZ.Business.Declaration.JobDeclaration", declaration.GetType().FullName);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TypeDecider = new CountrySpecificTypeDeciderForTest();
		}

		CountrySpecificTypeDeciderForTest TypeDecider;

		#region CountrySpecificTypeDeciderForTest

		class CountrySpecificTypeDeciderForTest : CountrySpecificTypeDecider
		{
			protected override Type DefaultTypeForUnsupportedCountry
			{
				get { return typeof(char); }
			}

			protected override Type DefaultTypeForEuCountry
			{
				get { return typeof(long); }
			}

			protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
				{
					new CountrySpecificType(Constants.CountryCodes.Australia, delegate { return typeof(double); }),
					new CountrySpecificType(Constants.CountryCodes.Singapore, delegate { return typeof(float); }),
					new CountrySpecificType(Constants.CountryCodes.Senegal, delegate { return typeof(int); }),
					new CountrySpecificType(Constants.CountryCodes.SouthAfrica, delegate { return typeof(string); }),
					new CountrySpecificType(Constants.CountryCodes.UnitedStates, delegate { return typeof(bool); }),
				};
		}

		class SimulateCountrySpecificTypeDeciderInDotNetDesigner : CountrySpecificTypeDeciderForTest
		{
			protected override IGlbCompany CurrentCompany
			{
				get { return null; }
			}
		}

		#endregion

		static Mock<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider> MockIEuropeanUnionCustomsMembersProvider(string countryCode, bool isInEuropeanCustomsUnion, bool isInEuropeanCustomsUnionOrInheritsFromEu)
		{
			var mock = new Mock<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>();
			mock.Setup(m => m.IsInEuropeanCustomsUnion(It.IsAny<string>()))
				.Returns((string arg) => arg == countryCode && isInEuropeanCustomsUnion);
			mock.Setup(m => m.IsInEuropeanCustomsUnionOrInheritsFromEU(It.IsAny<string>()))
				.Returns((string arg) => arg == countryCode && isInEuropeanCustomsUnionOrInheritsFromEu);
			return mock;
		}
	}
}
