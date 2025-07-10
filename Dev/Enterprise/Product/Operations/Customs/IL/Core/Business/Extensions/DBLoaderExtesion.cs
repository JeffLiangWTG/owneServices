using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IL.Business
{
	public static class DBLoaderExtesion
	{
		public static (GlbILStaffExternalPassword, ZQuery) FetchStaffExternalPassword(this BusinessObjectFactory factory, string staffCode)
		{
			var sqGlbStaff = new ZDBOnlySubQuery(typeof(GlbStaff), GlbExternalPasswordSchema.GP_GS);
			sqGlbStaff.AddToFilter(GlbStaffSchema.GS_IsActive, true);
			sqGlbStaff.AddToFilter(GlbStaffSchema.GS_Code, staffCode);

			var query = new ZDBOnlyQuery(typeof(GlbExternalPassword));
			query.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.ILS);
			query.AddToFilter(GlbExternalPasswordSchema.GP_CertificateAuthority, SQLComparisonOperator.NotEqual, string.Empty);
			query.AddToFilter(GlbExternalPasswordSchema.GP_UserID, SQLComparisonOperator.NotEqual, string.Empty);
			query.AddToFilter(GlbExternalPasswordSchema.GP_CurrentPassword, SQLComparisonOperator.NotEqual, string.Empty);
			query.AddSubQuery(sqGlbStaff, JoinCondition.And);

			return (
				staffCode.IsNullOrEmpty()
				? (null, query)
				: (factory.LoadTop1<GlbILStaffExternalPassword>(query), query));
		}

		public static (GlbILStaffExternalPassword, ZQuery) FetchStaffExternalPasswordForManager(this BusinessObjectFactory factory, string staffCode)
		{
			var sqGlbStaff = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffManagerSchema.GSM_GS_Staff);
			sqGlbStaff.AddToFilter(GlbStaffSchema.GS_IsActive, true);
			sqGlbStaff.AddToFilter(GlbStaffSchema.GS_Code, staffCode);

			var sqGlbStaffManager = new ZDBOnlySubQuery(typeof(GlbStaffManager), GlbStaffManagerSchema.GSM_GS_Manager);
			sqGlbStaffManager.AddToFilter(GlbStaffManagerSchema.GSM_ManagerType, DefaultStaffReportingRoles.Codes.DirectManager);
			sqGlbStaffManager.AddToFilter(GlbStaffManagerSchema.GSM_IsApproved, true);
			sqGlbStaffManager.AddToFilter(GlbStaffManagerSchema.GSM_EffectiveDate, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.Today);
			var endDateQuery = new ZQuery();
			endDateQuery.AddToFilter(GlbStaffManagerSchema.GSM_EndDate, DBNull.Value);
			endDateQuery.AddToFilter(JoinCondition.Or, GlbStaffManagerSchema.GSM_EndDate, SQLComparisonOperator.GreaterThan, ZDateTime.Today);
			sqGlbStaffManager.AddToFilter(endDateQuery, JoinCondition.And);

			sqGlbStaffManager.AddSubQuery(GlbStaffManagerSchema.GSM_GS_Staff, sqGlbStaff, JoinCondition.And);
			sqGlbStaffManager.AddSubQuery(sqGlbStaff, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(GlbExternalPassword));
			query.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.ILS);
			query.AddToFilter(GlbExternalPasswordSchema.GP_CertificateAuthority, SQLComparisonOperator.NotEqual, string.Empty);
			query.AddToFilter(GlbExternalPasswordSchema.GP_UserID, SQLComparisonOperator.NotEqual, string.Empty);
			query.AddToFilter(GlbExternalPasswordSchema.GP_CurrentPassword, SQLComparisonOperator.NotEqual, string.Empty);

			query.AddSubQuery(GlbExternalPasswordSchema.GP_GS, sqGlbStaffManager, JoinCondition.And);

			return (
				staffCode.IsNullOrEmpty()
				? (null, query)
				: (factory.LoadTop1<GlbILStaffExternalPassword>(query), query));
		}
	}
}
