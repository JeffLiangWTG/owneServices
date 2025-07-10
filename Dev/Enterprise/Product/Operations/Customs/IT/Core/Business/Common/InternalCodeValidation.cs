using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

sealed class InternalCodeValidation
{
	public void CheckInternalCode(ZPropertyInfo targetPropertyInfo, ZString internalCode)
	{
		MandatoryValidation.CheckEntered(targetPropertyInfo);

		if (!internalCode.IsEmpty)
		{
			CheckInternalCodeMinLength(targetPropertyInfo, internalCode);
			CheckInternalCodeFormat(targetPropertyInfo, internalCode);
		}
	}

	void CheckInternalCodeMinLength(ZPropertyInfo targetPropertyInfo, ZString internalCode)
	{
		const int internalCodeMinLength = 5;

		if (internalCode.Length < internalCodeMinLength)
		{
			targetPropertyInfo.AddError(ValidationCaptions.GlbMauExternalPassword.InternalCodeMustAtLeastFiveCharsLength);
		}
	}

	void CheckInternalCodeFormat(ZPropertyInfo targetPropertyInfo, ZString internalCode)
	{
		var internalCodeRegex = new Regex(InternalCodeRegexPattern);
		if (!internalCodeRegex.IsMatch(internalCode))
		{
			targetPropertyInfo.AddError(ValidationCaptions.InternalCode.InternalCodeFormat);
		}
	}

	const string InternalCodeRegexPattern = "^[A-Z0-9][A-Z0-9-_]*$";
}
