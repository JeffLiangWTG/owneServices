using System;
using System.Reflection;
using AppDomainWrappers.Net;
using CargoWise.Common;
using CargoWise.Common.Testing;

namespace CargoWise.Design
{
	internal static class SystemDesignLoader
	{
		public static Assembly SystemDesignAssembly
		{
			get
			{
				if (systemDesignAssembly == null)
				{
					try
					{
						systemDesignAssembly = Assembly.Load("System.Design");
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						var appDomainWrapper = new AppDomainWrapper();
						var assemblies = appDomainWrapper.GetAssemblies();
						foreach (Assembly next in assemblies)
						{
							if (next.GetName().Name == "System.Design" &&
								(systemDesignAssembly == null || next.GetName().Version > systemDesignAssembly.GetName().Version))
							{
								systemDesignAssembly = next;
							}
						}
					}
				}
				return systemDesignAssembly;
			}
		}
		[SuppressThreadStaticFieldMessage]
		static Assembly systemDesignAssembly;
	}
}
