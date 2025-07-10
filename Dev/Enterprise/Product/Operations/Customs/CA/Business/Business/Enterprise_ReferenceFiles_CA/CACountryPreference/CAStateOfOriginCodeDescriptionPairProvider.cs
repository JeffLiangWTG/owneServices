using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CAStateOfOriginCodeDescriptionPairProvider : CodeDescriptionPairList, Integration.Customs.CA.ICAStateOfOriginCodeDescriptionPairProvider, DocumentEngine.RuntimeOptions.IDependenceCodeDescriptionPairListProvider
	{
		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;

		ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList(ZGuid countryPK)
		{
			var country = Factory.Load<RefCountry>(countryPK);
			if (country == null)
			{
				return StateOfOriginCodeList();
			}
			CodeDescriptionPairList codeDescriptionPairList = new CodeDescriptionPairList();
			switch (country.RN_Code)
			{
				case "CA":
					codeDescriptionPairList.AddPair("AB", "Alberta");
					codeDescriptionPairList.AddPair("BC", "British Columbia");
					codeDescriptionPairList.AddPair("MB", "Manitoba");
					codeDescriptionPairList.AddPair("NB", "New Brunswick");
					codeDescriptionPairList.AddPair("NL", "Newfoundland and Labrador");
					codeDescriptionPairList.AddPair("NT", "Northwest Territories");
					codeDescriptionPairList.AddPair("NS", "Nova Scotia");
					codeDescriptionPairList.AddPair("NU", "Nunavut");
					codeDescriptionPairList.AddPair("ON", "Ontario");
					codeDescriptionPairList.AddPair("PE", "Prince Edward Island");
					codeDescriptionPairList.AddPair("QC", "Quebec");
					codeDescriptionPairList.AddPair("SK", "Saskatchewan");
					codeDescriptionPairList.AddPair("YT", "Yukon Territory");
					return codeDescriptionPairList;
				case "US":
					codeDescriptionPairList.AddPair("AL", "Alabama");
					codeDescriptionPairList.AddPair("AK", "Alaska");
					codeDescriptionPairList.AddPair("AZ", "Arizona");
					codeDescriptionPairList.AddPair("AR", "Arkansas");
					codeDescriptionPairList.AddPair("CA", "California");
					codeDescriptionPairList.AddPair("CO", "Colorado");
					codeDescriptionPairList.AddPair("CT", "Connecticut");
					codeDescriptionPairList.AddPair("DE", "Delaware");
					codeDescriptionPairList.AddPair("DC", "District Of Columbia");
					codeDescriptionPairList.AddPair("FL", "Florida");
					codeDescriptionPairList.AddPair("GA", "Georgia");
					codeDescriptionPairList.AddPair("HI", "Hawaii");
					codeDescriptionPairList.AddPair("ID", "Idaho");
					codeDescriptionPairList.AddPair("IL", "Illinois");
					codeDescriptionPairList.AddPair("IN", "Indiana");
					codeDescriptionPairList.AddPair("IA", "Iowa");
					codeDescriptionPairList.AddPair("KS", "Kansas");
					codeDescriptionPairList.AddPair("KY", "Kentucky");
					codeDescriptionPairList.AddPair("LA", "Louisiana");
					codeDescriptionPairList.AddPair("ME", "Maine");
					codeDescriptionPairList.AddPair("MD", "Maryland");
					codeDescriptionPairList.AddPair("MA", "Massachusetts");
					codeDescriptionPairList.AddPair("MI", "Michigan");
					codeDescriptionPairList.AddPair("MN", "Minnesota");
					codeDescriptionPairList.AddPair("MS", "Mississippi");
					codeDescriptionPairList.AddPair("MO", "Missouri");
					codeDescriptionPairList.AddPair("MT", "Montana");
					codeDescriptionPairList.AddPair("NE", "Nebraska");
					codeDescriptionPairList.AddPair("NV", "Nevada");
					codeDescriptionPairList.AddPair("NH", "New Hampshire");
					codeDescriptionPairList.AddPair("NJ", "New Jersey");
					codeDescriptionPairList.AddPair("NM", "New Mexico");
					codeDescriptionPairList.AddPair("NY", "New York");
					codeDescriptionPairList.AddPair("NC", "North Carolina");
					codeDescriptionPairList.AddPair("ND", "North Dakota");
					codeDescriptionPairList.AddPair("OH", "Ohio");
					codeDescriptionPairList.AddPair("OK", "Oklahoma");
					codeDescriptionPairList.AddPair("OR", "Oregon");
					codeDescriptionPairList.AddPair("PA", "Pennsylvania");
					codeDescriptionPairList.AddPair("RI", "Rhode Island");
					codeDescriptionPairList.AddPair("SC", "South Carolina");
					codeDescriptionPairList.AddPair("SD", "South Dakota");
					codeDescriptionPairList.AddPair("TN", "Tennessee");
					codeDescriptionPairList.AddPair("TX", "Texas");
					codeDescriptionPairList.AddPair("UT", "Utah");
					codeDescriptionPairList.AddPair("VT", "Vermont");
					codeDescriptionPairList.AddPair("VA", "Virginia");
					codeDescriptionPairList.AddPair("WA", "Washington");
					codeDescriptionPairList.AddPair("WV", "West Virginia");
					codeDescriptionPairList.AddPair("WI", "Wisconsin");
					codeDescriptionPairList.AddPair("WY", "Wyoming");
					return codeDescriptionPairList;
				default:
					return codeDescriptionPairList;
			}
		}

		public ReadOnlyCodeDescriptionPairList GetDependenceCodeDescriptionPairList(string value)
		{
			ZGuid.TryParse(value, out ZGuid zguid);
			return GetCodeDescriptionPairList(zguid);
		}

		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			//return values must include result of GetDependenceCodeDescriptionPairList, otherwise report will fail to run.
			return GetCodeDescriptionPairList(ZGuid.Empty);
		}

		public CodeDescriptionPairList StateOfOriginCodeList()
		{
			CodeDescriptionPairList codeDescriptionPairList = new CodeDescriptionPairList();
			codeDescriptionPairList.AddPair("AB", "Alberta");
			codeDescriptionPairList.AddPair("BC", "British Columbia");
			codeDescriptionPairList.AddPair("MB", "Manitoba");
			codeDescriptionPairList.AddPair("NB", "New Brunswick");
			codeDescriptionPairList.AddPair("NL", "Newfoundland and Labrador");
			codeDescriptionPairList.AddPair("NT", "Northwest Territories");
			codeDescriptionPairList.AddPair("NS", "Nova Scotia");
			codeDescriptionPairList.AddPair("NU", "Nunavut");
			codeDescriptionPairList.AddPair("ON", "Ontario");
			codeDescriptionPairList.AddPair("PE", "Prince Edward Island");
			codeDescriptionPairList.AddPair("QC", "Quebec");
			codeDescriptionPairList.AddPair("SK", "Saskatchewan");
			codeDescriptionPairList.AddPair("YT", "Yukon Territory");
			codeDescriptionPairList.AddPair("AL", "Alabama");
			codeDescriptionPairList.AddPair("AK", "Alaska");
			codeDescriptionPairList.AddPair("AZ", "Arizona");
			codeDescriptionPairList.AddPair("AR", "Arkansas");
			codeDescriptionPairList.AddPair("CA", "California");
			codeDescriptionPairList.AddPair("CO", "Colorado");
			codeDescriptionPairList.AddPair("CT", "Connecticut");
			codeDescriptionPairList.AddPair("DE", "Delaware");
			codeDescriptionPairList.AddPair("DC", "District Of Columbia");
			codeDescriptionPairList.AddPair("FL", "Florida");
			codeDescriptionPairList.AddPair("GA", "Georgia");
			codeDescriptionPairList.AddPair("HI", "Hawaii");
			codeDescriptionPairList.AddPair("ID", "Idaho");
			codeDescriptionPairList.AddPair("IL", "Illinois");
			codeDescriptionPairList.AddPair("IN", "Indiana");
			codeDescriptionPairList.AddPair("IA", "Iowa");
			codeDescriptionPairList.AddPair("KS", "Kansas");
			codeDescriptionPairList.AddPair("KY", "Kentucky");
			codeDescriptionPairList.AddPair("LA", "Louisiana");
			codeDescriptionPairList.AddPair("ME", "Maine");
			codeDescriptionPairList.AddPair("MD", "Maryland");
			codeDescriptionPairList.AddPair("MA", "Massachusetts");
			codeDescriptionPairList.AddPair("MI", "Michigan");
			codeDescriptionPairList.AddPair("MN", "Minnesota");
			codeDescriptionPairList.AddPair("MS", "Mississippi");
			codeDescriptionPairList.AddPair("MO", "Missouri");
			codeDescriptionPairList.AddPair("MT", "Montana");
			codeDescriptionPairList.AddPair("NE", "Nebraska");
			codeDescriptionPairList.AddPair("NV", "Nevada");
			codeDescriptionPairList.AddPair("NH", "New Hampshire");
			codeDescriptionPairList.AddPair("NJ", "New Jersey");
			codeDescriptionPairList.AddPair("NM", "New Mexico");
			codeDescriptionPairList.AddPair("NY", "New York");
			codeDescriptionPairList.AddPair("NC", "North Carolina");
			codeDescriptionPairList.AddPair("ND", "North Dakota");
			codeDescriptionPairList.AddPair("OH", "Ohio");
			codeDescriptionPairList.AddPair("OK", "Oklahoma");
			codeDescriptionPairList.AddPair("OR", "Oregon");
			codeDescriptionPairList.AddPair("PA", "Pennsylvania");
			codeDescriptionPairList.AddPair("RI", "Rhode Island");
			codeDescriptionPairList.AddPair("SC", "South Carolina");
			codeDescriptionPairList.AddPair("SD", "South Dakota");
			codeDescriptionPairList.AddPair("TN", "Tennessee");
			codeDescriptionPairList.AddPair("TX", "Texas");
			codeDescriptionPairList.AddPair("UT", "Utah");
			codeDescriptionPairList.AddPair("VT", "Vermont");
			codeDescriptionPairList.AddPair("VA", "Virginia");
			codeDescriptionPairList.AddPair("WA", "Washington");
			codeDescriptionPairList.AddPair("WV", "West Virginia");
			codeDescriptionPairList.AddPair("WI", "Wisconsin");
			codeDescriptionPairList.AddPair("WY", "Wyoming");
			return codeDescriptionPairList;
		}
	}
}
