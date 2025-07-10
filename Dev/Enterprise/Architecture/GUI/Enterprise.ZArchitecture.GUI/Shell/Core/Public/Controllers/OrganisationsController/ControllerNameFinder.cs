using System;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ZArchitecture.Modules
{
	class ControllerNameFinder : IControllerNameFinder
	{
		public string GetControllerNameForType(Type type)
		{
			return ZControllerFactory.Instance.GetControllerForType(type)?.ID?.Name;
		}
	}
}
