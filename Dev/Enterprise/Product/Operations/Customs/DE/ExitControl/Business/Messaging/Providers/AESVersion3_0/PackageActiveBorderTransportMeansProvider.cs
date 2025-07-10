using CargoWise.Customs.DE.MessageContracts;
using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0
{
	public class PackageActiveBorderTransportMeansProvider : PackageProvider, IPackageActiveBorderTransportMeans
	{
		public PackageActiveBorderTransportMeansProvider(CusExitReportItem reportItem, string informationType) : base(reportItem, informationType)
		{
		}

		public IActiveBorderTransportMeans ActiveBorderTransportMeans
		{
			get
			{
				if (activeBorderTransportMeans == null && packageDetailsNotMissing)
				{
					activeBorderTransportMeans = new ActiveBorderTransportMeansProvider((CusExitReport)reportItem.Report);
				}
				return activeBorderTransportMeans;
			}
		}
		IActiveBorderTransportMeans activeBorderTransportMeans;
	}
}
