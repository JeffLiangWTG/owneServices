using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.FeatureControl;
using CargoWise.FeatureControl.Abstractions;

namespace Enterprise.Licensing.Billing.Business
{
	public class FeatureControlManagerWrapper : IFeatureControlManager
	{
		public FeatureControlManagerWrapper()
		{
			FeatureControlManager = new FeatureControlManager(ObjectFactory.Get<IReadOnlyFeatureControlStorage>(), new ErrorReporterLogger<FeatureControlManager>(nameof(CargoWise.FeatureControl.FeatureControlManager)));
		}

		public IFeatureControlManager FeatureControlManager { get; }

		public Task<IFeatureData> GetFeatureDataAsync(string featureCode, CancellationToken cancellationToken)
		{
			return FeatureControlManager.GetFeatureDataAsync(featureCode, cancellationToken);
		}
	}
}
