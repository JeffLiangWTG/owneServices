using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using ResString = DocumentWrappers.ResString;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class CartageInfoWrapperFromIDocCartageAdvice : CartageInfoWrapper
	{
		public CartageInfoWrapperFromIDocCartageAdvice(IDocCartageAdvice parentInfo, BusinessObjectFactory factory)
			: base(parentInfo as BusinessObject, factory)
		{
			this.parentInfo = parentInfo ?? new EmptyIDocCartageAdvice();

			SetupFromIDocCartageAdvice(parentInfo);

			DocBaseJobDeclaration docBaseJobDeclaration = parentInfo as DocBaseJobDeclaration;
			if (docBaseJobDeclaration != null)
			{
				SetupFromDocBaseJobDeclaration(docBaseJobDeclaration);
				return;
			}

			DocCommonContainer docCommonContainer = parentInfo as DocCommonContainer;
			if (docCommonContainer != null)
			{
				SetupFromDocCommonContainer(docCommonContainer);
				return;
			}

			DocBaseCusContainer docBaseCusContainer = parentInfo as DocBaseCusContainer;
			if (docBaseCusContainer != null)
			{
				SetupFromDocBaseCusContainer(docBaseCusContainer);
				return;
			}

			DocPickupDeliveryConfirm docConfirm = parentInfo as DocPickupDeliveryConfirm;
			if (docConfirm != null)
			{
				SetupFromDocPickupDeliveryConfirm(docConfirm);
				return;
			}

			DocCommonCartageLeg docContainerLeg = parentInfo as DocCommonCartageLeg;
			if (docContainerLeg != null)
			{
				SetupFromDocCommonCartageLeg(docContainerLeg);
				return;
			}

			DocCommonCartage docCartage = parentInfo as DocCommonCartage;
			if (docCartage != null)
			{
				SetupFromDocCommonCartage(docCartage);
				return;
			}
		}

		void SetupFromDocBaseJobDeclaration(DocBaseJobDeclaration docBaseJobDeclaration)
		{
			if (!docBaseJobDeclaration.PrintTwoJourneys)
			{
				if (docBaseJobDeclaration.Shipment != null)
				{
					var shipment = docBaseJobDeclaration.Shipment;

					if (shipment.IsExportDocument && shipment.DocsAndCartage.PickupRequiredBy.IsValid)
					{
						journeyOnePickUpDate = shipment.DocsAndCartage.PickupRequiredBy;
					}
					else if (shipment.IsImportDocument && shipment.DocsAndCartage.EstimatedDelivery.IsValid)
					{
						journeyOnePickUpDate = shipment.DocsAndCartage.EstimatedDelivery;
					}

					if (shipment.IsExportDocument && shipment.BookingCutOffDate.IsValid)
					{
						journeyOneDeliverToDate = shipment.BookingCutOffDate;
					}
					else if (shipment.IsImportDocument && shipment.DocsAndCartage.DeliveryRequiredBy.IsValid)
					{
						journeyOneDeliverToDate = shipment.DocsAndCartage.DeliveryRequiredBy;
					}
				}
				else
				{
					if (docBaseJobDeclaration.IsExportMessage && docBaseJobDeclaration.DocsAndCartage.PickupRequiredBy.IsValid)
					{
						journeyOnePickUpDate = docBaseJobDeclaration.DocsAndCartage.PickupRequiredBy;
					}
					else if (!docBaseJobDeclaration.IsExportMessage && docBaseJobDeclaration.DocsAndCartage.EstimatedDelivery.IsValid)
					{
						journeyOnePickUpDate = docBaseJobDeclaration.DocsAndCartage.EstimatedDelivery;
					}

					if (!docBaseJobDeclaration.IsExportMessage && docBaseJobDeclaration.DocsAndCartage.DeliveryRequiredBy.IsValid)
					{
						journeyOneDeliverToDate = docBaseJobDeclaration.DocsAndCartage.DeliveryRequiredBy;
					}
				}
			}
		}

		void SetupFromDocCommonContainer(DocCommonContainer docCommonContainer)
		{
			//JourneyOnePickUpHeading
			//Export
			if (docCommonContainer.JourneyOnePickUpAddress != null && docCommonContainer.JourneyOnePickUpAddress.DocAddressType == DocAddressType.LocalCartageYard
				&& !docCommonContainer.ReleaseNum.IsEmpty)
			{
				journeyOnePickUpReleaseNum += " REF. " + docCommonContainer.ReleaseNum;
			}

			//Import
			if (docCommonContainer.JourneyOnePickUpAddress != null && docCommonContainer.JourneyOnePickUpAddress.DocAddressType == DocAddressType.LocalCartageCTO
				&& (!docCommonContainer.SlotArrivalReference.IsEmpty || docCommonContainer.SlotArrivalTime.IsValid))
			{
				journeyOnePickUpDate = docCommonContainer.SlotArrivalTime.IsValid ? docCommonContainer.SlotArrivalTime : ZDateTime.Empty;
				journeyOnePickUpSlofRef = docCommonContainer.SlotArrivalReference;
			}

			//JourneyOneDeliverToHeading
			//Export
			if (docCommonContainer.JourneyOnePickUpAddress != null && docCommonContainer.JourneyOnePickUpAddress.DocAddressType == DocAddressType.LocalCartageYard
				&& docCommonContainer.EmptyRequired.IsValid)
			{
				journeyOneDeliverToDate = docCommonContainer.EmptyRequired;
			}

			//Import
			if (docCommonContainer.Cartage.IsImport && docCommonContainer.EstimatedDelivery.IsValid)
			{
				journeyOneDeliverToDate = docCommonContainer.EstimatedDelivery;
			}

			//JourneyTwoPickUpHeading
			//Export
			if (docCommonContainer.Cartage.IsExport && docCommonContainer.EstimatedPickup.IsValid)
			{
				journeyTwoPickUpDate = docCommonContainer.EstimatedPickup;
			}

			//JourneyTwoDeliverToHeading
			//Export
			if (docCommonContainer.JourneyTwoDeliverToAddress != null && docCommonContainer.JourneyTwoDeliverToAddress.DocAddressType == DocAddressType.LocalCartageCTO
				&& (!docCommonContainer.SlotDepartureReference.IsEmpty || docCommonContainer.SlotDepartureTime.IsValid))
			{
				journeyTwoDeliverToDate = docCommonContainer.SlotDepartureTime.IsValid ? docCommonContainer.SlotDepartureTime : ZDateTime.Empty;
				journeyTwoDeliverToSlofRef = docCommonContainer.SlotDepartureReference;
			}

			//Import
			if (docCommonContainer.JourneyTwoDeliverToAddress != null && docCommonContainer.JourneyTwoDeliverToAddress.DocAddressType == DocAddressType.LocalCartageYard
				&& !docCommonContainer.EmptyReturnedBy.IsEmpty)
			{
				journeyTwoDeliverToDate = docCommonContainer.EmptyReturnedBy;
			}
		}

		void SetupFromDocBaseCusContainer(DocBaseCusContainer docCusContainer)
		{
			bool isExport = docCusContainer.IsExportDocument;
			bool isImport = docCusContainer.IsImportDocument;
			if (!isImport && !isExport)
			{
				isExport = docCusContainer.IsExportMessage;
				isImport = docCusContainer.IsImportMessage;
			}

			//JourneyOnePickUpHeading
			if (isExport && (!docCusContainer.ReleaseNum.IsEmpty || docCusContainer.DepartureEstimatedPickup.IsValid))
			{
				journeyOnePickUpReleaseNum = docCusContainer.ReleaseNum;
			}
			else if (isImport)
			{
				journeyOnePickUpReleaseNum = docCusContainer.ContainerImportDORelease;
				if (!docCusContainer.SlotArrivalReference.IsEmpty || docCusContainer.SlotArrivalTime.IsValid)
				{
					journeyOnePickUpDate = docCusContainer.SlotArrivalTime.IsValid ? docCusContainer.SlotArrivalTime : ZDateTime.Empty;
					journeyOnePickUpSlofRef = docCusContainer.SlotArrivalReference;
				}
			}

			//JourneyOneDeliverToHeading
			if (isExport)
			{
				journeyOneDeliverToDate = docCusContainer.DropOffEmpty;
			}
			else if (isImport)
			{
				journeyOneDeliverToDate = docCusContainer.EstimatedDelivery;
				if (journeyOneDeliverToDate.IsEmpty && docCusContainer.Declaration != null)
				{
					journeyOneDeliverToDate = docCusContainer.Declaration.EstimatedDeliveryOrPickup;
				}
			}

			//JourneyTwoPickUpHeading
			if (isExport && docCusContainer.PickUpFull.IsValid)
			{
				journeyTwoPickUpDate = docCusContainer.PickUpFull;
			}

			//JourneyTwoDeliverToHeading
			if (isExport && (!docCusContainer.SlotDepartureReference.IsEmpty || docCusContainer.SlotDepartureTime.IsValid))
			{
				journeyTwoDeliverToDate = docCusContainer.SlotDepartureTime.IsValid ? docCusContainer.SlotDepartureTime : ZDateTime.Empty;
				journeyTwoDeliverToSlofRef = docCusContainer.SlotDepartureReference;
			}
			else if (isImport && docCusContainer.ReturnEmpty.IsValid)
			{
				journeyTwoDeliverToDate = docCusContainer.ReturnEmpty;
			}
		}

		void SetupFromDocPickupDeliveryConfirm(DocPickupDeliveryConfirm docConfirm)
		{
			//Workaround for Cartage Request (it uses Container Yard for Journey two) until DJB can Add Emtpy Confirmations to Forwarding
			docConfirm.ShowContainerYardAsJourneyTwo = false;

			string confirmType = docConfirm.CommonConfirm.EU_PickupDeliveryType;
			bool showArrivalSlot = docConfirm.IsContainerised && (confirmType == Constants.PickupDeliveryConfirmTypes.OriginPickup || confirmType == Constants.PickupDeliveryConfirmTypes.OriginCFSDeparture);
			bool showDepartureSlot = docConfirm.IsContainerised && (confirmType == Constants.PickupDeliveryConfirmTypes.DestinationDelivery || confirmType == Constants.PickupDeliveryConfirmTypes.DestinationCFSArrival);

			switch (confirmType)
			{
				case Constants.PickupDeliveryConfirmTypes.OriginPickup:
				case Constants.PickupDeliveryConfirmTypes.OriginCFSDeparture:
				case Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture:
					{
						printJourneyOne = false;
						printJourneyTwo = true;
						break;
					}
				case Constants.PickupDeliveryConfirmTypes.DestinationDelivery:
				case Constants.PickupDeliveryConfirmTypes.OriginCFSArrival:
				case Constants.PickupDeliveryConfirmTypes.DestinationCFSArrival:
					{
						printJourneyOne = true;
						printJourneyTwo = false;
						break;
					}
			}

			legNotes = docConfirm.LegNotes;

			if (showDepartureSlot && docConfirm.Container != null)
			{
				journeyOnePickUpDate = docConfirm.Container.SlotArrivalTime.IsValid ? docConfirm.Container.SlotArrivalTime : ZDateTime.Empty;
				journeyOnePickUpSlofRef = docConfirm.Container.SlotArrivalReference;
			}
			else
			{
				journeyOnePickUpDate = docConfirm.PlannedPickupTime;
				journeyTwoPickUpDate = docConfirm.PlannedPickupTime;
			}

			if (showArrivalSlot && docConfirm.Container != null)
			{
				journeyTwoDeliverToDate = docConfirm.Container.SlotDepartureTime.IsValid ? docConfirm.Container.SlotDepartureTime : ZDateTime.Empty;
				journeyTwoDeliverToSlofRef = docConfirm.Container.SlotDepartureReference;
			}
			else
			{
				journeyOneDeliverToDate = docConfirm.EstimatedDeliveryTime;
				journeyTwoDeliverToDate = docConfirm.EstimatedDeliveryTime;
			}
		}

		void SetupFromDocCommonCartageLeg(DocCommonCartageLeg docContainerLeg)
		{
			journeyOnePickUpDate = docContainerLeg.PlannedPickupTime;
			journeyOneDeliverToDate = docContainerLeg.PlannedDeliveryTime;
			this.docCartage = docContainerLeg.Cartage;
			legNotes = docContainerLeg.LegNotes;
		}

		void SetupFromDocCommonCartage(DocCommonCartage docCartage)
		{
			this.docCartage = docCartage;
		}

		void SetupFromIDocCartageAdvice(IDocCartageAdvice parentInfo)
		{
			journeyOnePickUpDate = ZDateTime.Empty;
			journeyOneDeliverToDate = ZDateTime.Empty;
			journeyOnePickUpRequiredByDate = ZDateTime.Empty;
			journeyOneDeliverToRequiredByDate = ZDateTime.Empty;
			journeyTwoPickUpDate = ZDateTime.Empty;
			journeyTwoDeliverToDate = ZDateTime.Empty;
			journeyTwoPickUpRequiredByDate = ZDateTime.Empty;
			journeyTwoDeliverToRequiredByDate = ZDateTime.Empty;

			journeyOnePickUpDateHeading = Heading.DateHeading;
			journeyOneDeliverToDateHeading = Heading.DateHeading;
			journeyOnePickUpRequiredByDateHeading = Heading.Empty;
			journeyOneDeliverToRequiredByDateHeading = Heading.Empty;
			journeyTwoPickUpDateHeading = Heading.DateHeading;
			journeyTwoDeliverToDateHeading = Heading.DateHeading;
			journeyTwoPickUpRequiredByDateHeading = Heading.Empty;
			journeyTwoDeliverToRequiredByDateHeading = Heading.Empty;

			journeyOnePickUpReleaseNum = "";
			journeyOnePickUpSlofRef = "";
			journeyTwoDeliverToReleaseNum = "";
			journeyTwoDeliverToSlofRef = "";
		}

		readonly IDocCartageAdvice parentInfo;

		#region Empty

		// NOTE: If adding some additional properties to wrapper, don't forget to make EmptyWrapper && default constructor to return defaults for them
		class EmptyIDocCartageAdvice : IDocCartageAdvice
		{
			public ZString EmailSubjectNumber
			{
				get { return ""; }
			}

			public MultilingualString JourneyOnePickUpHeading
			{
				get { return (NoResString)""; }
			}

			public MultilingualString JourneyOneDeliverToHeading
			{
				get { return (NoResString)""; }
			}

			public MultilingualString JourneyTwoPickUpHeading
			{
				get { return (NoResString)""; }
			}

			public MultilingualString JourneyTwoDeliverToHeading
			{
				get { return (NoResString)""; }
			}

			#region Additional Headings

			public ZDateTime JourneyOnePickUpDate
			{
				get { return ZDateTime.Empty; }
			}

			public ZString JourneyOnePickUpDateHeading
			{
				get { return ZString.Empty; }
			}

			public ZDateTime JourneyOneDeliverToDate
			{
				get { return ZDateTime.Empty; }
			}

			public ZString JourneyOneDeliverToDateHeading
			{
				get { return ZString.Empty; }
			}

			public ZDateTime JourneyTwoPickUpDate
			{
				get { return ZDateTime.Empty; }
			}

			public ZString JourneyTwoPickUpDateHeading
			{
				get { return ZString.Empty; }
			}

			public ZDateTime JourneyTwoDeliverToDate
			{
				get { return ZDateTime.Empty; }
			}

			public ZString JourneyTwoDeliverToDateHeading
			{
				get { return ZString.Empty; }
			}

			public ZString JourneyOnePickUpSlofRef
			{
				get { return ""; }
			}

			public ZString JourneyOnePickUpReleaseNum
			{
				get { return ""; }
			}

			public ZString JourneyTwoDeliverToSlofRef
			{
				get { return ""; }
			}

			public ZString JourneyTwoDeliverToReleaseNum
			{
				get { return ""; }
			}

			#endregion

			public DocDocAddress JourneyOnePickUpAddress
			{
				get { return null; }
			}

			public DocDocAddress JourneyOneDeliverToAddress
			{
				get { return null; }
			}

			public DocDocAddress JourneyTwoPickUpAddress
			{
				get { return null; }
			}

			public DocDocAddress JourneyTwoDeliverToAddress
			{
				get { return null; }
			}

			public ZString JourneyOnePickUpContactName
			{
				get { return ""; }
			}

			public ZString JourneyOnePickUpContactPhone
			{
				get { return ""; }
			}

			public ZString JourneyOneDeliverToContactName
			{
				get { return ""; }
			}

			public ZString JourneyOneDeliverToContactPhone
			{
				get { return ""; }
			}

			public ZString JourneyTwoPickUpContactName
			{
				get { return ""; }
			}

			public ZString JourneyTwoPickUpContactPhone
			{
				get { return ""; }
			}

			public ZString JourneyTwoDeliverToContactName
			{
				get { return ""; }
			}

			public ZString JourneyTwoDeliverToContactPhone
			{
				get { return ""; }
			}

			public ZBool PrintAsContainers
			{
				get { return false; }
			}

			public ZBool PrintTwoJourneys
			{
				get { return false; }
			}

			public ZString EquipmentType
			{
				get { return ""; }
			}

			public ZString FullHandlingInstructions
			{
				get { return ""; }
			}

			public ZString FullCartageInstructions
			{
				get { return ""; }
			}

			public DocDocAddressCollection AddressesWithWareHousing
			{
				get { return null; }
			}

			public CartageAdviceHelper CartageAdvice
			{
				get { return null; }
			}

			public DocCompany CurrentCompany
			{
				get { return null; }
			}

			public ZBool IsAir
			{
				get { return false; }
			}

			public ZDateTime CartageCutOffDate
			{
				get { return ZDateTime.Empty; }
			}

			public ZDateTime CartageAvailableDate
			{
				get { return ZDateTime.Empty; }
			}

			public ZDateTime CutOffOrAvailableDate
			{
				get { return ZDateTime.Empty; }
			}

			public ZDateTime CartageReceivalDate
			{
				get { return ZDateTime.Empty; }
			}

			public ZDateTime CartageStorageCommenceDate
			{
				get { return ZDateTime.Empty; }
			}

			public ZString CutOffOrAvailableDateHeading
			{
				get { return ZString.Empty; }
			}

			public ZDateTime PickupOrStorageCommenceDate
			{
				get { return ZDateTime.Empty; }
			}

			public ZString PickupOrStorageCommenceDateHeading
			{
				get { return ZString.Empty; }
			}

			public ZDateTime DeliveryRequiredByDate
			{
				get { return ZDateTime.Empty; }
			}

			public ZString DeliveryRequiredByDateHeading
			{
				get { return ZString.Empty; }
			}

			public DocCommonCartage Cartage
			{
				get { return null; }
			}
		}

		#endregion

		#region Properties

		protected override ZString GetEmailSubjectNumber()
		{
			return parentInfo.EmailSubjectNumber;
		}

		protected override ZString GetJourneyOnePickUpDateHeading()
		{
			return GetHeadingText(journeyOnePickUpDateHeading);
		}
		Heading journeyOnePickUpDateHeading;

		protected override ZString GetJourneyOnePickUpRequiredByDateHeading()
		{
			return GetHeadingText(journeyOnePickUpRequiredByDateHeading);
		}
		Heading journeyOnePickUpRequiredByDateHeading;

		protected override ZString GetJourneyOneDeliverToDateHeading()
		{
			return GetHeadingText(journeyOneDeliverToDateHeading);
		}
		Heading journeyOneDeliverToDateHeading;

		protected override ZString GetJourneyOneDeliverToRequiredByDateHeading()
		{
			return GetHeadingText(journeyOneDeliverToRequiredByDateHeading);
		}
		Heading journeyOneDeliverToRequiredByDateHeading;

		protected override ZString GetJourneyTwoPickUpDateHeading()
		{
			return GetHeadingText(journeyTwoPickUpDateHeading);
		}
		Heading journeyTwoPickUpDateHeading;

		protected override ZString GetJourneyTwoPickUpRequiredByDateHeading()
		{
			return GetHeadingText(journeyTwoPickUpRequiredByDateHeading);
		}
		Heading journeyTwoPickUpRequiredByDateHeading;

		protected override ZString GetJourneyTwoDeliverToDateHeading()
		{
			return GetHeadingText(journeyTwoDeliverToDateHeading);
		}
		Heading journeyTwoDeliverToDateHeading;

		protected override ZString GetJourneyTwoDeliverToRequiredByDateHeading()
		{
			return GetHeadingText(journeyTwoDeliverToRequiredByDateHeading);
		}
		Heading journeyTwoDeliverToRequiredByDateHeading;

		protected override ZString GetJourneyOnePickUpHeading()
		{
			return StripAdditionalInfoFromHeading(parentInfo.JourneyOnePickUpHeading);
		}

		protected override ZString GetJourneyOneDeliverToHeading()
		{
			return StripAdditionalInfoFromHeading(parentInfo.JourneyOneDeliverToHeading);
		}

		protected override ZString GetJourneyTwoPickUpHeading()
		{
			return StripAdditionalInfoFromHeading(parentInfo.JourneyTwoPickUpHeading);
		}

		protected override ZString GetJourneyTwoDeliverToHeading()
		{
			return StripAdditionalInfoFromHeading(parentInfo.JourneyTwoDeliverToHeading);
		}

		#region Additional Headings

		protected override ZDateTime GetJourneyOnePickUpDate()
		{
			return journeyOnePickUpDate;
		}
		ZDateTime journeyOnePickUpDate;

		protected override ZDateTime GetJourneyOnePickUpRequiredByDate()
		{
			return journeyOnePickUpRequiredByDate;
		}
		ZDateTime journeyOnePickUpRequiredByDate;

		protected override ZDateTime GetJourneyOneDeliverToDate()
		{
			return journeyOneDeliverToDate;
		}
		ZDateTime journeyOneDeliverToDate;

		protected override ZDateTime GetJourneyOneDeliverToRequiredByDate()
		{
			return journeyOneDeliverToRequiredByDate;
		}
		ZDateTime journeyOneDeliverToRequiredByDate;

		protected override ZDateTime GetJourneyTwoPickUpDate()
		{
			return journeyTwoPickUpDate;
		}
		ZDateTime journeyTwoPickUpDate;

		protected override ZDateTime GetJourneyTwoPickUpRequiredByDate()
		{
			return journeyTwoPickUpRequiredByDate;
		}
		ZDateTime journeyTwoPickUpRequiredByDate;

		protected override ZDateTime GetJourneyTwoDeliverToDate()
		{
			return journeyTwoDeliverToDate;
		}
		ZDateTime journeyTwoDeliverToDate;

		protected override ZDateTime GetJourneyTwoDeliverToRequiredByDate()
		{
			return journeyTwoDeliverToRequiredByDate;
		}
		ZDateTime journeyTwoDeliverToRequiredByDate;

		protected override ZString GetJourneyOnePickUpSlofRef()
		{
			return journeyOnePickUpSlofRef;
		}
		ZString journeyOnePickUpSlofRef;

		protected override ZString GetJourneyOnePickUpReleaseNum()
		{
			return journeyOnePickUpReleaseNum;
		}
		ZString journeyOnePickUpReleaseNum;

		protected override ZString GetJourneyTwoDeliverToSlofRef()
		{
			return journeyTwoDeliverToSlofRef;
		}
		ZString journeyTwoDeliverToSlofRef;

		protected override ZString GetJourneyTwoDeliverToReleaseNum()
		{
			return journeyTwoDeliverToReleaseNum;
		}
		ZString journeyTwoDeliverToReleaseNum;

		#endregion

		protected override AddressWrapperWithIDocDocAddress GetJourneyOnePickUpAddress()
		{
			return GetLocalTransportAddress(parentInfo.JourneyOnePickUpAddress);
		}

		protected override AddressWrapperWithIDocDocAddress GetJourneyOneDeliverToAddress()
		{
			return GetLocalTransportAddress(parentInfo.JourneyOneDeliverToAddress);
		}

		protected override AddressWrapperWithIDocDocAddress GetJourneyTwoPickUpAddress()
		{
			return GetLocalTransportAddress(parentInfo.JourneyTwoPickUpAddress);
		}

		protected override AddressWrapperWithIDocDocAddress GetJourneyTwoDeliverToAddress()
		{
			return GetLocalTransportAddress(parentInfo.JourneyTwoDeliverToAddress);
		}

		protected override ZString GetJourneyOnePickUpContactName()
		{
			return parentInfo.JourneyOnePickUpContactName;
		}

		protected override ZString GetJourneyOnePickUpContactPhone()
		{
			return parentInfo.JourneyOnePickUpContactPhone;
		}

		protected override ZString GetJourneyOneDeliverToContactName()
		{
			return parentInfo.JourneyOneDeliverToContactName;
		}

		protected override ZString GetJourneyOneDeliverToContactPhone()
		{
			return parentInfo.JourneyOneDeliverToContactPhone;
		}

		protected override ZString GetJourneyTwoPickUpContactName()
		{
			return parentInfo.JourneyTwoPickUpContactName;
		}

		protected override ZString GetJourneyTwoPickUpContactPhone()
		{
			return parentInfo.JourneyTwoPickUpContactPhone;
		}

		protected override ZString GetJourneyTwoDeliverToContactName()
		{
			return parentInfo.JourneyTwoDeliverToContactName;
		}

		protected override ZString GetJourneyTwoDeliverToContactPhone()
		{
			return parentInfo.JourneyTwoDeliverToContactPhone;
		}

		protected override AddressWrapperWithIDocDocAddressCollection GetAddressesWithWareHousing()
		{
			if (addressesWithWarehousing == null)
			{
				addressesWithWarehousing = new AddressWrapperWithIDocDocAddressCollection(Factory);

				if (parentInfo.AddressesWithWareHousing != null)
				{
					foreach (DocDocAddress docDocAddress in parentInfo.AddressesWithWareHousing)
					{
						addressesWithWarehousing.Add(new AddressWrapperWithIDocDocAddress(docDocAddress.WrappedObject as JobDocAddress, Factory));
					}
				}
			}

			return addressesWithWarehousing;
		}
		AddressWrapperWithIDocDocAddressCollection addressesWithWarehousing;

		protected override ZBool GetPrintAsContainers()
		{
			return parentInfo.PrintAsContainers;
		}

		protected override ZBool GetPrintTwoJourneys()
		{
			return parentInfo.PrintTwoJourneys;
		}

		protected override ZBool GetPrintJourneyOne()
		{
			return printJourneyOne;
		}
		ZBool printJourneyOne = true;

		protected override ZBool GetPrintJourneyTwo()
		{
			return printJourneyTwo || PrintTwoJourneys;
		}
		ZBool printJourneyTwo = false;

		protected override ZString GetEquipmentType()
		{
			return parentInfo.EquipmentType;
		}

		protected override ZString GetFullHandlingInstructions()
		{
			return parentInfo.FullHandlingInstructions;
		}

		protected override ZString GetFullCartageInstructions()
		{
			return parentInfo.FullCartageInstructions;
		}

		protected override ZString GetLegNotes()
		{
			return legNotes;
		}
		ZString legNotes;

		protected override CartageAdviceHelper GetCartageAdvice()
		{
			return parentInfo.CartageAdvice;
		}

		protected override ZBool GetIsAir()
		{
			return parentInfo.IsAir;
		}

		protected override ZDateTime GetCutOffDate()
		{
			return parentInfo.CartageCutOffDate;
		}

		protected override ZDateTime GetAvailableDate()
		{
			return parentInfo.CartageAvailableDate;
		}

		protected override ZDateTime GetCutOffOrAvailableDate()
		{
			return parentInfo.CutOffOrAvailableDate;
		}

		protected override ZDateTime GetReceivalDate()
		{
			return parentInfo.CartageReceivalDate;
		}

		protected override ZDateTime GetStorageCommenceDate()
		{
			return parentInfo.CartageStorageCommenceDate;
		}

		protected override ZDateTime GetPickupOrStorageCommenceDate()
		{
			return parentInfo.PickupOrStorageCommenceDate;
		}

		protected override ZString GetPickupOrStorageCommenceDateHeading()
		{
			return parentInfo.PickupOrStorageCommenceDateHeading;
		}

		protected override DocCommonCartage GetCartage()
		{
			return docCartage;
		}
		DocCommonCartage docCartage;

		ZString StripAdditionalInfoFromHeading(MultilingualString source)
		{
			if (!source.IsEmpty)
			{
				MultilingualString[] correctHeadings = new MultilingualString[] {
					ResString.GetMultilingualString("1ea3d5bc-b76e-4e24-b9cc-864f77319820", "PICKUP FROM"),
					ResString.GetMultilingualString("5216f84d-6dd4-47c5-81c8-98a40c079293", "PICKUP EMPTY"),
					ResString.GetMultilingualString("0ff20798-dd73-4763-973a-6747dd980262", "PICKUP FULL"),
					ResString.GetMultilingualString("2b2aba77-23ca-4578-8d68-4ca8f200942a", "PICKUP"),
					ResString.GetMultilingualString("f112b6df-ac2a-4008-9a68-3c375fa22bfc", "DELIVER TO EMPTY"),
					ResString.GetMultilingualString("2865908e-e2bd-49a6-af61-c54dbe9142e2", "DELIVER FULL"),
					ResString.GetMultilingualString("10f6f2c0-6e8a-4692-942b-484821afa3e8", "DELIVER TO FULL"),
					ResString.GetMultilingualString("e10ae32e-7c1b-4e9c-9efc-75c285e602fd", "DELIVER TO"),
				};

				foreach (var heading in correctHeadings)
				{
					if (source.GetUnresolvedString().StartsWith(heading.GetUnresolvedString()))
					{
						return heading;
					}
				}
			}

			return ZString.Empty;
		}

		#endregion
	}
}
