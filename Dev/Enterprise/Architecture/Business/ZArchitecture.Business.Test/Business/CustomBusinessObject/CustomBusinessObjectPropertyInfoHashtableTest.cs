using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class CustomBusinessObjectPropertyInfoHashtableTest : TestCaseWithFactory
	{
		public void TestPropertyInfoHashtable()
		{
			CustomBusinessObject cusObj = GetCustomBusinessObject();

			CustomBusinessObjectPropertyInfoHashtable propertyInfoHashtable = cusObj.ZPropertyInfoHash as CustomBusinessObjectPropertyInfoHashtable;
			AssertNotNull(propertyInfoHashtable);

			AssertEquals(6, propertyInfoHashtable.Count);
			Assert(propertyInfoHashtable.ContainsKey("ZZZ_String"));
			Assert(!propertyInfoHashtable.ContainsKey("ZZZ_StringInfo"));
			AssertEquals(6, propertyInfoHashtable.GetPropertyInfos(PropertyInfoTypes.NonWrapping).OfType<ZPropertyInfo>().Count());
			AssertEquals(0, propertyInfoHashtable.GetPropertyInfos(PropertyInfoTypes.Wrapping).OfType<ZPropertyInfo>().Count());
			AssertNotNull(propertyInfoHashtable.GetPropertySafe("ZZZ_String"));
			AssertNull(propertyInfoHashtable.GetPropertySafe("ZZZ_StringInfo"));
		}

		public void TestParentCustomBizoIsWeaklyReferenced()
		{
			IDictionary<string, PropertyDescriptor> propertyDescriptors = GetPropertyInfoHashtableForTest();
			AssertNotNull(propertyDescriptors);

			Assert(propertyDescriptors.ContainsKey("ZZZ_String"));
			AssertEquals(6, propertyDescriptors.Count);

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			Assert(!propertyDescriptors.ContainsKey("ZZZ_String"));
			AssertEquals(0, propertyDescriptors.Count);
		}

		public void TestNonInfoFieldEndingInInfo()
		{
			CustomBusinessObject cusObj = GetCustomBusinessObject();

			CustomBusinessObjectPropertyInfoHashtable propertyInfoHashtable = cusObj.ZPropertyInfoHash as CustomBusinessObjectPropertyInfoHashtable;
			AssertNotNull(propertyInfoHashtable);

			Assert(propertyInfoHashtable.ContainsKey("ZZZ_Info"));
			Assert(!propertyInfoHashtable.ContainsKey("ZZZ_InfoInfo"));
			propertyInfoHashtable.GetPropertySafe("ZZZ_Info");
			AssertNoExceptionThrown(delegate
			{
				AssertEquals(6, propertyInfoHashtable.GetPropertyInfos(PropertyInfoTypes.NonWrapping).OfType<ZPropertyInfo>().Count());
			});
			AssertNotNull(propertyInfoHashtable.GetPropertyInfos(PropertyInfoTypes.NonWrapping).OfType<ZPropertyInfo>().Where(x => x.Name.Equals("ZZZ_InfoInfo")));
		}

		IDictionary<string, PropertyDescriptor> GetPropertyInfoHashtableForTest()
		{
			CustomBusinessObject cusObj = GetCustomBusinessObject(new BusinessObjectFactory());
			return ((CustomBusinessObjectPropertyInfoHashtable)cusObj.ZPropertyInfoHash).GetNewPropertyNamesToPropertyDescriptorsDictionaryForTest();
		}

		CustomBusinessObject GetCustomBusinessObject()
		{
			return GetCustomBusinessObject(Factory);
		}

		CustomBusinessObject GetCustomBusinessObject(BusinessObjectFactory factory)
		{
			return new CustomBusinessObject(factory, null, new CustomPropertyCollectionImpl(
				propertyName =>
				{
					object value;
					return values.TryGetValue(propertyName, out value) ? value : null;
				},
				(propertyName, value) =>
				{
					values[propertyName] = value;
					return true;
				})
			{
				{ typeof(ZString), "ZZZ_String" },
				{ typeof(ZDecimal), "ZZZ_Decimal" },
				{ typeof(ZDateTime), "ZZZ_DateTime" },
				{ typeof(ZDateTimeOffset), "ZZZ_DateTimeOffset" },
				{ typeof(ZGeography), "ZZZ_Geography" },
				{ typeof(ZBool), "ZZZ_Info" },
			});
		}

		readonly Dictionary<string, object> values = new Dictionary<string, object>();
	}
}
