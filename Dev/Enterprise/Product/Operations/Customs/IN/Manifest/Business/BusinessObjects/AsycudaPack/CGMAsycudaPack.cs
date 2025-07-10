using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IN.Manifest.Business;

public sealed class CGMAsycudaPack : ASYCUDA.Business.AsycudaPack, Integration.Customs.ASYCUDA.INManifest.ICGMAsycudaPack
{
	public CGMAsycudaPack(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : ASYCUDA.Business.AsycudaPack.Schema
	{
		public const string ContainerStatus = "Container+ACN_EmptyFullIndicator";
		public const string SealNo = "Container+ACN_Seal1";
		public const string ISOCode = "Container+ISOCode";
		public const string SOCFlag = "Container+ACN_IsShipperOwned";
		public const string ContainerAgentPAN = "Container+ContainerAgentPAN";
	}

	public new CGMAsycudaContainer Container => (CGMAsycudaContainer)base.Container;

	public new CGMAsycudaBill Bill => (CGMAsycudaBill)base.Bill;

	[ReadOnly(true)]
	public override ZString APA_WeightUQ => base.APA_WeightUQ;

	public override ZGuid ContainerPK
	{
		get => base.ContainerPK;
		set
		{
			var oldValue = ContainerPK;
			base.ContainerPK = value;
			if (!IsCopying && oldValue != ContainerPK)
			{
				SetQuantityAndWeight();
			}
		}
	}

	protected override ManifestBase.AsycudaPackValidation GetNewValidation() => new CGMAsycudaPackValidation(this);

	void SetQuantityAndWeight()
	{
		var bill = Bill;
		if (bill is not null
			&& ContainerPK.IsValid
			&& bill.Header?.Containers.Count == 1)
		{
			if (APA_PackQty.IsEmpty)
			{
				APA_PackQty = bill.ABL_ManifestQty;
			}
			if (APA_Weight.IsEmpty)
			{
				APA_Weight = bill.ABL_GrossWeight;
			}
		}
	}
}
