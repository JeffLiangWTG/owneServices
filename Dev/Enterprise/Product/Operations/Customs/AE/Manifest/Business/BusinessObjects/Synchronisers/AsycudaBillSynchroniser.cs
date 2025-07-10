using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AE.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.AE.Manifest.Business;

public class AsycudaBillSynchroniser : ASYCUDA.Business.AsycudaBillSynchroniser
{
	public AsycudaBillSynchroniser(ASYCUDA.Business.AsycudaBill destination, Freight.Forwarding.Business.ForwardingShipment shipmentSource) : base(destination, shipmentSource)
	{
	}

	protected new AsycudaBill Destination => (AsycudaBill)base.Destination;

	protected override void HookSynchronisers()
	{
		base.HookSynchronisers();

		Synchronisers.Add(new FieldSynchroniser(Destination.ABL_CargoTypeInfo, GetConsolMode, GetConsolModeInfo));
		Synchronisers.Add(new FieldSynchroniser(Destination.ABL_OA_ContainerAgentInfo, consolSource.JK_OA_SendingForwarderAddressInfo));
		Synchronisers.Add(new FieldSynchroniser(Destination.ABL_BolTypeInfo, GetConsolType, GetConsolTypeInfo));
		Synchronisers.Add(new FieldSynchroniser(Destination.ABL_SpecialCargoCodeInfo, GetContainerMode, GetContainerModeInfo));
	}

	IEnumerable<ZPropertyInfo> GetConsolModeInfo()
	{
		yield return consolSource.JK_ConsolModeInfo;
	}

	IZType GetConsolMode()
	{
		var consolMode = consolSource.JK_ConsolMode;
		return MapCargoType(consolMode);
	}

	ZString MapCargoType(ZString cargoType)
	{
		var map = ZZRefCusMapCombined.GetCW1CodeToCustomsCodeMapping(consolSource.Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.GulfCooperationCouncil, Core.Constants.Customs.Universal.RefCusMaps.CargoTypes, ZDateTime.Now);

		if (map.TryGetValue(cargoType, out var cw1Code))
		{
			return cw1Code;
		}

		return ZString.Empty;
	}

	IZType GetConsolType()
	{
		return new ZString(consolSource.JK_AgentType == Core.Constants.ShipmentTypes.CoLoadMaster ? Core.Constants.ShipmentTypes.CoLoadMaster : Core.Constants.ShipmentTypes.StandardHouse);
	}

	IEnumerable<ZPropertyInfo> GetConsolTypeInfo()
	{
		yield return consolSource.JK_AgentTypeInfo;
	}

	IZType GetContainerMode()
	{
		var containerMode = (string)Source.JS_PackingMode;
		var result = ZString.Empty;
		switch (containerMode)
		{
			case Core.Constants.ContainerModes.LCL:
			case Core.Constants.ContainerModes.BuyersConsol:
				result = AEConstants.RefCusCodeList.ServiceRequirementCode.LessThanFullLoads;
				break;
			case Core.Constants.ContainerModes.FCL:
				result = AEConstants.RefCusCodeList.ServiceRequirementCode.FullLoads;
				break;
		}
		return result;
	}

	IEnumerable<ZPropertyInfo> GetContainerModeInfo()
	{
		yield return Source.JS_PackingModeInfo;
	}
}
