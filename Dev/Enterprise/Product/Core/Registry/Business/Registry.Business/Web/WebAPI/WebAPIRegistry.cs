using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Web.WebAPI
{
	public sealed class WebAPIRegistry : RegistryItemSet
	{
		public static WebAPIRegistry Instance
		{
			get { return fInstance ?? (fInstance = new WebAPIRegistry()); }
		}

		[ThreadStatic]
		static WebAPIRegistry fInstance;

		public override bool IsForProductivityWise => false;

		public StringArrayRegistryItem ClientIDs => GetItem("WebAPIClientIDs",
			() => new StringArrayRegistryItem(
				"WebAPIClientIDs",
				WebDataRegistry.WebServicesCategory,
				(NoResString)"Web Service Client IDs",
				(NoResString)"List of valid OAuth Client IDs for applications that need to access CargoWise web services.",
				RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport));
	}
}
