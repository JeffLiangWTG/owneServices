using System;
using System.Collections.Generic;

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.DataMapping
{
	public class ImportWizardPreviewLine : CustomBusinessObject
	{
		public ImportWizardPreviewLine(IEnumerable<Tuple<Type, string, DynamicMetaData[]>> properties)
			: base(null, GetProperties(properties))
		{
		}

		static ICustomPropertyCollection GetProperties(IEnumerable<Tuple<Type, string, DynamicMetaData[]>> propertyInfos)
		{
			DataBag properties = new DataBag();
			foreach (Tuple<Type, string, DynamicMetaData[]> propertyInfo in propertyInfos)
			{
				properties.Add(propertyInfo.Item1, propertyInfo.Item2, true, propertyInfo.Item3);
			}

			return properties;
		}

		class DataBag : CustomPropertyCollection
		{
			protected override object GetValueCore(BusinessObject cusObj, string propertyName)
			{
				object value;
				return propertyValues.TryGetValue(propertyName, out value) ? value : null;
			}

			protected override bool TrySetValueCore(BusinessObject cusObj, string propertyName, object value)
			{
				propertyValues[propertyName] = value;
				return true;
			}

			readonly Dictionary<string, object> propertyValues = new Dictionary<string, object>();
		}
	}
}
