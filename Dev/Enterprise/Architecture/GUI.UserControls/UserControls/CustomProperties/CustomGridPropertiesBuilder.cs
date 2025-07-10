using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI
{
	public class CustomGridPropertiesBuilder
	{
		public CustomGridPropertiesBuilder(ZGrid grid)
		{
			this.grid = grid;
		}

		readonly ZGrid grid;

		#region Add

		public void Add(Func<BusinessObject, CustomBusinessObject> customBizObjGetter, IEnumerable<ICustomProperty> customProperties)
		{
			foreach (var property in customProperties.OrderBy(x => GetColumnPosition(x)))
			{
				var columnStyleInfo = GetNewGridColumnInfo(customBizObjGetter, property);
				grid.ColumnStyles.Add(columnStyleInfo);
			}
		}

		static int GetColumnPosition(ICustomProperty property)
		{
			var position = property.Info.GetPosition();
			if (position == null || !position.HasValue)
			{
				return int.MaxValue;
			}

			return position.Value;
		}

		static ZGridColumnInfo GetNewGridColumnInfo(Func<BusinessObject, CustomBusinessObject> customBizObjGetter, ICustomProperty property)
		{
			var propertyType = property.Info.Type;
			ZGridColumnInfo info = null;
			if (propertyType == typeof(ZBool))
			{
				info = new ZCheckBoxColumnStyleInfo();
			}
			else if (propertyType == typeof(ZDateTime))
			{
				info = new ZDateEditColumnStyleInfo();
			}
			else if (propertyType == typeof(ZDecimal))
			{
				info = new ZCalcEditColumnStyleInfo();
			}
			else if (propertyType == typeof(ZInt))
			{
				info = new ZCalcEditColumnStyleInfo();
				((ZCalcEditColumnStyleInfo)info).Decimals = 0;
			}
			else
			{
				info = new ZTextBoxColumnStyleInfo();
			}

			var descriptor = new CustomGridPropertyDescriptor(customBizObjGetter, property.Identifier, propertyType);
			((IOverridablePropertyDescriptor)info).PropertyDescriptor = descriptor;
			info.ColumnName = descriptor.Name;
			info.Caption = property.Info.GetCaption() ?? property.Identifier;
			info.IsVisible = GetIsVisible(property.Info);

			return info;
		}

		static bool GetIsVisible(DynamicBusinessObjectProperty property)
		{
			DynamicMetaData metadata = property.GetMetaData(MetaDataTypes.Visible);
			if (metadata != null)
			{
				return (bool)metadata.Value;
			}

			return true;
		}

		#endregion
	}
}
