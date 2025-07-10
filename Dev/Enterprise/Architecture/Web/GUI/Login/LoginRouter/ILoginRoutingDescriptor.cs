using System;

namespace Enterprise.ZArchitecture.Web.GUI.Login
{
	public interface ILoginRoutingDescriptor
	{
		bool IsRoutingRequired { get; }
		Uri RoutingUrl { get; }
		void RoutingAction();
	}
}
