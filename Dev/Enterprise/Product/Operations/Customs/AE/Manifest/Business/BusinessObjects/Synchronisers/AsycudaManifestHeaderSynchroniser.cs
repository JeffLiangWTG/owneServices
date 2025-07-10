using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AE.Manifest.Business;

public class AsycudaManifestHeaderSynchroniser : ASYCUDA.Business.AsycudaManifestHeaderSynchroniser
{
	public AsycudaManifestHeaderSynchroniser(AsycudaManifestHeader destination, ForwardingConsol sourceConsol) : base(destination, sourceConsol)
	{
	}

	protected new AsycudaManifestHeader Destination => (AsycudaManifestHeader)base.Destination;

	protected override BusinessObjectCollectionSynchroniser GetNewBillCollectionSynchroniser()
	{
		return new AsycudaBillCollectionSynchroniser(Destination);
	}

	protected override void HookSynchronisers()
	{
		base.HookSynchronisers();

		Synchronisers.Add(new FieldSynchroniser(Destination.AMA_CustomsOriginPortInfo, Source.JK_RL_NKLastForeignPortInfo));
		Synchronisers.Add(new FieldSynchroniser(Destination.AMA_MasterBillInfo, GetConvertedSourceMasterBill, GetSourceInfosAffectingConvertedSourceMasterBill));
		Synchronisers.Add(new FieldSynchroniser(Destination.AMA_OA_CarrierInfo, GetConvertedSourceCarrier, GetSourceInfosAffectingConvertedSourceCarrier));
	}

	IEnumerable<ZPropertyInfo> GetSourceInfosAffectingConvertedSourceMasterBill()
	{
		yield return Source.JK_AgentTypeInfo;
		yield return Source.JK_CoLoadMasterBillInfo;
		yield return Source.JK_MasterBillNumInfo;
	}

	IEnumerable<ZPropertyInfo> GetSourceInfosAffectingConvertedSourceCarrier()
	{
		yield return Source.JK_AgentTypeInfo;
		yield return Source.JK_OA_CreditorAddressInfo;
		yield return Source.JK_OA_ShippingLineAddressInfo;
	}

	IZType GetConvertedSourceMasterBill()
	{
		return Source.JK_AgentType == Core.Constants.AgentType.CoLoad ? Source.JK_CoLoadMasterBill : Source.JK_MasterBillNum; 
	}

	IZType GetConvertedSourceCarrier()
	{
		return Source.JK_AgentType == Core.Constants.AgentType.CoLoad ? Source.JK_OA_CreditorAddress : Source.JK_OA_ShippingLineAddress;
	}
}
