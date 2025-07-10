using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.IN.Manifest.Business;

sealed class CGMAsycudaPackValidation : AsycudaPackValidation
{
	public CGMAsycudaPackValidation(AsycudaPack parent) : base(parent)
	{
	}

	protected override void CheckAPA_PackQty()
	{
		base.CheckAPA_PackQty();
		var parent = Parent;
		var bill = parent.Bill;
		if (bill != null && bill.Header?.Containers.Count == 1 && parent.APA_PackQty != bill.ABL_ManifestQty)
		{
			parent.APA_PackQtyInfo.AddMessageError(Res.GetString("9EABFA43-700F-4301-ABF4-6802605B40E3", "Entered quantity is not equal to Packages specified at Bills level."));
		}
	}

	protected override void CheckAPA_Weight()
	{
		base.CheckAPA_Weight();
		var parent = Parent;
		var bill = parent.Bill;
		if (bill != null && bill.Header?.Containers.Count == 1 && parent.APA_Weight != bill.ABL_GrossWeight)
		{
			parent.APA_WeightInfo.AddMessageError(Res.GetString("4C507406-B255-4314-8CF3-3D11EF74422A", "Entered weight is not equal to gross weight specified at Bills level."));
		}
	}

	protected override void CheckContainerPK()
	{
		base.CheckContainerPK();
		var parent = Parent;
		var bill = parent.Bill;
		if (parent.Container is AsycudaContainer container && bill is not null)
		{
			var containerPKInfo = parent.ContainerPKInfo;
			CheckContainerPKDuplicateInSameBill(container, bill, containerPKInfo);
			CheckContainerPKDuplicateInOtherBillFullContainer(container, bill, containerPKInfo);
		}
	}

	void CheckContainerPKDuplicateInSameBill(AsycudaContainer container, ASYCUDA.Business.AsycudaBill bill, ZPropertyInfo containerPKInfo)
	{
		if (bill.Packs.Cast<CGMAsycudaPack>().Count(x => x.ContainerPK == container.PK) > 1)
		{
			containerPKInfo.AddError(Res.GetString("33B6C0D9-47C9-4C75-9D20-EB737C52BD66", "This container is already present in the grid for the current house bill."));
		}
	}

	void CheckContainerPKDuplicateInOtherBillFullContainer(AsycudaContainer container, ASYCUDA.Business.AsycudaBill bill, ZPropertyInfo containerPKInfo)
	{
		if (container.ACN_EmptyFullIndicator == EmptyFullIndicatorList.Codes.FullContainerLoad)
		{
			var billWithDuplicateFCLContainer = bill.Header?.Bills.FirstOrDefault(x => x.PK != bill.PK && x.Packs.Cast<CGMAsycudaPack>().Any(x => x.ContainerPK == container.PK));
			if (billWithDuplicateFCLContainer is not null)
			{
				containerPKInfo.AddError(Res.GetString("E903BFF4-B385-4033-9BBC-B30089EA1B69", "This FCL container is already selected in house bill number '{0}'", billWithDuplicateFCLContainer.ABL_BillNumber));
			}
		}
	}
}
