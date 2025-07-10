using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Core
{
	public enum ExchangeRateType
	{
		[Description("BUY")]
		Buy,
		[Description("SEL")]
		Sell,
		[Description("CUD")]
		CustomsMeasureEURExRate,
		[Description("CUS")]
		Customs,
		[Description("CUE")]
		CustomsSecondary,
		[Description("PER")]
		PeriodEnd,
		[Description("GCB")]
		GlobalCreditControl,
		[Description("IAT")]
		IATA,
		[Description("C01")]
		C01,
		[Description("C02")]
		C02,
		[Description("C03")]
		C03,
		[Description("C04")]
		C04,
		[Description("C05")]
		C05,
		[Description("C06")]
		C06,
		[Description("C07")]
		C07,
		[Description("C08")]
		C08,
		[Description("C09")]
		C09,
		[Description("C10")]
		C10,
		[Description("C11")]
		C11,
		[Description("C12")]
		C12,
		[Description("C13")]
		C13,
		[Description("C14")]
		C14,
		[Description("C15")]
		C15,
		[Description("C16")]
		C16,
		[Description("C17")]
		C17,
		[Description("C18")]
		C18,
		[Description("C19")]
		C19,
		[Description("C20")]
		C20,
		[Description("C21")]
		C21,
		[Description("C22")]
		C22,
		[Description("C23")]
		C23,
		[Description("C24")]
		C24,
		[Description("C25")]
		C25,
		[Description("C26")]
		C26,
		[Description("C27")]
		C27,
		[Description("C28")]
		C28,
		[Description("C29")]
		C29,
		[Description("C30")]
		C30,
		[Description("C31")]
		C31,
		[Description("C32")]
		C32,
		[Description("C33")]
		C33,
		[Description("C34")]
		C34,
		[Description("C35")]
		C35,
		[Description("C36")]
		C36,
		[Description("C37")]
		C37,
		[Description("C38")]
		C38,
		[Description("C39")]
		C39,
		[Description("C40")]
		C40,
		[Description("C41")]
		C41,
		[Description("C42")]
		C42,
		[Description("C43")]
		C43,
		[Description("C44")]
		C44,
		[Description("C45")]
		C45,
		[Description("C46")]
		C46,
		[Description("C47")]
		C47,
		[Description("C48")]
		C48,
		[Description("C49")]
		C49,
		[Description("C50")]
		C50,
		[Description("C51")]
		C51,
		[Description("C52")]
		C52,
		[Description("C53")]
		C53,
		[Description("C54")]
		C54,
		[Description("C55")]
		C55,
		[Description("C56")]
		C56,
		[Description("C57")]
		C57,
		[Description("C58")]
		C58,
		[Description("C59")]
		C59,
		[Description("C60")]
		C60,
		[Description("C61")]
		C61,
		[Description("C62")]
		C62,
		[Description("C63")]
		C63,
		[Description("C64")]
		C64,
		[Description("C65")]
		C65,
		[Description("C66")]
		C66,
		[Description("C67")]
		C67,
		[Description("C68")]
		C68,
		[Description("C69")]
		C69,
		[Description("C70")]
		C70,
		[Description("C71")]
		C71,
		[Description("C72")]
		C72,
		[Description("C73")]
		C73,
		[Description("C74")]
		C74,
		[Description("C75")]
		C75,
		[Description("C76")]
		C76,
		[Description("C77")]
		C77,
		[Description("C78")]
		C78,
		[Description("C79")]
		C79,
		[Description("C80")]
		C80,
		[Description("C81")]
		C81,
		[Description("C82")]
		C82,
		[Description("C83")]
		C83,
		[Description("C84")]
		C84,
		[Description("C85")]
		C85,
		[Description("C86")]
		C86,
		[Description("C87")]
		C87,
		[Description("C88")]
		C88,
		[Description("C89")]
		C89,
		[Description("C90")]
		C90,
		[Description("C91")]
		C91,
		[Description("C92")]
		C92,
		[Description("C93")]
		C93,
		[Description("C94")]
		C94,
		[Description("C95")]
		C95,
		[Description("C96")]
		C96,
		[Description("C97")]
		C97,
		[Description("C98")]
		C98,
		[Description("C99")]
		C99,
		[Description("L01")]
		L01,
		[Description("L02")]
		L02,
		[Description("L03")]
		L03,
		[Description("L04")]
		L04,
		[Description("L05")]
		L05,
		[Description("L06")]
		L06,
		[Description("L07")]
		L07,
		[Description("L08")]
		L08,
		[Description("L09")]
		L09,
		[Description("L10")]
		L10,
		[Description("L11")]
		L11,
		[Description("L12")]
		L12,
		[Description("L13")]
		L13,
		[Description("L14")]
		L14,
		[Description("L15")]
		L15,
		[Description("L16")]
		L16,
		[Description("L17")]
		L17,
		[Description("L18")]
		L18,
		[Description("L19")]
		L19,
		[Description("L20")]
		L20,
		[Description("L21")]
		L21,
		[Description("L22")]
		L22,
		[Description("L23")]
		L23,
		[Description("L24")]
		L24,
		[Description("L25")]
		L25,
		[Description("L26")]
		L26,
		[Description("L27")]
		L27,
		[Description("L28")]
		L28,
		[Description("L29")]
		L29,
		[Description("L30")]
		L30,
		[Description("L31")]
		L31,
		[Description("L32")]
		L32,
		[Description("L33")]
		L33,
		[Description("L34")]
		L34,
		[Description("L35")]
		L35,
		[Description("L36")]
		L36,
		[Description("L37")]
		L37,
		[Description("L38")]
		L38,
		[Description("L39")]
		L39,
		[Description("L40")]
		L40,
		[Description("L41")]
		L41,
		[Description("L42")]
		L42,
		[Description("L43")]
		L43,
		[Description("L44")]
		L44,
		[Description("L45")]
		L45,
		[Description("L46")]
		L46,
		[Description("L47")]
		L47,
		[Description("L48")]
		L48,
		[Description("L49")]
		L49,
		[Description("L50")]
		L50,
		[Description("L51")]
		L51,
		[Description("L52")]
		L52,
		[Description("L53")]
		L53,
		[Description("L54")]
		L54,
		[Description("L55")]
		L55,
		[Description("L56")]
		L56,
		[Description("L57")]
		L57,
		[Description("L58")]
		L58,
		[Description("L59")]
		L59,
		[Description("L60")]
		L60,
		[Description("L61")]
		L61,
		[Description("L62")]
		L62,
		[Description("L63")]
		L63,
		[Description("L64")]
		L64,
		[Description("L65")]
		L65,
		[Description("L66")]
		L66,
		[Description("L67")]
		L67,
		[Description("L68")]
		L68,
		[Description("L69")]
		L69,
		[Description("L70")]
		L70,
		[Description("L71")]
		L71,
		[Description("L72")]
		L72,
		[Description("L73")]
		L73,
		[Description("L74")]
		L74,
		[Description("L75")]
		L75,
		[Description("L76")]
		L76,
		[Description("L77")]
		L77,
		[Description("L78")]
		L78,
		[Description("L79")]
		L79,
		[Description("L80")]
		L80,
		[Description("L81")]
		L81,
		[Description("L82")]
		L82,
		[Description("L83")]
		L83,
		[Description("L84")]
		L84,
		[Description("L85")]
		L85,
		[Description("L86")]
		L86,
		[Description("L87")]
		L87,
		[Description("L88")]
		L88,
		[Description("L89")]
		L89,
		[Description("L90")]
		L90,
		[Description("L91")]
		L91,
		[Description("L92")]
		L92,
		[Description("L93")]
		L93,
		[Description("L94")]
		L94,
		[Description("L95")]
		L95,
		[Description("L96")]
		L96,
		[Description("L97")]
		L97,
		[Description("L98")]
		L98,
		[Description("L99")]
		L99,
		[Description("ALL")]
		All
	}

	public class ExchangeRateReader
	{
		public static ExchangeRateReader GetReaderInstance()
		{
			if (reader.Value == null)
			{
				reader.Value = new ExchangeRateReader();
			}

			return reader.Value;
		}

		internal ExchangeRateReader()
		{
		}

		public void ClearCache()
		{
			RateCache.Clear();
		}

		public void ClearCache(Guid companyPK, string rateType, string currencyToGetRateFor, DateTime timeToGet, bool includeExpired)
		{
			string hashKey = GetHashKey(companyPK, rateType, currencyToGetRateFor, timeToGet, includeExpired, null);
			if (RateCache.ContainsKey(hashKey))
			{
				RateCache.Remove(hashKey);
			}
		}

		public void ClearCache(Guid companyPK, string rateType, string currencyToGetRateFor, DateTime timeToGet)
		{
			ClearCache(companyPK, rateType, currencyToGetRateFor, timeToGet, false, null);
		}

		public void ClearCache(Guid companyPK, string rateType, string currencyToGetRateFor, DateTime timeToGet, bool includeExpired, Guid? localClientPK)
		{
			string hashKey = GetHashKey(companyPK, rateType, currencyToGetRateFor, timeToGet, includeExpired, localClientPK);
			if (RateCache.ContainsKey(hashKey))
			{
				RateCache.Remove(hashKey);
			}
		}

		public void ClearCache(Guid companyPK, string rateType, string currencyToGetRateFor, DateTime timeToGet, Guid? localClientPK)
		{
			ClearCache(companyPK, rateType, currencyToGetRateFor, timeToGet, false, localClientPK);
		}

		#region Implementation

		protected static readonly ThreadLocalOverridable<ExchangeRateReader> reader = new ThreadLocalOverridable<ExchangeRateReader>();
		protected readonly Dictionary<string, decimal> RateCache = new Dictionary<string, decimal>();
		protected DateTime LastCachedClearedTime = DateTime.MinValue;

		protected virtual DateTime GetCurrentDateTime()
		{
			return DateTime.Now;    // We need to use this hear, as we want to avoid hitting the database, and only a time difference is concerned here.
		}

		protected void ClearCacheIfRequired()
		{
			TimeSpan span = GetCurrentDateTime().Subtract(LastCachedClearedTime);
			if (span.Minutes >= 5)
			{
				ClearCache();
				LastCachedClearedTime = GetCurrentDateTime();
			}
		}

		protected string GetHashKey(Guid companyPK, string rateType, string currencyToGetRateFor, DateTime timeToGet, bool includeExpired, Guid? localClientPK)
		{
			return companyPK.ToString() + rateType + currencyToGetRateFor + timeToGet.Date + includeExpired.ToString() + (localClientPK == null ? "" : localClientPK.ToString());
		}

		public virtual decimal GetRate(Guid companyPK, string rateType, string currencyToGetRateFor, DateTime timeToGet, bool includeExpired = false, Guid? localClientPK = null)
		{
			decimal result = 0M;

			if (!string.IsNullOrEmpty(currencyToGetRateFor) && currencyToGetRateFor.Length == 3)
			{
				ClearCacheIfRequired();

				if (RateCache.Count > 50)
				{
					RateCache.Clear();
				}

				string hashKey = GetHashKey(companyPK, rateType, currencyToGetRateFor, timeToGet, includeExpired, localClientPK);

				if (!RateCache.TryGetValue(hashKey, out result))
				{
					if (localClientPK != null)
					{
						using (var command = GetCommandForReadingExchangeRate(companyPK, rateType, currencyToGetRateFor, timeToGet, localClientPK))
						{
							using (var reader = command.ExecuteReader())
							{
								if (reader.Read())
								{
									int expiryDateOrdinal = reader.GetOrdinal(RefExchangeRateSchema.Constants.RE_ExpiryDate);

									if (reader.IsDBNull(expiryDateOrdinal) || reader.GetDateTime(expiryDateOrdinal).Date >= timeToGet.Date || includeExpired)
									{
										result = reader.GetDecimal(reader.GetOrdinal(RefExchangeRateSchema.Constants.RE_SellRate));
										RateCache[hashKey] = result;

										return result;
									}
								}
							}
						}
					}

					using (var command = GetCommandForReadingExchangeRate(companyPK, rateType, currencyToGetRateFor, timeToGet))
					{
						if (command != null)
						{
							using (var reader = command.ExecuteReader())
							{
								if (reader.Read())
								{
									int expiryDateOrdinal = reader.GetOrdinal(RefExchangeRateSchema.Constants.RE_ExpiryDate);

									if (reader.IsDBNull(expiryDateOrdinal) || reader.GetDateTime(expiryDateOrdinal).Date >= timeToGet.Date || includeExpired)
									{
										result = reader.GetDecimal(reader.GetOrdinal(RefExchangeRateSchema.Constants.RE_SellRate));
										RateCache[hashKey] = result;
									}
								}
								else
								{
									RateCache[hashKey] = 0M;
								}
							}
						}
					}
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		DbCommand GetCommandForReadingExchangeRate(Guid companyPK, string rateType, string currencyToGetRateFor, DateTime timeToGet, Guid? localClientPK = null)
		{
			DbCommand command;

			if (localClientPK != null && !IsValidRateType(rateType))
			{
				using (PerformanceStatisticsCollector.StartMonitoring("GetCommandForReadingExchangeRate"))
				{
					var selectSQL =
					"SELECT TOP 1 " + RefExchangeRateSchema.Constants.RE_SellRate + ", " + RefExchangeRateSchema.Constants.RE_ExpiryDate
					+ " FROM " + RefExchangeRateSchema.Constants.SqlSchemaName + "." + RefExchangeRateSchema.Constants.TableName
					+ " WHERE " + RefExchangeRateSchema.Constants.RE_RX_NKExCurrency + " = @CurrencyToGetRateFor"
					+ " AND " + RefExchangeRateSchema.Constants.RE_GC + " = @CompanyPK"
					+ " AND " + RefExchangeRateSchema.Constants.RE_OH_Client + " = @LocalClientPK"
					+ " AND " + RefExchangeRateSchema.Constants.RE_StartDate + " < @Date"
					+ " ORDER BY " + RefExchangeRateSchema.Constants.RE_StartDate + " DESC";

					command = Db.Connection.Command(selectSQL);
					command.AddParameter("@LocalClientPK", SqlDbType.UniqueIdentifier, localClientPK);
				}
			}
			else if (localClientPK != null && IsValidRateType(rateType))
			{
				var selectSQL =
					"SELECT TOP 1 " + RefExchangeRateSchema.Constants.RE_SellRate + ", " + RefExchangeRateSchema.Constants.RE_ExpiryDate
					+ " FROM " + RefExchangeRateSchema.Constants.SqlSchemaName + "." + RefExchangeRateSchema.Constants.TableName
					+ " WHERE " + RefExchangeRateSchema.Constants.RE_RX_NKExCurrency + " = @CurrencyToGetRateFor"
					+ " AND " + RefExchangeRateSchema.Constants.RE_GC + " = @CompanyPK"
					+ " AND " + RefExchangeRateSchema.Constants.RE_ExRateType + " = '" + rateType + "'" // Not using parameter due to performance reasons
					+ " AND " + RefExchangeRateSchema.Constants.RE_OH_Client + " = @LocalClientPK"
					+ " AND " + RefExchangeRateSchema.Constants.RE_StartDate + " < @Date"
					+ " ORDER BY " + RefExchangeRateSchema.Constants.RE_StartDate + " DESC";

				command = Db.Connection.Command(selectSQL);
				command.AddParameter("@LocalClientPK", SqlDbType.UniqueIdentifier, localClientPK);
			}
			else if (IsValidRateType(rateType))
			{
				var selectSQL =
					"SELECT TOP 1 " + RefExchangeRateSchema.Constants.RE_SellRate + ", " + RefExchangeRateSchema.Constants.RE_ExpiryDate
					+ " FROM " + RefExchangeRateSchema.Constants.SqlSchemaName + "." + RefExchangeRateSchema.Constants.TableName
					+ " WHERE " + RefExchangeRateSchema.Constants.RE_RX_NKExCurrency + " = @CurrencyToGetRateFor"
					+ " AND " + RefExchangeRateSchema.Constants.RE_GC + " = @CompanyPK"
					+ " AND " + RefExchangeRateSchema.Constants.RE_ExRateType + " = '" + rateType + "'" // Not using parameter due to performance reasons
					+ " AND " + RefExchangeRateSchema.Constants.RE_OH_Client + " IS NULL"
					+ " AND " + RefExchangeRateSchema.Constants.RE_StartDate + " < @Date"
					+ " ORDER BY " + RefExchangeRateSchema.Constants.RE_StartDate + " DESC";

				command = Db.Connection.Command(selectSQL);
			}
			else
			{
				return null;
			}

			command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK);
			command.AddParameter("@CurrencyToGetRateFor", SqlDbType.Char, currencyToGetRateFor);
			command.AddParameter("@Date", SqlDbType.DateTime, timeToGet.AddDays(1D).Date);

			return command;
		}

		bool IsValidRateType(string rateType)
		{
			return !string.IsNullOrEmpty(rateType) && ExchangeRate.GetExchangeRateType(rateType.ToUpperInvariant()) != ExchangeRateType.All;
		}

		internal bool IsExchangeRateTypeNotEqualToSpecified(Guid companyPK, string rateType, string currencyToGetRateFor, DateTime timeToGet, bool includeExpired, Guid? localClientPK)
		{
			if (!string.IsNullOrEmpty(currencyToGetRateFor) && currencyToGetRateFor.Length == 3)
			{
				using (var command = GetCommandForReadingExchangeRate(companyPK, rateType, currencyToGetRateFor, timeToGet, localClientPK))
				{
					if (command != null)
					{
						using (var reader = command.ExecuteReader())
						{
							if (reader.Read())
							{
								int expiryDateOrdinal = reader.GetOrdinal(RefExchangeRateSchema.Constants.RE_ExpiryDate);

								if (reader.IsDBNull(expiryDateOrdinal) || reader.GetDateTime(expiryDateOrdinal).Date >= timeToGet.Date || includeExpired)
								{
									return false;
								}
							}
						}
					}
				}

				using (var command = GetCommandForReadingExchangeRate(companyPK, string.Empty, currencyToGetRateFor, timeToGet, localClientPK))
				{
					if (command != null)
					{
						using (var reader = command.ExecuteReader())
						{
							if (reader.Read())
							{
								int expiryDateOrdinal = reader.GetOrdinal(RefExchangeRateSchema.Constants.RE_ExpiryDate);

								if (reader.IsDBNull(expiryDateOrdinal) || reader.GetDateTime(expiryDateOrdinal).Date >= timeToGet.Date || includeExpired)
								{
									return true;
								}
							}
						}
					}
				}
			}

			return false;
		}

		#endregion
	}
}
