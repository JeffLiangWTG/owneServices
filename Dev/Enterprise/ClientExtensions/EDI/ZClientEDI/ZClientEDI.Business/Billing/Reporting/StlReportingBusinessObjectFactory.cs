using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Billing.Business
{
	public abstract class StlReportingBusinessObjectBase
	{
		public abstract void GetCsvUsageReport(Action<string> action);

		public abstract void GetCsvUsageReport(ICsvUsageReportWriter writer);

		public abstract StlRawUsage LoadStlRawUsage();

		protected DbConnection GetConnectionForReader()
		{
			if (Globals.IsTest)
			{
				return Db.Connection;
			}
			return Db.NewExtraConnectionToMainDb();
		}
	}

	public static class StlReportingBusinessObjectFactory
	{
		public static StlReportingBusinessObjectBase CreateReportingBusinessObject(BillingLoadRawUsageContext context, ZString systemCode)
		{
			if (systemCode == BillingConstants.BillingSystem.ODM && context.PriceItemCode == StlReportingBusinessObjectGPC.PriceItemCode)
			{
				return new StlReportingBusinessObjectGPC(context);
			}
			else if (systemCode == BillingConstants.BillingSystem.STL && context.PriceItemCode == StlReportingBusinessObjectBYO.PriceItemCode)
			{
				return new StlReportingBusinessObjectBYO(context);
			}

			return null;
		}
	}

	class StlReportingBusinessObjectGPC : StlReportingBusinessObjectBase
	{
		public const string PriceItemCode = "GPC";
		readonly BillingLoadRawUsageContext Context;

		public StlReportingBusinessObjectGPC(BillingLoadRawUsageContext context)
		{
			Context = context;
		}

		public override void GetCsvUsageReport(Action<string> action)
		{
			var headerColumns = new string[] { "Country", "Registered Users" };
			var headerCsvLine = new OCsvLine(headerColumns);
			action(headerCsvLine.ToString());

			GetUsage((countryCode, userCount) =>
			{
				var dataValues = new string[] { countryCode, userCount.ToString(CultureInfo.InvariantCulture) };
				var dataCsvLine = new OCsvLine(dataValues);
				action(dataCsvLine.ToString());
			});
		}

		public override void GetCsvUsageReport(ICsvUsageReportWriter writer)
		{
			GetUsage((countryCode, userCount) =>
			{
				var reference = string.Concat(countryCode, ' ', userCount);
				writer.WriteCsvUsageReport(Context.PeriodStartTimeUtc, "", "", "", reference, Context.PriceItemCode, Context.PriceItemDescription, userCount);
			});
		}

		public override StlRawUsage LoadStlRawUsage()
		{
			var rawUsage = new StlRawUsage(Context);

			rawUsage.Summary.Header.TopLevelDescription = "Usage Summary";
			rawUsage.Summary.Header.Column1 = "Country";
			rawUsage.Summary.Header.Column2 = "Registered Users";

			GetUsage((countryCode, userCount) =>
			{
				var line = rawUsage.Summary.Lines.AddNew();
				line.Column1 = countryCode;
				line.Column2 = userCount.ToString(CultureInfo.InvariantCulture);
			});

			return rawUsage;
		}

		void GetUsage(Action<string, int> dataAction)
		{
			using (var cmd = GetConnectionForReader().Command(Query))
			{
				cmd.AddParameter("@DatabasePK", System.Data.SqlDbType.UniqueIdentifier, Context.DatabasePK.ToGuid());
				cmd.AddParameter("@PeriodStart", System.Data.SqlDbType.SmallDateTime, Context.Period.ToDateTime());
				cmd.AddParameter("@ClientCompanyPK", System.Data.SqlDbType.UniqueIdentifier, Context.ClientCompanyPK.IsEmpty ? DBNull.Value : Context.ClientCompanyPK.ToGuid());

				using (var reader = cmd.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
				{
					while (reader.Read())
					{
						var countryCode = reader.GetString(0);
						var userCount = reader.GetInt32(1);
						dataAction(countryCode, userCount);
					}
				}
			}
		}

		readonly string Query = @"
select countries.CountryCode, UserCount = isnull(UserCount, 0)
from
(
	select CountryCode = ISNULL(BCG_MainCountryCode, LCC_RN_NKCountryCode)
	from dbo.ClientChargeableUsage
	JOIN dbo.ClientCompany ON U1_LCC = LCC_PK
	LEFT JOIN
	(
		SELECT BCG_CountryCode, BCG_MainCountryCode FROM EdiGetBillingCountryGroups() WHERE BDG_IsDomesticUserGroup = 'Y'
	) CountryGroups ON LCC_RN_NKCountryCode = BCG_CountryCode
	WHERE
		U1_Code = 'ODM'
		AND U1_SubCode = 'GPC'
		AND U1_LD = @DatabasePK
		AND U1_PeriodStart = @PeriodStart
		AND (@ClientCompanyPK IS NULL OR U1_LCC = @ClientCompanyPK)
) countries
left join
(
	SELECT CountryCode, UserCount = CAST(SUM(U1_UnitCount) AS INT)
	FROM
	(
		SELECT
			CountryCode = ISNULL(BCG_MainCountryCode, LCC_RN_NKCountryCode),
			U1_UnitCount
		FROM
			dbo.ClientChargeableUsage
			JOIN dbo.ClientCompany ON U1_LCC = LCC_PK
			LEFT JOIN
			(
				SELECT BCG_CountryCode, BCG_MainCountryCode FROM EdiGetBillingCountryGroups() WHERE BDG_IsDomesticUserGroup = 'Y'
			) CountryGroups ON LCC_RN_NKCountryCode = BCG_CountryCode
		WHERE
			U1_Code = 'STL'
			AND U1_SubCode = 'USR'
			AND U1_LD = @DatabasePK
			AND U1_PeriodStart = @PeriodStart
			AND (@ClientCompanyPK IS NULL OR U1_LCC = @ClientCompanyPK)
	)tb
	GROUP BY CountryCode
) usersPerCountry on countries.CountryCode = usersPerCountry.CountryCode
ORDER BY countries.CountryCode;
";
	}
}


