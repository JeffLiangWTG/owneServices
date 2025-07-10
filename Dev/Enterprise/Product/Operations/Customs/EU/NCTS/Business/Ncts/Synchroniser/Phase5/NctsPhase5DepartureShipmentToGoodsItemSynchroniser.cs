using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsPhase5DepartureShipmentToGoodsItemSynchroniser : BusinessObjectSynchroniser
	{
		public NctsPhase5DepartureShipmentToGoodsItemSynchroniser(NctsDepartureCargoDesc destination, ForwardingShipment source, ForwardingPackLine distinctSourcePackage
					, bool hasCommonCountryOfDispatch, bool hasCommonCountryOfDestination, bool hasCommonCTStatus, bool isConsol)
			: base(destination, source)
		{
			this.distinctSourcePackage = distinctSourcePackage;
			this.hasCommonCountryOfDispatch = hasCommonCountryOfDispatch;
			this.hasCommonCountryOfDestination = hasCommonCountryOfDestination;
			this.hasCommonCTStatus = hasCommonCTStatus;
			this.nctsHeader = Destination?.MoveHeader?.Header;
			this.isConsol = isConsol;
			isPhase5TransitionPeriod = nctsHeader?.IsInPhase5TransitionPeriod ?? false;
		}

		protected override void HookSynchronisers()
		{
			if (nctsHeader != null)
			{
				AddImportLoadPortFieldSynchroniser();
				AddDestinationPortFieldSynchroniser();

				if (Destination.Lookups.DeclarationTypeList.ContainsCode(Source.JS_CommunityTransitStatus))
				{
					Synchronisers.Add(new FieldSynchroniser(GetDestinationDeclarationType(), Source.JS_CommunityTransitStatusInfo));
				}
			}

			if (!isConsol || isPhase5TransitionPeriod)
			{
				Synchronisers.Add(new FieldSynchroniser(GetDestinationCountry(), GetSourceCountry()));
			}

			if (distinctSourcePackage != null)
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.BY_DescriptionInfo, distinctSourcePackage.JL_DescriptionInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.BY_FormattedHarmonisedTariffInfo, distinctSourcePackage.JL_HarmonisedCodeInfo));
				Synchronisers.Add(new NctsPhase5DeparturePackageCollectionSynchroniser(Destination, Source, GetSourcePackLines()));
			}
			else
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.BY_DescriptionInfo, Source.JS_GoodsDescriptionInfo));
				Synchronisers.Add(new NctsPhase5DeparturePackageCollectionSynchroniser(Destination, Source, null));
			}
		}

		ZPropertyInfo GetSourceCountry()
		{
			return distinctSourcePackage == null ? Source.JS_RL_NKDestinationInfo : distinctSourcePackage.JL_RN_NKOriginInfo;
		}

		ZPropertyInfo GetDestinationCountry()
		{
			return nctsHeader == null ? Destination.BY_RN_NKCountryOfDestinationInfo : Destination.BY_RN_NKCountryOfOriginInfo;
		}

		ForwardingPackLine[] GetSourcePackLines() => distinctSourcePackage.JL_HarmonisedCode.IsEmpty || distinctSourcePackage.JL_RN_NKOrigin.IsEmpty || distinctSourcePackage.JL_Description.IsEmpty
			? new[] { distinctSourcePackage }
			: Source.OuterPackLines.Cast<ForwardingPackLine>().Where(x => x.JL_HarmonisedCode.Equals(distinctSourcePackage.JL_HarmonisedCode) && x.JL_RN_NKOrigin.Equals(distinctSourcePackage.JL_RN_NKOrigin) && x.JL_Description.Equals(distinctSourcePackage.JL_Description)).ToArray();

		ZPropertyInfo GetDestinationCountryOfDispatch()
		{
			ZPropertyInfo destinationCountryOfDispatchProperty = null;
			if (isConsol && !isPhase5TransitionPeriod)
			{
				destinationCountryOfDispatchProperty = hasCommonCountryOfDispatch ? nctsHeader.MovementHeader.BM_RN_NKCountryOfDispatchInfo : Destination.Bill.B0_RN_NKCountryOfExportInfo;
			}
			else
			{
				destinationCountryOfDispatchProperty = hasCommonCountryOfDispatch ? nctsHeader.MovementHeader.BM_RN_NKCountryOfDispatchInfo : Destination.BY_RN_NKCountryOfDispatchInfo;
			}

			return destinationCountryOfDispatchProperty;
		}

		protected virtual void AddImportLoadPortFieldSynchroniser()
		{
			Synchronisers.Add(new FieldSynchroniser(GetDestinationCountryOfDispatch(), Source.JS_RL_NKOriginInfo));
		}

		ZPropertyInfo GetDestinationCountryOfDestination()
		{
			ZPropertyInfo destinationCountryOfDestinationProperty = null;
			if (isConsol && !isPhase5TransitionPeriod)
			{
				destinationCountryOfDestinationProperty = hasCommonCountryOfDestination ? nctsHeader.MovementHeader.BM_RL_NKDestinationPortInfo : Destination.Bill.B0_RN_NKCountryOfDestinationInfo;
			}
			else
			{
				destinationCountryOfDestinationProperty = hasCommonCountryOfDestination && nctsHeader.IsDepartureMovement ? nctsHeader.MovementHeader.BM_RL_NKDestinationPortInfo : Destination.BY_RN_NKCountryOfDestinationInfo;
			}

			return destinationCountryOfDestinationProperty;
		}

		protected virtual void AddDestinationPortFieldSynchroniser()
		{
			Synchronisers.Add(new FieldSynchroniser(GetDestinationCountryOfDestination(), Source.JS_RL_NKDestinationInfo));
		}

		ZPropertyInfo GetDestinationDeclarationType()
		{
			return ((nctsHeader.IsPluggedIntoConsol && hasCommonCTStatus) || nctsHeader.IsPluggedIntoShipment) && nctsHeader.IsDepartureMovement ? nctsHeader.MovementHeader.BM_InBondEntryTypeInfo : Destination.BY_TypeInfo;
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
		readonly bool hasCommonCTStatus;
		readonly bool isConsol;
		readonly ForwardingPackLine distinctSourcePackage;
		readonly bool isPhase5TransitionPeriod;
	}
}
