using System;
using System.Collections;
using System.Drawing.Design;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Design;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Globals = Enterprise.ZArchitecture.Environment.Globals;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture.Design
{
	public static class DesignTimeEnvironment
	{
		public static void InitializeDesignTimeEarlyWithoutServiceProvider()
		{
			if (!(AssemblyLoader.Instance is DesignTimeAssemblyLoader))
			{
				var dte = VisualStudioDTE.GetDTE();
				AssemblyLoader.Instance = new DesignTimeAssemblyLoader(new OleServiceProvider(dte as IOleServiceProvider));
				AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs args)
				{
					if (AssemblyLoader.Instance is DesignTimeAssemblyLoader)
					{
						return AssemblyLoader.LoadAssembly(args.Name);
					}
					return null;
				};
				EnterpriseApplicationConfiguration.ConfigureObjectFactory();
				ObjectFactory.Get<Enterprise.Integration.Environment.INullEnvProvider>().Enable();
				Res.SetResourceStringsGetter(GetResourceStringsInstance);
				AssemblyLoader.LoadAssembly("CargoWise.BrandManager.resources");
			}
		}

		static IResourceStrings GetResourceStringsInstance()
		{
			return DesignerSafeObjectFactory.GetDesignerSafe<IResourceStrings>();
		}

		public static void InitializeDesignTimeWithServiceProvider(IServiceProvider provider)
		{
			if (provider != null && !Globals.IsTest)
			{
				AddWarningAboutToolboxItemZEquivalent(provider);
			}
		}

		static void AddWarningAboutToolboxItemZEquivalent(IServiceProvider serviceProvider)
		{
			var toolbox = (IToolboxService)serviceProvider.GetService(typeof(IToolboxService));
			foreach (ToolboxItem item in new ArrayList(toolbox.GetToolboxItems()))
			{
				var type =
					string.IsNullOrEmpty(item.TypeName) ? null :
					typeof(Form).Assembly.GetType(item.TypeName) ??
					typeof(KForm).Assembly.GetType(item.TypeName);

				var ztype =
					type == null ? null :
					typeof(ZForm).Assembly.GetType("Enterprise.ZArchitecture.GUI." + type.Name.TrimStart('K')) ??
					typeof(ZForm).Assembly.GetType("Enterprise.ZArchitecture.GUI.Z" + type.Name.TrimStart('K')) ??
					typeof(ZForm).Assembly.GetType("Enterprise.ZArchitecture.Z" + type.Name.TrimStart('K'));
				if (type != null && ztype != null)
				{
					item.ComponentsCreating -= new ToolboxComponentsCreatingEventHandler(WarnAboutToolboxItemZEquivalent);
					item.ComponentsCreating += new ToolboxComponentsCreatingEventHandler(WarnAboutToolboxItemZEquivalent);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "for design time only, Developer design constant")]
		static void WarnAboutToolboxItemZEquivalent(object sender, ToolboxComponentsCreatingEventArgs e)
		{
			MessageBox.Show(
				"Consider using the product (Z-Architecture) version of this component which\r\n" +
				"is in the " + typeof(ZForm).Assembly.GetName().Name + " assembly, which inherits from this component.",
				"K-Architecture");
		}
	}
}
