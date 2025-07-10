using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.DTO;
using Enterprise.BufferManagement.Service.Helpers;
using Enterprise.BufferManagement.Service.Shared.Common;
using Enterprise.BufferManagement.Service.Shared.Incident;
using Enterprise.CustomerService.Business;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Integration.ZClientEDI;

namespace Enterprise.BufferManagement.Service
{
	public class IncidentService : IIncidentService
	{
		public bool TryUpdateSummary(Guid incidentId, UpdateContentWithHashRequest updateRequest, out PaveError error)
		{
			error = null;
			var factory = new BusinessObjectFactory() { NameForDebugging = nameof(IncidentService), RefreshEnabled = false };

			if (updateRequest.PreviousHash == null)
			{
				throw new ArgumentNullException(nameof(updateRequest), nameof(updateRequest.PreviousHash));
			}

			if (updateRequest.NewContent == null)
			{
				throw new ArgumentNullException(nameof(updateRequest), nameof(updateRequest.NewContent));
			}

			if (ClientHookLoader.Instance?.Client is Clients.EDI)
			{
				var incidentMain = factory.Load<IIncidentMain>(incidentId);

				var hash = HashHelper.GetHash(incidentMain.IM_Description);
				if (hash != updateRequest.PreviousHash)
				{
					error = new PaveError(PaveErrorCode.ValidationError, new string[] { LocalizationHelper.WorkflowTaskReordering.FieldChangedByAnotherUserMessage });
					return false;
				}

				incidentMain.IM_Description = updateRequest.NewContent;
			}
			else
			{
				var incidentRequest = factory.Load<IncidentRequest>(incidentId);

				var hash = HashHelper.GetHash(incidentRequest.INC_Summary);
				if (hash != updateRequest.PreviousHash)
				{
					error = new PaveError(PaveErrorCode.ValidationError, new string[] { LocalizationHelper.WorkflowTaskReordering.FieldChangedByAnotherUserMessage });
					return false;
				}

				incidentRequest.INC_Summary = updateRequest.NewContent;
			}

			factory.Save();

			return true;
		}
	}
}
