using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using Enterprise.Registry.Business.Web;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Shared;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	sealed class TestZPropertyAccessor : TestCaseWithFactory
	{
		#region Setup

		protected override BusinessObjectFactory NewFactory()
		{
			return new BusinessObjectFactory();
		}

		#region MyDummy

		class MyDummy : DummyBusinessObject
		{
			public MyDummy(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{ }

			public DummyBusinessObject TestChild
			{
				get
				{
					if (fTestChild == null)
					{
						fTestChild = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
					}
					return fTestChild;
				}
			}
			DummyBusinessObject fTestChild;
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			TestBizO = (MyDummy)Factory.New(typeof(MyDummy));
		}

		MyDummy TestBizO;

		#endregion

		public void TestGetPropertyValue()
		{
			AssertEquals(TestBizO.Z0_VarCharMax, ZPropertyAccessor.Get(TestBizO, "Z0_VarCharMax"));
			AssertEquals(TestBizO.Z0_Number, ZPropertyAccessor.Get(TestBizO, "Z0_Number"));

			ZDateTime testDate = new ZDateTime(2005, 12, 9);
			ZPropertyAccessor.Set(TestBizO, "TestChild.Z0_AnotherDate", testDate);
			AssertEquals("properties from child objects", testDate, ZPropertyAccessor.Get(TestBizO, "TestChild.Z0_AnotherDate"));
		}

		public void TestTryGet()
		{
			Assert(ZPropertyAccessor.TryGet(TestBizO, "Z0_Code", out object value));
			AssertEquals(TestBizO.Z0_Code, value);

			Assert(!ZPropertyAccessor.TryGet(TestBizO, "SomeProperty", out value));
			AssertNull(value);

			Assert(!ZPropertyAccessor.TryGet(null, "Z0_Code", out value));
			AssertNull(value);

			Assert(!ZPropertyAccessor.TryGet(TestBizO, null, out value));
			AssertNull(value);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestGetWithNullObject()
		{
			ZPropertyAccessor.Get(null, "Name");
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestGetWithNullPropertyName()
		{
			ZPropertyAccessor.Get(TestBizO, null);
		}

		public void TestNullPropertyValueGet()
		{
			TestBizO.Z0_Date = new ZDateTime(null);
			AssertEquals(null, ZPropertyAccessor.Get(TestBizO, "Z0_Date.Minute"));
		}

		public void TestSetPropertyValue()
		{
			ZString newCode = "Code";
			ZInt newNumber = 50;
			AssertEquals(TestBizO.Z0_Code, ZPropertyAccessor.Get(TestBizO, "Z0_Code"));
			AssertEquals(TestBizO.Z0_Number, ZPropertyAccessor.Get(TestBizO, "Z0_Number"));

			ZPropertyAccessor.Set(TestBizO, "Z0_Code", newCode);
			ZPropertyAccessor.Set(TestBizO, "Z0_Number", newNumber);

			AssertEquals(newCode, TestBizO.Z0_Code);
			AssertEquals(newNumber, TestBizO.Z0_Number);
		}

		public void TestGetBusinessObjectProperty()
		{
			AssertEquals("ZBool Get Failed", TestBizO.Z0_Bool, ZPropertyAccessor.Get(TestBizO, "Z0_Bool"));
			AssertEquals("ZCode Get Failed", TestBizO.Z0_Code, ZPropertyAccessor.Get(TestBizO, "Z0_Code"));
			AssertEquals("ZDateTime Get Failed", TestBizO.Z0_Date, ZPropertyAccessor.Get(TestBizO, "Z0_Date"));
			AssertEquals("ZDecimal Get Failed", TestBizO.Z0_Decimal, ZPropertyAccessor.Get(TestBizO, "Z0_Decimal"));
			AssertEquals("ZDescriptionInfo Get Failed", TestBizO.Z0_DescriptionInfo, ZPropertyAccessor.Get(TestBizO, "Z0_DescriptionInfo"));
			AssertEquals("ZGuid Get Failed", TestBizO.Z0_Guid, ZPropertyAccessor.Get(TestBizO, "Z0_Guid"));
			AssertEquals("ZInt Get Failed", TestBizO.Z0_Number, ZPropertyAccessor.Get(TestBizO, "Z0_Number"));
			AssertEquals("ZShort Get Failed", TestBizO.Z0_Short, ZPropertyAccessor.Get(TestBizO, "Z0_Short"));
			AssertEquals("Z0_CodeInfo Get Failed", TestBizO.Z0_CodeInfo, ZPropertyAccessor.Get(TestBizO, "Z0_CodeInfo"));
		}

		public void TestGetAccessControlledProperty()
		{
			AccessControlRegistryItem testItem = new AccessControlRegistryItem("testName", (NoResString)"testCategory", (NoResString)"testCaption", (NoResString)"testHint", new DummyAccessRules(), Enterprise.Integration.RegistryOptions.Default);

			DummyAccessControlled bizO = Factory.New<DummyAccessControlled>();
			bizO.SuppressionItem = testItem;
			bizO.IsInTextSuppressionMode = true;
			bizO.LoggedInOrgsRoles = new[] { DummyAccessRules.Roles.role2, DummyAccessRules.Roles.role3 };

			ZPropertyAccessor.Set(bizO, "Z0_Date", new ZDateTime(2009, 09, 09));
			ZPropertyAccessor.Set(bizO, "Z0_Bool", (ZBool)true);
			ZPropertyAccessor.Set(bizO, "Z0_Code", (ZString)"TST");
			ZPropertyAccessor.Set(bizO, "Z0_VarCharMax", (ZString)"Some text");

			AssertEquals("Nothing should be suppressed on new objects", new ZDateTime(2009, 09, 09), ZPropertyAccessor.Get(bizO, "Z0_Date"));
			AssertEquals("Nothing should be suppressed on new objects", "Some text", ZPropertyAccessor.Get(bizO, "Z0_VarCharMax"));
			AssertEquals("Nothing should be suppressed on new objects", true, ZPropertyAccessor.Get(bizO, "Z0_Bool"));
			AssertEquals("Nothing should be suppressed on new objects", "TST", ZPropertyAccessor.Get(bizO, "Z0_Code"));

			Factory.Save();

			AssertEquals("Z0_Date should be suppressed", SuppressUtil.SuppressedDateTime, ZPropertyAccessor.Get(bizO, "Z0_Date"));
			AssertEquals("Z0_VarCharMax should be suppressed", SuppressUtil.SuppressedText, ZPropertyAccessor.Get(bizO, "Z0_VarCharMax"));
			AssertEquals("Z0_Bool should not be suppressed", true, ZPropertyAccessor.Get(bizO, "Z0_Bool"));
			AssertEquals("Z0_Code should not be suppressed", "TST", ZPropertyAccessor.Get(bizO, "Z0_Code"));

			bizO.IsInTextSuppressionMode = false;

			AssertEquals("Z0_Date should not be suppressed", new ZDateTime(2009, 09, 09), ZPropertyAccessor.Get(bizO, "Z0_Date"));
			AssertEquals("Z0_VarCharMax should not be suppressed", "Some text", ZPropertyAccessor.Get(bizO, "Z0_VarCharMax"));
			AssertEquals("Z0_Bool should not be suppressed", true, ZPropertyAccessor.Get(bizO, "Z0_Bool"));
			AssertEquals("Z0_Code should not be suppressed", "TST", ZPropertyAccessor.Get(bizO, "Z0_Code"));
		}

		public void TestGetBusinessObjectChildProperty()
		{
			AssertEquals("Failed to get Child property", TestBizO.Z0_CodeInfo.Name, ZPropertyAccessor.Get(TestBizO, "Z0_CodeInfo.Name"));
		}

		public void TestSetBusinessObjectChildProperty()
		{
			ZDateTime testDate = new ZDateTime(2005, 12, 9);
			ZPropertyAccessor.Set(TestBizO, "TestChild.Z0_AnotherDate", testDate);
			AssertEquals("properties from child objects", testDate, ZPropertyAccessor.Get(TestBizO, "TestChild.Z0_AnotherDate"));
		}

		public void TestSetBusinessObjectProperty()
		{
			ZPropertyAccessor.Set(TestBizO, "Z0_Bool", ZBool.False);
			AssertEquals("ZBool Set to False Failed", ZBool.False, TestBizO.Z0_Bool);

			ZPropertyAccessor.Set(TestBizO, "Z0_Bool", ZBool.True);
			AssertEquals("ZBool Set to True Failed", ZBool.True, TestBizO.Z0_Bool);

			ZGuid testGuid = ZGuid.NewZGuid();
			ZPropertyAccessor.Set(TestBizO, "Z0_Guid", testGuid);
			AssertEquals("ZGuid Set Failed", testGuid, TestBizO.Z0_Guid);

			ZString testCode = new ZString("TST");
			ZPropertyAccessor.Set(TestBizO, "Z0_Code", testCode);
			AssertEquals("ZString Set Failed", testCode, TestBizO.Z0_Code);

			ZDateTime testDate = new ZDateTime(2004, 01, 01, 12, 00, 00);
			ZPropertyAccessor.Set(TestBizO, "Z0_Date", testDate);
			AssertEquals("ZDateTime Set Failed", testDate, TestBizO.Z0_Date);

			ZDecimal testNum = new ZDecimal(100);
			ZPropertyAccessor.Set(TestBizO, "Z0_Decimal", testNum);
			AssertEquals("ZDecimal Set Failed", testNum, TestBizO.Z0_Decimal);

			ZInt testInt = new ZInt(12329);
			ZPropertyAccessor.Set(TestBizO, "Z0_Number", testInt);
			AssertEquals("ZInt Test Failed", testInt, TestBizO.Z0_Number);

			ZShort testShort = new ZShort(28342);
			ZPropertyAccessor.Set(TestBizO, "Z0_Short", testShort);
			AssertEquals("ZShort Set Failed", testShort, TestBizO.Z0_Short);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestNonSetterProperty()
		{
			ZPropertyAccessor.Set(TestBizO, "PK_ColumnName", "PK");
		}

		public void TestGetPropertyType()
		{
			AssertEquals(typeof(ZShort), ZPropertyAccessor.GetPropertyType(TestBizO, "Z0_Short"));
			AssertEquals(typeof(ZDateTime), ZPropertyAccessor.GetPropertyType(TestBizO, "Z0_Date"));
			AssertEquals(typeof(ZDecimal), ZPropertyAccessor.GetPropertyType(TestBizO, "Z0_Decimal"));
			AssertEquals(typeof(ZGuid), ZPropertyAccessor.GetPropertyType(TestBizO, "Z0_Guid"));
			AssertEquals(typeof(ZString), ZPropertyAccessor.GetPropertyType(TestBizO, "Z0_Code"));

			AssertEquals("properties from child objects", typeof(ZDateTime), ZPropertyAccessor.GetPropertyType(TestBizO, "TestChild.Z0_AnotherDate"));
		}

		public void TestHtmlEncodeForStrings()
		{
			DummyBusinessObject testBizO = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));

			ZString testCode = new ZString("<script>TST</script>");
			ZPropertyAccessor.Set(testBizO, "Z0_Description", testCode);
			AssertEquals(testCode, testBizO.Z0_Description);
			AssertEquals("ZString should NOT be encoded", testCode, ZPropertyAccessor.Get(testBizO, "Z0_Description"));
			AssertEquals("ZString should be encoded", "&lt;script&gt;TST&lt;/script&gt;", ZPropertyAccessor.GetWithEncode(testBizO, "Z0_Description"));
		}
	}
}
