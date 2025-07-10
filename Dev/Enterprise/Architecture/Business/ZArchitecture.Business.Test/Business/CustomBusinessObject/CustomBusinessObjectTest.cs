using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public abstract class CustomBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestUnRegisterCustomBusinessObject()
		{
			var bo = Factory.New<CustomFieldProvider>();
			var cusObj = bo.GetCustomBusinessObject(true);
			bo.RegisterEditableChildObject(cusObj);
			AssertEquals(cusObj, ((IBusiness)bo).Children.OfType<CustomBusinessObject>().FirstOrDefault());
			bo.UnRegisterCustomBusinessObject();
			AssertNull(((IBusiness)bo).Children.OfType<CustomBusinessObject>().FirstOrDefault());
		}

		public void TestICustomFieldProvider()
		{
			var cusObj = (ICustomFieldProvider)GetNewBusinessObject();
			AssertEquals(cusObj, cusObj.GetCustomBusinessObject());
		}

		public void TestPropertyNames()
		{
			var cusObj = (IDynamicBusinessObject)GetNewBusinessObject();

			var propertyNames = cusObj.PropertyNames;
			AssertEquals(12, propertyNames.Length);
			Assert(propertyNames.Contains("ZZZ_String"));
			Assert(propertyNames.Contains("ZZZ_Decimal"));
			Assert(propertyNames.Contains("ZZZ_DateTime"));
			Assert(propertyNames.Contains("ZZZ_DateTimeOffset"));
			Assert(propertyNames.Contains("ZZZ_Geography"));
			Assert(propertyNames.Contains("ZZZ_Bool"));
			Assert(propertyNames.Contains("ZZZ_StringInfo"));
			Assert(propertyNames.Contains("ZZZ_DecimalInfo"));
			Assert(propertyNames.Contains("ZZZ_DateTimeInfo"));
			Assert(propertyNames.Contains("ZZZ_DateTimeOffsetInfo"));
			Assert(propertyNames.Contains("ZZZ_GeographyInfo"));
			Assert(propertyNames.Contains("ZZZ_BoolInfo"));
		}

		public void TestGetPropertyType()
		{
			var cusObj = (IDynamicBusinessObject)GetNewBusinessObject();

			AssertEquals(typeof(ZString), cusObj.GetProperty("ZZZ_String").Type);
			AssertEquals(typeof(ZDecimal), cusObj.GetProperty("ZZZ_Decimal").Type);
			AssertEquals(typeof(ZDateTime), cusObj.GetProperty("ZZZ_DateTime").Type);
			AssertEquals(typeof(ZDateTimeOffset), cusObj.GetProperty("ZZZ_DateTimeOffset").Type);
			AssertEquals(typeof(ZGeography), cusObj.GetProperty("ZZZ_Geography").Type);
			AssertEquals(typeof(ZBool), cusObj.GetProperty("ZZZ_Bool").Type);
			AssertNull(cusObj.GetProperty("!@#"));
			AssertNull(cusObj.GetProperty(null));
		}

		public void TestGetPropertyVisibility()
		{
			var cusObj = (IDynamicBusinessObject)GetNewBusinessObject();

			AssertEquals("Properties should be visible by default.", true, cusObj.GetProperty("ZZZ_String").Visible);
			AssertEquals("Properties should be visible by default.", true, cusObj.GetProperty("ZZZ_Decimal").Visible);
			AssertEquals("Properties should be visible by default.", true, cusObj.GetProperty("ZZZ_DateTime").Visible);
			AssertEquals("Properties should be visible by default.", true, cusObj.GetProperty("ZZZ_DateTimeOffset").Visible);
			AssertEquals("Properties should be visible by default.", true, cusObj.GetProperty("ZZZ_Geography").Visible);
			AssertEquals("Properties should be visible by default.", true, cusObj.GetProperty("ZZZ_Bool").Visible);
		}

		public void TestValue()
		{
			BusinessObject cusObj = GetNewBusinessObject();

			cusObj["ZZZ_String"] = "Test!";
			cusObj["ZZZ_Decimal"] = 3.9m;
			cusObj["ZZZ_DateTime"] = new DateTime(2000, 11, 2);
			cusObj["ZZZ_DateTimeOffset"] = new DateTimeOffset(2000, 11, 2, 3, 4, 5, TimeSpan.FromHours(1));
			cusObj["ZZZ_Geography"] = Microsoft.SqlServer.Types.SqlGeography.STGeomFromText(new SqlChars("POINT (-121 48)"), 4326);
			cusObj["ZZZ_Bool"] = true;

			AssertEquals("Test!", cusObj["ZZZ_String"]);
			AssertEquals(3.9m, cusObj["ZZZ_Decimal"]);
			AssertEquals(new ZDateTime(2000, 11, 2), cusObj["ZZZ_DateTime"]);
			AssertEquals(new ZDateTimeOffset(2000, 11, 2, 3, 4, 5, TimeSpan.FromHours(1)), cusObj["ZZZ_DateTimeOffset"]);
			AssertEquals(new ZGeography("-121 48"), cusObj["ZZZ_Geography"]);
			AssertEquals(ZBool.True, cusObj["ZZZ_Bool"]);
		}

		public void TestMetaData()
		{
			var values = new Dictionary<string, object>();

			var propertyCollection = new CustomPropertyCollectionImpl(
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
				{ typeof(ZString), "ZZZ_String", DynamicMetaData.ListDataSource(new string[] { "A" }), DynamicMetaData.MaxLength(20) },
				{ typeof(ZDecimal), "ZZZ_Decimal", DynamicMetaData.ReadOnly(true), DynamicMetaData.DecimalPlaces(1) },
				{ typeof(ZDateTime), "ZZZ_DateTime", DynamicMetaData.DateTimeFormat(KDateTimeFormat.Custom), DynamicMetaData.DateTimeCustomFormat("YYYY") },
				{ typeof(ZBool), "ZZZ_Bool", DynamicMetaData.Description(new SimpleDescription("Boolean property")) },
			};

			var cusObj = GetCustomBusinessObject(propertyCollection);

			AssertEquals(false, cusObj.GetZPropertyInfo("ZZZ_String").ReadOnly);
			AssertEquals(20, cusObj.GetZPropertyInfo("ZZZ_String").MaxLength);
			AssertEquals("A", MetaData.GetListDataSource(cusObj, cusObj.GetZPropertyInfo("ZZZ_String").PropertyDescriptor).OfType<string>().ElementAt(0));

			AssertEquals(true, cusObj.GetZPropertyInfo("ZZZ_Decimal").ReadOnly);
			AssertEquals(1, MetaData.GetDecimalPlaces(cusObj, cusObj.GetZPropertyInfo("ZZZ_Decimal").PropertyDescriptor));

			AssertEquals(KDateTimeFormat.Custom, MetaData.GetMetaData(cusObj, cusObj.GetZPropertyInfo("ZZZ_DateTime").PropertyDescriptor, MetaDataTypes.DateTimeFormat));
			AssertEquals("YYYY", MetaData.GetMetaData(cusObj, cusObj.GetZPropertyInfo("ZZZ_DateTime").PropertyDescriptor, MetaDataTypes.DateTimeCustomFormat));

			AssertNull(MetaData.GetDescription(cusObj, cusObj.GetZPropertyInfo("ZZZ_String").PropertyDescriptor));
			AssertEquals("Boolean property", MetaData.GetDescription(cusObj, cusObj.GetZPropertyInfo("ZZZ_Bool").PropertyDescriptor).GetDescription(0));
		}

		public void TestHasChanges()
		{
			BusinessObject cusObj = GetNewBusinessObject();
			Assert(!cusObj.HasChanges);
			cusObj["ZZZ_String"] = "Test!";
			Assert(cusObj.HasChanges);
			Factory.Save();
			Assert(!cusObj.HasChanges);

			cusObj["ZZZ_String"] = "Test!";
			Assert(!cusObj.HasChanges);
		}

		public void TestICustomPropertyContainer()
		{
			var customBusinessObject = (CustomBusinessObject)GetNewBusinessObject();
			List<ICustomProperty> customProperties = new List<ICustomProperty>(((ICustomPropertyContainer)customBusinessObject).CustomProperties);

			AssertEquals(((IDynamicBusinessObject)customBusinessObject).PropertyNames.Length / 2, customProperties.Count);
			for (int i = 0; i < customProperties.Count; i++)
			{
				AssertEquals(((IDynamicBusinessObject)customBusinessObject).PropertyNames[i], customProperties[i].Identifier);
			}
		}

		public void TestGetContextIDHasNoProperty()
		{
			var customBusinessObject = (CustomBusinessObject)GetNewBusinessObject();
			AssertEquals("Blah", ((ICustomTextTemplateContext)customBusinessObject).GetTextTemplateContextID(customBusinessObject, new KBindingMemberInfo("Blah")));
			var result = customBusinessObject.GetPropertyAndDescription("Blah");

			AssertEquals(null, result.Property);
			AssertEquals(null, result.Description);
		}

		public void TestGetContextIDHasNoDescription()
		{
			var values = new Dictionary<string, object>();

			var propertyCollection = new CustomPropertyCollectionImpl(
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
				{ typeof(ZBool), "ZZZ_Bool", DynamicMetaData.ReadOnly(true) },
			};

			var customBusinessObject = GetCustomBusinessObject(propertyCollection);
			customBusinessObject["ZZZ_Bool"] = false;

			AssertEquals("ZZZ_Bool", ((ICustomTextTemplateContext)customBusinessObject).GetTextTemplateContextID(customBusinessObject, new KBindingMemberInfo("ZZZ_Bool")));

			var result = customBusinessObject.GetPropertyAndDescription("ZZZ_Bool");

			AssertEquals(true, result.Property != null);
			AssertEquals(null, result.Description);
		}

		public void TestGetAlternateTextTemplateContextIDs()
		{
			var values = new Dictionary<string, object>();
			var customPropertyName = "Boolean pro.+perty";
			var customPropertyIdentifier = CustomPropertyHelper.GeneratePropertyIdentifier(customPropertyName, typeof(ZBool));

			var propertyCollection = new CustomPropertyCollectionImpl(
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
				{ typeof(ZBool), customPropertyIdentifier, DynamicMetaData.Description(new SimpleDescription(customPropertyName)) },
			};

			var customBusinessObject = GetCustomBusinessObject(propertyCollection);
			customBusinessObject[customPropertyIdentifier] = false;

			AssertEquals(customPropertyIdentifier, ((ICustomTextTemplateContext)customBusinessObject).GetTextTemplateContextID(customBusinessObject, new KBindingMemberInfo(customPropertyIdentifier)));
			var alternateContextIds = ((ICustomTextTemplateAlternateContexts)customBusinessObject).GetAlternateTextTemplateContextIDs(customBusinessObject, new KBindingMemberInfo(customPropertyIdentifier)).ToList();
			AssertEquals("." + CustomBusinessObject.GetLegacyIdentifier1(customPropertyName, typeof(ZBool)), alternateContextIds[0]);
			AssertEquals("." + CustomBusinessObject.GetLegacyIdentifier2(customPropertyName, typeof(ZBool)), alternateContextIds[1]);
		}

		class SimpleDescription : IDescription
		{
			public SimpleDescription(string description)
			{
				this.description = description;
			}

			public int Count
			{
				get { return 1; }
			}

			public string GetDescription(int index, System.Globalization.CultureInfo culture)
			{
				return description;
			}

			public string GetDescription(int index)
			{
				return description;
			}

			readonly string description;
		}

		protected virtual CustomBusinessObject GetCustomBusinessObject(CustomPropertyCollectionImpl propertyCollection)
		{
			return new CustomBusinessObject(Factory, null, propertyCollection);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var values = new Dictionary<string, object>();

			var propertyCollection = new CustomPropertyCollectionImpl(
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
				{ typeof(ZString), "ZZZ_String", DynamicMetaData.MaxLength(Int32.MaxValue) },
				{ typeof(ZDecimal), "ZZZ_Decimal" },
				{ typeof(ZDateTime), "ZZZ_DateTime" },
				{ typeof(ZDateTimeOffset), "ZZZ_DateTimeOffset" },
				{ typeof(ZGeography), "ZZZ_Geography" },
				{ typeof(ZBool), "ZZZ_Bool" },
			};

			return GetCustomBusinessObject(propertyCollection);
		}

		class CustomFieldProvider : DummyBusinessObject, ICustomFieldProvider
		{
			public CustomFieldProvider(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public CustomBusinessObject GetCustomBusinessObject(bool shouldRefresh = false)
			{
				if (customBizo == null || shouldRefresh)
				{
					customBizo = new CustomBusinessObject(Factory, this, null);
				}
				return customBizo;
			}
			CustomBusinessObject customBizo;
		}
	}
}
