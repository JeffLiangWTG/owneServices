using System;
using CargoWise.EntityFramework;

namespace Enterprise.Dash.Business.Services
{
	public interface IDashCompletionService
	{
		void Complete(Guid dashDocumentId, BusinessObjectFactory factory = null);

		void Complete(DashDocument dashDocument, BusinessObjectFactory factory = null);
	}
}
