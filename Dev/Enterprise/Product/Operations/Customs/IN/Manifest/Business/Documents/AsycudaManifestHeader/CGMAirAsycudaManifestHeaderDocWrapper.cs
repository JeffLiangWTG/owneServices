using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.DocumentWrappers;
using Enterprise.Customs.IN.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IN.Manifest.Business;

sealed class CGMAirAsycudaManifestHeaderDocWrapper : ASYCUDA.Business.AsycudaManifestHeaderDocWrapper
{
	public CGMAirAsycudaManifestHeaderDocWrapper(CGMAsycudaManifestHeader manifestHeader) : base(manifestHeader)
	{ }

	public new CGMAsycudaManifestHeader Manifest => (CGMAsycudaManifestHeader)base.Manifest;

	public ZString AgentCode => INCustomsDataRegistry.Instance.INConsolAgentRegistrationNumber.Value;

	public ZString AgentName => GlbCompany.CurrentCompany.CompanyName;

	public ZString IGMNumber => Manifest.ImportGeneralManifestNumber;

	public ZDate IGMDate => Manifest.ImportGeneralManifestDate;

	public ZString FlightNumber => Manifest.AMA_Voyage;

	public ZDateTime FlightDateTime => Manifest.AMA_E_ARV;

	public ZString MAWBNumber => MasterBillDetails.BillNumber;

	public ZDate MAWBDate => MasterBillDetails.BillDate;

	public ZString PortOfOrigin => MasterBillDetails.PortOfOrigin;

	public ZString PortOfDestination => MasterBillDetails.PortOfDestination;

	public ZInt NumberOfPackages => MasterBillDetails.NumberOfPackages;

	public ZDecimal GrossWeightInKilos => MasterBillDetails.GrossWeightInKilos;

	public ZString CargoDescription => MasterBillDetails.CargoDescription;

	public BusinessObjectCollectionWrapper<CGMAirAsycudaBillDocWrapper> BillsWrapper
	{
		get
		{
			if (billsWrapper == null)
			{
				var asycudaBillDocWrappers = Manifest
												.Bills.Cast<CGMAsycudaBill>()
												.OrderBy(x => x.ABL_SequenceNumber)
												.Select(x => new CGMAirAsycudaBillDocWrapper(x));
				billsWrapper = new BusinessObjectCollectionWrapper<CGMAirAsycudaBillDocWrapper>(asycudaBillDocWrappers);
			}
			return billsWrapper;
		}
	}
	BusinessObjectCollectionWrapper<CGMAirAsycudaBillDocWrapper> billsWrapper;

	ICGMAirwayBillDetails MasterBillDetails => masterBillDetails ??= new CGMAirwayBillDetailsProvider(Manifest.MasterBill);
	ICGMAirwayBillDetails masterBillDetails;
}
