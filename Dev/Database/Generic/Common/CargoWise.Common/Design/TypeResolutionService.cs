using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace CargoWise.Common.Design
{
	public static class TypeResolutionService
	{
		/// <summary>
		/// Load an assembly from the given ITypeResolutionService.
		/// </summary>
		/// <param name="typeResolutionService">The ITypeResolutionService to use to load the assembly.</param>
		/// <param name="serviceProvider">The IServiceProvider to use to load the assembly.</param>
		/// <param name="resolvedAssembly">
		/// An assembly that exists in the same directory and was resolved by the ITypeResolutionService instance.
		/// </param>
		/// <param name="assemblyName">The name of the assembly to load.</param>
		/// <returns></returns>
		public static Assembly LoadAssembly(this ITypeResolutionService typeResolutionService, IServiceProvider serviceProvider, Assembly resolvedAssembly, AssemblyName assemblyName)
		{
			Argument.NotNull(typeResolutionService, nameof(typeResolutionService));
			Argument.NotNull(serviceProvider, nameof(serviceProvider));
			Argument.NotNull(resolvedAssembly, nameof(resolvedAssembly));
			Argument.NotNull(assemblyName, nameof(assemblyName));
			return TypeResolutionServiceEx.GetInstance(typeResolutionService, serviceProvider).LoadAssembly(resolvedAssembly, assemblyName);
		}

		/// <summary>
		/// Load an assembly from the given ITypeResolutionService from the given path.
		/// This will cause the designer to shadow copy the assembly to a temporary directory and load it
		/// from that directory into memory.
		/// This method difers from ITypeResolutionService.GetAssembly in that it doesn't require the
		/// assembly to be referenced, and it takes an assembly path instead of just an assembly name.
		/// </summary>
		/// <param name="typeResolutionService">The ITypeResolutionService to use to load the assembly.</param>
		/// <param name="serviceProvider">The IServiceProvider to use to load the assembly.</param>
		/// <param name="path">The fully qualified path of the assembly to load.</param>
		/// <returns></returns>
		public static Assembly LoadAssembly(this ITypeResolutionService typeResolutionService, IServiceProvider serviceProvider, string path)
		{
			Argument.NotNull(typeResolutionService, nameof(typeResolutionService));
			Argument.NotNull(serviceProvider, nameof(serviceProvider));
			return TypeResolutionServiceEx.GetInstance(typeResolutionService, serviceProvider).LoadAssembly(path);
		}
	}

	internal interface ITypeResolutionServiceEx
	{
		Assembly LoadAssembly(Assembly resolvedAssembly, AssemblyName assemblyName);
		Assembly LoadAssembly(string path);
	}

	internal sealed class TypeResolutionServiceEx : ITypeResolutionServiceEx
	{
		TypeResolutionServiceEx(ITypeResolutionService typeResolutionService, IServiceProvider serviceProvider)
		{
			Argument.NotNull(typeResolutionService, nameof(typeResolutionService));
			Argument.NotNull(serviceProvider, nameof(serviceProvider));
			this.typeResolutionService = typeResolutionService;
			this.serviceProvider = serviceProvider;
		}

		public static ITypeResolutionServiceEx GetInstance(ITypeResolutionService typeResolutionService, IServiceProvider serviceProvider)
		{
			Argument.NotNull(typeResolutionService, nameof(typeResolutionService));
			Argument.NotNull(serviceProvider, nameof(serviceProvider));
			ITypeResolutionServiceEx result = typeResolutionService as ITypeResolutionServiceEx
				?? new TypeResolutionServiceEx(typeResolutionService, serviceProvider);
			return result;
		}

		public Assembly LoadAssembly(Assembly resolvedAssembly, AssemblyName assemblyName)
		{
			string assemblyPath = Path.GetDirectoryName(typeResolutionService.GetPathOfAssembly(resolvedAssembly.GetName()));
			if (string.IsNullOrEmpty(assemblyPath))
			{
				throw new InvalidOperationException("Can't resolve assembly path");
			}
			Assembly result = TryLoadAssembly(Path.Combine(assemblyPath, assemblyName.Name + ".dll"))
				?? TryLoadAssembly(Path.Combine(assemblyPath, assemblyName.Name + ".exe"));
			return result;
		}

		Assembly TryLoadAssembly(string path)
		{
			try
			{
				return LoadAssembly(path);
			}
			catch (FileNotFoundException)
			{
				return null;
			}
		}

		public Assembly LoadAssembly(string path)
		{
			if (SystemTypeResolutionService != null && DynamicTypeService == null)
			{
				throw new InvalidOperationException(GetType().FullName + " is known to work on Visual Studio 2008, but may not work on versions beyond this.");
			}
			return (Assembly)DynamicTypeService.GetType().InvokeMember("CreateDynamicAssembly", BindingFlags.InvokeMethod, null, DynamicTypeService, new object[] { path }, CultureInfo.InvariantCulture);
		}

		#region Implementation

		readonly ITypeResolutionService typeResolutionService;
		readonly IServiceProvider serviceProvider;

		ITypeResolutionService SystemTypeResolutionService
		{
			get
			{
				if (IsSystemTypeResolutionService(typeResolutionService))
				{
					systemTypeResolutionService = typeResolutionService;
				}
				else
				{
					PropertyInfo inner = typeResolutionService.GetType().GetProperty("Inner");
					if (inner != null)
					{
						systemTypeResolutionService = (ITypeResolutionService)inner.GetValue(typeResolutionService, null);
						if (!IsSystemTypeResolutionService(systemTypeResolutionService))
						{
							systemTypeResolutionService = null;
						}
					}
				}
				return systemTypeResolutionService;
			}
		}
		ITypeResolutionService systemTypeResolutionService;

		static bool IsSystemTypeResolutionService(ITypeResolutionService typeResolutionService)
		{
			Argument.NotNull(typeResolutionService, nameof(typeResolutionService));
			return
				typeResolutionService.GetType().FullName.StartsWith("Microsoft.VisualStudio", StringComparison.Ordinal) ||
				typeResolutionService.GetType().Name.StartsWith("Test", StringComparison.Ordinal); // test type name
		}

		object DynamicTypeService
		{
			get
			{
				return serviceProvider.GetService(DynamicTypeServiceType);
			}
		}

		internal static Type DynamicTypeServiceType
		{
			get
			{
				var value = dynamicTypeServiceType.Value;
				return value;
			}
		}

		static Type GetDynamicTypeServiceType()
		{
			var shellDesignAssembly = Assembly.Load("Microsoft.VisualStudio.Shell.Design");
			return shellDesignAssembly.GetType("Microsoft.VisualStudio.Shell.Design.DynamicTypeService");
		}

		static readonly Lazy<Type> dynamicTypeServiceType = new Lazy<Type>(GetDynamicTypeServiceType);

		#endregion
	}
}
