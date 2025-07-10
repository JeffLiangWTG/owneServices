//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusCAeMHHouseValidation
//
//    This class should be used for overriding validation in AutoCusCAeMHHouseValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Edifact;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using FreightRegistry = Enterprise.Registry.Business.FreightDataRegistry;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CusCAeMHHouseValidation : AutoCusCAeMHHouseValidation
	{
		public CusCAeMHHouseValidation(AutoCusCAeMHHouse parent)
			: base(parent)
		{
		}

		protected new CusCAeMHHouse Parent
		{
			get { return (CusCAeMHHouse)base.Parent; }
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateJobDocAddresses();
				ValidateContainer();
				ValidateUNDGs();
			}
		}

		void ValidateUNDGs()
		{
			if (Parent.NumberOfUNDG > CusCAeMHHouse.MaxNumberOfUNDG)
			{
				Parent.AddRowMessageError(Res.GetString("1a641acb-70bf-41a3-b9f4-a335b9d1638c", "Please make sure the number of Dangerous(DG) Pack Line Items is less than {0}.", CusCAeMHHouse.MaxNumberOfUNDG));
			}
		}

		void ValidateContainer()
		{
			var containerProvider = (IACIHouseBillProvider)Parent;
			if (containerProvider.Containers.Any(x => (x.ContainerNumber.Length < 5 || x.ContainerNumber.Length > 16)))
			{
				Parent.AddRowMessageError(ContainerIdentifierLessThanMaxLimt);
			}
		}

		void ValidateJobDocAddresses()
		{
			void ValidateAddressProperties(CAeMHDocAddressDependentCollection addresses, ZString addressType, ZBool isMandatory)
			{
				var address = addresses.Find(addressType);
				if (isMandatory && (address == null || address.IsEmpty))
				{
					Parent.AddRowMessageError(YouMustEnterAnAddress(CAeMHDocAddress.GetAddressCaption(addressType)));
				}
				else if (address != null)
				{
					ValidProperty(address.E2_CompanyNameInfo, CusCAeMHHouse.MaxPartyName, isMandatory, true);
					ValidProperty(address.E2_CityInfo, CusCAeMHHouse.MaxCity, isMandatory);
					if (address.E2_RN_NKCountryCode == Core.Constants.CountryCodes.Canada || address.E2_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedStates)
					{
						ValidProperty(address.E2_StateInfo, CusCAeMHHouse.MaxState, false);
						ValidProperty(address.E2_PostcodeInfo, CusCAeMHHouse.MaxPostCode, false);
					}
					ValidProperty(address.E2_ContactInfo, CusCAeMHHouse.MaxContactName, false);
					ValidProperty(address.E2_RN_NKCountryCodeInfo, CusCAeMHHouse.MaxCountryCode, isMandatory);
				}

				void ValidProperty(ZPropertyInfo propertyInfo, ZInt maxLength, ZBool isRequired, bool isCheckAfterReplacement = false)
				{
					var fieldName = propertyInfo.HumanReadableName;
					var fieldValue = (ZString)propertyInfo.Value.ToString();
					if (isRequired && fieldValue.IsEmpty)
					{
						Parent.AddRowMessageError(ErrorMessageFieldMandatory(fieldName));
					}
					else if (fieldValue.Length > maxLength)
					{
						Parent.AddRowMessageError(ErrorMessageMakeSureMaxLength(fieldName, maxLength));
					}

					if (!fieldValue.IsEmpty && isCheckAfterReplacement)
					{
						var fieldValueWithoutSpecialCharacters = (ZString)CharacterSet.ReplaceSpecialCharacters(fieldValue);
						if (fieldValueWithoutSpecialCharacters.IsEmpty)
						{
							Parent.AddRowMessageError(EorrorMessageFieldIsEmptyAfterReplacement(fieldName));
						}
					}
				}
			}

			var docAddresses = Parent.DocAddresses;
			var houseBill = (Parent as IACIHouseBillProvider);
			var masterBill = Parent.MasterBill;
			ValidateAddressProperties(docAddresses, DocAddressTypes.Codes.ConsigneeDocumentaryAddress, true);
			ValidateAddressProperties(docAddresses, DocAddressTypes.Codes.ConsignorDocumentaryAddress, true);
			ValidateAddressProperties(docAddresses, DocAddressTypes.Codes.ConsigneePickupDeliveryAddress, false);
			ValidateAddressProperties(docAddresses, DocAddressTypes.Codes.NotifyParty, false);
			ValidateAddressProperties(docAddresses, DocAddressTypes.Codes.Consolidator, false);
			ValidateAddressProperties(docAddresses, DocAddressTypes.Codes.PlaceOfConsolidation, (houseBill?.IsConsolidatedCargo ?? false) && (masterBill?.PlaceOfConsolidation.IsEmpty ?? false));
			foreach (JobDocAddress docAddress in docAddresses.ToArray())
			{
				docAddress.Validation.ValidateE2_OA_Address();
			}
		}

		UNOACharacterSet CharacterSet => fCharacterSet ?? (fCharacterSet = new UNOACharacterSet());
		UNOACharacterSet fCharacterSet;

		internal static string ErrorMessageMakeSureMaxLength(string fieldName, int maxNum) => Res.GetString("0E153BE0-3416-4B3E-BA37-7255D3E21E36", "The length of data entered into {0} exceeds the maximum allowed. Only the first {1} characters will be transmitted to Customs", fieldName, maxNum);
		internal static string ErrorMessageFieldMandatory(string fieldName) => Res.GetString("A08402C1-717F-4308-A1A2-759A18B22E27", "{0} is mandatory when you send a message.", fieldName);
		internal static string YouMustEnterAnAddress(string addressType) => Res.GetString("c068bee1-85c4-4fd4-bdee-db240d584a58", "You must enter {0} address.", addressType);
		internal static string EorrorMessageFieldIsEmptyAfterReplacement(string fieldName) => Res.GetString("6DC3DCC3-B4C2-413E-8C98-D161D952CDDC", "{0} are more than 2 illegal characters or the illegal characters represent more than 1/4.", fieldName);

		protected override void CheckBW_IsMasterHouse()
		{
			base.CheckBW_IsMasterHouse();

			if (Parent.BW_IsMasterHouse)
			{
				if (Parent.PlaceOfConsolidation == null)
				{
					Parent.BW_IsMasterHouseInfo.AddMessageError(Res.GetString("A662E298-330A-4980-B6EB-CC0DB8D517E0", "Place of Consolidation is required when Consolidation is marked Yes."));
				}
			}
			else
			{
				var warning = Res.GetString("4324A534-F9E9-4FEB-AF8D-8B0C9BB197C7", "{0} will not be sent when Consolidation is not ticked Yes.");

				var housePOC = Parent.DocAddresses.FindByDocAddressType(DocAddressType.PlaceOfConsolidation);
				if (housePOC != null)
				{
					Parent.BW_IsMasterHouseInfo.AddWarning(string.Format(warning, DocAddressTypes.Descriptions.PlaceOfConsolidation));
				}

				var consolidator = Parent.DocAddresses.FindByDocAddressType(DocAddressType.Consolidator);
				if (consolidator != null)
				{
					Parent.BW_IsMasterHouseInfo.AddWarning(string.Format(warning, DocAddressTypes.Descriptions.Consolidator));
				}
			}
		}

		protected override void CheckBW_HouseBill()
		{
			base.CheckBW_HouseBill();
			var houseBill = Parent.BW_HouseBill;
			if (houseBill.IsEmpty)
			{
				Parent.BW_HouseBillInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(Parent.BW_HouseBillInfo.HumanReadableName));
			}
			else
			{
				var pk = Parent.PK;
				var master = Parent.MasterBill;
				var billNumberDuplicated = false;

				if (master != null)
				{
					if (master.IsValidatingAll)
					{
#if DEBUG
						master.ValidationCallsOriginatingFromMaster++;
#endif
						if (master.HouseValidationDictionary.ContainsKey(houseBill))
						{
							billNumberDuplicated = master.HouseValidationDictionary[houseBill];
						}
						else
						{
							ErrorReporter.ReportOnce($"House bill {houseBill} is supposed to exist in the dictionary.");
						}
					}
					else
					{
						if (master.HouseBills.Any(x => x.PK != pk && x.BW_HouseBill.EqualsIgnoringCase(houseBill)))
						{
							billNumberDuplicated = true;
						}
					}

					if (billNumberDuplicated)
					{
						Parent.BW_HouseBillInfo.AddMessageError(Res.GetString("7B23C9BC-BD37-4140-BFDE-FBD2A92C8F02", "Another House Bill has same bill number."));
					}
				}
			}
		}

		protected override void CheckBW_HouseCCN()
		{
			base.CheckBW_HouseCCN();
			if (FreightRegistry.Instance.CanadaCargoControlNumberCustomization.Value == Core.Constants.ShipmentCCNCustomizationTypes.Code.NumberFountain)
			{
				if (Parent.BW_HouseCCN.IsEmpty)
				{
					Parent.BW_HouseCCNInfo.AddWarning(Res.GetString("82A07E32-D10D-4560-8E12-A393E9C694B3", "System will create a {0} automatically if it is not entered.", Parent.BW_HouseCCNInfo.HumanReadableName));
				}
			}
			else
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BW_HouseCCNInfo);
			}
			if (!Parent.BW_HouseCCN.IsEmpty && (Parent.BW_HouseCCN.Length < 5 ||
						(Parent.MasterBill != null && !Parent.MasterBill.BP_CBSACarrierCode.IsEmpty && !Parent.BW_HouseCCN.StartsWith(Parent.MasterBill.BP_CBSACarrierCode, System.StringComparison.OrdinalIgnoreCase))))
			{
				Parent.BW_HouseCCNInfo.AddMessageError(Res.GetString("7e678fae-5bc4-4fa4-ae92-1820ae205959", "The house CCN should start with the FF Carrier Code specified on the header and be followed by a unique value"));
			}
			if (Parent.IsLodged && Parent.BW_HouseCCNInfo.HasChanges)
			{
				Parent.BW_HouseCCNInfo.AddError(Res.GetString("a91fe7f4-e2f8-4401-bf3f-354c9fb2c8b8", "This is a key field and a House Bill message has already been filed. You must submit a withdrawal before you can change this field."));
			}
			CheckCCNIsUnique();
		}

		void CheckCCNIsUnique()
		{
			if (!Parent.BW_HouseCCN.IsEmpty)
			{
				var query = new ZQuery(CusCAeMHHouseSchema.BW_HouseCCN, Parent.BW_HouseCCN);
				query.AddToFilter(CusCAeMHHouseSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				var billWithTheSameCCN = Parent.Factory.LoadTop1<CusCAeMHHouse>(query);
				if (billWithTheSameCCN != null)
				{
					Parent.BW_HouseCCNInfo.AddMessageError(CCNMustBeUnique(billWithTheSameCCN.BW_HouseBill));
				}
			}
		}
		internal static string CCNMustBeUnique(string ccn)
		{
			return Res.GetString("2dfc09d7-2e5f-4768-9bc5-7598bea2cb0e", "This CCN was used in house bill {0}", ccn);
		}

		protected override void CheckBW_MovementType()
		{
			base.CheckBW_MovementType();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BW_MovementTypeInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.BW_MovementTypeInfo);
		}

		protected override void CheckBW_AmendReasonCode()
		{
			base.CheckBW_AmendReasonCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.BW_AmendReasonCodeInfo);
			if (Parent.IsPostArrival)
			{
				if (Parent.BW_AmendReasonCode.IsEmpty && Parent.IsLodged)
				{
					Parent.BW_AmendReasonCodeInfo.AddMessageError(Res.GetString("7c153fcb-b38a-4c5f-b0c7-541d72f7a971", "This shipment has arrived and has been reported so if you submit an amendment then an amendment reason will be required."));
				}
			}
			else
			{
				var masterBill = Parent.MasterBill;
				var amendReasonCode = Parent.BW_AmendReasonCode;
				if (masterBill != null && masterBill.BP_ATA.IsEmpty && !amendReasonCode.IsEmpty)
				{
					Parent.BW_AmendReasonCodeInfo.AddMessageError(CusCAeMHMasterValidation.AmendReasonCodeIsNotAllowedError);
				}
				else if (amendReasonCode != EManifestAmendmentReasonCodes.Codes.PortOrSubLocation)
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.BW_AmendReasonCodeInfo);
				}
			}
		}

		protected override void CheckBW_Weight()
		{
			base.CheckBW_Weight();
			MandatoryValidation.CheckNotNegative(Parent.BW_WeightInfo);

			if (Parent.BW_Weight == 0m)
			{
				Parent.BW_WeightInfo.AddMessageError(WeightIsMandatory);
			}
		}

		internal static string WeightIsMandatory => Res.GetString("43C5B2AE-A4CD-4455-A636-816EDAA7CB70", "Weight is mandatory for eManifest filings.");

		protected override void CheckBW_WeightUQ()
		{
			base.CheckBW_WeightUQ();
			if (Parent.BW_Weight != 0 && Parent.BW_WeightUQ.IsEmpty)
			{
				Parent.BW_WeightUQInfo.AddMessageError(Res.GetString("B75C015B-5643-4F6A-A5E3-700B521F314A", "You have not entered Weight UQ."));
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.BW_WeightUQInfo);
		}

		protected override void CheckBW_VolumeUQ()
		{
			base.CheckBW_VolumeUQ();
			if (Parent.BW_Volume != 0 && Parent.BW_VolumeUQ.IsEmpty)
			{
				Parent.BW_VolumeUQInfo.AddMessageError(Res.GetString("903809AC-F7AD-402B-B8DB-DD7EE9B90051", "You have not entered Volume UQ."));
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.BW_VolumeUQInfo);
		}

		protected override void CheckBW_CBSAReleasePort()
		{
			base.CheckBW_CBSAReleasePort();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BW_CBSAReleasePortInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.BW_CBSAReleasePortInfo);
		}

		protected override void CheckBW_CBSAReleaseSubLocation()
		{
			base.CheckBW_CBSAReleaseSubLocation();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BW_CBSAReleaseSubLocationInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.BW_CBSAReleaseSubLocationInfo);
		}

		public static string ContainerIdentifierLessThanMaxLimt => Res.GetString("107F24E1-5702-4917-83FF-AD50801D3461", "If containerized, the Equipment Identifier consists of an Equipment Initial and an Equipment Number. (min 5 , max 16)");
	}
}
