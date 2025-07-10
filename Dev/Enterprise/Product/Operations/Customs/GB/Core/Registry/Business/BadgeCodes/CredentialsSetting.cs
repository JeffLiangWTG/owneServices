using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Registry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.Registry
{
	[DebuggerDisplay("CredentialsSetting. Badge={BadgeCode}; User={Username}; Pwd={Password}; Printer={Printer}; Co={Company}")]
	[XmlSerializerAssembly("Enterprise.Customs.GB.Registry.XmlSerializers")]
	public class CredentialsSetting : RegistryBusinessObjectTemplate, System.Net.ICredentials
	{
		protected abstract class Schema
		{
			public const string BadgeCode = "BadgeCode";
			public const string Username = "Username";
			public const string Password = "Password";
			public const string Printer = "Printer"; // Maintained as old name for compatibility with existing data in rego xml
			public const string Company = "Company";  // Maintained as old name for compatibility with existing data in rego xml
			public const string FallbackForShed = "FallbackForShed";
			public const string PreferredAgent = "PreferredAgent";
			public const string CcsukFallbackAgentType = "CcsukFallbackAgentType";
			public const string WebServiceFailureCount = "WebServiceFailureCount";
			public const string IsMaritimeLoader = "IsMaritimeLoader";
			public const string DataTestStatus = "DataTestStatus";
			public const string Endpoint = "Endpoint";

			public const string SenderID = "SenderID";  // e.g.   PNTWTGDOVSND or PNTWTGDOVS
			public const string ReceiverID = "ReceiverID";  // e.g.   PNTWTGDOVRCV or PNTWTGDOVR
		}

		#region Constructions and cloning
		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CredentialsSetting(fallbackLevel, factory);
		}

		public CredentialsSetting()
			: base()
		{
		}

		public CredentialsSetting(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
			localFactory = factory;
		}
		#endregion

		#region BadgeCode

		public ZString CSP => Badge?.CSPCode ?? ZString.Empty;

		BadgeCodeSetting Badge
		{
			get => ExistingBadgesRaw.Where(t => t.Item1.BadgeCode == BadgeCode).Select(x => x.Item1).FirstOrDefault();
		}

		ZString fBadgeCode;

		/// <summary>
		/// Badge MNEMONIC
		/// </summary>
		[MaxLength(3)]
		public ZString BadgeCode
		{
			get { return fBadgeCode; }
			set
			{
				bool hasChanged = false;
				if (value != BadgeCode)
				{
					hasChanged = true;
				}
				SetNonPersistentPropertyValue(BadgeCodeInfo, ref fBadgeCode, value);
				if (!IsValidationSuspended)
				{
					ValidateBadge();
				}
				if (hasChanged)
				{
					SetDataTestStateDirty();
				}
			}
		}

		public CodeDescriptionPairList ExistingBadges
		{
			get
			{
				CodeDescriptionPairList plausibleBadges = new CodeDescriptionPairList();
				foreach (Tuple<BadgeCodeSetting, ZString> pair in ExistingBadgesRaw)
				{
					var badge = pair.Item1;
					plausibleBadges.AddPairIfNotExist(badge.BadgeCode, string.Format(CultureInfo.InvariantCulture, "{0} ({1}, {2})", badge.BadgeCode, badge.CSPCode, pair.Item2));
				}
				return plausibleBadges;
			}
		}

		List<Tuple<BadgeCodeSetting, ZString>> ExistingBadgesRaw
		{
			get
			{
				var plausibleBadges = new List<Tuple<BadgeCodeSetting, ZString>>();
				Dictionary<ZGuid, ZString> allbranchesOnCurrentCompany = GetCurrentBranches();
				foreach (ZGuid branchPK in allbranchesOnCurrentCompany.Keys)
				{
					BadgeCodeSettingCollection existingBadgesForThisBranch = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, branchPK.ToGuid(), Guid.Empty);
					foreach (BadgeCodeSetting badge in existingBadgesForThisBranch)
					{
						var pair = new Tuple<BadgeCodeSetting, ZString>(badge, allbranchesOnCurrentCompany[branchPK]);
						plausibleBadges.Add(pair);
					}
				}
				return plausibleBadges;
			}
		}

		/// <summary>
		/// Gets a dictionary containing the PK and name of all branches on the current company.  
		/// </summary>
		/// <returns></returns>
		Dictionary<ZGuid, ZString> GetCurrentBranches()
		{
			Dictionary<ZGuid, ZString> branchPKandNamePairs = new Dictionary<ZGuid, ZString>();
			foreach (GlbBranch branch in GlbCompany.CurrentCompany.Branches)
			{
				branchPKandNamePairs.Add(branch.PK, branch.GB_BranchName);
			}
			return branchPKandNamePairs;
		}

		public ZPropertyInfo BadgeCodeInfo
		{
			get { return GetZPropertyInfo(Schema.BadgeCode); }
		}

		#endregion

		#region Username
		ZString fUsername;
		[MaxLength(255)]
		public ZString Username
		{
			get { return fUsername; }
			set
			{
				bool hasChanged = false;
				if (value != Username)
				{
					hasChanged = true;
				}
				SetNonPersistentPropertyValue(UsernameInfo, ref fUsername, value);
				if (!IsValidationSuspended)
				{
					ValidateUsername();
				}
				if (hasChanged)
				{
					SetDataTestStateDirty();
				}
			}
		}

		public ZPropertyInfo UsernameInfo
		{
			get { return GetZPropertyInfo(Schema.Username); }
		}

		public void ValidateUsername()
		{
			UsernameInfo.ClearAllNotifications();
			var csp = CSP;
			if (csp == GatewayList.Codes.MCP_CUSDECOnly || csp == GatewayList.Codes.CNS_CUSDECOnly)
			{
				if (Username.IsEmpty)
				{
					UsernameInfo.AddError("Username cannot be empty.");
				}
				else
				{
					if (!IsCDS)
					{
						if (csp == GatewayList.Codes.MCP_CUSDECOnly && ((!Username.EndsWith("8") && !Username.EndsWith("T")) || Username.Length != 4))
						{
							UsernameInfo.AddMessageError("MCP usernames are usually 4 characters and end with '8' (live) or 'T' (test), e.g. 'XXX8' or 'XXXT'. " + doNotGetErrorText);
						}
						else if (csp == GatewayList.Codes.CNS_CUSDECOnly && !Username.EndsWith("CCMI"))
						{
							UsernameInfo.AddMessageError("CNS usernames usually end with 'CCMI', e.g. 'XXXYYYCCMI'. " + doNotGetErrorText);
						}
					}
				}
			}
		}
		#endregion

		#region Password
		ZString fPassword;
		[MaxLength(255)]
		public ZString Password
		{
			get { return fPassword; }
			set
			{
				bool hasChanged = false;
				if (value != Password)
				{
					hasChanged = true;
				}
				SetNonPersistentPropertyValue(PasswordInfo, ref fPassword, value);
				if (!IsValidationSuspended)
				{
					ValidatePassword();
				}
				if (hasChanged)
				{
					SetDataTestStateDirty();
				}
			}
		}

		public ZPropertyInfo PasswordInfo
		{
			get { return GetZPropertyInfo(Schema.Password); }
		}

		public void ValidatePassword()
		{
			PasswordInfo.ClearAllNotifications();
			var csp = CSP;
			if (csp == GatewayList.Codes.MCP_CUSDECOnly || csp == GatewayList.Codes.CNS_CUSDECOnly)
			{
				if (Password.IsEmpty)
				{
					PasswordInfo.AddError("Password cannot be empty.");
				}
				else
				{
					if (csp == GatewayList.Codes.CNS_CUSDECOnly && Password != "CCMIPWD")
					{
						PasswordInfo.AddMessageError("CNS password is usually 'CCMIPWD'. " + doNotGetErrorText);
					}
				}
			}
		}
		#endregion

		#region Printer or NES location 
		ZString fPrinterLocation;
		[MaxLength(255)]
		public ZString Printer
		{
			get { return fPrinterLocation; }
			set
			{
				bool hasChanged = false;
				if (value != Printer)
				{
					hasChanged = true;
				}
				SetNonPersistentPropertyValue(PrinterInfo, ref fPrinterLocation, value);
				if (!IsValidationSuspended)
				{
					ValidatePrinter();
				}
				if (hasChanged)
				{
					SetDataTestStateDirty();
				}
			}
		}

		/// <summary>
		/// Location/Printer/PIMA
		/// </summary>
		public ZString NesLocation
		{
			get { return Printer; }
			set { Printer = value; }
		}

		/// <summary>
		/// Company/Role/Real badge
		/// </summary>
		public ZString NesRole
		{
			get { return Company; }
			set { Company = value; }
		}

		public ZPropertyInfo PrinterInfo
		{
			get { return GetZPropertyInfo(Schema.Printer); }
		}

		public void ValidatePrinter()
		{
			PrinterInfo.ClearAllNotifications();
			var application = Badge?.ApplicationCode ?? ZString.Empty;
			if (application != DeclarationApplicationCodeList.Codes.Customs_Declaration_Services)
			{
				switch (CSP)
				{
					case GatewayList.Codes.NES:
						if (!Printer.StartsWith("LOCEDC"))
						{
							PrinterInfo.AddMessageError("Your NES CHIEF location should start with 'LOCEDC', e.g. 'LOCEDC1XXX'. " + doNotGetErrorText);
						}
						break;
					case GatewayList.Codes.MCP_CUSDECOnly:
						if (!Printer.EndsWith("9"))
						{
							PrinterInfo.AddMessageError("MCP output device ('printer') usually ends with '9', e.g. XXX9. " + doNotGetErrorText);
						}
						break;
					case GatewayList.Codes.CNS_CUSDECOnly:
						if (!Printer.EndsWith("MLBX"))
						{
							PrinterInfo.AddMessageError("CNS output device ('mailbox') usually ends with 'MLBX', e.g. XXXYYYMLBX. " + doNotGetErrorText);
						}
						break;
					case GatewayList.Codes.CCSUKviaNTMsgGW:
						if (!Printer.StartsWith("CUK"))
						{
							PrinterInfo.AddMessageError("CCS-UK PIMA must start with 'CUK', e.g. 'CUKXXX98YYYZZZ'. " + doNotGetErrorText);
						}
						break;
				}
			}
		}
		#endregion

		#region Company or role
		ZString fCompanyRole;
		[MaxLength(255)]
		public ZString Company
		{
			get { return fCompanyRole; }
			set
			{
				bool hasChanged = false;
				if (value != Company)
				{
					hasChanged = true;
				}
				SetNonPersistentPropertyValue(CompanyInfo, ref fCompanyRole, value);
				if (!IsValidationSuspended)
				{
					ValidateCompany();
				}
				if (hasChanged)
				{
					SetDataTestStateDirty();
				}
			}
		}

		public ZPropertyInfo CompanyInfo
		{
			get { return GetZPropertyInfo(Schema.Company); }
		}

		public void ValidateCompany()
		{
			CompanyInfo.ClearAllNotifications();
			var csp = CSP;
			if (csp == GatewayList.Codes.NES)
			{
				if (!Company.StartsWith("THS"))
				{
					CompanyInfo.AddMessageError("Your NES CHIEF role should start with 'THS', e.g. 'THS1XXX'. " + doNotGetErrorText);
				}
			}
			else if (!IsCcsuk)
			{
				if (Company.Length == 0 && (csp == GatewayList.Codes.MCP_CUSDECOnly || csp == GatewayList.Codes.CNS_CUSDECOnly))
				{
					CompanyInfo.AddError("Company code is required if CSP is MCP or CNS.  For CSP badges, supply the company code assigned to your organisation. " + doNotGetErrorText);
				}
				if (Company.Length != 3)
				{
					CompanyInfo.AddMessageError("Badge/Company code must be 3 characters.  For CSP badges, supply the company code assigned to your organisation. " + doNotGetErrorText);
				}
			}
		}
		#endregion

		public ZBool IsCDS => (ZString)Badge?.ApplicationCode == DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

		bool IsCcsuk
		{
			get
			{
				var existingBadges = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty);
				var bcs = existingBadges.FindByBadgeCodeOnly(this.BadgeCode);
				if (bcs == null)
				{
					return PIMA.StartsWith("CUK");
				}
				return bcs.CSPCode == GatewayList.Codes.CCSUKviaNTMsgGW;
			}
		}

		public bool IsPentant
		{
			get
			{
				BadgeCodeSettingCollection existingBadges = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty);
				BadgeCodeSetting bcs = existingBadges.FindByBadgeCodeOnly(this.BadgeCode);
				return !(bcs is null) && bcs.CSPCode == GatewayList.Codes.Pentant;
			}
		}

		public bool IsCcskShed
		{
			get
			{
				return IsCcsuk && PIMA.StartsWith("CUKAIR98");
			}
		}

		public bool IsCcskAgent
		{
			get
			{
				return IsCcsuk && PIMA.StartsWith("CUKFFW98");
			}
		}

		public bool IsDEPOperator
		{
			get
			{
				// CUKFFW98000XAA - the X means DEP
				/*	From: navinder.johal@hmrc.gsi.gov.uk [mailto:navinder.johal@hmrc.gsi.gov.uk] 
					Sent: 17 January 2012 16:11
					To: Daniel Clarke
					Subject: RE: DEP agents - how to identify

					Hi Daniel,

					Sure - and here’s a quick answer. CCS-UK DEP codes commence with ‘X’ followed by 2 alpha characters. 
				*/
				return IsCcskShed && new Regex("X[A-Z]{2}$").IsMatch(PIMA);
			}
		}

		public ZString PIMA
		{
			get { return Printer; }
			set { Printer = value; }
		}

		#region Endpoint
		ZString fEndpoint;
		[List(nameof(EndpointCodesList))]
		public ZString Endpoint
		{
			get { return fEndpoint; }
			set
			{
				SetNonPersistentPropertyValue(EndpointInfo, ref fEndpoint, value);
				if (!IsValidationSuspended)
				{
					ValidateEndpoint();
				}
			}
		}

		public EndpointList EndpointCodesList
		{
			get { return new EndpointList(); }
		}

		public ZPropertyInfo EndpointInfo
		{
			get { return GetZPropertyInfo(Schema.Endpoint); }
		}

		public void ValidateEndpoint()
		{
			EndpointInfo.ClearAllNotifications();
			if (CSP == GatewayList.Codes.Pentant)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(EndpointInfo);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(EndpointInfo);
			}
		}

		#endregion

		#region FallbackForShed
		ZString fFallbackForShed;
		[MaxLength(3)]
		public ZString FallbackForShed
		{
			get { return fFallbackForShed; }
			set
			{
				SetNonPersistentPropertyValue(FallbackForShedInfo, ref fFallbackForShed, value);
				if (!IsValidationSuspended)
				{
					ValidateFallbackForShed();
				}
			}
		}

		public ZPropertyInfo FallbackForShedInfo
		{
			get { return GetZPropertyInfo(Schema.FallbackForShed); }
		}

		BusinessObjectFactory localFactory;
		void CheckFactory()
		{
			if (localFactory == null)
			{
				localFactory = new BusinessObjectFactory();
			}
		}

		public void ValidateFallbackForShed()
		{
			FallbackForShedInfo.ClearAllNotifications();
			if (!IsCcskShed && !FallbackForShed.IsEmpty)
			{
				FallbackForShedInfo.AddError("Fallback is only for CCSUK");
				return;
			}

			if (FallbackForShed.Length > 0)
			{
				if (FallbackForShed.Length != 3)
				{
					FallbackForShedInfo.AddError("Shed code must be 3 characters, e.g. BAC");
				}
				else
				{
					CheckFactory();
					var airportCode = PIMA.Substring(8, 3);
					var agentCode = PIMA.Right(3);
					var shedCode = airportCode + FallbackForShed;

					var shed = Shed.LoadByCode(localFactory, Core.Constants.CountryCodes.UnitedKingdom, shedCode);
					if (shed == null)
					{
						FallbackForShedInfo.AddError("Fallback: Shed " + FallbackForShed + " is not valid or is not at airport " + airportCode);
					}

					var agentList = RefCusCodeListTypes.GetCachedList(localFactory, Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsAgentCode, ZDateTime.Today);
					if (!agentList.ContainsCode(agentCode))
					{
						FallbackForShedInfo.AddError("Fallback: Agent " + agentCode + " does not exist");
					}
				}

				ValidateBadge();
				ValidateCompany();
				ValidatePrinter();
			}
		}
		#endregion

		#region PreferredAgent
		ZString fPreferredAgent;
		[MaxLength(3)]
		public ZString PreferredAgent
		{
			get { return fPreferredAgent; }
			set
			{
				SetNonPersistentPropertyValue(PreferredAgentInfo, ref fPreferredAgent, value);
				if (!IsValidationSuspended)
				{
					ValidatePreferredAgent();
				}
			}
		}

		public ZPropertyInfo PreferredAgentInfo
		{
			get { return GetZPropertyInfo(Schema.PreferredAgent); }
		}

		public void ValidatePreferredAgent()
		{
			PreferredAgentInfo.ClearAllNotifications();
			if (!IsCcskShed && !PreferredAgent.IsEmpty)
			{
				PreferredAgentInfo.AddError("This field is only for CCSUK sheds");
				return;
			}

			if (PreferredAgent.Length > 0)
			{
				if (PreferredAgent.Length != 3)
				{
					PreferredAgentInfo.AddError("Agent code must be 3 characters, e.g. ABC");
				}
				ValidatePrinter();
			}
		}
		#endregion

		#region CcsukFallbackAgentType
		ZString fCcsukFallbackAgentType;
		[MaxLength(3)]
		[List(nameof(CcsukFallbackAgentTypeList))]
		public ZString CcsukFallbackAgentType
		{
			get { return fCcsukFallbackAgentType; }
			set
			{
				SetNonPersistentPropertyValue(CcsukFallbackAgentTypeInfo, ref fCcsukFallbackAgentType, value);
				if (!IsValidationSuspended)
				{
					ValidateCcsukFallbackAgentType();
				}
			}
		}

		public ZPropertyInfo CcsukFallbackAgentTypeInfo
		{
			get { return GetZPropertyInfo(Schema.CcsukFallbackAgentType); }
		}

		public void ValidateCcsukFallbackAgentType()
		{
			CcsukFallbackAgentTypeInfo.ClearAllNotifications();
			if (CcsukFallbackAgentType.Length > 0)
			{
				if (!IsCcskAgent)
				{
					CcsukFallbackAgentTypeInfo.AddError("This field is only for CCSUK agents");
					return;
				}
				ListValidation.ErrorIfInvalidCode(CcsukFallbackAgentTypeInfo);
				ValidatePrinter();
			}
		}

		public AgentTypeForExportFallbackList CcsukFallbackAgentTypeList
		{
			get { return new AgentTypeForExportFallbackList(); }
		}
		#endregion

		#region SenderID  

		ZString fSenderId;
		[MaxLength(35)]
		public ZString SenderID
		{
			get { return fSenderId; }
			set
			{
				SetNonPersistentPropertyValue(SenderIDInfo, ref fSenderId, value);
				if (!IsValidationSuspended)
				{
					ValidateSenderID();
				}
			}
		}

		public void ValidateSenderID()
		{ }

		public ZPropertyInfo SenderIDInfo
		{
			get { return GetZPropertyInfo(Schema.SenderID); }
		}
		#endregion

		#region ReceiverID  

		ZString fReceiverId;
		[MaxLength(35)]
		public ZString ReceiverID
		{
			get { return fReceiverId; }
			set
			{
				SetNonPersistentPropertyValue(ReceiverIDInfo, ref fReceiverId, value);
				if (!IsValidationSuspended)
				{
					ValidateReceiverID();
				}
			}
		}

		public void ValidateReceiverID()
		{ }

		public ZPropertyInfo ReceiverIDInfo
		{
			get { return GetZPropertyInfo(Schema.ReceiverID); }
		}
		#endregion

		#region WebServiceFailureCount
		ZInt fWebServiceFailureCount;
		[ReadOnlyMember(nameof(IsHostedButNotEdiSupport))]
		public ZInt WebServiceFailureCount
		{
			get { return fWebServiceFailureCount; }
			set
			{
				SetNonPersistentPropertyValue(WebServiceFailureCountInfo, ref fWebServiceFailureCount, value);
			}
		}

		public ZPropertyInfo WebServiceFailureCountInfo
		{
			get { return GetZPropertyInfo(Schema.WebServiceFailureCount); }
		}

		bool IsHostedButNotEdiSupport
		{
			get { return EnvProxy.IsHostedWithCargowise && IsNotEdiSupport; }
		}
		#endregion

		#region IsMaritimeLoader
		ZBool fIsMaritimeLoader;
		[ReadOnlyMember(nameof(IsNotEdiSupport))]
		public ZBool IsMaritimeLoader
		{
			get { return fIsMaritimeLoader; }
			set
			{
				SetNonPersistentPropertyValue(IsMaritimeLoaderInfo, ref fIsMaritimeLoader, value);
			}
		}

		public ZPropertyInfo IsMaritimeLoaderInfo
		{
			get { return GetZPropertyInfo(Schema.IsMaritimeLoader); }
		}

		bool IsNotEdiSupport
		{
			get { return !(GlbStaff.CurrentUser.IsSupportUser || GlbStaff.CurrentUser.GS_IsDeveloper); }
		}
		#endregion

		#region ICredentials

		public System.Net.NetworkCredential GetCredential(Uri uri, string authType)
		{
			return new System.Net.NetworkCredential(this.Username, this.Password);
		}

		#endregion

		public static CredentialsSettingCollection GetAllCredentials(ZGuid companyPK)
		{
			return GBCustomsDataRegistry.Instance.Credentials.GetFallBackValueAtAllLevels(companyPK.ToGuid(), Guid.Empty, Guid.Empty);
		}

		public static CredentialsSetting GetCredentialsForBadge(ZString badge, ZGuid companyPK)
		{
			CredentialsSettingCollection creds = GBCustomsDataRegistry.Instance.Credentials.GetFallBackValueAtAllLevels(companyPK.ToGuid(), Guid.Empty, Guid.Empty);
			if (creds != null)
			{
				CredentialsSetting cred = creds.FindByBadgeCode(badge);
				return cred;
			}
			return null;
		}

		public static CredentialsSetting GetCredentialsForBadge(ZString badge, CredentialsSettingCollection credentials)
		{
			if (credentials != null)
			{
				return credentials.FindByBadgeCode(badge);
			}
			return null;
		}

		#region XML Reading and Writing
		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.BadgeCode, BadgeCode);
			writer.WriteElementString(Schema.Username, Username);
			writer.WriteElementString(Schema.Password, Password);
			writer.WriteElementString(Schema.Printer, Printer);
			writer.WriteElementString(Schema.Company, Company);
			writer.WriteElementString(Schema.FallbackForShed, FallbackForShed);
			writer.WriteElementString(Schema.PreferredAgent, PreferredAgent);
			writer.WriteElementString(Schema.CcsukFallbackAgentType, CcsukFallbackAgentType);
			writer.WriteElementString(Schema.WebServiceFailureCount, WebServiceFailureCount.ToString());
			writer.WriteElementString(Schema.IsMaritimeLoader, IsMaritimeLoader.ToString());
			writer.WriteElementString(Schema.DataTestStatus, DataTestStatus);
			writer.WriteElementString(Schema.SenderID, SenderID);
			writer.WriteElementString(Schema.ReceiverID, ReceiverID);
			writer.WriteElementString(Schema.Endpoint, Endpoint);
		}

		protected sealed override void ReadElements(XmlReaderWrapper reader)
		{
			isSettingDefaultValues = true;
			BadgeCode = reader.ReadElementString(Schema.BadgeCode);
			Username = reader.ReadElementString(Schema.Username);
			Password = reader.ReadElementString(Schema.Password);
			Printer = reader.ReadElementString(Schema.Printer);
			Company = reader.ReadElementString(Schema.Company);
			FallbackForShed = reader.ReadElementString(Schema.FallbackForShed);
			PreferredAgent = reader.ReadElementString(Schema.PreferredAgent);
			CcsukFallbackAgentType = reader.ReadElementString(Schema.CcsukFallbackAgentType);
			WebServiceFailureCount = reader.ReadElementStringAsZInt(Schema.WebServiceFailureCount);
			IsMaritimeLoader = reader.ReadElementStringAsZBool(Schema.IsMaritimeLoader);
			DataTestStatus = reader.ReadElementString(Schema.DataTestStatus);
			SenderID = reader.ReadElementString(Schema.SenderID);
			ReceiverID = reader.ReadElementString(Schema.ReceiverID);
			Endpoint = reader.ReadElementString(Schema.Endpoint);
			isSettingDefaultValues = false;
		}

		bool isSettingDefaultValues;

		public void ValidateAll()
		{
			ValidateCompany();
			ValidatePassword();
			ValidatePrinter();
			ValidateUsername();
			ValidateBadge();
			ValidateFallbackForShed();
			ValidatePreferredAgent();
			ValidateCcsukFallbackAgentType();
			ValidateSenderID();
			ValidateSenderID();
			ValidateEndpoint();
		}

		void ValidateBadge()
		{
			BadgeCodeInfo.ClearAllNotifications();
			if (BadgeCode.IsEmpty && FallbackForShed.IsEmpty)
			{
				BadgeCodeInfo.AddError("Choose a badge code from the drop down list.");
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(BadgeCodeInfo, ExistingBadges, (NoResString)"badge code.  You may only select badges that are already defined in the 'Badge Codes' of the registry for the current company.");
			}
		}
		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		readonly string doNotGetErrorText = string.Format("Do not guess at this information, obtain it from the CSP. See the description above. Supplying bad data will probably mean that {0} cannot reach the CSP or CHIEF.", BrandingFactory.Instance.ProductName);

		public void CheckCredentialsAgainstCspWebService(IUserNotification notifier)
		{
			ValidateBadge();
			if (BadgeCodeInfo.HasErrors())
			{
				notifier.ShowError("Row is invalid.  Fix the errors before checking.");
				return;
			}

			if (!IsCDS)
			{
				notifier.ShowError("Check Credentials is only available for CDS credentials.");
				return;
			}

			var description = ExistingBadges.GetDescriptionFromCode(BadgeCode);
			if (string.IsNullOrEmpty(description))
			{
				notifier.ShowError("Could not determine CSP.  Cannot check.");
				return;
			}

			string cspName = "";
			ICspPrintsMailBoxProvider poller = null;

			if (description.Contains("(MCP"))
			{
				cspName = "MCP";
				poller = (ICspPrintsMailBoxProvider)ObjectFactory.Get("IDestin8WebService");
				poller.Url = GBCustomsDataRegistry.Instance.McpCdsCheckCredentialsUrl;
			}
			else if (description.Contains("(CNS"))
			{
				cspName = "CNS";
				poller = (ICspPrintsMailBoxProvider)ObjectFactory.Get("ICspPrintsMailBoxProvider");
				poller.Url = GBCustomsDataRegistry.Instance.CnsCdsCheckCredentialsUrl;
			}

			if (poller == null)
			{
				notifier.ShowError("Validation is not possible for these credentials; select an MCP or CNS row only.");
			}
			else
			{
				poller.Credentials = this;
				ICspDownloadResult waitingPrints = null;
				try
				{
					waitingPrints = poller.checkCdsCredentials(Company, Printer);
					var printsCount = waitingPrints.MessagesArray != null ? waitingPrints.MessagesArray.Length : 0;
					if (!string.IsNullOrEmpty(waitingPrints.errorText))
					{
						DataTestStatus = DataTestStatusList.Codes.Invalid;
						notifier.ShowError(string.Format(CultureInfo.InvariantCulture, "CSP rejected the request.  Please liaise directly with the CSP about their rejection of your details. \r\nThe error was: {0}", waitingPrints.errorText));
					}
					else
					{
						DataTestStatus = DataTestStatusList.Codes.Valid;
						WebServiceFailureCount = 0;
						if (!waitingPrints.MessagesArray.IsNullOrEmpty())
						{
							notifier.ShowInformation(string.Format(CultureInfo.InvariantCulture, "CSP confirmed that the credentials are known, and that the topic is registered to: \r\n{0}", waitingPrints.MessagesArray[0]));
						}
						else
						{
							notifier.ShowInformation(string.Format(CultureInfo.InvariantCulture, "CSP confirmed that the credentials are known, but the topic is not curently registered. \r\neHub will register the topic when first used. Please refer to eLearning unit 1BGB047"));
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					DataTestStatus = DataTestStatusList.Codes.Invalid;
					var message = ex.Message;
					var explanation = "";
					if (message.Contains("401"))
					{
						explanation = "Your username and password are probably incorrect for the URL and environment";
					}
					if (message.Contains("404"))
					{
						explanation = "The URL is probably incorrect";
					}
					notifier.ShowError(string.Format(CultureInfo.InvariantCulture, "Error in reaching {0}.\r\n{1}\r\n{2}\r\nEnsure that your workstation or terminal server is able to access this URL:\r\n     {3}", cspName, message, explanation, poller.Url));
				}
			}
		}

		#region DataTestStatus
		ZString fDataTestStatus;
		[MaxLength(8)]
		[ReadOnlyMember(nameof(IsNotEdiSupport))]
		[List(nameof(DataTestStatusCodesList))]
		public ZString DataTestStatus
		{
			get { return fDataTestStatus; }
			set
			{
				SetNonPersistentPropertyValue(DataTestStatusInfo, ref fDataTestStatus, value);
				if (!IsValidationSuspended)
				{
					ValidateDataTestStatus();
				}
			}
		}

		public DataTestStatusList DataTestStatusCodesList
		{
			get { return new DataTestStatusList(); }
		}

		public ZPropertyInfo DataTestStatusInfo
		{
			get { return GetZPropertyInfo(Schema.DataTestStatus); }
		}

		public void ValidateDataTestStatus()
		{
			DataTestStatusInfo.ClearAllNotifications();
			if (IsHttpWebCredential)
			{
				if (IsOrdinaryUser)
				{
					if (DataTestStatus != DataTestStatusList.Codes.Valid)
					{
						DataTestStatusInfo.AddError("These credentials have not been proved correct against the CSP. Either test them now or cancel your edit. To test, right-click the row's gutter and choose 'Check Credentials' from the popup menu.");
					}
				}
			}
			else if (!DataTestStatus.IsEmpty)
			{
				DataTestStatusInfo.AddError("This field should only be set for MCP and CNS.");
			}
		}

		void SetDataTestStateDirty()
		{
			if (!isSettingDefaultValues)
			{
				if (IsHttpWebCredential)
				{
					DataTestStatus = DataTestStatusList.Codes.Untested;
				}
			}
		}

		public bool IsHttpWebCredential
		{
			get
			{
				var csp = CSP;
				return (csp == GatewayList.Codes.MCP_CUSDECOnly || csp == GatewayList.Codes.CNS_CUSDECOnly);
			}
		}
		#endregion

		bool IsOrdinaryUser
		{
			get { return !GlbStaff.CurrentUser.GS_IsDeveloper && !GlbStaff.CurrentUser.IsSupportUser && GlbStaff.CurrentUser.GS_Code != User.ServiceUserCode; }
		}
	}
}
