using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PackQTYWrapper))]
	sealed class PackQTYWrapperTest : ValueAndUnitWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			PackQTYWrapper wrapperEmpty = new PackQTYWrapper(ZInt.Zero, ZString.Empty, WeightsList, Factory);
			AssertEquals("wrapperEmpty.Unit.Code", ZString.Empty, wrapperEmpty.Unit.Code);
			AssertEquals("wrapperEmpty.Value", ZDecimal.Zero, wrapperEmpty.Value);
			AssertEquals("wrapperEmpty.ValueAndUnitCode", "0", wrapperEmpty.ValueAndUnitCode);
			AssertEquals("wrapperEmpty.ValueAndUnitCodeBlankIfZero", ZString.Empty, wrapperEmpty.ValueAndUnitCodeBlankIfZero);
		}

		public void TestTotalWithDifferentUnits()
		{
			TestWrapperBO bO1 = new TestWrapperBO();
			bO1.PackValue = 12;
			bO1.PackUnit = "BAG";

			TestWrapperBO bO2 = new TestWrapperBO();
			bO2.PackValue = 34;
			bO2.PackUnit = "BLU";

			TestWrapperBO bO3 = new TestWrapperBO();
			bO3.PackValue = 98;
			bO3.PackUnit = "BSK";

			List<TestWrapperClass> testList = new List<TestWrapperClass>();
			testList.Add(new TestWrapperClass(bO1, Factory));
			testList.Add(new TestWrapperClass(bO2, Factory));
			testList.Add(new TestWrapperClass(bO3, Factory));

			var collection = (IBODocDataProviderCollection)(new TestWrapperClassCollection(testList, Factory));
			AssertEquals("Volume is totalled correctly", "144 PKG", collection.Total("WrapperPack", "0", null));
		}

		public void TestTotalWithSameUnits()
		{
			TestWrapperBO bO1 = new TestWrapperBO();
			bO1.PackValue = 73;
			bO1.PackUnit = "RLL";

			TestWrapperBO bO2 = new TestWrapperBO();
			bO2.PackValue = 25;
			bO2.PackUnit = "RLL";

			TestWrapperBO bO3 = new TestWrapperBO();
			bO3.PackValue = 89;
			bO3.PackUnit = "RLL";

			List<TestWrapperClass> testList = new List<TestWrapperClass>();
			testList.Add(new TestWrapperClass(bO1, Factory));
			testList.Add(new TestWrapperClass(bO2, Factory));
			testList.Add(new TestWrapperClass(bO3, Factory));

			var collection = (IBODocDataProviderCollection)(new TestWrapperClassCollection(testList, Factory));
			AssertEquals("Volume is totalled correctly", "187 RLL", collection.Total("WrapperPack", "0", null));
		}

		public void TestTotalWithInvalidUnitLast()
		{
			TestWrapperBO bO1 = new TestWrapperBO();
			bO1.PackValue = 12;
			bO1.PackUnit = "KEG";

			TestWrapperBO bO2 = new TestWrapperBO();
			bO2.PackValue = 56;
			bO2.PackUnit = "ZZZ";

			List<TestWrapperClass> testList = new List<TestWrapperClass>();
			testList.Add(new TestWrapperClass(bO1, Factory));
			testList.Add(new TestWrapperClass(bO2, Factory));

			var collection = (IBODocDataProviderCollection)(new TestWrapperClassCollection(testList, Factory));
			AssertEquals("Second Unit is invalid", "68 PKG", collection.Total("WrapperPack", "0", null));
		}

		public void TestTotalWithInvalidUnitFirst()
		{
			TestWrapperBO bO1 = new TestWrapperBO();
			bO1.PackValue = 834;
			bO1.PackUnit = "MNB";

			TestWrapperBO bO2 = new TestWrapperBO();
			bO2.PackValue = 89;
			bO2.PackUnit = "BBG";

			List<TestWrapperClass> testList = new List<TestWrapperClass>();
			testList.Add(new TestWrapperClass(bO1, Factory));
			testList.Add(new TestWrapperClass(bO2, Factory));

			var collection = (IBODocDataProviderCollection)(new TestWrapperClassCollection(testList, Factory));
			AssertEquals("First Unit is invalid", "923 PKG", collection.Total("WrapperPack", "0", null));
		}

		public void TestTotalWithInvalidUnitsThroughout()
		{
			TestWrapperBO bO1 = new TestWrapperBO();
			bO1.PackValue = 234;
			bO1.PackUnit = "MIX";

			TestWrapperBO bO2 = new TestWrapperBO();
			bO2.PackValue = 32;
			bO2.PackUnit = "ZAS";

			TestWrapperBO bO3 = new TestWrapperBO();
			bO3.PackValue = 456;
			bO1.PackUnit = "BND";

			TestWrapperBO bO4 = new TestWrapperBO();
			bO4.PackValue = 98;
			bO4.PackUnit = "POR";

			List<TestWrapperClass> testList = new List<TestWrapperClass>();
			testList.Add(new TestWrapperClass(bO1, Factory));
			testList.Add(new TestWrapperClass(bO2, Factory));
			testList.Add(new TestWrapperClass(bO3, Factory));
			testList.Add(new TestWrapperClass(bO4, Factory));

			var collection = (IBODocDataProviderCollection)(new TestWrapperClassCollection(testList, Factory));
			AssertEquals("First And Second Unit is invalid", "820 PKG", collection.Total("WrapperPack", "0", null));
		}

		public void TestTotalWithAllInvalidUnits()
		{
			TestWrapperBO bO1 = new TestWrapperBO();
			bO1.PackValue = 34;
			bO1.PackUnit = "M&M";

			TestWrapperBO bO2 = new TestWrapperBO();
			bO2.PackValue = 980;
			bO2.PackUnit = "M&M";

			TestWrapperBO bO3 = new TestWrapperBO();
			bO3.PackValue = 234;
			bO3.PackUnit = "M&M";

			List<TestWrapperClass> testList = new List<TestWrapperClass>();
			testList.Add(new TestWrapperClass(bO1, Factory));
			testList.Add(new TestWrapperClass(bO2, Factory));
			testList.Add(new TestWrapperClass(bO3, Factory));

			var collection = (IBODocDataProviderCollection)(new TestWrapperClassCollection(testList, Factory));
			AssertEquals("All Units are invalid", "1248 M&M", collection.Total("WrapperPack", "0", null));
		}

		public void TestStaticEmpty()
		{
			PackQTYWrapper wrapper = PackQTYWrapper.Empty;
			Assert("Value is empty from empty wrapper", wrapper.Value.IsEmpty);
			Assert("Unit is empty from empty wrapper", wrapper.Unit.Code.IsEmpty);
			PackQTYWrapper wrapper2 = PackQTYWrapper.Empty;
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new PackQTYWrapper(ZInt.Zero, ZString.Empty, WeightsList, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
PackQTY                   (Default Field: ValueAndUnitCodeBlankIfZero)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Unit                                    Unit
FormatValueAndUnitCode                  String
FormatValueAndUnitCodeBlankIfZero       String
Value                                   Decimal
ValueAndUnitCode                        String
ValueAndUnitCodeBlankIfZero             String
";
			}
		}

		public class TestWrapperBO : NonPersistentBusinessObject
		{
			public ZInt PackValue
			{
				get { return packValue; }
				set { packValue = value; }
			}
			ZInt packValue;

			public ZString PackUnit
			{
				get { return packUnit; }
				set { packUnit = value; }
			}
			ZString packUnit;
		}

		public class TestWrapperClass : GenericWrapper
		{
			public TestWrapperClass(TestWrapperBO testWrapperBO, BusinessObjectFactory factory)
				: base(testWrapperBO, factory)
			{
				TestWrapperBO = testWrapperBO;
			}
			readonly TestWrapperBO TestWrapperBO;

			public PackQTYWrapper WrapperPack
			{
				get { return wrapperPack ?? (wrapperPack = new PackQTYWrapper(TestWrapperBO.PackValue, TestWrapperBO.PackUnit, new CodeDescriptionPairList(), Factory)); }
			}
			PackQTYWrapper wrapperPack;
		}

		public class TestWrapperClassCollection : GenericWrapperCollection<TestWrapperClass>
		{
			public TestWrapperClassCollection(List<TestWrapperClass> list, BusinessObjectFactory factory)
				: base(factory)
			{
				if (list != null)
				{
					foreach (TestWrapperClass wrapper in list)
					{
						Add(wrapper);
					}
				}
			}
		}
	}
}
