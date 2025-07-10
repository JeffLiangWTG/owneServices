using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CargoWise.ActiveDirectory;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Security.ActiveDirectory.Synchronisation
{
	abstract class UserSyncStrategyBase : SyncStrategyBase<ADUser, GlbStaff>
	{
		protected UserSyncStrategyBase(IADEntity adEntity)
			: base(adEntity)
		{
		}

		#region AD master

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.String.ToLower")]
		string SetStaffLoginName(ADUser adUser)
		{
			if (!string.Equals(adUser.LoginName, EnterpriseEntity.GS_LoginName, StringComparison.Ordinal))
			{
				EnterpriseEntity.GS_LoginName = adUser.LoginName;
				if (EnterpriseEntity.GS_LoginNameInfo.HasErrors())
				{
					string bizoErrorMessage = "";
					foreach (var error in EnterpriseEntity.GS_LoginNameInfo.GetErrors())
					{
						bizoErrorMessage = string.Join(System.Environment.NewLine, bizoErrorMessage + error.Message);
					}

					var errorMessage = ResString.GetMultilingualString("243B5CFB-E882-467F-8977-6D1864DF4E09", "Cannot synchronize {0} '{1}' with Active Directory User's Logon Name '{2}' ", EnterpriseEntity.GS_LoginNameInfo.HumanReadableName, EnterpriseEntity.GS_LoginNameInfo.OriginalValue, adUser.LoginName);
					EnterpriseEntity.GS_LoginName = EnterpriseEntity.GS_LoginNameInfo.OriginalValue.ToString();
					EnterpriseEntity.HasChanges = false; // do not attempt to save this entity as it has errors
					throw new DirectoryServicesException(string.Join(System.Environment.NewLine, errorMessage, bizoErrorMessage));
				}
			}
			return EnterpriseEntity.GS_LoginName.ToLower();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.String.ToLower")]
		protected void SyncWithADAsMaster()
		{
			adEntity.GetDirectoryEntry()?.RefreshCache(); // The cached data could be obsolete
			SetValue(u => (u.LoginName ?? string.Empty).ToLower(), EnterpriseEntity.GS_LoginName.ToLower(), EnterpriseEntity.GS_LoginNameInfo, u => SetStaffLoginName(u));
			SetDomainName();
			SetIsLinked();
			SetValue(u => u.IsActive, EnterpriseEntity.GS_IsActive, EnterpriseEntity.GS_IsActiveInfo, u => EnterpriseEntity.GS_IsActive = u.IsActive, u => DeactivateStaff(u));
			SetValue(u => u.Title, EnterpriseEntity.GS_Title, EnterpriseEntity.GS_TitleInfo, u => EnterpriseEntity.GS_Title = u.Title);
			SetValue(u => u.FullName, EnterpriseEntity.GS_FullName, EnterpriseEntity.GS_FullNameInfo, u => EnterpriseEntity.GS_FullName = GetValueOrEmptyPlaceholder(u.FullName));
			SetValue(u => u.EmailAddress, EnterpriseEntity.GS_EmailAddress, EnterpriseEntity.GS_EmailAddressInfo, u => EnterpriseEntity.GS_EmailAddress = u.EmailAddress);
			SetValue(u => u.WorkExtension, EnterpriseEntity.GS_WorkExtension, EnterpriseEntity.GS_WorkExtensionInfo, u => EnterpriseEntity.GS_WorkExtension = u.WorkExtension);
			SetValue(u => u.WorkPhone, EnterpriseEntity.GS_WorkPhone, EnterpriseEntity.GS_WorkPhoneInfo, u => EnterpriseEntity.GS_WorkPhone = u.WorkPhone);
			SetValue(u => u.MobilePhone, EnterpriseEntity.GS_MobilePhone, EnterpriseEntity.GS_MobilePhoneInfo,
				u => EnterpriseEntity.GS_SavePersonalDataToActiveDirectory && !string.IsNullOrEmpty(u.MobilePhone) ? EnterpriseEntity.GS_MobilePhone = u.MobilePhone : EnterpriseEntity.GS_MobilePhone);
			SetValue(u => u.FaxNum, EnterpriseEntity.GS_FaxNum, EnterpriseEntity.GS_FaxNumInfo, u => EnterpriseEntity.GS_FaxNum = u.FaxNum);
			SetValue(u => u.HomePhone, EnterpriseEntity.GS_HomePhone, EnterpriseEntity.GS_HomePhoneInfo,
				u => EnterpriseEntity.GS_SavePersonalDataToActiveDirectory && !string.IsNullOrEmpty(u.HomePhone) ? EnterpriseEntity.GS_HomePhone = u.HomePhone : EnterpriseEntity.GS_HomePhone);
			SetValue(u => u.Pager, EnterpriseEntity.GS_Pager, EnterpriseEntity.GS_PagerInfo, u => EnterpriseEntity.GS_Pager = u.Pager);
			SetValue(u => u.StreetAddress, EnterpriseEntity.GS_UserAddress1, EnterpriseEntity.GS_UserAddress1Info,
				u => EnterpriseEntity.GS_SavePersonalDataToActiveDirectory && !string.IsNullOrEmpty(u.StreetAddress) ? EnterpriseEntity.GS_UserAddress1 = GetValueOrEmptyPlaceholder(u.StreetAddress) : EnterpriseEntity.GS_UserAddress1);
			SetValue(u => u.City, EnterpriseEntity.GS_City, EnterpriseEntity.GS_CityInfo,
				u => EnterpriseEntity.GS_SavePersonalDataToActiveDirectory && !string.IsNullOrEmpty(u.City) ? EnterpriseEntity.GS_City = GetValueOrEmptyPlaceholder(u.City) : EnterpriseEntity.GS_City);
			SetValue(u => u.State, EnterpriseEntity.GS_State, EnterpriseEntity.GS_StateInfo,
				u => EnterpriseEntity.GS_SavePersonalDataToActiveDirectory && !string.IsNullOrEmpty(u.State) ? EnterpriseEntity.GS_State = u.State : EnterpriseEntity.GS_State);
			SetValue(u => u.Postcode, EnterpriseEntity.GS_Postcode, EnterpriseEntity.GS_PostcodeInfo,
				u => EnterpriseEntity.GS_SavePersonalDataToActiveDirectory && !string.IsNullOrEmpty(u.Postcode) ? EnterpriseEntity.GS_Postcode = u.Postcode : EnterpriseEntity.GS_Postcode);
			SetValue(u => u.PreferredLanguage, EnterpriseEntity.GS_WorkingLanguage, EnterpriseEntity.GS_WorkingLanguageInfo,
				u => EnterpriseEntity.GS_WorkingLanguage = ADLanguageToCW1Language(u.PreferredLanguage));
			SetValue(u => EnterpriseEntity.GS_ChangePasswordAtNextLogin = u.PasswordExpired);
			SetValue(u =>
			{
				if (IsSynced(GlbStaffSchema.GS_ProfilePhoto) && u.ThumbnailImage != null && u.ThumbnailImage.Length > 0)
				{
					EnterpriseEntity.GS_ProfilePhoto = u.ThumbnailImage;
				}
			});

			EnterpriseEntity.LocalPasswordMustBeReset = true;
			EnterpriseEntity.GS_ChangePasswordAtNextLogin = false;
		}

		static ZString GetValueOrEmptyPlaceholder(ZString value)
		{
			return string.IsNullOrEmpty(value) ? MissingDataString : value;
		}

		static ZString MissingDataString
		{
			get { return ResString.GetMultilingualString("4b7828f4-67fa-4934-a1e4-c9bfb65f5409", "None"); }
		}

		#endregion

		#region Enterprise master

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.String.ToLower")]
		string SetADUserLoginName(ADUser adUser)
		{
			if (!string.Equals(adUser.LoginName, EnterpriseEntity.GS_LoginName, StringComparison.Ordinal))
			{
				adUser.RenameLoginName(EnterpriseEntity.GS_LoginName);
			}
			return adUser.LoginName.ToLower();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.String.ToLower")]
		protected void SyncWithEnterpriseAsMaster(bool deactivateStaff = true)
		{
			SetValue(u => (u.LoginName ?? string.Empty).ToLower(), EnterpriseEntity.GS_LoginName.ToLower(), EnterpriseEntity.GS_LoginNameInfo, u => SetADUserLoginName(u));
			SetDomainName();
			SetIsLinked();
			SetValue(u => u.IsActive, EnterpriseEntity.GS_IsActive, EnterpriseEntity.GS_IsActiveInfo, u => u.IsActive = EnterpriseEntity.GS_IsActive, u => deactivateStaff ? DeactivateStaff(u) : DoNothing());
			SetValue(u => u.Title, EnterpriseEntity.GS_Title, EnterpriseEntity.GS_TitleInfo, u => u.Title = EnterpriseEntity.GS_Title);
			SetValue(u => u.FullName, EnterpriseEntity.GS_FullName, EnterpriseEntity.GS_FullNameInfo, u => u.FullName = EnterpriseEntity.GS_FullName);
			SetValue(u => u.EmailAddress, EnterpriseEntity.GS_EmailAddress, EnterpriseEntity.GS_EmailAddressInfo, u => u.EmailAddress = EnterpriseEntity.GS_EmailAddress);
			SetValue(u => u.WorkExtension, EnterpriseEntity.GS_WorkExtension, EnterpriseEntity.GS_WorkExtensionInfo, u => u.WorkExtension = EnterpriseEntity.GS_WorkExtension);
			SetValue(u => u.WorkPhone, EnterpriseEntity.GS_WorkPhone, EnterpriseEntity.GS_WorkPhoneInfo, u => u.WorkPhone = EnterpriseEntity.GS_WorkPhone);
			SetValue(u => u.MobilePhone, EnterpriseEntity.GS_MobilePhone, EnterpriseEntity.GS_MobilePhoneInfo,
				u => u.MobilePhone = EnterpriseEntity.GS_SavePersonalDataToActiveDirectory ? EnterpriseEntity.GS_MobilePhone : ZString.Empty);
			SetValue(u => u.FaxNum, EnterpriseEntity.GS_FaxNum, EnterpriseEntity.GS_FaxNumInfo, u => u.FaxNum = EnterpriseEntity.GS_FaxNum);
			SetValue(u => u.HomePhone, EnterpriseEntity.GS_HomePhone, EnterpriseEntity.GS_HomePhoneInfo,
				u => u.HomePhone = EnterpriseEntity.GS_SavePersonalDataToActiveDirectory ? EnterpriseEntity.GS_HomePhone : ZString.Empty);
			SetValue(u => u.Pager, EnterpriseEntity.GS_Pager, EnterpriseEntity.GS_PagerInfo, u => u.Pager = EnterpriseEntity.GS_Pager);
			SetValue(u => u.StreetAddress, EnterpriseEntity.GS_UserAddress1, EnterpriseEntity.GS_UserAddress1Info,
				u => u.StreetAddress = EnterpriseEntity.GS_SavePersonalDataToActiveDirectory ? EnterpriseEntity.GS_UserAddress1 : ZString.Empty);
			SetValue(u => u.City, EnterpriseEntity.GS_City, EnterpriseEntity.GS_CityInfo,
				u => u.City = EnterpriseEntity.GS_SavePersonalDataToActiveDirectory ? EnterpriseEntity.GS_City : ZString.Empty);
			SetValue(u => u.State, EnterpriseEntity.GS_State, EnterpriseEntity.GS_StateInfo,
				u => u.State = EnterpriseEntity.GS_SavePersonalDataToActiveDirectory ? EnterpriseEntity.GS_State : ZString.Empty);
			SetValue(u => u.Postcode, EnterpriseEntity.GS_Postcode, EnterpriseEntity.GS_PostcodeInfo,
				u => u.Postcode = EnterpriseEntity.GS_SavePersonalDataToActiveDirectory ? EnterpriseEntity.GS_Postcode : ZString.Empty);
			SetValue(u => u.PreferredLanguage, EnterpriseEntity.GS_WorkingLanguage, EnterpriseEntity.GS_WorkingLanguageInfo,
				u => u.PreferredLanguage = CW1LanguageToADLanguage(EnterpriseEntity.GS_WorkingLanguage));
			SetValue(u =>
			{
				if (IsSynced(GlbStaffSchema.GS_ProfilePhoto) && !EnterpriseEntity.GS_ProfilePhoto.IsEmpty && EnterpriseEntity.GS_ProfilePhoto.Length <= GlbStaffValidationReal.MaxADProfilePhotoBytes)
				{
					u.ThumbnailImage = EnterpriseEntity.GS_ProfilePhoto;
				}
			});

			if (ClientHookLoader.Instance.Client == Clients.EDI)
			{
				SetValue(u => u.Manager, EnterpriseEntity.CurrentDRMManager, null, u => u.Manager = EnterpriseEntity.CurrentDRMManager);
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
		string CW1LanguageToADLanguage(string cw1Language)
		{
			if (string.IsNullOrEmpty(cw1Language) || cw1Language.ToUpperInvariant() == "EN")
			{
				return "en-AU";
			}
			else if (cw1Language.CountMatches('-') == 2) //for example Cy-az-AZ ( https://docs.microsoft.com/en-us/previous-versions/commerce-server/ee825488(v=cs.20)?redirectedfrom=MSDN )
			{
				var parts = cw1Language.Split('-');
				var myTI = new CultureInfo("en-US", false).TextInfo;
				return myTI.ToTitleCase(parts[0]) + "-" + parts[1].ToLowerInvariant() + "-" + parts[2].ToUpperInvariant();
			}
			else if (cw1Language.Contains("-"))
			{
				var parts = cw1Language.Split('-');
				return parts[0].ToLowerInvariant() + "-" + parts[1].ToUpperInvariant();
			}
			else
			{
				return cw1Language;
			}
		}

		string ADLanguageToCW1Language(string adLanguage)
		{
			if (string.IsNullOrEmpty(adLanguage) || adLanguage.ToUpperInvariant() == "EN-AU")
			{
				return "EN";
			}
			var result = adLanguage.ToUpperInvariant();
			return result;
		}

		#endregion

		static ZBool DeactivateStaff(ADUser adUser)
		{
			adUser.DisconnectFromAD();
			return ZBool.False;
		}

		ZBool DoNothing()
		{
			return EnterpriseEntity.GS_IsActive;
		}
	}
}
