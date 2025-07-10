using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WeightWrapper))]
	sealed class WeightWrapperTest : ValueAndUnitWrapperTest
	{
		public void TestRoundingForUnitConversions()
		{
			Env.Registry.FreightWeightUnit = Constants.Weight.Kilograms;
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.Weight);
			WeightWrapper wrapper = new WeightWrapper(1714.77, Constants.Weight.Pounds, 3, list, Factory);
			AssertEquals("Should convert to KG", Constants.Weight.Kilograms, wrapper.InFreightWeightUnit.Unit.Code);
			AssertEquals("Should round to 3 decimal places", new ZDecimal(777.807), wrapper.InFreightWeightUnit.Value);
		}

		public void TestInKilograms()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.Weight);

			WeightWrapper tonnes = new WeightWrapper(1.75, Core.Constants.Weight.Tonnes, 1, list, Factory);

			AssertEquals("precondition:", "1.8 T", tonnes.ValueAndUnitCode);
			AssertEquals("1800.0 KG", tonnes.InKilograms.ValueAndUnitCode);
		}

		public void TestInPounds()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.Weight);

			WeightWrapper ounces = new WeightWrapper(24, Core.Constants.Weight.Ounces, 1, list, Factory);

			AssertEquals("precondition:", "24.0 OZ", ounces.ValueAndUnitCode);
			AssertEquals("1.5 LB", ounces.InPounds.ValueAndUnitCode);
		}

		public void TestInFreightWeightUnit()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.Weight);
			WeightWrapper tonnes = new WeightWrapper(1.75, Core.Constants.Weight.Tonnes, 1, list, Factory);
			AssertEquals("precondition:", "1.8 T", tonnes.ValueAndUnitCode);

			Env.Registry.FreightWeightUnit = Core.Constants.Weight.Kilograms;
			AssertEquals("1800.0 KG", tonnes.InFreightWeightUnit.ValueAndUnitCode);

			Env.Registry.FreightWeightUnit = Core.Constants.Weight.Tonnes;
			AssertEquals("1.8 T", tonnes.InFreightWeightUnit.ValueAndUnitCode);
		}

		public void TestInPackageWeightUnit()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.Weight);
			WeightWrapper tonnes = new WeightWrapper(1.75, Core.Constants.Weight.Tonnes, 1, list, Factory);
			AssertEquals("precondition:", "1.8 T", tonnes.ValueAndUnitCode);

			Env.Registry.PackageWeightUnit = Core.Constants.Weight.Kilograms;
			AssertEquals("1800.0 KG", tonnes.InPackageWeightUnit.ValueAndUnitCode);

			Env.Registry.PackageWeightUnit = Core.Constants.Weight.Tonnes;
			AssertEquals("1.8 T", tonnes.InPackageWeightUnit.ValueAndUnitCode);
		}

		public override void TestWrapperMappingsEmpty()
		{
			WeightWrapper wrapperEmpty = new WeightWrapper(ZDecimal.Zero, ZString.Empty, 1, WeightsList, Factory);
			AssertEquals("wrapperEmpty.Unit.Code", ZString.Empty, wrapperEmpty.Unit.Code);
			AssertEquals("wrapperEmpty.Value", ZDecimal.Zero, wrapperEmpty.Value);
			AssertEquals("wrapperEmpty.ValueAndUnitCode", "0.0", wrapperEmpty.ValueAndUnitCode);
			AssertEquals("wrapperEmpty.ValueAndUnitCodeBlankIfZero", ZString.Empty, wrapperEmpty.ValueAndUnitCodeBlankIfZero);
		}

		public void TestTotalWithDifferentUnits()
		{
			TestWrapperBO bO1 = new TestWrapperBO();
			bO1.WeightValue = 12.34m;
			bO1.WeightUnit = "KG";

			TestWrapperBO bO2 = new TestWrapperBO();
			bO2.WeightValue = 34.89m;
			bO2.WeightUnit = "OT";

			TestWrapperBO bO3 = new TestWrapperBO();
			bO3.WeightValue = 98.47m;
			bO3.WeightUnit = "TL";

			List<TestWrapperClass> testList = new List<TestWrapperClass>();
			testList.Add(new TestWrapperClass(bO1, Factory));
			testList.Add(new TestWrapperClass(bO2, Factory));
			testList.Add(new TestWrapperClass(bO3, Factory));

			var collection = (IBODocDataProviderCollection)(new TestWrapperClassCollection(testList, Factory));
			AssertEquals("Weight is totalled correctly", "100094.00 KG", collection.Total("WrapperWeight", "2", null));
		}

		public void TestTotalWithSameUnits()
		{
			TestWrapperBO bO1 = new TestWrapperBO();
			bO1.WeightValue = 12.345m;
			bO1.WeightUnit = "OT";

			TestWrapperBO bO2 = new TestWrapperBO();
			bO2.WeightValue = 34.89m;
			bO2.WeightUnit = "OT";

			TestWrapperBO bO3 = new TestWrapperBO();
			bO3.WeightValue = 98.472m;
			bO3.WeightUnit = "OT";

			List<TestWrapperClass> testList = new List<TestWrapperClass>();
			testList.Add(new TestWrapperClass(bO1, Factory));
			testList.Add(new TestWrapperClass(bO2, Factory));
			testList.Add(new TestWrapperClass(bO3, Factory));

			var collection = (IBODocDataProviderCollection)(new TestWrapperClassCollection(testList, Factory));
			AssertEquals("Weight is totalled correctly", "145.7000 OT", collection.Total("WrapperWeight", "4", null));
		}

		public void TestTotalWithInvalidUnitLast()
		{
			TestWrapperBO bO1 = new TestWrapperBO();
			bO1.WeightValue = 12.34m;
			bO1.WeightUnit = "KG";

			TestWrapperBO bO2 = new TestWrapperBO();
			bO2.WeightValue = 34.89m;
			bO2.WeightUnit = "QWE";

			List<TestWrapperClass> testList = new List<TestWrapperClass>();
			testList.Add(new TestWrapperClass(bO1, Factory));
			testList.Add(new TestWrapperClass(bO2, Factory));

			var collection = (IBODocDataProviderCollection)(new TestWrapperClassCollection(testList, Factory));
			AssertEquals("Second Unit is invalid", ZString.Empty, collection.Total("WrapperWeight", "1", null));
		}

		public void TestTotalWithInvalidUnitFirst()
		{
			TestWrapperBO bO1 = new TestWrapperBO();
			bO1.WeightValue = 12.34m;
			bO1.WeightUnit = "QWE";

			TestWrapperBO bO2 = new TestWrapperBO();
			bO2.WeightValue = 34.89m;
			bO2.WeightUnit = "KG";

			List<TestWrapperClass> testList = new List<TestWrapperClass>();
			testList.Add(new TestWrapperClass(bO1, Factory));
			testList.Add(new TestWrapperClass(bO2, Factory));

			var collection = (IBODocDataProviderCollection)(new TestWrapperClassCollection(testList, Factory));
			AssertEquals("First Unit is invalid", ZString.Empty, collection.Total("WrapperWeight", "1", null));
		}

		public void TestTotalWithInvalidUnitsThroughout()
		{
			TestWrapperBO bO1 = new TestWrapperBO();
			bO1.WeightValue = 12.34m;
			bO1.WeightUnit = "QWE";

			TestWrapperBO bO2 = new TestWrapperBO();
			bO2.WeightValue = 34.89m;
			bO2.WeightUnit = "KG";

			TestWrapperBO bO3 = new TestWrapperBO();
			bO3.WeightValue = 12.34m;
			bO1.WeightUnit = "QWE";

			TestWrapperBO bO4 = new TestWrapperBO();
			bO4.WeightValue = 34.89m;
			bO4.WeightUnit = "OT";

			List<TestWrapperClass> testList = new List<TestWrapperClass>();
			testList.Add(new TestWrapperClass(bO1, Factory));
			testList.Add(new TestWrapperClass(bO2, Factory));
			testList.Add(new TestWrapperClass(bO3, Factory));
			testList.Add(new TestWrapperClass(bO4, Factory));

			var collection = (IBODocDataProviderCollection)(new TestWrapperClassCollection(testList, Factory));
			AssertEquals("First And Second Unit is invalid", ZString.Empty, collection.Total("WrapperWeight", "1", null));
		}

		public void TestTotalWithAllInvalidUnits()
		{
			TestWrapperBO bO1 = new TestWrapperBO();
			bO1.WeightValue = 12.34m;
			bO1.WeightUnit = "ZZ";

			TestWrapperBO bO2 = new TestWrapperBO();
			bO2.WeightValue = 34.89m;
			bO2.WeightUnit = "ZZ";

			TestWrapperBO bO3 = new TestWrapperBO();
			bO3.WeightValue = 48.78m;
			bO3.WeightUnit = "ZZ";

			List<TestWrapperClass> testList = new List<TestWrapperClass>();
			testList.Add(new TestWrapperClass(bO1, Factory));
			testList.Add(new TestWrapperClass(bO2, Factory));
			testList.Add(new TestWrapperClass(bO3, Factory));

			var collection = (IBODocDataProviderCollection)(new TestWrapperClassCollection(testList, Factory));
			AssertEquals("All Units are invalid", "96.0 ZZ", collection.Total("WrapperWeight", "1", null));
		}

		public void TestNonDefaultDecimalPlaces()
		{
			WeightWrapper wrapper = new WeightWrapper(123.4567, "KG", 3, WeightsList, Factory);

			AssertEquals("123.457 KG", wrapper.ValueAndUnitCode);
			AssertEquals(123.457m, wrapper.Value);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new WeightWrapper(ZDecimal.Zero, ZString.Empty, 1, WeightsList, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Weight                    (Default Field: ValueAndUnitCodeBlankIfZero)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Unit                                    Unit
InFreightWeightUnit                     Weight
InKilograms                             Weight
InPackageWeightUnit                     Weight
InPounds                                Weight
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
			public ZDecimal WeightValue
			{
				get { return weightValue; }
				set { weightValue = value; }
			}
			ZDecimal weightValue;

			public ZString WeightUnit
			{
				get { return weightUnit; }
				set { weightUnit = value; }
			}
			ZString weightUnit;
		}

		public class TestWrapperClass : GenericWrapper
		{
			public TestWrapperClass(TestWrapperBO testWrapperBO, BusinessObjectFactory factory)
				: base(testWrapperBO, factory)
			{
				TestWrapperBO = testWrapperBO;
			}
			readonly TestWrapperBO TestWrapperBO;

			public WeightWrapper WrapperWeight
			{
				get { return wrapperWeight ?? (wrapperWeight = new WeightWrapper(TestWrapperBO.WeightValue, TestWrapperBO.WeightUnit, 1, new CodeDescriptionPairList(), Factory)); }
			}
			WeightWrapper wrapperWeight;
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
