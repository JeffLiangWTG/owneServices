using System;
using System.Reflection;
using System.Xml;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ResString = Enterprise.Licensing.Core.ResString;

namespace Enterprise.Licensing
{
	public class LicenceCheckpoint : LicenceSegment, ILicenceCheckpoint
	{
		internal LicenceCheckpoint(string name,
			string displayName,
			Licences parentLicences,
			LicenceCheckpoint parentCheckpoint,
			string categoryCode,
			bool shouldCreateConsumptionLogOnLogin = true)
		{
			this.Name = name;
			this.DisplayName = displayName;
			SetParentLicences(parentLicences);
			this.ParentModule = parentCheckpoint == null ? "" : parentCheckpoint.Name;
			this.ParentCheckpoint = parentCheckpoint;
			this.shouldCreateConsumptionLogOnLogin = shouldCreateConsumptionLogOnLogin;
			this.CategoryCode = categoryCode;
		}

		#region Testing Only
#if DEBUG

		public LicenceCheckpoint()
		{
		}

#endif
		#endregion

		#region Login

		/// <summary>
		/// Logs the current user into this checkpoint if not already logged in. Returns true if login is successful.
		/// The current user is always considered "logged in" to this checkpoint while there is at least 1 licensedComponent held by this checkpoint.
		/// </summary>
		/// <param name="licensedComponent">No reference is held for this component (e.g. a form or module) in this checkpoint on successful login.</param>
		public virtual LicenceLoginResponse Login(ILicensedComponent licensedComponent)
		{
			return Login(licensedComponent, false);
		}

		public LicenceLoginResponse Login(ILicensedComponent licensedComponent, bool isDemoCompany)
		{
			LicenceLoginResponse loginSuccessful = Login(isDemoCompany);

			if (loginSuccessful != LicenceLoginResponse.Denied && licensedComponent != null)
			{
				if (((LicensedComponentManager)licensedComponent.LicensedComponentManager).AddLicenceCheckpointLoggedIn(this))
				{
					++componentLoginCount;
				}

				if (shouldCreateConsumptionLogOnLogin)
				{
					var utcNow = ZDateTime.UtcNow;
					var converter = new BillingTimeConverter();
					var lastLogDateInBillingTimeZone = converter.ConvertUtcToTimeInBillingTimeZone(lastConsumptionLogCreated).Date;
					var currentDateInBillingTimeZone = converter.ConvertUtcToTimeInBillingTimeZone(utcNow).Date;

					if (lastConsumptionLogCreated.IsEmpty || lastLogDateInBillingTimeZone != currentDateInBillingTimeZone)
					{
						ConsumptionLogCreator.CreateLog(this, utcNow.ToDateTime());
						lastConsumptionLogCreated = converter.ConvertTimeInBillingTimeZoneToUtc(currentDateInBillingTimeZone);
#if DEBUG
						ConsumptionLogCreated = true;
#endif
					}
				}
			}

			return loginSuccessful;
		}
		int componentLoginCount;

		/// <summary>
		/// Logs the current user to this module. Returns true if log to this module is successful.
		/// </summary>
		LicenceLoginResponse Login(bool isDemoCompany = false)
		{
			lastReasonForNotAllowing = null;
			LicenceLoginResponse allowed = LicenceLoginResponse.Denied;
			LicenceExpiryCheck expiryCheck;

			if (!parentLicences.CurrentUser.IsOperational || CurrentUserIsSupport)
			{
				allowed = LicenceLoginResponse.Granted;
				parentLicences.LogVerboseIfApplicable("Granted to " + (!parentLicences.CurrentUser.IsOperational ? parentLicences.CurrentUser.LoginName : "Support Password"));
			}
			else if (!isDemoCompany && ShouldDoExpiryCheck && (expiryCheck = LicenceExpiryCheck.Create()).SystemHasExpired)
			{
				lastReasonForNotAllowing = (NoResString)expiryCheck.StabilityErrorMessage;
			}
			else
			{
				allowed = CheckLicenceTypeAndLogin();
			}

			if (allowed != LicenceLoginResponse.Granted)
			{
				lastReasonForNotAllowing = MultilingualString.Join(System.Environment.NewLine + System.Environment.NewLine, lastReasonForNotAllowing);
			}

			return allowed;
		}

