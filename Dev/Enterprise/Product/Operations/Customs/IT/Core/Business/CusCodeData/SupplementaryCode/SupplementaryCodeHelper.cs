using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business;

public static class SupplementaryCodeHelper
{
	public static ZBool IsQVatAdditionalCode(this BaseSupplementaryCode supplementaryCode) => IsQVatSupplementaryCode(supplementaryCode?.CY_Code ?? ZString.Empty);

	public static ZBool HasMoreThanOneVatQVatAdditionalCode(this ISupplementaryCodeSupporter supplementaryCodeSupporter) => supplementaryCodeSupporter.SupplementaryCodes.Count(x => x.IsQVatAdditionalCode()) > 1;

	public static ZBool IsQVatSupplementaryCode(ZString code) => code.StartsWith(FirstCharacterVatAdditionalCode);

	public static ZBool IsSupplementaryCodeValid(this ZString code) => !code.IsEmpty && FirstCharactersValidAdditionalCode.Any(c => c == code[0]);

	const string FirstCharacterVatAdditionalCode = "Q";

	const string FirstCharactersValidAdditionalCode = "23468ABCDPQRSTUZ";
}
