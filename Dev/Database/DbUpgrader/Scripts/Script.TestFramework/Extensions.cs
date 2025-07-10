using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Build.Database.Script.TestFramework
{
	public static class Extensions
	{
		public static string ToSqlFormat(this DateTime dateTime) => dateTime.ToString("F", sqlServerDateTimeFormatInfo);

		public static char ToSqlFormat(this bool boolean) => boolean ? '1' : '0';

		public static Dictionary<string, object> CreatePortAndDepotSelection(this Guid guid, DbConnection connection, string serviceLevel, string direction, string ratingFreightMode, string packType, Guid dispatchDepotAddress)
		{
			var pk = Guid.NewGuid();
			var res = new Dictionary<string, object>();
			res["connection"] = connection;
			res["portDepotSelection"] = pk;

			var query = string.Format(
				"INSERT INTO dbo.PortHubSelection (TY_PK, TY_Direction, TY_RS_NKServiceLevel, TY_RatingFreightMode, TY_OA_DepotAddress, TY_F3_NKPackType, TY_OA_DispatchDepotAddress) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', {6})",
				pk,
				direction,
				serviceLevel,
				ratingFreightMode,
				guid,
				packType,
				dispatchDepotAddress == Guid.Empty ? "NULL" : string.Format("'{0}'", dispatchDepotAddress.ToString()));
			connection.ExecuteNonQuery(query);

			return res;
		}

		public static Dictionary<string, object> AddZone(this Dictionary<string, object> args, string zoneName, Guid carrier, string serviceLevel, string countryCode = "UA", string accountNumber = "")
		{
			var connection = (DbConnection)args["connection"];
			var portDepotSelection = (Guid)args["portDepotSelection"];
			var transportProviderPK = Guid.NewGuid();
			var zonePK = Guid.NewGuid();

			var query = string.Format(
				"INSERT INTO dbo.RateTransportProvider (TP_PK, TP_RN_NKCountry, TP_OH_RelatedParty, TP_SystemCreateTimeUtc, TP_SystemCreateUser, TP_SystemLastEditTimeUtc, TP_SystemLastEditUser) VALUES ('{0}','{1}','{2}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')",
				transportProviderPK,
				countryCode,
				carrier);
			connection.ExecuteNonQuery(query);

			query = string.Format("INSERT INTO dbo.RateTransportZones (TZ_PK, TZ_ZoneName, TZ_TP, TZ_SystemCreateTimeUtc, TZ_SystemCreateUser, TZ_SystemLastEditTimeUtc, TZ_SystemLastEditUser) VALUES ('{0}', '{1}', '{2}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", zonePK, zoneName, transportProviderPK);
			connection.ExecuteNonQuery(query);

			query = string.Format(
				"INSERT INTO dbo.PortHubZonePivot (TX_PK, TX_TY_Hub, TX_TZ_Zone, TX_PL_NKCarrierServiceLevel, TX_CarrierAccountNumber) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}')",
				Guid.NewGuid(),
				portDepotSelection,
				zonePK,
				serviceLevel,
				accountNumber);
			connection.ExecuteNonQuery(query);

			args["zone"] = zonePK;
			return args;
		}

		public static Dictionary<string, object> AddZoneItem(this Dictionary<string, object> args, string city, string state, string country)
		{
			var connection = (DbConnection)args["connection"];
			var zonePK = (Guid)args["zone"];

			var cityTownPK = Guid.NewGuid();
			var query = string.Format(
				"INSERT INTO dbo.RefCityTown (R9_PK, R9_InternationalName, R9_RW_NKState, R9_RN_NKCountry) VALUES ('{0}', '{1}', '{2}', '{3}')",
				cityTownPK,
				city,
				state,
				country);
			connection.ExecuteNonQuery(query);

			query = string.Format(
				"INSERT INTO dbo.RateTransportZoneItem (TQ_PK, TQ_TZ_DomesticZone, TQ_R9_CityTown, TQ_RN_NKCountry, TQ_SystemCreateTimeUtc, TQ_SystemCreateUser, TQ_SystemLastEditTimeUtc, TQ_SystemLastEditUser) VALUES ('{0}', '{1}', '{2}', '{3}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')",
				Guid.NewGuid(),
				zonePK,
				cityTownPK,
				country);
			connection.ExecuteNonQuery(query);

			return args;
		}

		public static Dictionary<string, object> AddZoneItem(this Dictionary<string, object> args, string countryCode, int fromPostCode, int endPostCode)
		{
			var connection = (DbConnection)args["connection"];
			var zonePK = (Guid)args["zone"];
			var fromPostCodePK = Guid.NewGuid();
			var toPostCodePK = Guid.NewGuid();

			var query = string.Format(
				"INSERT INTO dbo.RefPostCode (RK_PK, RK_CityTownPostCode, RK_RN_NKCountry, RK_Lattitude, RK_Longitude) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}')",
				fromPostCodePK,
				fromPostCode,
				countryCode,
				100,
				100);
			connection.ExecuteNonQuery(query);

			query = string.Format(
				"INSERT INTO dbo.RefPostCode (RK_PK, RK_CityTownPostCode, RK_RN_NKCountry, RK_Lattitude, RK_Longitude) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}')",
				toPostCodePK,
				endPostCode,
				countryCode,
				100,
				100);
			connection.ExecuteNonQuery(query);

			query = string.Format(
				"INSERT INTO dbo.RateTransportZoneItem (TQ_PK, TQ_TZ_DomesticZone, TQ_FromPostCode, TQ_ToPostCode, TQ_RN_NKCountry, TQ_SystemCreateTimeUtc, TQ_SystemCreateUser, TQ_SystemLastEditTimeUtc, TQ_SystemLastEditUser) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')",
				Guid.NewGuid(),
				zonePK,
				fromPostCode,
				endPostCode,
				countryCode);
			connection.ExecuteNonQuery(query);

			return args;
		}

		public static string CharactersString(this int length)
		{
			return new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", length).Select(s => s[Random.Value.Next(s.Length)]).ToArray());
		}

		[ThreadSafe]
		static readonly ThreadLocal<Random> Random = new ThreadLocal<Random>(() => new Random());

		[ThreadSafe]
		static readonly IFormatProvider sqlServerDateTimeFormatInfo = new DateTimeFormatInfo()
		{
			FullDateTimePattern = "yyyy-MM-dd HH:mm:ss.fff",
			TimeSeparator = ":",
			DateSeparator = "-",
		};
	}
}