		bool CurrentUserIsSupport
		{
			get
			{
#if DEBUG
				// The unit tester always logs in with the master password.
				if (Globals.IsTest)
				{
					return false;
				}
#endif
				return parentLicences.CurrentUser.LoggedInWithMasterPassword;
			}
		}

		LicenceLoginResponse CheckLicenceTypeAndLogin()
		{
			LicenceLoginResponse allowed = LicenceLoginResponse.Granted;

#if DEBUG
			if (AllowUsageForTest.HasValue && !AllowUsageForTest.Value)
			{
				allowed = LicenceLoginResponse.Denied;
				lastReasonForNotAllowing = (NoResString)("Usage prevented for " + DisplayName + " module");
			}
#endif
			return allowed;
		}

#if DEBUG
		public bool? AllowUsageForTest;
#endif

		static internal MultilingualString GetLoginInternalErrorText(Exception ex)
		{
			return ResString.GetMultilingualString("0c7d8dfb-f3ef-42df-b5d1-0718b47f9b98", @"An internal error happened while obtaining the license. Please contact support.
{0}",
				ex.Message);
		}

		#region Expiry

		bool ShouldDoExpiryCheck
		{
			get
			{
				bool result = (parentLicences.CurrentUser.IsOperational && !parentLicences.CurrentUser.IsBatchProcessor);
#if DEBUG
				result = result && ShouldCheckForSystemExpiry && Globals.IsTest;
#endif
				return result;
			}
		}

#if DEBUG
		public static bool ShouldCheckForSystemExpiry;
#endif

		#endregion

		ILicenceConsumptionLogCreator ConsumptionLogCreator
		{
			get
			{
				if (consumptionLogCreator == null)
				{
					consumptionLogCreator = (ILicenceConsumptionLogCreator)Activator.CreateInstance(Type.GetType("Enterprise.Licensing.LicenceConsumptionLogCreator, Enterprise.Licensing"));
				}
				return consumptionLogCreator;
			}
		}
		ILicenceConsumptionLogCreator consumptionLogCreator;

		readonly bool shouldCreateConsumptionLogOnLogin;
		ZDateTime lastConsumptionLogCreated;
#if DEBUG
		public bool ConsumptionLogCreated;
		internal void SetConsumptionLogCreatorForTest(ILicenceConsumptionLogCreator logCreator)
		{
			consumptionLogCreator = logCreator;
		}

		public void SetLastConsumptionLogCreatedForTest(ZDateTime logCreatedTime)
		{
			lastConsumptionLogCreated = logCreatedTime;
		}
#endif

		#endregion

		#region Logout

		/// <summary>
		/// Releases a licensedComponent from this checkpoint and logs the current user out of this checkpoint if there are no more LicensedComponents held by this checkpoint.
		/// </summary>
		/// <param name="licensedComponent">A licenced component to be removed from this check point</param>
		public virtual void Logout(ILicensedComponent licensedComponent)
		{
			if (((LicensedComponentManager)licensedComponent.LicensedComponentManager).RemoveLicenceCheckpointLoggedIn(this))
			{
				--componentLoginCount;
			}
		}

		#region Testing Only
#if DEBUG

		/// <summary>
		/// Force the current user to completely log out of this licence checkpoint, even if this user still has forms or plugins open using this licence checkpoint.
		/// This method is only available in DEBUG for testing purposes.
		/// </summary>
		public void ForceLogout()
		{
			componentLoginCount = 0;
		}

#endif
		#endregion

		#endregion

		#region Licence Storage

		protected virtual void SetParentLicences(Licences parentLicences)
		{
			parentLicences.Add(this);
			this.parentLicences = parentLicences;
		}

#if DEBUG
		internal override void AddToLicenceNode(XmlNode licenceKeyNode)
		{
		}
#endif

