using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public partial class CommonJobDeclarationValidation
{
	protected override void CheckJE_AuthorisationNumber()
	{
		base.CheckJE_AuthorisationNumber();
		var authorisationNumber = Parent.ZG_AuthorisationNumber;
		var authorisatioNumberPropertyInfo = Parent.ZG_AuthorisationNumberInfo;
		ListValidation.MessageErrorIfInvalidCode(authorisatioNumberPropertyInfo);

		var entryInstructions = Parent.CustomsEntryInstructions;
		if (authorisationNumber.IsEmpty && entryInstructions.AnyEntryInstructionIsAtPlace())
		{
			authorisatioNumberPropertyInfo.AddMessageError(ValidationCaptions.JobDeclaration.AuthorisationIsRequiredForEntryInstructionsAtPlace);
		}
		else if (!authorisationNumber.IsEmpty && entryInstructions.AnyEntryInstructionIsAtCustoms())
		{
			authorisatioNumberPropertyInfo.AddMessageError(ValidationCaptions.JobDeclaration.AuthorisationMustBeEmptyForEntryInstructionsAtCustoms);
		}
	}

	protected override void CheckJE_CTStatusID()
	{
		base.CheckJE_CTStatusID();
		var targetPropertyInfo = Parent.ZG_CTStatusIDInfo;
		ListValidation.MessageErrorIfInvalidCode(targetPropertyInfo);
	}

	protected override void CheckJE_SpecificCircumstanceIndicator()
	{
		base.CheckJE_SpecificCircumstanceIndicator();

		var declaration = Declaration;
		ListValidation.MessageErrorIfInvalidCode(declaration.ZG_SpecificCircumstanceIndicatorInfo, declaration.AddInfoLookups.SpecificCircumstanceIndicatorList);

		if (declaration.IsExport && !declaration.IsUCC6)
		{
			CheckSpecificCircumstanceIndicatorForExport();
		}
	}

	protected override void CheckJE_IsSecurityDeclaration()
	{
		var declaration = Declaration;
		var parent = Parent;

		if (declaration.IsTransitionPeriodAES30 || !declaration.IsUCC6AndIsExport)
		{
			return;
		}

		if (parent.JE_IsSecurityDeclaration && !declaration.ItineraryCountries.Any())
		{
			parent.JE_IsSecurityDeclarationInfo.AddMessageError(ValidationCaptions.JobDeclaration.ItineraryCountriesRequired);
		}
	}

	protected override void CheckJE_Box18TransportID()
	{
		base.CheckJE_Box18TransportID();

		var declaration = Declaration;
		if (declaration.IsUCC6AndIsImport)
		{
			CheckJE_Box18TransportIDMandatoryForImport(declaration);
		}
	}

	protected override void CheckJE_AgreedPlaceCode()
	{
		if (Declaration.IsUcc6ExportAndIsShipmentIncoTermOther)
		{
			return;
		}

		base.CheckJE_AgreedPlaceCode();
	}

	protected override void CheckJE_BorderTransportMeans()
	{
		base.CheckJE_BorderTransportMeans();
		var declaration = Declaration;
		if (declaration.IsUCC6AndIsExport)
		{
			new Ucc6ExportActiveBorderGroupValidation(declaration).ValidateIfActiveBorderGroupFieldsAreRequired(Parent.ZG_BorderTransportMeansInfo);
		}
	}

	protected override void CheckJE_AdditionalDeliveryTerms()
	{
		base.CheckJE_AdditionalDeliveryTerms();

		var declaration = Declaration;
		if (declaration.IsUcc6ExportAndIsShipmentIncoTermOther)
		{
			MandatoryValidation.MessageErrorIfNotEntered(declaration.ZG_AdditionalDeliveryTermsInfo);
		}
	}

	#region Implementation

	void CheckSpecificCircumstanceIndicatorForExport()
	{
		var declaration = Declaration;
		var specificCircumstanceIndicator = Parent.ZG_SpecificCircumstanceIndicator;
		var specificCircumstanceIndicatorInfo = Parent.ZG_SpecificCircumstanceIndicatorInfo;

		if (specificCircumstanceIndicator == SpecificCircumstanceIndicator.Codes.AuthorizedEconomicOperators && AnySupplierDoesNotHaveAEOFSCode())
		{
			specificCircumstanceIndicatorInfo.AddMessageError(ValidationCaptions.JobDeclaration.AllSuppliersMustHaveValidAEOofTypeAEOForAEOS);
		}
		else if (specificCircumstanceIndicatorTransportModeMapDictionary.TryGetValue(specificCircumstanceIndicator, out ZString[] transports) && !transports.Contains(declaration.JE_TransportMode))
		{
			specificCircumstanceIndicatorInfo.AddMessageError(ValidationCaptions.JobDeclaration.CircumstanceAndTransportAreInCongruent);
		}

		bool AnySupplierDoesNotHaveAEOFSCode() => declaration.Invoices
			.Select(x => x.Supplier)
			.Union(new OrgHeader[] { declaration.DeclarantOrgAddress?.Header })
			.Union(new OrgHeader[] { declaration.Supplier })
			.WhereNotNull()
			.Any(x => !x.HasAeoFSCode());
	}

	JobDeclaration Declaration => Parent;

	void CheckJE_Box18TransportIDMandatoryForImport(JobDeclaration declaration)
	{
		if (!declaration.JE_TransportMeans.IsEmpty)
		{
			MandatoryValidation.MessageErrorIfNotEntered(declaration.ZG_Box18TransportIDInfo);
		}
	}

	readonly ImmutableDictionary<ZString, ZString[]> specificCircumstanceIndicatorTransportModeMapDictionary = new Dictionary<ZString, ZString[]>
				{
					{ SpecificCircumstanceIndicator.Codes.PostalAndExpressConsignments, new ZString[] { TransportTypeList.Codes.Mail } },
					{ SpecificCircumstanceIndicator.Codes.RailModeOfTransport, new ZString[] { TransportTypeList.Codes.Rail } },
					{ SpecificCircumstanceIndicator.Codes.RoadModeOfTransport, new ZString[] { TransportTypeList.Codes.Road } },
					{ SpecificCircumstanceIndicator.Codes.ShipAndAircraftSupplies, new ZString[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.Air } }
				}.ToImmutableDictionary();

	#endregion
}
