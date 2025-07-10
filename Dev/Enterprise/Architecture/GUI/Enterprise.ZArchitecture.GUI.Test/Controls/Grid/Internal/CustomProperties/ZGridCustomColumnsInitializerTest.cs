using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZGridCustomColumnsInitializerTest : TestCaseWithFactory
	{
		public void TestAddZTextBoxColumnStyleInfo()
		{
			AssertAddZGridColumnStyleInfo(typeof(ZString), typeof(ZTextBoxColumnStyleInfo));
			AssertAddZGridColumnStyleInfo(typeof(string), typeof(ZTextBoxColumnStyleInfo));
			AssertAddZGridColumnStyleInfo(typeof(int), typeof(ZTextBoxColumnStyleInfo));
			AssertAddZGridColumnStyleInfo(typeof(object), typeof(ZTextBoxColumnStyleInfo));
		}

		public void TestAddZCalcEditColumnStyleInfo()
		{
			AssertAddZGridColumnStyleInfo(typeof(ZInt), typeof(ZCalcEditColumnStyleInfo),
				columnInfo => AssertEquals(0, ((ZCalcEditColumnStyleInfo)columnInfo).Decimals));
			AssertAddZGridColumnStyleInfo(typeof(ZShort), typeof(ZCalcEditColumnStyleInfo),
				columnInfo => AssertEquals(0, ((ZCalcEditColumnStyleInfo)columnInfo).Decimals));
			AssertAddZGridColumnStyleInfo(typeof(ZDecimal), typeof(ZCalcEditColumnStyleInfo),
				columnInfo => AssertEquals(2, ((ZCalcEditColumnStyleInfo)columnInfo).Decimals));
			AssertAddZGridColumnStyleInfo(typeof(ZByte), typeof(ZCalcEditColumnStyleInfo),
				columnInfo => AssertEquals(0, ((ZCalcEditColumnStyleInfo)columnInfo).Decimals));
		}

		public void TestAddColumnsOfMixedVisibility()
		{
			var dummies = new DummyBusinessObjectCollection(Factory);
			var visibleCustomProperty = new CustomPropertyImplementation<BusinessObject>("abc", "abc", typeof(ZString));
			var invisibleCustomProperty = new CustomPropertyImplementation<BusinessObject>("def", "def", typeof(ZString), visible: false);

			using (var grid = new ZGrid())
			{
				AssertEquals(0, grid.ColumnStyles.Count);
				new ZGridCustomColumnsInitializer(grid, dummies, new ResourceStringData("", "Custom Columns"), isVisible: true)
					.AddCustomColumns(new[] { visibleCustomProperty, invisibleCustomProperty });
				AssertEquals(2, grid.ColumnStyles.Count);

				var visibleCustomPropertyColumnInfo = grid.ColumnStyles.Cast<ZGridColumnInfo>().First(c => c.ColumnName == "abc");
				var invisibleCustomPropertyColumnInfo = grid.ColumnStyles.Cast<ZGridColumnInfo>().First(c => c.ColumnName == "def");
				Assert("If the custom properties are set to visible, inidividual properties visibility should be honoured.", visibleCustomPropertyColumnInfo.IsVisible);
				Assert("If the custom properties are set to visible, inidividual properties visibility should be honoured.", !invisibleCustomPropertyColumnInfo.IsVisible);
			}

			using (var grid = new ZGrid())
			{
				AssertEquals(0, grid.ColumnStyles.Count);
				new ZGridCustomColumnsInitializer(grid, dummies, new ResourceStringData("", "Custom Columns"), isVisible: false)
					.AddCustomColumns(new[] { visibleCustomProperty, invisibleCustomProperty });
				AssertEquals(2, grid.ColumnStyles.Count);

				var visibleCustomPropertyColumnInfo = grid.ColumnStyles.Cast<ZGridColumnInfo>().First(c => c.ColumnName == "abc");
				var invisibleCustomPropertyColumnInfo = grid.ColumnStyles.Cast<ZGridColumnInfo>().First(c => c.ColumnName == "def");
				Assert("If the custom properties are set to invisible, all properties should be invisible.", !visibleCustomPropertyColumnInfo.IsVisible);
				Assert("If the custom properties are set to invisible, all properties should be invisible.", !invisibleCustomPropertyColumnInfo.IsVisible);
			}
		}

		public void TestAddZDateEditColumnStyleInfo()
		{
			AssertAddZGridColumnStyleInfo(typeof(ZDateTime), typeof(ZDateEditColumnStyleInfo),
				columnInfo => AssertEquals(ZDateTimePickerFormat.Short, ((ZDateEditColumnStyleInfo)columnInfo).DateTimeFormat));

			var dynamicProperty = new DynamicBusinessObjectProperty(typeof(ZDateTime), true, metaData: DynamicMetaData.DateTimeFormat(KDateTimeFormat.Long));
			AssertAddZGridColumnStyleInfo(dynamicProperty, typeof(ZDateEditColumnStyleInfo),
				columnInfo => AssertEquals(ZDateTimePickerFormat.Long, ((ZDateEditColumnStyleInfo)columnInfo).DateTimeFormat));

			dynamicProperty = new DynamicBusinessObjectProperty(typeof(ZDateTime), true, metaData: DynamicMetaData.DateTimeFormat(KDateTimeFormat.Short));
			AssertAddZGridColumnStyleInfo(dynamicProperty, typeof(ZDateEditColumnStyleInfo),
				columnInfo => AssertEquals(ZDateTimePickerFormat.Short, ((ZDateEditColumnStyleInfo)columnInfo).DateTimeFormat));

			dynamicProperty = new DynamicBusinessObjectProperty(typeof(ZDateTime), true, metaData: DynamicMetaData.DateTimeFormat(KDateTimeFormat.Time));
			AssertAddZGridColumnStyleInfo(dynamicProperty, typeof(ZDateEditColumnStyleInfo),
				columnInfo => AssertEquals(ZDateTimePickerFormat.Time, ((ZDateEditColumnStyleInfo)columnInfo).DateTimeFormat));
		}

		public void TestAddZDateOffsetEditColumnStyleInfo()
		{
			AssertAddZGridColumnStyleInfo(typeof(ZDateTimeOffset), typeof(ZDateTimeOffsetEditColumnStyleInfo),
				columnInfo => AssertEquals(ZDateTimePickerFormat.Short, ((ZDateTimeOffsetEditColumnStyleInfo)columnInfo).DateTimeFormat));

			var dynamicProperty = new DynamicBusinessObjectProperty(typeof(ZDateTimeOffset), true, metaData: DynamicMetaData.DateTimeFormat(KDateTimeFormat.Long));
			AssertAddZGridColumnStyleInfo(dynamicProperty, typeof(ZDateTimeOffsetEditColumnStyleInfo),
				columnInfo => AssertEquals(ZDateTimePickerFormat.Long, ((ZDateTimeOffsetEditColumnStyleInfo)columnInfo).DateTimeFormat));

			dynamicProperty = new DynamicBusinessObjectProperty(typeof(ZDateTimeOffset), true, metaData: DynamicMetaData.DateTimeFormat(KDateTimeFormat.Short));
			AssertAddZGridColumnStyleInfo(dynamicProperty, typeof(ZDateTimeOffsetEditColumnStyleInfo),
				columnInfo => AssertEquals(ZDateTimePickerFormat.Short, ((ZDateTimeOffsetEditColumnStyleInfo)columnInfo).DateTimeFormat));

			dynamicProperty = new DynamicBusinessObjectProperty(typeof(ZDateTimeOffset), true, metaData: DynamicMetaData.DateTimeFormat(KDateTimeFormat.Time));
			AssertAddZGridColumnStyleInfo(dynamicProperty, typeof(ZDateTimeOffsetEditColumnStyleInfo),
				columnInfo => AssertEquals(ZDateTimePickerFormat.Time, ((ZDateTimeOffsetEditColumnStyleInfo)columnInfo).DateTimeFormat));
		}

		public void TestAddZCheckBoxColumnStyleInfo()
		{
			AssertAddZGridColumnStyleInfo(typeof(ZBool), typeof(ZCheckBoxColumnStyleInfo));
		}

		public void TestAddZMultiControlColumnStyleInfo()
		{
			var dynamicProperty = new DynamicBusinessObjectProperty(typeof(ZString), true, metaData: DynamicMetaData.ListDataSource(null));
			AssertAddZGridColumnStyleInfo(dynamicProperty, typeof(ZMultiControlColumnStyleInfo));
		}

		#region Implementation

		void AssertAddZGridColumnStyleInfo(Type propetyType, Type expecedColumnInfoType, Action<ZTextBoxColumnStyleInfo> addtionalChekc = null)
		{
			AssertAddZGridColumnStyleInfo(new CustomPropertyImplementation<BusinessObject>("abc.", "Abc", propetyType), expecedColumnInfoType, "Abc", addtionalChekc);
		}

		void AssertAddZGridColumnStyleInfo(DynamicBusinessObjectProperty dynamicProperty, Type expecedColumnInfoType, Action<ZTextBoxColumnStyleInfo> addtionalChekc = null)
		{
			AssertAddZGridColumnStyleInfo(new CustomPropertyImplementation<BusinessObject>("abc.", dynamicProperty), expecedColumnInfoType, "abc.", addtionalChekc);
		}

		void AssertAddZGridColumnStyleInfo(ICustomProperty customProperty, Type expecedColumnInfoType, string expectedCaption, Action<ZTextBoxColumnStyleInfo> addtionalChekc)
		{
			var dummies = new DummyBusinessObjectCollection(Factory);
			using (var grid = new ZGrid())
			{
				AssertEquals(0, grid.ColumnStyles.Count);

				new ZGridCustomColumnsInitializer(grid, dummies, new ResourceStringData("", "Custom Columns")).AddCustomColumns(new[] { customProperty });

				AssertEquals(1, grid.ColumnStyles.Count);
				AssertEquals(expecedColumnInfoType, grid.ColumnStyles[0].GetType());

				var columnInfo = (ZTextBoxColumnStyleInfo)grid.ColumnStyles[0];

				AssertEquals("abc_", columnInfo.ColumnName);
				AssertEquals(expectedCaption, columnInfo.Caption);
				AssertEquals("Custom Columns", columnInfo.GroupName.Caption);
				AssertEquals(false, columnInfo.IsSubmissive);
				Assert(!columnInfo.IsVisible);
				Assert(columnInfo.IsReadOnly);
				Assert(columnInfo.IsCustomColumn);
				AssertEquals(typeof(ZCustomPropertyDescriptor), ((IOverridablePropertyDescriptor)columnInfo).PropertyDescriptor.GetType());

				if (addtionalChekc != null)
				{
					addtionalChekc(columnInfo);
				}
			}
		}

		#endregion
	}
}
