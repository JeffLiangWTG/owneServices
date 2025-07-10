using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;

namespace Enterprise.Accounting.Export.Business.Testing
{
	class AuthorizationDataHelperTest : TestCaseWithFactory
	{
		public void TestLoadCountryOrNull_ReturnsExpectedCountry_ForAllRecordTypes()
		{
			var dataHelper = CreateDataHelper();
			var recordTypesWithCountry = AllRecordTypesFromDatabaseConstraint()
				.OrderBy(recordType => recordType)
				.Select(recordType => new { recordType, country = dataHelper.LoadCountryOrNull(recordType, ZGuid.Empty) })
				.Select(x => $"{x.recordType}|{x.country?.Code}|{x.country?.Name}");
			var result = string.Join("\r\n", recordTypesWithCountry);

			var expected = @"ARG|AR|Argentina
BRZ|BR|Brazil
CHL|CL|Chile
COL|CO|Colombia
DOM|DO|Dominican Republic
EGY|EG|Egypt
ESP|ES|Spain
FJI|FJ|Fiji
INI|IN|India
JOR|JO|Jordan
KRS|KR|Korea, Republic of
KSA|SA|Saudi Arabia
LVA|LV|Latvia
MAY|MY|Malaysia
MEX|MX|Mexico
MUS|MU|Mauritius
PAN|PA|Panama
POL|PL|Poland
ROA|RO|Romania
URU|UY|Uruguay
WSI|WS|Samoa
ZZZ||";
			AssertMultilineASCIIEquals("All possible record types must map to a country, or to null", expected, result);
		}

		public void TestLoadCountryOrNull_ReturnsCountry_FromTransactionCompany()
		{
			var auTransaction = CreateTransactionForCompanyInCountry("AU");
			var adTransaction = CreateTransactionForCompanyInCountry("AD");
			var brTransaction = CreateTransactionForCompanyInCountry("BR");
			Factory.Save();

			var dataHelper = CreateDataHelper();
			var australia = dataHelper.LoadCountryOrNull(null, auTransaction.PK);
			CombineAssertions("AU Transaction should return Australia Country", () =>
			{
				AssertEquals("AU", australia.Code);
				AssertEquals("Australia", australia.Name);
			});

			var andorra = dataHelper.LoadCountryOrNull(null, adTransaction.PK);
			CombineAssertions("AD Transaction should return Andorra Country", () =>
			{
				AssertEquals("AD", andorra.Code);
				AssertEquals("Andorra", andorra.Name);
			});

			var brazil = dataHelper.LoadCountryOrNull(null, brTransaction.PK);
			CombineAssertions("BR Transaction should return Brazil Country", () =>
			{
				AssertEquals("BR", brazil.Code);
				AssertEquals("Brazil", brazil.Name);
			});

			var noTransaction = dataHelper.LoadCountryOrNull(null, ZGuid.Empty);
			AssertNull("Invalid transaction id should return null", noTransaction);
		}

		public void TestLoadCountryOrNull_PrefersRecordTypeOverTransaction()
		{
			var auTransaction = CreateTransactionForCompanyInCountry("AU");
			Factory.Save();

			var dataHelper = CreateDataHelper();
			var mexico = dataHelper.LoadCountryOrNull(AccTransactionHeaderAuthorisationRecordTypes.Mexico, auTransaction.PK);
			CombineAssertions("Mexico record type should return Mexico Country even when transaction is for AU", () =>
			{
				AssertEquals("MX", mexico.Code);
				AssertEquals("Mexico", mexico.Name);
			});

			var australia = dataHelper.LoadCountryOrNull(AccTransactionHeaderAuthorisationRecordTypes.AllOtherCountries, auTransaction.PK);
			CombineAssertions("All other countries record type should return AU Country from transaction", () =>
			{
				AssertEquals("AU", australia.Code);
				AssertEquals("Australia", australia.Name);
			});
		}

		public void TestGetVersion_ReturnsOne_ForKnownRecordTypesExceptZZZ()
		{
			var dataHelper = CreateDataHelper();
			var nullVersion = dataHelper.GetVersion(null);
			AssertEquals("Null record type should version 1", "1", nullVersion);
			var emptyVersion = dataHelper.GetVersion("");
			AssertEquals("Empty record type should version 1", "1", emptyVersion);

			var recordTypesFromConstraint = AllCountryRecordTypesFromDatabaseConstraint();
			foreach (var recordType in recordTypesFromConstraint)
			{
				var version = dataHelper.GetVersion(recordType);
				AssertEquals("(Almost) all record types should version 1", "1", version);
			}
		}

		public void TestGetVersion_ReturnsZero_ForZZZRecordType()
		{
			var dataHelper = CreateDataHelper();
			var zzzVersion = dataHelper.GetVersion(AccTransactionHeaderAuthorisationRecordTypes.AllOtherCountries);
			AssertEquals("All Other Countries 'ZZZ' record type should version 0", "0", zzzVersion);
		}

		#region Implementation

		IEnumerable<string> AllRecordTypesFromDatabaseConstraint()
		{
			var constraintDefinition = TestConnection.ExecuteScalar<string>("SELECT TOP 1 definition FROM sys.check_constraints WHERE name = 'Constraint_AHF_RecordType'");
			var regex = new Regex(@"\[AHF_RecordType\]='(?<code>.*?)'", RegexOptions.IgnoreCase);
			var recordTypes = regex.Matches(constraintDefinition).Cast<Match>().Select(m => m.Groups["code"].Value);
			return recordTypes;
		}

		IEnumerable<string> AllCountryRecordTypesFromDatabaseConstraint()
			=> AllRecordTypesFromDatabaseConstraint().Where(code => code != AccTransactionHeaderAuthorisationRecordTypes.AllOtherCountries);

		AuthorizationDataHelper CreateDataHelper()
			=> new AuthorizationDataHelper(
				new BaseDataAccess(
					((IDbConnectionInternals)base.TestConnection).ADOConnection,
					((IDbConnectionInternals)base.TestConnection).ADOTransaction
				)
			);

		AccTransactionHeader CreateTransactionForCompanyInCountry(string countryCode)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = countryCode;
			var result = Factory.NewWithValidTestData<AccTransactionHeader>();
			result.AH_GC = company.PK;
			return result;
		}

		#endregion
	}
}
