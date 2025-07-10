using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;
using ManifestValidationRuleCodes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeList.ManifestValidationRuleCodes;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public abstract partial class BaseAsycudaManifestHeaderValidation : ManifestBase.AsycudaManifestHeaderValidation
	{
		public BaseAsycudaManifestHeaderValidation(AsycudaManifestHeader parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateAMA_MasterBill();
			ValidateMasterBOL();
			ValidateRegistrationDate();
			ValidateSpecificCircumstanceIndicator();
			ValidateMethodOfPayment();
			ValidateSpecialMentions();
			ValidateETAatFirstCustomsOffice();
			ValidateATAatFirstCustomsOffice();
			ValidateRegistrationStatus();
		}

		public void ValidateMasterBOL()
		{
			ValidateCalculatedProperty(Parent.MasterBOLInfo);
		}

		protected virtual void CheckMasterBOL()
		{
			if (Parent.MasterBOL.IsEmpty)
			{
				var (isMandatory, agentType, manifestType, country) = Parent.IsMasterBOLMandatory();
				if (isMandatory)
				{
					Parent.MasterBOLInfo.AddMessageError(Invariant($"{GetMasterBOLHumanReadable(agentType)} required when Agent Type is {agentType} and Manifest Type is {manifestType} for {country.Code} ({country.Description})."));
				}
			}
		}

		protected virtual ZString GetMasterBOLHumanReadable(ZString agentType) => "Master Bill";

		protected override void CheckAMA_RN_NKCountry()
		{
			base.CheckAMA_RN_NKCountry();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.AMA_RN_NKCountryInfo);
		}

		protected override void CheckAMA_CustomsOffice()
		{
			base.CheckAMA_CustomsOffice();
			ListValidation.MessageErrorIfInvalidCode(Parent.AMA_CustomsOfficeInfo);
		}

		protected override void CheckAMA_ManifestType()
		{
			base.CheckAMA_ManifestType();
			var info = Parent.AMA_ManifestTypeInfo;
			if (NeedsToCheckAMA_ManifestType)
			{
				MandatoryValidation.CheckEntered(info);
				if (Parent.Lookups.ManifestTypes.Count > 0)
				{
					ListValidation.ErrorIfInvalidCode(info);
				}
			}
			var bills = Parent.HasBillsAndPacks ? Parent.Bills : null;
			if (bills != null && bills.Count == 0)
			{
				info.AddMessageError(Res.GetString("{C5847463-5E5C-4128-A43B-CE07C1657D11}", "At least one bill (shipment) is required."));
			}
		}
		protected virtual ZBool NeedsToCheckAMA_ManifestType => true;

		protected override void CheckAMA_Nature()
		{
			base.CheckAMA_Nature();
			CheckAMA_NatureCore();
		}

		protected virtual void CheckAMA_NatureCore()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.AMA_NatureInfo);
		}

		protected override void CheckAMA_RL_NKPortOfFirstArrival()
		{
			base.CheckAMA_RL_NKPortOfFirstArrival();
			ListValidation.MessageErrorIfInvalidCode(Parent.AMA_RL_NKPortOfFirstArrivalInfo);
		}

		public void ValidateAMA_MasterBill()
		{
			ValidateCalculatedProperty(Parent.AMA_MasterBillInfo);
		}

		public void ValidateAMA_RL_NKPortOfLoading()
		{
			ValidateCalculatedProperty(Parent.AMA_RL_NKPortOfLoadingInfo);
		}
		public void ValidateAMA_RL_NKPortOfDischarge()
		{
			ValidateCalculatedProperty(Parent.AMA_RL_NKPortOfDischargeInfo);
		}

		protected virtual void CheckAMA_MasterBill()
		{
			var masterBill = Parent.MasterBill;
			if (masterBill != null)
			{
				masterBill.Validation.ValidateABL_BillNumber();
				Parent.AMA_MasterBillInfo.AddAllNotificationsFrom(masterBill.ABL_BillNumberInfo);
			}

			if (Parent.IsAir && !Parent.AMA_MasterBill.IsEmpty)
			{
				var manifestNumber = Parent.AMA_MasterBill.Replace("-", "").Replace(" ", "");
				CheckManifestNumberWithAirWayBillValidator(manifestNumber);
			}
		}

		protected virtual void CheckManifestNumberWithAirWayBillValidator(string manifestNumber)
		{
			var warningMessage = new AirWayBillValidator().GetWarningMessage(manifestNumber);
			if (!string.IsNullOrEmpty(warningMessage))
			{
				Parent.AMA_MasterBillInfo.AddWarning(warningMessage);
			}
		}

		protected override void CheckAMA_JobReference()
		{
			base.CheckAMA_JobReference();
			if (Parent.IsInDatabase)
			{
				MandatoryValidation.CheckEntered(Parent.AMA_JobReferenceInfo);
			}
		}

		protected override void CheckAMA_OA_Carrier()
		{
			base.CheckAMA_OA_Carrier();
			CheckAMA_OA_CarrierMandatory();
			RunCarrierValidation(Parent.AMA_OA_CarrierInfo, Parent.AMA_TransportMode);
			if (Parent.IsSea)
			{
				ValidateAMA_VesselName();
			}
		}

		protected virtual void CheckAMA_OA_CarrierMandatory()
		{
			MandatoryValidation.WarnIfNotEntered(Parent.AMA_OA_CarrierInfo);
		}

		protected override void CheckAMA_TransportMode()
		{
			base.CheckAMA_TransportMode();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.AMA_TransportModeInfo);
			var masterBillValidation = Parent.MasterBill?.Validation;
			masterBillValidation?.ValidateABL_RL_NKPortOfLoading();
			masterBillValidation?.ValidateABL_RL_NKPortOfDischarge();

			if (!Parent.Persons.Any())
			{
				Parent.ZZValidationHelper?.CheckIsMandatoryWhenTransportModeMatches(
					Parent.AMA_TransportModeInfo,
					ManifestValidationRuleCodes.Person,
					Parent.AMA_TransportMode
				);
			}
		}

		protected new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		protected override void CheckAMA_VehicleRegistration()
		{
			base.CheckAMA_VehicleRegistration();
			CheckAMA_VehicleRegistrationCore();
		}

		protected virtual void CheckAMA_VehicleRegistrationCore()
		{
			var parent = Parent;
			if (parent.AMA_VehicleRegistration.IsEmpty && parent.AMA_TransportMode == Core.Constants.TransportModes.Road)
			{
				parent.AMA_VehicleRegistrationInfo.AddMessageError(ResString.GetMultilingualString("88C78FB7-4E52-4DEA-8773-8031D64EEEE1", "Vehicle Registration Number is mandatory for Transport Mode Road"));
			}
		}

		protected override void CheckAMA_Voyage()
		{
			base.CheckAMA_Voyage();
			CheckAMA_VoyageCore();
		}

		protected virtual bool IsVoyageMandatory => !Parent.IsRoad;

		protected virtual void CheckAMA_VoyageCore()
		{
			var parent = Parent;
			if (IsVoyageMandatory)
			{
				CheckAMA_VoyageMandatory();
			}

			var voyage = parent.AMA_Voyage;
			if (!voyage.IsEmpty && parent.IsAir && !RefAirline.IsValidAirline2LetterCode(parent.Factory, voyage.Left(2)))
			{
				parent.AMA_VoyageInfo.AddMessageError(ValidationConstants.FlightNumberDoesNotStartWithAValidIATAAirCode);
			}
		}

		protected virtual void CheckAMA_VoyageMandatory()
		{
			var parent = Parent;
			MandatoryValidation.MessageErrorIfNotEntered(parent.AMA_VoyageInfo, parent.VoyageFlightNoLabel.Caption);
		}

		protected override void CheckAMA_ContainerMode()
		{
			base.CheckAMA_ContainerMode();

			if (Parent.AMA_ContainerMode.IsEmpty)
			{
				var factory = Parent.Factory;
				var countryCode = Parent.AMA_RN_NKCountry;
				var manifestType = Parent.AMA_ManifestType;
				if (ZZDatabaseValidationHelper.IsMandatoryForOneCountryWhenAttributeMatches(factory, countryCode, ManifestValidationRuleCodes.Container, ManifestValidationRuleCodes.MANDATORYFORMESSAGETYPE, manifestType))
				{
					Parent.AMA_ContainerModeInfo.AddMessageError(Invariant($"Container Mode is compulsory when Manifest Type is {manifestType} for {countryCode} ({Parent.CountryName})."));
				}
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.AMA_ContainerModeInfo);
			}

			if (Parent.Containers.Count == 0)
			{
				Parent.ZZValidationHelper?.CheckIsMandatoryWhenMatchingAttribute(
					Parent.AMA_ContainerModeInfo,
					ManifestValidationRuleCodes.Container,
					ManifestValidationRuleCodes.MANDATORYFORCONTAINERMODE,
					Parent.AMA_ContainerMode
				);
			}
		}

		protected override void CheckAMA_VesselName()
		{
			base.CheckAMA_VesselName();
			if (Parent.IsSea)
			{
				ListValidation.WarnIfInvalidCode(Parent.AMA_VesselNameInfo, ResString.GetMultilingualString("5EA5C0AD-3BB8-4143-8A1A-FD5DE44071E0", "The Vessel Name entered does not exist in the reference file."));
				RunVesselValidation(Parent.AMA_VesselNameInfo, Parent.AMA_TransportMode);
				ValidateAMA_OA_Carrier();
			}
		}

		void RunCarrierValidation(ZPropertyInfo zpi, string transportMode)
		{
			var parent = Parent;
			var rule = parent.ZZValidationHelper?.CompileRulesForCheckingZzVesselAndCarrier(transportMode);
			if (rule != null)
			{
				var carrierCccCode = parent.CarrierCCCCode;
				if (carrierCccCode.IsEmpty)
				{
					if (rule.IsCarrierCodeNeeded)
					{
						if (ShouldValidateCCCCodeOfCarrier)
						{
							var transportModeIsSea = transportMode == Core.Constants.TransportModes.Sea;
							if (transportModeIsSea && ShouldValidateZZCarrierLinkedToZZVessel)
							{
								CheckZZCarrierLinkedToZZVessel(rule, zpi, transportMode);
							}
							else
							{
								zpi.AddMessageError(ValidationConstants.CarrierRequiresCCC(rule.CountryCode));
							}
						}
					}
				}
				else if (rule.CccShouldBeInList)
				{
					var zzCarrier = parent.GetZZCarrier(CarrierType, parent.CarrierCCCCode);
					if (zzCarrier == null)
					{
						zpi.AddMessageError("Carrier's CCC is not in the list of known " + rule.CountryCode + " carrier codes");
					}
				}
			}
		}

		protected virtual bool ShouldValidateCCCCodeOfCarrier => true;

		void RunVesselValidation(ZPropertyInfo zpi, ZString transportMode)
		{
			var transportModeIsSea = transportMode == Core.Constants.TransportModes.Sea;
			if (transportModeIsSea)
			{
				var parent = Parent;
				var rule = parent.ZZValidationHelper?.CompileRulesForCheckingZzVesselAndCarrier(transportMode);
				if (rule != null)
				{
					var globalVessel = parent.Vessel;

					if (rule.IsRadioNeeded)
					{
						if (ShouldValidateRadioCallSignOfVessel && globalVessel != null)
						{
							var gRadioCallSign = globalVessel.RV_RadioCallSign;
							if (gRadioCallSign.IsEmpty)
							{
								zpi.AddMessageError("The vessel has no radio call sign to default to this manifest. Please enter the radio call sign here for this manifest. (And update the vessel reference details if desired).");
							}
						}
					}

					if (ShouldValidateZZCarrierLinkedToZZVessel && rule.IsCarrierCodeNeeded)
					{
						var carrierCccCode = parent.CarrierCCCCode;
						if (carrierCccCode.IsEmpty)
						{
							CheckZZCarrierLinkedToZZVessel(rule, zpi, transportMode);
						}
					}
				}
			}
		}

		protected virtual bool ShouldValidateRadioCallSignOfVessel => true;

		protected virtual bool ShouldValidateZZCarrierLinkedToZZVessel => true;

		void CheckZZCarrierLinkedToZZVessel(ZZDatabaseValidationHelper.ZzVesselWithRadioAndCarrierRule rule, ZPropertyInfo zpi, string transportMode)
		{
			var transportModeIsSea = transportMode == Core.Constants.TransportModes.Sea;
			if (transportModeIsSea)
			{
				var parent = Parent;
				var globalVessel = parent.Vessel;
				if (globalVessel != null)
				{
					var zzVessel = parent.GetZZVesselFromGlobalVessel(globalVessel);
					if (zzVessel == null)
					{
						zpi.AddMessageError("No carrier code can be determined. There is no related " + rule.CountryCode + " Customs reference file for the vessel, and there is no record selected in the Carrier field where a 'CCC' code is present. Please supply a carrier.");
					}
					else
					{
						var zzCarrier = GetZZCarrierFromZZVessel(zzVessel, rule.CountryCode);
						if (zzCarrier == null && AnyCarriersInZz(rule.CountryCode))
						{
							zpi.AddMessageError("No carrier code can be determined. The related " + rule.CountryCode + " Customs reference file for this vessel has no carrier, and there is no record selected in the Carrier field where a 'CCC' code is present. Please supply a carrier.");
						}
					}
				}
			}
		}

		protected virtual ZString CarrierType => Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER;

		bool AnyCarriersInZz(string countryCode)
		{
			return Parent.Factory.ExistsInDatabase(Universal.RefCarrierCode.Schema.TableName, new ZQuery(RefCarrierCodeSchema.ZZ4_ZZZ_NKDataGrouping, countryCode));
		}

		Universal.RefCarrierCode GetZZCarrierFromZZVessel(RefVesselZZ zzVessel, string countryCode)
		{
			Universal.RefCarrierCode zzCarrier = null;
			if (zzVessel != null)
			{
				var pivotsForAllCarrriersForAllCountries = zzVessel.Factory.Load<Universal.RefCarrierVesselPivot>(new ZQuery(RefCarrierVesselPivotSchema.ZZQ_ZZO, zzVessel.PK));
				zzCarrier = (from Universal.RefCarrierVesselPivot p in pivotsForAllCarrriersForAllCountries let carrier = p.Carrier where carrier != null && carrier.ZZ4_ZZZ_NKDataGrouping == countryCode select carrier).FirstOrDefault();
			}
			return zzCarrier;
		}

		protected override void CheckAMA_RN_NKConveyanceNationality()
		{
			base.CheckAMA_RN_NKConveyanceNationality();
			ListValidation.MessageErrorIfInvalidCode(Parent.AMA_RN_NKConveyanceNationalityInfo);
			CheckAMA_RN_NKConveyanceNationalityCore();
		}

		protected virtual void CheckAMA_RN_NKConveyanceNationalityCore()
		{
			if (ShouldValidateConveyanceCountry)
			{
				MandatoryValidation.WarnIfNotEntered(Parent.AMA_RN_NKConveyanceNationalityInfo);
			}
		}

		protected virtual bool ShouldValidateConveyanceCountry => Parent.IsSea;

		protected override void CheckAMA_ApplicationCode()
		{
			base.CheckAMA_ApplicationCode();
			Parent.MasterBill.Validation.ValidateABL_BillNumber();
		}
	}
}
