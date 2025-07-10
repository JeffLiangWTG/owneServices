using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CusGoodsLocationValidation : EU.Business.CusGoodsLocationValidation
	{
		public CusGoodsLocationValidation(CusGoodsLocation parent) : base(parent)
		{
		}

		new CusGoodsLocation Parent => (CusGoodsLocation)base.Parent;

		protected virtual string RuleC0394Code => ValidationRuleCodeConstants.C0394;

		protected override void CheckCGL_AdditionalIdentifier()
		{
			base.CheckCGL_AdditionalIdentifier();
			CheckAdditionalIdentifierRuleNR0023();
			CheckAdditionalIdentifierRuleNR0050();
			CheckAdditionalIdentifierRuleTR0069();
			if (!Parent.CGL_AdditionalIdentifier.IsEmpty)
			{
				CheckAdditionalIdentifierRuleNR0013();
			}
		}

		protected override void CheckCGL_CustomsOffice()
		{
			base.CheckCGL_CustomsOffice();
			if (!Parent.CGL_CustomsOffice.IsEmpty)
			{
				CheckCustomsOfficeRuleTR0061();
			}
		}

		protected override void CheckCGL_Qualifier()
		{
			base.CheckCGL_Qualifier();
			var parent = Parent;
			if (IsLocationUseArrivalAndArrivalMovementHeaderAuthorizationACE
				&& ValidationDecider is IArrivalPhase5CusGoodsLocationValidationDecider { IsRuleNR0011Active: true }
				&& parent.CGL_Qualifier != CusGoodsLocationQualifierList.Codes.UnLocode)
			{
				parent.CGL_QualifierInfo.AddMessageError(Res.GetString("2E15F950-9A14-4AAD-A32B-C7A38F558D81", "[NR0011] The location Qualifier must be U."));
			}
			CheckQualifierRuleNR0053();
		}

		protected override void CheckCGL_Type()
		{
			base.CheckCGL_Type();
			var parent = Parent;

			if (ValidationDecider is IArrivalPhase5CusGoodsLocationValidationDecider arrivalCusGoodsLocationValidationDecider )
			{
				if (arrivalCusGoodsLocationValidationDecider.IsRuleNR0012Active
					&& IsLocationUseArrivalAndArrivalMovementHeaderAuthorizationACE
					&& parent.CGL_Type != CusGoodsLocationTypeList.Codes.ApprovedPlace)
				{
					parent.CGL_TypeInfo.AddMessageError(Res.GetString("DA2A23A8-631F-4610-87EF-000776F949C0", "[NR0012] The location Type must be C."));
				}

				if (arrivalCusGoodsLocationValidationDecider.IsRuleNR0075Active
					&& !IsLocationUseArrivalAndArrivalMovementHeaderAuthorizationACE
					&& !IsLocationUseArrivalAndArrivalMovementHeaderAuthorizationACT
					&& parent.CGL_Type != CusGoodsLocationTypeList.Codes.DesignatedLocation)
				{
					parent.CGL_TypeInfo.AddMessageError(parent.Header.Configuration.ValidationRuleConfiguration.Messages.NR0075Message);
				}
			}
			else if (ValidationDecider is IDeparturePhase5CusGoodsLocationValidationDecider departureCusGoodsLocationValidationDecider)
			{
				var movementHeader = parent.DepartureMovementHeader;
				if (departureCusGoodsLocationValidationDecider.IsRuleNR0063Active
					&& ((parent.CGL_Type == CusGoodsLocationTypeList.Codes.DesignatedLocation && movementHeader.CusAuthorizationUsages.Any(x => x.AGC_Code == CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit))
						|| (parent.CGL_Type == CusGoodsLocationTypeList.Codes.AuthorizedPlace && !movementHeader.CusAuthorizationUsages.Any(x => x.AGC_Code == CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit)))
					&& parent.Header is NctsHeader header)
				{
					var configuration = header.Configuration.ValidationRuleConfiguration;
					parent.CGL_TypeInfo.AddMessageError(configuration.Messages.NR0063Message);
				}
			}
		}

		protected override bool IsRuleC0382Active => ValidationDecider?.IsRuleC0382Active ?? false;

		protected override IDictionary<string, (ZString ruleCode, ZPropertyInfo requiredInfo)> GetCustomsOfficeRequirementRule()
		{
			var result = base.GetCustomsOfficeRequirementRule();
			result[CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier] = (RuleC0394Code, Parent.CGL_CustomsOfficeInfo);
			return result;
		}

		public ICusGoodsLocationValidationDecider ValidationDecider => Parent.Factory.GetValue(ref validationDeciderCached, GetValidationDecider);
		CachedProperty<ICusGoodsLocationValidationDecider> validationDeciderCached;

		ICusGoodsLocationValidationDecider GetValidationDecider() => (Parent.Parent as ICusGoodsLocationProviderWithValidationDecider)?.GoodsLocationValidationDecider;

		void CheckAdditionalIdentifierRuleNR0023()
		{
			var parent = Parent;
			if (parent.CGL_AdditionalIdentifier.IsEmpty)
			{
				var header = parent.Header;
				if (parent.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.AuthorizationNumber
					&& (parent.CGL_Type == CusGoodsLocationTypeList.Codes.AuthorizedPlace || parent.CGL_Type == CusGoodsLocationTypeList.Codes.ApprovedPlace)
					&& header != null
					&& ValidationDecider is IDeparturePhase5CusGoodsLocationValidationDecider { IsRuleNR0023Active: true })
				{
					parent.AdditionalIdentifierInfo.AddMessageError(Res.GetString("B951D1B1-FEAF-4041-9276-CA722F53DA10", "{0} This field must be filled.", ValidationRuleCodeConstants.NR0023.GetRuleCodeMessagePrefix()));
				}
			}
		}

		void CheckAdditionalIdentifierRuleNR0013()
		{
			var parent = Parent;

			if (IsLocationUseArrivalAndArrivalMovementHeaderAuthorizationACE
				&& ValidationDecider is IArrivalPhase5CusGoodsLocationValidationDecider { IsRuleNR0013Active: true })
			{
				var arrivalMovementHeader = (NctsArrivalMovementHeader)parent.Parent;
				if (!CusAuthorisationHeader.Loader.HolderHasSpecificAuthorisationWithRule(parent.Factory,
					arrivalMovementHeader.AuthorizationOwner, arrivalMovementHeader.AuthorizationCode, arrivalMovementHeader.AuthorizationNumber,
					arrivalMovementHeader.CountryCode, ZDateTime.Today, CusAuthorisationRuleTypeList.Codes.Location,
					parent.CGL_AdditionalIdentifier))
				{
					parent.CGL_AdditionalIdentifierInfo.AddMessageError(Res.GetString("2368563F-BC59-4C75-A38E-9C4F8E535916", "[NR0013] Locations is not mentioned in the authorization ACE/C522. Please fill in correct UN/LOCODE out of authorization."));
				}
			}
		}

		void CheckCustomsOfficeRuleTR0061()
		{
			var parent = Parent;
			var qualifier = parent.CGL_Qualifier;

			if (qualifier == CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier)
			{
				if (parent.Header is NctsHeader nctsHeader
					&& nctsHeader.IsPhase5
					&& nctsHeader.Configuration is NctsConfiguration nctsConfiguration
					&& (parent.ArrivalMovementHeader is NctsCommonMovementHeader arrivalMovementHeader ? arrivalMovementHeader : parent.DepartureMovementHeader) is NctsCommonMovementHeader movementHeader
					&& nctsConfiguration.ValidationRuleConfiguration is ValidationRuleConfiguration validationRuleConfiguration
					&& nctsConfiguration.MovementHeaderConfiguration is MovementHeaderConfiguration movementHeaderConfiguration
					&& movementHeaderConfiguration.GetGoodsLocationValidationDecider(movementHeader) is ICusGoodsLocationValidationDecider goodsLocationValidationDecider
					&& goodsLocationValidationDecider.IsRuleTR0061Active
					&& IsCustomsOfficeNotInList())
				{
					parent.CGL_CustomsOfficeInfo.AddMessageError(validationRuleConfiguration.Messages.TR0061Message);
				}
			}
		}

		void CheckAdditionalIdentifierRuleNR0050()
		{
			var parent = Parent;
			var nctsHeader = parent.Header;
			var configuration = nctsHeader?.Configuration.ValidationRuleConfiguration;
			var isRuleNR0050Active = ValidationDecider is IDeparturePhase5CusGoodsLocationValidationDecider { IsRuleNR0050Active: true };
			var isSimplifiedNctsProcedure = nctsHeader?.MovementHeader?.IsSimplifiedNctsProcedure ?? false;
			if (isRuleNR0050Active && isSimplifiedNctsProcedure && parent.CGL_AdditionalIdentifier.IsEmpty && parent.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.AuthorizationNumber)
			{
				parent.AdditionalIdentifierInfo.AddMessageError(configuration.Messages.NR0050Message);
			}
		}

		void CheckQualifierRuleNR0053()
		{
			if (IsRuleNR0053Violated())
			{
				Parent.CGL_QualifierInfo.AddMessageError(Parent.Header?.Configuration.ValidationRuleConfiguration.Messages.NR0053Message);
			}
		}

		public bool IsRuleNR0053Violated() => IsRuleNR0053ViolatedCore();

		protected virtual bool IsRuleNR0053ViolatedCore()
		{
			var ruleViolated = false;
			var parent = Parent;
			var nctsHeader = parent.Header;
			var isNctsPhase5Departure = nctsHeader?.IsPhase5Departure ?? false;
			var configuration = nctsHeader?.Configuration.ValidationRuleConfiguration;
			var isRuleNR0053Active = configuration?.IsRuleNR0053Active ?? false;
			if (isNctsPhase5Departure && isRuleNR0053Active)
			{
				var movementHeader = nctsHeader.MovementHeader;
				if (movementHeader.BM_AdditionalDeclarationType == NctsTypeOfAdditionalDeclarationList.Codes.A)
				{
					if (parent.CGL_Qualifier != CusGoodsLocationQualifierList.Codes.UnLocode || parent.CGL_Type != CusGoodsLocationTypeList.Codes.ApprovedPlace)
					{
						ruleViolated = true;
					}
				}
				else if (movementHeader.BM_AdditionalDeclarationType == NctsTypeOfAdditionalDeclarationList.Codes.D)
				{
					if ((parent.CGL_Qualifier != CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier
							&& parent.CGL_Qualifier != CusGoodsLocationQualifierList.Codes.UnLocode
							&& !parent.CGL_Qualifier.IsEmpty)
						|| (parent.CGL_Type != CusGoodsLocationTypeList.Codes.DesignatedLocation
							&& parent.CGL_Type != CusGoodsLocationTypeList.Codes.ApprovedPlace
							&& !parent.CGL_Type.IsEmpty))
					{
						ruleViolated = true;
					}
				}
			}
			return ruleViolated;
		}

		void CheckAdditionalIdentifierRuleTR0069()
		{
			var parent = Parent;

			var isNctsPhase5Arrival = parent.Header?.IsPhase5Arrival ?? false;
			var isRuleTR0069Active = ValidationDecider is IArrivalPhase5CusGoodsLocationValidationDecider { IsRuleTR0069Active: true };

			if (isNctsPhase5Arrival && isRuleTR0069Active)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.CGL_AdditionalIdentifierInfo, messagePrefix: Parent.Header.Configuration.ValidationRuleConfiguration.Messages.TR0069RuleCode.GetRuleCodeMessagePrefix(true));
			}
		}

		bool IsCustomsOfficeNotInList()
		{
			var parent = Parent;
			var customsOffice = parent.CGL_CustomsOffice;
			var cusCodeList = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(
				parent.Factory,
				customsOffice,
				parent.Header.DefaultDataGroupingCode,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice,
				ZDateTime.Today);
			return cusCodeList == null;
		}

		bool IsLocationUseArrivalAndArrivalMovementHeaderAuthorizationACE
		{
			get
			{
				var parent = Parent;
				return parent.CGL_LocationUse == CusGoodsLocationUseList.Codes.Arrival &&
						parent.Parent is NctsArrivalMovementHeader arrivalMovementHeader &&
						arrivalMovementHeader.AuthorizationCode == NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForUnionTransit;
			}
		}

		bool IsLocationUseArrivalAndArrivalMovementHeaderAuthorizationACT
		{
			get
			{
				var parent = Parent;
				return parent.CGL_LocationUse == CusGoodsLocationUseList.Codes.Arrival &&
						parent.Parent is NctsArrivalMovementHeader arrivalMovementHeader &&
						arrivalMovementHeader.AuthorizationCode == NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForTirProcedure;
			}
		}
	}
}
