using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AsycudaManifestHeaderValidation : ASYCUDA.Business.ManifestHeaderValidation
	{
		public AsycudaManifestHeaderValidation(AsycudaManifestHeader parent)
			: base(parent)
		{
		}

		public new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		protected override bool IsVoyageMandatory => base.IsVoyageMandatory && !Parent.IsSea && !Parent.IsInlandWaterway && !Parent.SpecificCircumstanceIndicator.EqualsIgnoringCase(EUICS2SpecificCircumstanceList.Codes.F44);

		public void ValidateSplitConsignmentIndicator()
		{
			ValidateCalculatedProperty(Parent.SplitConsignmentIndicatorInfo);
		}

		public void ValidateMOTIdentifierType()
		{
			ValidateCalculatedProperty(Parent.MOTIdentifierTypeInfo);
		}

		public void ValidateAddressedMemberState()
		{
			ValidateCalculatedProperty(Parent.AddressedMemberStateInfo);
		}

		public void ValidatePreviousMRN()
		{
			ValidateCalculatedProperty(Parent.PreviousMRNInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateSplitConsignmentIndicator();
			ValidateAddressedMemberState();
			ValidatePreviousMRN();
			ValidateReceptacleId();
		}

		protected override void CheckSpecificCircumstanceIndicator()
		{
			var parent = Parent;
			ListValidation.MessageErrorIfInvalidCode(parent.SpecificCircumstanceIndicatorInfo);
			MandatoryValidation.MessageErrorIfNotEntered(parent.SpecificCircumstanceIndicatorInfo);

			var dict = parent.Factory.GetCachedValue("ValidSpecificCircumstanceIndicatorDataDictionary", () => SpecificCircumstanceIndicatorValidDataCombinationCollection.GetValidSpecificCircumstanceIndicatorDataDictionary());

			if (dict.TryGetValue(parent.SpecificCircumstanceIndicator, out var validData))
			{
				if (!validData.Match(parent))
				{
					parent.SpecificCircumstanceIndicatorInfo.AddMessageError(Res.GetString("7FBF88E5-C176-40F5-9C8F-6ED8A78213CD", "The selected ICS2 submission may fail, based on the Type of Manifest and Mode of Transport entered."));
				}

				if (ValidationHelper.SpecificCircumstanceListRequiringTwoItineraryRecords.Contains(parent.SpecificCircumstanceIndicator) && parent.Itinerary.Count < 2)
				{
					parent.SpecificCircumstanceIndicatorInfo.AddMessageError(Res.GetString("D31141ED-63F4-4DD8-87FB-207961BEB6BE", "The selected ICS2 submission may fail, at least 2 records on Itinerary Tab are required."));
				}
			}

			ValidateMOTIdentifierType();
			ValidateAMA_MasterBill();
			ValidateAMA_CustomsOffice();
			parent.MasterBill.Validation.ValidateABL_RL_NKPortOfLoading();
			parent.MasterBill.Validation.ValidateABL_RL_NKPortOfDischarge();
		}

		protected override void CheckAMA_OA_Carrier()
		{
			base.CheckAMA_OA_Carrier();
			if (Parent.Carrier != null && EU.Business.Extensions.GetICS2EoriDetails(Parent.Carrier.Header).IsEmpty)
			{
				Parent.AMA_OA_CarrierInfo.AddMessageError(Res.GetString("c09e7c39-e0de-47b6-ad9f-4550e06b5fb2", "Carrier must have EORI entered"));
			}
		}

		protected override void CheckAMA_OA_CarrierMandatory()
		{
			if (Parent.SpecificCircumstanceIndicator != EUICS2SpecificCircumstanceList.Codes.F44)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_OA_CarrierInfo);
			}
		}

		protected override void CheckAMA_OA_ShippingAgent()
		{
			base.CheckAMA_OA_ShippingAgent();

			if (Parent.ShippingAgent != null)
			{
				if (EU.Business.Extensions.GetICS2EoriDetails(Parent.ShippingAgent.Header).IsEmpty)
				{
					Parent.AMA_OA_ShippingAgentInfo.AddMessageError(Res.GetString("0f30dcb4-dcf1-4118-a59b-bd7704bd51c8", "Shipping Agent must have EORI entered"));
				}

				if (Parent.ShippingAgent.OA_Email.IsEmpty && Parent.ShippingAgent.OA_Phone.IsEmpty)
				{
					Parent.AMA_OA_ShippingAgentInfo.AddMessageError(ValidationHelper.EmptyPhoneNumberAndEmailErrorMessage);
				}

				ValidationHelper.CheckValidPhoneNumber(Parent.ShippingAgent.OA_Phone, Parent.AMA_OA_ShippingAgentInfo, Res.GetString("b4fb1c20-501d-4f30-b3c4-6763c8a1deb4", "Shipping Agent"));
			}
		}

		protected override void CheckAMA_TransportMeans()
		{
			base.CheckAMA_TransportMeans();

			var header = Parent;
			if (header.SpecificCircumstanceIndicator != EUICS2SpecificCircumstanceList.Codes.F44)
			{
				MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyHasValues(Parent.AMA_TransportMeansInfo,
					Parent.AMA_TransportModeInfo,
					new IZType[] { (ZString)TransportTypeList.Codes.Road, (ZString)TransportTypeList.Codes.Rail, },
					MandatoryValidation.YouHaveNotEnteredMessage(Parent.AMA_TransportMeansInfo.HumanReadableName));
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.AMA_TransportMeansInfo);
		}

		protected override void CheckAMA_TransportMode()
		{
			base.CheckAMA_TransportMode();

			ValidateSpecificCircumstanceIndicator();
		}

		protected void CheckMOTIdentifierType()
		{
			var parent = Parent;
			ListValidation.MessageErrorIfInvalidCode(parent.MOTIdentifierTypeInfo);
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyHasValue(parent.MOTIdentifierTypeInfo, parent.SpecificCircumstanceIndicatorInfo,
				(ZString)EUICS2SpecificCircumstanceList.Codes.F40,
				MandatoryValidation.YouHaveNotEnteredMessage(parent.MOTIdentifierTypeInfo.HumanReadableName));
		}

		protected void CheckAddressedMemberState()
		{
			if (Parent.AddressedMemberState.IsEmpty || !Parent.Lookups.CountryCodeICS2MS.ContainsCode(Parent.AddressedMemberState))
			{
				Parent.AddressedMemberStateInfo.AddError(Res.GetString("038FFE6E-0F55-4443-B490-07F152B1BA67", "Please enter a valid Country."));
			}
		}

		protected override void CheckAMA_CustomsOffice()
		{
			base.CheckAMA_CustomsOffice();

			var parent = Parent;

			var message = Res.GetString("7E03C690-2F10-4D68-8755-BA4D72DDE8A9", "You have not entered a Customs Office of First Entry.");

			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyHasValues(parent.AMA_CustomsOfficeInfo, parent.SpecificCircumstanceIndicatorInfo,
				new IZType[] { (ZString)EUICS2SpecificCircumstanceList.Codes.F40, (ZString)EUICS2SpecificCircumstanceList.Codes.F50 },
				message);
		}

		public void ValidateReceptacleId()
		{
			ValidateCalculatedProperty(Parent.ReceptacleIdInfo);
		}

		protected void CheckReceptacleId()
		{
			var parent = Parent;

			var message = Res.GetString("D9E60BB9-F7CC-4BDD-A7E6-E3716158CF6F", "You have not entered a Receptacle Identification Number.");

			if (parent.IsCarrierManifest && parent.Receptacles.Count == 0)
			{
				MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyHasValues(parent.ReceptacleIdInfo, parent.SpecificCircumstanceIndicatorInfo,
					new IZType[] { (ZString)EUICS2SpecificCircumstanceList.Codes.F40, (ZString)EUICS2SpecificCircumstanceList.Codes.F41 },
					message);
			}
		}

		protected void CheckPreviousMRN()
		{
			var parent = Parent;
			if (parent.SpecificCircumstanceIndicator == EUICS2SpecificCircumstanceList.Codes.F25)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.PreviousMRNInfo);
			}
		}

		protected override void CheckAMA_RN_NKConveyanceNationality()
		{
			var parent = Parent;
			if (parent.AMA_TransportMode == Constants.TransportModes.Road && !parent.AMA_VehicleRegistration.IsEmpty && parent.AMA_RN_NKConveyanceNationality.IsEmpty)
			{
				parent.AMA_RN_NKConveyanceNationalityInfo.AddMessageError(Res.GetString("31DE6B4B-B00B-4B4C-8EBA-D36A795C1887", "Vehicle Registration Country is mandatory for Transport Mode Road"));
			}
		}

		protected override void CheckAMA_MasterBill()
		{
			base.CheckAMA_MasterBill();
			var parent = Parent;
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyHasValue(parent.AMA_MasterBillInfo, parent.SpecificCircumstanceIndicatorInfo,
				(ZString)EUICS2SpecificCircumstanceList.Codes.F40,
				MandatoryValidation.YouHaveNotEnteredMessage(parent.AMA_MasterBillInfo.HumanReadableName));
		}

		protected override void CheckAMA_PaymentMethod()
		{
			base.CheckAMA_PaymentMethod();
			var parent = Parent;
			if (parent.IsCarrierManifest && (parent.IsRoad || parent.IsRail) && (parent.SpecificCircumstanceIndicator == EUICS2SpecificCircumstanceList.Codes.F50 || parent.SpecificCircumstanceIndicator == EUICS2SpecificCircumstanceList.Codes.F51))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.AMA_PaymentMethodInfo);
			}
		}

		protected override void CheckAMA_OA_Declarant()
		{
			base.CheckAMA_OA_Declarant();

			var parent = Parent;
			var propertyInfo = parent.AMA_OA_DeclarantInfo;

			MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);

			var declarant = parent.Declarant;
			if (declarant != null && EU.Business.Extensions.GetICS2EoriDetails(declarant.Header).IsEmpty)
			{
				propertyInfo.AddMessageError(Res.GetString("BD63AF76-D81F-49C0-A37E-01C77E32E0AF", "Declarant must have EORI entered."));
			}
		}

		protected override void CheckAMA_VehicleRegistrationCore()
		{
			if(Parent.SpecificCircumstanceIndicator != EUICS2SpecificCircumstanceList.Codes.F44)
			{
				base.CheckAMA_VehicleRegistrationCore();
			}
		}

		protected override void CheckAMA_RN_NKCountry()
		{
		}
	}
}
