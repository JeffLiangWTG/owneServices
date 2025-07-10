using System;
using System.ServiceModel;
using System.ServiceModel.Activation;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;

namespace Enterprise.ZClientWebCargoWiseEDI.Services
{
	[ServiceContract(Namespace = "http://schemas.cargowise.com/")]
	public interface ISubscriptionPreferenceService
	{
		[OperationContract]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
		string GetSubscriptionPreferenceUrl(Guid contactPK, string publishedListCode);
	}

	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	[ServiceBehavior(Namespace = "http://schemas.cargowise.com/")]
	public class SubscriptionPreferenceService : MyAccountWebServiceBase, ISubscriptionPreferenceService
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
		public string GetSubscriptionPreferenceUrl(Guid contactPK, string publishedListCode)
		{
			ZString result = ZString.Empty;

			try
			{
				ValidateRequestIpAddress();
				SetupWebEnviroment();
				result = UnsubscribeUrlHelper.GetSubscriptionPreferenceUrlString(contactPK, publishedListCode, 5);
			}
			catch (Exception ex) when (!ex.IsCriticalException() && !(ex is FaultException<ErrorData>))
			{
				HandleError("Server Error", ex.Message + System.Environment.NewLine + ex.StackTrace);
				ErrorReporter.ReportOnce(ex.Message, ex);
			}

			return result;
		}
	}
}
