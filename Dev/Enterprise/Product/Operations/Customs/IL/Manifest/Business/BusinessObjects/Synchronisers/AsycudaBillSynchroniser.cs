using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IL.Business;
using Enterprise.Freight.Forwarding.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaBillSynchroniser : ASYCUDA.Business.AsycudaBillSynchroniser
	{
		public AsycudaBillSynchroniser(ASYCUDA.Business.AsycudaBill destination, ForwardingShipment shipmentSource)
			: base(destination, shipmentSource)
		{
		}

		public new AsycudaBill Destination => (AsycudaBill)base.Destination;

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.ABL_ConditionInfo, GetHBLContainerPackModeOverride, GetInfosAffectingHBLContainerPackModeOverride));
			Destination.Header.AMA_TransportModeInfo.ValueChanged += HandleTransportModeChanged;

			HandleTransportModeChanged(null, null);
		}

		protected override void UnHookSynchronisers()
		{
			base.UnHookSynchronisers();
			Destination.Header.AMA_TransportModeInfo.ValueChanged -= HandleTransportModeChanged;
		}

		void HandleTransportModeChanged(object sender, EventArgs e)
		{
			if (Destination.Header.Consol is not null && Destination.Header.AMA_TransportMode == TransportModes.Road)
			{
				var additionalInfoToSync = Destination.AdditionalInfos.FirstOrDefault(ai => ai.CSI_Code == Constants.AsycudaAdditionalInfoCodes.PackageFeeType);

				if (additionalInfoToSync is null)
				{
					additionalInfoToSync = Destination.AdditionalInfos.AddNew();
					additionalInfoToSync.CSI_Code = Constants.AsycudaAdditionalInfoCodes.PackageFeeType;
				}

				var synchronizer = new AsycudaAdditionalInfoSynchronizer(additionalInfoToSync, Destination.Header.Consol);
				synchronizer.SetEnabled(IsEnabled, DetectEnabled);
				Synchronisers.Add(synchronizer);
			}
			else
			{
				var additionalInfoToSync = Destination.AdditionalInfos.FirstOrDefault(ai => ai.CSI_Code == Constants.AsycudaAdditionalInfoCodes.PackageFeeType);

				if (additionalInfoToSync is not null)
				{
					var synchronizer = Synchronisers.FirstOrDefault(s => s is AsycudaAdditionalInfoSynchronizer additionalInfoSynchronizer && additionalInfoSynchronizer.Destination == additionalInfoToSync);
					Synchronisers.Remove(synchronizer);
					Destination.AdditionalInfos.RemoveAndDelete(additionalInfoToSync);
				}
			}
		}

		protected override ASYCUDA.Business.AsycudaTransportDocumentCollectionSynchroniser GetAsycudaTransportDocumentCollectionSynchroniser() => new AsycudaTransportDocumentCollectionSynchroniser(consolSource, Destination);

		protected override IZType GetSourceShipmentType()
		{
			return new ZString(AsycudaBillKindList.Codes.HWB);
		}

		IZType GetHBLContainerPackModeOverride()
			=> (string)Source.JS_HBLContainerPackModeOverride switch
			{
				HBLDeliveryModes.Codes.CFS_CFS => new ZString(ILBillConditionList.Codes.PierToPier),
				HBLDeliveryModes.Codes.CY_CY => new ZString(ILBillConditionList.Codes.PierToPier),
				HBLDeliveryModes.Codes.CFS_CY => new ZString(ILBillConditionList.Codes.PierToPier),
				HBLDeliveryModes.Codes.CY_CFS => new ZString(ILBillConditionList.Codes.PierToPier),

				HBLDeliveryModes.Codes.CFS_DOOR => new ZString(ILBillConditionList.Codes.PierToDoor),
				HBLDeliveryModes.Codes.CY_DOOR => new ZString(ILBillConditionList.Codes.PierToDoor),

				HBLDeliveryModes.Codes.DOOR_CFS => new ZString(ILBillConditionList.Codes.DoorToPier),
				HBLDeliveryModes.Codes.DOOR_CY => new ZString(ILBillConditionList.Codes.DoorToPier),

				HBLDeliveryModes.Codes.DOOR_DOOR => new ZString(ILBillConditionList.Codes.DoorToDoor),
				_ => null,
			};

		IEnumerable<ZPropertyInfo> GetInfosAffectingHBLContainerPackModeOverride()
		{
			yield return this.Source.JS_HBLContainerPackModeOverrideInfo;
		}

		protected override ASYCUDA.Business.AsycudaPackCollectionSynchroniser GetAsycudaPackCollectionSynchroniser()
			=> new AsycudaPackCollectionSynchroniser(Source, Destination);
	}
}
