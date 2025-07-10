using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.Business.Declaration;

public partial class ExportJobDeclarationValidation : JobDeclarationValidation
{
	public ExportJobDeclarationValidation(JobDeclaration parent) : base(parent)
	{
	}

	protected override void CheckJE_TransportIDInland()
	{
		base.CheckJE_TransportIDInland();
		var parent = Parent;
		if (!parent.IsTransportIDInlandDisabled)
		{
			var propertyDescription = ZString.Empty;
			var isTransportIdInlandMandatory = true;
			switch (parent.JE_TransportModeInland)
			{
				case TransportTypeList.Codes.InlandWaterwayTransport:
				case TransportTypeList.Codes.Sea:
				case TransportTypeList.Codes.OwnPropulsion:
					propertyDescription = Res.GetString("F5E25F2D-E3AD-46BC-84F5-8D187661E322", "Vessel ID");
					break;

				case TransportTypeList.Codes.Air:
					propertyDescription = Res.GetString("8D428DF9-1259-49A3-880D-D0F6413E7B7D", "Flight Number");
					break;

				case TransportTypeList.Codes.Rail when parent.JE_TransportIDInland.IsEmpty && parent.JE_Trailer1RegNo.IsEmpty:
					propertyDescription = Res.GetString("BE365009-8F5C-4F6A-BA07-97D1CC22D495", "Train or Wagon Number");
					break;

				case TransportTypeList.Codes.Road:
					propertyDescription = Res.GetString("FE8D1744-753B-45DD-BFE9-1A35160DBE3D", "Transport ID");
					break;

				default:
					isTransportIdInlandMandatory = false;
					break;
			}

			var vesselTransportMeansCodes = new HashSet<string> { ExportInlandTransportTypeList.Codes._11, ExportInlandTransportTypeList.Codes._81 };
			if (!(parent.IsWaterwayTransportsInland || parent.IsSeaInland || parent.IsOwnPropulsionInland && vesselTransportMeansCodes.Contains(parent.JE_TransportMeans))
				&& ((string)parent.JE_TransportIDInland).Any(char.IsLower))
			{
				parent.JE_TransportIDInlandInfo.AddMessageError(Res.GetString("47BEA713-86EF-4C7A-AF61-CA8264EE1581", "No lower case letters may be specified."));
			}

			if (parent.IsSeaInland)
			{
				ListValidation.WarnIfInvalidCode(parent.JE_TransportIDInlandInfo, parent.Lookups.Vessels, ResString.GetMultilingualString("F40B6B15-8C0D-40CF-8250-348B5E40A8F4", "Warning: No reference file for this Vessel was found."));
			}

			if (isTransportIdInlandMandatory && IsTransportInlandFieldsMandatory)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.JE_TransportIDInlandInfo, propertyDescription);
			}

