using System;
using System.Drawing;
using System.Web.Compilation;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.UI.WebControls
{
	[ToolboxData("<{0}:BusinessObjectDataSource runat=server></{0}:BusinessObjectDataSource>"), ToolboxBitmap(typeof(ObjectDataSource))]
	public class BusinessObjectDataSource : ObjectDataSource
	{
		public BusinessObjectDataSource()
		{
			TypeName = typeof(BusinessObjectDataSourceProvider).AssemblyQualifiedName;
			ObjectCreating += BusinessObjectDataSource_ObjectCreating;
		}

		public string BindTo
		{
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					if (string.IsNullOrEmpty(TypeName))
					{
						TypeName = typeof(BusinessObjectDataSourceProvider).FullName;
					}

					SelectMethod = "GetProperty";
					SelectParameters.Clear();
					SelectParameters.Add("propertyName", value);
					SortParameterName = "orderBy";
				}
			}
		}

		void BusinessObjectDataSource_ObjectCreating(object sender, ObjectDataSourceEventArgs e)
		{
			ZPage zPage = Page as ZPage;
			if (zPage != null)
			{
				Type dataSourceType = BuildManager.GetType(TypeName, false, true);
				if (dataSourceType == null || !typeof(BusinessObjectDataSourceProvider).IsAssignableFrom(dataSourceType))
				{
					throw new InvalidOperationException("TypeName has to be of type " + typeof(BusinessObjectDataSourceProvider).FullName);
				}
				e.ObjectInstance = Activator.CreateInstance(dataSourceType, zPage.DataSource);
			}
		}

		public override void Dispose()
		{
			ObjectCreating -= BusinessObjectDataSource_ObjectCreating;
			base.Dispose();
		}
	}
}
