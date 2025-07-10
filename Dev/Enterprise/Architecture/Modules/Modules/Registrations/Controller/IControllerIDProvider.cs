using System;

namespace Enterprise.ZArchitecture.Modules
{
	public interface IControllerIDProvider
	{
		ControllerID ControllerID { get; }
		Guid BusinessObjectPK { get; }
	}
}
