using System;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Core
{
	public abstract class ExceptionBuilder
	{
		protected ExceptionBuilder(ExceptionReportArgs reportArgs, string errorTime)
		{
			ReportArgs = reportArgs;
			ErrorTime = errorTime;
		}

		public string ErrorReportID
		{
			get { return ReportArgs.ErrorReportID; }
		}

		public bool IsValidErrorReportId
		{
			get { return ErrorReportID != InvalidReportId; }
		}

		protected const string InvalidReportId = "ReportIDFailed";

		protected string ErrorTime { get; private set; }

		protected ExceptionReportArgs ReportArgs { get; private set; }

		protected string NewLine
		{
			get { return System.Environment.NewLine; }
		}

		protected internal string VersionNumber
		{
			get { return ReleaseInfo.Instance.VersionNumber.ToString(); }
		}

		protected internal string ExeDateTimeCreated
		{
			get
			{
				if (exeDateTimeCreated == null)
				{
					exeDateTimeCreated = EnvProxy.Instance.Time.FormatDateTimeWithSeconds(ReleaseInfo.Instance.ExeDate);
				}
				return exeDateTimeCreated;
			}
		}

		string exeDateTimeCreated;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception string")]
		protected string Subject
		{
			get
			{
				var currentCompany = GetCurrentCompany();

				string result = "Exception Report (" + ErrorReportID + ") in " + ActiveFormInfo + " from " + (currentCompany != null ? currentCompany.Name : "UNKNOWN") + " - " + System.Environment.UserName;
				if (!string.IsNullOrEmpty(ReportArgs.SubjectPrefix))
				{
					result = ReportArgs.SubjectPrefix + ' ' + result;
				}
				return result;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception string")]
		protected string LoginName
		{
			get
			{
				var currentUser = GetCurrentUser();
				return (currentUser == null) ? "Unknown Login" : (currentUser.IsDeveloperLogin ? "Developer Login" : currentUser.LoginName);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception string")]
		protected string RegNo1
		{
			get
			{
				var currentCompany = GetCurrentCompany();
				return (currentCompany != null) && (currentCompany.BusinessRegNo1 != null) ? currentCompany.BusinessRegNo1 : "Not specified";
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception string")]
		protected string RegNo2
		{
			get
			{
				var currentCompany = GetCurrentCompany();
				return (currentCompany != null) && (currentCompany.BusinessRegNo2 != null) ? currentCompany.BusinessRegNo2 : "Not specified";
			}
		}

		protected ICompany GetCurrentCompany()
		{
			ICompany currentCompany = null;
			try
			{
				currentCompany = EnvProxy.Instance.CurrentCompany;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
			}

			return currentCompany;
		}

		protected IUser GetCurrentUser()
		{
			IUser currentUser = null;
			try
			{
				currentUser = EnvProxy.Instance.CurrentUser;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
			}

			return currentUser;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected string GetAllCompanyNamesFromDatabase()
		{
			StringBuilder result = new StringBuilder();
			using (var reader = Db.Connection.Command("select GC_Name from dbo.GlbCompany where GC_Code != 'DEM'").ExecuteReader())
			{
				while (reader.Read())
				{
					result.AppendLine(reader["GC_Name"].ToString());
				}
			}

			return result.ToString();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CW1161:Do not use constant string literals - use resource strings instead.", Justification = "Database query")]
		protected string GetTestRigOriginFromDatabase()
		{
			using (var cmd = Db.Connection.Command("SELECT CAST(SD_BinaryValue AS NVARCHAR) FROM dbo.StmData WHERE SD_Name = @name"))
			{
				cmd.AddParameterBasedOnDbColumn("@name", "TEST_RIG_ORIGIN", StmDataSchema.SD_Name);
				var origin = (string)cmd.ExecuteScalar();

				return origin;
			}
		}

		string ActiveFormInfo => ObjectFactory.Get<IFormsErrorReportDetailsProvider>().LastActiveFormInfo;
	}
}
