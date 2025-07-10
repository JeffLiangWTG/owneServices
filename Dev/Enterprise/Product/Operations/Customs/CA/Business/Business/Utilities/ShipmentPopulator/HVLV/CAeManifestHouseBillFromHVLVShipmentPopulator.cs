using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.CA;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.CA;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Customs.CA.Business
{
	public class CAeManifestHouseBillFromHVLVShipmentPopulator : ICAeManifestHouseBillFromHVLVShipmentPopulator
	{
		public CAeManifestHouseBillFromHVLVShipmentPopulator(CusCAeMHMaster master)
			: this(master, null)
		{
		}

		public CAeManifestHouseBillFromHVLVShipmentPopulator(CusCAeMHMaster master, Action<string, int> progressUpdate)
		{
			factory = master.Factory;
			this.master = master;
			this.progressUpdate = progressUpdate;
		}

		ForwardingShipment shipment;
		ForwardingConsol consol;
		readonly BusinessObjectFactory factory;
		readonly CusCAeMHMaster master;
		readonly Action<string, int> progressUpdate;

		public ICusCAeMHMaster Populate(IForwardingShipment shipment)
		{
			var forwardingShipment = (ForwardingShipment)shipment;
			InitialisePopulatorFromShipment(forwardingShipment);

#if DEBUG
			RefreshBindingCalledOnHouseByAddress = false;
#endif

			if (!HouseBillsHaveBeenPopulated)
			{
				PopulateHouseCollection(forwardingShipment.HVLVConsignments.OfType<IHVLVConsignmentForDocument>());
				HouseCreationStatusMapping.Add(forwardingShipment, false);

				factory.Saving -= factory_Saving;
				factory.Saving += factory_Saving;
				factory.Saved -= factory_Saved;
				factory.Saved += factory_Saved;
			}

			return master;
		}

		void InitialisePopulatorFromShipment(ForwardingShipment shipment)
		{
			this.shipment = shipment;
			consol = shipment.ArrivalConsol;
		}

		void factory_Saving(BusinessObjectFactory factory)
		{
			master.PopulateJobReferenceIfNeeded();

			var shipmentsWithoutLog = HouseCreationStatusMapping.Keys.Where(shipment => !HouseCreationStatusMapping[shipment]).ToArray();
			foreach (var shipmentWithoutLog in shipmentsWithoutLog)
			{
				if (!HouseCreationStatusMapping[shipmentWithoutLog])
				{
					shipmentWithoutLog.Logs.AddNew(AutoEvents.Transferred, new KeyValuePair<string, string>[]
					{
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, "MAN"),
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, master.BP_MessageReference)
					});

					master.Logs.AddNew(AutoEvents.Transferred, new KeyValuePair<string, string>[]
					{
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.JobNumber, shipmentWithoutLog.JobNumber),
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Core.Constants.ShipmentTypes.HighVolumeLowValue)
					});

					HouseCreationStatusMapping[shipmentWithoutLog] = true;
				}
			}

			factory.RefreshEnabled = false;
		}

		void factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			factory.RefreshEnabled = true;
			if (savedSuccessfully)
			{
				factory.Saving -= factory_Saving;
				factory.Saved -= factory_Saved;
			}
		}

#if DEBUG
		internal void factory_SavingForTest()
		{
			factory_Saving(factory);
		}

		internal void factory_SavedForTest()
		{
			factory_Saved(factory, true);
		}

		internal bool RefreshBindingCalledOnHouseByAddress { get; private set; }
