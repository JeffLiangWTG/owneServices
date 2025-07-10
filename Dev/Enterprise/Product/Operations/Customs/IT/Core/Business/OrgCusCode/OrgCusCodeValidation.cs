using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business;

public class OrgCusCodeValidation : EU.Business.OrgCusCodeValidation, Integration.Customs.IT.IOrgCusCodeValidation
{
	public OrgCusCodeValidation(OrgCusCode parent)
		: base(parent)
	{
	}

	protected override void CheckOK_CustomsRegNo()
	{
		base.CheckOK_CustomsRegNo();

		CheckDanAndDatCodeTypeCannotCoexistWithSameValue(Parent.OK_CustomsRegNoInfo);
		CheckCcpCodeExistsInAuthorisations();
	}

	void CheckDanAndDatCodeTypeCannotCoexistWithSameValue(ZPropertyInfo propertyInfo)
	{
		var codeType = Parent.OK_CodeType;
		if (codeType == OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber || codeType == ItalyOrgCusCodeInfo.OrgCusCodes.DefermentApprovaNumberForTrieste)
		{
			var defermentApprovaNumberCodeTypeCannotExistsWithSameValue = codesCannotExistsWithSameValue[codeType];
			if (Header != null)
			{
				if (Header.CustomsCodes.Cast<OrgCusCode>().Any(x => x.OK_CodeType == defermentApprovaNumberCodeTypeCannotExistsWithSameValue && x.OK_CustomsRegNo == Parent.OK_CustomsRegNo))
				{
					propertyInfo.AddError(ValidationCaptions.OrgCusCode.EachDanAndDatMustBeUniqueMessage);
				}
			}
		}
	}

	void CheckCcpCodeExistsInAuthorisations()
	{
		var ccpValue = Parent.OK_CustomsRegNo;
		if (Parent.OK_CodeType == OrgCusCode.CodeTypes.ControlledPremisesID && !ccpValue.IsEmpty && !Header.HasLocAuthorisation(ccpValue))
		{
			Parent.OK_CustomsRegNoInfo.AddWarning(ValidationCaptions.OrgHeader.CcpDoesNotHaveValidLocAuthorisation);
		}
	}

	OrgHeader Header => Parent.Header;

	readonly ImmutableDictionary<string, string> codesCannotExistsWithSameValue = new Dictionary<string, string>()
	{
		{ OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber , ItalyOrgCusCodeInfo.OrgCusCodes.DefermentApprovaNumberForTrieste },
		{ ItalyOrgCusCodeInfo.OrgCusCodes.DefermentApprovaNumberForTrieste ,OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber }
	}.ToImmutableDictionary();
}
