using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Common;

namespace CargoWise.ComponentModel.Design
{
	/// <summary>
	/// Registers required design time services.
	/// </summary>
	public class DesignerServices
	{
		public DesignerServices(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
		}

		public void TryRegister()
		{
			if (serviceProvider != null)
			{
				ReplaceTypeResolutionService();
			}
		}

		#region Implementation

		readonly IServiceProvider serviceProvider;

		IServiceContainer ServiceContainer
		{
			get { return serviceProvider != null ? (IServiceContainer)serviceProvider.GetService(typeof(IServiceContainer)) : null; }
		}

		void ReplaceTypeResolutionService()
		{
			Argument.NotNull(serviceProvider, nameof(serviceProvider));
			var typeResolutionService = (ITypeResolutionService)serviceProvider.GetService(typeof(ITypeResolutionService));
			if (typeResolutionService != null && !(typeResolutionService is KTypeResolutionService) && ServiceContainer != null)
			{
				ServiceContainer.RemoveService(typeof(ITypeResolutionService));
				var cargowiseTypeResolutionService = new KTypeResolutionService(serviceProvider, typeResolutionService);
				ServiceContainer.AddService(typeof(ITypeResolutionService), cargowiseTypeResolutionService);
				var cargowiseDesignAssembly = TryLoadCargoWiseDesignAssembly(cargowiseTypeResolutionService);
				if (cargowiseDesignAssembly != null)
				{
					cargowiseTypeResolutionService.AddAssembly(cargowiseDesignAssembly);
				}
			}
		}

		Assembly TryLoadCargoWiseDesignAssembly(KTypeResolutionService typeResolutionService)
		{
			Argument.NotNull(serviceProvider, nameof(serviceProvider));
			var cargowiseDesignAssemblyFile = FindCargoWiseDesignAssemblyFile();
			Assembly result;
			if (cargowiseDesignAssemblyFile != null && typeResolutionService != null)
			{
				result = typeResolutionService.LoadAssemblyFrom(cargowiseDesignAssemblyFile);
			}
			else
			{
				result = null;
			}
#if DEBUG
			if (result == null && serviceProvider.GetService(DTEType) != null)
			{
				throw new InvalidOperationException("Could not find assembly CargoWise.Design at '" + (cargowiseDesignAssemblyFile ?? "unknown file location") + "'");
			}
#endif
			return result;
		}

		string FindCargoWiseDesignAssemblyFile()
		{
			return GetCurrentSolutionsReferences()
				.Select(referenceFile => Path.Combine(Path.GetDirectoryName(referenceFile), "CargoWise.Design.dll"))
				.FirstOrDefault(File.Exists);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Name search string")]
		IEnumerable<string> GetCurrentSolutionsReferences()
		{
			var solution = GetPropertyValue(serviceProvider?.GetService(DTEType), "Solution");
			var projects = GetPropertyValue(solution, "Projects");

			return ToEnumerable(projects)
				.Select(project => GetPropertyValue(project, "Object"))
				.SelectMany(vsobject => ToEnumerable(GetPropertyValue(vsobject, "References")))
				.Select(reference => (string)GetPropertyValue(reference, "Path"))
				.Where(path => !string.IsNullOrEmpty(path));
		}

		static IEnumerable<object> ToEnumerable(object o)
			=> ((IEnumerable)o)?.Cast<object>() ?? Enumerable.Empty<object>();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
		static Type DTEType
		{
			get { return Type.GetTypeFromCLSID(new Guid("04A72314-32E9-48E2-9B87-A63603454F3E")); }
		}

		static object GetPropertyValue(object component, string propertyName)
		{
			if (component == null)
			{
				return null;
			}

			return component.GetType().InvokeMember(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty, null, component, null, CultureInfo.InvariantCulture);
		}

		#endregion
	}
}
