using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public enum ValidateForMessageType
	{
		ACROSS,
		Both,
		B3CUSDEC,
		None,
		Default
	}

	public class CADeclarationValidator
	{
		public CADeclarationValidator(JobDeclaration declaration)
		{
			this.declaration = declaration;
			OverrideValidationType = ValidateForMessageType.Default;
		}
		readonly JobDeclaration declaration;

		public ValidateForMessageType CurrentValidationTypeRequired
		{
			get
			{
				if (OverrideValidationType != ValidateForMessageType.Default)
				{
					return OverrideValidationType;
				}
				if (declaration.IsWarehouseEntry || declaration.IsLVS)
				{
					return ValidateForMessageType.B3CUSDEC;
				}

				if (declaration.IsEnableACROSSValidation && declaration.IsEnableB3Validation)
				{
					return ValidateForMessageType.Both;
				}

				if (declaration.IsEnableACROSSValidation)
				{
					return ValidateForMessageType.ACROSS;
				}

				if (declaration.IsEnableB3Validation)
				{
					return ValidateForMessageType.B3CUSDEC;
				}

				return ValidateForMessageType.None;
			}
		}
		public ValidateForMessageType OverrideValidationType { get; set; }

		public bool IsValidationRequired(ValidateForMessageType validateType)
		{
			var currentValidationTypeRequired = CurrentValidationTypeRequired;
			return currentValidationTypeRequired != ValidateForMessageType.None && (currentValidationTypeRequired == ValidateForMessageType.Both || validateType == currentValidationTypeRequired);
		}

		public void AddMessageError(ZPropertyInfo info, string message, ValidateForMessageType validateType)
		{
			if (IsValidationRequired(validateType))
			{
				info.AddMessageError(message);
			}
		}

		public void MessageErrorIfInvalidCode(ZPropertyInfo info, ICodeDescriptionPairList list, ValidateForMessageType validateType)
		{
			if (IsValidationRequired(validateType))
			{
				ListValidation.MessageErrorIfInvalidCode(info, list);
			}
		}

		public void MessageErrorIfInvalidCode(ZPropertyInfo info, IBusinessObjectCollection list, ValidateForMessageType validateType)
		{
			if (IsValidationRequired(validateType))
			{
				ListValidation.MessageErrorIfInvalidCode(info, list);
			}
		}

		public void MessageErrorIfInvalidCodeOrEmpty(ZPropertyInfo info, ICodeDescriptionPairList list, ValidateForMessageType validateType)
		{
			if (IsValidationRequired(validateType))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(info, list);
			}
		}

		public void MessageErrorIfNotEntered(ZPropertyInfo info, string description, ValidateForMessageType validateType)
		{
			if (IsValidationRequired(validateType))
			{
				if (string.IsNullOrEmpty(description))
				{
					MandatoryValidation.MessageErrorIfNotEntered(info);
				}
				else
				{
					MandatoryValidation.MessageErrorIfNotEntered(info, description);
				}
			}
		}

		public void MessageErrorIfIsEntered(ZPropertyInfo info, string description, ValidateForMessageType validateType)
		{
			if (IsValidationRequired(validateType))
			{
				if (string.IsNullOrEmpty(description))
				{
					MandatoryValidation.MessageErrorIfIsEntered(info);
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(info, description);
				}
			}
		}

		public ZBool IsIID
		{
			get { return declaration.IsIID; }
		}
	}
}
