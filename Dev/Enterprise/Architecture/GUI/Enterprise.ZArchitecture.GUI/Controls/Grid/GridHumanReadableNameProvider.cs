using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI
{
	public class GridHumanReadableNameProvider : IHumanReadableNameProvider
	{
		public GridHumanReadableNameProvider(ZGrid grid)
		{
			this.grid = grid;
		}

		readonly ZGrid grid;

		public ZString GetHumanReadableName(ZPropertyInfo propertyInfo)
		{
			try
			{
				var collection = grid.List as IBusinessObjectCollection;
				if (collection != null)
				{
					if (collection.Contains(propertyInfo.BizObj))
					{
						foreach (ZGridColumnStyle columnStyle in grid.TableStyles[0].GridColumnStyles)
						{
							if (columnStyle.MappingName == propertyInfo.Name)
							{
								return GetHeaderText(columnStyle);
							}
						}
					}
					else
					{
						var propertyBizObjType = propertyInfo.BizObj.GetType();
						var humanReadalbeNameAttribute = (HumanReadableNameAttribute)propertyBizObjType.GetCustomAttributes(typeof(HumanReadableNameAttribute), true).FirstOrDefault();
						if (humanReadalbeNameAttribute != null)
						{
							var propertyInfoBizO = propertyBizObjType.InvokeMember(humanReadalbeNameAttribute.PropertyInfoBusinessObjectName,
														BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty, null, propertyInfo.BizObj, null, CultureInfo.InvariantCulture);

							if (propertyInfoBizO != null)
							{
								if (collection.Contains(propertyInfoBizO))
								{
									foreach (ZGridColumnStyle columnStyle in grid.TableStyles[0].GridColumnStyles)
									{
										if (columnStyle.MappingName == propertyInfo.Name)
										{
											return GetHeaderText(columnStyle);
										}
									}
								}
							}
						}
						else
						{
							var firstElement = collection.Count > 0 ? collection[0] as BusinessObject : null;
							if (firstElement != null && propertyInfo.BizObj != null)
							{
								foreach (ZGridColumnStyle columnStyle in grid.TableStyles[0].GridColumnStyles)
								{
									if (columnStyle.MappingName.EndsWith(propertyInfo.Name))
									{
										var wrappedProperty = firstElement.FindPropertyInfo(columnStyle.MappingName) as ZWrappedPropertyInfo;
										ZPropertyInfo innerInfo = null;
										while (wrappedProperty != null)
										{
											innerInfo = wrappedProperty.InnerInfo;
											wrappedProperty = innerInfo as ZWrappedPropertyInfo;
										}
										if (innerInfo != null && innerInfo.Name == propertyInfo.Name &&
											innerInfo.BizObj != null && innerInfo.BizObj.GetType() == propertyBizObjType)
										{
											return GetHeaderText(columnStyle);
										}
									}
								}
							}
						}
					}
				}
				return ZString.Empty;
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}
				return ZString.Empty;
			}
		}

		ZString GetHeaderText(ZGridColumnStyle columnStyle)
		{
			return columnStyle.ResourceHeader.GetHeaderText(columnStyle.HeaderText, columnStyle.HeaderTextWasDefaulted, true);
		}
	}
}
