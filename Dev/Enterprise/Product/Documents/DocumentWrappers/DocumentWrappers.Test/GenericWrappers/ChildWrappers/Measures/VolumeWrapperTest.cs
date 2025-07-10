using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(VolumeWrapper))]
	sealed class VolumeWrapperTest : ValueAndUnitWrapperTest
	{
		public void TestInCubicMeters()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.Volume);

			VolumeWrapper litres = new VolumeWrapper(1750, Core.Constants.Volume.Litre, list, Factory);
			AssertEquals("precondition:", "1750.000 L", litres.ValueAndUnitCode);
			AssertEquals("1.750 M3", litres.InCubicMeters.ValueAndUnitCode);
		}

		public void TestInPounds()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.Volume);

			VolumeWrapper cubicInches = new VolumeWrapper(3024, Core.Constants.Volume.CubicInches, list, Factory);

			AssertEquals("precondition:", "3024.000 CI", cubicInches.ValueAndUnitCode);
			AssertEquals("1.750 CF", cubicInches.InCubicFeet.ValueAndUnitCode);
		}

		public void TestInFreightVolumeUnit()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.Weight);
			VolumeWrapper litres = new VolumeWrapper(1750, Core.Constants.Volume.Litre, list, Factory);
			AssertEquals("precondition:", "1750.000 L", litres.ValueAndUnitCode);

			Env.Registry.FreightVolumeUnit = Core.Constants.Volume.Litre;
			AssertEquals("1750.000 L", litres.ValueAndUnitCode);

			Env.Registry.FreightVolumeUnit = Core.Constants.Volume.CubicMetres;
			AssertEquals("1.750 M3", litres.InCubicMeters.ValueAndUnitCode);
		}

		public void TestInPackageVolumeUnit()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.Volume);
			VolumeWrapper litres = new VolumeWrapper(1750, Core.Constants.Volume.Litre, list, Factory);
			AssertEquals("precondition:", "1750.000 L", litres.ValueAndUnitCode);

			Env.Registry.PackageVolumeUnit = Core.Constants.Volume.Litre;
			AssertEquals("1750.000 L", litres.InPackageVolumeUnit.ValueAndUnitCode);

			Env.Registry.PackageVolumeUnit = Core.Constants.Volume.CubicMetres;
			AssertEquals("1.750 M3", litres.InPackageVolumeUnit.ValueAndUnitCode);
		}

		public override void TestWrapperMappingsEmpty()
		{
			VolumeWrapper wrapperEmpty = VolumeWrapper.Empty;
			AssertEquals("wrapperEmpty.Unit.Code", ZString.Empty, wrapperEmpty.Unit.Code);
			AssertEquals("wrapperEmpty.Value", ZDecimal.Zero, wrapperEmpty.Value);
			AssertEquals("wrapperEmpty.ValueAndUnitCode", "0.000", wrapperEmpty.ValueAndUnitCode);
			AssertEquals("wrapperEmpty.ValueAndUnitCodeBlankIfZero", ZString.Empty, wrapperEmpty.ValueAndUnitCodeBlankIfZero);
		}

		public void TestTotalWithDifferentUnits()
		{
			TestWrapperBO bO1 = new TestWrapperBO();
			bO1.VolumeValue = 12.349m;
			bO1.VolumeUnit = "D3";

			TestWrapperBO bO2 = new TestWrapperBO();
			bO2.VolumeValue = 34.891m;
			bO2.VolumeUnit = "CY";

			TestWrapperBO bO3 = new TestWrapperBO();
			bO3.VolumeValue = 98.473m;
			bO3.VolumeUnit = "CI";

			List<TestWrapperClass> testList = new List<TestWrapperClass>();
			testList.Add(new TestWrapperClass(bO1, Factory));
			testList.Add(new TestWrapperClass(bO2, Factory));
			testList.Add(new TestWrapperClass(bO3, Factory));

			var collection = (IBODocDataProviderCollection)(new TestWrapperClassCollection(testList, Factory));
			AssertEquals("Volume is totalled correctly", "26.69 M3", collection.Total("WrapperVolume", "2", null));
		}

		public void TestTotalWithSameUnits()
		{
			TestWrapperBO bO1 = new TestWrapperBO();
			bO1.VolumeValue = 12.345m;
			bO1.VolumeUnit = "L";

			TestWrapperBO bO2 = new TestWrapperBO();
			bO2.VolumeValue = 34.89m;
			bO2.VolumeUnit = "L";

			TestWrapperBO bO3 = new TestWrapperBO();
			bO3.VolumeValue = 98.472m;
			bO3.VolumeUnit = "L";

			List<TestWrapperClass> testList = new List<TestWrapperClass>();
			testList.Add(new TestWrapperClass(bO1, Factory));
			testList.Add(new TestWrapperClass(bO2, Factory));
			testList.Add(new TestWrapperClass(bO3, Factory));

			var collection = (IBODocDataProviderCollection)(new TestWrapperClassCollection(testList, Factory));
			AssertEquals("Volume is totalled correctly", "145.7070 L", collection.Total("WrapperVolume", "4", null));
		}

		public void TestTotalWithInvalidUnitLast()
		{
			TestWrapperBO bO1 = new TestWrapperBO();
			bO1.VolumeValue = 12.34m;
			bO1.VolumeUnit = "TE";

			TestWrapperBO bO2 = new TestWrapperBO();
			bO2.VolumeValue = 34.89m;
			bO2.VolumeUnit = "PKU";

			List<TestWrapperClass> testList = new List<TestWrapperClass>();
			testList.Add(new TestWrapperClass(bO1, Factory));
			testList.Add(new TestWrapperClass(bO2, Factory));

			var collection = (IBODocDataProviderCollection)(new TestWrapperClassCollection(testList, Factory));
			AssertEquals("Second Unit is invalid", ZString.Empty, collection.Total("WrapperVolume", "1", null));
		}

		public void TestTotalWithInvalidUnitFirst()
		{
			TestWrapperBO bO1 = new TestWrapperBO();
			bO1.VolumeValue = 12.34m;
			bO1.VolumeUnit = "MJN";

			TestWrapperBO bO2 = new TestWrapperBO();
			bO2.VolumeValue = 34.89m;
			bO2.VolumeUnit = "ML";

			List<TestWrapperClass> testList = new List<TestWrapperClass>();
			testList.Add(new TestWrapperClass(bO1, Factory));
			testList.Add(new TestWrapperClass(bO2, Factory));

			var collection = (IBODocDataProviderCollection)(new TestWrapperClassCollection(testList, Factory));
			AssertEquals("First Unit is invalid", ZString.Empty, collection.Total("WrapperVolume", "1", null));
		}

		public void TestTotalWithInvalidUnitsThroughout()
		{
			TestWrapperBO bO1 = new TestWrapperBO();
			bO1.VolumeValue = 12.34m;
			bO1.VolumeUnit = "LPK";

			TestWrapperBO bO2 = new TestWrapperBO();
			bO2.VolumeValue = 34.89m;
			bO2.VolumeUnit = "ML";

			TestWrapperBO bO3 = new TestWrapperBO();
			bO3.VolumeValue = 12.34m;
			bO1.VolumeUnit = "HJN";

			TestWrapperBO bO4 = new TestWrapperBO();
			bO4.VolumeValue = 34.89m;
			bO4.VolumeUnit = "CF";

			List<TestWrapperClass> testList = new List<TestWrapperClass>();
			testList.Add(new TestWrapperClass(bO1, Factory));
			testList.Add(new TestWrapperClass(bO2, Factory));
			testList.Add(new TestWrapperClass(bO3, Factory));
			testList.Add(new TestWrapperClass(bO4, Factory));

			var collection = (IBODocDataProviderCollection)(new TestWrapperClassCollection(testList, Factory));
			AssertEquals("First And Second Unit is invalid", ZString.Empty, collection.Total("WrapperVolume", "9", null));
		}

		public void TestTotalWithAllInvalidUnits()
		{
			TestWrapperBO bO1 = new TestWrapperBO();
			bO1.VolumeValue = 12.34m;
			bO1.VolumeUnit = "QQ";

			TestWrapperBO bO2 = new TestWrapperBO();
			bO2.VolumeValue = 34.89m;
			bO2.VolumeUnit = "QQ";

			TestWrapperBO bO3 = new TestWrapperBO();
			bO3.VolumeValue = 48.78m;
			bO3.VolumeUnit = "QQ";

			List<TestWrapperClass> testList = new List<TestWrapperClass>();
			testList.Add(new TestWrapperClass(bO1, Factory));
			testList.Add(new TestWrapperClass(bO2, Factory));
			testList.Add(new TestWrapperClass(bO3, Factory));

			var collection = (IBODocDataProviderCollection)(new TestWrapperClassCollection(testList, Factory));
			AssertEquals("All Units are invalid", "96.0 QQ", collection.Total("WrapperVolume", "1", null));
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new VolumeWrapper(ZDecimal.Zero, ZString.Empty, WeightsList, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Volume                    (Default Field: ValueAndUnitCodeBlankIfZero)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Unit                                    Unit
InCubicFeet                             Volume
InCubicMeters                           Volume
InFreightVolumeUnit                     Volume
InPackageVolumeUnit                     Volume
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
			public ZDecimal VolumeValue
			{
				get { return volumeValue; }
				set { volumeValue = value; }
			}
			ZDecimal volumeValue;

			public ZString VolumeUnit
			{
				get { return volumeUnit; }
				set { volumeUnit = value; }
			}
			ZString volumeUnit;
		}

		public class TestWrapperClass : GenericWrapper
		{
			public TestWrapperClass(TestWrapperBO testWrapperBO, BusinessObjectFactory factory)
				: base(testWrapperBO, factory)
			{
				TestWrapperBO = testWrapperBO;
			}
			readonly TestWrapperBO TestWrapperBO;

			public VolumeWrapper WrapperVolume
			{
				get { return wrapperVolume ?? (wrapperVolume = new VolumeWrapper(TestWrapperBO.VolumeValue, TestWrapperBO.VolumeUnit, new CodeDescriptionPairList(), Factory)); }
			}
			VolumeWrapper wrapperVolume;
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
