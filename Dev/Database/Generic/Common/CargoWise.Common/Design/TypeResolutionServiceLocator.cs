using System;
using System.ComponentModel.Design;
using System.Reflection;

namespace CargoWise.Common.Design
{
	public static class TypeResolutionServiceLocator
	{
		public static ITypeResolutionService Get(IServiceProvider serviceProvider)
		{
			Argument.NotNull(serviceProvider, nameof(serviceProvider)); // Suggested By ReviewBot 
			ITypeResolutionService result = (ITypeResolutionService)serviceProvider.GetService(typeof(ITypeResolutionService));
			if (result == null)
			{
				object dynamicTypeService = GetDynamicTypeService(serviceProvider);
				if (dynamicTypeService != null)
				{
					result = (ITypeResolutionService)GetActiveResolverProperty(dynamicTypeService).GetValue(dynamicTypeService, null);
				}
			}
			return result;
		}

		public static object GetDynamicTypeService(IServiceProvider serviceProvider)
		{
			Argument.NotNull(serviceProvider, nameof(serviceProvider)); // Suggested By ReviewBot 
			return serviceProvider.GetService(DynamicTypeServiceType);
		}

		public static Type DynamicTypeServiceType
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

		readonly static Lazy<Type> dynamicTypeServiceType = new Lazy<Type>(GetDynamicTypeServiceType);

		#region Implementation

		static PropertyInfo GetActiveResolverProperty(object dynamicTypeService)
		{
			Argument.NotNull(dynamicTypeService, nameof(dynamicTypeService)); // Suggested By ReviewBot 
			var property = dynamicTypeService.GetType().GetProperty("ActiveResolver", BindingFlags.NonPublic | BindingFlags.Instance);
			return property;
		}

#endregion
	}
}
