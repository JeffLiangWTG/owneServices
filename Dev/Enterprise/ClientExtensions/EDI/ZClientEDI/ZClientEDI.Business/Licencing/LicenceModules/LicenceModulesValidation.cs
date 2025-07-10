using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Customs.Business;
using Enterprise.Licensing;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class LicenceModulesValidation : AutoLicenceModulesValidation
	{
		public LicenceModulesValidation(AutoLicenceModules parent) : base(parent)
		{
			MasterModule = (LicenceModules)parent;
		}

		readonly LicenceModules MasterModule;

		#region LM_Checksum

		protected override void CheckLM_ChecksumIsValidZDecimal()
		{
		}

		#endregion

		#region LM_LicenceType

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "This is meant to specify an exact product name.")]
		protected override void CheckLM_LicenceType()
		{
			MandatoryValidation.CheckEntered(Parent.LM_LicenceTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.LM_LicenceTypeInfo);
			if (MasterModule.LM_LicenceType == LicenceTypes.Codes.NON && (MasterModule.LM_UserCount > 0 || !MasterModule.LM_ExpiryDate.IsEmpty))
			{
				MasterModule.LM_LicenceTypeInfo.AddError("The licence type or the number of users for module " + MasterModule.LM_Calc_GroupModuleDescription + " is invalid.");
			}

			if (MasterModule.HasLineLevelOnDemandLicenceType
				&& MasterModule.LicHeader != null
				&& MasterModule.LicHeader.SupportsLineLevelOnDemandLicenceTypes != TriState.True
				&& MasterModule.LicHeader.IsInDatabase)
			{
				string warningMessage = string.Format(CultureInfo.CurrentCulture, "Licence type '{0}' might not be supported by the client's system.\r\nTry using '{1}' instead, which actually means 'Always Allow' in the older systems",
					MasterModule.LM_LicenceType, LicenceTypes.Codes.NON);
				MasterModule.LM_LicenceTypeInfo.AddWarning(warningMessage);
			}

			if (MasterModule.LM_LicenceType == LicenceTypes.Codes.NON
				&& MasterModule.LM_GroupModuleCode == LegacyLicence.Codes.Core
				&& MasterModule.LicHeader != null
				&& MasterModule.LicHeader.IsOnDemandModuleTypeAllowed
				&& MasterModule.LicHeader.SupportsLineLevelOnDemandLicenceTypes == TriState.True)
			{
				string warningMessage = "You have locked out the Core module licence required to run ediEnterprise/CargoWise One. Please ensure this is intended.";
				MasterModule.LM_LicenceTypeInfo.AddWarning(warningMessage);
			}

			if (MasterModule.LM_LicenceType == LicenceTypes.Codes.OPN && MasterModule.LicHeader != null && !MasterModule.LicHeader.SupportsOpenLicence)
			{
				string message = string.Format(CultureInfo.CurrentCulture, "Licence type '{0}' might not be supported by the client's system.\r\n. It requires build version {1} or later.",
					MasterModule.LM_LicenceType, LicenceHeader.OpenLicenceVersion.ToString());
				MasterModule.LM_LicenceTypeInfo.AddWarning(message);
			}
		}

		#endregion

		#region LM_UserCount

		protected override void CheckLM_UserCount()
		{
			if (MasterModule.LM_Calc_IsEnabled)
			{
				switch (MasterModule.LM_LicenceType)
				{
					case LicenceTypes.Codes.ODM:
						if (!MasterModule.LM_UserCount.IsEmpty
							&&
							(MasterModule.LicHeader == null || MasterModule.LicHeader.IsPureOnDemand))
						{
							MasterModule.LM_UserCountInfo.AddError(string.Format(CultureInfo.CurrentCulture, "User Count must be 0 if licence type is '{0}'", LicenceTypes.Descriptions.ODM));
						}
						break;

					case LicenceTypes.Codes.REN:
					case LicenceTypes.Codes.PUR:
					case LicenceTypes.Codes.OTM:
					case LicenceTypes.Codes.OPN:
						CompareValidation.CheckNumberGreaterThanZero(MasterModule.LM_UserCountInfo);
						break;

					case LicenceTypes.Codes.SRU:
						if (MasterModule.LM_UserCount != 9999)
						{
							MasterModule.LM_UserCountInfo.AddError(string.Format(CultureInfo.CurrentCulture, "User Count must be 9999 if licence type is '{0}'", LicenceTypes.Descriptions.SRU));
						}
						break;
				}
			}

			LicenceModules nonOnDemandParentModule = MasterModule.GetNonOnDemandParentModule();
			if (nonOnDemandParentModule != null && MasterModule.LM_UserCount > nonOnDemandParentModule.LM_UserCount)
			{
				MasterModule.LM_UserCountInfo.AddWarning(ZString.Format(ChildMustNotHaveMoreUsersThanParent, MasterModule.LM_Calc_GroupModuleDescription, nonOnDemandParentModule.LM_Calc_GroupModuleDescription));
			}
		}

		#endregion

		#region LM_ExpiryDate

		protected override void CheckLM_ExpiryDate()
		{
			if (MasterModule.LM_LicenceType == LicenceTypes.Codes.TRI)
			{
				if (MasterModule.LM_ExpiryDate.IsEmpty)
				{
					MasterModule.LM_ExpiryDateInfo.AddError(MustSpecifyExpiryDate);
				}
				else if (MasterModule.LM_ExpiryDate <= ZDateTime.Now)
				{
					MasterModule.LM_ExpiryDateInfo.AddWarning(ExpirationDateCannotBeInPast);
				}
			}
			else if (MasterModule.LM_LicenceType != LicenceTypes.Codes.TRI && MasterModule.LM_LicenceType != LicenceTypes.Codes.REN && !MasterModule.LM_ExpiryDate.IsEmpty)
			{
				MasterModule.LM_ExpiryDateInfo.AddError(CannotSpecifyExpiryDate);
			}
		}

		const string CannotSpecifyExpiryDate = "You cannot have an expiry date for this licence type";
		const string MustSpecifyExpiryDate = "You must specify an expiry date for this licence type";
		const string ExpirationDateCannotBeInPast = "The expiration date cannot be in the past";
		const string ChildMustNotHaveMoreUsersThanParent = "The module {0} should not have more users than the module {1}";

		#endregion
	}
}

