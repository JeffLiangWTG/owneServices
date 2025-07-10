using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Ucc6ExportDefermentMethodCodes = Enterprise.Customs.IT.Business.Ucc6ExportDefermentMethodList.Codes;

namespace Enterprise.Customs.IT.Business.Declaration;

class Ucc6ExportJobDeclarationValidation : ExportJobDeclarationValidation
{
	public Ucc6ExportJobDeclarationValidation(JobDeclaration parent) : base(parent)
	{
	}

	protected override void CheckJE_CustomsOffice()
	{
		base.CheckJE_CustomsOffice();
		ValidateCustomsOfficePurposePREIsNeededR0675(Parent);
	}

	void ValidateCustomsOfficePurposePREIsNeededR0675(JobDeclaration parent)
	{
		var hasAnyEntryInstructionWithAuthorizationOfTypeCCL = parent.CustomsEntryInstructions
			.Cast<CusEntryInstruction>()
			.Any(x => x.CusAuthorizationUsages
			.HasAuthorizationOfType(CusAuthorizationHeaderTypeList.Codes.CentralizedClearance));

		if (hasAnyEntryInstructionWithAuthorizationOfTypeCCL && !HasOfficesOfType(EuOfficeCodesTypes.Codes.OfficeOfPresentation))
		{
			parent.JE_CustomsOfficeInfo.AddMessageError(ValidationCaptions.JobDeclaration.DeclarationRequiresPresentationOfficeForCentralizedClearanceWithPurposePRE_R0675);
		}
	}

	protected override void CheckJE_GoodsOrigin()
	{
		base.CheckJE_GoodsOrigin();

		var parent = Parent;
		new HeaderOrLineValueValidator<ZString>(
			headerValueProvider: () => parent.JE_GoodsOrigin,
			lineValuesProvider: () => parent
										.Invoices
										.SelectMany(x => x.InvoiceLines)
										.Cast<JobComInvoiceLine>()
										.Select(x => x.JI_RN_NKCountryOfExport))
		{
			IsEmptyFunc = x => x.IsEmpty
		}
		.ValidateHeader(parent.JE_GoodsOriginInfo);
	}

	protected override void CheckJE_GoodsDestination()
	{
		base.CheckJE_GoodsDestination();

		var parent = Parent;
		new HeaderOrLineValueValidator<ZString>(
			headerValueProvider: () => parent.JE_GoodsDestination,
			lineValuesProvider: () => parent
										.Invoices
										.SelectMany(x => x.InvoiceLines)
										.Cast<JobComInvoiceLine>()
										.Select(x => x.ZG_CountryOfDestination))
		{
			IsEmptyFunc = x => x.IsEmpty
		}
		.ValidateHeader(parent.JE_GoodsDestinationInfo);
	}

	protected override void CheckJE_TransportModeInland()
	{
		base.CheckJE_TransportModeInland();
		CheckJE_TransportModeInlandC0843(Parent);
	}

	protected override void CheckJE_TransportIDInland()
	{
		base.CheckJE_TransportIDInland();
		new DepartureTransportMeansValidation(Parent).CheckJE_TransportIDInland();
	}

	protected override void CheckJE_Trailer1RegNo()
	{
		base.CheckJE_Trailer1RegNo();
		new DepartureTransportMeansValidation(Parent).CheckJE_Trailer1RegNo();
	}

	protected override void CheckJE_Trailer2RegNo()
	{
		base.CheckJE_Trailer2RegNo();
		new DepartureTransportMeansValidation(Parent).CheckJE_Trailer2RegNo();
	}

	protected override void CheckJE_AircraftRegistrationInland()
	{
		base.CheckJE_AircraftRegistrationInland();
		new DepartureTransportMeansValidation(Parent).CheckJE_AircraftRegistrationInland();
	}

	protected override void CheckJE_RN_NKTransportNationalityInland()
	{
		base.CheckJE_RN_NKTransportNationalityInland();
		new DepartureTransportMeansValidation(Parent).CheckJE_RN_NKTransportNationalityInland();
	}

	protected override void CheckJE_RN_NKTrailer1Nationality()
	{
		base.CheckJE_RN_NKTrailer1Nationality();
		new DepartureTransportMeansValidation(Parent).CheckJE_RN_NKTrailer1Nationality();
	}

	protected override void CheckJE_RN_NKTrailer2Nationality()
	{
		base.CheckJE_RN_NKTrailer2Nationality();
		new DepartureTransportMeansValidation(Parent).CheckJE_RN_NKTrailer2Nationality();
	}

	protected override void CheckJE_TransportMeans()
	{
		base.CheckJE_TransportMeans();
		new DepartureTransportMeansValidation(Parent).CheckJE_TransportMeans();
	}

	protected override void CheckJE_DefermentAccountNumber()
	{
		base.CheckJE_DefermentAccountNumber();
		var parent = Parent;
		if (!parent.JE_DefermentAccountNumber.IsEmpty)
		{
			ValidateDefermentAccountNumberForDPOAuthorizationIfRequired(parent);
		}
		new JobDeclarationUcc6DefermentAccountNumberValidator(Parent).Validate();
	}

	protected override ZBool RequireJE_ShipmentIncoTermPlaceMandatory
		=> Parent.IsUcc6ExportAndIsShipmentIncoTermOther
			? ZBool.False
			: base.RequireJE_ShipmentIncoTermPlaceMandatory;

	protected override void CheckJE_VesselName()
	{
		base.CheckJE_VesselName();
		var parent = Parent;
		if (parent.JE_TransportMode.IsEmpty
			|| parent.IsRoad
			|| parent.IsWaterwayTransport
			|| parent.IsOwnPropulsion
			|| parent.IsMail
			|| parent.IsSea)
		{
			new Ucc6ExportActiveBorderGroupValidation(parent).ValidateIfActiveBorderGroupFieldsAreRequired(parent.JE_VesselNameInfo);
		}
	}