#endif

		public ZString GetCCN(string houseCCNSuffix)
		{
			ZString returnCCN = string.Empty;
			if (!shipment.Numbers.IsNullOrEmpty() && !shipment.Numbers.GetFirstReferenceNumberByType(CanadaAdditionalReferenceNumberTypes.Codes.CCN).CE_EntryNum.IsEmpty)
			{
				returnCCN = shipment.Numbers.GetFirstReferenceNumberByType(CanadaAdditionalReferenceNumberTypes.Codes.CCN).CE_EntryNum;
			}
			else if (!consol.Numbers.IsNullOrEmpty() && !consol.Numbers.GetFirstReferenceNumberByType(CanadaAdditionalReferenceNumberTypes.Codes.CCN).CE_EntryNum.IsEmpty)
			{
				returnCCN = consol.Numbers.GetFirstReferenceNumberByType(CanadaAdditionalReferenceNumberTypes.Codes.CCN).CE_EntryNum;
			}

			return returnCCN + houseCCNSuffix;
		}

		void PopulateHouseCollection(IEnumerable<IHVLVConsignmentForDocument> consignments)
		{
			var totalConsignmentsCount = consignments.Count();
			var processedCount = 0;
			var houseCCNSuffix = string.Empty;

			try
			{
				master.Factory.SuspendValidation();

				consignments.ForEach(x =>
				{
					houseCCNSuffix = GenerateHouseCCNSuffix(processedCount++);
					PopulateHouse(x, houseCCNSuffix);
					progressUpdate?.Invoke(Res.GetString("e47698af-5fe4-474a-abb1-8d46c8852022", "[{0} / {1}] Consignments processed",
						processedCount,
						totalConsignmentsCount),
						(int)(processedCount * 100F / totalConsignmentsCount));
				});
			}
			finally
			{
				master.Factory.ResumeValidation();
			}

			progressUpdate?.Invoke(Res.GetString("68c1c815-f2ce-4cc3-9f21-603cb1c61b0f", "All Consignments processed"), 100);
		}

		string GenerateHouseCCNSuffix(int nextSequenceNumber)
		{
			return ProgressiveNumberToBase36(nextSequenceNumber);
		}

		void PopulateHouse(IHVLVConsignmentForDocument consignment, string houseCCNSuffix)
		{
			var house = master.HouseBills.AddNew();
			house.BW_ParentID = consignment.PK;
			house.BW_ParentTableCode = HVLVConsignmentSchema.Constants.Prefix;
			house.BW_HouseBill = consignment.HVC_WaybillNumber.SubstringSafe(0, CusCAeMHHouseSchema.BW_HouseBill.MaxLength);
			house.BW_MovementType = eMHMovementTypeList.Codes.Import;
			house.BW_HouseCCN = GetCCN(houseCCNSuffix);
			house.BW_Weight = consignment.HVC_ActualWeight.IsEmpty ? (int)consignment.HVC_ManifestedWeight : (int)consignment.HVC_ActualWeight;
			house.BW_WeightUQ = CanadianUnitOfWeightList.ConvertWeightUnitToCanadianWeightUnit(consignment.HVC_WeightUQ);
			house.BW_Volume = consignment.HVC_ActualVolume.IsEmpty ? (int)consignment.HVC_ManifestedVolume : (int)consignment.HVC_ActualVolume;
			house.BW_VolumeUQ = CustomsUnitOfMeasureList.ConvertStockUnitsToCustomsUnits(consignment.HVC_VolumeUQ, house.Factory);
			house.BW_HandlingInstructions = consignment.HVC_ConsigneeInstructions;

			if (consignment.ManifestedOnShipment != null)
			{
				var forwardingShipment = consignment.ManifestedOnShipment as ForwardingShipment;
				var populator = new ReleaseCustomsPortAndSubLocationPopulator(forwardingShipment, house);
				house.BW_CBSAReleasePort = populator.GetReleaseCustomsPort().ToString();
				house.BW_CBSAReleaseSubLocation = populator.GetReleaseSubLocation().ToString();
			}

			PopulateItemsCollection(consignment, house);
			PopulateAddressCollection(consignment, house);
			PopulateContainerCollection(consol.Containers, house, consignment);
		}

		void PopulateItemsCollection(IHVLVConsignment consignment, CusCAeMHHouse house)
		{
			var itemLines = consignment.Items.OfType<IHVLVItemForDocument>().SelectMany(item => item.Lines.OfType<IHVLVItemLine>());

			foreach (var line in itemLines)
			{
				var item = house.Items.AddNew();
				item.BX_Quantity = (decimal)line.HVS_Quantity;
				item.BX_HSCode = consol.IsImport() ? line.HVS_FormattedDestinationTariff : line.HVS_FormattedOriginTariff;
				item.BX_Description = line.HVS_GoodsDescription;
				item.BX_QuantityUQ = ACROSSPackageTypes.Codes.PIECES;
			}
		}

		void PopulateAddressCollection(IHVLVConsignmentForDocument consignment, CusCAeMHHouse house)
		{
#if DEBUG
			var listChangedHandlerForTest = new ListChangedEventHandler(delegate
			{
				RefreshBindingCalledOnHouseByAddress = true;
			});

			if (Globals.IsTest)
			{
				((IBusiness)house).ListChanged += listChangedHandlerForTest;
			}
#endif

			var consignee = house.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsigneeDocumentaryAddress);
			using (consignee.SuspendRefreshingParent())
			{
				if (consignment.ConsigneeIsOrganisation)
				{
					PopulateOrganisationDocAddress(consignee, consignment.HVC_OA_ConsigneeAddress, consignment.HVC_ConsigneeContact);
				}
				else
				{
					PopulateNonOrganisationDocAddress(consignee, consignment.HVC_ConsigneeName,
						consignment.HVC_ConsigneeAddress1.SubstringSafe(0, JobDocAddressSchema.E2_Address1.MaxLength),
						consignment.HVC_ConsigneeAddress2.SubstringSafe(0, JobDocAddressSchema.E2_Address2.MaxLength),
						consignment.HVC_ConsigneeCity,
						consignment.HVC_ConsigneeState,
						consignment.HVC_ConsigneePostcode,
						consignment.HVC_RN_NKConsigneeCountryCode,
						consignment.HVC_ConsigneeContact,
						consignment.HVC_ConsigneePhone,
						consignment.HVC_ConsigneeMobile,
						consignment.HVC_ConsigneeFax,
						consignment.HVC_ConsigneeEmail);
				}
			}

			var shipper = house.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsignorDocumentaryAddress);
			using (shipper.SuspendRefreshingParent())
			{
				if (consignment.ShipperIsOrganisation)
				{
					PopulateOrganisationDocAddress(shipper, consignment.HVC_OA_ShipperAddress, consignment.HVC_ShipperContact);
				}
				else
				{
					PopulateNonOrganisationDocAddress(shipper, consignment.HVC_ShipperName,
						consignment.HVC_ShipperAddress1.SubstringSafe(0, JobDocAddressSchema.E2_Address1.MaxLength),
						consignment.HVC_ShipperAddress2.SubstringSafe(0, JobDocAddressSchema.E2_Address2.MaxLength),
						consignment.HVC_ShipperCity,
						consignment.HVC_ShipperState,
						consignment.HVC_ShipperPostcode,
						consignment.HVC_RN_NKShipperCountryCode,
						consignment.HVC_ShipperContact,
						consignment.HVC_ShipperPhone,
						consignment.HVC_ShipperMobile,
						consignment.HVC_ShipperFax,
						consignment.HVC_ShipperEmail);
				}
			}

