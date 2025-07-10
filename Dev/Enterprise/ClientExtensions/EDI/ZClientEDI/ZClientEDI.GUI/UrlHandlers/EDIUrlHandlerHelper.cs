using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture;

namespace ZClientEDI.GUI.UrlHandlers
{
	public class EDIUrlHandlerHelper : IEDIUrlHandlerHelper
	{
		public bool RequiresExtensionController(string queryStringControllerID)
		{
			return queryStringControllerID == "IncidentRequest";
		}

		public (string controllerID, Guid businessEntityPk) GetExtensionControllerIDAndBusinessEntityPk(string queryStringControllerID, Guid queryStringBusinessEntityPk)
		{
			var factory = new BusinessObjectFactory() { NameForDebugging = nameof(EDIUrlHandlerHelper) };
			var incidentRequest = factory.Load<EdiIncidentRequest>(queryStringBusinessEntityPk);
			var supportIncident = incidentRequest?.RelatedSupportIncident;

			if (incidentRequest != null && supportIncident == null)
			{
				ErrorReporter.ReportOnce("CannotFindSupportIncident", $"Cannot find a support incident corresponding to the incident request {incidentRequest.PK}");
			}

			return ("SupportIncident", supportIncident?.PK.ToGuid() ?? Guid.Empty);
		}
	}
}
