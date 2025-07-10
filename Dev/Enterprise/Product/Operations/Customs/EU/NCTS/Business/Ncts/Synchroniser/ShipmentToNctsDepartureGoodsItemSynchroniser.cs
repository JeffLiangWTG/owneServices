using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class ShipmentToNctsDepartureGoodsItemSynchroniser : BusinessObjectSynchroniser
	{
		public ShipmentToNctsDepartureGoodsItemSynchroniser(NctsDepartureCargoDesc destination, ForwardingShipment source
					, bool hasCommonCountryOfDispatch, bool hasCommonCountryOfDestination, bool hasCommonConsignor, bool hasCommonConsignee, bool hasCommonCTStatus)
			: base(destination, source)
		{
			this.hasCommonCountryOfDispatch = hasCommonCountryOfDispatch;
			this.hasCommonCountryOfDestination = hasCommonCountryOfDestination;
			this.hasCommonConsignor = hasCommonConsignor;
			this.hasCommonConsignee = hasCommonConsignee;
			this.hasCommonCTStatus = hasCommonCTStatus;
			nctsHeader = Destination?.MoveHeader?.Header;
		}

		protected override void HookSynchronisers()
		{
			if (nctsHeader != null)
			{
				AddConsignorDocumentaryAddressSynchroniser();
				AddConsigneeDocumentaryAddressSynchroniser();
				AddImportLoadPortFieldSynchroniser();
				AddDestinationPortFieldSynchroniser();
				Synchronisers.Add(new FieldSynchroniser(Destination.BY_RN_NKCountryOfOriginInfo, Source.JS_RL_NKOriginInfo));

				if (Destination.Lookups.DeclarationTypeList.ContainsCode(Source.JS_CommunityTransitStatus))
				{
					Synchronisers.Add(new FieldSynchroniser(GetDestinationDeclarationType(), Source.JS_CommunityTransitStatusInfo));
				}
			}
			else
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.BY_RN_NKCountryOfDestinationInfo, Source.JS_RL_NKDestinationInfo));
			}

			Synchronisers.Add(new FieldSynchroniser(Destination.BY_DescriptionInfo, Source.JS_GoodsDescriptionInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.BY_GrossWeightInfo, Source.JS_ActualWeightInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.BY_GrossWeightUnitInfo, Source.JS_UnitOfWeightInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.BY_CommercialReferenceNumberInfo, Source.JS_UniqueConsignRefInfo));
			Synchronisers.Add(new NctsDeparturePackagesCollectionSynchroniser(Destination, Source));
		}

		void AddConsignorDocumentaryAddressSynchroniser()
		{
			if (!nctsHeader.DefaultConsignorConsigneeRegistryManager.IsRegistryEnabled())
			{
				Synchronisers.Add(new JobDocAddressSynchroniser(GetDestinationConsignor(), Source.ConsignorDocumentaryAddress));
			}
			else
			{
				if (nctsHeader.DefaultConsignorConsigneeRegistryManager.IsConsignor() && Source.JobDirection == Directions.Export)
				{
					Synchronisers.Add(new JobDocAddressAddressOnlySynchroniser(GetDestinationConsignor(), (ZPropertyInfoGuid)Source.JS_OA_ExportReceivingDepotInfo));
				}
			}
		}

		void AddConsigneeDocumentaryAddressSynchroniser()
		{
			if (!nctsHeader.DefaultConsignorConsigneeRegistryManager.IsRegistryEnabled())
			{
				Synchronisers.Add(new JobDocAddressSynchroniser(GetDestinationConsignee(), Source.ConsigneeDocumentaryAddress));
			}
			else
			{
				if (nctsHeader.DefaultConsignorConsigneeRegistryManager.IsConsignee() && Source.JobDirection == Directions.Import)
				{
					Synchronisers.Add(new JobDocAddressSynchroniser(GetDestinationConsignee(), Source.ConsigneeDeliveryAddress));
				}
			}
		}

		JobDocAddress GetDestinationConsignor()
		{
			return hasCommonConsignor ? nctsHeader.Consignor : Destination.Consignor;
		}

		JobDocAddress GetDestinationConsignee()
		{
			return hasCommonConsignee ? nctsHeader.Consignee : Destination.Consignee;
		}

		ZPropertyInfo GetDestinationCountryOfDispatch()
		{
			return hasCommonCountryOfDispatch ? nctsHeader.BH_RL_NKImportLoadPortInfo : Destination.BY_RN_NKCountryOfDispatchInfo;
		}

		protected virtual void AddImportLoadPortFieldSynchroniser()
		{
			Synchronisers.Add(new FieldSynchroniser(GetDestinationCountryOfDispatch(), Source.JS_RL_NKOriginInfo));
		}

		ZPropertyInfo GetDestinationCountryOfDestination()
		{
			return hasCommonCountryOfDestination && nctsHeader.IsDepartureMovement ? nctsHeader.MovementHeader.BM_RL_NKDestinationPortInfo : Destination.BY_RN_NKCountryOfDestinationInfo;
		}

		protected virtual void AddDestinationPortFieldSynchroniser()
		{
			Synchronisers.Add(new FieldSynchroniser(GetDestinationCountryOfDestination(), Source.JS_RL_NKDestinationInfo));
		}

		ZPropertyInfo GetDestinationDeclarationType()
		{
			return (nctsHeader.IsPluggedIntoConsol && hasCommonCTStatus || nctsHeader.IsPluggedIntoShipment) && nctsHeader.IsDepartureMovement ? nctsHeader.MovementHeader.BM_InBondEntryTypeInfo : Destination.BY_TypeInfo;
		}

		protected override void OnSynchronised()
		{
			base.OnSynchronised();
			if (IsEnabled)
			{
				foreach (NonPersistentDepartureContainerPivot pivot in Destination.ContainersPivots)
				{
					pivot.ContainerSelected = true;  // Tick the "is for line" box - we ony bring over from the shipment/consol onto the header the containers that are relevant to the shipment, and we're making one line, so all containers need to be ticked
				}
			}
		}

		protected new NctsDepartureCargoDesc Destination => (NctsDepartureCargoDesc)base.Destination;

		protected new ForwardingShipment Source => (ForwardingShipment)base.Source;

		readonly NctsHeader nctsHeader;
		readonly bool hasCommonCountryOfDispatch;
		readonly bool hasCommonCountryOfDestination;
		readonly bool hasCommonConsignor;
		readonly bool hasCommonConsignee;
		readonly bool hasCommonCTStatus;
	}
}
