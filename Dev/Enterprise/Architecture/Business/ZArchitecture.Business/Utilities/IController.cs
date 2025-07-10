using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture
{
	public interface IController
	{
		ControllerID ID { get; }
		ModuleIdentifier ModuleID { get; }
		Type TypeOfTopLevelBusinessObject { get; }
		BusinessObject GetFormBusinessObject(IBusiness sourceEntity);
	}
}
