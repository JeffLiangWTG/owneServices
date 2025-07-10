using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class RequiringEoriUcc6TraderJobDocAddressValidation : Ucc6TraderJobDocAddressValidation
{
	public RequiringEoriUcc6TraderJobDocAddressValidation(AutoJobDocAddress parent, string traderName, JobDeclaration declaration, bool isMandatory = true) : base(parent, traderName, declaration, isMandatory)
	{
	}

	protected override ZString[] GetRequiredCodeTypeListForEuropeanTrader(OrgHeader organisation) => requiredCoeTypeList.ToArray();

	protected override ZString[] GetRequiredCodeTypeListForNotEuropeanTrader(OrgHeader organisation) => requiredCoeTypeList.ToArray();

	protected override string GetMissingCustomsCodeMessageError()
	{
		if (Parent.E2_AddressType == DocAddressTypes.Codes.ImporterDocumentaryAddress)
		{
			return ValidationCaptions.UCC6TraderJobDocAddressValidation.EoriCodeForImporterIsRequired;
		}

		return ValidationCaptions.UCC6TraderJobDocAddressValidation.GetEoriOrTCUCodesForTraderAreRequiredCaption(TraderName);
	}

	readonly HashSet<ZString> requiredCoeTypeList = new HashSet<ZString> { OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU };
}
