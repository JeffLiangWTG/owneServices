using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business.Maintenance
{
	public class MaintenanceFilter : NonPersistentBusinessObject, IObsoleteValidation
	{
		public MaintenanceFilter(BusinessObjectFactory factory)
			: base(factory)
		{
			InitDateRange();
		}

		#region Business Object Overrides

		public override bool HasChanges
		{
			get { return false; }
			set { }
		}

		#endregion

		#region OrganisationPK

		[List("Organisations")]
		public ZGuid OrganisationPK
		{
			get { return organisationPK; }
			set
			{
				SetNonPersistentPropertyValue(OrganisationPKInfo, ref organisationPK, value);
				if (!IsValidationSuspended)
				{
					ValidateOrganisationPK();
				}
			}
		}
		ZGuid organisationPK;

		public ZPropertyInfo OrganisationPKInfo
		{
			get { return this.GetZPropertyInfo(nameof(OrganisationPK)); }
		}

		public void ValidateOrganisationPK()
		{
			OrganisationPKInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(OrganisationPKInfo);
		}

		public OrgHeaderCollection Organisations
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		#endregion

		void InitDateRange()
		{
			ZDateTime thisMonth = ZDateTime.Now;
			thisMonth = new ZDateTime(thisMonth.Year, thisMonth.Month, 1);
			DueDateFrom = thisMonth;
			DueDateTo = thisMonth.AddMonths(6);
		}

		public bool IsDateInRange(ZDateTime date)
		{
			return (DueDateFrom.IsEmpty || date >= DueDateFrom)
				&& (DueDateTo.IsEmpty || date <= DueDateTo);
		}

		#region DueDateFrom

		public ZDateTime DueDateFrom
		{
			get { return dueDateFrom; }
			set
			{
				SetNonPersistentPropertyValue(DueDateFromInfo, ref dueDateFrom, value);
				if (!IsValidationSuspended)
				{
					ValidateDueDateFrom();
				}
			}
		}
		ZDateTime dueDateFrom;

		public ZPropertyInfo DueDateFromInfo { get { return GetZPropertyInfo(nameof(DueDateFrom)); } }

		public void ValidateDueDateFrom()
		{
			DueDateFromInfo.ClearAllNotifications();

			if (DueDateFrom.IsValid && DueDateTo.IsValid && DueDateFrom > DueDateTo)
			{
				DueDateFromInfo.AddError("Date From must not be after Date To.");
			}
		}

		#endregion

		#region DueDateTo

		public ZDateTime DueDateTo
		{
			get { return dueDateTo; }
			set
			{
				SetNonPersistentPropertyValue(DueDateToInfo, ref dueDateTo, value);
				if (!IsValidationSuspended)
				{
					ValidateDueDateTo();
				}
			}
		}
		ZDateTime dueDateTo;
		public ZPropertyInfo DueDateToInfo { get { return GetZPropertyInfo(nameof(DueDateTo)); } }

		public void ValidateDueDateTo()
		{
			DueDateToInfo.ClearAllNotifications();

			if (DueDateFrom.IsValid && DueDateTo.IsValid && DueDateFrom > DueDateTo)
			{
				DueDateToInfo.AddError("Date To must not be before Date From.");
			}
		}

		#endregion

		#region Include NonBilled

		public ZBool IncludeNonBilled
		{
			get { return includeNonBilled; }
			set
			{
				SetNonPersistentPropertyValue(IncludeNonBilledInfo, ref includeNonBilled, value);
			}
		}
		ZBool includeNonBilled;

		public ZPropertyInfo IncludeNonBilledInfo { get { return GetZPropertyInfo(nameof(IncludeNonBilled)); } }

		#endregion

		#region Include Login Company Only

		public ZBool IncludeLoginCompanyInvoicesOnly
		{
			get { return includeLoginCompanyInvoicesOnly; }
			set
			{
				SetNonPersistentPropertyValue(IncludeLoginCompanyInvoicesOnlyInfo, ref includeLoginCompanyInvoicesOnly, value);
			}
		}
		ZBool includeLoginCompanyInvoicesOnly = true;

		public ZPropertyInfo IncludeLoginCompanyInvoicesOnlyInfo { get { return GetZPropertyInfo(nameof(IncludeLoginCompanyInvoicesOnly)); } }

		#endregion

		#region EnterpriseCode

		[MaxLength(3)]
		public ZString EnterpriseCode
		{
			get { return enterpriseCode; }
			set
			{
				if (enterpriseCode != value)
				{
					CheckMaximumLength(EnterpriseCodeInfo, value);
					SetNonPersistentPropertyValue(EnterpriseCodeInfo, ref enterpriseCode, value);
				}
			}
		}
		ZString enterpriseCode;
		public ZPropertyInfo EnterpriseCodeInfo { get { return GetZPropertyInfo(nameof(EnterpriseCode)); } }

		#endregion

		#region

		public ZInt RenewalMonths
		{
			get { return renewalMonths; }
			set
			{
				if (renewalMonths != value)
				{
					SetNonPersistentPropertyValue(RenewalMonthsInfo, ref renewalMonths, value);
					if (!IsValidationSuspended)
					{
						ValidateRenewalMonths();
					}
				}
			}
		}
		ZInt renewalMonths;
		public ZPropertyInfo RenewalMonthsInfo { get { return GetZPropertyInfo(nameof(RenewalMonths)); } }

		void ValidateRenewalMonths()
		{
			RenewalMonthsInfo.ClearAllNotifications();
			MandatoryValidation.CheckNotNegative(RenewalMonthsInfo);
		}

		#endregion

		public LicenceHeader[] LoadLicenceHeaders(BusinessObjectFactory factory)
		{
			ZDBOnlyQuery licenceHeaderQuery = new ZDBOnlyQuery(typeof(LicenceHeader));
			licenceHeaderQuery.AddToFilter(LicenceHeaderSchema.LA_IsActive, ZBool.True);

			ZDBOnlySubQuery databaseQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.PK);
			databaseQuery.AddToFilter(LicenceDatabaseSchema.LD_LicenceType, new string[] { DatabaseTypes.Codes.Production, DatabaseTypes.Codes.Test, DatabaseTypes.Codes.Training });
			databaseQuery.AddToFilter(LicenceDatabaseSchema.LD_IsActive, ZBool.True);

			ZDBOnlySubQuery moduleSubQuery = new ZDBOnlySubQuery(typeof(LicenceModules), LicenceModulesSchema.LM_LA);
			moduleSubQuery.AddToFilter(LicenceModulesSchema.LM_LicenceType, new ZString[] { LicenceTypes.Codes.PUR, LicenceTypes.Codes.OTM, LicenceTypes.Codes.ODM, LicenceTypes.Codes.OPN });
			moduleSubQuery.AddToFilter(LicenceModulesSchema.LM_UserCount, SQLComparisonOperator.GreaterThan, ZShort.Zero);

			ZDBOnlySubQuery companySubQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceHeaderSchema.LA_LC);

			ZDBOnlySubQuery orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), LicenceCompanySchema.LC_OH);
			orgSubQuery.AddToFilter(OrgHeaderSchema.OH_IsActive, ZBool.True);

			ZDBOnlySubQuery enterpriseSubQuery = new ZDBOnlySubQuery(typeof(LicenceEnterprise), LicenceCompanySchema.LC_LE);
			enterpriseSubQuery.AddToFilter(LicenceEnterpriseSchema.LE_IsInternal, ZBool.False);

			if (!OrganisationPK.IsEmpty)
			{
				orgSubQuery.AddToFilter(OrgHeaderSchema.PK, OrganisationPK);
			}
			else
			{
				if (DueDateFrom.IsValid)
				{
					licenceHeaderQuery.AddToFilter(LicenceHeaderSchema.LA_ContractExpiryDate, SQLComparisonOperator.GreaterThanOrEqualTo, DueDateFrom.AddDays(-1));
				}
				if (DueDateTo.IsValid)
				{
					licenceHeaderQuery.AddToFilter(LicenceHeaderSchema.LA_ContractExpiryDate, SQLComparisonOperator.LessThan, DueDateTo);
				}

				if (!EnterpriseCode.IsEmpty)
				{
					enterpriseSubQuery.AddToFilter(LicenceEnterpriseSchema.LE_EnterpriseCode, EnterpriseCode);
				}

				if (RenewalMonths != 0)
				{
					ZDBOnlySubQuery billingQuery = new ZDBOnlySubQuery(typeof(ClientLicenceHeaderEx), ClientLicenceHeaderExSchema.L0_LA);
					billingQuery.AddToFilter(ClientLicenceHeaderExSchema.L0_RenewalMonths, RenewalMonths);
					licenceHeaderQuery.AddSubQuery(LicenceHeaderSchema.PK, billingQuery, JoinCondition.And);
				}
			}

			companySubQuery.AddSubQuery(enterpriseSubQuery, JoinCondition.And);
			companySubQuery.AddSubQuery(orgSubQuery, JoinCondition.And);
			licenceHeaderQuery.AddSubQuery(LicenceHeaderSchema.LA_LD, databaseQuery, JoinCondition.And);
			licenceHeaderQuery.AddSubQuery(moduleSubQuery, JoinCondition.And);
			licenceHeaderQuery.AddSubQuery(companySubQuery, JoinCondition.And);

			return factory.Load<LicenceHeader>(licenceHeaderQuery);
		}
	}
}

