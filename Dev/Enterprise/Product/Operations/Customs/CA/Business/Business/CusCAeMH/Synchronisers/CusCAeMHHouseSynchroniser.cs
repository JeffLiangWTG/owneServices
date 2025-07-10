using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.CA;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CA.Business
{
	public class CusCAeMHHouseSynchroniser : BusinessObjectSynchroniser
	{
		public CusCAeMHHouseSynchroniser(CusCAeMHHouse destination, ForwardingShipment source)
			: base(destination, source)
		{ }

		public new ForwardingShipment Source
		{
			get { return (ForwardingShipment)base.Source; }
		}

		public new CusCAeMHHouse Destination
		{
			get { return (CusCAeMHHouse)base.Destination; }
		}

		protected override void OnEnabledChanged()
		{
			base.OnEnabledChanged();
			if (Destination.BW_OverrideFreightDefaults)
			{
				var addresses = from address in Destination.DocAddresses.Cast<CAeMHDocAddress>()
								where
									address.E2_AddressType == DocAddressTypes.Codes.ImportBroker ||
									address.E2_AddressType == DocAddressTypes.Codes.Consolidator ||
									address.E2_AddressType == DocAddressTypes.Codes.PlaceOfConsolidation
								select address;
				addresses.ForEach(a => a.ReadOnly = !Destination.BW_OverrideFreightDefaults);
			}
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			if (Source != null && !Source.IsDeleted && Destination != null && !Destination.IsDeleted)
			{
				if (Destination.BW_ParentID.IsEmpty)
				{
					Destination.BW_ParentID = Source.PK;
					Destination.BW_ParentTableCode = Source.TablePrefix;
				}
				AddHouseBillInfoSynchroniser();
				AddImportBrokerSynchroniser();
				AddCosolidatorSynchroniser();
				AddPlaceOfConsolidationSynchroniser();
				AddNotesInfoSynchroniser();
				houseCCNSynchroniser = new FieldSynchroniser(Destination.BW_HouseCCNInfo, GetCCN, GetAdditionNumberInfo);
				Synchronisers.Add(houseCCNSynchroniser);
				ucrSynchroniser = new FieldSynchroniser(Destination.BW_UCRInfo, GetUCR, GetAdditionNumberInfo);
				Synchronisers.Add(ucrSynchroniser);
				Synchronisers.Add(new FieldSynchroniser(Destination.BW_CBSAReleaseSubLocationInfo, () => ReleaseCustomsPortAndSubLocationPopulator.GetReleaseSubLocation(), GetDepotAddressInfos));
				Synchronisers.Add(new FieldSynchroniser(Destination.BW_CBSAReleasePortInfo, () => ReleaseCustomsPortAndSubLocationPopulator.GetReleaseCustomsPort(), GetDepotAddressInfos));
				if (CACustomsDataRegistry.Instance.SynchronizeAssemblyMasterwithLeadShipment.Value)
				{
					Synchronisers.Add(new FieldSynchroniser(Destination.BW_MovementTypeInfo, GetMoveType, GetMoveTypeInfo, true));
				}
				Synchronisers.Add(new FieldSynchroniser(Destination.BW_WeightInfo,
					() => (ZDecimal)Math.Max(1, Source.JS_ActualWeight.Round(0)),
					() => new[] { Source.JS_ActualWeightInfo }));
				Synchronisers.Add(new FieldSynchroniser(Destination.BW_WeightUQInfo,
					() => { return (ZString)CanadianUnitOfWeightList.ConvertWeightUnitToCanadianWeightUnit(Source.JS_UnitOfWeight); },
					() => { return new[] { Source.JS_UnitOfWeightInfo }; }));
				Synchronisers.Add(new FieldSynchroniser(Destination.BW_VolumeInfo, Source.JS_ActualVolumeInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.BW_VolumeUQInfo,
					() => { return (ZString)CustomsUnitOfMeasureList.ConvertStockUnitsToCustomsUnits(Source.JS_UnitOfVolume, Destination.Factory); },
					() => { return new[] { Source.JS_UnitOfVolumeInfo }; }
					));

				var docAddresses = ((IDocAddresses)Destination).DocAddresses;
				Synchronisers.Add(new CusCAeMHItemCollectionSynchroniser(Source, Destination));
				Synchronisers.Add(new CusCAeMHHouseContainerPivotCollectionSynchroniser(Source, Destination, Destination.MasterBill.Consol));
				Synchronisers.Add(new CusCAeMHDocAddressCollectionSynchroniser(Source, Destination));

				Source.Numbers.CountChanged -= NumberCount_Changed;
				Source.Numbers.CountChanged += NumberCount_Changed;
			}
		}
		FieldSynchroniser houseCCNSynchroniser;
		FieldSynchroniser ucrSynchroniser;

		void AddImportBrokerSynchroniser()
		{
			FieldSynchroniser importBrokerSynchroniser = new FieldSynchroniser(delegate
			{
				var address = Destination.DocAddresses.Cast<CAeMHDocAddress>().FirstOrDefault(x => x.E2_AddressType == DocAddressTypes.Codes.ImportBroker);
				if (address == null)
				{
					address = Destination.DocAddresses.AddNew();
					address.E2_AddressType = DocAddressTypes.Codes.ImportBroker;
				}
				address.ReadOnly = !Destination.BW_OverrideFreightDefaults;
				return address.OrganisationPKInfo;
			},
			delegate
			{
				return Source.JS_OH_ImportBroker;
			},
			delegate
			{
				return new[] { Source.JS_OH_ImportBrokerInfo };
			},
			delegate
			{
				return false;
			});
			Synchronisers.Add(importBrokerSynchroniser);
		}

		void AddHouseBillInfoSynchroniser()
		{
			FieldSynchroniser houseBillSynchroniser = new FieldSynchroniser(Destination.BW_HouseBillInfo, delegate
			{
				return Source.JS_HouseBill;
			}, GetZPropertyInfosRelatedToHouseBill);
			Synchronisers.Add(houseBillSynchroniser);
		}
		ZPropertyInfo[] GetZPropertyInfosRelatedToHouseBill()
		{
			return new[] { Source.JS_HouseBillInfo };
		}

		void AddCosolidatorSynchroniser()
		{
			var sendingForwarderMainAddress = Destination.MasterBill.Consol?.SendingForwarder?.MainAddress;
			FieldSynchroniser consolidatorSynchroniser = new FieldSynchroniser(delegate
			{
				var address = Destination.DocAddresses.Cast<CAeMHDocAddress>().FirstOrDefault(x => x.E2_AddressType == DocAddressTypes.Codes.Consolidator);
				if (address == null)
				{
					address = Destination.DocAddresses.AddNew();
					address.E2_AddressType = DocAddressTypes.Codes.Consolidator;
				}
				address.ReadOnly = !Destination.BW_OverrideFreightDefaults;
				return address.OrganisationPKInfo;
			},
			delegate
			{
				if (Source.JS_ShipmentType == Constants.ShipmentTypes.CoLoadMaster || Source.JS_ShipmentType == Constants.ShipmentTypes.BlindCoLoadMaster)
				{
					return sendingForwarderMainAddress?.OA_OH;
				}
				else
				{
					return ZGuid.Empty;
				}
			},
			delegate
			{
				return new[] { sendingForwarderMainAddress?.OA_OHInfo, Destination.MasterBill.Consol?.JK_OA_SendingForwarderAddressInfo, Source.JS_ShipmentTypeInfo };
			},
			delegate
			{
				return false;
			});
			Synchronisers.Add(consolidatorSynchroniser);
		}

		void AddPlaceOfConsolidationSynchroniser()
		{
			var sendingForwarder = Destination.MasterBill.Consol?.SendingForwarder;
			var pickupAddress = sendingForwarder?.ActiveOrAllAddresses.Cast<OrgAddress>().FirstOrDefault(x => (x.IsAddressOfType(OrgAddressType.PickupAndDelivery) || x.IsAddressOfType(OrgAddressType.Pickup)) && x.OA_IsActive);
			pickupAddress = pickupAddress ?? sendingForwarder?.MainAddress;
			FieldSynchroniser placeOfConsolidationSynchroniser = new FieldSynchroniser(delegate
			{
				var address = Destination.DocAddresses.Cast<CAeMHDocAddress>().FirstOrDefault(x => x.E2_AddressType == DocAddressTypes.Codes.PlaceOfConsolidation);
				if (address == null)
				{
					address = Destination.DocAddresses.AddNew();
					address.E2_AddressType = DocAddressTypes.Codes.PlaceOfConsolidation;
				}
				address.ReadOnly = !Destination.BW_OverrideFreightDefaults;
				return address.E2_OA_AddressInfo;
			},
			delegate
			{
				if (Source.JS_ShipmentType == Constants.ShipmentTypes.CoLoadMaster || Source.JS_ShipmentType == Constants.ShipmentTypes.BlindCoLoadMaster)
				{
					return pickupAddress?.PK;
				}
				else
				{
					return ZGuid.Empty;
				}
			},
			delegate
			{
				return new[] { pickupAddress?.FindPropertyInfo("PK"), Destination.MasterBill.Consol?.JK_OA_SendingForwarderAddressInfo, Source.JS_ShipmentTypeInfo };
			},
			delegate
			{
				return false;
			});
			Synchronisers.Add(placeOfConsolidationSynchroniser);
		}

		void AddNotesInfoSynchroniser()
		{
			var dangerousGoodsNote = Source.Notes.FindByDescription(PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description).FirstOrDefault();
			if (dangerousGoodsNote != null)
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.BW_DGSpecialInstructionsInfo, dangerousGoodsNote.ST_NoteTextInfo));
			}
			var handlingInstructionsNote = Source.Notes.FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description).FirstOrDefault();
			if (handlingInstructionsNote != null)
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.BW_HandlingInstructionsInfo, handlingInstructionsNote.ST_NoteTextInfo));
			}
		}

		void NumberCount_Changed(object sender, CollectionCountChangedEventArgs e)
		{
			if (houseCCNSynchroniser != null)
			{
				houseCCNSynchroniser.UpdateInfoEventsAndReSynchronise();
			}
			if (ucrSynchroniser != null)
			{
				ucrSynchroniser.UpdateInfoEventsAndReSynchronise();
			}
		}

		protected override void UnHookSynchronisers()
		{
			base.UnHookSynchronisers();
			Source.Numbers.CountChanged -= NumberCount_Changed;
		}

		IZType GetMoveType()
		{
			var countryCodeOfLastLeg = Source.ArrivalConsol?.Transports.ArrivalTransport?.JW_RL_NKDiscPort.SubstringSafe(0, 2) ?? ZString.Empty;
			if (countryCodeOfLastLeg != Core.Constants.CountryCodes.Canada)
			{
				return (ZString)eMHMovementTypeList.Codes.InTransit;
			}
			return (ZString)eMHMovementTypeList.Codes.Import;
		}

		IEnumerable<ZPropertyInfo> GetMoveTypeInfo()
		{
			if (Source.ArrivalConsol?.MostInterestingTransportForBinding?.FirstOrDefault() is Transport transportLast)
			{
				yield return transportLast.JW_RL_NKDiscPortInfo;
			}
		}

		IEnumerable<ZPropertyInfo> GetDepotAddressInfos()
		{
			yield return Source.JS_OA_ImportReleaseDepotInfo;
			if (Source.ArrivalConsol is ForwardingConsol arrivalConsol)
			{
				yield return arrivalConsol.JK_OA_UnpackDepotAddressInfo;
				foreach (var info in Calculator.GetInfosAffectingFirstCountryPortOfDischarge())
				{
					yield return info;
				}
			}
			yield return Source.JS_PackingModeInfo;
		}

		IZType GetUCR()
		{
			var ucr = Source.Numbers.GetFirstReferenceNumberByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.UniqueConsignmentReference);
			return ucr != null ? ucr.CE_EntryNum : ZString.Empty;
		}

		IZType GetCCN()
		{
			var ccn = Source.Numbers.GetFirstReferenceNumberByType(CanadaAdditionalReferenceNumberTypes.Codes.CCN);
			return ccn != null ? ccn.CE_EntryNum : ZString.Empty;
		}

		IEnumerable<ZPropertyInfo> GetAdditionNumberInfo()
		{
			foreach (CusEntryNumber number in Source.Numbers)
			{
				yield return number.CE_EntryNumInfo;
				yield return number.CE_EntryTypeInfo;
				yield return number.CE_RN_NKCountryCodeInfo;
			}
		}

		ConsolDataCalculator Calculator
		{
			get { return fCalculator ?? (fCalculator = new ConsolDataCalculator(Source.ArrivalConsol, Destination.MasterBill)); }
		}
		ConsolDataCalculator fCalculator;

		ReleaseCustomsPortAndSubLocationPopulator ReleaseCustomsPortAndSubLocationPopulator
		{
			get { return fReleaseCustomsPortAndSubLocationPopulator ?? (fReleaseCustomsPortAndSubLocationPopulator = new ReleaseCustomsPortAndSubLocationPopulator(Source, Destination)); }
		}
		ReleaseCustomsPortAndSubLocationPopulator fReleaseCustomsPortAndSubLocationPopulator;
	}
}
