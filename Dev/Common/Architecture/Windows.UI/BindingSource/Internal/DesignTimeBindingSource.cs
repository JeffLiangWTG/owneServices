using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common.Collections;

namespace CargoWise.Windows.UI
{
	[DesignTimeVisible(false)]
	internal class DesignTimeBindingSource : FixedDotNetBindingSource
	{
		public static Type GetTypeToUseAsDataSource(Type type)
		{
			Type result = type;
			if (typeof(IList).IsAssignableFrom(type))
			{
				Type elementType = ListUtil.GetListElementType(type);
				result = elementType == null ? typeof(BindingList<>).MakeGenericType(type) : GetTypeToUseAsDataSource(elementType);
			}
			return result;
		}

		public override CurrencyManager GetRelatedCurrencyManager(string dataMember)
		{
			CurrencyManager result = null;
			if (string.IsNullOrEmpty(dataMember))
			{
				result = base.GetRelatedCurrencyManager(dataMember);
			}
			else if (dataMember.IndexOf('.') == -1)
			{
				PropertyDescriptor property = GetItemProperties(null)[dataMember];
				if (property != null)
				{
					DesignTimeBindingSource relatedSource = new DesignTimeBindingSource();
					relatedSource.DataSource = GetTypeToUseAsDataSource(property.PropertyType);
					result = relatedSource.CurrencyManager;
				}
			}
			else
			{
				BindingMemberInfo info = new BindingMemberInfo(dataMember);
				CurrencyManager parentCM = GetRelatedCurrencyManager(info.BindingPath);
				BindingSource parentSource = (BindingSource)parentCM.List;
				result = parentSource.GetRelatedCurrencyManager(info.BindingField);
			}
			return result;
		}
	}
}
