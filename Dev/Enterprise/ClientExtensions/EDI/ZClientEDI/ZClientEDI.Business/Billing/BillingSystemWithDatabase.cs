using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Billing.Business
{
	public abstract class BillingSystemWithDatabase : BillingSystem
	{
		protected BillingSystemWithDatabase()
			: base()
		{
		}

		#region Load Raw Usage

		protected abstract string Query_Raw_Usage { get; }

		protected DbCommand GetRawUsageQuery(BillingLoadRawUsageContext context)
		{
			string databaseId = string.Empty;
			string companyCode = string.Empty;
			string clientNumber = string.Empty;
			if (!context.DatabaseId.IsEmpty || !context.CompanyCode.IsEmpty)
			{
				databaseId = !context.DatabaseId.IsEmpty ? context.DatabaseId.ToString() : "%";
				companyCode = !context.CompanyCode.IsEmpty ? context.CompanyCode.ToString() : "%";
				clientNumber = databaseId + "." + companyCode;
			}

			var query = Query_Raw_Usage;

			// use date literals rather than date parameters to get SQL server to compile an optimal query plan
			// ( https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev/pullrequest/15120?_a=files&path=%2FEnterprise%2FClientExtensions%2FEDI%2FZClientEDI%2FZClientEDI.Business%2FBilling%2FBillingSystemWithDatabase.cs for rationalization)
			query = query.Replace("@DateFrom", "'" + context.PeriodStartTimeUtc.SqlFormat + "'");
			query = query.Replace("@DateTo", "'" + context.PeriodEndTimeUtc.SqlFormat + "'");
			query = query.Replace("@Period", context.PeriodAsInt.ToString(CultureInfo.InvariantCulture));

			var command = GetConnectionForReader().Command(query);

			// however, everything ELSE is parameterized
			command.AddParameter("@DatabaseId", SqlDbType.VarChar, databaseId);
			command.AddParameter("@CompanyCode", SqlDbType.VarChar, companyCode);
			command.AddParameter("@ClientNumber", SqlDbType.VarChar, clientNumber);

			command.AddParameter("@OrgPk", SqlDbType.UniqueIdentifier, context.OrganisationPK.IsEmpty ? Guid.Empty : (object)context.OrganisationPK.ToGuid());
			command.AddParameter("@LicenceCompanyPk", SqlDbType.UniqueIdentifier, context.LicenceCompanyPK.IsEmpty ? Guid.Empty : (object)context.LicenceCompanyPK.ToGuid());
			command.AddParameter("@DatabasePk", SqlDbType.UniqueIdentifier, context.DatabasePK.IsEmpty ? Guid.Empty : (object)context.DatabasePK.ToGuid());

			command.AddParameter("@PriceItemCode", SqlDbType.VarChar, context.PriceItemCode.ToString());
			command.AddParameter("@ClientCompanyPk", SqlDbType.UniqueIdentifier, context.ClientCompanyPK.IsEmpty ? DBNull.Value : context.ClientCompanyPK.ToGuid());

			return command;
		}

		public override SystemRawUsage LoadOdplRawUsage(BillingLoadRawUsageContext context)
		{
			using (var command = GetRawUsageQuery(context))
			using (var reader = command.ExecuteReader())
			{
				return LoadOdplRawUsageFromDataReader(reader, context);
			}
		}

		protected abstract SystemRawUsage LoadOdplRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context);

		public override StlRawUsage LoadStlRawUsage(BillingLoadRawUsageContext context)
		{
			using (var command = GetRawUsageQuery(context))
			using (var reader = command.ExecuteReader())
			{
				return LoadStlRawUsageFromDataReader(reader, context);
			}
		}

		protected abstract StlRawUsage LoadStlRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context);

		#endregion

		#region Implementation

		protected ZDateTime AsDateTime(object value)
		{
			if (value == null || value is DBNull)
			{
				return ZDateTime.Empty;
			}
			else if (value is DateTime)
			{
				return (DateTime)value;
			}
			else
			{
				return ZDateTime.Invalid;
			}
		}

		protected DbConnection GetConnectionForReader()
		{
			if (Globals.IsTest)
			{
				return Db.Connection;
			}
			return Db.NewExtraConnectionToMainDb();
		}

		#endregion
	}
}

