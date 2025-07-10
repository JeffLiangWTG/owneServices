using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.CA.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.BatchProcessor
{
	public static class BatchProcessorUtilities
	{
		#region CompanyInCanada

		public static bool CompanyInCanada
		{
			get
			{
				if (!companyInCanada.HasValue)
				{
					companyInCanada = GlbCompany.GetActiveCompanies(Constants.CountryCodes.Canada).Any();
				}
				return companyInCanada.GetValueOrDefault();
			}
		}
		[ThreadStatic]
		static bool? companyInCanada;

		public static string CheckCompanyInCanadaOrAppliesAllCountries()
		{
			if (CompanyInCanada || CACustomsDataRegistry.Instance.RunServiceProviderClientServiceTaskAppliesAllCountries.Value)
			{
				return string.Empty;
			}

			var settingLocation = CACustomsDataRegistry.Instance.RunServiceProviderClientServiceTaskAppliesAllCountries.GetLocationInEnglish();
			return string.Format(CultureInfo.InvariantCulture, "There is no company in Canada, and the registry setting '{0}' has not been enabled.", settingLocation);
		}

		#endregion

		#region AllActiveComapnyBranchesMessageFilter

		public static ZQuery AllActiveComapnyBranchesMessageFilter
		{
			get
			{
				if (allActiveComapnyBranchesMessageFilter == null)
				{
					var factory = new BusinessObjectFactory();
					var filter = new ZQuery(GlbCompanySchema.GC_IsActive, true);
					filter.AddToFilter(GlbCompanySchema.GC_Code, SQLComparisonOperator.NotEqual, "DEM");
					var activeCompanies = factory.Load<GlbCompany>(filter);
					var branchPKs = from company in activeCompanies from branch in company.Branches select branch.PK;
					allActiveComapnyBranchesMessageFilter = new ZQuery(EDIMessageSchema.EM_GB, branchPKs);
				}
				return allActiveComapnyBranchesMessageFilter;
			}
		}

		[ThreadStatic]
		static ZQuery allActiveComapnyBranchesMessageFilter;

		#endregion

		#region CBSAClientID

		public static string CBSAClientID(bool isTest)
		{
			return isTest ? CACustomsDataRegistry.Instance.CBSATestNetworkIDAppliesAllCountries.Value : CACustomsDataRegistry.Instance.CBSAProdNetworkIDAppliesAllCountries.Value;
		}

		#endregion

		#region CBSAeHubID

		public static string CBSAeHubID(bool isTest)
		{
			return isTest ? CACustomsTest : CACustoms;
		}

		public static bool IsCBSAeHubID(string recipientId)
		{
			return recipientId == CACustomsTest || recipientId == CACustoms;
		}

		const string CACustomsTest = "CACustomsTest";
		const string CACustoms = "CACustoms";

		#endregion

		#region MailBoxID

		public static string MailBoxID
		{
			get
			{
				var result = CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.Value;
				return string.IsNullOrEmpty(result) ? CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.Value : result;
			}
		}

		#endregion

		#region Get Application Code & Message Type

		#region GetApplicationCode

		public static string GetApplicationCode(string interchangeText)
		{
			var ung = new UNGSegment(interchangeText);
			switch (ung.FuncGroupId)
			{
				case FunctionalGroupId.CUSRES:
					switch (ung.AppSenderId)
					{
						case "G7CCR":
							return EDIInterchange.ApplicationCodes.CAEXP;
						case "CCR":
							switch (ung.MessageVersion)
							{
								case "D:00A":
								case "D:11B":
									return EDIInterchange.ApplicationCodes.CAACI;
								case "D:96A":
								case "S:99B":
									return EDIInterchange.ApplicationCodes.CAIMP;
							}
							break;
						case "CCS":
						case K84AppSenderId.Overdue:
							switch (ung.MessageVersion)
							{
								case "S:99B":
									return EDIInterchange.ApplicationCodes.CAIMP;
							}
							break;
						case QueryMessageSubTypes.Codes.CLASSFILE:
						case QueryMessageSubTypes.Codes.TARIFFCODE:
						case QueryMessageSubTypes.Codes.GSTFILE:
						case QueryMessageSubTypes.Codes.EXCISETAX:
						case QueryMessageSubTypes.Codes.EXCHANGERATE:
						case QueryMessageSubTypes.Codes.QRCLASSTAR:
						case QueryMessageSubTypes.Codes.QREXCHANGE:
						case QueryMessageSubTypes.Codes.BROADCAST:
						case "CCSSYNTAX":
							return EDIInterchange.ApplicationCodes.CAIMP;
					}
					break;
				case FunctionalGroupId.GSIMEX:
					switch (ung.MessageVersion)
					{
						case "D:00A:EX1STP":
							return EDIInterchange.ApplicationCodes.CAEXP;
					}
					break;
				case FunctionalGroupId.CUSDEC:
					switch (ung.MessageVersion)
					{
						case "S:99B":
						case "D:96A":
							return EDIInterchange.ApplicationCodes.CAIMP;
					}
					break;
				case FunctionalGroupId.CUSREP:
					switch (ung.MessageVersion)
					{
						case "D:96A":
							return EDIInterchange.ApplicationCodes.CAIMP;
					}
					break;
				case FunctionalGroupId.GSMCAR:
					switch (ung.MessageVersion)
					{
						case "D:00A:SUPRPT":
							return EDIInterchange.ApplicationCodes.CAACI;
					}
					break;
				case FunctionalGroupId.GOVCBR:
					switch (ung.MessageVersion)
					{
						case "D:11B":
							return EDIInterchange.ApplicationCodes.CAACI;
					}
					break;
			}
			return EDIInterchange.ApplicationCodes.CACustoms;
		}

		#endregion

		#region GetMessageType

		public static string GetMessageType(string interchangeText, string bodyText = "")
		{
			var ung = new UNGSegment(interchangeText);
			switch (ung.FuncGroupId)
			{
				case FunctionalGroupId.GSIMEX:
					switch (ung.MessageVersion)
					{
						case "D:00A:EX1STP":
							return MessageTypeList.Codes.G7Export;
					}
					break;
				case FunctionalGroupId.CUSDEC:
					switch (ung.MessageVersion)
					{
						case "S:99B":
							switch (ung.AppRecipientId)
							{
								case "KI":
									return MessageTypeList.Codes.B3CUSDEC;
								case "QA":
								case "QE":
									return MessageTypeList.Codes.Query;
								case "TMG":
									return MessageTypeList.Codes.TestMessage;
							}
							switch (ung.AppSenderId)
							{
								case K84AppSenderId.Daily:
								case K84AppSenderId.Monthly:
								case K84AppSenderId.Overdue:
									return MessageTypeList.Codes.K84Report;
							}
							break;
						case "D:96A":
							return MessageTypeList.Codes.EDIRelease;
					}
					break;
				case FunctionalGroupId.CUSREP:
					switch (ung.MessageVersion)
					{
						case "D:96A":
							return MessageTypeList.Codes.RNSRequest;
					}
					break;
				case FunctionalGroupId.GSMCAR:
					switch (ung.MessageVersion)
					{
						case "D:00A:SUPRPT":
							return MessageTypeList.Codes.SupplementaryCargoReport;
					}
					break;
				case FunctionalGroupId.CUSRES:
					switch (ung.AppSenderId)
					{
						case "CCR":
							switch (ung.MessageVersion)
							{
								case "D:96A":
									return MessageTypeList.Codes.EDIRelease;
								case "S:99B":
									var documentName = new BGMSegment(bodyText).DocumentName;
									var result = ZString.Empty;
									if (documentName == "1000" || documentName == "2000")
									{
										result = MessageTypeList.Codes.TradeChainPartner;
									}
									else if (documentName == "1010" || documentName == "2010")
									{
										result = MessageTypeList.Codes.CSARevenueSummaryForm;
									}
									return result;
							}
							break;
						case "CCS":
							switch (ung.MessageVersion)
							{
								case "S:99B":
									return MessageTypeList.Codes.B3CUSDEC;
							}
							break;
						case "CCSSYNTAX":
							return MessageTypeList.Codes.SyntaxError;
						case QueryMessageSubTypes.Codes.CLASSFILE:
						case QueryMessageSubTypes.Codes.TARIFFCODE:
						case QueryMessageSubTypes.Codes.GSTFILE:
						case QueryMessageSubTypes.Codes.EXCISETAX:
						case QueryMessageSubTypes.Codes.QRCLASSTAR:
						case QueryMessageSubTypes.Codes.BROADCAST:
						case QueryMessageSubTypes.Codes.EXCHANGERATE:
						case QueryMessageSubTypes.Codes.QREXCHANGE:
							return MessageTypeList.Codes.Query;
						case K84AppSenderId.Overdue:
							return MessageTypeList.Codes.K84Report;
					}
					break;
				case FunctionalGroupId.GOVCBR:
					switch (ung.AppRecipientId)
					{
						case "ACIHGT":
						case "ACIHGP":
							return MessageTypeList.Codes.ACIHouseBill;
						case "ACIHCMGT":
						case "ACIHCMGP":
							return MessageTypeList.Codes.ACIForwarderClose;
					}
					break;
			}
			return EDIInterchange.ApplicationCodes.CACustoms;
		}

		#endregion

		#region GetMessageSubType

		public static string GetQueryMessageSubType(string interchangeText)
		{
			return new UNGSegment(interchangeText).AppSenderId;
		}

		public static string GetK84MessageSubType(string interchangeText)
		{
			switch (new UNGSegment(interchangeText).AppSenderId)
			{
				case K84AppSenderId.Daily:
					return K84ReportTypes.Codes.Daily;
				case K84AppSenderId.Monthly:
					return K84ReportTypes.Codes.Monthly;
				case K84AppSenderId.Overdue:
					return K84ReportTypes.Codes.Overdue;
			}
			return string.Empty;
		}

		#endregion

		internal static ZString GetAccountSecurityCode(ZString interchangeText)
		{
			return new ZString(new UNGSegment(interchangeText).ApplicationPassword).Left(5);
		}

		internal static ZString GetAccountSecurityCodeFromEDIMessage(Enterprise.Messaging.Business.EDIMessage message)
		{
			var result = CACustomsDataRegistry.Instance.AccountSecurityNo.Value;
			if (message.EM_LinkedObject is CusEntryHeader entryHeader)
			{
				var declaration = entryHeader.Declaration;
				if (declaration != null)
				{
					result = declaration.TransactionNumber.AccountSecurityCode;
				}
			}
			else if (message.EM_LinkedObject is OrgHeader orgHeader)
			{
				var importerAddInfo = OrgImpAddInfo.Get(orgHeader);
				if (importerAddInfo != null && importerAddInfo.HasAccountSecurityNumber)
				{
					result = importerAddInfo.ZO_AccountSecurityNumber;
				}
			}
			return result;
		}

		internal static ZString GetAccountSecurityPasswordFromEDIMessage(Enterprise.Messaging.Business.EDIMessage message)
		{
			var result = CACustomsDataRegistry.Instance.AccountSecurityNoPassword.Value;
			if (message.EM_LinkedObject is CusEntryHeader entryHeader)
			{
				var declaration = entryHeader.Declaration;
				if (declaration != null)
				{
					result = declaration.TransactionNumber.AccountSecurityPassword;
				}
			}
			else if (message.EM_LinkedObject is OrgHeader orgHeader)
			{
				var importerAddInfo = OrgImpAddInfo.Get(orgHeader);
				if (importerAddInfo != null && importerAddInfo.HasAccountSecurityNumber)
				{
					result = importerAddInfo.ZO_AccountSecirityPassword;
				}
			}
			return result;
		}

		#region UNGSegment

		class UNGSegment
		{
			public UNGSegment(string interchangeText)
			{
				match = GetUNGMatch(interchangeText);
			}

			#region Properties

			#region FuncGroupId

			public string FuncGroupId
			{
				get { return match.Groups["FuncGroupId"].Value; }
			}

			#endregion

			#region MessageVersion

			public string MessageVersion
			{
				get { return match.Groups["MessageVersion"].Value; }
			}

			#endregion

			#region AppSenderId

			public string AppSenderId
			{
				get { return match.Groups["AppSenderId"].Value; }
			}

			#endregion

			#region AppRecipientId

			public string AppRecipientId
			{
				get { return match.Groups["AppRecipientId"].Value; }
			}

			#endregion

			#region ApplicationPassword

			public string ApplicationPassword
			{
				get { return match.Groups["ApplicationPassword"].Value; }
			}

			#endregion

			#endregion

			#region GetUNGMatch

			Match GetUNGMatch(string interchangeText)
			{
				return Regex.Match(interchangeText, @"
					('|^)UNG
					\+(?<FuncGroupId>\w{0,6})
					\+(?<AppSenderId>
						(?<SenderId>.{0,35})
						(:(?<SenderQualifier>\w{0,4}))?)
					\+(?<AppRecipientId>
						(?<RecipientId>.{0,35})
						(:(?<RecipientQualifier>\w{0,4}))?)
					\+(?<PreparationDateTime>\w{6,8}:\d{4})
					\+(?<FuncGroupRefNumber>\w{0,14})
					\+(?<ControllingAgency>\w{0,2})
					\+(?<MessageVersion>
						(?<Version>\w{0,3})
						:(?<MessageRelease>\w{0,3})
						(:(?<AssociationCode>\w{0,6}))?)
					(\+(?<ApplicationPassword>.{0,14}))?('|$)
					", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
			}

			readonly Match match;

			#endregion
		}

		class BGMSegment
		{
			public BGMSegment(string bodyText)
			{
				match = GetBGMMatch(bodyText);
			}

			#region Properties

			public string DocumentName
			{
				get { return match.Groups["DocumentName"].Value; }
			}

			#endregion

			#region GetBGMMatch

			Match GetBGMMatch(string bodyText)
			{
				return Regex.Match(bodyText, @"
					('|^)BGM
					\+:::(?<DocumentMessageName>
						(?<DocumentName>\d{4}))
					\+(?<DocumentMessageIdentification>
						(?<DocumentMessageNumber>.{0,35}))
					", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
			}

			readonly Match match;

			#endregion
		}

		static class FunctionalGroupId
		{
			public const string CUSRES = "CUSRES";
			public const string CUSDEC = "CUSDEC";
			public const string GSIMEX = "GSIMEX";
			public const string GSMCAR = "GSMCAR";
			public const string CUSREP = "CUSREP";
			public const string GOVCBR = "GOVCBR";
		}

		static class K84AppSenderId
		{
			public const string Daily = "NOTICE";
			public const string Monthly = "K84";
			public const string Overdue = "OVERDUE REPORT";
		}

		#endregion

		#endregion

		#region DepositInterchangeInToFolderSafely

		public static bool DepositInterchangeInToFolderSafely(string fileName, EDIInterchange interchange, out string errorText)
		{
			return DepositDataIntoFolderSafely(fileName, interchange.EI_InterchangeText, out errorText);
		}

		public static bool DepositDataIntoFolderSafely(string fileName, ZString fileContents, out string errorText)
		{
			bool result;
			try
			{
				errorText = string.Empty;
				File.Delete(fileName);
				File.WriteAllText(fileName, fileContents);
				result = true;
			}
#pragma warning disable ENT0001
			catch (Exception e) when (e.IsFileException())
#pragma warning restore ENT0001
			{
				errorText = e.Message;
				result = false;
			}
			return result;
		}

		public static bool DepositDataIntoFolderSafely(string fileName, byte[] fileContents, out string errorText)
		{
			bool result;
			try
			{
				errorText = string.Empty;
				File.Delete(fileName);
				File.WriteAllBytes(fileName, fileContents);
				result = true;
			}
#pragma warning disable ENT0001
			catch (Exception e) when (e.IsFileException())
#pragma warning restore ENT0001
			{
				errorText = e.Message;
				result = false;
			}
			return result;
		}

		/// <summary>
		/// These are all the exceptions that are thrown by File.Delete, File.WriteAllBytes and File.WriteAllText
		/// </summary>
		//[ValidExceptionFilterMember] - this is the attribute required when dev analyzers are active
		static bool IsFileException(this Exception e)
		{
			return
				e is ArgumentException ||
				e is IOException ||
				e is NotSupportedException ||
				e is SecurityException ||
				e is UnauthorizedAccessException;
		}

		#endregion

		#region Registry

		internal static string RegistryLocation(IRegistryItemInternals registryItem)
		{
			return "Admin -> System -> Registry -> " + registryItem.Location;
		}

		internal static string BasicRegistryChecksForMessageSending(bool isTest, bool isCancelled)
		{
			var result = new ZStringBuilder();
			if (string.IsNullOrEmpty(BatchProcessorUtilities.CBSAClientID(isTest)))
			{
				result.Append(Res.GetString("b747ba6a-1e38-4d24-86b0-637dacfb8804", "The CBSA Client ID is not configured,"));
			}
			if (string.IsNullOrEmpty(BatchProcessorUtilities.MailBoxID))
			{
				result.Append(Res.GetString("f003f526-d62c-4c3b-adad-e90e49cd79b1", "The Network Client ID is not configured,"));
			}
			if (string.IsNullOrEmpty(CACustomsDataRegistry.Instance.TransmissionSite.Value))
			{
				result.Append(Res.GetString("0fe62e4b-ad3c-43b6-abcb-e80e98ccd92b", "The Transmission Site is not configured,"));
			}
			if (result.Length > 0)
			{
				result.Append(Res.GetString("daa7b17d-4265-428b-b464-3f6e278e7b51", "in the registry for Company - {0}, Branch - {1}. Please contact your System Administrator.", GlbCompany.CurrentCompany.GC_Code, GlbBranch.CurrentBranch.GB_Code));
			}
			if (isCancelled)
			{
				result.Append(Res.GetString("9B3D392D-5C06-4C74-B8A2-E23F29E7C6D0", "Sending messages for this job is not allowed as it has been marked as inactive. Please check why this job has been deactivated and, if required, mark as active so you can send messages (see 'Make Active' on the actions menu)."));
			}
			return result.ToStringWithNewLineBetweenAppends();
		}

		#endregion

		#region GetReturnEMailAddresses

		public static ZString[] GetReturnEMailAddresses(ZString interchangeClientID, OrgHeader clientOrg)
		{
			List<ZString> result = new List<ZString>();
			var contacts = Array.Empty<OrgContact>();
			var clientQuery = new ZQuery(OrgContactSchema.OC_OH, clientOrg.PK);
			if (interchangeClientID == BatchProcessorUtilities.CBSAClientID(false))
			{
				clientQuery.AddToFilter(OrgContactSchema.OC_ContactName, SQLComparisonOperator.StartsWith, "#CAP");
				contacts = clientOrg.Factory.Load<OrgContact>(clientQuery);
			}
			else if (interchangeClientID == BatchProcessorUtilities.CBSAClientID(true))
			{
				clientQuery.AddToFilter(OrgContactSchema.OC_ContactName, SQLComparisonOperator.StartsWith, "#CAT");
				contacts = clientOrg.Factory.Load<OrgContact>(clientQuery);
			}
			if (contacts.Length > 0)
			{
				foreach (OrgContact contact in contacts)
				{
					if (!contact.OC_Email.IsEmpty)
					{
						result.Add(contact.OC_Email);
					}
				}
			}
			if (result.Count == 0 && clientOrg.MainAddress != null && !clientOrg.MainAddress.OA_Email.IsEmpty)
			{
				result.Add(clientOrg.MainAddress.OA_Email);
			}
			return result.ToArray();
		}

		#endregion

		#region Testing
#if DEBUG

		public static void ResetCompanyInCanadaForTesting()
		{
			companyInCanada = null;
		}

		public static void ResetValidACIBranchesForTesting()
		{
			allActiveComapnyBranchesMessageFilter = null;
		}

#endif
		#endregion
	}
}