		internal override void LoadFromLicenceNode(XmlNode licenceNode)
		{
		}

		#endregion

		#region Properties

		public static readonly Guid LicenceConsumptionActivityLogKey = new Guid("D0D2148A-6188-47b4-8D59-70E591518437");

		public ModuleLicenceType GetModuleLicenceTypeFromStringCode(string code)
		{
			foreach (FieldInfo info in typeof(ModuleLicenceType).GetFields())
			{
				if (info.Name == code)
				{
					return (ModuleLicenceType)info.GetValue(null);
				}
			}

			return ModuleLicenceType.NON;
		}

		public bool IsLoggedIn
		{
			get { return componentLoginCount > 0; }
		}

		public MultilingualString ModuleLicenceTypeDescription
		{
			get
			{
				MultilingualString result = LicenceTypes.GetMultilingualDescriptionFromCode(LicenceType.ToString());
				if (result == null || string.IsNullOrEmpty(result.GetUnresolvedString()))
				{
					result = ResString.GetMultilingualString("BE71E369-F0C3-43e1-B581-CA7DF0BFB959", "Unknown");
				}
				return result;
			}
		}

		public string Name { get; set; }
		public string DisplayName { get; set; }
		public string ParentModule { get; internal set; }
		public ILicenceCheckpoint ParentCheckpoint { get; internal set; }
		public string LastReasonForNotAllowing { get { return lastReasonForNotAllowing ?? string.Empty; } }
		MultilingualString lastReasonForNotAllowing;

		/// <summary>
		/// Groups these objects into a small number of categories.
		/// Was used in the AllowExcessUsage registry node, but no longer needed.
		/// </summary>
		public string CategoryCode { get; set; }

		/// <summary>
		/// Indicates if the module is no longer used.
		/// Was used to hide in the AllowExcessUsage registry node, but no longer needed.
		/// </summary>
		public bool IsObsolete { get; set; }

		/// <summary>
		/// If true, then the default LicenceType is CPT
		/// </summary>
		public bool IsDefaultTransactional { get; set; }

		public ModuleLicenceType LicenceType
		{
			get
			{
				if (licenceType == 0)
				{
					licenceType = GetDefaultLicenceValue();
				}
				return licenceType;
			}
			internal set { licenceType = value; }
		}

		public virtual ModuleLicenceType GetDefaultLicenceValue()
		{
			if (IsDefaultTransactional)
			{
				return ModuleLicenceType.CPT;
			}
			else
			{
				return ModuleLicenceType.ODM;
			}
		}

		/// <summary>
		/// Licence Value to use when a module is enabled in the ediProd licencing module.
		/// If NON then value follows header.
		/// </summary>
		public virtual ModuleLicenceType GetDefaultEnabledLicenceValue()
		{
			if (IsDefaultTransactional)
			{
				return ModuleLicenceType.CPT;
			}

			return ModuleLicenceType.NON;
		}

		LicenceTypes LicenceTypes
		{
			get { return licenceTypes ?? (licenceTypes = new LicenceTypes()); }
		}

		ModuleLicenceType licenceType;
		LicenceTypes licenceTypes;

		#endregion

		internal Licences ParentLicences
		{
			get { return parentLicences; }
		}
		protected Licences parentLicences;

		public ILicenceCheckpointUserContext ParentUserContext
		{
			get { return ParentLicences; }
		}
	}

	#region Custom LicenceCheckpoints

	#region Always Allow Checkpoint

	internal class AlwaysAllowLicenceCheckpoint : LicenceCheckpoint
	{
		public AlwaysAllowLicenceCheckpoint()
			: base("---", "", null, null, "", false)
		{
		}

		protected override void SetParentLicences(Licences parentLicences)
		{
		}

		public override LicenceLoginResponse Login(ILicensedComponent licensedComponent)
		{
			return LicenceLoginResponse.Granted;
		}

		public override void Logout(ILicensedComponent licensedComponent)
		{
		}
	}

	#endregion

	#endregion
}