	protected override void CheckJE_VoyageFlightNo()
	{
		base.CheckJE_VoyageFlightNo();
		var parent = Parent;
		if (parent.IsAir || parent.IsSea)
		{
			new Ucc6ExportActiveBorderGroupValidation(parent).ValidateIfActiveBorderGroupFieldsAreRequired(parent.JE_VoyageFlightNoInfo);
		}
	}

	protected override void ApplyMandatoryValidationToJE_RN_NKTransportNationalityIfApplicable()
	{
		var parent = Parent;
		new Ucc6ExportActiveBorderGroupValidation(parent).ValidateIfActiveBorderGroupFieldsAreRequired(parent.JE_RN_NKTransportNationalityInfo);
	}

	protected override void CheckJE_OH_ShippingLine()
	{
		base.CheckJE_OH_ShippingLine();
		CheckCarrierForCusCodes(Parent);
	}

	protected override void CheckJE_OA_DeclarantAddress()
	{
		base.CheckJE_OA_DeclarantAddress();
		CheckDeclarantAddress_EORIAndTCUNeeded(Parent);
	}

	protected override void CheckJE_OA_Representative()
	{
		base.CheckJE_OA_Representative();
		CheckRepresentativeAddress_EORINeeded(Parent);
	}

	#region Implementation

	void ValidateDefermentAccountNumberForDPOAuthorizationIfRequired(JobDeclaration parent)
	{
		ZGuid ownerFromDeclaration;
		ZString messageError;
		switch (parent.JE_PaymentMethod)
		{
			case Ucc6ExportDefermentMethodCodes.DeclarantsAccountFromCustomsDecisions:
				ownerFromDeclaration = parent.Declarant?.OA_OH ?? ZGuid.Empty;
				messageError = ValidationCaptions.JobDeclaration.DeclarantAuthorizationMissingForParty1;
				break;
			case Ucc6ExportDefermentMethodCodes.ConsigneesAccountFromCustomsDecisions:
				ownerFromDeclaration = parent.Importer?.PK ?? ZGuid.Empty;
				messageError = ValidationCaptions.JobDeclaration.ImporterAuthorizationMissingForParty2;
				break;
			case Ucc6ExportDefermentMethodCodes.ForwardersAccountFromCustomsDecisions:
				ownerFromDeclaration = parent.Forwarder?.PK ?? ZGuid.Empty;
				messageError = ValidationCaptions.JobDeclaration.ForwarderAuthorizationMissingForParty3;
				break;
			case Ucc6ExportDefermentMethodCodes.SuppliersAccountFromCustomsDecisions:
				ownerFromDeclaration = parent.Supplier?.PK ?? ZGuid.Empty;
				messageError = ValidationCaptions.JobDeclaration.SupplierAuthorizationMissingForParty4;
				break;
			case Ucc6ExportDefermentMethodCodes.ExportersAccountFromCustomsDecisions:
				ownerFromDeclaration = parent.ExporterDocAddress?.OrganisationPK ?? ZGuid.Empty;
				messageError = ValidationCaptions.JobDeclaration.ExporterAuthorizationMissingForParty5;
				break;
			default:
				return;
		}

		if (parent.CustomsEntryInstructions.AnyEntryInstructionIsMissingDPOAuthorizationOrEmptyOrDifferentOwner(ownerFromDeclaration))
		{
			parent.JE_DefermentAccountNumberInfo.AddMessageError(messageError);
		}
	}

	void CheckJE_TransportModeInlandC0843(JobDeclaration parent)
	{
		if (!parent.JE_TransportModeInland.IsEmpty)
		{
			return;
		}

		var isTransportModeInlandMandatory = parent
			.CustomsEntryInstructions
			.Cast<CusEntryInstruction>()
			.Select(x => new CusEntryInstructionInlandTransportModeAssessor(x))
			.Any(x => x.IsMandatory);

		if (isTransportModeInlandMandatory)
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.JE_TransportModeInlandInfo);
		}
	}

	void CheckCarrierForCusCodes(JobDeclaration declaration)
	{
		var carrier = declaration.ShippingLine;
		var declarantPK = declaration.Declarant?.OA_OH ?? ZGuid.Empty;
		if (carrier is null || carrier.PK == declarantPK)
		{
			return;
		}

		var requiredCodeTypeListForTrader = new ZString[] { OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU };
		var shippingLineInfo = declaration.JE_OH_ShippingLineInfo;
		if (carrier.GetFirstCusCodeMatchingTypeInOrder(requiredCodeTypeListForTrader) is null)
		{
			shippingLineInfo.AddMessageError(ValidationCaptions.UCC6TraderJobDocAddressValidation.GetEoriOrTCUCodesForTraderAreRequiredCaption(shippingLineInfo.HumanReadableName));
		}
	}

	void CheckDeclarantAddress_EORIAndTCUNeeded(JobDeclaration declaration)
	{
		if (declaration.DeclarantAddress?.Header is OrgHeader organization && organization.GetEoriCode().IsEmpty && organization.GetTcuCode().IsEmpty)
		{
			declaration.JE_OA_DeclarantAddressInfo.AddMessageError(ValidationCaptions.JobDeclaration.DeclarantHasNoEORIorTCUcode);
		}
	}

	void CheckRepresentativeAddress_EORINeeded(JobDeclaration declaration)
	{
		if (declaration.Representative?.Header is OrgHeader organization && organization.GetEoriCode().IsEmpty)
		{
			declaration.JE_OA_RepresentativeInfo.AddMessageError(ValidationCaptions.JobDeclaration.RepresentativeHasNoEORI);
		}
	}

	#endregion
}
