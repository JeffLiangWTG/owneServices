using System;
using CargoWise.Data;
using Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CAStateOfOriginCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new CAStateOfOriginCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			var provider = CreateCodeDescriptionPairListProvider() as DocumentEngine.RuntimeOptions.IDependenceCodeDescriptionPairListProvider;
			var countryCA = Guid.Empty;
			var countryUS = Guid.Empty;
			var countryAU = Guid.Empty;
			using (var reader = Db.Connection.Command("SELECT RN_PK,RN_Code FROM dbo.RefCountry WHERE RN_Code='US' OR RN_Code='CA' OR RN_Code='AU'").ExecuteReader())
			{
				while (reader.Read())
				{
					var countryPk = reader.IsDBNull(0) ? Guid.Empty : reader.GetGuid(0);
					var countryCode = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
					if (countryCode == "CA")
					{
						countryCA = countryPk;
					}
					if (countryCode == "US")
					{
						countryUS = countryPk;
					}
					if (countryCode == "AU")
					{
						countryAU = countryPk;
					}
				}
			}

			CodeDescriptionPairList codeDescriptionPairListCA = new CodeDescriptionPairList();
			codeDescriptionPairListCA.AddPair("AB", "Alberta");
			codeDescriptionPairListCA.AddPair("BC", "British Columbia");
			codeDescriptionPairListCA.AddPair("MB", "Manitoba");
			codeDescriptionPairListCA.AddPair("NB", "New Brunswick");
			codeDescriptionPairListCA.AddPair("NL", "Newfoundland and Labrador");
			codeDescriptionPairListCA.AddPair("NT", "Northwest Territories");
			codeDescriptionPairListCA.AddPair("NS", "Nova Scotia");
			codeDescriptionPairListCA.AddPair("NU", "Nunavut");
			codeDescriptionPairListCA.AddPair("ON", "Ontario");
			codeDescriptionPairListCA.AddPair("PE", "Prince Edward Island");
			codeDescriptionPairListCA.AddPair("QC", "Quebec");
			codeDescriptionPairListCA.AddPair("SK", "Saskatchewan");
			codeDescriptionPairListCA.AddPair("YT", "Yukon Territory");
			AssertListEqual(provider.GetDependenceCodeDescriptionPairList(countryCA.ToString()), codeDescriptionPairListCA);

			CodeDescriptionPairList codeDescriptionPairListUS = new CodeDescriptionPairList();
			codeDescriptionPairListUS.AddPair("AL", "Alabama");
			codeDescriptionPairListUS.AddPair("AK", "Alaska");
			codeDescriptionPairListUS.AddPair("AZ", "Arizona");
			codeDescriptionPairListUS.AddPair("AR", "Arkansas");
			codeDescriptionPairListUS.AddPair("CA", "California");
			codeDescriptionPairListUS.AddPair("CO", "Colorado");
			codeDescriptionPairListUS.AddPair("CT", "Connecticut");
			codeDescriptionPairListUS.AddPair("DE", "Delaware");
			codeDescriptionPairListUS.AddPair("DC", "District Of Columbia");
			codeDescriptionPairListUS.AddPair("FL", "Florida");
			codeDescriptionPairListUS.AddPair("GA", "Georgia");
			codeDescriptionPairListUS.AddPair("HI", "Hawaii");
			codeDescriptionPairListUS.AddPair("ID", "Idaho");
			codeDescriptionPairListUS.AddPair("IL", "Illinois");
			codeDescriptionPairListUS.AddPair("IN", "Indiana");
			codeDescriptionPairListUS.AddPair("IA", "Iowa");
			codeDescriptionPairListUS.AddPair("KS", "Kansas");
			codeDescriptionPairListUS.AddPair("KY", "Kentucky");
			codeDescriptionPairListUS.AddPair("LA", "Louisiana");
			codeDescriptionPairListUS.AddPair("ME", "Maine");
			codeDescriptionPairListUS.AddPair("MD", "Maryland");
			codeDescriptionPairListUS.AddPair("MA", "Massachusetts");
			codeDescriptionPairListUS.AddPair("MI", "Michigan");
			codeDescriptionPairListUS.AddPair("MN", "Minnesota");
			codeDescriptionPairListUS.AddPair("MS", "Mississippi");
			codeDescriptionPairListUS.AddPair("MO", "Missouri");
			codeDescriptionPairListUS.AddPair("MT", "Montana");
			codeDescriptionPairListUS.AddPair("NE", "Nebraska");
			codeDescriptionPairListUS.AddPair("NV", "Nevada");
			codeDescriptionPairListUS.AddPair("NH", "New Hampshire");
			codeDescriptionPairListUS.AddPair("NJ", "New Jersey");
			codeDescriptionPairListUS.AddPair("NM", "New Mexico");
			codeDescriptionPairListUS.AddPair("NY", "New York");
			codeDescriptionPairListUS.AddPair("NC", "North Carolina");
			codeDescriptionPairListUS.AddPair("ND", "North Dakota");
			codeDescriptionPairListUS.AddPair("OH", "Ohio");
			codeDescriptionPairListUS.AddPair("OK", "Oklahoma");
			codeDescriptionPairListUS.AddPair("OR", "Oregon");
			codeDescriptionPairListUS.AddPair("PA", "Pennsylvania");
			codeDescriptionPairListUS.AddPair("RI", "Rhode Island");
			codeDescriptionPairListUS.AddPair("SC", "South Carolina");
			codeDescriptionPairListUS.AddPair("SD", "South Dakota");
			codeDescriptionPairListUS.AddPair("TN", "Tennessee");
			codeDescriptionPairListUS.AddPair("TX", "Texas");
			codeDescriptionPairListUS.AddPair("UT", "Utah");
			codeDescriptionPairListUS.AddPair("VT", "Vermont");
			codeDescriptionPairListUS.AddPair("VA", "Virginia");
			codeDescriptionPairListUS.AddPair("WA", "Washington");
			codeDescriptionPairListUS.AddPair("WV", "West Virginia");
			codeDescriptionPairListUS.AddPair("WI", "Wisconsin");
			codeDescriptionPairListUS.AddPair("WY", "Wyoming");
			AssertListEqual(provider.GetDependenceCodeDescriptionPairList(countryUS.ToString()), codeDescriptionPairListUS);

			CodeDescriptionPairList codeDescriptionPairListAU = new CodeDescriptionPairList();
			AssertListEqual(provider.GetDependenceCodeDescriptionPairList(countryAU.ToString()), codeDescriptionPairListAU);
		}
	}
}
