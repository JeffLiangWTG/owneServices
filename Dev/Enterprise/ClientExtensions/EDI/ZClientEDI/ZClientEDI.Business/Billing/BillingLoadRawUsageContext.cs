using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class BillingLoadRawUsageContext
	{
		public BillingLoadRawUsageContext(BusinessObjectFactory factory, ZDateTime period, ZGuid organisationPK, ZGuid clientCompanyPK, ZGuid licenceCompanyPK, ZGuid databasePK)
			: this(factory, period, databasePK, clientCompanyPK)
		{
			this.OrganisationPK = organisationPK;
			this.LicenceCompanyPK = licenceCompanyPK;

			Init();
		}

		public BillingLoadRawUsageContext(BusinessObjectFactory factory, ZDateTime period, ZGuid databasePk, ZGuid priceItemPk, ZGuid clientCompanyPk)
			: this(factory, period, databasePk, clientCompanyPk)
		{
			this.PriceItemPK = priceItemPk;

			Init();
		}

		BillingLoadRawUsageContext(BusinessObjectFactory factory, ZDateTime period, ZGuid databasePk, ZGuid clientCompanyPk)
		{
			this.Factory = factory;

			var billingPeriod = new BillingPeriod(period);
			this.Period = billingPeriod.PeriodStartDate;
			this.PeriodAsInt = Period.Year * 100 + Period.Month;
			this.PeriodStartTimeUtc = billingPeriod.StartTimeUtc;
			this.PeriodEndTimeUtc = billingPeriod.EndTimeUtc;

			this.DatabasePK = databasePk;
			this.ClientCompanyPK = clientCompanyPk;
		}

		void Init()
		{
			org = Factory.Load<EDIOrgHeader>(OrganisationPK);
			clientCompany = Factory.Load<ClientCompany>(ClientCompanyPK);
			licenceCompany = Factory.Load<LicenceCompany>(LicenceCompanyPK);
			database = Factory.Load<LicenceDatabase>(DatabasePK);
			priceItem = Factory.Load<ClientLicencePriceItem>(PriceItemPK);
			CategoryAndUsageCodes = priceItem != null ? GetCategoryAndUsageCodes(priceItem.PK) : Enumerable.Empty<UsageCodeKey>();
		}

		public readonly BusinessObjectFactory Factory;
		public readonly ZDateTime Period;
		public readonly int PeriodAsInt;
		public readonly ZDateTime PeriodStartTimeUtc;
		public readonly ZDateTime PeriodEndTimeUtc;
		public readonly ZGuid OrganisationPK;
		public readonly ZGuid ClientCompanyPK;
		public readonly ZGuid LicenceCompanyPK;
		public readonly ZGuid DatabasePK;
		public readonly ZGuid PriceItemPK;
		public IEnumerable<UsageCodeKey> CategoryAndUsageCodes { get; private set; }

		EDIOrgHeader org;
		ClientCompany clientCompany;
		LicenceCompany licenceCompany;
		LicenceDatabase database;
		ClientLicencePriceItem priceItem;

		public ZString OrgCode
		{
			get { return org != null ? org.OH_Code : ZString.Empty; }
		}

		public ZString OrgName
		{
			get { return org != null ? org.OH_FullName : ZString.Empty; }
		}

		public ZString CompanyCode
		{
			get
			{
				var result = ZString.Empty;
				if (clientCompany != null)
				{
					result = clientCompany.LCC_Code;
				}
				else if (licenceCompany != null)
				{
					result = licenceCompany.LC_CompanyCode;
				}
				return result;
			}
		}

		public ZString ServerCode
		{
			get { return database != null ? database.LD_ServerCode : ZString.Empty; }
		}

		public ZString DatabaseId
		{
			get { return database != null ? database.DatabaseId : ZString.Empty; }
		}

		public ZInt DatabaseNumber => database?.LD_DatabaseNumber ?? ZInt.Zero;

		public ZString HostDatabaseName => database?.LD_HostDBName ?? ZString.Empty;

		public bool IsConsolidatedDatabase => database?.IsConsolidatedDatabase ?? false;

		public ZString EnterpriseCode => database?.EnterpriseCode ?? ZString.Empty;

		public ZString PriceItemCode
		{
			get { return priceItem != null ? priceItem.L7_Code : ZString.Empty; }
		}

		public ZString PriceItemFeeType
		{
			get { return priceItem != null ? priceItem.L7_FeeType : ZString.Empty; }
		}

		public ZString PriceItemDescription
		{
			get { return priceItem != null ? priceItem.L7_DescriptionLocalized : ZString.Empty; }
		}

		public ZGuid PriceHeaderPK
		{
			get { return priceItem != null ? priceItem.L7_L6 : ZGuid.Empty; }
		}

		#region IDatabaseUsers

		public IDatabaseUsers DatabaseUsersService
		{
			get { return databaseUsersService ?? (databaseUsersService = new DatabaseUsers(!OrganisationPK.IsEmpty)); }
			internal set
			{
				if (Globals.IsTest)
				{
					databaseUsersService = value;
				}
			}
		}

		IDatabaseUsers databaseUsersService;

		#endregion

		IEnumerable<UsageCodeKey> GetCategoryAndUsageCodes(ZGuid priceItemPk)
		{
			const string sql =
@"declare @L7_L6 uniqueidentifier;
declare @L7_Code varchar(3);
declare @L7_Category varchar(3);
select @L7_L6 = L7_L6, @L7_Code = L7_Code, @L7_Category = L7_Category from dbo.ClientLicencePriceItem where L7_PK = @priceItemPk

select @L7_Category, @L7_Code
union all
select PUM_UsageCategory, PUM_UsageCode from dbo.EdiPriceUsageMapping 
where PUM_L6 = @L7_L6
	and PUM_PriceCategory = @L7_Category
	and PUM_PriceCode = @L7_Code";

			var result = new List<UsageCodeKey>(4);

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@priceItemPk", System.Data.SqlDbType.UniqueIdentifier, priceItemPk.ToGuid());

				using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
				{
					while (reader.Read())
					{
						var category = reader.GetString(0);
						var code = reader.GetString(1);
						result.Add(new UsageCodeKey(category, code));
					}
				}
			}

			return result;
		}
	}
}
