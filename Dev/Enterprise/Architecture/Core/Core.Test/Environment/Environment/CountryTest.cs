using System.Data.Common;
using System.Reflection;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class CountryTest : TransactionedTestCase
	{
		#region Cash Basis GST

		public void TestIsGSTCashBasis()
		{
			Countries countries = new Countries();
			foreach (string country in countries)
			{
				if (country == Constants.CountryCodes.CoteDivoire
					|| country == Constants.CountryCodes.Cameroon
					|| country == Constants.CountryCodes.EquatorialGuinea
					|| country == Constants.CountryCodes.Chad)
				{
					AssertEquals(country + " IsCashBasisGST", true, Country.IsGSTCashBasis(country));
				}
				else
				{
					AssertEquals(country + " IsCashBasisGST", false, Country.IsGSTCashBasis(country));
				}
			}
		}

		#endregion

		#region Consumption Tax Description / Consumption Tax Regsitration Code /GST Registered

		public void TestNoDuplicateOrgCusCode()
		{
			var orgHeaderPK = Db.Connection.ExecuteScalar("SELECT TOP 1 OH_PK FROM dbo.OrgHeader INNER JOIN dbo.OrgAddress ON OA_OH = OH_PK");
			var orgAddressPK = Db.Connection.ExecuteScalar($"SELECT TOP 1 OA_PK FROM dbo.OrgAddress WHERE OA_OH = @orgHeaderPK", cmd => cmd.AddParameterBasedOnDbColumn("@orgHeaderPK", orgHeaderPK, OrgAddressSchema.OA_OH));

			var countries = new Countries();
			foreach (var countryCode in countries)
			{
				var orgCusCode = Country.GetConsumptionTaxRegistrationOrgCusCode(countryCode);
				if (!orgCusCode.IsNullOrEmpty())
				{
					var insertSQL = $@"
							DECLARE @OhPk uniqueidentifier = (SELECT TOP 1 OH_PK FROM dbo.OrgHeader);
							DECLARE @oaPk uniqueidentifier = (SELECT TOP 1 OA_PK FROM dbo.OrgAddress WHERE OA_OH = @OhPk)

							INSERT INTO[dbo].[OrgCusCode] ([OK_PK],[OK_IsValid],[OK_CustomsRegNo],[OK_CodeType],[OK_OH],[OK_OA_PremisesAddress],[OK_RN_NKCodeCountry],[OK_CountryDefault])
							VALUES (newid(), 1, '12345', '{orgCusCode}', '{orgHeaderPK}', '{orgAddressPK}', '{countryCode}', 0)
							";

					var insertSQL2 = $@"
							DECLARE @OhPk uniqueidentifier = (SELECT TOP 1 OH_PK FROM dbo.OrgHeader);
							DECLARE @oaPk uniqueidentifier = (SELECT TOP 1 OA_PK FROM dbo.OrgAddress WHERE OA_OH = @OhPk)

							INSERT INTO[dbo].[OrgCusCode] ([OK_PK],[OK_IsValid],[OK_CustomsRegNo],[OK_CodeType],[OK_OH],[OK_OA_PremisesAddress],[OK_RN_NKCodeCountry],[OK_CountryDefault])
							VALUES (newid(), 1, '12345', '{orgCusCode}', '{orgHeaderPK}', NULL, '{countryCode}', 0)
							";

					AssertNoExceptionThrown(() => Db.Connection.ExecuteNonQuery(insertSQL));
					AssertExceptionThrown<DbException>("Duplicate sql exception",
						$"Cannot insert duplicate key row in object 'dbo.OrgCusCode' with unique index 'NR_UX__OK_CodeType_OK_OH_OK_RN_NKCodeCountry_OK_OA_PremisesAddress'. The duplicate key value is ({orgCusCode}, {orgHeaderPK}, {countryCode}, {orgAddressPK}).\r\nThe statement has been terminated.",
						() => Db.Connection.ExecuteNonQuery(insertSQL));
					AssertExceptionThrown<DbException>("Duplicate sql exception",
						$"Cannot insert duplicate key row in object 'dbo.OrgCusCode' with unique index 'NR_UX__OK_OH_OK_RN_NKCodeCountry_OK_CodeType'. The duplicate key value is ({orgHeaderPK}, {countryCode}, {orgCusCode}).\r\nThe statement has been terminated.",
						() => Db.Connection.ExecuteNonQuery(insertSQL2));
				}
			}
		}

		public void TestConsumptionTaxRegistrationOrgCusCodeMaxLength()
		{
			var fields = typeof(Constants.CountryCodes).GetFields(BindingFlags.Public | BindingFlags.Static);
			foreach (FieldInfo field in fields)
			{
				string countryCode = (string)field.GetValue(null);
				object result = Db.Connection.ExecuteScalar("select " + RefCountrySchema.PK.Name + " from " + RefCountrySchema.Constants.SqlSchemaName + "." + RefCountrySchema.Constants.TableName + " where + " + RefCountrySchema.RN_Code.Name + " = '" + countryCode + "'");
				if (result != null)
				{
					string consumptionTaxRegistrationOrgCusCode = Country.GetConsumptionTaxRegistrationOrgCusCode(countryCode);
					Assert("Max length should be the same as OrgCusCodeSchema.OK_CodeType.MaxLength but for country " + countryCode + " was " + consumptionTaxRegistrationOrgCusCode.Length, consumptionTaxRegistrationOrgCusCode.Length <= OrgCusCodeSchema.OK_CodeType.MaxLength);
				}
			}
		}

		public void TestGetDomesticNameofTaxCode()
		{
			var fields = typeof(Constants.CountryCodes).GetFields(BindingFlags.Public | BindingFlags.Static);
			foreach (FieldInfo field in fields)
			{
				string countryCode = (string)field.GetValue(null);
				string domesticTaxCode = Country.GetDomesticNameofTaxCode(countryCode);
				switch (countryCode)
				{
					case Constants.CountryCodes.Germany:
						AssertEquals("MWST", domesticTaxCode);
						continue;
					case Constants.CountryCodes.Denmark:
					case Constants.CountryCodes.Sweden:
						AssertEquals("MOMS", domesticTaxCode);
						continue;
					default:
						AssertEquals(Country.GetConsumptionTaxDescription(countryCode), domesticTaxCode);
						continue;
				}
			}
		}

		public void TestGetConsumptionTaxRegistrationCodesForOrgCountry()
		{
			AssertGetConsumptionTaxRegistrationCodesForOrgCountry("fallback order for China", Constants.CountryCodes.China, new string[] { "VAT", "GBR", "GCR" });

			AssertGetConsumptionTaxRegistrationCodesForOrgCountry("fallback order for Brazil", Constants.CountryCodes.Brazil, new string[] { "CMT", "CJN", "GBR", "GCR" });

			AssertGetConsumptionTaxRegistrationCodesForOrgCountry("fallback order for Argentina", Constants.CountryCodes.Argentina, new string[] { "IVA", "CUI", "GBR", "GCR" });

			AssertGetConsumptionTaxRegistrationCodesForOrgCountry("fallback order for India", Constants.CountryCodes.India, new string[] { "SER", "PAN", "GBR", "GCR" });

			AssertGetConsumptionTaxRegistrationCodesForOrgCountry("fallback order for Chile", Constants.CountryCodes.Chile, new string[] { "IVA", "RUT", "GBR", "GCR" });

			AssertGetConsumptionTaxRegistrationCodesForOrgCountry("fallback order for Spain", Constants.CountryCodes.Spain, new string[] { "NIF", "DNI", "IGC", "GBR", "GCR" });
		}

		void AssertGetConsumptionTaxRegistrationCodesForOrgCountry(string descStr, string orgCountryCode, string[] expectedResult)
		{
			var codeList = Country.GetConsumptionTaxRegistrationCodesForOrgCountry(orgCountryCode);
			AssertEquals(descStr, string.Join("-", expectedResult), string.Join("-", codeList));
		}

		#endregion
	}
}
