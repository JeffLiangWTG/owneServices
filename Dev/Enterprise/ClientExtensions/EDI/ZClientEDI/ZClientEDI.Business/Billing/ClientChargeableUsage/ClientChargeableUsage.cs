using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ClientChargeableUsage : AutoClientChargeableUsage
	{
		public ClientChargeableUsage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public LicenceCompany LicenceCompany
		{
			get { return Factory.Load<LicenceCompany>(U1_LC); }
		}

		public ZGuid OrganisationPK
		{
			get
			{
				var company = LicenceCompany;
				if (company == null)
				{
					var client = ClientCompany;
					LicenceHeader owner = null;
					if (client != null)
					{
						owner = client.UsageOwnerLicence;
					}
					else
					{
						var db = Database;
						if (db != null)
						{
							owner = db.UsageOwnerOrFirstLicence;
						}
					}
					company = owner != null ? owner.Company : null;
				}

				return company != null ? company.LC_OH : ZGuid.Empty;
			}
		}

		public ClientCompany ClientCompany
		{
			get { return clientCompany ?? (clientCompany = Factory.Load<ClientCompany>(U1_LCC)); }
		}
		ClientCompany clientCompany;

		public string CompanyCode
		{
			get
			{
				string result;
				var client = ClientCompany;
				if (client != null)
				{
					result = client.LCC_Code;
				}
				else
				{
					var company = LicenceCompany;
					if (company != null)
					{
						result = company.LC_CompanyCode;
					}
					else
					{
						result = string.Empty;
					}
				}

				return result;
			}
		}

		public LicenceDatabase Database
		{
			get { return database ?? (database = Factory.Load<LicenceDatabase>(U1_LD)); }
		}
		LicenceDatabase database;

		public LicenceDatabase LicenceDatabase
		{
			get { return Database; }
		}

		public new ARInvoice Invoice
		{
			get { return Factory.Load<ARInvoice>(U1_AH_Invoice); }
		}

		public ZInt U1_UnitCountAsInt => U1_UnitCount.ToZInt();

		/// <summary>
		/// Return the last usage in the given date range for an Org or any Org billed to it
		/// </summary>
		/// <param name="start">start date</param>
		/// <param name="endExclusive">end date</param>
		/// <param name="enabledSystemCodes">system codes to query</param>
		/// <returns>last usage</returns>
		public static ClientChargeableUsage LastGroupUsageInDateRange(BusinessObjectFactory factory,
			EDIOrgHeader org,
			ZDateTime start,
			ZDateTime endExclusive,
			IEnumerable<string> enabledSystemCodes)
		{
			ZQuery query = new ZQuery();
			if (!start.IsEmpty)
			{
				query.AddToFilter(JoinCondition.And, ClientChargeableUsageSchema.U1_PeriodStart, SQLComparisonOperator.GreaterThanOrEqualTo, start);
			}

			if (!endExclusive.IsEmpty)
			{
				query.AddToFilter(JoinCondition.And, ClientChargeableUsageSchema.U1_PeriodStart, SQLComparisonOperator.LessThan, endExclusive);
			}

			List<ZGuid> companyPKs = new List<ZGuid>();
			if (org.LicCompany != null)
			{
				companyPKs.Add(org.LicCompany.PK);
			}
			companyPKs.AddRange(BilledTo(factory, org.PK).Select(x => x.L9_LC));
			ZQuery companyQuery = new ZQuery(ClientChargeableUsageSchema.U1_LC, companyPKs);

			if (enabledSystemCodes != null && enabledSystemCodes.Any())
			{
				query.AddToFilter(ClientChargeableUsageSchema.U1_Code, enabledSystemCodes);
			}

			query.AddToFilter(companyQuery);
			query.OrderBy = ClientChargeableUsageSchema.Constants.U1_PeriodStart + " DESC";

			return factory.LoadTop1<ClientChargeableUsage>(query);
		}

		static ClientInvoiceDelivery[] BilledTo(BusinessObjectFactory factory, ZGuid orgPk)
		{
			ZQuery query = new ZQuery(ClientInvoiceDeliverySchema.L9_OH_InvoiceTo, orgPk);
			return factory.Load<ClientInvoiceDelivery>(query);
		}

		public ClientChargeableUsage GetInAnotherFactory(BusinessObjectFactory anotherFactory)
		{
			if (Factory == anotherFactory)
			{
				return this;
			}

			return anotherFactory.GetBizOsForPK(PK.ToGuid())
				.Select(x => x as ClientChargeableUsage)
				.FirstOrDefault(x => x != null)
				?? (ClientChargeableUsage)anotherFactory.ImportFromAnotherFactory(this);
		}

		#region Test Data
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			var today = ZDateTime.Today.Date;
			U1_PeriodStart = today.AddDays(1 - today.Day);
		}
#endif
#endregion
	}
}

