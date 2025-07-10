using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Progress;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class BillingRunContext
	{
		public BillingRunContext(BusinessObjectFactory factory, ZDateTime generateDate, ZDateTime dateToInclusive)
		{
			Argument.NotNull(factory, "factory");
			if (!dateToInclusive.IsValid || dateToInclusive.IsEmpty)
			{
				throw new ArgumentException("DateTo is invalid or empty");
			}

			PeriodStart = dateToInclusive.AddDays(1).AddMonths(-1);
			if (PeriodStart.Day != 1)
			{
				throw new ArgumentException("DateTo is not the last day of the month");
			}

			Factory = factory;
			GenerateDate = generateDate;
			DateToInclusive = dateToInclusive;
			OrganisationPK = ZGuid.Empty;
			Progress = null;
			IncludeOdpl = true;
			IncludeStl = true;

			companyDeliveries = new LicenceCompanyDeliverySet(factory);
		}

		public BillingRunContext(BusinessObjectFactory factory, ZDateTime generateDate, ZDateTime dateToInclusive, ZGuid organisationPK, string enterpriseCode = "")
			: this(factory, generateDate, dateToInclusive)
		{
			if (!organisationPK.IsValid && !organisationPK.IsEmpty)
			{
				throw new ArgumentException("OrganisationPK is invalid");
			}

			this.OrganisationPK = organisationPK;
			this.EnterpriseCode = enterpriseCode;
		}

		public BillingRunContext(BusinessObjectFactory factory, ZDateTime generateDate, ZDateTime dateToInclusive, ZGuid organisationPK, IEdiProgress progress)
			: this(factory, generateDate, dateToInclusive, organisationPK)
		{
			Progress = progress;
		}

		public readonly BusinessObjectFactory Factory;
		public readonly ZDateTime DateToInclusive;
		public readonly ZGuid OrganisationPK;
		public readonly IEdiProgress Progress;
		public readonly ZDateTime PeriodStart;
		public readonly string EnterpriseCode;

		public bool IncludeOdpl { get; set; }
		public bool IncludeStl { get; set; }
		public bool IsPreviewOnly { get; private set; }
		public bool IsBackPost { get; set; }
		public ZDateTime GenerateDate { get; private set; }
		public ClientLicencePriceHeader PreviewPriceHeader { get; private set; }

		public ZDateTime PeriodStartBillingRegisteredUser => new ZDateTime(2016, 11, 1);
		public ZDateTime EarliestPriceCommenceDateCanBillRegisteredUser => new ZDateTime(2016, 7, 1);

		public void SetPreviewOnly(LicenceKeyBuilder.Business.LicenceHeader licHeader, ClientLicencePriceHeader previewPriceHeader)
		{
			IsPreviewOnly = true;
			PreviewPriceHeader = previewPriceHeader;
			billingGroupDatabasePKs = new ZGuid[] { licHeader.LA_LD };
			billingGroupOrgPKs = new ZGuid[] { licHeader.Company.LC_OH };
		}

		#region Billing Group PKs

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public ZGuid[] BillingGroupOrgPKs
		{
			get
			{
				LoadBillingGroupPKs();
				return billingGroupOrgPKs;
			}
		}
		ZGuid[] billingGroupOrgPKs;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public ZGuid[] BillingGroupDatabasePKs
		{
			get
			{
				LoadBillingGroupPKs();
				return billingGroupDatabasePKs;
			}
		}
		ZGuid[] billingGroupDatabasePKs;

		void LoadBillingGroupPKs()
		{
			if (billingGroupOrgPKs == null)
			{
				GetBillingGroupPKs();
			}
		}

		void GetBillingGroupPKs()
		{
			var orgPks = new List<ZGuid>();
			var databasePks = new List<ZGuid>();

			var query = "SELECT IsOrg, GroupPk FROM EdiGetBillingGroups(@OrgPk, @PeriodStart, NULL)";
			using (DbCommand command = Db.Connection.Command(query))
			{
				command.AddParameter("@OrgPk", System.Data.SqlDbType.UniqueIdentifier, OrganisationPK.ToGuid());
				command.AddParameter("@PeriodStart", System.Data.SqlDbType.DateTime, PeriodStart.ToDateTime().AddDays(15));
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var isOrg = reader.GetBoolean(0);
						var pk = reader.GetGuid(1);
						(isOrg ? orgPks : databasePks).Add(pk);
					}
				}
			}

			billingGroupOrgPKs = orgPks.ToArray();
			billingGroupDatabasePKs = databasePks.ToArray();
		}

		#endregion

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

		#region IModuleUsers

		public IModuleUsers ModuleUsersService { get; set; }

		#endregion

		#region ISystemMinimumFees

		public ISystemMinimumFees SystemMinimumFeesService { get; set; }

		#endregion

		#region Currency Exchange

		public CurrencyExchangeService CurrencyExchange
		{
			get { return currencyExchange ?? (currencyExchange = CurrencyExchangeService.GetInstance(Factory, DateForExchangeRate)); }
		}
		CurrencyExchangeService currencyExchange;

		public ZDateTime DateForExchangeRate
		{
			get
			{
				ZDateTime dateForExchangeRate = GenerateDate;
				if (IsBackPost)
				{
					var backPostDate = GenerateDate.AddDays(-GenerateDate.Day);
					var isLocalCurrency = !(PreviewPriceHeader != null && PreviewPriceHeader.LicCompany != null)
						|| PreviewPriceHeader.L6_RX_NKCurrency == PreviewPriceHeader.LicCompany.LC_RX_NKCurrency;
					var companyPK = PreviewPriceHeader?.L6_LC ?? GlbCompany.CurrentCompany.PK;
					dateForExchangeRate = BillingInvoicingHelper.GetDateForExchangeRate(backPostDate, isLocalCurrency, companyPK);
				}
				return dateForExchangeRate;
			}
		}

		#endregion

		#region LicenceCompanies

		public LicenceCompanyDeliverySet CompanyDeliveries
		{
			get { return companyDeliveries; }
		}

		readonly LicenceCompanyDeliverySet companyDeliveries;

		#endregion

		#region Discount Versions

		public DiscountVersionSet DiscountVersions
		{
			get
			{
				return discountVersionSet ?? (discountVersionSet = new DiscountVersionSet(Factory));
			}
		}
		DiscountVersionSet discountVersionSet;

		#endregion

		#region Database Country Users

		public DatabaseCountryUserSet DatabaseCountryUsers
		{
			get
			{
				return databaseCountryUsers ?? (databaseCountryUsers = new DatabaseCountryUserSet());
			}
		}
		DatabaseCountryUserSet databaseCountryUsers;

		#endregion

		public ProductBundleSet ProductBundles { get; } = new ProductBundleSet();
	}
}

