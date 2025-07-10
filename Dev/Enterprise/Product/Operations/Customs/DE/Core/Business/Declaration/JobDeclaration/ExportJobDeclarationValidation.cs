using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class ExportJobDeclarationValidation : JobDeclarationValidation
	{
		public ExportJobDeclarationValidation(JobDeclaration parent) : base(parent)
		{
		}

		protected override void CheckJE_TransportModeMandatory()
		{
			if (Parent.CustomsEntryInstructions.Any(e =>
					e.Style4thDigitIs0()
					|| e.Style3rdDigitIs1And5thIs0()
				))
			{
				base.CheckJE_TransportModeMandatory();
			}
			else
			{
				MandatoryValidation.WarnIfNotEntered(Parent.JE_TransportModeInfo);
			}
		}

		protected override void CheckJE_OA_Representative()
		{
			base.CheckJE_OA_Representative();
			var parent = Parent;
			var representative = parent.Representative;
			if (representative != null)
			{
				var targetInfo = parent.JE_OA_RepresentativeInfo;
				var orgType = Res.GetString("d60b7fa6-9cb9-48c3-9823-445384dbfd98", "Representative");

				CheckOrgCountryCode(representative, targetInfo, orgType);
				CheckDeclarantAndRepresentativeIdentical(targetInfo);
			}
		}

		protected override void CheckJE_OA_RepresentativeIsNotEmpty()
		{
			if (HasConstellationWithSpecificDigit(2, '1'))
			{
				Parent.JE_OA_RepresentativeInfo.AddMessageError(Res.GetString("5EECB31C-0462-47A0-A2E3-2CC91F7DBF19", "The chosen Party Constellation requires a [14] Representative to be entered."));
			}
		}

		protected override void CheckJE_OA_SellerAddress()
		{
			base.CheckJE_OA_SellerAddress();

			var parent = Parent;
			if (parent.JE_OA_SellerAddress.IsEmpty && HasConstellationWithSpecificDigit(3, '1'))
			{
				parent.JE_OA_SellerAddressInfo.AddMessageError(Res.GetString("F1AD85EB-B46F-433F-830E-E729CF03056B", "The chosen Party Constellation requires a [2] Subcontractor to be entered."));
			}
			else if (!parent.JE_OA_SellerAddress.IsEmpty && parent.CustomsEntryInstructions.Any(i => i.Constellation2ndDigitIs0And4thDigitIs1()))
			{
				var declarant = (OrgHeader)parent.JE_OA_DeclarantAddress_ZAddress.OrgHeader;
				if (declarant.HasSameEori(parent.Seller))
				{
					parent.JE_OA_SellerAddressInfo.AddMessageError(Res.GetString("47131217-49dd-474a-b816-1797c6d12263", "[2] Subcontractor and [14] Declarant must not be equal."));
				}
			}
		}

		protected override void CheckJE_GoodsOrigin()
		{
			base.CheckJE_GoodsOrigin();

			var parent = Parent;
			var customsEntryInstructions = parent.CustomsEntryInstructions;
			var targetInfo = parent.JE_GoodsOriginInfo;
			if (customsEntryInstructions.Any(x => !x.Style4thDigitIs9()))
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}
			if (parent.JE_GoodsOrigin != Core.Constants.CountryCodes.Germany)
			{
				if (customsEntryInstructions.Any(x => !x.Style4thDigitIs9() && !x.Style4thDigitIs4()))
				{
					targetInfo.AddMessageError(Res.GetString("A9E8A5EF-1E00-4BF4-8E3E-67DCC84334A9", "For the selected Type(Procedure) Country/Region of Origin must be 'DE'."));
				}
			}
		}

		protected override void CheckJE_GoodsDestination()
		{
			base.CheckJE_GoodsDestination();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_GoodsDestinationInfo);
		}

		protected override void CheckJE_TransportModeInland()
		{
			base.CheckJE_TransportModeInland();
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_TransportModeInlandInfo);
		}

		protected override void CheckJE_OA_DeclarantAddress()
		{
			base.CheckJE_OA_DeclarantAddress();
			var targetInfo = Parent.JE_OA_DeclarantAddressInfo;
			if (Parent.JE_OA_DeclarantAddress.IsEmpty)
			{
				if (Parent.CustomsEntryInstructions.Any())
				{
					targetInfo.AddMessageError(Res.GetString("826D5497-38E7-4732-9F6F-C21817D521A2", "Declarant Address is required"));
				}
			}
			else
			{
				var declarant = Res.GetString("5c2dac7a-356f-47da-9f92-2d99317da1d4", "Declarant");
				CheckOrgCountryCode(Parent.Declarant, targetInfo, declarant);
				CheckDeclarantAndRepresentativeIdentical(targetInfo);
			}
		}

		protected override void CheckJE_TransportIDInland()
		{
			base.CheckJE_TransportIDInland();
			if (!Parent.IsAirInland && !Parent.IsRailInland)
			{
				EU.Business.Declaration.JobDeclarationUCC6ExportValidation.CheckJE_TransportIDInland(Parent);
			}

			var vesselTransportMeansCode = new HashSet<string> { TransportMeansList.Codes.NameOfTheSeaGoingVessel, TransportMeansList.Codes.NameOfTheInlandWaterwaysVessel };
			if (!vesselTransportMeansCode.Contains(Parent.JE_TransportMeans) && ((string)Parent.JE_TransportIDInland).Any(char.IsLower))
			{
				Parent.JE_TransportIDInlandInfo.AddMessageError(Res.GetString("a58b325b-d0dc-42b7-84cc-831723eae26e", "No lower case letters may be specified."));
			}

			if (Parent.IsSeaInland && Parent.JE_TransportMeans == TransportMeansList.Codes.NameOfTheSeaGoingVessel)
			{
				ListValidation.WarnIfInvalidCode(Parent.JE_TransportIDInlandInfo, Parent.Lookups.Vessels, ResString.GetMultilingualString("96eaee85-f9c4-405d-b828-bbacd36e091f", "Warning: No reference file for this Vessel was found."));
			}

			var isTransportIdInlandMandatory = false;
			switch (Parent.JE_TransportModeInland)
			{
				case TransportTypeList.Codes.InlandWaterwayTransport:
				case TransportTypeList.Codes.Sea:
				case TransportTypeList.Codes.OwnPropulsion:
				case TransportTypeList.Codes.Road:
				case TransportTypeList.Codes.Air:
					isTransportIdInlandMandatory = true;
					break;
				case TransportTypeList.Codes.Rail:
					if (Parent.IsTransitionPeriodAES30)
					{
						isTransportIdInlandMandatory = true;
					}
					break;
			}
			if (isTransportIdInlandMandatory)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_TransportIDInlandInfo, Parent.JE_TransportIDInlandInfo.HumanReadableName.TrimEnd('.'));
			}
		}

		protected override void CheckJE_RN_NKTransportNationalityInland()
		{
			base.CheckJE_RN_NKTransportNationalityInland();
			var parent = Parent;
			EU.Business.Declaration.JobDeclarationUCC6ExportValidation.CheckJE_RN_NKTransportNationalityInland(parent);
			ListValidation.MessageErrorIfInvalidCode(parent.JE_RN_NKTransportNationalityInlandInfo);
			var transportModesWhereTransportIdRequiredForNationality = new HashSet<string> { TransportTypeList.Codes.InlandWaterwayTransport, TransportTypeList.Codes.Sea, TransportTypeList.Codes.Air, TransportTypeList.Codes.OwnPropulsion, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Road };
			if (!parent.JE_TransportIDInland.IsEmpty && transportModesWhereTransportIdRequiredForNationality.Contains(parent.JE_TransportModeInland)
				|| (parent.IsAirInland && !parent.JE_AircraftRegistrationInland.IsEmpty))
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.JE_RN_NKTransportNationalityInlandInfo);
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

			var inlandTransportMeans = parent.JE_TransportModeInland;

			if (inlandTransportMeans != TransportTypeList.Codes.FixedTransportInstallations &&
				inlandTransportMeans != TransportTypeList.Codes.Mail)
			{
				MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(parent.JE_TransportMeansInfo, parent.JE_TransportModeInlandInfo);
			}
		}

		protected override void CheckJE_VesselName()
		{
			base.CheckJE_VesselName();
			EU.Business.Declaration.JobDeclarationUCC6ExportValidation.CheckJE_VesselName(Parent);
		}

		protected override void CheckJE_VoyageFlightNo()
		{
			base.CheckJE_VoyageFlightNo();

			var parent = Parent;
			var targetInfo = parent.JE_VoyageFlightNoInfo;
			var borderTransportMeans = parent.ZG_BorderTransportMeans;
			if (parent.IsAir)
			{
				if (borderTransportMeans == ExportBorderTransportMeansList.Codes._40)
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo, Res.GetString("9D9EEB6B-256D-48B9-A8B8-910624ABAE96", "Flight Number"));
				}
				else if (borderTransportMeans == ExportBorderTransportMeansList.Codes._41)
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo, Res.GetString("D294E5BB-0010-434D-B7F5-C8FBACE3B9E8", "Registration Number of the Aircraft"));
				}
			}
		}

		protected override void CheckJE_RN_NKTransportNationality()
		{
			base.CheckJE_RN_NKTransportNationality();

			var parent = Parent;
			if (!parent.ZG_BorderTransportMeans.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.JE_RN_NKTransportNationalityInfo, Res.GetString("3B36C385-92E8-4F74-94BB-929A8D3811E7", "[21] Nationality of Means of Transport at the Border"));
			}
		}

		protected override void CheckJE_OwnerRef()
		{
			var parent = Parent;

			if (parent.JE_OwnerRef.IsEmpty)
			{
				parent.JE_OwnerRefInfo.AddWarning(Res.GetString("E3E7E25B-3101-42E9-8379-23E4EC58E54C", "If Declarant's Reference is empty, Declaration Reference ({0}) will be determined as Local Reference Number and sent to Customs.", parent.JE_DeclarationReference));
			}
			else if (parent.JE_OwnerRef.Length > 22)
			{
				parent.JE_OwnerRefInfo.AddWarning(Res.GetString("65FAC0D4-101E-4358-9F28-0E2B275042C5", "The maximum length for [7] Declarant's Ref is 22 characters."));
			}
		}

		protected override void CheckJE_CustomsOffice()
		{
			base.CheckJE_CustomsOffice();

			var parent = Parent;
			var fourthNumInStyleIs9 = parent.CustomsEntryInstructions.Any(x => x.Style4thDigitIs9());
			var customsOffice = ZZRefCusCodeListCombined.Loader.LoadByCode(parent.Factory, Core.Constants.CountryCodes.Germany, new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice }, parent.JE_CustomsOffice, ZDate.Today).FirstOrDefault();
			var officeRoleEXP = customsOffice != null && customsOffice.Attributes.Cast<ZZRefCusCodeListAttributeCombined>().Any(x => x.ZZE_Value.EqualsIgnoringCase(EuOfficeCodesTypes.Codes.OfficeOfExport));
			var officeRoleEXT = customsOffice != null && customsOffice.Attributes.Cast<ZZRefCusCodeListAttributeCombined>().Any(x => x.ZZE_Value.EqualsIgnoringCase(EuOfficeCodesTypes.Codes.OfficeOfExit));

			if (!fourthNumInStyleIs9 && !officeRoleEXP)
			{
				parent.JE_CustomsOfficeInfo.AddMessageError(Res.GetString("D4C8598B-7328-4C6E-B5BC-5FAA4B71B597", $"Please enter an Office of Export with role '{EuOfficeCodesTypes.Codes.OfficeOfExport}'"));
			}
			else if (fourthNumInStyleIs9 && !officeRoleEXT)
			{
				parent.JE_CustomsOfficeInfo.AddMessageError(Res.GetString("E65B1BAF-9FA0-4A48-92BC-D2A2CE23C9CC", $"Please enter an Office of Export with role '{EuOfficeCodesTypes.Codes.OfficeOfExit}'"));
			}
		}

		protected override void CheckJE_ContainerMode_Mandatory()
		{
			var declaration = Parent;
			if (declaration.IsTransitionPeriodAES30)
			{
				MandatoryValidation.MessageErrorIfNotEntered(declaration.JE_ContainerModeInfo);
			}

			base.CheckJE_ContainerMode_Mandatory();
		}

		protected override void CheckJE_GS_NKCusAgent()
		{
			base.CheckJE_GS_NKCusAgent();
			CheckBrokerHasWorkPhone();
		}

		void CheckOrgCountryCode(OrgAddress org, ZPropertyInfo targetInfo, string orgType)
		{
			var countryCode = org?.Country;
			if (countryCode != null && countryCode.RN_EconomicGrouping != EconomicGroupList.Codes.EuropeanUnion)
			{
				var isDeclarant = targetInfo.Name.Equals(JobDeclaration.Schema.JE_OA_DeclarantAddress);
				if (isDeclarant && HasConstellationWithSpecificDigit(3, '1'))
				{
					targetInfo.AddMessageError(Res.GetString("F64D7BCD-384D-414E-89DB-6596EFACE9EB", "The {0} must be resident in the EU", orgType));
				}
				else if (countryCode.RN_Code != Core.Constants.CountryCodes.Switzerland && countryCode.RN_Code != Core.Constants.CountryCodes.Liechtenstein)
				{
					targetInfo.AddMessageError(Res.GetString("4624AD21-EE29-47D2-90AB-77BC6F6B1CD0", "The {0} must be resident in the EU, Switzerland or Liechtenstein", orgType));
				}
			}
		}

		void CheckDeclarantAndRepresentativeIdentical(ZPropertyInfo targetInfo)
		{
			if (Parent.JE_OA_DeclarantAddress == Parent.JE_OA_Representative)
			{
				targetInfo.AddMessageError(Res.GetString("EFFA4583-B783-43BC-BA64-EA01869FD554", "[14] Representative and [14] Declarant must not be equal"));
			}
		}

		bool HasConstellationWithSpecificDigit(ZInt index, char expectedCharacter) => Parent.CustomsEntryInstructions.Any(x => x.ZG_PartyConstellation.IsDigitCheckSatisfied(index, expectedCharacter));

		protected override void CheckJE_PresentationStartDate()
		{
			base.CheckJE_PresentationStartDate();

			var declaration = Parent;
			var presentationStartDate = declaration.JE_PresentationStartDate;
			var targetInfo = Parent.JE_PresentationStartDateInfo;
			if (presentationStartDate.IsEmpty)
			{
				if (HasInstructionsWithTheFourthNumberOfCEI_StyleIs2(declaration))
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				}
			}
			else if (presentationStartDate.IsValid)
			{
				var presentationEndDate = declaration.JE_PresentationEndDate;
				if (!declaration.EntryHasBeenSubmitted && presentationStartDate < ZDateTime.Today)
				{
					targetInfo.AddMessageError(Res.GetString("14ABC561-2AAE-46FB-86CB-0AC9567EC57A", "The Start Date of the Presentation must not be earlier than the current date."));
				}
				if (presentationEndDate.IsValid && presentationStartDate >= presentationEndDate)
				{
					targetInfo.AddMessageError(Res.GetString("9157C769-E78E-420A-A14B-64485B3150B0", "The Start Date of the Presentation must be earlier than the End date."));
				}
			}
		}

		protected override void CheckJE_PresentationEndDate()
		{
			base.CheckJE_PresentationEndDate();

			var declaration = Parent;
			var presentationEndDate = declaration.JE_PresentationEndDate;
			var targetInfo = Parent.JE_PresentationEndDateInfo;
			if (presentationEndDate.IsEmpty)
			{
				if (HasInstructionsWithTheFourthNumberOfCEI_StyleIs2(declaration))
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				}
			}
			else if (presentationEndDate.IsValid)
			{
				var presentationStartDate = declaration.JE_PresentationStartDate;
				if (!declaration.EntryHasBeenSubmitted && presentationEndDate > ZDateTime.Today.AddDays(7))
				{
					targetInfo.AddMessageError(Res.GetString("93E5CC7B-64FD-407A-BA7C-6EFD0EC8C37F", "The End Date of the Presentation must not be more than 7 days in the future."));
				}
				if (presentationStartDate.IsValid && presentationEndDate <= presentationStartDate)
				{
					targetInfo.AddMessageError(Res.GetString("D6F6391C-D377-43A5-881D-0AB3420AD13F", "The End Date of the Presentation must be after the Start date."));
				}
			}
		}

		ZBool HasInstructionsWithTheFourthNumberOfCEI_StyleIs2(JobDeclaration declaration) => declaration.CustomsEntryInstructions.Any(x => x.Style4thDigitIs2());
	}
}
