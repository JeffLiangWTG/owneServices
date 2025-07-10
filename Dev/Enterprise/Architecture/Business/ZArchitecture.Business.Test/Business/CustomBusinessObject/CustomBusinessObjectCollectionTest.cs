using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(CustomBusinessObjectCollection))]
	sealed class CustomBusinessObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CustomBusinessObjectCollection>
	{
		public void TestSort()
		{
			var collection = GetCollectionToTest();
			for (int i = 0; i < 26; i++)
			{
				var bizObj = collection.AddNew();
				bizObj["ZZZ_String"] = new ZString((char)('Z' - i));
				bizObj["ZZZ_Decimal"] = new ZDecimal(i);
			}

			collection.Sort("ZZZ_String");
			AssertEquals("A", collection[0]["ZZZ_String"]);
			AssertEquals("Z", collection[25]["ZZZ_String"]);

			collection.Sort("ZZZ_Decimal");
			AssertEquals("Z", collection[0]["ZZZ_String"]);
			AssertEquals("A", collection[25]["ZZZ_String"]);
		}

		protected override CustomBusinessObjectCollection GetCollectionToTest()
		{
			return new CustomBusinessObjectCollection(Factory, Properties);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CustomBusinessObject(Factory, null, Properties);
		}

		CustomPropertyCollectionImpl Properties
		{
			get
			{
				if (properties == null)
				{
					var values = new Dictionary<ZGuid, Dictionary<string, object>>();
					properties = new CustomPropertyCollectionImpl(
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
						{ typeof(ZString), "ZZZ_String", DynamicMetaData.MaxLength(Int32.MaxValue) },
						{ typeof(ZDecimal), "ZZZ_Decimal" },
						{ typeof(ZDateTime), "ZZZ_DateTime" },
						{ typeof(ZDateTimeOffset), "ZZZ_DateTimeOffset" },
						{ typeof(ZGeography), "ZZZ_Geography" },
						{ typeof(ZBool), "ZZZ_Bool" },
					};
				}

				return properties;
			}
		}
		CustomPropertyCollectionImpl properties;
	}
}
