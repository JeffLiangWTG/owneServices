using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.NCTS;

public class CusGoodsLocationAddressLookups(CusGoodsLocationAddress parent) : EU.Business.CusGoodsLocationAddressLookups(parent)
{
	public new CusGoodsLocationAddress Parent => (CusGoodsLocationAddress)base.Parent;

	public override ICollection AuthorisationNumberList
	{
		get
		{
			var location = Parent.GoodsLocation;
			var header = location.Header;
			return header.IsPhase4 || !(location.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.AuthorizationNumber && location.CGL_Type == CusGoodsLocationTypeList.Codes.AuthorizedPlace) ? base.AuthorisationNumberList : (header.IsDepartureMovement ? GetDepartureCusAuthorisationLOCValues() : GetDestinationCusAuthorisationLOCValues());
		}
	}

	CodeDescriptionPairList GetDepartureCusAuthorisationLOCValues()
	{
		var header = Parent.GoodsLocation.Header;
		var result = new CodeDescriptionPairList();
		if (header.Consignor?.E2_OA_Address is ZGuid { IsEmpty: false } validHolderAddress
			&& header.Principal?.OrganisationPK is ZGuid { IsEmpty: false } holderPk
			&& header.MovementHeader.DepartureCustomsOfficeCode is ZString { IsEmpty: false } officeCode)
		{
			var authorisations = CusAuthorisationHeader.Loader.GetAuthorisationsForAddressesAndPermitHolder(Factory, header.CountryCode, [CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit], ZDate.Today, [holderPk], [validHolderAddress]);
			result = GetCusAuthorisationLOCValues(authorisations, officeCode);
		}
		return result;
	}

	CodeDescriptionPairList GetDestinationCusAuthorisationLOCValues()
	{
		var header = Parent.GoodsLocation.Header;
		var result = new CodeDescriptionPairList();
		var arrivalMovementHeader = header.ArrivalMovementHeader;
		if (header.DestinationTrader?.OrganisationPK is ZGuid { IsEmpty: false } holderPk
			&& arrivalMovementHeader.AuthorizationCode is ZString { IsEmpty: false } authType
			&& arrivalMovementHeader.AuthorizationNumber is ZString { IsEmpty: false } authNumber
			&& arrivalMovementHeader.DestinationCustomsOfficeCode is ZString { IsEmpty: false } officeCode)
		{
			var authorisations = CusAuthorisationHeader.Loader.GetAuthorisations(Factory, header.CountryCode, [authType], ZDateTime.Now, [holderPk]).Where(authHeader => authHeader.CPH_Number == authNumber);
			result = GetCusAuthorisationLOCValues(authorisations, officeCode);
		}
		return result;
	}

	CodeDescriptionPairList GetCusAuthorisationLOCValues(IEnumerable<CusAuthorisationHeader> authorisations, ZString officeCode)
	{
		var result = new CodeDescriptionPairList();
		authorisations.SelectMany(authHeader => authHeader.CusAuthorisationRules.Where(rule => rule.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.Location && rule.LinkedCusAuthorisationRules.Any(linkedRule => linkedRule.CPR_RuleCode == Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice && linkedRule.CPR_ValueFrom == officeCode)))
				.ForEach(rule => result.AddPair(rule.CPR_ValueFrom, rule.CPR_Description));
		return result;
	}
}
