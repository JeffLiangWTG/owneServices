using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Core
{
	public class ExceptionReportBuilder : ExceptionBuilder
	{
		public ExceptionReportBuilder(ExceptionReportArgs reportArgs)
			: base(reportArgs, GenerateErrorTime())
		{
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Exception report")]
		public string GenerateReport()
		{
			var userEventTracker = ObjectFactory.Get<IUserEventTracker>();
			var userSqlEventInfo = userEventTracker.SqlEventDescription;
			var userSqlFailedEventInfo = userEventTracker.SqlFailedEventDescription;
			var userConnectionEventInfo = userEventTracker.DatabaseConnectionEventDescription;
			var previousErrorReported = PreviousExceptionsStackTrace;

			using (Db.DisposableActionForDbConnection())
			using (StringWriter strWriter = new StringWriter())
			using (XmlTextWriter xtw = new XmlTextWriter(strWriter))
			{
				xtw.WriteStartElement("EDI_Exception_Report");

				try
				{
#if DEBUG
					if (Globals.IsTest && TestActionHandler != null)
					{
						TestActionHandler(this, EventArgs.Empty);
					}
#endif

					IEnvironment env = EnvProxy.Instance;

					xtw.WriteElementString("ErrorReportID", ErrorReportID);
					xtw.WriteElementString("Subject", Subject);
					xtw.WriteElementString("Key", ReportArgs.Key);
					xtw.WriteElementString("LoginName", LoginName);
					xtw.WriteElementString("DBServerName", DatabaseServerName);
					xtw.WriteElementString("MachineName", System.Environment.MachineName);
					xtw.WriteElementString("MachineLocalUserName", System.Environment.UserName);

					var currentUser = GetCurrentUser();
					xtw.WriteElementString("UsersEmailAddress", (currentUser != null) ? currentUser.EmailAddress : string.Empty);

					var currentCompany = GetCurrentCompany();
					if (currentCompany != null)
					{
						xtw.WriteElementString("Company", currentCompany.Name);
						xtw.WriteElementString("CompanyCountryCode", currentCompany.Country.Code);
						xtw.WriteElementString("RegistrationNo1", RegNo1);
						xtw.WriteElementString("RegistrationNo2", RegNo2);
					}

					var testRigOrigin = GetTestRigOriginFromDatabase();
					if (!string.IsNullOrEmpty(testRigOrigin))
					{
						xtw.WriteElementString("TestRigOrigin", testRigOrigin);
					}

					using (env.SuspendBranchAccessError())
					{
						IBranch currentBranch = null;
						try
						{
							currentBranch = env.CurrentBranch;
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
						}

						if (currentBranch != null)
						{
							xtw.WriteElementString("Branch", currentBranch.Name);
							xtw.WriteElementString("BranchPhone", currentBranch.Phone);
						}
					}

					IDepartment currentDepartment = null;
					try
					{
						currentDepartment = env.CurrentDepartment;
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
					}

					if (currentDepartment != null)
					{
						xtw.WriteElementString("Department", currentDepartment.Description);
					}

					var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
					if (!string.IsNullOrEmpty(registrationKey.EnterpriseCode))
					{
						xtw.WriteElementString("LicenceEnterpriseCode", registrationKey.EnterpriseCode);
						if (currentCompany != null)
						{
							xtw.WriteElementString("LicenceCompanyCode", currentCompany.Code);
						}
						xtw.WriteElementString("LicenceServerCode", registrationKey.ServerCode);
					}

					xtw.WriteElementString("TimeOfException", ErrorTime);
					if (ReportArgs.SessionId != Guid.Empty)
					{
						xtw.WriteElementString("SessionId", ReportArgs.SessionId.ToString());
					}
					if (ReportArgs.Sequence != -1)
					{
						xtw.WriteElementString("Sequence", ReportArgs.Sequence.ToString());
					}
					xtw.WriteElementString("ShutdownEnterprise", ReportArgs.ShutDownApplication ? "Yes" : "No");
					xtw.WriteElementString("VersionNumber", VersionNumber);
					xtw.WriteElementString("ExeCreationTime", ExeDateTimeCreated);
					xtw.WriteElementString("Source", ReportArgs.Ex.Source);

					IWinFormsEnvironment winFormsEnv = env as IWinFormsEnvironment;
					if (winFormsEnv != null)
					{
						xtw.WriteElementString("CurrentModule", winFormsEnv.CurrentModule);
					}

					xtw.WriteElementString("ExceptionMessage", ReportArgs.Ex.Message);
					xtw.WriteRaw(EscapeNulls(ObjectFactory.Get<IFormsErrorReportDetailsProvider>().GetDetails()));
					WriteCommandLineInformation(xtw);
					xtw.WriteElementString("ExceptionDescription", ReportArgs.ErrorDescription);
					GetExceptionDetails(ReportArgs.Ex).WriteFullReport(xtw);
					WriteFactoryDebugInfo(xtw);

					xtw.WriteRaw(EscapeNulls(ObjectFactory.Get<IUserEventTracker>().UserEventDescription));
					xtw.WriteRaw(EscapeNulls(userSqlEventInfo));
					xtw.WriteRaw(EscapeNulls(userSqlFailedEventInfo));
					xtw.WriteRaw(EscapeNulls(userConnectionEventInfo));
					xtw.WriteRaw(EscapeNulls(previousErrorReported));

					xtw.WriteEndElement();

					xtw.Flush();

					return strWriter.GetStringBuilder().ToString();
				}
				catch (Exception ex) when (!ex.IsCriticalException() && !ex.IsOutOfDiskSpaceException())
				{
					using (StringWriter errorStrWriter = new StringWriter())
					using (XmlTextWriter errorXtw = new XmlTextWriter(errorStrWriter))
					{
						errorXtw.WriteStartElement("EDI_Exception_Report");
						errorXtw.WriteElementString("Error", "Failed to get report:" + NewLine + ex.Message);
						errorXtw.WriteElementString("Inner_Error", strWriter.GetStringBuilder().ToString());
						errorXtw.WriteEndElement();

						errorXtw.Flush();

						return errorStrWriter.GetStringBuilder().ToString();
					}
				}
			}
		}

		internal static string EscapeNulls(string input)
		{
			//Error	43	Character ' ', hexadecimal value 0x0 is illegal in XML documents.
			//only needs to be used on things that are sent to WriteRaw - you can send damn near anything into WriteElementString and it produces valid xml as far as I can tell
			//technically there's some other things we'd need to escape (like raw &s) but those are trickier (since we don't want to re-escape &amp; and so on)
			return input.Replace("\0", "\\0");
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message to be sent to us")]
		void WriteFactoryDebugInfo(XmlTextWriter xtw)
		{
			xtw.WriteStartElement("FactoryDebugInfo");
			try
			{
				var stats = new PerformanceStatistic();
				var mainFactoryStats = stats.FactoryStatistics.Where(x => x.IncludeWithOtherFactoriesForIssueReport);
				var otherFactoryStats = stats.FactoryStatistics.Where(x => !x.IncludeWithOtherFactoriesForIssueReport);
				xtw.WriteStartElement("FactoryStatistics");
				try
				{
					foreach (BusinessObjectFactoryStatistic factoryStat in mainFactoryStats)
					{
						WriteFactoryStatInfo(factoryStat, xtw);
					}
				}
				finally
				{
					xtw.WriteEndElement();
				}

				if (!otherFactoryStats.IsNullOrEmpty())
				{
					xtw.WriteStartElement("OtherFactoryStatistics");
					try
					{
						foreach (BusinessObjectFactoryStatistic factoryStat in otherFactoryStats)
						{
							WriteFactoryStatInfo(factoryStat, xtw);
						}
					}
					finally
					{
						xtw.WriteEndElement();
					}
				}

				xtw.WriteStartElement("UncollectedTypes");
				try
				{
					foreach (UncollectedType uncollectedType in stats.UncollectedTypes)
					{
						WriteUncollectedTypeInfo(uncollectedType, xtw);
					}
				}
				finally
				{
					xtw.WriteEndElement();
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				xtw.WriteString("Failed to get details:" + NewLine + ex.Message);
			}
			finally
			{
				xtw.WriteEndElement();
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Internal-only description of factory visibility, Exception message to be sent to us")]
		void WriteFactoryStatInfo(BusinessObjectFactoryStatistic factoryStat, XmlTextWriter xtw)
		{
			xtw.WriteStartElement("FactoryStatistic");
			try
			{
				// Error reporter will sweep through all factories in the application, and we need to make sure that thread checks are honored
				// since we cannot guarantee that we will be accessing all factories from their creation thread
				using (Db.DisposableActionForDbConnection())
				{
					var accessible = factoryStat.IsOwnedByCurrentThread;
					var currentThread = Thread.CurrentThread.ManagedThreadId;
					var inacessibleMsg = string.Format(CultureInfo.InvariantCulture, "*inaccessible from error reporter thread #{0}*", currentThread);

					xtw.WriteElementString("Name", factoryStat.Name);
					xtw.WriteElementString("ThreadID", factoryStat.CreationThreadID.ToString());
					xtw.WriteElementString("ActiveFetchHintsCount", accessible ? factoryStat.ActiveFetchHintsCount.ToString() : inacessibleMsg);
					xtw.WriteElementString("BusinessObjectCount", accessible ? factoryStat.BusinessObjectCount.ToString() : inacessibleMsg);
					xtw.WriteElementString("ChildFactoriesCount", accessible ? factoryStat.ChildFactoriesCount.ToString() : inacessibleMsg);
					xtw.WriteElementString("ChildFactoryIDs", accessible ? factoryStat.ChildFactoryIDs : inacessibleMsg);
					xtw.WriteElementString("DatabaseLoadCount", accessible ? factoryStat.DatabaseLoadCount.ToString() : inacessibleMsg);
					xtw.WriteElementString("DataRowCount", accessible ? factoryStat.DataRowCount.ToString() : inacessibleMsg);
					xtw.WriteElementString("FactoryInstance", ((BusinessObjectFactory)factoryStat.FactoryInternals)._Instance.ToString(CultureInfo.InvariantCulture));
					xtw.WriteElementString("FactoryCreationTime", factoryStat.FactoryCreationTime.ToString());
					xtw.WriteElementString("AllocationPath", factoryStat.AllocationPath);

					xtw.WriteStartElement("TrackedInstances");
					try
					{
						if (accessible)
						{
							var bizosByType = factoryStat.FactoryInternals.AllBusinessObjects.GroupBy(b => b.GetType());
							foreach (var bizoType in bizosByType)
							{
								xtw.WriteStartElement("Bizo");
								xtw.WriteElementString("Type", bizoType.Key.FullName);
								xtw.WriteElementString("Count", bizoType.Count().ToString());
								xtw.WriteEndElement();
							}
						}
						else
						{
							xtw.WriteString(inacessibleMsg);
						}
					}
					finally
					{
						xtw.WriteEndElement();
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				xtw.WriteString("Failed to get statistic details:" + NewLine + ex.Message);
			}
			finally
			{
				xtw.WriteEndElement();
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message to be sent to us")]
		void WriteUncollectedTypeInfo(UncollectedType uncollectedType, XmlTextWriter xtw)
		{
			xtw.WriteStartElement("UncollectedType");
			try
			{
				xtw.WriteElementString("TypeName", uncollectedType.TypeName.ToString());
				xtw.WriteElementString("Quantity", uncollectedType.Quantity.ToString());
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				xtw.WriteString("Failed to get uncollected type details:" + NewLine + ex.Message);
			}
			finally
			{
				xtw.WriteEndElement();
			}
		}

#if DEBUG
		internal EventHandler TestActionHandler;
#endif

		#region Implementation

		protected virtual ExceptionDetails GetExceptionDetails(Exception ex)
		{
			return new ExceptionDetails(ex);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message to be sent to developers")]
		void WriteCommandLineInformation(XmlTextWriter xtw)
		{
			xtw.WriteStartElement("CommandLine");

			try
			{
				xtw.WriteString(System.Environment.CommandLine);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				xtw.WriteString("Failed to get command line information:" + NewLine + ex.Message);
			}

			xtw.WriteEndElement();
		}

		static string GenerateErrorTime()
		{
			DateTime utcNow;

			try
			{
				utcNow = EnvProxy.Instance.Time.CurrentUtcDateTime;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				utcNow = DateTime.UtcNow; // This is a last resort if DB time cannot be established
			}

			return utcNow.ToString("u");
		}

		protected string DatabaseServerName
		{
			get
			{
				if (databaseServerName == null)
				{
					try
					{
						databaseServerName = Db.Connection.ServerNameReportedByDatabase;
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						databaseServerName = Db.ServerName;
					}
				}

				return databaseServerName;
			}
		}
		string databaseServerName;

		public static string PreviousExceptionsStackTrace => GetPreviousExceptionsThrown(ErrorReporter.LastExceptionsReported());

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Exception message")]
		static string GetPreviousExceptionsThrown(List<string> list)
		{
			Argument.NotNull(list, nameof(list)); // Suggested By ReviewBot 

			StringWriter strWriter = new StringWriter();
			XmlTextWriter xtw = new CustomXmlWriter(strWriter);

			xtw.WriteStartElement("PreviousExceptions");
			{
				xtw.WriteElementString("Count", list.Count.ToString());

				foreach (string exceptionMessage in list)
				{
					xtw.WriteElementString("Exception", exceptionMessage);
				}
			}
			xtw.WriteEndElement();

			return strWriter.GetStringBuilder().ToString();
		}

		#endregion
	}
}
