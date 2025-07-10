using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using CountryCodes = Enterprise.Core.Constants.CountryCodes;

namespace Enterprise.Accounting.Export.Business
{
	public class AuthorizationDataHelper
	{
		readonly BaseDataAccess DataAccess;

		public AuthorizationDataHelper(BaseDataAccess dataAccess)
		{
			this.DataAccess = dataAccess;
		}

		public Country LoadCountryOrNull(ZString? recordType, ZGuid transactionPk)
		{
			var result = TryLookupCountryBy(recordType);
			if (result != null)
			{
				return result;
			}
			return LoadCountryByTransactionPk(transactionPk);
		}

		public ZString GetVersion(ZString? recordType)
		{
			var recordTypeNotNull = recordType.GetValueOrDefault(ZString.Empty);
			return recordTypeNotNull == AccTransactionHeaderAuthorisationRecordTypes.AllOtherCountries
				? DataTransferConstants.AccTransactionHeaderAuthorisationRecord.Version0
				: DataTransferConstants.AccTransactionHeaderAuthorisationRecord.Version1;
		}

		#region Implementation

		readonly Dictionary<string, string> CountryCodeToDescriptionLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		Country TryLookupCountryBy(ZString? recordType)
		{
			switch (recordType)
			{
				// Please keep this list in alphabetical order.
				case AccTransactionHeaderAuthorisationRecordTypes.Argentina:
					return TryCreateCountry(CountryCodes.Argentina);
				case AccTransactionHeaderAuthorisationRecordTypes.Brazil:
					return TryCreateCountry(CountryCodes.Brazil);
				case AccTransactionHeaderAuthorisationRecordTypes.Chile:
					return TryCreateCountry(CountryCodes.Chile);
				case AccTransactionHeaderAuthorisationRecordTypes.Colombia:
					return TryCreateCountry(CountryCodes.Colombia);
				case AccTransactionHeaderAuthorisationRecordTypes.DominicanRepublic:
					return TryCreateCountry(CountryCodes.DominicanRepublic);
				case AccTransactionHeaderAuthorisationRecordTypes.Egypt:
					return TryCreateCountry(CountryCodes.Egypt);
				case AccTransactionHeaderAuthorisationRecordTypes.Fiji:
					return TryCreateCountry(CountryCodes.Fiji);
				case AccTransactionHeaderAuthorisationRecordTypes.India:
					return TryCreateCountry(CountryCodes.India);
				case AccTransactionHeaderAuthorisationRecordTypes.Jordan:
					return TryCreateCountry(CountryCodes.Jordan);
				case AccTransactionHeaderAuthorisationRecordTypes.KoreaSouth:
					return TryCreateCountry(CountryCodes.KoreaSouth);
				case AccTransactionHeaderAuthorisationRecordTypes.Latvia:
					return TryCreateCountry(CountryCodes.Latvia);
				case AccTransactionHeaderAuthorisationRecordTypes.Mexico:
					return TryCreateCountry(CountryCodes.Mexico);
				case AccTransactionHeaderAuthorisationRecordTypes.Malaysia:
					return TryCreateCountry(CountryCodes.Malaysia);
				case AccTransactionHeaderAuthorisationRecordTypes.Mauritius:
					return TryCreateCountry(CountryCodes.Mauritius);
				case AccTransactionHeaderAuthorisationRecordTypes.Panama:
					return TryCreateCountry(CountryCodes.Panama);
				case AccTransactionHeaderAuthorisationRecordTypes.Poland:
					return TryCreateCountry(CountryCodes.Poland);
				case AccTransactionHeaderAuthorisationRecordTypes.Romania:
					return TryCreateCountry(CountryCodes.Romania);
				case AccTransactionHeaderAuthorisationRecordTypes.SaudiArabia:
					return TryCreateCountry(CountryCodes.SaudiArabia);
				case AccTransactionHeaderAuthorisationRecordTypes.Spain:
					return TryCreateCountry(CountryCodes.Spain);
				case AccTransactionHeaderAuthorisationRecordTypes.Uruguay:
					return TryCreateCountry(CountryCodes.Uruguay);
				case AccTransactionHeaderAuthorisationRecordTypes.WesternSamoa:
					return TryCreateCountry(CountryCodes.WesternSamoa);
				default:
					return null;
			}

			Country TryCreateCountry(string countryTwoLetterCode) =>
				CountryCodeToDescriptionLookup.TryGetValue(countryTwoLetterCode, out var countryName)
					? new Country() { Code = countryTwoLetterCode, Name = countryName }
					: LoadCountryByCode(countryTwoLetterCode);
		}

		Country LoadCountryByCode(string twoLetterCountryCode)
		{
			var sql = @"
SELECT TOP 1 RN_Code, RN_Desc
FROM dbo.RefCountry
WHERE RN_Code = @code
";
			using (var cmd = DataAccess.GetCommand(sql))
			{
				var param = new SqlParameterWrapper(cmd.CreateParameter());
#pragma warning disable CW1161 // sql param name
				param.ParameterName = "@code";
#pragma warning restore CW1161
				param.SqlDbType = System.Data.SqlDbType.Char;
				param.Size = 2;
				param.Value = twoLetterCountryCode;
				cmd.Parameters.Add(param.Base);
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						var result = new Country()
						{
							Code = (string)reader["RN_Code"],
							Name = (string)reader["RN_Desc"]
						};
						CountryCodeToDescriptionLookup.Add(result.Code, result.Name);
						return result;
					}
					else
					{
						return null;
					}
				}
			}
		}

		Country LoadCountryByTransactionPk(ZGuid transactionPk)
		{
			if (!transactionPk.IsValid)
			{
				return null;
			}

			var sql = @"
SELECT TOP 1 GC_RN_NKCountryCode, RN_Desc
FROM dbo.AccTransactionHeader
INNER JOIN dbo.GlbCompany
	ON AH_GC = GC_PK
LEFT JOIN dbo.RefCountry
	ON GC_RN_NKCountryCode = RN_Code
WHERE AH_PK = @pk";

			using (var cmd = DataAccess.GetCommand(sql))
			{
				var param = new SqlParameterWrapper(cmd.CreateParameter());
#pragma warning disable CW1161 // sql param name
				param.ParameterName = "@pk";
#pragma warning restore CW1161
				param.SqlDbType = System.Data.SqlDbType.UniqueIdentifier;
				param.Value = transactionPk.ToGuid();
				cmd.Parameters.Add(param.Base);
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						return new Country()
						{
							Code = (string)reader["GC_RN_NKCountryCode"],
							Name = (string)reader["RN_Desc"]
						};
					}
					else
					{
						return null;
					}
				}
			}
		}

		#endregion
	}
}
