using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZCodeFindBoxFetchHintHandler
	{
		public ZCodeFindBoxFetchHintHandler(IBindToList bindToList, object dataSource)
		{
			this.Control = bindToList;
			this.DataSource = dataSource;
		}

		protected readonly IBindToList Control;
		protected readonly object DataSource;

		protected virtual string BindingMember
		{
			get { return ((Control)Control).GetBindingMember(); }
		}

		protected string BindToList
		{
			get { return Control.BindToList; }
		}

		public void Add()
		{
			if (!BindingMember.Contains(".") && !BindingMember.Contains("+"))
			{
				var bizO = DataSource as BusinessObject;
				if (bizO != null && !bizO.IsDeleted)
				{
					AddFetchHint(bizO);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Property name")]
		protected Type GetListType(BusinessObject bizO)
		{
			Type result = null;
			if (!string.IsNullOrEmpty(BindToList))
			{
				var flatBindToList = BindToList.Replace(".", "+");
				var flattenedProperties = TypeDescriptor.GetProperties(bizO);
				var listDescriptor = flattenedProperties[flatBindToList];
				if (listDescriptor != null)
				{
					try
					{
						var propInfo = listDescriptor.PropertyType.GetProperty("Item", new Type[] { typeof(int) });
						if (propInfo != null)
						{
							result = propInfo.PropertyType;
						}
					}
					catch (AmbiguousMatchException)
					{
						var propInfos = listDescriptor.PropertyType.GetProperties();
						foreach (var propInfo in propInfos)
						{
							if (propInfo.Name == "Item")
							{
								result = propInfo.PropertyType;
								break;
							}
						}
					}
				}
			}

			return result;
		}

		protected virtual void AddFetchHint(BusinessObject bizO)
		{
			if (bizO.Factory != null)
			{
				var type = GetListType(bizO);
				if (type != null && type.IsSubclassOf(typeof(BusinessObject)))
				{
					var codePropertyName = CodePropertyAttribute.CodePropertyNameFromType(type);
					if (codePropertyName != null)
					{
						var tableName = BusinessObjectFactory.GetTableNameFromType(type);
						var column = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(codePropertyName, tableName);
						if (column != null)
						{
							bizO.Factory.AddFetchHint(column, (IZType)bizO[BindingMember]);
						}
					}
				}
			}
		}
	}
}
