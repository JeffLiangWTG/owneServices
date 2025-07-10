using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Environment
{
	public class DisposableEnvironment : IDisposable
	{
		const string GlbBranchTypeName = "IGlbBranch";
		const string GlbCompanyTypeName = "IGlbCompany";

		public static IDisposable ForBranchCodeSlowerThanPK(
			string branchCode,
			bool reportInactive = true,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = -1)
		{
			return ForBranch(GetBranchPk(branchCode), reportInactive, callerFilePath, callerMemberName, callerLineNumber);
		}

		public static IDisposable ForBranch(
			Guid branchPK,
			bool reportInactive = true,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = -1)
		{
			if (branchPK == Guid.Empty && reportInactive)
			{
				ErrorReporter.ReportOnce("DisposableEnvironment_ForBranch_NoBranch", string.Format(CultureInfo.InvariantCulture, "Cannot find an active branch with code '{0}'.", branchPK));
				return null;
			}
			return new DisposableEnvironment(branchPK, User.ServiceUserName, callerFilePath, callerMemberName, callerLineNumber);
		}

		/// <summary>
		/// This may return null - when either the company code is invalid, or there are no active branches for the company.
		/// </summary>
		/// <param name="companyCode">Company code.</param>
		/// <param name="reportInactive">If true, error report will be generated if there is no active branch in specified company.</param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]

		public static IDisposable ForCompany(
			string companyCode,
			bool reportInactive = true,
			IUser user = null,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = -1)
		{
			var initialCurrentCompanyFromEnvironment = Env.Instance.CurrentCompany;
			var isUberFactory = initialCurrentCompanyFromEnvironment == null ? string.Empty : ((IFactoryProvider)Env.CurrentCompany).Factory.IsUberFactory.ToString();
			Guid branch = GetBranchPkFromCompanyCode(companyCode);
			if (branch == Guid.Empty)
			{
				if (reportInactive)
				{
					ErrorReporter.ReportOnce("DisposableEnvironment_ForCompany_NoBranch", string.Format(CultureInfo.InvariantCulture, "Cannot find an active branch for company with code '{0}'.\r\n{1}", companyCode, GetCompanyDetails()));
				}
				return null;
			}

			var env = new DisposableEnvironment(branch, user?.LoginName ?? User.ServiceUserName, callerFilePath, callerMemberName, callerLineNumber, reportMissingBranch: false);
			if (Env.Instance.CurrentBranch == null)
			{
				RowFactory.ClearSpecificTableFromUberFactory(GlbBranchSchema.Constants.TableName);
				RowFactory.ClearSpecificTableFromUberFactory(GlbCompanySchema.Constants.TableName);

				env.Dispose();
				branch = GetBranchPkFromCompanyCode(companyCode);

				if (branch == Guid.Empty)
				{
					if (reportInactive)
					{
						ErrorReporter.ReportOnce("DisposableEnvironment_ForCompany_NoBranch", string.Format(CultureInfo.InvariantCulture, "Cannot find an active branch for company with code '{0}'.\r\n{1}", companyCode, GetCompanyDetails()));
					}
					return null;
				}

				env = new DisposableEnvironment(branch, user?.LoginName ?? User.ServiceUserName, callerFilePath, callerMemberName, callerLineNumber, reportMissingBranch: false);
				if (Env.Instance.CurrentBranch == null && reportInactive)
				{
					ErrorReporter.ReportOnce("DisposableEnvironment_ForCompany_MissingBranchTwice",
						$"Company {companyCode} attempted to load a corresponding branch from DB twice but it was missing both times.\r\n{GetCompanyDetails()}");
				}
			}
			return env;

			string GetCompanyDetails()
			{
				var sb = new StringBuilder();
				sb.AppendLine($"initialCurrentCompanyFromEnvironment Exists: {initialCurrentCompanyFromEnvironment != null},  Code: {initialCurrentCompanyFromEnvironment?.Code}");
				sb.AppendLine($"initialCurrentCompanyFromEnvironment IsUberFactory: {isUberFactory}");
				sb.AppendLine($"companyCode:{companyCode}, branch:{branch}");
				sb.AppendLine($"Creating a new BusinessObjectFactory? {Env.CurrentCompany == null}");
				var factory = Env.CurrentCompany == null ? new BusinessObjectFactory() : ((IFactoryProvider)Env.CurrentCompany).Factory;

				var branchInFactory = factory.Load(ObjectFactory.GetType(GlbBranchTypeName), branch) as IBranch;
				sb.AppendLine($"Object Factory Type for IGlbBranch: {ObjectFactory.GetType(GlbBranchTypeName)}");
				sb.AppendLine($"BranchInFactory:{branchInFactory != null}, PK:{branchInFactory?.PK}, Code:{branchInFactory?.Code}, Company:{branchInFactory?.Company?.Code}, Active:{branchInFactory?.IsActive}, RowState:{(branchInFactory as INeedRow)?.Row.RowState}");
				sb.AppendLine(factory.GetDebugInformation("GlbBranch", branch));
				var company = Env.Instance.CurrentCompany;
				sb.AppendLine($"Env CurrentCompany:{company?.Code}, CurrentBranch:{Env.Instance.CurrentBranch?.Code}, PK:{Env.Instance.CurrentBranchPK}");

				foreach (var br in company?.Branches ?? new List<IBranch>())
				{
					sb.AppendLine($" - Branch in CurrentCompany PK:{br?.PK}, Code:{br?.Code}, IsActive:{br?.IsActive}");
				}

				sb.AppendLine($"The Company {companyCode} exist in GetActiveCompanies(): {GetActiveCompanies().Contains(companyCode)}");

				var companySqlText =
					$@"SELECT {GlbCompanySchema.Constants.GC_Code}, {GlbCompanySchema.Constants.GC_IsActive}, {GlbCompanySchema.Constants.GC_SystemCreateTimeUtc}, {GlbCompanySchema.Constants.GC_SystemLastEditUser}, {GlbCompanySchema.Constants.GC_SystemLastEditTimeUtc}
					FROM {GlbCompanySchema.Constants.SqlSchemaName}.{GlbCompanySchema.Constants.TableName}
					WHERE {GlbCompanySchema.Constants.GC_Code} = @CompanyCode";
				using (var commandCompany = Db.Connection.Command(companySqlText))
				{
					commandCompany.AddParameterBasedOnDbColumn("@CompanyCode", companyCode, GlbCompanySchema.GC_Code);
					using (var reader = commandCompany.ExecuteReader())
					{
						while (reader.Read())
						{
							sb.AppendLine($"InDB Company:{reader[0]}, IsActive:{reader[1]}, CreateTime:{reader[2]}, EditUser:{reader[3]}, EditTime:{reader[4]}");
						}
					}
				}

				var branchSqlText =
					$@"SELECT {GlbBranchSchema.Constants.PK}, {GlbBranchSchema.Constants.GB_Code}, {GlbBranchSchema.Constants.GB_IsActive}, {GlbBranchSchema.Constants.GB_SystemCreateTimeUtc}, {GlbBranchSchema.Constants.GB_SystemLastEditUser}, {GlbBranchSchema.Constants.GB_SystemLastEditTimeUtc}
					FROM {GlbBranchSchema.Constants.SqlSchemaName}.{GlbBranchSchema.Constants.TableName}
					JOIN {GlbCompanySchema.Constants.SqlSchemaName}.{GlbCompanySchema.Constants.TableName}
					ON {GlbBranchSchema.Constants.GB_GC} = {GlbCompanySchema.Constants.PK}
					WHERE {GlbCompanySchema.Constants.GC_Code} = @CompanyCode ORDER BY {GlbBranchSchema.Constants.GB_Code}";

				using (var command = Db.Connection.Command(branchSqlText))
				{
					command.AddParameterBasedOnDbColumn("@CompanyCode", companyCode, GlbCompanySchema.GC_Code);
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							sb.AppendLine($" - Branch PK:{reader[0]}, Code:{reader[1]}, IsActive:{reader[2]}, CreateTime:{reader[3]}, EditUser:{reader[4]}, EditTime:{reader[5]}");
						}
					}
				}
				return sb.ToString();
			}
		}

