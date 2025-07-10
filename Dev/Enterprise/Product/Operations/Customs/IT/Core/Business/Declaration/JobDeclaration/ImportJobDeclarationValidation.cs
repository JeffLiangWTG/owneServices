using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.IT.Business.SADConstants;

namespace Enterprise.Customs.IT.Business.Declaration;

public class ImportJobDeclarationValidation : CommonJobDeclarationValidation
{
	public ImportJobDeclarationValidation(JobDeclaration parent) : base(parent)
	{
	}

	protected override void CheckJE_LocationOfGoods()
	{
		switch (Parent.JE_LocationQualifier)
		{
			case GoodsLocationQualifierList.Codes.GoodsAreInCustomsAreaForInspection:
				CheckJE_LocationOfGoodsWhenQualifierIsD();
				break;
			case GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuitAtAnAuthorizedLocation:
				CheckJE_LocationOfGoodsWhenQualifierIsFC();
				break;
			case ImportGoodsLocationQualifierWithAuthorisationList.Codes.AuthorizedPlace:
			case ImportGoodsLocationQualifierWithAuthorisationList.Codes.ApprovedPlace:
				CheckJE_LocationOfGoodsWhenQualifierIsLBOrLC();
				break;
		}
	}

	void CheckJE_LocationOfGoodsWhenQualifierIsLBOrLC()
	{
		if (!IsAuthorisationNumberEmpty)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_LocationOfGoodsInfo, (CodeDescriptionPairList)Parent.Lookups.Locations);
		}
	}

	void CheckJE_LocationOfGoodsWhenQualifierIsFC()
	{
		if (IsAuthorisationNumberEmpty)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_LocationOfGoodsInfo);
		}
	}

	void CheckJE_LocationOfGoodsWhenQualifierIsD()
	{
		if (IsAuthorisationNumberEmpty)
		{
			var locationOfGoodsInfo = Parent.JE_LocationOfGoodsInfo;
			var officeOfPresentation = Parent.JE_CustomsOffice;
			var locationOfGoods = Parent.JE_LocationOfGoods;

			ListValidation.MessageErrorIfInvalidCodeOrEmpty(locationOfGoodsInfo, Parent.Lookups.CustomsOffices);

			if (!officeOfPresentation.IsEmpty && !locationOfGoods.IsEmpty && locationOfGoods != officeOfPresentation)
			{
				locationOfGoodsInfo.AddWarning(ValidationCaptions.JobDeclaration.GoodsLocationCustomsOfficeDiffersFromOfficeOfPresentation);
			}
		}
	}

	protected override void CheckJE_ShipmentIncoTerm()
	{
		base.CheckJE_ShipmentIncoTerm();

		MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_ShipmentIncoTermInfo);
	}

	protected override void CheckJE_SubLocationOfGoods()
	{
		base.CheckJE_SubLocationOfGoods();

		if (Parent.JE_SubLocationOfGoods.Length > CustomsFieldMaxLength.PlaceOfUnloading)
		{
			Parent.JE_SubLocationOfGoodsInfo.AddMessageError(ValidationCaptions.Shared.GetFieldExceedsCustomsMaxLengthCaption(CustomsFieldMaxLength.PlaceOfUnloading));
		}

		switch (Parent.JE_LocationQualifier)
		{
			case GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuitAtAnAuthorizedLocation:
				CheckJE_SubLocationOfGoodsWhenQualifierIsFC();
				break;
			case ImportGoodsLocationQualifierWithAuthorisationList.Codes.AuthorizedPlace:
			case ImportGoodsLocationQualifierWithAuthorisationList.Codes.ApprovedPlace:
				CheckJE_SubLocationOfGoodsWhenQualifierIsLBOrLC();
				break;
		}
	}

	void CheckJE_SubLocationOfGoodsWhenQualifierIsFC()
	{
		if (IsAuthorisationNumberEmpty)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_SubLocationOfGoodsInfo, Parent.Lookups.SubLocationOfGoodsList);
		}
	}

	void CheckJE_SubLocationOfGoodsWhenQualifierIsLBOrLC()
	{
		if (!IsAuthorisationNumberEmpty)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ImportJE_SubLocationOfGoodsInfo, Parent.Lookups.SubLocationOfGoodsList);
		}
	}

	protected override void CheckJE_RL_NKOrigin()
	{
		base.CheckJE_RL_NKOrigin();
		MandatoryValidation.WarnIfNotEntered(Parent.JE_RL_NKOriginInfo);
	}

	protected override void CheckJE_VoyageFlightNo()
	{
	}

	protected override void CheckJE_VesselName()
	{
	}

	protected override void CheckJE_RN_NKTransportNationality()
	{
		base.CheckJE_RN_NKTransportNationality();
		if (TransportNationalityMandatoryBasedOnTransportMode(Parent.JE_TransportMode) && AnyEntryInstructionIsNonWarehouseProcedure)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_RN_NKTransportNationalityInfo);
		}
	}

	protected override void CheckJE_TransportMeans()
	{
		base.CheckJE_TransportMeans();

		CheckJE_TransportMeansMandatoryForImport();
	}

	void CheckJE_TransportMeansMandatoryForImport()
	{
		var declaration = Parent;

		if (declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().Any(cei => validateInlandTransportCodeForStyles.Contains(cei.CEI_Style))
			&& !declaration.ZG_Box18TransportID.IsEmpty)
		{
			MandatoryValidation.MessageErrorIfNotEntered(declaration.JE_TransportMeansInfo);
		}
	}

	ZBool TransportNationalityMandatoryBasedOnTransportMode(ZString transportMode)
	{
		const string empty = "";
		switch (transportMode)
		{
			case TransportTypeList.Codes.Rail:
			case TransportTypeList.Codes.Mail:
			case TransportTypeList.Codes.FixedTransportInstallations:
			case empty:
				return ZBool.False;
			default:
				return ZBool.True;
		}
	}

	protected override void CheckJE_RL_NKFinalDestination()
	{
		base.CheckJE_RL_NKFinalDestination();
		if (AnyEntryInstructionIsNonWarehouseProcedure)
		{
			var finalDestinationInfo = Parent.JE_RL_NKFinalDestinationInfo;
			if (Parent.JE_RL_NKFinalDestination.IsEmpty)
			{
				finalDestinationInfo.AddMessageError(ValidationCaptions.JobDeclaration.YouHaveNoEnteredDestination);
			}
			else if (Parent.FinalDestination?.RL_RW.IsEmpty ?? false)
			{
				finalDestinationInfo.AddMessageError(ValidationCaptions.JobDeclaration.DestinationDoesNotHaveProvince);
			}
		}
	}

	protected override void CheckJE_DefermentAccountNumber()
	{
		base.CheckJE_DefermentAccountNumber();
		ValidateDefermentAccountNumberForDPOAuthorizationIfRequired();
		new JobDeclarationUcc6DefermentAccountNumberValidator(Parent).Validate();
	}

	protected override void CheckJE_RL_NKPortOfArrival()
	{
		base.CheckJE_RL_NKPortOfArrival();
		CheckBarrierPort(Parent.JE_RL_NKPortOfArrivalInfo);
	}

	protected override void CheckJE_GoodsDestination()
	{
		base.CheckJE_GoodsDestination();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_GoodsDestinationInfo);
	}

	protected override void CheckJE_GoodsOrigin()
	{
		base.CheckJE_GoodsOrigin();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_GoodsOriginInfo);
	}

	protected override void CheckJE_LocationQualifier()
	{
		base.CheckJE_LocationQualifier();
		if (Parent.JE_LocationQualifier.IsEmpty && (IsAuthorisationNumberEmpty || Parent.Authorization == null))
		{
			Parent.JE_LocationQualifierInfo.AddMessageError(ValidationCaptions.JobDeclaration.LocationQualifierRequired);
		}

		CheckLocationQualifierWithAuthorizationType();
	}

	void CheckLocationQualifierWithAuthorizationType()
	{
		if (!IsAuthorisationNumberEmpty)
		{
			var authorisation = Parent.Authorization;
			var locationQualifierInfo = Parent.JE_LocationQualifierInfo;
			var locationQualifier = Parent.JE_LocationQualifier;
			if (authorisation.SupportImportLocationQualifierLB() && locationQualifier != ImportGoodsLocationQualifierWithAuthorisationList.Codes.AuthorizedPlace)
			{
				locationQualifierInfo.AddMessageError(ValidationCaptions.JobDeclaration.GetSelectedAuthorizationRequiresLocationQualifierOfTypeCaption(ImportGoodsLocationQualifierWithAuthorisationList.Codes.AuthorizedPlace));
			}
			else if (authorisation.SupportImportLocationQualifierLC() && locationQualifier != ImportGoodsLocationQualifierWithAuthorisationList.Codes.ApprovedPlace)
			{
				locationQualifierInfo.AddMessageError(ValidationCaptions.JobDeclaration.GetSelectedAuthorizationRequiresLocationQualifierOfTypeCaption(ImportGoodsLocationQualifierWithAuthorisationList.Codes.ApprovedPlace));
			}
		}
	}

	protected override void CheckJE_OA_DeclarantAddress()
	{
		base.CheckJE_OA_DeclarantAddress();
		AddMessageErrorIfEORICodeMissing(DeclarantOrgHeader, Parent.JE_OA_DeclarantAddressInfo, ValidationCaptions.ImportJobDeclarationValidation.EoriCodeForDeclarantIsRequired);
	}

	protected override void CheckJE_OA_Representative()
	{
		base.CheckJE_OA_Representative();
		CheckJE_OA_RepresentativeIsNotEmpty();

		var declaration = Parent;
		var propertyInfo = declaration.JE_OA_RepresentativeInfo;

		if (IsRepresentativeMandatory)
		{
			AddMessageErrorIfEORICodeMissing(RepresentativeOrgHeader, propertyInfo, ValidationCaptions.ImportJobDeclarationValidation.EoriCodeForRepresentativeIsRequired);
		}

		if (declaration.RepresentativeOrgAddress != null)
		{
			new CustomsAddressValidator(declaration.RepresentativeOrgAddress, propertyInfo.HumanReadableName, declaration)
					.ValidateMaximumLengthCustomsFields(propertyInfo);
		}
	}

	protected override void CheckJE_OA_SellerAddress()
	{
		base.CheckJE_OA_SellerAddress();

		var declaration = Parent;
		var propertyInfo = declaration.JE_OA_SellerAddressInfo;

		if (declaration.SellerAddress != null)
		{
			new CustomsAddressValidator(declaration.SellerAddress, propertyInfo.HumanReadableName, declaration)
					.ValidateMaximumLengthCustomsFields(propertyInfo);
		}
	}

	protected override void CheckJE_OH_Buyer()
	{
		base.CheckJE_OH_Buyer();

		var declaration = Parent;
		var propertyInfo = declaration.JE_OH_BuyerInfo;
		var buyer = declaration.Buyer;

		if (buyer != null)
		{
			new CustomsAddressValidator(buyer.MainAddress, propertyInfo.HumanReadableName, declaration)
					.ValidateMaximumLengthCustomsFields(propertyInfo);
		}
	}

	#region Implementation

	ZBool IsAuthorisationNumberEmpty => Parent.ZG_AuthorisationNumber.IsEmpty;

	ZBool AnyEntryInstructionIsNonWarehouseProcedure => Parent.CustomsEntryInstructions.AnyEntryInstructionIsNonWarehouseProcedure();

	OrgHeader DeclarantOrgHeader => Parent.Factory.Load<OrgHeader>(Parent.DeclarantAddress?.OA_OH ?? ZGuid.Empty);

	OrgHeader RepresentativeOrgHeader => Parent.Factory.Load<OrgHeader>(Parent.Representative?.OA_OH ?? ZGuid.Empty);

	ZBool IsDeclarationUCC6 => Parent.IsUCC6;

	ZBool IsRepresentativeMandatory => Parent.JE_DeclarantType == EU.Business.RepresentationTypeList.Codes._2Direct || Parent.JE_DeclarantType == EU.Business.RepresentationTypeList.Codes._3Indirect;

	void AddMessageErrorIfEORICodeMissing(OrgHeader orgHeader, ZPropertyInfo propertyInfoToAddError, string errorMessage)
	{
		if (IsDeclarationUCC6 && orgHeader != null && string.IsNullOrEmpty(orgHeader.GetEoriCode()))
		{
			propertyInfoToAddError.AddMessageError(errorMessage);
		}
	}

	void CheckJE_OA_RepresentativeIsNotEmpty()
	{
		if (IsDeclarationUCC6 && IsRepresentativeMandatory)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OA_RepresentativeInfo);
		}
	}

	void ValidateDefermentAccountNumberForDPOAuthorizationIfRequired()
	{
		var parent = Parent;

		if (!parent.JE_DefermentAccountNumber.IsEmpty)
		{
			ZGuid ownerFromDeclaration;
			ZString messageError;
			switch (parent.JE_PaymentMethod)
			{
				case ImportDefermentMethodList.Codes.DeclarantsAccountFromCustomsDecisions:
					ownerFromDeclaration = parent.Declarant?.OA_OH ?? ZGuid.Empty;
					messageError = ValidationCaptions.JobDeclaration.DeclarantAuthorizationMissingForParty1;
					break;

				case ImportDefermentMethodList.Codes.ConsigneesAccountFromCustomsDecisions:
					ownerFromDeclaration = parent.Importer?.PK ?? ZGuid.Empty;
					messageError = ValidationCaptions.JobDeclaration.ImporterAuthorizationMissingForParty2;
					break;

				case ImportDefermentMethodList.Codes.ForwardersAccountFromCustomsDecisions:
					ownerFromDeclaration = parent.Forwarder?.PK ?? ZGuid.Empty;
					messageError = ValidationCaptions.JobDeclaration.ForwarderAuthorizationMissingForParty3;
					break;

				case ImportDefermentMethodList.Codes.RepresentativesAccountFromCustomsDecisions:
					ownerFromDeclaration = parent.Representative?.OA_OH ?? ZGuid.Empty;
					messageError = ValidationCaptions.JobDeclaration.RepresentativeAuthorizationMissingForParty4;
					break;

				default:
					return;
			}

			if (parent.CustomsEntryInstructions.AnyEntryInstructionIsMissingDPOAuthorizationOrEmptyOrDifferentOwner(ownerFromDeclaration))
			{
				parent.JE_DefermentAccountNumberInfo.AddMessageError(messageError);
			}
		}
	}

	readonly ImmutableArray<string> validateInlandTransportCodeForStyles = new string[]
	{
		ImportUCC6DeclarationTypeList.Codes.ImmissioneLiberaPraticaH1,
		ImportUCC6DeclarationTypeList.Codes.RegimeSpecialeAmmissioneTemporaneaH3,
		ImportUCC6DeclarationTypeList.Codes.RegimeSpecialePerfezionamentoAttivoH4,
		ImportUCC6DeclarationTypeList.Codes.DichiarazioneScambiTerritoriFiscaliSpecialiH5
	}.ToImmutableArray();

	#endregion
}
