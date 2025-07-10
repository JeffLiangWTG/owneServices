using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "it's being used as parent in derived classes")]
public class PassarValidation
{
	public static void CheckNP70177(ZPropertyInfo targetPropertyInfo, ZString cfrReference)
	{
		if (!cfrReference.IsEmpty && !IsValidId())
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNP70177);
		}

		bool IsValidId() => Regex.IsMatch(cfrReference, @"^(CHE|1)?\d{9}$", RegexOptions.IgnoreCase);
	}
}
