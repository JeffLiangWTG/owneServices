using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.DocumentWrappers;
using Enterprise.Customs.IN.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IN.Manifest.Business;

sealed class CGMSeaAsycudaManifestHeaderDocWrapper : ASYCUDA.Business.AsycudaManifestHeaderDocWrapper
{
	public CGMSeaAsycudaManifestHeaderDocWrapper(CGMAsycudaManifestHeader manifestHeader) : base(manifestHeader)
	{
	}

	public new CGMAsycudaManifestHeader Manifest => (CGMAsycudaManifestHeader)base.Manifest;

	public ZString CARNNumber => INCustomsDataRegistry.Instance.INConsolAgentRegistrationNumber.Value;

	public ZString ConsolAgentName => GlbCompany.CurrentCompany.OrgProxy.OH_FullName;

	public ZString JobReference => Manifest.AMA_JobReference;

	public ZString CustomHouse => Manifest.AMA_CustomsOffice;

	public ZString IGMNumber => Manifest.ImportGeneralManifestNumber;

	public ZDate IGMDate => Manifest.ImportGeneralManifestDate;

	public ZString VesselCode => Manifest.AMA_VesselName;

	public ZString IMOCode => Manifest.AMA_LloydsNumber;

	public ZString VoyageNumber => Manifest.AMA_Voyage;

	public ZString ShippingLineName => Manifest.Carrier?.CompanyName ?? ZString.Empty;

	public ZString LineNumber => Manifest.MasterBill.ABL_CarrierReference;

	public ZString MBLNumber => Manifest.AMA_MasterBill;

	public ZDate MBLDate => Manifest.AMA_MasterBillIssueDate;

	public BusinessObjectCollectionWrapper<CGMSeaAsycudaBillDocWrapper> BillsWrapper
	{
		get
		{
			if (billsWrapper == null)
			{
				var asycudaBillDocWrappers = Bills.Select(bill => new CGMSeaAsycudaBillDocWrapper(bill));
				billsWrapper = new BusinessObjectCollectionWrapper<CGMSeaAsycudaBillDocWrapper>(asycudaBillDocWrappers);
			}
			return billsWrapper;
		}
	}
	BusinessObjectCollectionWrapper<CGMSeaAsycudaBillDocWrapper> billsWrapper;

	public BusinessObjectCollectionWrapper<CGMAsycudaContainerDocWrapper> ContainersWrapper
	{
		get
		{
			if (containersWrapper == null)
			{
				var asycudaBillDocWrappers = Manifest.PacksWithContainer.Select(pack => new CGMAsycudaContainerDocWrapper(pack));
				containersWrapper = new BusinessObjectCollectionWrapper<CGMAsycudaContainerDocWrapper>(asycudaBillDocWrappers);
			}
			return containersWrapper;
		}
	}
	BusinessObjectCollectionWrapper<CGMAsycudaContainerDocWrapper> containersWrapper;

	IEnumerable<CGMAsycudaBill> Bills => Manifest.Bills.Cast<CGMAsycudaBill>().OrderBy(x => x.ABL_SequenceNumber);
}
