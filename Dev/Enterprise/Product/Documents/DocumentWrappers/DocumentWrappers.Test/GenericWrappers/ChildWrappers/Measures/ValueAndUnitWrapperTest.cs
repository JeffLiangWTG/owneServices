using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ValueAndUnitWrapper))]
	internal class ValueAndUnitWrapperTest : Base.Testing.GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			ValueAndUnitWrapper wrapperEmpty = new ValueAndUnitWrapper(ZDecimal.Zero, ZString.Empty, ZInt.Zero, WeightsList, Factory);
			AssertEquals("wrapperEmpty.Unit.Code", ZString.Empty, wrapperEmpty.Unit.Code);
			AssertEquals("wrapperEmpty.Value", ZDecimal.Zero, wrapperEmpty.Value);
			AssertEquals("wrapperEmpty.ValueAndUnitCode", "0", wrapperEmpty.ValueAndUnitCode);
			AssertEquals("wrapperEmpty.ValueAndUnitCodeBlankIfZero", ZString.Empty, wrapperEmpty.ValueAndUnitCodeBlankIfZero);
		}

		public void TestWrapperKilos()
		{
			ValueAndUnitWrapper wrapperKilos = new ValueAndUnitWrapper(234.5m, Constants.Weight.Kilograms, 2, WeightsList, Factory);
			AssertEquals("wrapperKilos.Unit.Code", Constants.Weight.Kilograms, wrapperKilos.Unit.Code);
			AssertEquals("wrapperKilos.Value", 234.5m, wrapperKilos.Value);
			AssertEquals("wrapperKilos.ValueAndUnitCode", "234.50 KG", wrapperKilos.ValueAndUnitCode);
			AssertEquals("wrapperKilos.ValueAndUnitCodeBlankIfZero", "234.50 KG", wrapperKilos.ValueAndUnitCodeBlankIfZero);
		}

		public void TestWrapperCalculcatedDecimals()
		{
			ValueAndUnitWrapper wrapper = new ValueAndUnitWrapper(123.4567m, Constants.Weight.Kilograms, WeightsList, Factory);
			AssertEquals(1, wrapper.DecimalPlaces);

			wrapper = new ValueAndUnitWrapper(123.4567m, Constants.Volume.CubicMetres, WeightsList, Factory);
			AssertEquals(3, wrapper.DecimalPlaces);

			wrapper = new ValueAndUnitWrapper(123.4567m, "XX", WeightsList, Factory);
			AssertEquals(0, wrapper.DecimalPlaces);
		}

		public void TestWrapperPounds()
		{
			ValueAndUnitWrapper wrapperPounds = new ValueAndUnitWrapper(789.5555m, Constants.Weight.Pounds, 1, WeightsList, Factory);
			AssertEquals("wrapperPounds.Unit.Code", Constants.Weight.Pounds, wrapperPounds.Unit.Code);
			AssertEquals("wrapperPounds.Value", 789.6m, wrapperPounds.Value);
			AssertEquals("wrapperPounds.ValueAndUnitCode", "789.6 LB", wrapperPounds.ValueAndUnitCode);
			AssertEquals("wrapperPounds.ValueAndUnitCodeBlankIfZero", "789.6 LB", wrapperPounds.ValueAndUnitCodeBlankIfZero);
		}

		public void TestWrapperUnKnown()
		{
			ValueAndUnitWrapper wrapperUnKnown = new ValueAndUnitWrapper(89.4m, "XXZX", ZInt.Zero, WeightsList, Factory);
			AssertEquals("wrapperUnKnown.Unit.Code", "XXZX", wrapperUnKnown.Unit.Code);
			AssertEquals("wrapperUnKnown.Value", 89m, wrapperUnKnown.Value);
			AssertEquals("wrapperUnKnown.ValueAndUnitCode", "89 XXZX", wrapperUnKnown.ValueAndUnitCode);
			AssertEquals("wrapperUnKnown.ValueAndUnitCodeBlankIfZero", "89 XXZX", wrapperUnKnown.ValueAndUnitCodeBlankIfZero);
		}

		public void TestWrapperZeroKilos()
		{
			ValueAndUnitWrapper wrapperZeroKilos = new ValueAndUnitWrapper(0m, Constants.Weight.Kilograms, 2, WeightsList, Factory);
			AssertEquals("wrapperZeroKilos.Unit.Code", Constants.Weight.Kilograms, wrapperZeroKilos.Unit.Code);
			AssertEquals("wrapperZeroKilos.Value", 0m, wrapperZeroKilos.Value);
			AssertEquals("wrapperZeroKilos.ValueAndUnitCode", "0.00 KG", wrapperZeroKilos.ValueAndUnitCode);
			AssertEquals("wrapperZeroKilos.ValueAndUnitCodeBlankIfZero", ZString.Empty, wrapperZeroKilos.ValueAndUnitCodeBlankIfZero);
		}

		public void TestFormatValueAndUnitCode()
		{
			ValueAndUnitWrapper wrapper1 = new ValueAndUnitWrapper(234.5m, Constants.Weight.Kilograms, 2, WeightsList, Factory);
			AssertEquals("wrapper1.FormatValueAndUnitCode", "234.5 KG", wrapper1.FormatValueAndUnitCode);
			AssertEquals("wrapper1.FormatValueAndUnitCodeBlankIfZero", "234.5 KG", wrapper1.FormatValueAndUnitCodeBlankIfZero);

			ValueAndUnitWrapper wrapper2 = new ValueAndUnitWrapper(0m, Constants.Weight.Kilograms, 2, WeightsList, Factory);
			AssertEquals("wrapper2.FormatValueAndUnitCode", "0 KG", wrapper2.FormatValueAndUnitCode);
			AssertEquals("wrapper2.FormatValueAndUnitCodeBlankIfZero", ZString.Empty, wrapper2.FormatValueAndUnitCodeBlankIfZero);
		}

		public void TestRightToLeft()
		{
			using (Res.TemporarilySwitchLanguage(Core.Constants.Languages.Arabic))
			{
				ValueAndUnitWrapper wrapperKilos = new ValueAndUnitWrapper(234.5m, Constants.Weight.Kilograms, 2, WeightsList, Factory);
				AssertEquals("wrapperKilos.ValueAndUnitCode", "KG 234.50", wrapperKilos.ValueAndUnitCode);
				AssertEquals("wrapperKilos.ValueAndUnitCodeBlankIfZero", "KG 234.50", wrapperKilos.ValueAndUnitCodeBlankIfZero);
			}
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
ValueAndUnit              (Default Field: ValueAndUnitCodeBlankIfZero)
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

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Registry : (No Default Field Value Available on Registry)
Unit : KG - Kilograms
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new ValueAndUnitWrapper(234.5m, Constants.Weight.Kilograms, 2, WeightsList, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new ValueAndUnitWrapper(ZDecimal.Zero, ZString.Empty, ZInt.Zero, new DummyListForTesting(), Factory);
		}

		internal CodeDescriptionPairList WeightsList
		{
			get
			{
				if (fWeightsList == null)
				{
					fWeightsList = new CodeDescriptionPairList(OLookUpEditType.Weight);
				}
				return fWeightsList;
			}
		}
		CodeDescriptionPairList fWeightsList;
	}
}
