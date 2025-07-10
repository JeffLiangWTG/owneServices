using System.ServiceModel;
using System.ServiceModel.Activation;
using CargoWise.Data;
using Enterprise.Client.EDI.Licencing.Business;

namespace Enterprise.ZClientWebCargoWiseEDI.Services
{
	[ServiceContract]
	public interface IUpgradePackageService
	{
		[OperationContract]
		UpgradePackageUrlResponse GetPackageUrl(UpgradePackageUrlRequest request);
	}

	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
	public class UpgradePackageService : IUpgradePackageService
	{
		public UpgradePackageUrlResponse GetPackageUrl(UpgradePackageUrlRequest request)
		{
			using (Db.DisposableActionForDbConnection())
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var logger = new NLogWrapper(typeof(UpgradePackageService));
				return new UpgradePackageUrlGenerator(connection, logger).GetPackageUrl(request);
			}
		}
	}
}
