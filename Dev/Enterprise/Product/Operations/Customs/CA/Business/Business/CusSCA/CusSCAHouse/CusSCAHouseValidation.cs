using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CusSCAHouseValidation : Customs.Business.CusSCAHouseValidation
	{
		public CusSCAHouseValidation(Customs.Business.BaseCusSCAHouse parent)
			: base(parent)
		{
		}

		public CusSCAHouse House
		{
			get { return Parent; }
		}

		protected new CusSCAHouse Parent
		{
			get { return (CusSCAHouse)base.Parent; }
		}

		public void ValidateMaxCountOfPacking()
		{
			var message = ExceedsMaxCountOfPacking;
			Parent.RemoveRowMessageError(message);
			if (Parent.PackLines.Count > MaxCountAllowed)
			{
				Parent.AddRowMessageError(message);
			}
		}
		const int MaxCountAllowed = 998;

		static string ExceedsMaxCountOfPacking
		{
			get { return Res.GetString("8420a64e-32b7-4576-b421-a312a7fb029e", @"The maximum number of packing rows allowed in ACI is {0}.", MaxCountAllowed); }
		}

		public void ValidateCarrierCode()
		{
			ValidateCalculatedProperty(House.CarrierCodeInfo);
		}

		public void ValidateSupplementaryReferenceNumber()
		{
			ValidateCalculatedProperty(House.SupplementaryReferenceNumberInfo);
		}

		public void ValidateOriginalCCN()
		{
			ValidateCalculatedProperty(House.OriginalCCNInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCarrierCode();
			ValidateSupplementaryReferenceNumber();
			ValidateOriginalCCN();
			ValidateMaxCountOfPacking();
		}

		protected void CheckCarrierCode()
		{
			MandatoryValidation.MessageErrorIfNotEntered(House.CarrierCodeInfo, CarrierCodeMessage);
		}
		static string CarrierCodeMessage
		{
			get { return Res.GetString("e07c6f3e-cb2b-4ea3-b099-6c8bb8f3bcdb", @"Carrier Code. The carrier code is entered on the Proxy Organization assigned on the Canadian Company.
(i.e. Maintain -> User Admin -> Companies -> Organization -> Detail -> Config -> Standard Carrier Alpha Code)"); }
		}

		protected void CheckSupplementaryReferenceNumber()
		{
			MandatoryValidation.MessageErrorIfNotEntered(House.SupplementaryReferenceNumberInfo, CCNMessage);
		}
		static string CCNMessage
		{
			get { return Res.GetString("94a61994-4702-49f9-b484-9c783c244c8f", @"Supplementary Reference Number. This number is allocated automatically to the shipment.
The first 4 characters of the CCN should be your CBSA assigned alpha Carrier Code, followed by the Shipment Number and may then be followed by an optional suffix specified by a registry item (Admin-> Registry -> Customs -> Country or Region Specific -> Canada -> Import -> ACI -> Supplementary Reference Number Suffix)."); }
		}

		protected void CheckOriginalCCN()
		{
			if (House.OriginalCCN.Length < 5)
			{
				House.OriginalCCNInfo.AddMessageError(OrigCCNMessage);
			}
		}
		internal static string OrigCCNMessage
		{
			get { return Res.GetString("76616926-3f3d-4a80-b401-471d4052cbb0", @"You have not entered a valid Original Cargo Control Number. The CCN is defaulted from the consol to which the shipment is attached.
Either enter it on the ACI Master/Ocean Bill tab, or enter an Original CCN on the consol: Consol -> Numbers -> CCN or PCN.
Note that the first 4 characters of the CCN for a SEA shipment should be the CBSA assigned alpha Carrier Code for the carrier. An AIR shipment should start with the 3 character airline code, followed by a '-', then followed by the 4 character carrier code, and then a unique number (e.g. 014-999012345678)."); }
		}

		protected override void CheckCA_RL_NK_PortOfDestination()
		{
			base.CheckCA_RL_NK_PortOfDestination();
			MandatoryValidation.MessageErrorIfNotEntered(House.CA_RL_NK_PortOfDestinationInfo);
			ListValidation.MessageErrorIfInvalidCode(House.CA_RL_NK_PortOfDestinationInfo);
		}

		protected override void CheckCA_FROBTransitImportCode()
		{
			base.CheckCA_FROBTransitImportCode();
			MandatoryValidation.MessageErrorIfNotEntered(House.CA_FROBTransitImportCodeInfo);
			ListValidation.MessageErrorIfInvalidCode(House.CA_FROBTransitImportCodeInfo);
		}

		protected override void CheckCA_ConsigneeName()
		{
			base.CheckCA_ConsigneeName();
			MandatoryValidation.MessageErrorIfNotEntered(House.CA_ConsigneeNameInfo);
			EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(House.CA_ConsigneeNameInfo);
		}

		protected override void CheckCA_ConsigneeAddress1()
		{
			base.CheckCA_ConsigneeAddress1();
			MandatoryValidation.MessageErrorIfNotEntered(House.CA_ConsigneeAddress1Info);
			CAAddressValidator.MaxLengthValidation(House.CA_ConsigneeAddress1Info, 35);
			EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(House.CA_ConsigneeAddress1Info);
		}

		protected override void CheckCA_ConsigneeAddress2()
		{
			base.CheckCA_ConsigneeAddress2();
			CAAddressValidator.MaxLengthValidation(House.CA_ConsigneeAddress2Info, 35);
			EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(House.CA_ConsigneeAddress2Info);
		}

		protected override void CheckCA_ConsigneeState()
		{
			base.CheckCA_ConsigneeState();
			CAAddressValidator.MaxLengthValidation(House.CA_ConsigneeStateInfo, 9);
			CAAddressValidator.StateValidation(House.CA_ConsigneeStateInfo, House.CA_RN_NKConsigneeCountryCode);
		}

		protected override void CheckCA_ConsigneePostcode()
		{
			base.CheckCA_ConsigneePostcode();
			CAAddressValidator.MaxLengthValidation(House.CA_ConsigneePostcodeInfo, 9);
			CAAddressValidator.PostCodeValidation(House.CA_ConsigneePostcodeInfo, House.CA_RN_NKConsigneeCountryCode);
			EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(House.CA_ConsigneePostcodeInfo);
		}

		protected override void CheckCA_ConsigneeSuburb()
		{
			base.CheckCA_ConsigneeSuburb();
			MandatoryValidation.MessageErrorIfNotEntered(House.CA_ConsigneeSuburbInfo);
			CAAddressValidator.MaxLengthValidation(House.CA_ConsigneeSuburbInfo, 35);
			EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(House.CA_ConsigneeSuburbInfo);
		}

		protected override void CheckCA_RN_NKConsigneeCountryCode()
		{
			base.CheckCA_RN_NKConsigneeCountryCode();
			MandatoryValidation.MessageErrorIfNotEntered(House.CA_RN_NKConsigneeCountryCodeInfo);
			ListValidation.MessageErrorIfInvalidCode(House.CA_RN_NKConsigneeCountryCodeInfo);
		}

		protected override void CheckCA_ConsigneeContactName()
		{
			base.CheckCA_ConsigneeContactName();
			EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(House.CA_ConsigneeContactNameInfo);
		}

		protected override void CheckCA_ConsignorName()
		{
			base.CheckCA_ConsignorName();
			MandatoryValidation.MessageErrorIfNotEntered(House.CA_ConsignorNameInfo);
			EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(House.CA_ConsignorNameInfo);
		}

		protected override void CheckCA_ConsignorAddress1()
		{
			base.CheckCA_ConsignorAddress1();
			MandatoryValidation.MessageErrorIfNotEntered(House.CA_ConsignorAddress1Info);
			CAAddressValidator.MaxLengthValidation(House.CA_ConsignorAddress1Info, 35);
			EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(House.CA_ConsignorAddress1Info);
		}

		protected override void CheckCA_ConsignorAddress2()
		{
			base.CheckCA_ConsignorAddress2();
			CAAddressValidator.MaxLengthValidation(House.CA_ConsignorAddress2Info, 35);
			EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(House.CA_ConsignorAddress2Info);
		}

		protected override void CheckCA_ConsignorState()
		{
			base.CheckCA_ConsignorState();
			CAAddressValidator.MaxLengthValidation(House.CA_ConsignorStateInfo, 9);
			CAAddressValidator.StateValidation(House.CA_ConsignorStateInfo, House.CA_RN_NKConsignorCountryCode);
		}

		protected override void CheckCA_ConsignorPostcode()
		{
			base.CheckCA_ConsignorPostcode();
			CAAddressValidator.MaxLengthValidation(House.CA_ConsignorPostcodeInfo, 9);
			CAAddressValidator.PostCodeValidation(House.CA_ConsignorPostcodeInfo, House.CA_RN_NKConsignorCountryCode);
			EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(House.CA_ConsignorPostcodeInfo);
		}

		protected override void CheckCA_ConsignorSuburb()
		{
			base.CheckCA_ConsignorSuburb();
			MandatoryValidation.MessageErrorIfNotEntered(House.CA_ConsignorSuburbInfo);
			CAAddressValidator.MaxLengthValidation(House.CA_ConsignorSuburbInfo, 35);
			EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(House.CA_ConsignorSuburbInfo);
		}

		protected override void CheckCA_RN_NKConsignorCountryCode()
		{
			base.CheckCA_RN_NKConsignorCountryCode();
			MandatoryValidation.MessageErrorIfNotEntered(House.CA_RN_NKConsignorCountryCodeInfo);
			ListValidation.MessageErrorIfInvalidCode(House.CA_RN_NKConsignorCountryCodeInfo);
		}

		protected override void CheckCA_ConsignorContactName()
		{
			base.CheckCA_ConsignorContactName();
			EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(House.CA_ConsignorContactNameInfo);
		}

		protected override void CheckCA_DeliveryName()
		{
			base.CheckCA_DeliveryName();
			EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(House.CA_DeliveryNameInfo);
		}

		protected override void CheckCA_DeliveryAddress1()
		{
			base.CheckCA_DeliveryAddress1();
			if (!House.CA_DeliveryName.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(House.CA_DeliveryAddress1Info);
				CAAddressValidator.MaxLengthValidation(House.CA_DeliveryAddress1Info, 35);
				EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(House.CA_DeliveryAddress1Info);
			}
		}

		protected override void CheckCA_DeliveryAddress2()
		{
			base.CheckCA_DeliveryAddress2();
			CAAddressValidator.MaxLengthValidation(House.CA_DeliveryAddress2Info, 35);
			EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(House.CA_DeliveryAddress2Info);
		}

		protected override void CheckCA_DeliveryState()
		{
			base.CheckCA_DeliveryState();
			CAAddressValidator.MaxLengthValidation(House.CA_DeliveryStateInfo, 9);
			CAAddressValidator.StateValidation(House.CA_DeliveryStateInfo, House.CA_RN_NKDeliveryCountryCode);
		}

		protected override void CheckCA_DeliveryPostcode()
		{
			base.CheckCA_DeliveryPostcode();
			CAAddressValidator.MaxLengthValidation(House.CA_DeliveryPostcodeInfo, 9);
			CAAddressValidator.PostCodeValidation(House.CA_DeliveryPostcodeInfo, House.CA_RN_NKDeliveryCountryCode);
			EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(House.CA_DeliveryPostcodeInfo);
		}

		protected override void CheckCA_DeliverySuburb()
		{
			base.CheckCA_DeliverySuburb();
			if (!House.CA_DeliveryName.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(House.CA_DeliverySuburbInfo);
				CAAddressValidator.MaxLengthValidation(House.CA_DeliverySuburbInfo, 35);
				EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(House.CA_DeliverySuburbInfo);
			}
		}

		protected override void CheckCA_RN_NKDeliveryCountryCode()
		{
			base.CheckCA_RN_NKDeliveryCountryCode();
			if (!House.CA_DeliveryName.IsEmpty || !House.CA_RN_NKDeliveryCountryCode.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(House.CA_RN_NKDeliveryCountryCodeInfo);
				ListValidation.MessageErrorIfInvalidCode(House.CA_RN_NKDeliveryCountryCodeInfo);
			}
		}

		protected override void CheckCA_DeliveryContactName()
		{
			base.CheckCA_DeliveryContactName();

			if (!House.CA_DeliveryName.IsEmpty && (House.CA_DeliveryName != House.CA_ConsigneeName ||
House.CA_DeliveryAddress1 != House.CA_ConsigneeAddress1 ||
House.CA_DeliveryAddress2 != House.CA_ConsigneeAddress2 ||
House.CA_DeliverySuburb != House.CA_ConsigneeSuburb ||
House.CA_DeliveryState != House.CA_ConsigneeState ||
House.CA_DeliveryContactName != House.CA_ConsigneeContactName ||
House.CA_DeliveryPhone != House.CA_ConsigneePhone))
			{
				MandatoryValidation.MessageErrorIfNotEntered(House.CA_DeliveryContactNameInfo);
				EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(House.CA_DeliveryContactNameInfo);
			}
		}

		protected override void CheckCA_NotifyName()
		{
			base.CheckCA_NotifyName();
			EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(House.CA_NotifyNameInfo);
		}

		protected override void CheckCA_NotifyAddress1()
		{
			base.CheckCA_NotifyAddress1();
			if (!House.CA_NotifyName.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(House.CA_NotifyAddress1Info);
				CAAddressValidator.MaxLengthValidation(House.CA_NotifyAddress1Info, 35);
				EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(House.CA_NotifyAddress1Info);
			}
		}

		protected override void CheckCA_NotifySuburb()
		{
			base.CheckCA_NotifySuburb();
			if (!House.CA_NotifyName.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(House.CA_NotifySuburbInfo);
				CAAddressValidator.MaxLengthValidation(House.CA_NotifySuburbInfo, 35);
				EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(House.CA_NotifySuburbInfo);
			}
		}

		protected override void CheckCA_NotifyAddress2()
		{
			base.CheckCA_NotifyAddress2();
			CAAddressValidator.MaxLengthValidation(House.CA_NotifyAddress2Info, 35);
			EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(House.CA_NotifyAddress2Info);
		}

		protected override void CheckCA_NotifyState()
		{
			base.CheckCA_NotifyState();
			CAAddressValidator.MaxLengthValidation(House.CA_NotifyStateInfo, 9);
			CAAddressValidator.StateValidation(House.CA_NotifyStateInfo, House.CA_RN_NKNotifyCountryCode);
		}

		protected override void CheckCA_NotifyPostcode()
		{
			base.CheckCA_NotifyPostcode();
			CAAddressValidator.MaxLengthValidation(House.CA_NotifyPostcodeInfo, 9);
			CAAddressValidator.PostCodeValidation(House.CA_NotifyPostcodeInfo, House.CA_RN_NKNotifyCountryCode);
			EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(House.CA_NotifyPostcodeInfo);
		}

		protected override void CheckCA_NotifyContactName()
		{
			base.CheckCA_NotifyContactName();
			EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(House.CA_NotifyContactNameInfo);
		}

		protected override void CheckCA_RN_NKNotifyCountryCode()
		{
			base.CheckCA_RN_NKNotifyCountryCode();
			if (!House.CA_NotifyName.IsEmpty || !House.CA_RN_NKNotifyCountryCode.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(House.CA_RN_NKNotifyCountryCodeInfo);
				ListValidation.MessageErrorIfInvalidCode(House.CA_RN_NKNotifyCountryCodeInfo);
			}
		}
	}
}
