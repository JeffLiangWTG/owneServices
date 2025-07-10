using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(DimensionsWrapper))]
	sealed class DimensionsWrapperTest : Base.Testing.GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			DimensionsWrapper wrapperEmpty = new DimensionsWrapper(ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZString.Empty, DimensionsList, Factory);
			AssertEquals("wrapperEmpty.Unit.Code", ZString.Empty, wrapperEmpty.Unit.Code);
			AssertEquals("wrapperEmpty.Value", "0 x 0 x 0", wrapperEmpty.Value);
			AssertEquals("wrapperEmpty.ValueAndUnitCode", "0 x 0 x 0", wrapperEmpty.ValueAndUnitCode);
			AssertEquals("wrapperEmpty.ValueAndUnitCodeBlankIfZero", ZString.Empty, wrapperEmpty.ValueAndUnitCodeBlankIfZero);
		}

		public void TestWrapperMetres()
		{
			DimensionsWrapper wrapperMetres = new DimensionsWrapper(2.5m, 3.45m, 1m, Constants.Length.Metres, 2, DimensionsList, Factory);
			AssertEquals("wrapperMetres.Unit.Code", Constants.Length.Metres, wrapperMetres.Unit.Code);
			AssertEquals("wrapperMetres.Value", "2.5 x 3.45 x 1", wrapperMetres.Value);
			AssertEquals("wrapperMetres.ValueAndUnitCode", "2.5 x 3.45 x 1 M", wrapperMetres.ValueAndUnitCode);
			AssertEquals("wrapperMetres.ValueAndUnitCodeBlankIfZero", "2.5 x 3.45 x 1 M", wrapperMetres.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperMetres.Length", 2.5m, wrapperMetres.Length);
			AssertEquals("wrapperMetres.LengthAndUnitCode", "2.5 M", wrapperMetres.LengthAndUnitCode);
			AssertEquals("wrapperMetres.LengthAndUnitCodeBlankIfZero", "2.5 M", wrapperMetres.LengthAndUnitCodeBlankIfZero);
			AssertEquals("wrapperMetres.Width", 3.45m, wrapperMetres.Width);
			AssertEquals("wrapperMetres.WidthAndUnitCode", "3.45 M", wrapperMetres.WidthAndUnitCode);
			AssertEquals("wrapperMetres.WidthAndUnitCodeBlankIfZero", "3.45 M", wrapperMetres.WidthAndUnitCodeBlankIfZero);
			AssertEquals("wrapperMetres.Height", 1m, wrapperMetres.Height);
			AssertEquals("wrapperMetres.HeightAndUnitCode", "1 M", wrapperMetres.HeightAndUnitCode);
			AssertEquals("wrapperMetres.HeightAndUnitCodeBlankIfZero", "1 M", wrapperMetres.HeightAndUnitCodeBlankIfZero);
		}

		public void TestWrapperDecimals()
		{
			DimensionsWrapper wrapper = new DimensionsWrapper(123.4567m, 12.3m, 1m, Constants.Length.Metres, DimensionsList, Factory);
			AssertEquals(2, wrapper.DecimalPlaces);

			AssertEquals("Length should display 2 decimal places", "123.46 M", wrapper.LengthAndUnitCode);
			AssertEquals("Width should display 1 decimal places", "12.3 M", wrapper.WidthAndUnitCode);
			AssertEquals("Height should display 0 decimal places", "1 M", wrapper.HeightAndUnitCode);
		}

		public void TestWrapperPounds()
		{
			DimensionsWrapper wrapperFeet = new DimensionsWrapper(2.5m, 3.45m, 1m, Constants.Length.Feet, 2, DimensionsList, Factory);
			AssertEquals("wrapperFeet.Unit.Code", Constants.Length.Feet, wrapperFeet.Unit.Code);
			AssertEquals("wrapperFeet.Value", "2.5 x 3.45 x 1", wrapperFeet.Value);
			AssertEquals("wrapperFeet.ValueAndUnitCode", "2.5 x 3.45 x 1 FT", wrapperFeet.ValueAndUnitCode);
			AssertEquals("wrapperFeet.ValueAndUnitCodeBlankIfZero", "2.5 x 3.45 x 1 FT", wrapperFeet.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperFeet.Length", 2.5m, wrapperFeet.Length);
			AssertEquals("wrapperFeet.LengthAndUnitCode", "2.5 FT", wrapperFeet.LengthAndUnitCode);
			AssertEquals("wrapperFeet.LengthAndUnitCodeBlankIfZero", "2.5 FT", wrapperFeet.LengthAndUnitCodeBlankIfZero);
			AssertEquals("wrapperFeet.Width", 3.45m, wrapperFeet.Width);
			AssertEquals("wrapperFeet.WidthAndUnitCode", "3.45 FT", wrapperFeet.WidthAndUnitCode);
			AssertEquals("wrapperFeet.WidthAndUnitCodeBlankIfZero", "3.45 FT", wrapperFeet.WidthAndUnitCodeBlankIfZero);
			AssertEquals("wrapperFeet.Height", 1m, wrapperFeet.Height);
			AssertEquals("wrapperFeet.HeightAndUnitCode", "1 FT", wrapperFeet.HeightAndUnitCode);
			AssertEquals("wrapperFeet.HeightAndUnitCodeBlankIfZero", "1 FT", wrapperFeet.HeightAndUnitCodeBlankIfZero);
		}

		public void TestWrapperUnknown()
		{
			DimensionsWrapper wrapperUnknown = new DimensionsWrapper(2.5m, 3.45m, 1m, "XXZX", 2, DimensionsList, Factory);
			AssertEquals("wrapperUnknown.Unit.Code", "XXZX", wrapperUnknown.Unit.Code);
			AssertEquals("wrapperUnknown.Value", "2.5 x 3.45 x 1", wrapperUnknown.Value);
			AssertEquals("wrapperUnknown.ValueAndUnitCode", "2.5 x 3.45 x 1 XXZX", wrapperUnknown.ValueAndUnitCode);
			AssertEquals("wrapperUnknown.ValueAndUnitCodeBlankIfZero", "2.5 x 3.45 x 1 XXZX", wrapperUnknown.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperUnknown.Length", 2.5m, wrapperUnknown.Length);
			AssertEquals("wrapperUnknown.LengthAndUnitCode", "2.5 XXZX", wrapperUnknown.LengthAndUnitCode);
			AssertEquals("wrapperUnknown.LengthAndUnitCodeBlankIfZero", "2.5 XXZX", wrapperUnknown.LengthAndUnitCodeBlankIfZero);
			AssertEquals("wrapperUnknown.Width", 3.45m, wrapperUnknown.Width);
			AssertEquals("wrapperUnknown.WidthAndUnitCode", "3.45 XXZX", wrapperUnknown.WidthAndUnitCode);
			AssertEquals("wrapperUnknown.WidthAndUnitCodeBlankIfZero", "3.45 XXZX", wrapperUnknown.WidthAndUnitCodeBlankIfZero);
			AssertEquals("wrapperUnknown.Height", 1m, wrapperUnknown.Height);
			AssertEquals("wrapperUnknown.HeightAndUnitCode", "1 XXZX", wrapperUnknown.HeightAndUnitCode);
			AssertEquals("wrapperUnknown.HeightAndUnitCodeBlankIfZero", "1 XXZX", wrapperUnknown.HeightAndUnitCodeBlankIfZero);
		}

		public void TestWrapperZeroMetres()
		{
			DimensionsWrapper wrapperZeroMetres = new DimensionsWrapper(0m, 0m, 0m, Constants.Length.Metres, 2, DimensionsList, Factory);
			AssertEquals("wrapperZeroMetres.Unit.Code", Constants.Length.Metres, wrapperZeroMetres.Unit.Code);
			AssertEquals("wrapperZeroMetres.Value", "0 x 0 x 0", wrapperZeroMetres.Value);
			AssertEquals("wrapperZeroMetres.ValueAndUnitCode", "0 x 0 x 0 M", wrapperZeroMetres.ValueAndUnitCode);
			AssertEquals("wrapperZeroMetres.ValueAndUnitCodeBlankIfZero", ZString.Empty, wrapperZeroMetres.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperZeroMetres.Length", 0m, wrapperZeroMetres.Length);
			AssertEquals("wrapperZeroMetres.LengthAndUnitCode", "0 M", wrapperZeroMetres.LengthAndUnitCode);
			AssertEquals("wrapperZeroMetres.LengthAndUnitCodeBlankIfZero", ZString.Empty, wrapperZeroMetres.LengthAndUnitCodeBlankIfZero);
			AssertEquals("wrapperZeroMetres.Width", 0m, wrapperZeroMetres.Width);
			AssertEquals("wrapperZeroMetres.WidthAndUnitCode", "0 M", wrapperZeroMetres.WidthAndUnitCode);
			AssertEquals("wrapperZeroMetres.WidthAndUnitCodeBlankIfZero", ZString.Empty, wrapperZeroMetres.WidthAndUnitCodeBlankIfZero);
			AssertEquals("wrapperZeroMetres.Height", 0m, wrapperZeroMetres.Height);
			AssertEquals("wrapperZeroMetres.HeightAndUnitCode", "0 M", wrapperZeroMetres.HeightAndUnitCode);
			AssertEquals("wrapperZeroMetres.HeightAndUnitCodeBlankIfZero", ZString.Empty, wrapperZeroMetres.HeightAndUnitCodeBlankIfZero);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Dimensions                (Default Field: ValueAndUnitCodeBlankIfZero)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Unit                                    Unit
Height                                  Decimal
HeightAndUnitCode                       String
HeightAndUnitCodeBlankIfZero            String
Length                                  Decimal
LengthAndUnitCode                       String
LengthAndUnitCodeBlankIfZero            String
Value                                   String
ValueAndUnitCode                        String
ValueAndUnitCodeBlankIfZero             String
Width                                   Decimal
WidthAndUnitCode                        String
WidthAndUnitCodeBlankIfZero             String
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Registry : (No Default Field Value Available on Registry)
Unit : M - Meters
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new DimensionsWrapper(234.5m, 123.4m, 12.3m, Constants.Length.Metres, 2, DimensionsList, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new DimensionsWrapper(ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZString.Empty, ZInt.Zero, new DummyListForTesting(), Factory);
		}

		CodeDescriptionPairList DimensionsList
		{
			get
			{
				if (dimensionsList == null)
				{
					dimensionsList = new CodeDescriptionPairList(OLookUpEditType.Length);
				}
				return dimensionsList;
			}
		}
		CodeDescriptionPairList dimensionsList;
	}
}
