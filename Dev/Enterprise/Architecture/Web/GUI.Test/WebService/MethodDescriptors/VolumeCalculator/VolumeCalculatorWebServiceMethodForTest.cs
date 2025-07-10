using System;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.Web.ServerServices.Testing
{
	public class VolumeCalculatorWebServiceMethodForTest : VolumeCalculatorWebServiceMethod
	{
		public decimal Execute(VolumeCalculatorParameters parameters)
		{
			var response = new WebServiceResponse();
			base.ExecuteCore(parameters, response);
			if (response.Count != 0)
			{
				try
				{
					return decimal.Parse(response[0].Value, Shared.WebEnvShared.ClientCulture);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
			}
			return 0;
		}
	}
}
