using System;

namespace Enterprise.Customs.EU.Business;

public class ValidationResult
{
	public static ValidationResult Valid => valid ?? (valid = new ValidationResult(true, string.Empty));

	[ThreadStatic]
	static ValidationResult valid;

	public static ValidationResult Invalid(string message) => new ValidationResult(false, message);

	ValidationResult(bool isValid, string message)
	{
		IsValid = isValid;
		Message = message;
	}

	public bool IsValid { get; }
	public string Message { get; }
}
