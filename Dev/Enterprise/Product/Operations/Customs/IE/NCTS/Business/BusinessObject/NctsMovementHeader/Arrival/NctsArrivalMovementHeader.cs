using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class NctsArrivalMovementHeader : EU.NCTS.Business.NctsArrivalMovementHeader
		, Integration.Customs.IENCTS.IArrivalMovementHeader
		, IAdditionalBusinessObjectFetchStrategyProvider
	{
		public NctsArrivalMovementHeader(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}
		public new NctsHeader Header => (NctsHeader)base.Header;

		protected override IDictionary<ZString, Type> GetCusCodeDataTypesCore()
		{
			var result = base.GetCusCodeDataTypesCore();
			result[EU.Business.CusCodeDataTypeList.Codes.OfficeCode] = typeof(NctsIEOfficeCode);
			return result;
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		}

		protected override CusInBondMoveHeaderValidation GetNewValidation() => new NctsArrivalMovementHeaderValidation(this);

		public new NctsArrivalMovementHeaderValidation Validation => (NctsArrivalMovementHeaderValidation)base.Validation;

		protected override void SetDefaultValuesAfterNctsHeaderIsSet(EU.NCTS.Business.NctsHeader header)
		{
			base.SetDefaultValuesAfterNctsHeaderIsSet(header);
			BM_ArrivalDate = ZDateTime.Now;
			AuthorizationCode = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
			Header.BH_ExportFlag = YesNoList.Codes.No;
		}

		protected override void UpdateAfterAuthorizationDataChange()
		{
			base.UpdateAfterAuthorizationDataChange();
			if (IsPhase5Arrival)
			{
				DefaultArrivalLocationCodeFromCusAuthorisationIfBlank();
			}
		}

		void DefaultArrivalLocationCodeFromCusAuthorisationIfBlank()
		{
			var header = Header;
			
			if (!AuthorizationCode.IsEmpty && AuthorizationCode == CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit && !header.DestinationTrader.OrganisationPK.IsEmpty)
			{
				var authorizations = CusAuthorisationHeader.Loader.GetAuthorisations(Factory, header.CountryCode, new[] { AuthorizationCode }, ZDate.Today, header.DestinationTrader.OrganisationPK);
				if (authorizations.Length > 0)
				{
					var authorizationHeader = authorizations[0];
					if (authorizationHeader != null && (GoodsLocation?.Address?.AuthorisationNumber ?? ZString.Empty).IsEmpty && GetLocationCodeFromCusAuthorisation(authorizationHeader) is LinkedCusAuthorisationRule locationCode)
					{
						GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
						GoodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
						GoodsLocation.CGL_CustomsOffice = locationCode?.CPR_ValueFrom ?? ZString.Empty;
					}
				}
			}
		}

		LinkedCusAuthorisationRule GetLocationCodeFromCusAuthorisation(CusAuthorisationHeader authorizationToUse)
		{
			if(authorizationToUse.CusAuthorisationRules?.FirstOrDefault(r => r.CPR_RuleCode == EU.Business.CusAuthorisationRuleTypeList.Codes.Location && !r.CPR_ValueFrom.IsEmpty) is CusAuthorisationRule rulesToConsider)
			{
				return rulesToConsider?.LinkedCusAuthorisationRules.FirstOrDefault(lcr => lcr.CPR_RuleCode == Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice && !lcr.CPR_ValueFrom.IsEmpty);
			}
			return null;
		}
	}
}
