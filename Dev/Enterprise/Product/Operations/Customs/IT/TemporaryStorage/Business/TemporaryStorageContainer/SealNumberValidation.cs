using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

static class SealNumberValidation
{
	public static void ValidateSealNumber(ZString sealIdentifier, ZPropertyInfo sealInfo)
	{
		ZString invalidSealNumberMessage = Res.GetString("CA826517-DFAB-42D4-A9ED-1F7EF8034315", "The field does not match validation pattern [A-Z0-9]{1,20}: it must contain only uppercase letters and numbers, and have length from 1 to 20 characters");

		if (!sealIdentifier.IsEmpty && !Regex.IsMatch(sealIdentifier, @"^[A-Z0-9]{1,20}$"))
		{
			sealInfo.AddMessageError(invalidSealNumberMessage);
		}
	}
}
