using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Encryption;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	sealed class UtilitiesTest : TransactionedTestCase
	{
		public void TestGetDatabaseStatus()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				var status = Utilities.GetDatabaseStatus(adminConnection, Db.DatabaseName);
				AssertEquals("DB status should be 'ONLINE'", DatabaseStatus.Online, status);
				// Ideally we would test other tests here but the current test framework does not allow us to set arbitrary DB states
			}
		}

		[UseSnapshotProtection]
		public void TestGetAuditServer_BlankData()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				var result = Utilities.GetAuditServer(adminConnection, Db.DatabaseName);
				AssertEquals(null, result);
			}
		}

		[UseSnapshotProtection]
		public void TestGetDataWarehouseServer_BlankData()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				var result = Utilities.GetDataWarehouseServer(adminConnection, Db.DatabaseName);
				AssertEquals(null, result);
			}
		}

		[UseSnapshotProtection]
		public void TestGetAuditServer_ValidData()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				var insertWarehouseData =
					@"
					DELETE FROM dbo.StmData WHERE SD_Name = 'BiAuditServer'
					INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_BinaryValue)
					VALUES (newid(), 'BiAuditServer', @binaryVal)";

				using (var cmd = adminConnection.Command(insertWarehouseData))
				{
					cmd.AddParameter("@binaryVal", SqlDbType.Binary, System.Text.Encoding.Unicode.GetBytes(Db.ServerName));
					cmd.ExecuteNonQuery();

					var result = Utilities.GetAuditServer(adminConnection, Db.DatabaseName);
					AssertEquals("Audit server", Db.ServerName, result);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestGetDataWarehouseServer_ValidData()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				var insertWarehouseData =
					@"
					DELETE FROM dbo.StmData WHERE SD_Name = 'BiDataWarehouseServer'
					INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_BinaryValue)
					VALUES (newid(), 'BiDataWarehouseServer', @binaryVal)";

				using (var cmd = adminConnection.Command(insertWarehouseData))
				{
					cmd.AddParameter("@binaryVal", SqlDbType.Binary, System.Text.Encoding.Unicode.GetBytes(Db.ServerName));
					cmd.ExecuteNonQuery();

					var result = Utilities.GetDataWarehouseServer(adminConnection, Db.DatabaseName);
					AssertEquals("Data warehouse server", Db.ServerName, result);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestGetDataWarehouseServer_InvalidSynonym()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				var dbName = "DbTestGetDataWarehouseServerInvalidSynonym";

				try
				{
					AdoTestUtils.CreateDbDropExisting(adminConnection, dbName);

					using (((ICurrentDbControl)adminConnection).UseDatabase(dbName))
					{
						adminConnection.ExecuteNonQuery(@"CREATE SYNONYM StmData FOR [SomeTable]");

						var result = Utilities.GetDataWarehouseServer(adminConnection, dbName);
						Assert("Data warehouse server should be null.", result == null);
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(adminConnection, dbName);
				}
			}
		}

		public void TestDatabaseExists()
		{
			Assert("Database should exist", Utilities.DatabaseExists(Db.Connection, Db.DatabaseName));
			Assert("Database shouldn't exist", !Utilities.DatabaseExists(Db.Connection, "notExistsDatabase4687as78f7899f9fffqwuut"));
		}

		public void TestIsProductionDb()
		{
			var isProduction = Utilities.IsProductionDb(Db.Connection, Db.DatabaseName);
			AssertEquals("Database Type (initial)", false, isProduction);

			var sqlText = "INSERT dbo.StmData (SD_PK, SD_Name, SD_BinaryValue) VALUES (NEWID(), 'FreightNotesHeaderLength', convert(varbinary(max), N'GAPLcP4y8YaMhFHYeaeNGKCfLexsSSDCx2Ylb3nUg4rc0NQbGAIJGcufyV1XoKPWW0F0Vh5qhmKsbOTxJq4cqZz9jGdkVUFUX2nLLVhvCqgwOOV8/g2MNUjgCRrmXl4AJasEcWKOEuN2/17gtipST/hvRYPIB/WNV/5CgLeBiL2kGZKSLbZjyRQK7ZLdCPVypFxNI1QI5o1GHrSik5fLeUJbTfWboTMZ/P0yUcAe3cRIEU5ofaYkAL6FLbNMViEkD5eUPED7Yr63/SIhXK6fwNYjKLaWUVR/Sy1wvtDFXyqnQSnsTUXIXVwJ55kQnL8e+HJH1IBm1ljgYTSIu1rVP3S+kKJr9AyLZRoahyVmEBfzXJw+jLO/DtzvYVh9Ah/Rhrx2T3TI1lV1fJC3uTQAmMQnIYIzEX78GmpkuNR+t4rpbdj2NLFzGGuUauDmef1C05jezo4fxtTB6cxPRiaIM+iUgvrN/2V1COJt31yZiQky3fKc6Jyd9HhKAmzeb07BIz5NDBoIreUAM8HCmqIdl6hlF8iBEU8wNZrkWeS19tKMWQNNJKsDk6uBsr/a9GoA1u5+mWs0mvP3mx0PaNEg6+ngdXxmRaL5Oh3Wxc5QV/rmc7RuXwlnn5knW/FI7OEqwwyrQhzbF1DJcU6Be+JGR7TSD0VALkidUzNtJBgpqThYDwd6S8lGJp/ms4RktNclzkLYMS6krbgU0zO8g+RvaOXF6IUj8KhIIhPEBJtYj8+FIA5umd6dzBmI7dN3fD14XTx+w4ef7MdMhqJoTXsYFpr8u9erEAaghxfy2Ou30gWaiCONrqohgKhQ7/6vGzk+0nSp5i4HFkhc1hEczAuZngdAGB1vlZxgVw8P/2gMhSUxtjtHROoM3ZCNF/HOogU+NCPBnXJ56rQrOrSJTUhi68yL8zDgTQtDVRk+Z0mH2F6hc+5Ts3XUuAJtcQzGC1Kf+prdTgraYkKzxFsvzlSf/UQX8PeaLi8uIsM3yDbhTBe07On4Uo96jQNL5jrvJGsB'))";
			Db.Connection.ExecuteNonQuery(sqlText);

			isProduction = Utilities.IsProductionDb(Db.Connection, Db.DatabaseName);
			AssertEquals("Database Type (with PRD licence but no non-system staff)", false, isProduction);

			var personPk = Guid.NewGuid();
			Db.Connection.ExecuteNonQuery($"insert dbo.GlbPerson (PER_PK, PER_FullName) values ('{personPk}', 'aaa')");

			sqlText = $"INSERT INTO dbo.GlbStaff(GS_PK, GS_Code, GS_IsOperational, GS_IsController, GS_IsActive, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES(NEWID(), 'NEW', 1, 0, 1, '{personPk}', GETUTCDATE(), 'E', GETUTCDATE(), 'E')";
			Db.Connection.ExecuteNonQuery(sqlText);

			isProduction = Utilities.IsProductionDb(Db.Connection, Db.DatabaseName);
			AssertEquals("Database Type (with PRD licence and a non-system staff)", true, isProduction);

			sqlText = @"
				UPDATE dbo.StmData SET SD_BinaryValue = convert(varbinary(max), N'GAPLcP4y8YaMhFHYeaeNGKCfLexsSSDCx2Ylb3nUg4rc0NQbGAIJGcufyV1XoKPWW0F0Vh5qhmKsbOTxJq4cqZz9jGdkVUFUX2nLLVhvCqgwOOV8/g2MNUjgCRrmXl4AJasEcWKOEuN2/17gtipST/hvRYPIB/WNV/5CgLeBiL2kGZKSLbZjyRQK7ZLdCPVypFxNI1QI5o1GHrSik5fLeUJbTfWboTMZ/P0yUcAe3cRIEU5ofaYkAL6FLbNMViEkD5eUPED7Yr63/SIhXK6fwNYjKLaWUVR/Sy1wvtDFXyqnQSnsTUXIXVwJ55kQnL8e+HJH1IBm1ljgYTSIu1rVP3S+kKJr9AyLZRoahyVmEBfzXJw+jLO/DtzvYVh9Ah/Rhrx2T3TI1lV1fJC3uTQAmMQnIYIzEX78GmpkuNR+t4rpbdj2NLFzGGuUauDmef1C05jezo4fxtTB6cxPRiaIM+iUgvrN/2V1COJt31yZiQky3fKc6Jyd9HhKAmzeb07BIz5NDBoIreUAM8HCmqIdl6hlF8iBEU8wNZrkWeS19tKMWQNNJKsDk6uBsr/a9GoA1u5+mWs0mvP3mx0PaNEg6+ngdXxmRaL5Oh3Wxc5QV/rmc7RuXwlnn5knW/FI7OEqwwyrQhzbF1DJcU6Be+JGR7TSD0VALkidUzNtJBgpqTg6SeEtWt9qkK7xIeliXLHOkJF9Opcr9jdpm1wsXIxeXyoAoaEMRkzUkw5xgLfvJtQo9qeNrr2Sm4XwIYp38K5fSmTFVcc2fvLtOTvchB7QOPS5RT7BnwNu4KZjsPscVhxNA5vQkG4b5Jm6ay9oY5xbdSj6DoNFAitP/1QsQLT0ZoYJCCbI5fSyrblo8me0eRjEz8RJfQ4YdZSmdQrxOoH+v72s0MaUt25mdQ764umZ/zv+sryj2X8MiQvnuJ/VXjeBmNeJBqlHbtp/CYcusVSEYVe0DD2wul63Rorwhng3JxkNqeuSdGyaN0w3RHLjultvB9KUshpUXdWE8w08FGak')
				WHERE SD_Name = 'FreightNotesHeaderLength'";
			Db.Connection.ExecuteNonQuery(sqlText);

			isProduction = Utilities.IsProductionDb(Db.Connection, Db.DatabaseName);
			AssertEquals("Database Type (with DEM licence and a non-system staff)", false, isProduction);
		}

		public void TestIsProductionDb_InvalidDb()
		{
			var dbName = "TestSampleProductionDb";
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					DropDatabaseIfExists(connection, dbName);

					var sqlText = $"CREATE DATABASE [{dbName}]";
					connection.ExecuteNonQuery(sqlText);

					using (((ICurrentDbControl)connection).UseDatabase(dbName))
					{
						sqlText = $"CREATE SCHEMA [TestSchema];";
						connection.ExecuteNonQuery(sqlText);

						sqlText = $"CREATE TABLE [TestSchema].[GlbStaff] (GS_PK uniqueidentifier);";
						connection.ExecuteNonQuery(sqlText);

						var isProduction = Utilities.IsProductionDb(connection, dbName);
						AssertEquals("IsProduction value?", false, isProduction);
					}
				}
				finally
				{
					DropDatabaseIfExists(connection, dbName);
				}
			}
		}

		void DropDatabaseIfExists(DbConnection connection, string dbName)
		{
			var sqlText = $"IF EXISTS (SELECT NULL FROM sys.databases WHERE name = '{dbName}') DROP DATABASE [{dbName}]";
			connection.ExecuteNonQuery(sqlText);
		}

		public void TestGetDbType()
		{
			var dbType = Utilities.GetDbType(Db.Connection, Db.DatabaseName);
			AssertEquals("Database Type (initial)", "", dbType);

			var sqlText = "INSERT dbo.StmData (SD_PK, SD_Name, SD_BinaryValue) VALUES (NEWID(), 'FreightNotesHeaderLength', convert(varbinary(max), N'GAPLcP4y8YaMhFHYeaeNGKCfLexsSSDCx2Ylb3nUg4rc0NQbGAIJGcufyV1XoKPWW0F0Vh5qhmKsbOTxJq4cqZz9jGdkVUFUX2nLLVhvCqgwOOV8/g2MNUjgCRrmXl4AJasEcWKOEuN2/17gtipST/hvRYPIB/WNV/5CgLeBiL2kGZKSLbZjyRQK7ZLdCPVypFxNI1QI5o1GHrSik5fLeUJbTfWboTMZ/P0yUcAe3cRIEU5ofaYkAL6FLbNMViEkD5eUPED7Yr63/SIhXK6fwNYjKLaWUVR/Sy1wvtDFXyqnQSnsTUXIXVwJ55kQnL8e+HJH1IBm1ljgYTSIu1rVP3S+kKJr9AyLZRoahyVmEBfzXJw+jLO/DtzvYVh9Ah/Rhrx2T3TI1lV1fJC3uTQAmMQnIYIzEX78GmpkuNR+t4rpbdj2NLFzGGuUauDmef1C05jezo4fxtTB6cxPRiaIM+iUgvrN/2V1COJt31yZiQky3fKc6Jyd9HhKAmzeb07BIz5NDBoIreUAM8HCmqIdl6hlF8iBEU8wNZrkWeS19tKMWQNNJKsDk6uBsr/a9GoA1u5+mWs0mvP3mx0PaNEg6+ngdXxmRaL5Oh3Wxc5QV/rmc7RuXwlnn5knW/FI7OEqwwyrQhzbF1DJcU6Be+JGR7TSD0VALkidUzNtJBgpqThYDwd6S8lGJp/ms4RktNclzkLYMS6krbgU0zO8g+RvaOXF6IUj8KhIIhPEBJtYj8+FIA5umd6dzBmI7dN3fD14XTx+w4ef7MdMhqJoTXsYFpr8u9erEAaghxfy2Ou30gWaiCONrqohgKhQ7/6vGzk+0nSp5i4HFkhc1hEczAuZngdAGB1vlZxgVw8P/2gMhSUxtjtHROoM3ZCNF/HOogU+NCPBnXJ56rQrOrSJTUhi68yL8zDgTQtDVRk+Z0mH2F6hc+5Ts3XUuAJtcQzGC1Kf+prdTgraYkKzxFsvzlSf/UQX8PeaLi8uIsM3yDbhTBe07On4Uo96jQNL5jrvJGsB'))";
			Db.Connection.ExecuteNonQuery(sqlText);

			dbType = Utilities.GetDbType(Db.Connection, Db.DatabaseName);
			AssertEquals("Database Type: production", DatabaseTypes.Codes.Production, dbType);

			sqlText = @"
				UPDATE dbo.StmData SET SD_BinaryValue = convert(varbinary(max), N'GAPLcP4y8YaMhFHYeaeNGKCfLexsSSDCx2Ylb3nUg4rc0NQbGAIJGcufyV1XoKPWW0F0Vh5qhmKsbOTxJq4cqZz9jGdkVUFUX2nLLVhvCqgwOOV8/g2MNUjgCRrmXl4AJasEcWKOEuN2/17gtipST/hvRYPIB/WNV/5CgLeBiL2kGZKSLbZjyRQK7ZLdCPVypFxNI1QI5o1GHrSik5fLeUJbTfWboTMZ/P0yUcAe3cRIEU5ofaYkAL6FLbNMViEkD5eUPED7Yr63/SIhXK6fwNYjKLaWUVR/Sy1wvtDFXyqnQSnsTUXIXVwJ55kQnL8e+HJH1IBm1ljgYTSIu1rVP3S+kKJr9AyLZRoahyVmEBfzXJw+jLO/DtzvYVh9Ah/Rhrx2T3TI1lV1fJC3uTQAmMQnIYIzEX78GmpkuNR+t4rpbdj2NLFzGGuUauDmef1C05jezo4fxtTB6cxPRiaIM+iUgvrN/2V1COJt31yZiQky3fKc6Jyd9HhKAmzeb07BIz5NDBoIreUAM8HCmqIdl6hlF8iBEU8wNZrkWeS19tKMWQNNJKsDk6uBsr/a9GoA1u5+mWs0mvP3mx0PaNEg6+ngdXxmRaL5Oh3Wxc5QV/rmc7RuXwlnn5knW/FI7OEqwwyrQhzbF1DJcU6Be+JGR7TSD0VALkidUzNtJBgpqTg6SeEtWt9qkK7xIeliXLHOkJF9Opcr9jdpm1wsXIxeXyoAoaEMRkzUkw5xgLfvJtQo9qeNrr2Sm4XwIYp38K5fSmTFVcc2fvLtOTvchB7QOPS5RT7BnwNu4KZjsPscVhxNA5vQkG4b5Jm6ay9oY5xbdSj6DoNFAitP/1QsQLT0ZoYJCCbI5fSyrblo8me0eRjEz8RJfQ4YdZSmdQrxOoH+v72s0MaUt25mdQ764umZ/zv+sryj2X8MiQvnuJ/VXjeBmNeJBqlHbtp/CYcusVSEYVe0DD2wul63Rorwhng3JxkNqeuSdGyaN0w3RHLjultvB9KUshpUXdWE8w08FGak')
				WHERE SD_Name = 'FreightNotesHeaderLength'";
			Db.Connection.ExecuteNonQuery(sqlText);

			dbType = Utilities.GetDbType(Db.Connection, Db.DatabaseName);
			AssertEquals("Database Type: Demonstration", DatabaseTypes.Codes.Demo, dbType);

			sqlText = @"
				UPDATE dbo.StmData SET SD_BinaryValue = convert(varbinary(max), N'GAPLcP4y8YaMhFHYeaeNGKCfLexsSSDCx2Ylb3nUg4rc0NQbGAIJGcufyV1XoKPWW0F0Vh5qhmKsbOTxJq4cqZz9jGdkVUFUX2nLLVhvCqgwOOV8/g2MNUjgCRrmXl4AJasEcWKOEuN2/17gtipST/hvRYPIB/WNV/5CgLeBiL2kGZKSLbZjyRQK7ZLdCPVypFxNI1QI5o1GHrSik5fLeT3k8xv1/l4tggC2nwfjoqXvxMWy+k10hxuAwcnZ+LrVfcuASNwB+y5FAudrxQI2tnXkY6WJZtaG3qJMAx8eSA0X7ApDK5A+EfW/jTNJg3loDnJw1BxRIilTkITk69+4f39RlBy5+HdCGiCPfEt4f+o++3oHL5U70a12idAnEXq7jtAjmqc3KEjfvY0Vjj1hwWuoUOrHdC1d22Xv2SgWhnE+uyvZiEz3eN0uTV4omOaYx99C5AiBMN9J/OaJPupZD0rohHSY47QMZcXRx0Op4KQtWg3nMF2BMhfhUOiJ42kdd/RFFUP9i/H7NQl1YRCXwaUcrdtbbSU3M+2eHJGfp31ZqeTnMJPkwp8YNBu2jbQpQENhIcnqPPPKyATJ/3uNt0p4mR425ds6/GX4SqQ+onI9jyTtsRv/r/v/dVZWGQMyTGjupx0Sdd+Xd2S8oQ6EnXRQRmi1Scn+q/NsvwG7sluX+K9zgCO435AnLF99fitdPuRpLciVAd4+eYhc+vaYXybmp32yjF/mZuuIirqz80B23KJuPVHqxupQ7ysinKni6CkwFQ1MIcj0Lx8/MOfPvGebX/+MKHFUUIf81hGBldDpI053zja1wbjZ7GZVGXGSRaAwMeNnXRPBlZE88b/o8pJX895Cixd5INJw7lO/IXu2duNl3HPdeIU62kClAFcKJdgbstEEjboRwVBFo8secaH4agsG0+ritpb48yoK9SU7F27Hmy3IJfwiiXIG/t3hA35/v6Cu5Qyd+m31G9GnsMl1XhFkLgrLZ/yCv2PkFrU7nusuJXMF5Rk/f5LnafaQ')
				WHERE SD_Name = 'FreightNotesHeaderLength'";
			Db.Connection.ExecuteNonQuery(sqlText);

			dbType = Utilities.GetDbType(Db.Connection, Db.DatabaseName);
			AssertEquals("Database Type: Test", DatabaseTypes.Codes.Test, dbType);

			var regoXml = @"<?xml version=""1.0"" encoding=""utf-16""?>
	<RegistrationKey xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
	  <DbType>PRD</DbType>
	</RegistrationKey>";

			var sqlRegoText = "INSERT dbo.StmData (SD_PK, SD_Name, SD_BinaryValue) VALUES (NEWID(), 'FreightNotesRegistration', convert(varbinary(max), @Rego))";
			var cmd = Db.Connection.Command(sqlRegoText);
			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			cmd.AddParameter("@Rego", SqlDbType.NVarChar, 10000, encoder.Encrypt(regoXml));
			cmd.ExecuteNonQuery();
			dbType = Utilities.GetDbType(Db.Connection, Db.DatabaseName);
			AssertEquals("Database Type", DatabaseTypes.Codes.Production, dbType);
		}

		public void TestDetermineFileValidity()
		{
			var filePath = @"D:\TestDir\Test";
			AssertEquals("[" + filePath + "] is a standard file name...", FileValidity.NotValid, Utilities.DetermineFileValidity(filePath));

			filePath = @"D:\TestDir\EdiBkp_T-20060331-115800_S-DBSERVER-DBINSTANCE_D-Odyssey.bak";
			AssertEquals("[" + filePath + "] is a standard file name...", FileValidity.ValidStandard, Utilities.DetermineFileValidity(filePath));

			filePath = @"D:\TestDir\CW1Bkp_T-20060331-115800_S-DBSERVER-DBINSTANCE_D-Odyssey.bak";
			AssertEquals("[" + filePath + "] is a standard file name...", FileValidity.ValidStandard, Utilities.DetermineFileValidity(filePath));

			filePath = @"D:\TestDir\Odyssey.bak";
			AssertEquals("[" + filePath + "] is an FBK file name...", FileValidity.ValidFBK, Utilities.DetermineFileValidity(filePath));

			filePath = @"D:\TestDir\Odyssey.dbk";
			AssertEquals("[" + filePath + "] is a DBK file name...", FileValidity.ValidDBK, Utilities.DetermineFileValidity(filePath));

			filePath = @"D:\TestDir\EdiBkp_T-20060331-115800_S-DB-SERVER-DBINSTANCE_D-Odyssey_Test.bak";
			AssertEquals("[" + filePath + "] is a standard file name...", FileValidity.ValidStandard, Utilities.DetermineFileValidity(filePath));

			filePath = @"D:\TestDir\EdiBkp_T-20060331-115900_S-DBSERVER_D-Odyssey_SD001.bak";
			AssertEquals("[" + filePath + "] is a standard dependent database (eDocs) file name...", FileValidity.ValidDepStandard, Utilities.DetermineFileValidity(filePath));

			filePath = @"D:\TestDir\EdiBkp_T-20060331-120100_S-DBSERVER-EDI_D-Odyssey_RefDb_Ent_AU.bak";
			AssertEquals("[" + filePath + "] is a standard dependent database (RefDb) file name...", FileValidity.ValidDepStandard, Utilities.DetermineFileValidity(filePath));

			filePath = @"D:\TestDir\CW1Bkp_T-20060331-115800_S-DBSERVER-EDI_D-Odyssey_RefDb_Ent_CA.bak";
			AssertEquals("[" + filePath + "] is a standard file name...", FileValidity.ValidDepStandard, Utilities.DetermineFileValidity(filePath));

			filePath = @"D:\TestDir\CW1Bkp_T-20060331-115800_S-DBSERVER-DBINSTANCE_D-CW-RefDb-ALB-AL-000000.bak";
			AssertEquals("[" + filePath + "] is a standard file name...", FileValidity.ValidStandard, Utilities.DetermineFileValidity(filePath));

			filePath = @"D:\TestDir\CW1Bkp_T-20060331-115800_S-DBSERVER-DBINSTANCE_D-CW-AG-RefDb-AonAg-Name-Here-alb-sk-585858.bak";
			AssertEquals("[" + filePath + "] is a standard file name...", FileValidity.ValidStandard, Utilities.DetermineFileValidity(filePath));

			filePath = @"D:\TestDir\Odyssey_SD001.bak";
			AssertEquals("[" + filePath + "] is an FBK dependent database file name...", FileValidity.ValidDepFBK, Utilities.DetermineFileValidity(filePath));

			filePath = @"D:\TestDir\Odyssey_RefDb_Trf_AU.dbk";
			AssertEquals("[" + filePath + "] is a DBK dependent database file name...", FileValidity.ValidDepDBK, Utilities.DetermineFileValidity(filePath));

			filePath = @"D:\TestDir\Odyssey_something_weird.weirdext";
			AssertEquals("[" + filePath + "] is not a standard name...", FileValidity.NotValid, Utilities.DetermineFileValidity(filePath));
		}

		public void TestUpdateRegistry()
		{
			var registryName = "RegistryName";
			var registryValue = "RegistryValue";

			Utilities.UpdateRegistry(TestConnection, Db.DatabaseName, registryName, registryValue, true);
			var query = @"SELECT SD_BinaryValue, SD_PreserveTestValue FROM dbo.StmData WHERE SD_Name = @Name";
			using (var cmd = TestConnection.Command(query))
			{
				cmd.AddParameter("@Name", SqlDbType.VarChar, registryName);
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						var binaryValue = reader["SD_BinaryValue"];
						if (binaryValue != DBNull.Value)
						{
							var value = System.Text.Encoding.ASCII.GetString((byte[])binaryValue).Replace("\0", "");
							AssertEquals("Wrong value found for registry", registryValue, value);
						}
						else
						{
							Fail("No value found for registry");
						}

						var preserveTestValue = Convert.ToBoolean(reader["SD_PreserveTestValue"]);
						AssertEquals("Preserve Test Value", true, preserveTestValue);
					}
					else
					{
						Fail("Registry not found.");
					}
				}
			}
		}

		public void TestUpdateRegistry_InvalidColumn()
		{
			var registryName = "RegistryName";
			var registryValue = "RegistryValue";

			new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, "StmData", "SD_BinaryValue").DropRelateObjects(TestConnection);
			var sqlText = @"
				ALTER TABLE dbo.StmData DROP COLUMN SD_BinaryValue";
			TestConnection.ExecuteNonQuery(sqlText);

			AssertNoExceptionThrown(() => Utilities.UpdateRegistry(TestConnection, Db.DatabaseName, registryName, registryValue, true));
		}

		public void TestGetDbServerDateTime()
		{
			// Arrange
			var databaseTime = DateTime.MinValue;
			// Act
			// Assert
			AssertNoExceptionThrown(() => databaseTime = Utilities.GetDbServerDateTime(Db.Connection));
			AssertNotEquals("Database Time should be refreshed.", DateTime.MinValue, databaseTime);
		}
	}
}
