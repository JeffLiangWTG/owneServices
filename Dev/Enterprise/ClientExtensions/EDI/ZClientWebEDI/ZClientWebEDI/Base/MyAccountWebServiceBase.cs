using System;
using System.ServiceModel;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZClientWebCargoWiseEDI.Services;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public abstract class MyAccountWebServiceBase
	{
		protected virtual void ValidateRequestIpAddress()
		{
			var (isAccessAllowed, clientIp) = requestIPValidationHelper.IsCurrentRequestFromValidIP();
			if (!isAccessAllowed)
			{
				HandleError(FormattableString.Invariant($"Access from IP address {clientIp} is denied"));
			}
		}

		readonly RequestIPValidationHelper requestIPValidationHelper = new RequestIPValidationHelper();

		protected virtual void HandleError(string errorMessage, string errorDetail = "")
		{
			ErrorData error = new ErrorData(errorMessage, errorDetail);
			throw new FaultException<ErrorData>(error, error.Reason);
		}

		protected void SetupWebEnviroment()
		{
			if (Env.CurrentUser == null)
			{
				WebAppEnvironment.Setup();
			}
		}
	}
}
