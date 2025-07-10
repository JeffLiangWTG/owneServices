using System;
using System.Collections.Concurrent;
using System.Data;
using CargoWise.Data;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ResString = Enterprise.ZArchitecture.Core.SourceGenerated.ResString;

namespace Enterprise.ZArchitecture.Environment
{
	class EnglishSpellingRegistryItem : CodePairRegistryItem
	{
		public EnglishSpellingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new EnglishSpellingRegistryItemImpl(name, category, caption, hint, storage))
		{ }
	}

	class EnglishSpellingRegistryItemImpl : RegistryItemImpl
	{
		public EnglishSpellingRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(name, category, caption, hint, new CodePairRegistryDataType(new CodeDescriptionPairListProvider(() => new EnglishSpellingOptionsList()), false, true), storage)
		{
		}

		protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
#if DEBUG
			if (companyPK == Guid.Parse("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC")) // Default dev company
			{
				return Constants.Languages.EnglishAmerican;
			}
#endif

			if (companyPK == Guid.Empty)
			{
				return Constants.Languages.EnglishAmerican;
			}

			var result = cache.GetOrAdd(companyPK, GetResults);

			if (!string.IsNullOrEmpty(result.languageCode) && Res.IsEnglish(result.languageCode) && result.languageCode != Res.DefaultLanguage)
			{
				return result.languageCode;
			}
			else
			{
				return Culture.GetDefaultEnglishSpellingForCountry(result.countryCode);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		(string countryCode, string languageCode) GetResults(Guid companyPK)
		{
			const string sql = "SELECT " + GlbCompanySchema.Constants.GC_RN_NKCountryCode + "," + OrgHeaderSchema.Constants.OH_Language + " FROM " + GlbCompanySchema.Constants.SqlSchemaName + "." + GlbCompanySchema.Constants.TableName +
							   " LEFT OUTER JOIN " + OrgHeaderSchema.Constants.SqlSchemaName + "." + OrgHeaderSchema.Constants.TableName + " ON " + OrgHeaderSchema.Constants.PK + " = " + GlbCompanySchema.Constants.GC_OH_OrgProxy +
							   " WHERE " + GlbCompanySchema.Constants.PK + " = @CompanyPK";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("CompanyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					if (reader.Read())
					{
						return (reader[GlbCompanySchema.Constants.GC_RN_NKCountryCode] as string ?? string.Empty, reader[OrgHeaderSchema.Constants.OH_Language] as string ?? string.Empty);
					}
				}
			}
			return ("", "");
		}

		static readonly ConcurrentDictionary<Guid, (string countryCode, string languageCode)> cache = new ConcurrentDictionary<Guid, (string countryCode, string languageCode)>();
	}

	public class EnglishSpellingOptionsList : CodeDescriptionPairList
	{
		public EnglishSpellingOptionsList()
		{
			AddPair(Constants.Languages.EnglishAmerican, ResString.GetMultilingualString("c91fa268-5698-4033-b3c6-5172a6bd4395", "American English Spelling"));
			AddPair(Constants.Languages.EnglishBritish, ResString.GetMultilingualString("558e5688-9c56-4970-ba3e-0ee61656cd32", "British English Spelling"));
		}
	}
}
