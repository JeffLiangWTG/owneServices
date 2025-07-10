using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class CustomGridPropertiesBuilderTest : TestCase
	{
		public void TestAdd()
		{
			Dictionary<string, object> values = new Dictionary<string, object>();
			CustomPropertyCollection propertyCollection = new CustomPropertyCollectionImpl(
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
				{ typeof(ZBool), "ZZZ_Bool", DynamicMetaData.Position(1) },
				{ typeof(ZDateTime), "ZZZ_DateTime", DynamicMetaData.Position(5), DynamicMetaData.Description(new SimpleDescription("DateTime")) },
				{ typeof(ZDecimal), "ZZZ_Decimal", DynamicMetaData.Position(4), DynamicMetaData.Description(new SimpleDescription("Decimal")) },
				{ typeof(ZInt), "ZZZ_Int", DynamicMetaData.Position(2), DynamicMetaData.Visible(false) },
				{ typeof(ZString), "ZZZ_String", DynamicMetaData.Position(3), DynamicMetaData.Description(new SimpleDescription("String")), DynamicMetaData.Visible(true) },
			};

			CustomBusinessObject customBizObj = new CustomBusinessObject(null, propertyCollection);

			using (var grid = new ZGrid())
			{
				var builder = new CustomGridPropertiesBuilder(grid);
				builder.Add(x => ((BusinessObjectWithCustomBusinessObject)x).CustomBusinessObject, propertyCollection);

				AssertColumnInfoProperties((ZGridColumnInfo)grid.ColumnStyles[0],
					typeof(ZCheckBoxColumnStyleInfo),
					"ZZZ_Bool",
					"ZZZ_Bool",
					true);

				AssertColumnInfoProperties((ZGridColumnInfo)grid.ColumnStyles[1],
					typeof(ZCalcEditColumnStyleInfo),
					"ZZZ_Int",
					"ZZZ_Int",
					false);

				AssertColumnInfoProperties((ZGridColumnInfo)grid.ColumnStyles[2],
					typeof(ZTextBoxColumnStyleInfo),
					"ZZZ_String",
					"String",
					true);

				AssertColumnInfoProperties((ZGridColumnInfo)grid.ColumnStyles[3],
					typeof(ZCalcEditColumnStyleInfo),
					"ZZZ_Decimal",
					"Decimal",
					true);

				AssertColumnInfoProperties((ZGridColumnInfo)grid.ColumnStyles[4],
					typeof(ZDateEditColumnStyleInfo),
					"ZZZ_DateTime",
					"DateTime",
					true);
			}
		}

		void AssertColumnInfoProperties(ZGridColumnInfo columnInfo, Type expectedType, ZString expectedColumnName, ZString expectedCaption, ZBool expectedIsVisible)
		{
			CombineAssertions(() =>
			{
				AssertType("Type", expectedType, columnInfo);
				AssertEquals("ColumnName", expectedColumnName, columnInfo.ColumnName);
				AssertEquals("Caption", expectedCaption, columnInfo.Caption);
				AssertEquals("IsVisible", expectedIsVisible, columnInfo.IsVisible);
			});
		}
	}
}
