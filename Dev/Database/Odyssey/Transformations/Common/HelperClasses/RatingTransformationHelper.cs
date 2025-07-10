#if DEBUG

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.DbUpgrader.Shared.RowWrapperRepositoryExtensions;

namespace Enterprise.DbUpgrader.Transformation.Common
{
	public static class RatingTransformationHelper
	{
		public static Guid CreateRateEntryAndRateLine(RowWrapperRepository repository, Entity ratingHeader, string rateEntryCategory, string rateEntryMode, string origin, string destination, string rateEntryCurrency, Guid testChargeCodePK, string calculator, string rateLineCurrency, Guid? publisherPK)
		{
			var rateEntry = ratingHeader.CreateRateEntry(tI_RateCategory: rateEntryCategory, tI_Mode: rateEntryMode, origin: origin, destination: destination, tI_RX_NKCurrency: rateEntryCurrency, publisherPK: publisherPK);
			var rateLine = repository.CreateRateLine(rateEntry.Row.PK, testChargeCodePK, tL_RateCalculator: calculator, tL_RX_NKCurrency: rateLineCurrency);
			return rateLine.Row.PK;
		}

		public static Guid CreateRateHeader(string type)
		{
			Guid pk = Guid.NewGuid();

			const string sql = @"
				insert into dbo.RatingHeader(TH_PK, TH_RateType, TH_SystemCreateTimeUtc, TH_SystemCreateUser, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser)
				values(@pk, @type, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				";

			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameterBasedOnDbColumn("@type", type, RatingHeaderSchema.TH_RateType);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public static Guid CreateRateHeader(string type, DateTime quoteDateTime)
		{
			var pk = Guid.NewGuid();

			const string sql = @"
				insert into dbo.RatingHeader(TH_PK, TH_RateType, TH_QuoteDate, TH_SystemCreateTimeUtc, TH_SystemCreateUser, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser)
				values(@pk, @type, @quoteDate, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				";

			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameterBasedOnDbColumn("@type", type, RatingHeaderSchema.TH_RateType);
				command.AddParameter("@quoteDate", SqlDbType.Date, quoteDateTime);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public static Guid CreateRateHeader(string type, Guid client, Guid? company  = null)
		{
			Guid pk = Guid.NewGuid();

			const string sql = @"
				insert into dbo.RatingHeader(TH_PK, TH_OH, TH_GC, TH_RateType, TH_SystemCreateTimeUtc, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser)
				values(@pk, @client, @company, @type, GetUtcDate(), GetUtcDate(), '~BP')
				";

			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@client", SqlDbType.UniqueIdentifier, client);
				command.AddParameter("@company", SqlDbType.UniqueIdentifier, company ?? (object)DBNull.Value);
				command.AddParameterBasedOnDbColumn("@type", type, RatingHeaderSchema.TH_RateType);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public static Guid CreateRateHeader(string type, Guid companyPK, Guid clientPK, string quoteNumber, DateTime quoteDateTime, bool isOneTimeQuote = true)
		{
			var pk = Guid.NewGuid();

			const string sql = @"
				insert into dbo.RatingHeader(TH_PK, TH_OH, TH_GC, TH_QuoteNumber, TH_RateType, TH_QuoteDate, TH_OneTimeQuote, TH_SystemCreateTimeUtc, TH_SystemCreateUser, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser)
				values(@pk, @clientPK, @companyPK, @quoteNumber, @type, @quoteDate, @isOneTimeQuote, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				";

			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@clientPK", SqlDbType.UniqueIdentifier, clientPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameterBasedOnDbColumn("@quoteNumber", quoteNumber, RatingHeaderSchema.TH_QuoteNumber);
				command.AddParameterBasedOnDbColumn("@type", type, RatingHeaderSchema.TH_RateType);
				command.AddParameter("@quoteDate", SqlDbType.Date, quoteDateTime);
				command.AddParameter("@isOneTimeQuote", SqlDbType.Bit, isOneTimeQuote ? 1 : 0 );
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public static Guid CreateRateEntry(Guid rateHeader, Guid companyPK, string type, string mode, string originLRC, string destinationLRC, string viaLRC)
		{
			Guid pk = Guid.NewGuid();

			const string sql = @"
				insert into dbo.RateEntry(TI_PK, TI_TH, TI_GC_Publisher, TI_RateCategory, TI_Mode, TI_RateStartDate, TI_OriginLRC, TI_DestinationLRC, TI_ViaLRC, TI_SystemCreateTimeUtc, TI_SystemCreateTimeUser, TI_SystemLastEditTimeUtc, TI_SystemLastEditUser)
				values(@pk, @header, @companyPK, @type, @mode, @quoteDate, @originlrc, @destinationlrc, @viaLRC, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				";

			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@header", SqlDbType.UniqueIdentifier, rateHeader);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameterBasedOnDbColumn("@type", type, RateEntrySchema.TI_RateCategory);
				command.AddParameterBasedOnDbColumn("@mode", mode, RateEntrySchema.TI_Mode);
				command.AddParameterBasedOnDbColumn("@originlrc", originLRC, RateEntrySchema.TI_OriginLRC);
				command.AddParameterBasedOnDbColumn("@destinationlrc", destinationLRC, RateEntrySchema.TI_DestinationLRC);
				command.AddParameterBasedOnDbColumn("@viaLRC", viaLRC, RateEntrySchema.TI_ViaLRC);
				command.AddParameter("@quoteDate", SqlDbType.Date, DateTime.Today.AddMonths(-6));
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public static void CreateRateEntry(Guid rateHeaderPk, Guid companyPK, Guid rateEntryPK, string type, string mode, string originLRC, string destinationLRC, string viaLRC)
		{
			const string sql = @"
				insert into dbo.RateEntry(TI_PK, TI_TH, TI_GC_Publisher, TI_RateCategory, TI_Mode, TI_RateStartDate, TI_OriginLRC, TI_DestinationLRC, TI_ViaLRC, TI_SystemCreateTimeUtc, TI_SystemCreateTimeUser, TI_SystemLastEditTimeUtc, TI_SystemLastEditUser)
				values(@pk, @header, @companyPK, @type, @mode, @quoteDate, @originlrc, @destinationlrc, @viaLRC, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				";

			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, rateEntryPK);
				command.AddParameter("@header", SqlDbType.UniqueIdentifier, rateHeaderPk);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameterBasedOnDbColumn("@type", type, RateEntrySchema.TI_RateCategory);
				command.AddParameterBasedOnDbColumn("@mode", mode, RateEntrySchema.TI_Mode);
				command.AddParameterBasedOnDbColumn("@originlrc", originLRC, RateEntrySchema.TI_OriginLRC);
				command.AddParameterBasedOnDbColumn("@destinationlrc", destinationLRC, RateEntrySchema.TI_DestinationLRC);
				command.AddParameterBasedOnDbColumn("@viaLRC", viaLRC, RateEntrySchema.TI_ViaLRC);
				command.AddParameter("@quoteDate", SqlDbType.Date, DateTime.Today.AddMonths(-6));
				command.ExecuteNonQuery();
			}
		}

		public static Guid CreateRateEntry(Guid rateHeader, Guid companyPK, string type, string mode, DateTime startDate, DateTime createdDate)
		{
			var pk = Guid.NewGuid();

			const string sql = @"
				insert into dbo.RateEntry(TI_PK, TI_TH, TI_GC_Publisher, TI_RateCategory, TI_Mode, TI_RateStartDate, TI_RateEndDate, TI_SystemCreateTimeUtc, TI_SystemCreateUser, TI_SystemLastEditTimeUtc, TI_SystemLastEditUser)
				values(@pk, @header, @companyPK, @type, @mode, @startDate, @endDate, @createdDate, '~BP', GetUtcDate(), '~BP')
				";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@header", SqlDbType.UniqueIdentifier, rateHeader);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameterBasedOnDbColumn("@type", type, RateEntrySchema.TI_RateCategory);
				command.AddParameterBasedOnDbColumn("@mode", mode, RateEntrySchema.TI_Mode);

				command.AddParameter("@startDate", SqlDbType.Date, startDate);
				command.AddParameter("@endDate", SqlDbType.Date, DBNull.Value);
				command.AddParameter("@createdDate", SqlDbType.Date, createdDate);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public static Guid CreateRateLine(Guid accountPK, string calculatorCode, Guid rateEntryPK = default(Guid))
		{
			Guid guid = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command("INSERT INTO dbo.RateLines (TL_PK, TL_RateCalculator, TL_AC, TL_TI, TL_SystemCreateTimeUtc, TL_SystemCreateUser, TL_SystemLastEditTimeUtc, TL_SystemLastEditUser) VALUES (@pk, @rateCalc, @accountPK, @rateEntryPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')"))
			{
				command.AddParameter("pk", SqlDbType.UniqueIdentifier, guid);
				command.AddParameterBasedOnDbColumn("rateCalc", calculatorCode, RateLinesSchema.TL_RateCalculator);
				command.AddParameter("accountPK", SqlDbType.UniqueIdentifier, accountPK);
				command.AddParameter("rateEntryPK", SqlDbType.UniqueIdentifier, rateEntryPK);
				command.ExecuteNonQuery();
			}

			return guid;
		}

		public static Guid CreateAccChargeCode(string chargeCode, string chargeCodeGroup, Guid? company)
		{
			var accGuid = Guid.NewGuid();

			var insertSql = "INSERT INTO dbo.AccChargeCode (AC_PK, AC_Code, AC_ChargeGroup, AC_GC, AC_SystemCreateTimeUtc, AC_SystemCreateUser, AC_SystemLastEditTimeUtc, AC_SystemLastEditUser) VALUES (@pk, @chargeCode, @chargeCodeGroup, @company, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var command = Db.Connection.Command(insertSql))
			{
				command.AddParameter("pk", SqlDbType.UniqueIdentifier, accGuid);
				command.AddParameter("chargeCode", SqlDbType.VarChar, chargeCode);
				command.AddParameter("chargeCodeGroup", SqlDbType.VarChar, chargeCodeGroup);
				command.AddParameter("company", SqlDbType.UniqueIdentifier, company ?? (object)DBNull.Value);
				command.ExecuteNonQuery();
			}

			return accGuid;
		}

		public static Guid CreateRateLineItem(Guid rateLinePK, decimal flatAmount = 0m, string tm_type = "", string tm_text = "")
		{
			Guid guid = Guid.NewGuid();

			string insertSql = string.Format("INSERT INTO dbo.RateLineItems (TM_PK, TM_FlatAmount, TM_TL, TM_TYPE, TM_TEXT, TM_SystemCreateTimeUtc, TM_SystemCreateTimeUser, TM_SystemLastEditTimeUtc, TM_SystemLastEditUser) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", guid, flatAmount, rateLinePK, tm_type, tm_text);
			using (DbCommand command = Db.Connection.Command(insertSql))
			{
				command.ExecuteNonQuery();
			}

			return guid;
		}

		public static Guid CreateOrgHeader(string code, string name)
		{
			var pk = Guid.NewGuid();
			var query = "INSERT INTO dbo.OrgHeader(OH_PK, OH_Code, OH_FullName, OH_SystemCreateTimeUtc, OH_SystemCreateUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser) VALUES (@pk, @code, @name, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var command = Db.Connection.Command(query))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameterBasedOnDbColumn("@code", code, OrgHeaderSchema.OH_Code);
				command.AddParameterBasedOnDbColumn("@name", name, OrgHeaderSchema.OH_FullName);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public static Guid CreateOrgAddress(Guid orgPK, string adressLine1)
		{
			var pk = Guid.NewGuid();
			var query = "INSERT INTO dbo.OrgAddress(OA_PK, OA_OH, OA_Address1, OH_SystemCreateTimeUtc, OH_SystemCreateTimeUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser) VALUES (@pk, @orgPK, @address1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var command = Db.Connection.Command(query))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameterBasedOnDbColumn("@orgPK", orgPK, OrgAddressSchema.OA_OH);
				command.AddParameterBasedOnDbColumn("@address1", adressLine1, OrgAddressSchema.OA_Address1);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public static void DisableConstraint(string tableName, string constraintName)
		{
			var query = string.Format(@"
										IF (OBJECT_ID('{1}', 'C') IS NOT NULL)
										BEGIN
											ALTER TABLE {0} NOCHECK CONSTRAINT {1}
										END", tableName, constraintName);

			using (var command = Db.Connection.Command(query))
			{
				command.ExecuteNonQuery();
			}
		}

		public static void EnableConstraint(string tableName, string constraintName)
		{
			var query = string.Format(@"
										IF (OBJECT_ID('{1}', 'C') IS NOT NULL)
										BEGIN
											ALTER TABLE {0} CHECK CONSTRAINT {1}
										END", tableName, constraintName);

			using (var command = Db.Connection.Command(query))
			{
				command.ExecuteNonQuery();
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Not required for parameters in this query")]
		public static void DisableTrigger(string tableName, string triggerName)
		{
			var sqlDisableTrigger = $@"
	IF EXISTS (SELECT null FROM sys.triggers WHERE Name = '{triggerName}' AND Object_Name(parent_id) = '{tableName}')
		ALTER TABLE {tableName} DISABLE TRIGGER {triggerName}
";

			Db.Connection.ExecuteNonQuery(sqlDisableTrigger);
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Not required for parameters in this query")]
		public static void EnableTrigger(string tableName, string triggerName)
		{
			var sqlEnableTrigger = $@"
	IF EXISTS (SELECT null FROM sys.triggers WHERE Name = '{triggerName}' AND Object_Name(parent_id) = '{tableName}')
		ALTER TABLE {tableName} ENABLE TRIGGER {triggerName}
";

			Db.Connection.ExecuteNonQuery(sqlEnableTrigger);
		}

		public static IEnumerable<T> LoadValues<T>(string tableName, string columnName)
		{
			var query = string.Format("SELECT {0} FROM {1}", columnName, tableName);
			var result = new List<T>();

			using (var reader = Db.Connection.Command(query).ExecuteReader())
			{
				while (reader.Read())
				{
					result.Add((T)reader[columnName]);
				}
			}

			return result;
		}
	}
}

#endif