			if (parent.JE_TransportMeans.IsEmpty && (CheckCPC5DigitForUC9008() || !parent.IfDisableForUC9009) && IsTransportInlandFieldsMandatory)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.JE_TransportIDInlandInfo, Res.GetString("A5A12E25-1331-4175-ACC1-9077CF336850", "Transport ID details required"));
			}
		}
	}

	protected override void CheckJE_Trailer1RegNo()
	{
		base.CheckJE_Trailer1RegNo();
		var parent = Parent;
		if (parent.IsRailInland)
		{
			if (parent.JE_TransportIDInland.IsEmpty && parent.JE_Trailer1RegNo.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.JE_Trailer1RegNoInfo, Res.GetString("DEF33A4C-04EA-4C77-9F51-25A294EC3148", "Train or Wagon Number"));
			}

			if (!parent.JE_TransportIDInland.IsEmpty && !parent.JE_Trailer1RegNo.IsEmpty)
			{
				parent.JE_Trailer1RegNoInfo.AddMessageError(Res.GetString("5869241F-0409-4463-B3E8-819BFFF3BEEA", "You may only enter either a Train or a Wagon Number."));
			}
		}
	}

	protected override void CheckJE_RN_NKTransportNationalityInland()
	{
		base.CheckJE_RN_NKTransportNationalityInland();
		var parent = Parent;
		ListValidation.MessageErrorIfInvalidCode(parent.JE_RN_NKTransportNationalityInlandInfo);
		var transportModesWhereTransportIdRequiredForNationality = new HashSet<string> { TransportTypeList.Codes.InlandWaterwayTransport, TransportTypeList.Codes.Sea, TransportTypeList.Codes.Air, TransportTypeList.Codes.OwnPropulsion, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Road };
		if (!parent.JE_TransportIDInland.IsEmpty && transportModesWhereTransportIdRequiredForNationality.Contains(parent.JE_TransportModeInland)
			|| (parent.IsAirInland && !parent.JE_AircraftRegistrationInland.IsEmpty))
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.JE_RN_NKTransportNationalityInlandInfo);
		}
	}

	protected override void CheckJE_AircraftRegistrationInland()
	{
		base.CheckJE_AircraftRegistrationInland();
		var parent = Parent;
		if (parent.IsAirInland && !Parent.JE_TransportModeInland.IsEmpty)
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.JE_AircraftRegistrationInlandInfo);
		}
	}

	protected override void CheckJE_RN_NKTrailer1Nationality()
	{
		base.CheckJE_RN_NKTrailer1Nationality();
		var parent = Parent;
		ListValidation.MessageErrorIfInvalidCode(parent.JE_RN_NKTrailer1NationalityInfo);
		if (!parent.JE_Trailer1RegNo.IsEmpty)
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.JE_RN_NKTrailer1NationalityInfo);
		}
	}

	protected override void CheckJE_RN_NKTrailer2Nationality()
	{
		base.CheckJE_RN_NKTrailer2Nationality();
		var parent = Parent;
		ListValidation.MessageErrorIfInvalidCode(parent.JE_RN_NKTrailer2NationalityInfo);
		if (!parent.JE_Trailer2RegNo.IsEmpty)
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.JE_RN_NKTrailer2NationalityInfo);
		}
	}

	protected override void CheckJE_TransportMeans()
	{
		base.CheckJE_TransportMeans();
		var parent = Parent;
		if (!parent.IsTransportMeansDisabled)
		{
			var transportMeansInfo = parent.JE_TransportMeansInfo;
			ListValidation.MessageErrorIfInvalidCode(transportMeansInfo);
			if ((parent.IsOwnPropulsionInland || CheckCPC5DigitForUC9008() || !parent.IfDisableForUC9009) && IsTransportInlandFieldsMandatory)
			{
				MandatoryValidation.MessageErrorIfNotEntered(transportMeansInfo);
			}

			if (HasIdentificationTypeCodeOnDepartureTransportMeansInOriginalMessage && parent.JE_TransportMeans.IsEmpty)
			{
				transportMeansInfo.AddMessageError(Res.GetString("27ACE425-5E60-4E8E-80BA-BEF7B5D47151", "[UC9015] Inland MOT/Code sent in original 515 message."));
			}
		}
	}

	XmlDocument OriginalMessage => originalMessage ??= GetOriginalMessage();
	XmlDocument originalMessage;

	bool HasIdentificationTypeCodeOnDepartureTransportMeansInOriginalMessage
	{
		get
		{
			var identificationTypeCodeNode = OriginalMessage?.SelectSingleNode((NoResString)"//*[local-name()='MetaData']//*[local-name()='Declaration']//*[local-name()='GoodsShipment']//*[local-name()='Consignment']//*[local-name()='DepartureTransportMeans']//*[local-name()='IdentificationTypeCode']");
			return (identificationTypeCodeNode?.InnerText ?? ZString.Empty) != ZString.Empty;
		}
	}

	XmlDocument GetOriginalMessage()
	{
		var entryHeader = Parent.CustomsEntryHeaders.FirstOrDefault(x => !x.EntryNumber.IsEmpty);
		if (entryHeader != null)
		{
			var message = entryHeader.Messages.Cast<NLEDIMessage>().FirstOrDefault(x => x.EM_MessageSubType == ExportSendMessageTypes.Codes.DEC);
			if (message != null && !message.EM_MessageText.IsEmpty)
			{
				var emMessageXML = new XmlDocument();
				emMessageXML.LoadXml(message.EM_MessageText);
				return emMessageXML;
			}
		}
		return null;
	}

	protected override void CheckJE_VesselName()
	{
		base.CheckJE_VesselName();

		var transportModes = new IZType[] { (ZString)ModeOfTransportCodeList.Codes._SEA, (ZString)ModeOfTransportCodeList.Codes._ROA, (ZString)ModeOfTransportCodeList.Codes._IWT, (ZString)ModeOfTransportCodeList.Codes._OWN };

		if (Parent.HasInvoiceLineWithC9008Procedure)
		{
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyHasValues(Parent.JE_VesselNameInfo, Parent.JE_TransportModeInfo, transportModes, Res.GetString("4C75CD08-448A-49D7-A61A-6C1B97871708", "[C9008] {0} is required.", Parent.JE_VesselNameInfo.HumanReadableName));
		}
	}

	protected override void CheckJE_VoyageFlightNo()
	{
		base.CheckJE_VoyageFlightNo();

		if (Parent.HasInvoiceLineWithC9008Procedure)
		{
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyHasValues(Parent.JE_VoyageFlightNoInfo, Parent.JE_TransportModeInfo, new IZType[] { (ZString)ModeOfTransportCodeList.Codes._AIR }, Res.GetString("BDBE4D4F-B64D-4BDC-AE28-5238D2DA7825", "[C9008] {0} is required.", Parent.JE_VoyageFlightNoInfo.HumanReadableName));
		}
	}

	protected override void CheckJE_RN_NKTransportNationality()
	{
		base.CheckJE_RN_NKTransportNationality();

		var transportModesC9008 = new IZType[] { (ZString)ModeOfTransportCodeList.Codes._AIR, (ZString)ModeOfTransportCodeList.Codes._SEA, (ZString)ModeOfTransportCodeList.Codes._ROA, (ZString)ModeOfTransportCodeList.Codes._IWT, (ZString)ModeOfTransportCodeList.Codes._OWN };

		if (Parent.HasInvoiceLineWithC9008Procedure)
		{
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyHasValues(Parent.JE_RN_NKTransportNationalityInfo, Parent.JE_TransportModeInfo, transportModesC9008, Res.GetString("7317F015-A511-4A93-A40A-054519F82A58", "[C9008] {0} is required.", Parent.JE_RN_NKTransportNationalityInfo.HumanReadableName));
		}
	}

	public override void ValidateSupplierDocumentaryAddress(JobDocAddressValidation validation)
	{
		base.ValidateSupplierDocumentaryAddress(validation);

		if (Parent.SupplierDocumentaryAddress.Organisation == null)
		{
			Parent.SupplierDocumentaryAddress.E2_OA_AddressInfo.AddMessageError(Res.GetString("E9A31033-D164-434D-9BC4-302CDFA84569", "Supplier is required"));
		}

		if (!DoesMasterDataMatchDeclarantType())
		{
			Parent.SupplierDocumentaryAddress.E2_OA_AddressInfo.AddMessageError(MessageErrorWhenMasterDataDoesNotMatchDeclarantType);
		}
	}

	protected override void CheckJE_DeclarantType()
	{
		base.CheckJE_DeclarantType();
		if (!DoesMasterDataMatchDeclarantType())
		{
			Parent.JE_DeclarantTypeInfo.AddMessageError(MessageErrorWhenMasterDataDoesNotMatchDeclarantType);
		}

		if (Parent.SupplierDocumentaryAddress.Organisation != null && Parent.SupplierDocumentaryAddress.Organisation.PK != GlbCompany.CurrentCompany.OrgProxy?.PK && Parent.JE_DeclarantType == EU.Business.RepresentationTypeList.Codes._1Self)
		{
			Parent.JE_DeclarantTypeInfo.AddMessageError(Res.GetString("9AF202E8-62E0-4DA5-8EE4-D3BEDFF188B1", "For representation type SEL the supplier must be equal to the login company"));
		}
	}

	bool DoesMasterDataMatchDeclarantType() => Parent.JE_DeclarantType == NLUniversalHelper.GetDeclarantType(Parent.SupplierDocumentaryAddress.Organisation);

	ZString MessageErrorWhenMasterDataDoesNotMatchDeclarantType => Res.GetString("DE9D1D85-7CC6-44A4-80B2-22925A616AC0", "Representation type doesn't match the agreed Representation type stored in the Customs defaults");

	bool IsTransportInlandFieldsMandatory => !(ValidationDecider?.IsRuleC0843Active ?? false) || (Parent.CustomsOffices.Cast<EuOfficeCode>().Any(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfExit && x.CY_Data != Parent.JE_CustomsOffice) && Parent.CustomsEntryInstructions.Any(x => !x.CEI_SubStyle.In(new ZString[] { EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC })));

	bool CheckCPC5DigitForUC9008()
	{
		var parent = Parent;
		if (parent.FilteredInvoiceLines.Count > 0)
		{
			foreach (var line in parent.FilteredInvoiceLines)
			{
				if (line.JI_FormattedProcedure.Length >= 5)
				{
					return line.JI_FormattedProcedure[4].Equals('E');
				}
			}
		}
		return false;
	}
}
