using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business;

public class CusGoodsLocationValidation : EU.NCTS.Business.CusGoodsLocationValidation
{
	public CusGoodsLocationValidation(CusGoodsLocation parent) : base(parent)
	{
	}

	new CusGoodsLocation Parent => (CusGoodsLocation)base.Parent;

	protected override void CheckCGL_Type()
	{
		if (!Parent.IsParentIncidentPhase5Arrival)
		{
			base.CheckCGL_Type();
		}
	}

	protected override bool IsRuleNR0053ViolatedCore() => (!Parent.CGL_Type.IsEmpty || !Parent.CGL_Qualifier.IsEmpty) && base.IsRuleNR0053ViolatedCore();

	protected override void CheckCGL_AdditionalIdentifier()
	{
		base.CheckCGL_AdditionalIdentifier();

		var parent = Parent;
		var additionalIdentifier = parent.CGL_AdditionalIdentifier;
		if (parent.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier && parent.Parent is NctsDepartureMovementHeader movementHeader)
		{
			var depCustomsOffice = movementHeader.CustomsOfficesForDeparture.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.IsOfficeDeparture);
			if (depCustomsOffice != null && depCustomsOffice.CY_Data.StartsWith(Core.Constants.CountryCodes.Belgium) && (!additionalIdentifier.StartsWith(Core.Constants.CountryCodes.Belgium) || additionalIdentifier.Length != 8))
			{
				parent.CGL_AdditionalIdentifierInfo.AddMessageError(Res.GetString("CC012D7E-7D2F-4356-8727-1220365396DB", "The customs office of the location of goods, must be a Belgian Customs Office that is 8 positions long."));
			}
		}
	}

	protected override IDictionary<string, (ZString ruleCode, ZPropertyInfo requiredInfo)> GetCustomsOfficeRequirementRule()
		=> new Dictionary<string, (ZString, ZPropertyInfo)>
		{
			{ CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier, (RuleC0394Code, Parent.CGL_CustomsOfficeInfo) },
		};
}
