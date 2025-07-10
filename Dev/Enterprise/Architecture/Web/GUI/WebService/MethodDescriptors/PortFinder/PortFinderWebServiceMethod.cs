using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.ServerServices
{
	public class PortFinderWebServiceMethod : WebServiceMethod<PortFinderParameters>
	{
		#region Overrides

		protected override void ExecuteCore(PortFinderParameters parameters, WebServiceResponse response)
		{
			var factory = new BusinessObjectFactory(Db.Connection);
			response.Add(new UpdateValueResponseToken(parameters.PortControlID, (new PortLoader()).GetClosestPortCode(parameters.PostalCode, parameters.City, parameters.State, "", parameters.Country, factory)));
		}

		protected override string GetMethodName()
		{
			return "FindPort";
		}

		#endregion

	}
}
