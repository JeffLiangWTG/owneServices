using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.Business.Design
{
	public class TopLevelDataSourceTypeHelper : IDesignTimeDataSourceType
	{
		public TopLevelDataSourceTypeHelper(IComponent component)
		{
			this.Component = component;
			dataSourceAssemblyName = "";
			dataSourceTypeName = "";
		}

		public IComponent Component { get; private set; }

		#region DataSourceAssemblyName / DataSourceTypeName

		public string DataSourceAssemblyName
		{
			get { return dataSourceAssemblyName; }
			set
			{
				dataSourceAssemblyName = value;
				if (DesignerHost != null && Component == DesignerHost.RootComponent)
				{
					if (BindingSource != null && LegacyDataSourceType != null)
					{
						BindingSource.DataSourceType = LegacyDataSourceType;
					}
				}
			}
		}
		string dataSourceAssemblyName;

		public string DataSourceTypeName
		{
			get { return dataSourceTypeName; }
			set
			{
				dataSourceTypeName = value;
				if (DesignerHost != null && Component == DesignerHost.RootComponent)
				{
					if (BindingSource != null && LegacyDataSourceType != null)
					{
						BindingSource.DataSourceType = LegacyDataSourceType;
					}
				}
			}
		}
		string dataSourceTypeName;

		#endregion

		#region DataSourceType

		public Type DataSourceType
		{
			get
			{
				var result = BindingSource == null ? null : BindingSource.DataSourceType;
				if ((result == null || result == typeof(object)) && !inGetDataSourceType)
				{
					inGetDataSourceType = true;
					try
					{
						var type = LegacyDataSourceType;
						if (type != null)
						{
							result = type;
						}
					}
					finally
					{
						inGetDataSourceType = false;
					}
				}
				return result;
			}
		}
		bool inGetDataSourceType;

		Type LegacyDataSourceType
		{
			get
			{
				var dataSourceTypeName = GetValueFromProperty(nameof(DataSourceTypeName));
				var dataSourceAssemblyName = GetValueFromProperty(nameof(DataSourceAssemblyName));
				if (dataSourceType == null ||
					dataSourceTypeName != lastDataSourceTypeName ||
					dataSourceAssemblyName != lastDataSourceAssemblyName)
				{
					if (!string.IsNullOrEmpty(dataSourceTypeName))
					{
						if (TypeResolutionService != null)
						{
							dataSourceType = GetTypeFromTypeResolutionService(dataSourceTypeName);
						}
						else
						{
							Assembly assembly = null;
							try
							{
								assembly = string.IsNullOrEmpty(dataSourceAssemblyName) ? null : AssemblyLoader.LoadAssembly(dataSourceAssemblyName);
							}
							catch (Exception ex) when (!ex.IsCriticalException())
							{
							}
							dataSourceType = (assembly == null || string.IsNullOrEmpty(dataSourceTypeName)) ? null : assembly.GetType(dataSourceTypeName);
						}
					}
					lastDataSourceAssemblyName = dataSourceAssemblyName;
					lastDataSourceTypeName = dataSourceTypeName;
				}
				return dataSourceType;
			}
			set
			{
				DataSourceTypeName = value == null ? null : value.FullName;
				DataSourceAssemblyName = value == null ? null : value.Assembly.GetName().Name;
			}
		}
		Type dataSourceType;
		string lastDataSourceTypeName;
		string lastDataSourceAssemblyName;

		Type GetTypeFromTypeResolutionService(string dataSourceTypeName)
		{
			try
			{
				return TypeResolutionService == null ? null : TypeResolutionService.GetType(dataSourceTypeName);
			}
			catch (ArgumentException)
			{
				// A .net bug throws an ArgumentException 'The path is not of a legal form'.
				// This is not a problem for us as we will return a fake type instead.
			}
			return null;
		}

		string GetValueFromProperty(string propertyName)
		{
			return (string)TypeDescriptor.GetProperties(Component)[propertyName].GetValue(Component);
		}

		#endregion

		#region Implementation

		ICompositeControlBindingSource BindingSource
		{
			get
			{
				if (bindingSource == null)
				{
					var bindingSourceProvider = Component as ICompositeControlBindingSourceProvider;
					bindingSource = bindingSourceProvider == null ? null : bindingSourceProvider.BindingSource;
				}
				return bindingSource;
			}
		}
		ICompositeControlBindingSource bindingSource;

		IDesignerHost DesignerHost
		{
			get { return Component.Site == null ? null : (IDesignerHost)Component.Site.GetService(typeof(IDesignerHost)); }
		}

		ITypeResolutionService TypeResolutionService
		{
			get { return Component.Site == null ? null : (ITypeResolutionService)Component.Site.GetService(typeof(ITypeResolutionService)); }
		}

		#endregion
	}
}
