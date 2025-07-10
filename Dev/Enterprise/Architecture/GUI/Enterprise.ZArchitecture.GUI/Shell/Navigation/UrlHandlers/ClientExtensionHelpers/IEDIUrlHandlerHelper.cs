using System;

namespace Enterprise.ZArchitecture
{
	public interface IEDIUrlHandlerHelper
	{
		bool RequiresExtensionController(string queryStringControllerID);

		(string controllerID, Guid businessEntityPk) GetExtensionControllerIDAndBusinessEntityPk(string queryStringControllerID, Guid queryStringBusinessEntityPk);
	}
}