#if DEBUG
			if (Globals.IsTest)
			{
				((IBusiness)house).ListChanged -= listChangedHandlerForTest;
			}
#endif
		}

		void PopulateContainerCollection(ForwardingContainerCollection containerCollection, CusCAeMHHouse house, IHVLVConsignment consignment)
		{
			containerCollection.OfType<ForwardingContainer>().ForEach(x => PopulateContainer(x, house, consignment));
		}

		void PopulateOrganisationDocAddress(CAeMHDocAddress docAddress, ZGuid address, ZString contact)
		{
			docAddress.E2_AddressOverride = false;
			docAddress.E2_ValidationStatus = AddressValidationStatus.ManuallyVerified;
			docAddress.E2_OA_Address = address;
			docAddress.E2_Contact = contact;
		}

		void PopulateNonOrganisationDocAddress(CAeMHDocAddress docAddress, ZString companyName, ZString address1, ZString address2, ZString city, ZString state, ZString postCode, ZString countryCode, ZString contact, ZString phone, ZString mobile, ZString fax, ZString email)
		{
			docAddress.E2_AddressOverride = true;
			docAddress.E2_ValidationStatus = AddressValidationStatus.ManuallyVerified;
			docAddress.E2_CompanyName = companyName;
			docAddress.E2_Address1 = address1.SubstringSafe(0, JobDocAddressSchema.E2_Address1.MaxLength);
			docAddress.E2_Address2 = address2.SubstringSafe(0, JobDocAddressSchema.E2_Address2.MaxLength);
			docAddress.E2_City = city;
			docAddress.E2_State = state;
			docAddress.E2_Postcode = postCode.SubstringSafe(0, JobDocAddressSchema.E2_Postcode.MaxLength);
			docAddress.E2_RN_NKCountryCode = countryCode;
			docAddress.E2_Contact = contact;
			docAddress.E2_Phone = phone;
			docAddress.E2_Mobile = mobile;
			docAddress.E2_Fax = fax;
			docAddress.E2_Email = email;
		}

		void PopulateContainer(ForwardingContainer sourceContainer, CusCAeMHHouse house, IHVLVConsignment consignment)
		{
			if (sourceContainer != null)
			{
				var existingContainer = master.Containers.Find(x => x.BQ_ContainerNumber == sourceContainer.JC_ContainerNum).FirstOrDefault();
				if (existingContainer == null)
				{
					var targetContainer = master.Containers.AddNew();
					targetContainer.BQ_ContainerNumber = sourceContainer.JC_ContainerNum;
					targetContainer.BQ_RC_NKContainerType = sourceContainer.Container.RC_Code;
					targetContainer.BQ_Seal1 = sourceContainer.JC_SealNum;
					targetContainer.BQ_Seal2 = sourceContainer.JC_AdditionalSealNum;

					if (consignment.Items.OfType<IHVLVItem>().Any(x => x.HVI_ContainerNumber == sourceContainer.JC_ContainerNum))
					{
						var pivot = house.Pivots.AddNew();
						pivot.BPA_BQ_Container = sourceContainer.PK;
					}
				}
			}
		}

		bool HouseBillsHaveBeenPopulated
		{
			get
			{
				if (HouseCreationStatusMapping.ContainsKey(shipment))
				{
					return true;
				}
				else
				{
					var masterTRFLogs = master.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.TransferredCode
						&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, out var type)
						&& type == Core.Constants.ShipmentTypes.HighVolumeLowValue
						&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.JobNumber, out var jobNumber)
						&& jobNumber == shipment.JobNumber);

					if (masterTRFLogs.Any())
					{
						HouseCreationStatusMapping.Add(shipment, true);
						return true;
					}
					else
					{
						return false;
					}
				}
			}
		}

		Dictionary<ForwardingShipment, bool> HouseCreationStatusMapping => houseCreationStatusMapping ?? (houseCreationStatusMapping = new Dictionary<ForwardingShipment, bool>());
		Dictionary<ForwardingShipment, bool> houseCreationStatusMapping;

		string ProgressiveNumberToBase36(int number)
		{
			if (number < 0 || number > maxSupportedBase36Number)
			{
				throw new ArgumentOutOfRangeException(nameof(number), number, FormattableString.Invariant($"Only numbers between 0 and {maxSupportedBase36Number} are supported, to fit in 4 characters."));
			}

			var value = number;
			var result = string.Empty;
			const int targetBase = 36;

			do
			{
				result = base36Alphabet[value % targetBase] + result;
				value = value / targetBase;
			} while (value > 0);

			while (result.Length < 4)
			{
				result = "0" + result;
			}

			return result;
		}

		const string base36Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		const int maxSupportedBase36Number = 1_679_615;     // Maximum which can be represented in 4 characters of base36.
	}
}
