using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Dash.Integration.Services
{
	public interface IDashPostingService
	{
		public ZGuid PostUxml(ZGuid branchId, ZGuid departmentId, ZGuid documentId, string uxml, BusinessObjectFactory factory = null);
	}
}