#if DEBUG
		[ThreadStatic]
		static Guid[] branchPKsToDeleteForTest;

		public static Guid[] BranchPKsToDeleteForTest
		{
			get => branchPKsToDeleteForTest;
			set => branchPKsToDeleteForTest = value;
		}

		[ThreadStatic]
		static bool deleteAllAtOnce;

		public static bool DeleteAllAtOnce
		{
			get => deleteAllAtOnce;
			set => deleteAllAtOnce = value;
		}
#endif

		public static IDisposable ForBranch(
			string branchCode,
			bool reportInactive = true,
			IUser user = null,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = -1)
		{
			Guid branch = GetBranchPk(branchCode);
			if (branch == Guid.Empty && reportInactive)
			{
				ErrorReporter.ReportOnce("DisposableEnvironment_ForBranch_NoBranch", string.Format(CultureInfo.InvariantCulture, "Cannot find an active branch with code '{0}'.", branchCode));
			}
			return branch != Guid.Empty ? new DisposableEnvironment(branch, user?.LoginName ?? User.ServiceUserName, callerFilePath, callerMemberName, callerLineNumber) : null;
		}

		public static string[] GetActiveCompanies(string inCountryCode = "")
		{
			return GetActiveCompanies(new[] { inCountryCode });
		}

		public static string[] GetActiveCompanies(IEnumerable<string> inCountryCodes)
		{
			var branchSubQuery = new ZDBOnlySubQuery(ObjectFactory.GetType(GlbBranchTypeName), GlbBranchSchema.GB_GC);
			branchSubQuery.AddToFilter(GlbBranchSchema.GB_IsActive, true);

			var groupLinkQuery = new ZDBOnlyQuery(ObjectFactory.GetType(GlbCompanyTypeName));
			groupLinkQuery.AddToFilter(JoinCondition.And, GlbCompanySchema.GC_IsActive, SQLComparisonOperator.Equal, true);

			var effectiveCountryCodes = inCountryCodes.Where(code => !String.IsNullOrEmpty(code));
			if (effectiveCountryCodes.Any())
			{
				groupLinkQuery.AddToFilter(JoinCondition.And, GlbCompanySchema.GC_RN_NKCountryCode, SQLComparisonOperator.Equal, effectiveCountryCodes);
			}

			groupLinkQuery.AddToFilter(JoinCondition.And, GlbCompanySchema.GC_Code, SQLComparisonOperator.NotEqual, "DEM");
			groupLinkQuery.AddSubQuery(GlbCompanySchema.PK, branchSubQuery, JoinCondition.And);

			var rowFactory = new RowFactory();
			var rows = rowFactory.Load(GlbCompanySchema.Constants.TableName, groupLinkQuery);

			return rows.Select(company => company[GlbCompanySchema.Constants.GC_Code].ToString()).ToArray();
		}

		public DisposableEnvironment(
			Guid branchPk,
			string loginName,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = -1,
			bool reportMissingBranch = true)
		{
#if DEBUG
			if (branchPKsToDeleteForTest != null)
			{
				if (DeleteAllAtOnce)
				{
					Db.Connection.ExecuteNonQuery($"DELETE FROM {GlbBranchSchema.Constants.SqlSchemaName}.{GlbBranchSchema.Constants.TableName} WHERE {GlbBranchSchema.Constants.PK} IN ('{string.Join("', '", BranchPKsToDeleteForTest)}')");
				}
				else if (BranchPKsToDeleteForTest.Any())
				{
					var pk = BranchPKsToDeleteForTest.First();
					BranchPKsToDeleteForTest = BranchPKsToDeleteForTest.Skip(1).ToArray();
					Db.Connection.ExecuteNonQuery($"DELETE FROM {GlbBranchSchema.Constants.SqlSchemaName}.{GlbBranchSchema.Constants.TableName} WHERE {GlbBranchSchema.Constants.PK} = '{pk}'");
				}
			}
#endif
			temporaryUserContext = Env.Instance.SetTemporaryUserContext(
				UserContext.SetupAndReturnUserContextForTemporarySwitch(loginName, branchPk, GetDepartmentPkWithFallbackToFirstActive(), factory: Env.CurrentCompany == null ? null : ((IFactoryProvider)Env.CurrentCompany).Factory, reportMissingBranch: reportMissingBranch),
				callerFilePath,
				callerMemberName,
				callerLineNumber);
		}

		static Guid GetBranchPk(string branchCode)
		{
			string sqlText = String.Format(
				"SELECT {0} FROM {1} WHERE {2} = @BranchCode",
				GlbBranchSchema.Constants.PK,
				GlbBranchSchema.Constants.TableName,
				GlbBranchSchema.Constants.GB_Code);
			using (DbCommand command = Db.Connection.Command(sqlText))
			{
				command.AddParameterBasedOnDbColumn("@BranchCode", branchCode, GlbBranchSchema.GB_Code);
				object guidAsObject = command.ExecuteScalar();
				return guidAsObject != null ? (Guid)guidAsObject : Guid.Empty;
			}
		}

		static Guid GetBranchPkFromCompanyCode(string companyCode)
		{
			string sqlText = String.Format(
				"SELECT TOP 1 {0} FROM {1} JOIN {2} ON {3} = {4} WHERE {5} = 1 AND {6} = @CompanyCode ORDER BY {7}",
				GlbBranchSchema.Constants.PK,
				GlbBranchSchema.Constants.TableName,
				GlbCompanySchema.Constants.TableName,
				GlbBranchSchema.Constants.GB_GC,
				GlbCompanySchema.Constants.PK,
				GlbBranchSchema.Constants.GB_IsActive,
				GlbCompanySchema.Constants.GC_Code,
				GlbBranchSchema.Constants.GB_Code);
			using (DbCommand command = Db.Connection.Command(sqlText))
			{
				command.AddParameterBasedOnDbColumn("@CompanyCode", companyCode, GlbCompanySchema.GC_Code);
				object guidAsObject = command.ExecuteScalar();
				return guidAsObject != null ? (Guid)guidAsObject : Guid.Empty;
			}
		}

		[ThreadSafe]
		protected static Guid brnDepartment;
		static Guid GetDepartmentPkWithFallbackToFirstActive()
		{
			if (brnDepartment != Guid.Empty)
			{
				return brnDepartment;
			}

			string sqlText = String.Format(
				CultureInfo.InvariantCulture,
				"SELECT {0} FROM {1} WHERE {2} = @DepartmentCode AND {3} =  @IsActive",
				GlbDepartmentSchema.Constants.PK,
				GlbDepartmentSchema.Constants.TableName,
				GlbDepartmentSchema.Constants.GE_Code,
				GlbDepartmentSchema.Constants.GE_IsActive);
			using (DbCommand command = Db.Connection.Command(sqlText))
			{
				command.AddParameterBasedOnDbColumn("@DepartmentCode", "BRN", GlbDepartmentSchema.GE_Code);
				command.AddParameterBasedOnDbColumn("@IsActive", true, GlbDepartmentSchema.GE_IsActive);
				object guidAsObject = command.ExecuteScalar();

				if (guidAsObject != null)
				{
					brnDepartment = (Guid)guidAsObject;
				}
				else
				{
					using (DbCommand firstActiveCommand = Db.Connection.Command(String.Format(CultureInfo.InvariantCulture, "SELECT TOP 1 {0} FROM {1} WHERE {2} = @IsActive ORDER BY {3}", GlbDepartmentSchema.Constants.PK, GlbDepartmentSchema.Constants.TableName, GlbDepartmentSchema.Constants.GE_IsActive, GlbDepartmentSchema.Constants.GE_Code)))
					{
						firstActiveCommand.AddParameterBasedOnDbColumn("@IsActive", true, GlbDepartmentSchema.GE_IsActive);
						brnDepartment = (Guid)(firstActiveCommand.ExecuteScalar() ?? Guid.Empty);
					}
				}

				return brnDepartment;
			}
		}

		readonly IDisposable temporaryUserContext;

		#region IDisposable

		public void Dispose()
		{
			Dispose(true);
			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SupressFinalize to
			// take this object off the finalization queue 
			// and prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}

		~DisposableEnvironment()
		{
			Dispose(false);
		}

		void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					temporaryUserContext.Dispose();
				}
			}
			disposed = true;
		}

		bool disposed;

		#endregion
	}
}
