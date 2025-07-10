using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Declaration;

public partial class ImportJobDeclarationValidation
{
	protected JobDeclaration JobDeclaration => Parent;

	protected override void CheckJE_DestinationState()
	{
		base.CheckJE_DestinationState();

		var declaration = Parent;
		if (declaration.JE_GoodsDestination.StartsWith(Core.Constants.CountryCodes.Spain, StringComparison.OrdinalIgnoreCase)
			&& declaration.HasAnyDiffH2Entry
			&& declaration.HasAnyDiffT2CEntry)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_DestinationStateInfo, declaration.Lookups.DestinationStateIslandCodeList);
		}
	}

	protected override void CheckJE_OtherEmailAddr()
	{
		base.CheckJE_OtherEmailAddr();
		if (!Parent.JE_OtherEmailAddrInfo.HasMessageErrors())
		{
			ValidateEmailAddress(Parent.JE_OtherEmailAddrInfo, Parent.ZG_OtherEmailAddr);
		}
	}

	protected void ValidateEmailAddress(ZPropertyInfo propertyInfo, params ZString[] emailAddresses)
	{
		foreach (ZString emailAddress in emailAddresses)
		{
			if (emailAddress.Trim().IsEmpty)
			{
				propertyInfo.AddWarning(Res.GetString("D9FD54F4-2FB3-4ED9-B99E-9E9062F7E1E0", "If this email is not provided, some relevant communications from ES Authorities may not be received."));
			}
			else if (!EmailAddressValidation.IsEmailAddressValid(emailAddress))
			{
				propertyInfo.AddMessageError(Res.GetString("D407CF68-880E-4DB8-A7FB-1AB0FB8EA6A8", @"The email address ""{0}"" is invalid.", emailAddress));
			}
		}
	}

	protected override void CheckJE_AgreedPlaceCode()
	{
		base.CheckJE_AgreedPlaceCode();
		if (HasAnyDiffT2CAndT2lEntry && HasEntryHeaderValidationModeImportNone && HasAnyDiffH2Entry)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ZG_AgreedPlaceCodeInfo);
		}
	}

	protected override void CheckJE_Box18TransportID()
	{
		base.CheckJE_Box18TransportID();

	if (HasAnyDiffT2CAndT2lEntry && HasAnyDiffH2Entry && HasEntryHeaderValidationModeImportNoneOrPDI && Parent.ZG_Box18TransportID.IsEmpty && JobDeclaration.IsTransportModeInList(ESConstants.TransportModeTypes.TransportModesForImportTransportIDAndNationality))
	{
		Parent.ZG_Box18TransportIDInfo.AddMessageError(Res.GetString("BFBB3DC3-B29E-426C-94E2-ADC5E24C935F", "If transport is not Post/Mail, Rail Freight or Fixed Transport Installations, [18] Transport ID (Inland) must be filled."));
	}
}

bool HasAnyDiffT2CAndT2lEntry => JobDeclaration.HasAnyDiffT2CAndT2lEntry;

	bool HasAnyDiffH2Entry => JobDeclaration.HasAnyDiffH2Entry;
}
