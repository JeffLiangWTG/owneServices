using System;

namespace Enterprise.ZArchitecture.Modules
{
	public interface IShowViewFormUrlCreator
	{
		string Create(ControllerID controllerID, Guid pk);
	}
}
