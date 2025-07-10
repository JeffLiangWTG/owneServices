using System;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Modules
{
	public interface IShowEditFormUrlCreator
	{
		string Create(ControllerID controllerID, Guid pk);
		string Create(IControllerIDProvider provider);
		string CreateWithoutApplicationContext(ControllerID controllerID, ZGuid pk);
		string CreateWithSpecifiedLicenceCode(ControllerID controllerID, ZGuid pk, string licenceCode);
	}
}
