using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	sealed class CustomDummyChildBusinessObject : DummyChildBusinessObject, ICustomFieldProvider
	{
		public CustomDummyChildBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public ZString __customFieldString__prop__ZString { get; set; }
		public ZDecimal __customFieldDecimal__prop__ZDecimal { get; set; }
		public ZDateTime __customFieldDatetime__prop__ZDateTime { get; set; }
		public ZInt __customFieldInteger__prop__ZInt { get; set; }
		public ZBool __customFieldBoolean__prop__ZBool { get; set; }

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			if (customBusinessObject == null || shouldRefresh)
			{
				customBusinessObject = new CustomBusinessObject(Factory, this, CustomProperties);
			}
			return customBusinessObject;
		}
		CustomBusinessObject customBusinessObject;

		CustomPropertyCollectionImpl CustomProperties
		{
			get
			{
				if (customProperties == null)
				{
					var values = new Dictionary<ZGuid, Dictionary<string, object>>();
					customProperties = new CustomPropertyCollectionImpl(
					(cusObj, propertyName) =>
					{
						Dictionary<string, object> propValues;
						if (values.TryGetValue(cusObj.PK, out propValues))
						{
							object value;
							return propValues.TryGetValue(propertyName, out value) ? value : null;
						}
						else
						{
							return null;
						}
					},
					(cusObj, propertyName, value) =>
					{
						Dictionary<string, object> propValues;
						if (!values.TryGetValue(cusObj.PK, out propValues))
						{
							propValues = new Dictionary<string, object>();
							values.Add(cusObj.PK, propValues);
						}
						propValues[propertyName] = value;
						return true;
					})
					{
						{ typeof(ZString), "__customFieldString__prop__ZString", DynamicMetaData.MaxLength(int.MaxValue) },
						{ typeof(ZDecimal), "__customFieldDecimal__prop__ZDecimal", DynamicMetaData.DecimalPlaces(2) },
						{ typeof(ZDateTime), "__customFieldDatetime__prop__ZDateTime" },
						{ typeof(ZInt), "__customFieldInteger__prop__ZInt" },
						{ typeof(ZBool), "__customFieldBoolean__prop__ZBool" },
					};
				}
				return customProperties;
			}
		}
		CustomPropertyCollectionImpl customProperties;
	}
}
