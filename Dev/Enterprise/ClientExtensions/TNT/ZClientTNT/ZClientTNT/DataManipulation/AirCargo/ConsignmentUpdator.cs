using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.TNT.AirCargo
{
	public class ConsignmentUpdator
	{
		public ConsignmentUpdator(ConsignmentRecord consignment, ConsignmentNoteRecordCollection consignmentNotes, INotifications notify)
		{
			this.Consignment = consignment;
			this.ConsignmentNotes = consignmentNotes;
			this.Notify = notify;
		}

		public void Update(CusHAWB houseBill)
		{
			UpdateConsignor(houseBill);
			UpdateConsignee(houseBill);
			SetPropertyInfoValue(houseBill.CS_HAWBInfo, Consignment.HouseBill, ForeignKeyType.None);
			SetPropertyInfoValue(houseBill.CS_RL_NKOriginInfo, Consignment.Origin, ForeignKeyType.PortNK);
			SetPropertyInfoValue(houseBill.CS_RL_NKDestinationInfo, (houseBill.CS_RN_NKConsigneeCountry == Core.Constants.CountryCodes.Australia && houseBill.MAWB != null) ? houseBill.MAWB.CM_RL_NKDischargePort : Consignment.Destination, ForeignKeyType.PortNK);
			houseBill.CS_Weight = Consignment.Weight;
			houseBill.CS_ChargableWeight = houseBill.CS_Weight;
			SetPropertyInfoValue(houseBill.CS_WeightUQInfo, Core.Constants.Weight.Kilograms, ForeignKeyType.None);
			houseBill.CS_GoodsValue = Consignment.GoodsValue;
			SetPropertyInfoValue(houseBill.CS_RX_NKGoodsCurrencyInfo, Consignment.GoodsCurrency, ForeignKeyType.CurrencyNK);
			UpdateGoodsDescription(houseBill);
			houseBill.CS_PiecesManifested = Consignment.PackageCount;
			UpdateTermsOfPayment(houseBill);
			UpdateShipmentType(houseBill);
			UpdateSelfAssessedClearanceFlag(houseBill);
		}

		public void UpdateQuantumOriginalValue(CusHAWB houseBill, ZString sector, ZString branchCode)
		{
			if (houseBill != null)
			{
				StmNote[] sectorNotes = houseBill.Notes.FindByDescription(OriginalQuantumSectorNoteDescription);
				StmNote sectorNote = null;
				if (sectorNotes.Length > 0)
				{
					sectorNote = sectorNotes[0];
				}
				else
				{
					sectorNote = houseBill.Notes.AddNew(true, OriginalQuantumSectorNoteDescription, "");
				}

				sectorNote.ST_NoteDataAsText = ZString.Format("{0}-{1}-{2}-{3}", sector, Consignment.Origin.Left(3).PadRight(3),
						Consignment.Destination.Left(3).PadRight(3), branchCode.Left(3).PadRight(3));
			}
		}

		#region Implementation

		#region Update HouseBill

		void UpdateConsignor(CusHAWB houseBill)
		{
			houseBill.CS_OH_Consignor = ZGuid.Empty;
			SetPropertyInfoValue(houseBill.CS_ConsignorContactNameInfo, Consignment.ConsignorContactName.Trim(), ForeignKeyType.None);
			SetPropertyInfoValue(houseBill.CS_ConsignorNameInfo, SetNotSuppliedIfBlank(Consignment.ConsignorName), ForeignKeyType.None);
			SetPropertyInfoValue(houseBill.CS_ConsignorStreetInfo, SetNotSuppliedIfBlank(Consignment.ConsignorAddress1.IsEmpty ? Consignment.ConsignorAddress2 : Consignment.ConsignorAddress1), ForeignKeyType.None);

			if (!Consignment.ConsignorAddress1.IsEmpty && !Consignment.ConsignorAddress2.IsEmpty)
			{
				SetPropertyInfoValue(houseBill.CS_ConsignorStreet2Info, SetNotSuppliedIfBlank(Consignment.ConsignorAddress2), ForeignKeyType.None);
			}

			SetPropertyInfoValue(houseBill.CS_ConsignorCityInfo, Consignment.ConsignorCity, ForeignKeyType.None);
			SetPropertyInfoValue(houseBill.CS_RN_NKConsignorCountryInfo, Consignment.ConsignorCountry, ForeignKeyType.CountryNK);
			SetPropertyInfoValue(houseBill.CS_ConsignorStateInfo, GetStateCode(houseBill.CS_RN_NKConsignorCountry, Consignment.ConsignorState), ForeignKeyType.None);
			SetPropertyInfoValue(houseBill.CS_ConsignorPostcodeInfo, Consignment.ConsignorPostCode, ForeignKeyType.None);
			SetPropertyInfoValue(houseBill.CS_ConsignorPhoneInfo, Consignment.ConsignorContactPhone.IsEmpty ? Consignment.ConsignorPhone : Consignment.ConsignorContactPhone, ForeignKeyType.None);

			if (IsPickupDetailSpecified(Consignment))
			{
				UpdateConsignorPickup(LoadOrCreateCusHawbAddress(houseBill, DocAddressTypes.Codes.SupplierPickupDeliveryAddress), true);
			}
			else
			{
				UpdateConsignorPickup(LoadOrCreateCusHawbAddress(houseBill, DocAddressTypes.Codes.SupplierPickupDeliveryAddress), false);
			}
		}

		bool IsPickupDetailSpecified(ConsignmentRecord consignment)
		{
			return (!consignment.PickupAddress1.KeepChars(ImportAirCargo.ValidCharacters, " ").IsEmpty || !consignment.PickupAddress2.KeepChars(ImportAirCargo.ValidCharacters, " ").IsEmpty);
		}

		void UpdateConsignorPickup(JobDocAddress pickup, bool isPickupDetailSpecified)
		{
			JobDocAddressRecord record = null;

			if (isPickupDetailSpecified)
			{
				record = new JobDocAddressRecord(Consignment.PickupContactName, Consignment.PickupName, Consignment.PickupAddress1, Consignment.PickupAddress2,
					Consignment.PickupCity, Consignment.PickupCountry, Consignment.PickupPostCode, Consignment.PickupState,
					(Consignment.PickupContactPhone.IsEmpty ? Consignment.PickupPhone : Consignment.PickupContactPhone));
			}
			else
			{
				record = new JobDocAddressRecord(Consignment.ConsignorContactName, Consignment.ConsignorName, Consignment.ConsignorAddress1, Consignment.ConsignorAddress2,
					Consignment.ConsignorCity, Consignment.ConsignorCountry, Consignment.ConsignorPostCode, Consignment.ConsignorState, Consignment.ConsignorPhone);
			}

			MapJobDecAddressDetails(pickup, record);
		}

		void MapJobDecAddressDetails(JobDocAddress jobdocAddress, JobDocAddressRecord record)
		{
			SetPropertyInfoValue(jobdocAddress.E2_ContactInfo, record.ContactName, ForeignKeyType.None);
			SetPropertyInfoValue(jobdocAddress.E2_CompanyNameInfo, record.Name, ForeignKeyType.None);
			if (record.Address1.IsEmpty)
			{
				SetPropertyInfoValue(jobdocAddress.E2_Address1Info, record.Address2, ForeignKeyType.None);
			}
			else
			{
				SetPropertyInfoValue(jobdocAddress.E2_Address1Info, record.Address1, ForeignKeyType.None);
				SetPropertyInfoValue(jobdocAddress.E2_Address2Info, record.Address2, ForeignKeyType.None);
			}
			SetPropertyInfoValue(jobdocAddress.E2_CityInfo, record.City, ForeignKeyType.None);
			SetPropertyInfoValue(jobdocAddress.E2_StateInfo, GetStateCode(record.Country, record.State), ForeignKeyType.None);
			SetPropertyInfoValue(jobdocAddress.E2_PostcodeInfo, record.Postcode, ForeignKeyType.None);
			SetPropertyInfoValue(jobdocAddress.E2_PhoneInfo, record.Phone, ForeignKeyType.None);
			SetPropertyInfoValue(jobdocAddress.E2_RN_NKCountryCodeInfo, record.Country.Trim(), ForeignKeyType.None);
		}

		void UpdateConsignee(CusHAWB houseBill)
		{
			houseBill.CS_OH_Consignee = ZGuid.Empty;
			SetPropertyInfoValue(houseBill.CS_ConsigneeContactNameInfo, Consignment.ConsigneeContactName, ForeignKeyType.None);
			SetPropertyInfoValue(houseBill.CS_ConsigneeNameInfo, SetNotSuppliedIfBlank(Consignment.ConsigneeName), ForeignKeyType.None);
			SetPropertyInfoValue(houseBill.CS_ConsigneeStreetInfo, SetNotSuppliedIfBlank(Consignment.ConsigneeAddress1.IsEmpty ? Consignment.ConsigneeAddress2 : Consignment.ConsigneeAddress1), ForeignKeyType.None);

			if (!Consignment.ConsigneeAddress1.IsEmpty && !Consignment.ConsigneeAddress2.IsEmpty)
			{
				SetPropertyInfoValue(houseBill.CS_ConsigneeStreet2Info, SetNotSuppliedIfBlank(Consignment.ConsigneeAddress2), ForeignKeyType.None);
			}

			SetPropertyInfoValue(houseBill.CS_ConsigneeCityInfo, Consignment.ConsigneeCity, ForeignKeyType.None);
			SetPropertyInfoValue(houseBill.CS_RN_NKConsigneeCountryInfo, Consignment.ConsigneeCountry, ForeignKeyType.CountryNK);
			SetPropertyInfoValue(houseBill.CS_ConsigneeStateInfo, GetStateCode(houseBill.CS_RN_NKConsigneeCountry, Consignment.ConsigneeState), ForeignKeyType.None);
			SetPropertyInfoValue(houseBill.CS_ConsigneePostcodeInfo, Consignment.ConsigneePostCode, ForeignKeyType.None);
			SetPropertyInfoValue(houseBill.CS_ConsigneePhoneInfo, Consignment.ConsigneeContactPhone.IsEmpty ? Consignment.ConsigneePhone : Consignment.ConsigneeContactPhone, ForeignKeyType.None);

			if (IsDeliveryDetailSpecified(Consignment))
			{
				UpdateConsigneeDelivery(LoadOrCreateCusHawbAddress(houseBill, DocAddressTypes.Codes.ImporterPickupDeliveryAddress), true);
			}
			else
			{
				UpdateConsigneeDelivery(LoadOrCreateCusHawbAddress(houseBill, DocAddressTypes.Codes.ImporterPickupDeliveryAddress), false);
			}
		}

		bool IsDeliveryDetailSpecified(ConsignmentRecord consignment)
		{
			return (!consignment.DeliveryAddress1.KeepChars(ImportAirCargo.ValidCharacters, " ").IsEmpty || !consignment.DeliveryAddress2.KeepChars(ImportAirCargo.ValidCharacters, " ").IsEmpty);
		}

		void UpdateConsigneeDelivery(JobDocAddress delivery, bool deliveryDetailSpecified)
		{
			JobDocAddressRecord record = null;

			if (deliveryDetailSpecified)
			{
				record = new JobDocAddressRecord(Consignment.DeliveryContactName, Consignment.DeliveryName, Consignment.DeliveryAddress1, Consignment.DeliveryAddress2,
					Consignment.DeliveryCity, Consignment.DeliveryCountry, Consignment.DeliveryPostCode, Consignment.DeliveryState,
					(Consignment.DeliveryContactPhone.IsEmpty ? Consignment.DeliveryPhone : Consignment.DeliveryContactPhone));
			}
			else
			{
				record = new JobDocAddressRecord(Consignment.ConsigneeContactName, Consignment.ConsigneeName, Consignment.ConsigneeAddress1, Consignment.ConsigneeAddress2,
					Consignment.ConsigneeCity, Consignment.ConsigneeCountry, Consignment.ConsigneePostCode, Consignment.ConsigneeState, Consignment.ConsigneePhone);
			}
			MapJobDecAddressDetails(delivery, record);
		}

		JobDocAddress LoadOrCreateCusHawbAddress(CusHAWB housebill, ZString addressType)
		{
			ZQuery filter = new ZQuery(JobDocAddressSchema.E2_ParentID, housebill.PK);
			filter.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, CusHAWBSchema.Constants.Prefix);
			filter.AddToFilter(JobDocAddressSchema.E2_AddressType, addressType);

			JobDocAddress result = housebill.Factory.LoadTop1<JobDocAddress>(filter);
			if (result == null)
			{
				result = housebill.Factory.New<JobDocAddress>();
				result.E2_AddressOverride = true;
				result.E2_ParentID = housebill.PK;
				result.E2_AddressType = addressType;
				result.E2_ParentTableCode = CusHAWBSchema.Constants.Prefix;
				result.E2_OA_Address = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.PrimaryOfficeAddress;
			}

			return result;
		}

		void UpdateGoodsDescription(CusHAWB houseBill)
		{
			ConsignmentNoteRecord consignmentNote = ConsignmentNotes[Consignment.HouseBill, 1];
			SetPropertyInfoValue(houseBill.CS_GoodsDescriptionInfo, consignmentNote != null && !consignmentNote.NoteText.IsEmpty ? consignmentNote.NoteText.ToString() : DefaultEmptyGoodsDescriptionText, ForeignKeyType.None);
		}

		ZString SetNotSuppliedIfBlank(ZString source)
		{
			return source.IsEmpty ? NotSuppliedString : source.ToString();
		}

		void UpdateTermsOfPayment(CusHAWB houseBill)
		{
			ZString termsOfPayment;
			switch (Consignment.TermsOfPayment)
			{
				case "S":
					termsOfPayment = "PO";
					break;
				case "R":
					termsOfPayment = "CC";
					break;
				default:
					termsOfPayment = ZString.Empty;
					break;
			}

			SetPropertyInfoValue(houseBill.CS_FreightPrepaidCollectInfo, termsOfPayment, ForeignKeyType.None);
		}

		void UpdateShipmentType(CusHAWB houseBill)
		{
			SetPropertyInfoValue(houseBill.CS_ShipmentTypeInfo, Consignment.DocumentIndicator == "D" ? "DOC" : "STD", ForeignKeyType.None);
		}

		void UpdateSelfAssessedClearanceFlag(CusHAWB houseBill)
		{
			houseBill.CS_IsSelfAssessedClearance = IsOKToSetSAC(houseBill);
		}

		bool IsNotTranshipment(CusHAWB houseBill)
		{
			return houseBill.CS_RL_NKDestination.Left(2) == GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

		bool IsNotMasterHouse(CusHAWB houseBill)
		{
			return !houseBill.CS_IsMasterHouse;
		}

		bool IsOKToSetSAC(CusHAWB houseBill)
		{
			bool result = IsNotTranshipment(houseBill) && IsNotMasterHouse(houseBill);
			if (result)
			{
				SACDecider decider = new SACDecider(houseBill.Factory, houseBill.GoodsValueInLocalCurrency, houseBill.GoodsDescription);
				result = decider.IsValidForSAC;
			}
			return result;
		}

		#endregion

		#region GetStateCode

		internal ZString GetStateCode(ZString country, ZString state)
		{
			ZString result = state;

			if (country.Trim() == Core.Constants.CountryCodes.Australia)
			{
				result = GetAustraliaStateCode(state);
			}
			return result;
		}

		ZString GetAustraliaStateCode(ZString state)
		{
			ZString result = "";

			switch (state.Trim().ToUpper().Replace(".", ""))
			{
				case "NEW SOUTH WALES":
					result = "NSW";
					break;
				case "VICTORIA":
					result = "VIC";
					break;
				case "QUEENSLAND":
					result = "QLD";
					break;
				case "WESTERN AUSTRALIA":
					result = "WA";
					break;
				case "SOUTH AUSTRALIA":
					result = "SA";
					break;
				case "TASMANIA":
					result = "TAS";
					break;
				case "NORTHERN TERRITORY":
					result = "NT";
					break;
				default:
					result = state;
					break;
			}

			return result;
		}

		#endregion

		#region SetPropertyInfoValue

		void SetPropertyInfoValue(ZPropertyInfo property, ZString valueAsString, ForeignKeyType fKType)
		{
			Mapper.SetPropertyInfoValue(property, GetValidString(valueAsString).Trim().Left(property.MaxLength), fKType, Notify);
		}

		ZString GetValidString(ZString value)
		{
			ZString result = ZString.Empty;

			if (!value.IsEmpty)
			{
				result = value.KeepChars(ImportAirCargo.ValidCharacters, " ");
			}

			return result;
		}

		#endregion

		#region Mapper

		TNTStringToBusinessObjectFieldConverter Mapper
		{
			get
			{
				if (fMapper == null)
				{
					fMapper = TNTStringToBusinessObjectFieldConverter.Instance;
				}
				return fMapper;
			}
		}

		TNTStringToBusinessObjectFieldConverter fMapper;

		#endregion

		class JobDocAddressRecord
		{
			public JobDocAddressRecord(ZString contactName, ZString name, ZString address1, ZString address2, ZString city, ZString country, ZString postcode, ZString state, ZString phone)
			{
				ContactName = contactName;
				Name = name;
				Address1 = address1.KeepChars(ImportAirCargo.ValidCharacters, " ");
				Address2 = address2.KeepChars(ImportAirCargo.ValidCharacters, " ");
				City = city;
				Country = country;
				Postcode = postcode;
				State = state;
				Phone = phone;
			}

			internal ZString ContactName;
			internal ZString Name;
			internal ZString Address1;
			internal ZString Address2;
			internal ZString City;
			internal ZString Country;
			internal ZString Postcode;
			internal ZString State;
			internal ZString Phone;
		}

		internal readonly ConsignmentRecord Consignment;
		internal readonly ConsignmentNoteRecordCollection ConsignmentNotes;
		internal readonly INotifications Notify;
		internal const string NotSuppliedString = "NOT SUPPLIED";
		internal const string DefaultEmptyGoodsDescriptionText = "No Information Available";
		internal const string OriginalQuantumSectorNoteDescription = "ORIGINAL QUANTUM SECTOR";

		#endregion
	}
}
