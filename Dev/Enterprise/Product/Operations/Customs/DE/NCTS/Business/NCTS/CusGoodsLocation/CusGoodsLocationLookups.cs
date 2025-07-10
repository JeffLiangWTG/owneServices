using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class CusGoodsLocationLookups : EU.NCTS.Business.CusGoodsLocationLookups
	{
		public CusGoodsLocationLookups(CusGoodsLocation parent) : base(parent)
		{
		}

		protected new CusGoodsLocation Parent => (CusGoodsLocation)base.Parent;

		public override CodeDescriptionPairList QualifierList
		{
			get
			{
				var parentIsMovementHeader = Parent.ParentIsMovementHeader;
				return Factory.GetCachedValue($"DE.NCTS.CusGoodsLocationLookups.QualifierList_{nameof(Parent.ParentIsMovementHeader)}_{parentIsMovementHeader}",
					() => parentIsMovementHeader ? GetMovementHeaderQualifierList() : base.QualifierList);
			}
		}

		public CodeDescriptionPairList AdditionalIdentifierList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var parent = Parent;
				var header = parent.Header;
				if (header.IsDepartureMovement)
				{
					var departureCustomsOfficeCode = header.MovementHeader.CustomsOffices.GetFirstElementHaving(EuOfficeCodesTypes.Codes.OfficeOfDeparture)?.CY_Data ?? ZString.Empty;
					if (!departureCustomsOfficeCode.IsEmpty)
					{
						var cusAuthorizationUsage = header.IsPhase5
							? (IBusinessObjectCollection<CusAuthorizationUsage>)header.MovementHeader.CusAuthorizationUsages
							: header.CusAuthorizationUsages;
						var acrAuthorization = cusAuthorizationUsage.FirstOrDefault(x => x.AGC_Code == CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit);
						if (acrAuthorization != null)
						{
							result.AddRange(DE.Business.CusAuthorizationHelper.GetCachedAuthorizationRules(Factory, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, acrAuthorization.AGC_OH_Owner, Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, departureCustomsOfficeCode, ZDate.Today, acrAuthorization.AGC_Number));
						}
					}
					return result;
				}
				else
				{
					var arrivalMovement = header.ArrivalMovementHeader;
					var addressAuthorisationNumber = parent.AddressAuthorisationNumber;
					var destinationCustomsOffice = header.IsPhase5 ? arrivalMovement.DestinationCustomsOfficeCodeForArrival : header.DestinationCustomsOfficeCodeForArrival;
					var authorizationType = arrivalMovement.AuthorizationCode;
					var addressIdentificationHolderPK = parent.AddressIdentificationHolderPK;
					result.AddRange(DE.Business.CusAuthorizationHelper.GetCachedAuthorizationRules(Factory, authorizationType, addressIdentificationHolderPK, Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, destinationCustomsOffice, ZDate.Today, addressAuthorisationNumber));
					return result;
				}
			}
		}

		static CodeDescriptionPairList GetMovementHeaderQualifierList()
		{
			var qualifierList = new CodeDescriptionPairList();
			qualifierList.AddPair(CusGoodsLocationQualifierList.Codes.AuthorizationNumber, CusGoodsLocationQualifierList.Descriptions.AuthorizationNumber);

			return qualifierList;
		}
	}
}
