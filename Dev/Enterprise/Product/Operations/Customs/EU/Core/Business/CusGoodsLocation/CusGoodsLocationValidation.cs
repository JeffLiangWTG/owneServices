using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.EU.Business
{
	public class CusGoodsLocationValidation : Customs.Business.CusGoodsLocationValidation
	{
		public CusGoodsLocationValidation(CusGoodsLocation parent) : base(parent)
		{
		}

		protected new CusGoodsLocation Parent => (CusGoodsLocation)base.Parent;

		protected override void CheckCGL_Type()
		{
			base.CheckCGL_Type();
			var parent = Parent;
			if (parent.Parent is TemporaryStorageHeader temporaryStorageHeader)
			{
				if (parent.CGL_Type.IsEmpty)
				{
					CheckRuleO58(temporaryStorageHeader);
				}
				else if (temporaryStorageHeader.AMA_MessageType.EqualsIgnoringCase(PNTSMessageTypeList.Codes.PreLodgedTempStorage) && !IsAuthorisedTypeForPreLodgedTempStorage(parent))
				{
					parent.CGL_TypeInfo.AddMessageError(Res.GetString("E0F3D412-80F7-40D3-8F64-1C6441FBF7D2", "For a Pre-Lodged declaration only Type A , B , C are allowed"));
				}
			}
		}

		void CheckRuleO58(TemporaryStorageHeader temporaryStorageHeader)
		{
			if (!temporaryStorageHeader.AMA_MessageType.EqualsIgnoringCase(PNTSMessageTypeList.Codes.PreLodgedTempStorage)
				|| (temporaryStorageHeader.AMA_MessageType.EqualsIgnoringCase(PNTSMessageTypeList.Codes.PreLodgedTempStorage) && temporaryStorageHeader.PlaceOfUnloading.IsEmpty))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CGL_TypeInfo);
			}
		}

		protected override void CheckCGL_Qualifier()
		{
			base.CheckCGL_Qualifier();
			var parent = Parent;
			var type = parent.CGL_Type.ToUpperInvariant();
			var qualifier = parent.CGL_Qualifier.ToUpperInvariant();

			if (parent.Parent is TemporaryStorageHeader)
			{
				MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(parent.CGL_QualifierInfo, parent.CGL_TypeInfo);
			}

			if (!type.IsEmpty && !qualifier.IsEmpty && parent.Parent is TemporaryStorageHeader && IsQualifierInvalidForType(type, qualifier))
			{
				parent.CGL_QualifierInfo.AddMessageError(Res.GetString("F88B7471-6CAE-4656-B38A-4E100628E088", "The chosen Qualifier does not match the Location Type."));
			}

			var goodsLocationAddressValidation = parent.Address.Validation;

			goodsLocationAddressValidation.ValidateE2_GovRegNum();
			goodsLocationAddressValidation.ValidateE2_Address1AndE2_Address2();
			goodsLocationAddressValidation.ValidateE2_City();
			goodsLocationAddressValidation.ValidateE2_RN_NKCountryCode();
			goodsLocationAddressValidation.ValidateE2_Postcode();
			goodsLocationAddressValidation.ValidateE2_GeoLocation();
			goodsLocationAddressValidation.ValidateE2_AdditionalAddressInformation();
		}

		protected virtual bool IsQualifierInvalidForType(string type, string qualifier) =>
			type switch
			{
				CusGoodsLocationTypeList.Codes.Other => !((ZString)qualifier).In(AuthorisedQualifierForOtherTypeList()),
				CusGoodsLocationTypeList.Codes.DesignatedLocation => qualifier is not(CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier or CusGoodsLocationQualifierList.Codes.UnLocode),
				CusGoodsLocationTypeList.Codes.AuthorizedPlace or CusGoodsLocationTypeList.Codes.ApprovedPlace => qualifier is not CusGoodsLocationQualifierList.Codes.UnLocode,
				_ => false,
			};

		protected override void CheckCGL_AdditionalIdentifier()
		{
			base.CheckCGL_AdditionalIdentifier();
			var parent = Parent;
			var qualifier = parent.CGL_Qualifier.ToUpperInvariant();
			var propertyInfo = parent.CGL_AdditionalIdentifierInfo;

			if (parent.CGL_AdditionalIdentifier.IsEmpty)
			{
				if (AdditionalIdentifierRequirementRule.TryGetValue(qualifier, out var requirement))
				{
					propertyInfo.AddMessageError(CusGoodsLocationValidationHelper.YouHaveNotEnteredMessageWithQualifier(
						requirement.requiredInfo.HumanReadableName,
						qualifier,
						requirement.ruleCode,
						IncludeRuleCode
					));
				}
			}
			else if (ShouldPerformCGL_AdditionalInfoListValidation)
			{
				ListValidation.MessageErrorIfInvalidCode(propertyInfo, (IBusinessObjectCollection)parent.Lookups.UnlocodeList);
			}
		}
		bool ShouldPerformCGL_AdditionalInfoListValidation => ShouldPerformCGL_AdditionalInfoListValidationCore;
		protected virtual bool ShouldPerformCGL_AdditionalInfoListValidationCore => Parent.CGL_Qualifier.EqualsIgnoringCase(CusGoodsLocationQualifierList.Codes.UnLocode);

		public virtual bool IncludeRuleCode => true;

		IDictionary<string, (ZString ruleCode, ZPropertyInfo requiredInfo)> fAdditionalIdentifierRequirementRule;
		IDictionary<string, (ZString ruleCode, ZPropertyInfo requiredInfo)> AdditionalIdentifierRequirementRule =>
			fAdditionalIdentifierRequirementRule ??= GetAdditionalIdentifierRequirementRule();

		protected virtual IDictionary<string, (ZString ruleCode, ZPropertyInfo requiredInfo)> GetAdditionalIdentifierRequirementRule()
		{
			var parent = Parent;
			var result = new Dictionary<string, (ZString, ZPropertyInfo)>
			{
				{ CusGoodsLocationQualifierList.Codes.UnLocode, (ValidationRuleCodeConstants.Codes.C0061, parent.UnlocodeInfo) },
			};

			if (IsRuleC0382Active)
			{
				var optionalHouseNumberCountryList = parent.Factory.GetCountryAddressPostcodeOnlyList();
				if (!optionalHouseNumberCountryList.ContainsCode(parent.Address.E2_RN_NKCountryCode))
				{
					result[CusGoodsLocationQualifierList.Codes.PostcodeAddress] = (ValidationRuleCodeConstants.Codes.C0382, parent.CGL_AdditionalIdentifierInfo);
				}
			}

			return result;
		}

		protected virtual bool IsRuleC0382Active => Parent.Parent is Declaration.CusEntryInstruction entryInstruction
			&& entryInstruction.Validation.ValidationDecider is { IsRuleC0382ActiveForGoodsLocationAddressHouseNumber: true };

		protected override void CheckCGL_CustomsOffice()
		{
			base.CheckCGL_CustomsOffice();
			var parent = Parent;
			var propertyInfo = parent.CGL_CustomsOfficeInfo;

			if (parent.CGL_CustomsOffice.IsEmpty)
			{
				var qualifier = parent.CGL_Qualifier.ToUpperInvariant();
				if (CustomsOfficeRequirementRule.TryGetValue(qualifier, out var requirement))
				{
					propertyInfo.AddMessageError(CusGoodsLocationValidationHelper.YouHaveNotEnteredMessageWithQualifier(
						requirement.requiredInfo.HumanReadableName,
						qualifier,
						requirement.ruleCode
					));
				}
			}
			else
			{
				ValidateCGL_CustomsOfficeInvalidCode(propertyInfo);
			}
		}

		IDictionary<string, (ZString ruleCode, ZPropertyInfo requiredInfo)> fCustomsOfficeRequirementRule;
		IDictionary<string, (ZString ruleCode, ZPropertyInfo requiredInfo)> CustomsOfficeRequirementRule =>
			fCustomsOfficeRequirementRule ?? (fCustomsOfficeRequirementRule = GetCustomsOfficeRequirementRule());

		protected virtual IDictionary<string, (ZString ruleCode, ZPropertyInfo requiredInfo)> GetCustomsOfficeRequirementRule()
			=> new Dictionary<string, (ZString, ZPropertyInfo)>
			{
				{ CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier, (ValidationRuleCodeConstants.Codes.C0062, Parent.CGL_CustomsOfficeInfo) },
			};

		protected virtual void ValidateCGL_CustomsOfficeInvalidCode(ZPropertyInfo propertyInfo)
		{
			ListValidation.MessageErrorIfInvalidCode(propertyInfo);
		}

		bool IsAuthorisedTypeForPreLodgedTempStorage(CusGoodsLocation parent)
		{
			return parent.CGL_Type.ToUpperInvariant().In(new ZString[] { CusGoodsLocationTypeList.Codes.DesignatedLocation, CusGoodsLocationTypeList.Codes.ApprovedPlace, CusGoodsLocationTypeList.Codes.AuthorizedPlace });
		}

		protected ZString[] AuthorisedQualifierForOtherTypeList() => new ZString[]
		{
			CusGoodsLocationQualifierList.Codes.UnLocode,
			CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier,
			CusGoodsLocationQualifierList.Codes.GnssCoordinates,
			CusGoodsLocationQualifierList.Codes.EoriNumber,
			CusGoodsLocationQualifierList.Codes.AuthorizationNumber
		};
	}
}
