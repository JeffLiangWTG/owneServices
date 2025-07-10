using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.AFR.Business
{
	public static class VesselDetailValidationHelper
	{
		public static void CheckCarrierCode(ZPropertyInfo targetInfo, JobDocAddress carrier)
		{
			var targetValue = new ZString(targetInfo.Value.ToString());
			carrier?.Validation.ValidateOrganisationPK();
			MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			targetInfo.AddMessageErrorIfContainsInvalidNACCSCharacter(targetValue);
			if (!targetValue.IsValidBillPrefix())
			{
				targetInfo.AddMessageError(ValidationConstants.Header.CarrierCodeInvalid);
			}
		}

		public static void CheckVesselName(ZPropertyInfo targetInfo, RefVessel vessel)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(targetInfo);
			targetInfo.AddMessageErrorIfContainsInvalidNACCSCharacter(new ZString(targetInfo.Value));
			if (vessel == null)
			{
				targetInfo.AddWarning(ValidationConstants.Header.VesselIsNotOnFile);
			}
		}

		public static void CheckCallSign(ZPropertyInfo targetInfo)
		{
			var callSign = new ZString(targetInfo.Value);
			MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			targetInfo.AddMessageErrorIfContainsInvalidNACCSCharacter(callSign);
			if (callSign.Length > 9)
			{
				targetInfo.AddMessageError(ValidationConstants.Header.VesselCallSignReachingMaxAllowed);
			}
			else if (callSign.Length < 3 || ValidationUtils.IsInappropriateVesselCode(callSign))
			{
				targetInfo.AddWarning(ValidationConstants.Header.InappropriateVesselCode(callSign));
			}
		}

		public static void CheckCountryOfReg(ZPropertyInfo targetInfo)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(targetInfo);
			targetInfo.AddMessageErrorIfContainsInvalidNACCSCharacter(new ZString(targetInfo.Value));
		}

		public static void CheckVoyageNumber(ZPropertyInfo targetInfo)
		{
			MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			targetInfo.AddMessageErrorIfContainsInvalidNACCSCharacter(new ZString(targetInfo.Value));
		}

		public static void CheckDischargePortCode(ZPropertyInfo targetInfo)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(targetInfo);
			targetInfo.AddMessageErrorIfContainsInvalidNACCSCharacter(new ZString(targetInfo.Value));
		}

		public static void CheckDischargePortSuffix(ZPropertyInfo targetInfo)
		{
			var targetValue = new ZString(targetInfo.Value);
			if (targetValue.KeepChars("123456789") != targetValue)
			{
				targetInfo.AddMessageError(ValidationConstants.Header.InvalidPortSuffix);
			}
		}

		public static void CheckLoadingPortCode(ZPropertyInfo targetInfo, RefUNLOCO loading)
		{
			var targetValue = targetInfo.Value;
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(targetInfo);
			targetInfo.AddMessageErrorIfContainsInvalidNACCSCharacter(new ZString(targetValue));
			if (loading != null)
			{
				var portName = loading.RL_PortName;
				if (loading.Country != null && loading.Country.Code == Core.Constants.CountryCodes.Japan)
				{
					targetInfo.AddMessageError(ValidationConstants.Header.LoadingPortCannotBeInJP);
				}
				else if (portName.ContainsInvalidNACCSCharacters())
				{
					targetInfo.AddMessageError(ValidationConstants.Shared.InvalidNACCSChar(loading.RL_PortNameInfo.HumanReadableName));
				}
				else if (portName.Length > ValidationConstants.Constants.PortNameMaxLength)
				{
					targetInfo.AddWarning(ValidationConstants.Shared.PortNameLengthExceeded);
				}
			}
		}

		public static void CheckLoadingPortSuffix(ZPropertyInfo targetInfo)
		{
			var targetValue = new ZString(targetInfo.Value);
			if (targetValue.KeepChars("123456789") != targetValue)
			{
				targetInfo.AddMessageError(ValidationConstants.Header.InvalidPortSuffix);
			}
		}

		public static void CheckETD(ZPropertyInfo targetInfo)
		{
			MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
		}

		public static void CheckETA(ZPropertyInfo targetInfo, ZDateTime eTD)
		{
			var eTA = new ZDateTime(targetInfo.Value);
			MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			if (eTA < eTD)
			{
				targetInfo.AddMessageError(ValidationConstants.Header.ETAShouldBeNoEarlierThanETD);
			}
		}
	}
}
