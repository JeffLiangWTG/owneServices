using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI
{
	/// <summary>
	/// Object responsible for capturin user login details
	/// </summary>
	public class LoginManager : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string CompanyCode = "CompanyCode";
			public const string UserName = "UserName";
			public const string Password = "Password";
			public const string RememberMe = "RememberMe";
			public const string LoginHash = "LoginHash";
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateCompanyCode();
			ValidateUserName();
			ValidatePassword();
			ValidateRememberMe();
		}

		#region CompanyCode

		protected ZString fCompanyCode;

		[ReadOnlyMember(nameof(CompanyCodeAndUsernameNotAllowedToBeChanged))]
		[ResourceStringData("56E4D8A2-5AD3-44CC-BF16-AD0C4757E631", Caption = "Company Code")]
		[MaxLength(OrgHeader.Schema.OH_CodeMaxLength)]
		public ZString CompanyCode
		{
			get { return fCompanyCode; }
			set
			{
				value = value.SubstringSafe(0, OrgHeader.Schema.OH_CodeMaxLength);
				if (fCompanyCode != value)
				{
					fCompanyCode = value;
					CompanyCodeInfo.RefreshBinding();
				}
				if (!IsValidationSuspended)
				{
					ValidateCompanyCode();
				}
			}
		}

		public virtual ZPropertyInfo CompanyCodeInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.CompanyCode);
			}
		}

		bool fIsCompanyCodeRequired = true;
		public bool IsCompanyCodeRequired
		{
			get { return fIsCompanyCodeRequired; }
			set { fIsCompanyCodeRequired = value; }
		}

		public void ValidateCompanyCode()
		{
			CompanyCodeInfo.ClearAllNotifications();
			if (IsCompanyCodeRequired && CompanyCode.Trim() == "")
			{
				CompanyCodeInfo.AddError(Res.GetString("ef86f585-320e-47ce-b8da-64197b8c3fda", "Company code is required"));
			}
		}

		#endregion

		#region UserName

		protected ZString fUserName;

		[ReadOnlyMember(nameof(CompanyCodeAndUsernameNotAllowedToBeChanged))]
		[ResourceStringData("5D5CD54A-8824-4D87-B014-260D6ACE4424", Caption = "E-mail")]
		[MaxLength(OrgContact.Schema.OC_EmailMaxLength)]
		public ZString UserName
		{
			get { return fUserName; }
			set
			{
				value = value.SubstringSafe(0, OrgContact.Schema.OC_EmailMaxLength);
				if (fUserName != value)
				{
					fUserName = value;
					UserNameInfo.RefreshBinding();
				}
				if (!IsValidationSuspended)
				{
					ValidateUserName();
				}
			}
		}

		public virtual ZPropertyInfo UserNameInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.UserName);
			}
		}

		public void ValidateUserName()
		{
			UserNameInfo.ClearAllNotifications();
			if (UserName.Trim() == "")
			{
				UserNameInfo.AddError(Res.GetString("ae3ee2db-a018-4979-adc1-ef964e35de0c", "User name is required"));
			}
		}

		#endregion

		#region Password

		protected ZString fPassword;
		[ResourceStringData("95D84135-58E1-46E7-BC6C-A46FE278B480}", Caption = "Password")]
		[MaxLength("Password_MaxLength")]
		public ZString Password
		{
			get { return fPassword; }
			set
			{
				value = value.SubstringSafe(0, Password_MaxLength);
				if (fPassword != value)
				{
					fPassword = value;
					PasswordInfo.RefreshBinding();
				}
				if (!IsValidationSuspended)
				{
					ValidatePassword();
				}
			}
		}

		protected virtual int Password_MaxLength => OrgContact.PasswordMaxLength;

		public virtual ZPropertyInfo PasswordInfo
		{
			get { return GetZPropertyInfo(Schema.Password); }
		}

		public void ValidatePassword()
		{
			PasswordInfo.ClearAllNotifications();
			if (Password.Trim() == "")
			{
				PasswordInfo.AddError(Res.GetString("0b466644-c8a6-439c-8881-6536dca7a366", "Password is required"));
			}
		}

		#endregion

		#region RememberMe

		public virtual ZBool RememberMe
		{
			get { return fRememberMe; }
			set
			{
				fRememberMe = value;
				if (!IsValidationSuspended)
				{
					ValidateRememberMe();
				}
				RememberMeInfo.RefreshBinding();
			}
		}

		protected ZBool fRememberMe;

		public virtual ZPropertyInfo RememberMeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.RememberMe); }
		}

		public void ValidateRememberMe()
		{
			RememberMeInfo.ClearAllNotifications();
		}

		#endregion

		#region LoginHash

		byte[] loginHash;

		[MaxLength(StmLoginFailureLog.Schema.SFL_LoginNameMaxLength)]
		[BusinessObjectTestExclude] // ignore non nullable test, this property can be null
		public byte[] LoginHash
		{
			get { return loginHash; }
			set
			{
				if (loginHash != value)
				{
					loginHash = value;
				}
			}
		}

		#endregion

		#region Current SiteUser for switching CompanyCode

		public WebUser SiteUser
		{
			get { return WebEnv.AppInstance != null ? WebEnv.AppInstance.SiteUser : null; }
		}

		#endregion

		public bool AllowCompanyCodeAndUsernameToBeChanged
		{
			get { return fAllowCompanyCodeAndUsernameToBeChanged; }
			set { fAllowCompanyCodeAndUsernameToBeChanged = value; }
		}

		protected bool CompanyCodeAndUsernameNotAllowedToBeChanged
		{
			get { return !AllowCompanyCodeAndUsernameToBeChanged; }
		}

		bool fAllowCompanyCodeAndUsernameToBeChanged = true;
	}
}
