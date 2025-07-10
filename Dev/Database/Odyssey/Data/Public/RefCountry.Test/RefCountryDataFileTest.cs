using System;
using System.IO;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class RefCountryDataFileTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDataFileExists()
		{
			var dataFile = new RefCountryDataFile();
			Assert(File.Exists(dataFile.FileFullPath));
		}

		public void TestRefCountryDataFile()
		{
			string insertSql = @"
				INSERT dbo.RefCountry (RN_PK, RN_Code, RN_IsActive, RN_IsSystem, RN_Desc, RN_CountryDialingCode) VALUES ('B90956E5-9E68-406D-A52E-A90AE1F083BD', '11', 1, 1, 'Country 1', '111')
				INSERT dbo.RefCountry (RN_PK, RN_Code, RN_IsActive, RN_IsSystem, RN_Desc, RN_CountryDialingCode) VALUES ('78EDED25-FFCA-458E-AFB0-FBFA7C6795CF', '22', 1, 1, 'Country 2', '222')
				INSERT dbo.RefCurrency (RX_PK, RX_Code, RX_Desc, RX_ISOSubUnitRatio) VALUES ('ECD6A6FA-2ED8-4C28-9DB1-15B600DF13CB', '~CD', 'Soldi', 100)";
			Db.Connection.ExecuteNonQuery(insertSql);

			RefCountryDataFile file = new RefCountryDataFile();
			var data = file.LoadDataFromDatabase();

			AssertEquals("Table Count", 2, data.Tables.Count);
			AssertEquals("Table 0 Name", RefCountrySchema.Constants.TableName, data.Tables[0].TableName);
			AssertEquals("Table 1 Name", RefCurrencySchema.Constants.TableName, data.Tables[1].TableName);

			Assert("RefCountry row count", data.Tables[RefCountrySchema.Constants.TableName].Rows.Count >= 2);
			Assert("Contains Country 1", data.Tables[RefCountrySchema.Constants.TableName].Rows.Contains(new Guid("B90956E5-9E68-406D-A52E-A90AE1F083BD")));
			Assert("Contains Country 2", data.Tables[RefCountrySchema.Constants.TableName].Rows.Contains(new Guid("78EDED25-FFCA-458E-AFB0-FBFA7C6795CF")));

			Assert("RefCurrency row count", data.Tables[RefCurrencySchema.Constants.TableName].Rows.Count >= 1);
			Assert("Contains Currency", data.Tables[RefCurrencySchema.Constants.TableName].Rows.Contains(new Guid("ECD6A6FA-2ED8-4C28-9DB1-15B600DF13CB")));
		}
	}
}
