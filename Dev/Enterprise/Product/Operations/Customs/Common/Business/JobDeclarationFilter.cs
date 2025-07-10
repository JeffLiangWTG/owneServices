using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Common
{
	public static class JobDeclarationFilter
	{
		public static ZQuery ForCompanyAndShipment(bool ignoreActiveFilter, GlbCompany company, ZGuid shipmentPK)
		{
			Argument.NotNull(company, "company");
			var result = new ZQuery
			{
				IgnoreActiveFilter = ignoreActiveFilter,
			};

			if (!shipmentPK.IsEmpty)
			{
				result.AddToFilter(JobDeclarationSchema.JE_JS, SQLComparisonOperator.Equal, shipmentPK);
				if (company.IsInSingapore())
				{
					result.AddToFilter(JobDeclarationSchema.JE_ApplicationCode, SQLComparisonOperator.NotEqual, ZString.Empty);
				}
			}
			result.AddToFilter(JobDeclarationSchema.JE_GC, company.PK);

			return result;
		}

		public static ZQuery ForCountry(bool ignoreActiveFilter, ZString countryCode, BusinessObjectFactory factory)
		{
			var result = new ZDBOnlyQuery(typeof(IBaseJobDeclaration))
			{
				IgnoreActiveFilter = ignoreActiveFilter,
			};

			var companyQuery = new ZDBOnlySubQuery(typeof(GlbCompany), JobDeclarationSchema.JE_GC);
			companyQuery.AddToFilter(JoinCondition.And, GlbCompanySchema.GC_RN_NKCountryCode, SQLComparisonOperator.Equal, countryCode);
			result.AddSubQuery(companyQuery, JoinCondition.And);

			return result;
		}

		public static ZQuery ForDeclarationReference(bool ignoreActiveFilter, ZString declarationReference)
		{
			var result = new ZQuery
			{
				IgnoreActiveFilter = ignoreActiveFilter,
			};

			result.AddToFilter(JobDeclarationSchema.JE_DeclarationReference, SQLComparisonOperator.Equal, declarationReference);
			result.AddToFilter(JobDeclarationSchema.JE_GC, GlbCompany.CurrentCompany.PK);

			return result;
		}

		public static ZQuery GetEntryStatusQueryAllEntries(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetEntryStatusQueryAllEntries(comparisonOperator, value, CusEntryHeaderSchema.CH_EntryStatus);
		}
		public static ZQuery GetEntryPhaseStatusQueryAllEntries(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetEntryStatusQueryAllEntries(comparisonOperator, value, CusEntryHeaderSchema.CH_PhaseStatus);
		}

		public static ZQuery GetMessageStatusQueryAllEntries(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetEntryStatusQueryAllEntries(comparisonOperator, value, CusEntryHeaderSchema.CH_Status);
		}

		static ZQuery GetEntryStatusQueryAllEntries(SQLComparisonOperator comparisonOperator, ZString value, SchemaStringColumn entryStatusColumn)
		{
			if (value == NotSentCustomsStatusForFilter)
			{
				value = ZString.Empty;
			}

			var additionalSql = FormattableString.Invariant($@"JE_PK IN (SELECT CH_JE FROM CusEntryHeader
										GROUP BY CH_JE HAVING COUNT(DISTINCT {entryStatusColumn.Name}) = 1)");

			var entryHeadersAllWithSameStatusAndParentQuery = new ZDBOnlyQuery(typeof(IBaseJobDeclaration));
			entryHeadersAllWithSameStatusAndParentQuery.AddFilterAndZSQLParameterCollection(additionalSql, null);

			var jobDeclarationQuery = new ZDBOnlyQuery(typeof(IBaseJobDeclaration));
			var entryStatusQuery = new ZDBOnlySubQuery(typeof(ICusEntryHeader), CusEntryHeaderSchema.CH_JE);
			entryStatusQuery.AddToFilter(entryStatusColumn, comparisonOperator, value);
			jobDeclarationQuery.AddSubQuery(entryStatusQuery, JoinCondition.And);
			jobDeclarationQuery.AddToFilter(entryHeadersAllWithSameStatusAndParentQuery, JoinCondition.And);

			var result = new ZDBOnlyQuery(typeof(IBaseJobDeclaration));
			result.AddToFilter(jobDeclarationQuery, JoinCondition.And);
			if (value.IsEmpty)
			{
				var doesNotHaveEntrySql = FormattableString.Invariant($@"JE_PK NOT IN (SELECT CH_JE FROM CusEntryHeader)");

				var doesNotHaveEntrySqlQuery = new ZDBOnlyQuery(typeof(IBaseJobDeclaration));
				doesNotHaveEntrySqlQuery.AddFilterAndZSQLParameterCollection(doesNotHaveEntrySql, null);
				result.AddToFilter(doesNotHaveEntrySqlQuery, JoinCondition.Or);
			}
			return result;
		}

		static bool IsInSingapore(this GlbCompany company)
		{
			Argument.NotNull(company, "company");
			return company.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Singapore;
		}

		const string NotSentCustomsStatusForFilter = "NOT";
	}
}
