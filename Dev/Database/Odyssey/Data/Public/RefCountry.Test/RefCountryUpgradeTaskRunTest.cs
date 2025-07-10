using System;
using System.Data;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class RefCountryUpgradeTaskRunTest : TransactionedTestCase
	{
		public void TestRun()
		{
			// -----------------
			// PREPARE TEST DATA
			// -----------------

			// Existing and new test PKs and NKs
			InitialiseTestPks();
			InitialiseTestNks();

			// Prepare extra test data
			PrepareRefCountryTestData();
			PrepareRefCurrencyTestData();

			// --------
			// RUN TASK
			// --------

			RefCountryUpgradeTask testTask = new RefCountryUpgradeTask();
			testTask.Run();

			// --------------
			// CHECK RESULTS
			// --------------

			// Asserts
			AssertRefCountryChanges();

			AssertRefCurrencyChanges();
		}

		void InitialiseTestNks()
		{
			// Existing Nks
			exCapeVerdeEscudoNk = "CVE";
			imfSpecialCurrencyNk = "XDR";
		}

		void InitialiseTestPks()
		{
			// Gather existing PKs
			string sqlText = "SELECT RN_PK FROM dbo.RefCountry WHERE RN_Code = 'AU'";
			exAustraliaPk = (Guid)TestConnection.ExecuteScalar(sqlText);
			sqlText = "SELECT RN_PK FROM dbo.RefCountry WHERE RN_Code = 'VA'";
			exVaticanPk = (Guid)TestConnection.ExecuteScalar(sqlText);
			sqlText = "SELECT RN_PK FROM dbo.RefCountry WHERE RN_Code = 'XZ'";
			internationalWatersPk = (Guid)TestConnection.ExecuteScalar(sqlText);
			sqlText = "SELECT RX_PK FROM dbo.RefCurrency WHERE RX_Code = 'AUD'";
			exAuDollarPk = (Guid)TestConnection.ExecuteScalar(sqlText);
			sqlText = "SELECT RX_PK FROM dbo.RefCurrency WHERE RX_Code = 'CVE'";
			exCapeVerdeEscudoPk = (Guid)TestConnection.ExecuteScalar(sqlText);
			sqlText = "SELECT RX_PK FROM dbo.RefCurrency WHERE RX_Code = 'XDR'";
			imfSpecialCurrencyPk = (Guid)TestConnection.ExecuteScalar(sqlText);

			// New random PKs
			newVaticanPk = Guid.NewGuid();
			inexistingCodeCountryPk = Guid.NewGuid();
			inexistingCodeSystemCurrencyPk = Guid.NewGuid();
			inexistingCodeUserCurrencyPk = Guid.NewGuid();
			newCapeVerdeEscudoPk = Guid.NewGuid();
			newNonSystemCountryPk = Guid.NewGuid();
		}

		/// <summary>
		/// INSERT INEXISTING CODE       => To be marked as INACTIVE
		/// UPDATE AU -> INEXISTING CODE => To be marked as INACTIVE + 
		///                                 NEW AU record with NEW PK to be INSERTED
		/// UPDATE VA -> NEW PK          => NEW PK to be PRESERVED (NO changes to the record)
		/// DELETE XZ                    => NEW XZ record with SAME PK to be INSERTED
		/// </summary>
		void PrepareRefCountryTestData()
		{
			string sqlText = @"
				INSERT dbo.RefCountry (RN_PK, RN_Code, RN_IsActive, RN_Desc, RN_CountryDialingCode) VALUES (@InexistingCodeCountryPk, '~1', 1, 'Country Tildo1', '~1')
				INSERT dbo.RefCountry (RN_PK, RN_Code, RN_IsActive, RN_Desc, RN_CountryDialingCode, RN_IsSystem) VALUES (@NewNonSystemCountryPk, '~4', 1, 'Country Tildo1', '~4', 0)
				UPDATE dbo.RefCountry SET RN_Code = '~2' WHERE RN_PK = @ExAustraliaPk
				UPDATE dbo.RefCountry SET RN_PK = @NewVaticanPk WHERE RN_PK = @ExVaticanPk
				DELETE dbo.RefCountry WHERE RN_PK = @InternationalWatersPk";

			using (DbCommand cmd = TestConnection.Command(sqlText))
			{
				cmd.AddParameter("@ExAustraliaPk", SqlDbType.UniqueIdentifier, exAustraliaPk);
				cmd.AddParameter("@ExVaticanPk", SqlDbType.UniqueIdentifier, exVaticanPk);
				cmd.AddParameter("@NewVaticanPk", SqlDbType.UniqueIdentifier, newVaticanPk);
				cmd.AddParameter("@InternationalWatersPk", SqlDbType.UniqueIdentifier, internationalWatersPk);
				cmd.AddParameter("@InexistingCodeCountryPk", SqlDbType.UniqueIdentifier, inexistingCodeCountryPk);
				cmd.AddParameter("@NewNonSystemCountryPk", SqlDbType.UniqueIdentifier, newNonSystemCountryPk);

				cmd.ExecuteNonQuery();
			}
		}

		void AssertRefCountryChanges()
		{
			//   INSERT INEXISTING CODE       => To be marked as INACTIVE
			using (var cmd = TestConnection.Command("SELECT RN_IsActive FROM dbo.RefCountry WHERE RN_PK = @InexistingCodeCountryPk AND RN_Code = '~1'"))
			{
				cmd.AddParameter("@InexistingCodeCountryPk", SqlDbType.UniqueIdentifier, inexistingCodeCountryPk);
				var actualValue = cmd.ExecuteScalar();
				AssertEquals("Unmatching code country should have been inactivated (~1)", false, actualValue);
			}

			//   Manually added country should not be inactivated
			using (var cmd = TestConnection.Command("SELECT RN_IsActive FROM dbo.RefCountry WHERE RN_PK = @NewNonSystemCountryPk AND RN_Code = '~4'"))
			{
				cmd.AddParameter("@NewNonSystemCountryPk", SqlDbType.UniqueIdentifier, newNonSystemCountryPk);
				var actualValue = cmd.ExecuteScalar();
				AssertEquals("Manually added country should not be inactivated", true, actualValue);
			}

			//   UPDATE AU -> INEXISTING CODE => To be marked as INACTIVE
			using (var cmd = TestConnection.Command("SELECT RN_IsActive FROM dbo.RefCountry WHERE RN_PK = @ExAustraliaPk AND RN_Code = '~2'"))
			{
				cmd.AddParameter("@ExAustraliaPk", SqlDbType.UniqueIdentifier, exAustraliaPk);
				var actualValue = cmd.ExecuteScalar();
				AssertEquals("Unmatching code should have been inactivated (~2)", false, actualValue);
				//   UPDATE AU -> INEXISTING CODE => NEW AU record with NEW PK to be INSERTED
				cmd.CommandText = "SELECT RN_Desc FROM dbo.RefCountry WHERE RN_PK != @ExAustraliaPk AND RN_Code = 'AU'";
				actualValue = cmd.ExecuteScalar();
				AssertEquals("A NEW Australia record should have been inserted", "Australia", actualValue);
			}

			//   UPDATE VA -> NEW PK          => NEW PK to be PRESERVED (NO changes to the record)
			using (var cmd = TestConnection.Command("SELECT RN_Desc FROM dbo.RefCountry WHERE RN_PK = @NewVaticanPk AND RN_Code = 'VA'"))
			{
				cmd.AddParameter("@NewVaticanPk", SqlDbType.UniqueIdentifier, newVaticanPk);
				var actualValue = cmd.ExecuteScalar();
				AssertEquals("Vatican record should not have changed", "Holy See (Vatican City State)", actualValue);
			}

			//   DELETE XZ                    => NEW XZ record with SAME PK to be INSERTED
			using (var cmd = TestConnection.Command("SELECT RN_Desc FROM dbo.RefCountry WHERE RN_PK = @InternationalWatersPk AND RN_Code = 'XZ'"))
			{
				cmd.AddParameter("@InternationalWatersPk", SqlDbType.UniqueIdentifier, internationalWatersPk);
				var actualValue = cmd.ExecuteScalar();
				AssertEquals("International Waters record should have been re-inserted", "International Waters Installations", actualValue);
			}
		}

		/// <summary>
		/// INSERT INEXISTING SYSTEM CODE => To be marked as INACTIVE
		/// INSERT INEXISTING USER CODE   => Should NOT be CHANGED
		/// UPDATE AUD -> INEXISTING CODE => To be marked as INACTIVE + 
		///                                  NEW AUD record with NEW PK to be INSERTED
		/// UPDATE CVE -> NEW PK          => NEW PK to be PRESERVED (NO changes to the record)
		/// DELETE XDR                    => NEW XDR record with SAME PK to be INSERTED
		/// </summary>
		void PrepareRefCurrencyTestData()
		{
			string sqlText = @"
				INSERT dbo.RefCurrency (RX_PK, RX_Code, RX_IsSystem, RX_IsActive, RX_Desc, RX_ISOSubUnitRatio) VALUES (@InexistingCodeSystemPk, '~1', 1, 1, 'Sys Currency Tildo1', 100)
				INSERT dbo.RefCurrency (RX_PK, RX_Code, RX_IsSystem, RX_IsActive, RX_Desc, RX_ISOSubUnitRatio) VALUES (@InexistingCodeUserPk, '~2', 0, 1, 'User Currency Tildo2', 100)
				UPDATE dbo.RefCurrency SET RX_Code = '~3' WHERE RX_PK = @ExAuDollarPk
				DELETE dbo.RefExchangeRate WHERE RE_RX_NKExCurrency = @ExCapeVerdeEscudoNk
				UPDATE dbo.RefCurrency SET RX_PK = @NewCapeVerdeEscudoPk WHERE RX_PK = @ExCapeVerdeEscudoPk
				DELETE dbo.RefExchangeRate WHERE RE_RX_NKExCurrency = @ImfSpecialCurrencyNk
				DELETE dbo.RefCurrency WHERE RX_PK = @ImfSpecialCurrencyPk";

			using (DbCommand cmd = TestConnection.Command(sqlText))
			{
				cmd.AddParameter("@InexistingCodeSystemPk", SqlDbType.UniqueIdentifier, inexistingCodeSystemCurrencyPk);
				cmd.AddParameter("@InexistingCodeUserPk", SqlDbType.UniqueIdentifier, inexistingCodeUserCurrencyPk);
				cmd.AddParameter("@ExAuDollarPk", SqlDbType.UniqueIdentifier, exAuDollarPk);
				cmd.AddParameter("@ExCapeVerdeEscudoPk", SqlDbType.UniqueIdentifier, exCapeVerdeEscudoPk);
				cmd.AddParameter("@ExCapeVerdeEscudoNk", SqlDbType.VarChar, exCapeVerdeEscudoNk);
				cmd.AddParameter("@NewCapeVerdeEscudoPk", SqlDbType.UniqueIdentifier, newCapeVerdeEscudoPk);
				cmd.AddParameter("@ImfSpecialCurrencyPk", SqlDbType.UniqueIdentifier, imfSpecialCurrencyPk);
				cmd.AddParameter("@ImfSpecialCurrencyNk", SqlDbType.VarChar, imfSpecialCurrencyNk);

				cmd.ExecuteNonQuery();
			}
		}

		void AssertRefCurrencyChanges()
		{
			// INSERT INEXISTING SYSTEM CODE => To be marked as INACTIVE
			using (var cmd = TestConnection.Command("SELECT RX_IsActive FROM dbo.RefCurrency WHERE RX_PK = @InexistingCodeSystemCurrencyPk AND RX_Code = '~1'"))
			{
				cmd.AddParameter("@InexistingCodeSystemCurrencyPk", SqlDbType.UniqueIdentifier, inexistingCodeSystemCurrencyPk);
				var actualValue = cmd.ExecuteScalar();
				AssertEquals("Unmatching code system currency should be inactivated (~1)", false, actualValue);
			}

			// INSERT INEXISTING USER CODE   => NO changes to the record
			using (var cmd = TestConnection.Command("SELECT RX_IsActive FROM dbo.RefCurrency WHERE RX_PK = @InexistingCodeUserCurrencyPk AND RX_Code = '~2'"))
			{
				cmd.AddParameter("@InexistingCodeUserCurrencyPk", SqlDbType.UniqueIdentifier, inexistingCodeUserCurrencyPk);
				var actualValue = cmd.ExecuteScalar();
				AssertEquals("Unmatching code user currency should NOT change (~2)", true, actualValue);
			}

			// UPDATE AUD -> INEXISTING CODE => To be marked as INACTIVE
			using (var cmd = TestConnection.Command("SELECT RX_IsActive FROM dbo.RefCurrency WHERE RX_PK = @ExAuDollarPk AND RX_Code = '~3'"))
			{
				cmd.AddParameter("@ExAuDollarPk", SqlDbType.UniqueIdentifier, exAuDollarPk);
				var actualValue = cmd.ExecuteScalar();
				AssertEquals("Unmatching code should be inactivated (~3)", false, actualValue);
				// UPDATE AUD -> INEXISTING CODE => NEW AUD record with NEW PK to be INSERTED
				cmd.CommandText = "SELECT RX_Desc FROM dbo.RefCurrency WHERE RX_PK != @ExAuDollarPk AND RX_Code = 'AUD'";
				actualValue = cmd.ExecuteScalar().ToString();
				AssertEquals("A NEW Australian Dollar record should have been inserted", "Australian Dollar", actualValue);
			}

			// UPDATE CVE -> NEW PK          => NEW PK to be PRESERVED (NO changes to the record)
			using (var cmd = TestConnection.Command("SELECT RX_Desc FROM dbo.RefCurrency WHERE RX_PK = @NewCapeVerdeEscudoPk AND RX_Code = 'CVE'"))
			{
				cmd.AddParameter("@NewCapeVerdeEscudoPk", SqlDbType.UniqueIdentifier, newCapeVerdeEscudoPk);
				var actualValue = cmd.ExecuteScalar().ToString();
				AssertEquals("Cape Verde Escudos record should not have changed", "Cape Verde Escudo", actualValue);
			}

			// DELETE XDR                    => NEW XDR record with SAME PK to be INSERTED
			using (var cmd = TestConnection.Command("SELECT RX_Desc FROM dbo.RefCurrency WHERE RX_PK = @ImfSpecialCurrencyPk AND RX_Code = 'XDR'"))
			{
				cmd.AddParameter("@ImfSpecialCurrencyPk", SqlDbType.UniqueIdentifier, imfSpecialCurrencyPk);
				var actualValue = cmd.ExecuteScalar().ToString();
				AssertEquals("IMF special currency record should have been re-inserted", "IMF Special Drawing Rights", actualValue);
			}
		}

		// Existing PKs
		Guid exAustraliaPk;
		Guid exVaticanPk;
		Guid internationalWatersPk;
		Guid exAuDollarPk;
		Guid exCapeVerdeEscudoPk;
		Guid imfSpecialCurrencyPk;

		// Existing NKs
		string exCapeVerdeEscudoNk;
		string imfSpecialCurrencyNk;

		// New random PKs
		Guid newVaticanPk;
		Guid inexistingCodeCountryPk;
		Guid inexistingCodeSystemCurrencyPk;
		Guid inexistingCodeUserCurrencyPk;
		Guid newCapeVerdeEscudoPk;
		Guid newNonSystemCountryPk;
	}
}
