using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using WTG.ErrorReporting;

namespace Enterprise.ZArchitecture.Core
{
	internal class ExceptionQueuer
	{
		public ExceptionQueuer()
		{
		}

		public void QueueExceptionReportForLaterSending(string report, string initialTransmitStatus, bool useNewDbConnection)
		{
			QueueExceptionReportForLaterSendingCore(report, initialTransmitStatus, useNewDbConnection);
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception in error reporting.")]
		internal virtual void QueueExceptionReportForLaterSendingCore(string report, string initialTransmitStatus, bool useNewDbConnection)
		{
			var userInitials = GetUserInitials();

			DbConnection connection = null;
			try
			{
				connection = useNewDbConnection ? Db.NewExtraConnectionToMainDb() : Db.Connection;
				InsertStmErrorReport(connection, report, initialTransmitStatus, userInitials);

				const string serviceTaskCode = "RET";

				if (ObjectFactory.Get<IServiceManagerQuerier>().CheckStateOfNamedServiceTask(serviceTaskCode) == ServiceTaskStatus.AtLeastOneHostIsRunningHealthily)
				{
					ObjectFactory.Get<IServiceTaskNudger>().NudgeServiceTask(serviceTaskCode);
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				// if we cant even store this report then we are really screwed...
			}
			finally
			{
				if (connection != null && useNewDbConnection)
				{
					connection.Dispose();
				}
			}
		}

		#region Implementation

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void InsertStmErrorReport(DbConnection connection, string report, string initialTransmitStatus, string currentUserCode)
		{
			var sqlFormat = @"INSERT INTO [{0}]
([{1}], [{2}], [{3}], [{4}], [{5}], [{6}], [{7}] ,[{8}])
VALUES
(NEWID(), @reportType, @reportXml, @transmitStatus, SYSUTCDATETIME(), @currentUserCode, SYSUTCDATETIME(), @currentUserCode)";

			var sql = string.Format(
			  sqlFormat,
			  StmErrorReportSchema.Constants.TableName,
			  StmErrorReportSchema.Constants.PK,
			  StmErrorReportSchema.Constants.QER_ReportType,
			  StmErrorReportSchema.Constants.QER_ReportXml,
			  StmErrorReportSchema.Constants.QER_TransmitStatus,
			  StmErrorReportSchema.Constants.QER_SystemCreateTimeUtc,
			  StmErrorReportSchema.Constants.QER_SystemCreateUser,
			  StmErrorReportSchema.Constants.QER_SystemLastEditTimeUtc,
			  StmErrorReportSchema.Constants.QER_SystemLastEditUser);

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@reportType", SqlDbType.Int, (int)ErrorReportType.EnterpriseXml);
				cmd.AddParameter("@reportXml", SqlDbType.Xml, report);
				cmd.AddParameter("@transmitStatus", SqlDbType.Char, initialTransmitStatus);
				cmd.AddParameter("@currentUserCode", SqlDbType.VarChar, StmErrorReportSchema.QER_SystemCreateUser.MaxLength, string.IsNullOrEmpty(currentUserCode) ? "ZZ" : currentUserCode);

				cmd.ExecuteNonQuery();
			}
		}

		static string GetUserInitials()
		{
			try
			{
				if (EnvProxy.Instance != null && EnvProxy.Instance.CurrentUser != null)
				{
					return EnvProxy.Instance.CurrentUser.Initials;
				}
			}
			catch (Exception e) when (!e.IsCriticalException()) { } // If something goes wrong there's nothing we can do

			return string.Empty;
		}

		#endregion
	}
}
