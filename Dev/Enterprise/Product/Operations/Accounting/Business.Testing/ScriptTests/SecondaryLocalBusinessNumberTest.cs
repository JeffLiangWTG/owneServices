
using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class SecondaryLocalBusinessNumberTest : ScriptTest
	{
		public void TestSecondaryLocalBusinessNumber()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			DataTable result = RunScript(Core.Constants.CountryCodes.SriLanka, header.PK);
			AssertEquals(DBNull.Value, result.Rows[0][0]);

			header.CustomsCodes.AddNew(OrgCusCode.SriLankaCodeTypes.SVATBusinessRegistrationNumber, "SL12345", RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.SriLanka));
			header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GSTCode, "XX12345", RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.SriLanka));
			Factory.Save();
			result = RunScript(Core.Constants.CountryCodes.SriLanka, header.PK);
			AssertEquals("SL12345", result.Rows[0][0]);

			header = Factory.NewWithValidTestData<OrgHeader>();
			result = RunScript(Core.Constants.CountryCodes.Spain, header.PK);
			AssertEquals(DBNull.Value, result.Rows[0][0]);

			header.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.DNI, "ES12345", RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Spain));
			header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GSTCode, "XX12345", RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Spain));
			Factory.Save();
			result = RunScript(Core.Constants.CountryCodes.Spain, header.PK);
			AssertEquals("ES12345", result.Rows[0][0]);

			header = Factory.NewWithValidTestData<OrgHeader>();
			result = RunScript(Core.Constants.CountryCodes.Italy, header.PK);
			AssertEquals(DBNull.Value, result.Rows[0][0]);

			header.CustomsCodes.AddNew(ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale, "IT12345", RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Italy));
			header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.IVA, "XX12345", RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Italy));
			Factory.Save();
			result = RunScript(Core.Constants.CountryCodes.Italy, header.PK);
			AssertEquals("IT12345", result.Rows[0][0]);

			header = Factory.NewWithValidTestData<OrgHeader>();
			result = RunScript(Core.Constants.CountryCodes.France, header.PK);
			AssertEquals(DBNull.Value, result.Rows[0][0]);

			header.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.Siren, "FR12345", RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.France));
			header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GSTCode, "XX12345", RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.France));
			Factory.Save();
			result = RunScript(Core.Constants.CountryCodes.France, header.PK);
			AssertEquals("FR12345", result.Rows[0][0]);

			header.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.Siret, "FR98765", RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.France));
			Factory.Save();
			result = RunScript(Core.Constants.CountryCodes.France, header.PK);
			AssertEquals("FR98765", result.Rows[0][0]);

			header = Factory.NewWithValidTestData<OrgHeader>();
			result = RunScript(Core.Constants.CountryCodes.Thailand, header.PK);
			AssertEquals(DBNull.Value, result.Rows[0][0]);

			header.CustomsCodes.AddNew(OrgCusCode.ThailandCodeTypes.BID, "TH12345", RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Thailand));
			header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GSTCode, "XX12345", RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Thailand));
			Factory.Save();
			result = RunScript(Core.Constants.CountryCodes.Thailand, header.PK);
			AssertEquals("TH12345", result.Rows[0][0]);
		}

		DataTable RunScript(ZString country, ZGuid orgPK)
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"
SELECT OK_CustomsRegNO FROM dbo.SecondaryLocalBusinessNumber(
'{0}', 
'{1}'
)  
",
			country,
			orgPK
			));
		}
	}
}

