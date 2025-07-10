using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IssueManager.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1058:TypesShouldNotExtendCertainBaseTypes")]
	public class ExceptionXml : XmlDocument
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public ExceptionXml(string xmlData)
		{
			this.KeyFields = new ExceptionKeyFields();
			try
			{
				using (var stringReader = new StringReader(xmlData))
				using (var reader = XmlTextReader.Create(stringReader, new XmlReaderSettings() { DtdProcessing = DtdProcessing.Ignore }))
				{
					Load(reader);
				}
			}
			catch (XmlException ex)
			{
				invalidXmlData = xmlData ?? "";
				HandleException(ex);
			}
		}

		#region Process

		public EdiHelpErrorLog Process(IHelpErrorLogCollection logs, bool shouldSave, bool isIssueError = false)
		{
			EdiHelpErrorLog log = null;

			PopulateFromXML(isIssueError);

			if (!IssueWorkItemCreator.IsFromOldSystem(exeDate))
			{
				var hasExistingIssue = true;
				log = KeyFields.MatchingLog(logs);
				if (log == null)
				{
					log = logs.AddNew(KeyFields.LogKey);
					log.HE_FirstProcessed = ZDateTime.UtcNow;
					KeyFields.CopyToLog(log);
					hasExistingIssue = false;
				}
				else
				{
					AutoReOpenIfFixedIssue(log);
				}

				var processedOcurrence = ProcessOccurrence(log, logs, shouldSave);

				if (log.HE_FixedDate.IsEmpty)
				{
					var formatter = new DataFormatter();

					formatter.CreateTable("KeyFields", new List<string> { "Name", "Value" });
					formatter.AddRowToTable("KeyFields", new List<string> { "Key", KeyFields.Key });
					formatter.AddRowToTable("KeyFields", new List<string> { "Type", KeyFields.Type });
					formatter.AddRowToTable("KeyFields", new List<string> { "Source", KeyFields.Source });

					IssueWorkItemCreator.LinkOrCreateIssueForWorkItemIfRequired(log, formatter, IssueAssignmentCalculator, ZDateTime.Today, processedOcurrence, !hasExistingIssue);
				}
			}

			return log;
		}

		void AutoReOpenIfFixedIssue(EdiHelpErrorLog log)
		{
			if (!log.HE_FixedDate.IsEmpty && log.HE_FixedDate < exeDate)
			{
				log.AutoReOpen();
			}
		}

		public IIssueAssignmentCalculator IssueAssignmentCalculator
		{
			get;
			set;
		} = new StackLinesWeightsLogAutoAssigner();

		#endregion

		public bool IsValid
		{
			get { return invalidXmlData == null; }
		}

		public string UserLoginName
		{
			get { return NodeText("LoginName"); }
		}

		public string ExceptionDescription
		{
			get { return NodeText("ExceptionDescription"); }
		}

		#region Implementation

		public string XmlData
		{
			get { return IsValid ? InnerXml : invalidXmlData; }
		}

		HelpErrorLogOccurrence ProcessOccurrence(EdiHelpErrorLog log, IHelpErrorLogCollection logs, bool shouldSave)
		{
			var occurrence = log.Factory.New<HelpErrorLogOccurrence>();
			occurrence.HO_HE = log.PK;
			AddXmlOccurrenceInfo(occurrence);

			if (log.ShouldCreateIncidentsForNewOccurrences)
			{
				log.CreateIncident(occurrence);
			}

			if (shouldSave)
			{
				try
				{
					logs.Save();
				}
				catch (ZSaveConcurrencyException ex)
				{
					throw new InvalidOperationException(new ConcurrencyExceptionHandler(ex).Info);
				}
			}

			return occurrence;
		}

		CultureInfo CultureInfoCompatibleWithExceptionReporter
		{
			get { return dateCultureInfo ?? (dateCultureInfo = CreateCultureInfoCompatibleWithExceptionReporter()); }
		}

		static CultureInfo CreateCultureInfoCompatibleWithExceptionReporter()
		{
			CultureInfo result = new CultureInfo("");
			result.DateTimeFormat.DateSeparator = "-";
			result.DateTimeFormat.TimeSeparator = ":";
			result.DateTimeFormat.ShortDatePattern = "dd/MMM/yy";
			result.DateTimeFormat.ShortTimePattern = "hh:mm:ss";
			return result;
		}

		public void PopulateFromXML(bool isIssueError = false)
		{
			if (IsValid)
			{
				try
				{
					ZString subject = NodeText("Subject");

					KeyFields.Key = NodeText("Key");
					KeyFields.Type = NodeText("ExceptionDetails/ExceptionType");
					KeyFields.Source = NodeText("ExceptionDetails/Source");
					KeyFields.AddMessage(NodeText("ExceptionMessage"));
					KeyFields.AddMessage(NodeText("ExceptionDetails/Message"));

					string innerExceptionPath = "ExceptionDetails";
					string rawSource = NodeText(innerExceptionPath);
					List<string> innerExceptions = new List<string>();

					while (true)
					{
						innerExceptionPath += "/InnerException";
						XmlNode node = SingleNode(innerExceptionPath);
						if (node != null)
						{
							KeyFields.Type = GetInnerNodeText(innerExceptionPath + "/ExceptionType", KeyFields.Type);
							KeyFields.Source = GetInnerNodeText(innerExceptionPath + "/Source", KeyFields.Source);

							string exceptionMessage = NodeText(innerExceptionPath + "/Message");
							if (!string.IsNullOrEmpty(exceptionMessage))
							{
								KeyFields.AddMessage(exceptionMessage);
								innerExceptions.Add(exceptionMessage);
							}
						}
						else
						{
							break;
						}
					}

					string sourceLine = GetAccurateCall(rawSource, innerExceptions).Trim();

					var children = SingleNode("ExceptionDetails/StackTrace");

					if (children != null)
					{
						foreach (XmlNode node in children.ChildNodes)
						{
							if (node.InnerText.Contains(sourceLine))
							{
								var assemblyAttribute = node.Attributes["Assembly"];
								if (assemblyAttribute != null)
								{
									KeyFields.Source = assemblyAttribute.InnerText;
								}
							}
						}
					}

					KeyFields.CallStack = CallStackText();
					KeyFields.IsClientVisible = !subject.StartsWith(ExceptionReporter.AutoGeneratedSubjectPrefix, StringComparison.OrdinalIgnoreCase);

					serverName = NodeText("DBServerName");
					company = NodeText("Company");
					exceptionTime = NodeDate("TimeOfException");
					sessionId = NodeGuid("SessionId", true);
					sequence = NodeInt("Sequence");
					versionNumber = NodeText("VersionNumber");
					exeDate = NodeDate("ExeCreationTime");

					exceptionID = NodeText("ErrorReportID");
					if (string.IsNullOrEmpty(exceptionID) || exceptionID.StartsWith("ReportIDFailed", StringComparison.OrdinalIgnoreCase))
					{
						exceptionID = "Unknown";
					}

					databaseName = NodeText("ExceptionDetails/EnvironmentInfo/DatabaseInfo/MainDatabaseName");
					licenceEnterpriseCode = NodeText("LicenceEnterpriseCode");
					licenceCompanyCode = NodeText("LicenceCompanyCode");
					licenceServerCode = NodeText("LicenceServerCode");

					testRigOrigin = NodeText("TestRigOrigin");
					fromTestRig = !string.IsNullOrEmpty(testRigOrigin);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					HandleException(ex);
				}
			}
			else
			{
				KeyFields.Type = typeof(XmlException).ToString();
				KeyFields.Source = "Container for all issues with invalid XML";
				KeyFields.Message = "Container for all issues with invalid XML";
				KeyFields.Key = "InvalidXml";
			}
		}

		string GetAccurateCall(string rawSource, List<string> innerExceptions)
		{
			string result = "UNKNOWN";
			if (string.IsNullOrWhiteSpace(rawSource))
			{
				return result;
			}

			bool isSeeking = true;
			string[] toSplitWith = new string[1] { " at" };
			string[] lines = rawSource.Split(toSplitWith, StringSplitOptions.RemoveEmptyEntries);

			if (lines.Length < 2)
			{
				return result;
			}

			for (int i = 1; i < lines.Length - 1; i++)
			{
				foreach (var inner in innerExceptions)
				{
					if (lines[i].Contains(inner))
					{
						isSeeking = true;
						innerExceptions.Remove(inner);
						break;
					}
				}

				if (!isSeeking || lines[i].Contains("System.") || lines[i].Contains("Exception") || lines[i].Contains("Error"))
				{
					continue;
				}
				else
				{
					try
					{
						result = lines[i];
						isSeeking = false;
					}
					catch (ArgumentOutOfRangeException)
					{
						continue;
					}
				}
			}

			return result;
		}

		string GetInnerNodeText(string node, string valueToUseIfInnerNodeIsEmpty)
		{
			string result = NodeText(node);
			return (!string.IsNullOrEmpty(result)) ? result : valueToUseIfInnerNodeIsEmpty;
		}

		void HandleException(Exception ex)
		{
			KeyFields.Type = ex.GetType().ToString();
			KeyFields.Message = ex.Message;
			KeyFields.Source = ex.Source;
			exceptionTime = ZDateTime.UtcNow;
		}

		void AddXmlOccurrenceInfo(HelpErrorLogOccurrence occurrence)
		{
			occurrence.HO_ServerName = serverName.Left(HelpErrorLogOccurrenceSchema.GetSchemaColumn(HelpErrorLogOccurrenceSchema.Constants.HO_ServerName).MaxLength);
			occurrence.HO_Company = company.Left(50);
			occurrence.HO_ExceptionDateTime = exceptionTime;
			occurrence.HO_VersionNumber = versionNumber;
			occurrence.HO_EXEDateTime = exeDate;
			occurrence.HO_XMLData = XmlData;
			occurrence.HO_ExceptionID = exceptionID;
			occurrence.HO_SessionID = sessionId;
			occurrence.HO_Sequence = sequence;

			if (!string.IsNullOrEmpty(versionNumber))
			{
				VersionNumber version = new VersionNumber(versionNumber);
				ZQuery releaseBuildQuery = new ZQuery(ReleaseBuildSchema.HL_MajorVersion, version.Major);
				releaseBuildQuery.AddToFilter(ReleaseBuildSchema.HL_MinorVersion, version.Minor);
				releaseBuildQuery.AddToFilter(ReleaseBuildSchema.HL_Release, version.Release);
				releaseBuildQuery.AddToFilter(ReleaseBuildSchema.HL_Patch, version.Patch);
				ReleaseBuild build = occurrence.Factory.LoadTop1<ReleaseBuild>(releaseBuildQuery);
				if (build != null)
				{
					occurrence.HO_HL = build.PK;
				}
			}

			var clientCompany = GetClientCompany(occurrence.Factory);
			if (clientCompany != null)
			{
				occurrence.HO_LCC = clientCompany.PK;
				occurrence.HO_LD = clientCompany.LCC_LD;
			}
			else
			{
				var licenceDatabase = GetLicenceDatabase(occurrence.Factory);
				if (licenceDatabase != null)
				{
					occurrence.HO_LD = licenceDatabase.PK;
				}
			}
		}

		LicenceDatabase GetLicenceDatabase(BusinessObjectFactory factory)
		{
			LicenceDatabase result = null;

			if (!string.IsNullOrEmpty(licenceEnterpriseCode) && !string.IsNullOrEmpty(licenceServerCode))
			{
				result = LicenceDatabase.LoadFromEnterpriseAndServerCode(factory, licenceEnterpriseCode, licenceServerCode);
			}

			if (result == null && !string.IsNullOrEmpty(company) && !string.IsNullOrEmpty(serverName) && !string.IsNullOrEmpty(databaseName))
			{
				result = factory.LoadTop1<LicenceHeader>(GetLicenceHeaderQuery())
					?.Database;
			}

			return result;
		}

		ZDBOnlyQuery GetLicenceHeaderQuery()
		{
			ZDBOnlySubQuery orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), LicenceCompanySchema.LC_OH);
			orgSubQuery.AddToFilter(OrgHeaderSchema.OH_FullName, company);

			ZDBOnlySubQuery databaseSubQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceHeaderSchema.LA_LD);
			databaseSubQuery.AddToFilter(LicenceDatabaseSchema.LD_HostServerName, serverName);
			databaseSubQuery.AddToFilter(LicenceDatabaseSchema.LD_HostDBName, databaseName);

			ZDBOnlySubQuery companySubQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceHeaderSchema.LA_LC);
			companySubQuery.AddSubQuery(orgSubQuery, JoinCondition.And);

			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(LicenceHeader));
			result.AddSubQuery(companySubQuery, JoinCondition.And);
			result.AddSubQuery(databaseSubQuery, JoinCondition.And);

			return result;
		}

		ClientCompany GetClientCompany(BusinessObjectFactory factory)
		{
			ClientCompany result = null;
			if (!string.IsNullOrEmpty(licenceEnterpriseCode) && !string.IsNullOrEmpty(licenceServerCode) && !string.IsNullOrEmpty(licenceCompanyCode))
			{
				result = ClientCompany.LoadFromLicenceCode(factory, licenceEnterpriseCode, licenceCompanyCode, licenceServerCode);
			}
			return result;
		}

		string NodeText(string xPath)
		{
			XmlNode node = SingleNode(xPath);
			return (node != null) ? node.InnerText : "";
		}

		int NodeInt(string xPath)
		{
			XmlNode node = SingleNode(xPath);
			int result;
			if (node == null || !int.TryParse(node.InnerText, out result))
			{
				result = -1;
			}
			return result;
		}

		ZGuid NodeGuid(string xPath, bool ignoreCase = false)
		{
			XmlNode node = SingleNode(xPath, ignoreCase);
			if (node == null || !ZGuid.TryParse(node.InnerText, out ZGuid result))
			{
				result = ZGuid.Empty;
			}

			return result;
		}

		ZDateTime NodeDate(string xPath)
		{
			XmlNode node = SingleNode(xPath);
			return ParseDateTimeFromXmlNode(node, CultureInfoCompatibleWithExceptionReporter);
		}

		internal static ZDateTime ParseDateTimeFromXmlNode(XmlNode node, CultureInfo cultureInfo)
		{
			var result = ZDateTime.Empty;
			if (node != null)
			{
				string text = node.InnerText;
				if (text.Length > 0)
				{
					DateTime d;
					if (text[text.Length - 1] == 'Z' && DateTime.TryParseExact(text, "u", CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
					{
						result = DateTime.SpecifyKind(d, DateTimeKind.Utc);
					}
					else if (DateTime.TryParse(text, cultureInfo ?? CreateCultureInfoCompatibleWithExceptionReporter(), DateTimeStyles.None, out d))
					{
						result = d.ToUniversalTime();
					}
				}
			}
			return result;
		}

		public string CallStackText()
		{
			return new CallStackReader(this).CallStackText(GetAppendStrategy());
		}

		IAppendStrategy GetAppendStrategy()
		{
			IAppendStrategy strategy = new KeyAppendStrategy();
			strategy = TryWrapAppendStrategyForGlowWebException(strategy);

			return strategy;
		}

		public XmlNode SingleNode(string xPath, bool ignoreCase = false)
		{
			if (DocumentElement != null)
			{
				var result = DocumentElement.SelectSingleNode(xPath);
				if (result == null && ignoreCase)
				{
					result = DocumentElement.ChildNodes.Cast<XmlNode>()
						.FirstOrDefault(x => string.Equals(x.Name, xPath, StringComparison.OrdinalIgnoreCase));
				}

				return result;
			}

			return null;
		}

		public readonly ExceptionKeyFields KeyFields;
		readonly string invalidXmlData;
		CultureInfo dateCultureInfo;

		protected ZString company;
		string databaseName;
		protected ZString serverName;
		protected string versionNumber;
		ZDateTime exceptionTime;
		ZGuid sessionId;
		int sequence = -1;
		protected ZDateTime exeDate;
		ZString exceptionID;
		string licenceEnterpriseCode;
		string licenceCompanyCode;
		string licenceServerCode;
		protected bool fromTestRig;
		protected string testRigOrigin;

		public ZDateTime ExceptionTime => exceptionTime;

		public string TestRigOrigin => testRigOrigin;

		#endregion

		#region Post-Processing

		IAppendStrategy TryWrapAppendStrategyForGlowWebException(IAppendStrategy inner)
		{
			// This is a dummy exception name that Glow Web provides, since Javascript errors don't have a type.
			if (!string.Equals(KeyFields.Type, "ProxiedJavascriptException", StringComparison.OrdinalIgnoreCase))
			{
				return inner;
			}

			var glowVersionNumber = NodeText("VersionNumber");
			if (glowVersionNumber.Length == 0)
			{
				return inner;
			}

			var requestURLText = NodeText("RequestURL");
			if (requestURLText.Length == 0)
			{
				return inner;
			}

			Uri requestURL;
			if (!Uri.TryCreate(requestURLText, UriKind.Absolute, out requestURL))
			{
				return inner;
			}

			return new GlowWebKeyAppendStrategy(inner, glowVersionNumber, requestURL);
		}

		#endregion
	}
}

