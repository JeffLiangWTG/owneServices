using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZDropDownListColumnExcelExportTest : TestCaseWithFactory
	{
		public void TestGetValue()
		{
			DummyEnterpriseBusinessObject testBizOWithLookup = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			testBizOWithLookup.Z0_Description = "YYY";
			testBizOWithLookup.Lookups.DummyCodeList.AddPair("XXX", "Description1");
			testBizOWithLookup.Lookups.DummyCodeList.AddPair("YYY", "Description2");
			AssertEquals("Lookup list has two pairs", 4, testBizOWithLookup.Lookups.DummyCodeList.Count); //2DummyLookup + 2 new lookups(XXX and YYY)

			ZDropDownListColumn testColumnWithBindToList = new ZDropDownListColumn("Test Column", DummyBusinessObject.Schema.Z0_Description, "Lookups.DummyCodeList");
			AssertEquals("BindToList should be Lookups.DummyCodeList", "Lookups.DummyCodeList", testColumnWithBindToList.BindToList);

			testColumnWithBindToList.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AssertEquals("GetCustomValue should be return Description2", "Description2", testColumnWithBindToList.GetCustomValue(testBizOWithLookup));

			testColumnWithBindToList.DisplayStyle = OComboBoxDropDownStyle.CodeOnly;
			AssertEquals("GetCustomValue should be return YYY", "YYY", testColumnWithBindToList.GetCustomValue(testBizOWithLookup));

			testColumnWithBindToList.DisplayStyle = OComboBoxDropDownStyle.CodeAndDescription;
			AssertEquals("GetCustomValue should be return Description1 (YYY)", "Description2 (YYY)", testColumnWithBindToList.GetCustomValue(testBizOWithLookup));

			testBizOWithLookup.Z0_Description = "XXX";
			AssertEquals("GetCustomValue should be return Description1 (XXX)", "Description1 (XXX)", testColumnWithBindToList.GetCustomValue(testBizOWithLookup));

			testBizOWithLookup.Z0_Description = "ZZZ";

			testColumnWithBindToList.DisplayStyle = OComboBoxDropDownStyle.CodeOnly;
			AssertEquals("GetCustomValue should be return Empty", ZString.Empty, testColumnWithBindToList.GetCustomValue(testBizOWithLookup));

			testColumnWithBindToList.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AssertEquals("GetCustomValue should be return Empty", ZString.Empty, testColumnWithBindToList.GetCustomValue(testBizOWithLookup));

			testColumnWithBindToList.DisplayStyle = OComboBoxDropDownStyle.CodeAndDescription;
			AssertEquals("GetCustomValue should be return Empty", ZString.Empty, testColumnWithBindToList.GetCustomValue(testBizOWithLookup));

			ZDropDownListColumn testColumnWithOutBindToList = new ZDropDownListColumn("Test Column2", DummyBusinessObject.Schema.Z0_Code);
			DummyBusinessObject testBizOWithoutLookup = Factory.NewWithValidTestData<DummyBusinessObject>();
			testBizOWithoutLookup.Z0_Code = "BBB";
			AssertEquals("GetCustomValue should be return BBB", "BBB", testColumnWithOutBindToList.GetCustomValue(testBizOWithoutLookup));
		}

		public void TestGetValue_IWrappedBizOProvider()
		{
			var dummyWrapper = new DummyWrappedBizOProvider(Factory);

			var testColumnToWrappedProperty = new ZDropDownListColumn("Test Column2", "WrappedBizO.Z0_Description");
			dummyWrapper.WrappedBizO.Z0_Description = "XXX";

			AssertEquals("GetCustomValue should be return XXX", "XXX", testColumnToWrappedProperty.GetCustomValue(dummyWrapper));

			var testColumnToDummyProperty = new ZDropDownListColumn("Test Column2", "SomeProperty");
			dummyWrapper.SomeProperty = "ZZZ";

			AssertEquals("GetCustomValue should be return ZZZ", "ZZZ", testColumnToDummyProperty.GetCustomValue(dummyWrapper));
		}

		public void TestGetDescription()
		{
			ZDropDownListColumn testColumn = new ZDropDownListColumn("Test Column", "Test");
			AssertEquals("GetDescription should be return Test Column", "Test Column", testColumn.GetDescription());
			testColumn.HeaderText = "Test";
			AssertEquals("GetDescription should be return Test", "Test", testColumn.GetDescription());
		}

		public void TestGetValueFormat()
		{
			ZDropDownListColumn testColumn = new ZDropDownListColumn("Test Column", "Test");
			AssertEquals("GetValueFormat should be return Test Column", "", testColumn.GetValueFormat(new ZString("test")));
		}

		public void TestMultilingualStringDescription()
		{
			var testBizOWithLookup = Factory.NewWithValidTestData<DummyBusinessObjectWithMultilingualStringLookup>();
			testBizOWithLookup.Z0_Description = "YYY";

			var biz1 = testBizOWithLookup.Lookup.AddNew();
			biz1.Z0_Description = "XXX";
			biz1.Z0_MultilingualString = (NoResString)"Description 1";

			var biz2 = testBizOWithLookup.Lookup.AddNew();
			biz2.Z0_Description = "YYY";
			biz2.Z0_MultilingualString = (NoResString)"Description 2";

			var biz3 = testBizOWithLookup.Lookup.AddNew();
			biz3.Z0_Description = "ZZZ";

			var testColumnWithBindToList = new ZDropDownListColumn("Test Column", DummyBusinessObject.Schema.Z0_Description, "Lookup") { TextFieldName = "Z0_MultilingualString" };
			testColumnWithBindToList.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AssertEquals("GetCustomValue should be return Description 2", "Description 2", testColumnWithBindToList.GetCustomValue(testBizOWithLookup));

			testBizOWithLookup.Z0_Description = "ZZZ";
			AssertEquals("GetCustomValue should be return empty string", ZString.Empty, testColumnWithBindToList.GetCustomValue(testBizOWithLookup));
		}

		#region Helpers

		class DummyBusinessObjectWithMultilingualStringLookup : DummyBaseBusinessObject
		{
			public DummyBusinessObjectWithMultilingualStringLookup(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
				Lookup = new DummyBusinessObjectWithMultilingualStringCollection(factory);
			}

			public DummyBusinessObjectWithMultilingualStringCollection Lookup { get; set; }
		}

		class DummyBusinessObjectWithMultilingualString : DummyBaseBusinessObject
		{
			public DummyBusinessObjectWithMultilingualString(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public MultilingualString Z0_MultilingualString { get; set; }
		}

		class DummyBusinessObjectWithMultilingualStringCollection : BusinessObjectCollection<DummyBusinessObjectWithMultilingualString>
		{
			public DummyBusinessObjectWithMultilingualStringCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected DummyBusinessObjectWithMultilingualStringCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
				: base(factory, additionalFilter)
			{
			}
		}

		class DummyWrappedBizOProvider : NonPersistentBusinessObject, IWrappedBizOProvider
		{
			public DummyWrappedBizOProvider(BusinessObjectFactory factory)
				: base(factory)
			{
				WrappedBizO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			}

			public DummyEnterpriseBusinessObject WrappedBizO { get; }

			public ZString SomeProperty { get; set; }

			public ZPropertyInfo SomePropertyInfo
			{
				get
				{
					return new ZPropertyInfoString(this, nameof(SomeProperty));
				}
			}

			public string GetWrappedBindTo(string bindTo) => "WrappedBizO." + bindTo;

			public BusinessObject GetWrappedBizO() => WrappedBizO;
		}

		#endregion
	}
}
