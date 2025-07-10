using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DocumentEngineCore.DocWrappers.CustomIndexerList(typeof(RegNumberListForThisCountry))]
	public class RegistrationNumberCodeWrapperCollection : GenericWrapperCollection<RegistrationNumberCodeWrapper>
	{
		public RegistrationNumberCodeWrapperCollection(OrgCusCodeCollection cusCodeCollection, BusinessObjectFactory factory)
			: base(factory)
		{
			LoadCusCodeCollection(cusCodeCollection);
		}

		public RegistrationNumberCodeWrapperCollection(OrgAddressCusCodeCollection cusCodeCollection, BusinessObjectFactory factory)
			: base(factory)
		{
			if (cusCodeCollection != null)
			{
				foreach (OrgCusCode cusCode in cusCodeCollection)
				{
					Add(new RegistrationNumberCodeWrapper(cusCode, factory));
				}
			}
		}

		public RegistrationNumberCodeWrapperCollection(OrgHeader orgHeader, BusinessObjectFactory factory)
			: base(factory)
		{
			Organisation = orgHeader;

			if (orgHeader != null)
			{
				LoadCusCodeCollection(orgHeader.CustomsCodes);
			}
		}

		OrgHeader Organisation { get; set; }

		void LoadCusCodeCollection(OrgCusCodeCollection cusCodeCollection)
		{
			if (cusCodeCollection != null)
			{
				foreach (OrgCusCode cusCode in cusCodeCollection)
				{
					Add(new RegistrationNumberCodeWrapper(cusCode, Factory));
				}
			}
		}

		protected override DocumentEngineCore.DocWrappers.IBODocDataProvider GetRow(ZString index)
		{
			const char divider = ':';

			int dividerIndex = index.IndexOf(divider);
			string countryCode;
			string cusCode;

			if (dividerIndex < 0)
			{
				countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				cusCode = index;
			}
			else
			{
				countryCode = index.SubstringSafe(0, dividerIndex);
				cusCode = index.SubstringSafe(dividerIndex + 1);
			}

			if (cusCode == "EORI") // European Union EORI Code
			{
				return new RegistrationNumberCodeWrapper(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, Organisation.GetEuIdentificationNumber(), Factory);
			}
			else
			{
				if (cusCode == "CONSUMPTIONTAX" && !string.IsNullOrWhiteSpace(countryCode)) // Country Specific Consumption Tax
				{
					RefCountry country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode);
					if (country != null)
					{
						cusCode = country.ConsumptionTaxRegistrationCode;
					}
				}

				foreach (RegistrationNumberCodeWrapper wrapper in this)
				{
					if (wrapper.CountryOfIssue.Code == countryCode && wrapper.Type.Code == cusCode)
					{
						return wrapper;
					}
				}
			}

			return base.GetRow(index);
		}
	}

	public class RegNumberListForThisCountry : CodeDescriptionPairList
	{
		public RegNumberListForThisCountry()
		{
			CodeDescriptionPairList countryList = new OrgCodeLists().CustomsCodes_List(GlbCompany.CurrentCompany.Country);
			foreach (ICodeDescription codeDescriptionPair in countryList)
			{
				Add(codeDescriptionPair);
			}
		}
	}
}
