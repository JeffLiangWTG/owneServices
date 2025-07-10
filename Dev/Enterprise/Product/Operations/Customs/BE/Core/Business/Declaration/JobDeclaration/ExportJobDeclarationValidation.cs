using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.BE.Business.Declaration;

public class ExportJobDeclarationValidation : JobDeclarationValidation
{
	public ExportJobDeclarationValidation(JobDeclaration parent) : base(parent)
	{
	}

	protected override void CheckJE_CustomsOffice()
	{
		base.CheckJE_CustomsOffice();

		if (Parent.JE_ApplicationCode == DeclarationApplicationCodeList.Codes.Builtin)
		{
			ListValidation.ErrorIfInvalidCode(Parent.JE_CustomsOfficeInfo);
		}
		else
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_CustomsOfficeInfo);
		}
	}

	protected override void CheckJE_TransportIDInland()
	{
		base.CheckJE_TransportIDInland();
		var parent = Parent;
		ZString propertyDescription = ZString.Empty;
		var isTransportIdInlandMandatory = true;
		switch (parent.JE_TransportModeInland)
		{
			case TransportTypeList.Codes.InlandWaterwayTransport:
			case TransportTypeList.Codes.Sea:
			case TransportTypeList.Codes.OwnPropulsion:
				propertyDescription = Res.GetString("67441308-4977-428A-A48B-39C0934ADFA7", "Vessel ID");
				break;

			case TransportTypeList.Codes.Air:
				propertyDescription = Res.GetString("3A5401B2-65FF-45AB-A9CA-3DF7289E920B", "Flight Number");
				break;

			case TransportTypeList.Codes.Rail when parent.JE_TransportIDInland.IsEmpty && parent.JE_Trailer1RegNo.IsEmpty:
				propertyDescription = Res.GetString("51D50779-E625-47A3-AFF1-38ACB226195E", "Train or Wagon Number");
				break;

			case TransportTypeList.Codes.Road:
				propertyDescription = Res.GetString("21D5A286-01BC-463E-A597-4D7E3CDFFDC2", "Transport ID");
				break;

			default:
				isTransportIdInlandMandatory = false;
				break;
		}

		var vesselTransportMeansCodes = new HashSet<string> { ExportInlandTransportTypeList.Codes._11, ExportInlandTransportTypeList.Codes._81 };
		if (!(parent.IsWaterwayTransportsInland || parent.IsSeaInland || parent.IsOwnPropulsionInland && vesselTransportMeansCodes.Contains(parent.JE_TransportMeans))
			&& ((string)parent.JE_TransportIDInland).Any(char.IsLower))
		{
			parent.JE_TransportIDInlandInfo.AddMessageError(Res.GetString("1949268C-8B2D-49F7-8B68-9A2B6757A022", "No lower case letters may be specified."));
		}

		if (parent.IsSeaInland)
		{
			ListValidation.WarnIfInvalidCode(parent.JE_TransportIDInlandInfo, parent.Lookups.Vessels, ResString.GetMultilingualString("7C40E844-DD09-41EB-9FB3-31072E205D21", "Warning: No reference file for this Vessel was found."));
		}

		if (isTransportIdInlandMandatory)
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.JE_TransportIDInlandInfo, propertyDescription);
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
				MandatoryValidation.MessageErrorIfNotEntered(parent.JE_Trailer1RegNoInfo, Res.GetString("CF0FA923-3A4C-4164-875A-B4294C649AF4", "Train or Wagon Number"));
			}

			if (!parent.JE_TransportIDInland.IsEmpty && !parent.JE_Trailer1RegNo.IsEmpty)
			{
				parent.JE_Trailer1RegNoInfo.AddMessageError(Res.GetString("3B4865E9-0E55-476E-9CAD-9FEB38D1CF7D", "You may only enter either a Train or a Wagon Number."));
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
		if (!parent.JE_Trailer1RegNo.IsEmpty)
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.JE_RN_NKTrailer1NationalityInfo);
		}
	}

	protected override void CheckJE_RN_NKTrailer2Nationality()
	{
		base.CheckJE_RN_NKTrailer2Nationality();
		var parent = Parent;
		if (!parent.JE_Trailer2RegNo.IsEmpty)
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.JE_RN_NKTrailer2NationalityInfo);
		}
	}

	protected override void CheckJE_TransportMeans()
	{
		base.CheckJE_TransportMeans();
		var parent = Parent;
		ListValidation.MessageErrorIfInvalidCode(parent.JE_TransportMeansInfo);
		if (parent.IsOwnPropulsionInland)
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.JE_TransportMeansInfo);
		}
	}
}
