using System;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.Integration;
using CargoWise.Types;
using Microsoft.CSharp.RuntimeBinder;

namespace CargoWise.EntityFramework
{
	public class ZGuidPropertyComparer : PropertyComparer
	{
		public ZGuidPropertyComparer(PropertyDescriptor propertyDescriptor, ListSortDirection direction, string listProviderPropertyName)
			: base(propertyDescriptor, direction)
		{
			Argument.NotNull(listProviderPropertyName, "listProviderPropertyName");
			this.isPropertyTypeZString = propertyDescriptor.PropertyType == typeof(ZString);
			this.ListProviderPropertyName = listProviderPropertyName;
		}

		internal protected override IComparable GetPropertyValueFromObject(BusinessObject businessObject)
		{
			object result = base.GetPropertyValueFromObject(businessObject);

			ZGuid zguid;
			if (!ZGuid.TryParse(result, out zguid))
			{
				result = result.ToString();
			}
			else
			{
				var propertyValue = businessObject.GetPropertyValueByName(ListProviderPropertyName);
				var listProvider = propertyValue as IFindBoxListProvider;
				if (listProvider != null)
				{
					result = listProvider.CodeFromPrimaryKey(zguid);
				}
				else if (zguid.IsValid && propertyValue != null)
				{
					var guid = zguid.ToGuid();
					var codeDescriptionPairListWithIndexer = propertyValue as ICodeDescriptionPairListIndexer;
					if (codeDescriptionPairListWithIndexer != null)
					{
						var codeDescriptionPair = codeDescriptionPairListWithIndexer[guid];
						if (codeDescriptionPair != null)
						{
							result = (ZString)codeDescriptionPair.Code;
						}
						else
						{
							result = ZString.Empty;
						}
					}
					else
					{
						try
						{
							dynamic maybeCodeDescriptionPairList = propertyValue;
							result = (ZString)maybeCodeDescriptionPairList[guid].Code;
						}
						catch (RuntimeBinderException)
						{
							// wasn't a CodeDescriptionPairList
						}
					}
				}
			}

			return (isPropertyTypeZString ? new ZString(result) : result is string ? (IComparable)result : "");
		}

		public override bool Equals(object obj)
		{
			ZGuidPropertyComparer rhs = obj as ZGuidPropertyComparer;
			return
				rhs != null &&
				base.Equals(rhs) &&
				ListProviderPropertyName == rhs.ListProviderPropertyName;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required for Equals override")]
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		readonly bool isPropertyTypeZString;
		readonly string ListProviderPropertyName;
	}
}
