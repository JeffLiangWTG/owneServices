using System;
using System.ComponentModel.Design;
using System.IO;
using System.Reflection;

namespace CargoWise.Common.Design
{
	/// <summary>
	/// An IAssemblyLoader implementation for design time use using an ITypeResolutionService.
	/// Assign AssemblyLoader.Instance to an instance of this class at design time.
	/// </summary>
	[WTG.StaticAnalysis.Annotation.Immutable]
	public sealed class DesignTimeAssemblyLoader : IAssemblyLoader
	{
		public DesignTimeAssemblyLoader(IServiceProvider serviceProvider)
		{
			Argument.NotNull(serviceProvider, nameof(serviceProvider));
			this.serviceProvider = serviceProvider;
		}

		public Assembly LoadAssembly(AssemblyName assemblyName)
		{
			ITypeResolutionService typeResolutionService = TypeResolutionServiceLocator.Get(serviceProvider);
			return typeResolutionService != null
					? typeResolutionService.LoadAssembly(serviceProvider, Assembly.GetCallingAssembly(), assemblyName)
					: null;
		}

		public string GetBinPath()
		{
			ITypeResolutionService typeResolutionService = TypeResolutionServiceLocator.Get(serviceProvider);
			var assemblyLocation = typeResolutionService == null ?
				GetType().Assembly.Location :
				typeResolutionService.GetPathOfAssembly(Assembly.GetCallingAssembly().GetName());
			return Path.GetDirectoryName(assemblyLocation);
		}

		public string GetParentBinPath()
		{
			var path = GetBinPath();
			return Directory.GetParent(path).FullName;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "Assume that IServiceProvider is immutable")]
		readonly IServiceProvider serviceProvider;
	}
}
