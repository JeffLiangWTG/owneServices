using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture
{
	public interface IControllerFactory
	{
		IController Create(ControllerID controllerId);
		IController GetControllerForType(Type type);
		ControllerID GetRegisteredIdentifierByName(string identifier);
		(IController Controller, BusinessObject BusinessObject) GetCorrectControllerAndBusinessObject(ControllerID controllerId, ZGuid pk, bool shouldReportError);
	}
}
