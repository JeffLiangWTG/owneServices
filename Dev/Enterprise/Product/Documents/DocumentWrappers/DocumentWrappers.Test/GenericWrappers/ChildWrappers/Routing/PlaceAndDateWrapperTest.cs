using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PlaceAndDateWrapper))]
	sealed class PlaceAndDateWrapperTest : Base.Testing.GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			PlaceAndDateWrapper wrapperEmpty = new PlaceAndDateWrapper(ZString.Empty, ZDateTime.Empty, ZDateTime.Empty, Factory);
			AssertEquals(ZString.Empty, wrapperEmpty.Location.UNLOCO);
			AssertEquals(ZDateTime.Empty, wrapperEmpty.ActualDate);
			AssertEquals(ZDateTime.Empty, wrapperEmpty.Date);
			AssertEquals(ZDateTime.Empty, wrapperEmpty.EstimatedDate);
		}

		public void TestWrapperMappingSydney()
		{
			PlaceAndDateWrapper wrapperSydney = new PlaceAndDateWrapper("AUSYD", new ZDateTime(2006, 1, 2), new ZDateTime(2006, 2, 1), Factory);
			AssertEquals("AUSYD", wrapperSydney.Location.UNLOCO);
			AssertEquals(new ZDateTime(2006, 2, 1), wrapperSydney.ActualDate);
			AssertEquals(new ZDateTime(2006, 2, 1), wrapperSydney.Date);
			AssertEquals(new ZDateTime(2006, 1, 2), wrapperSydney.EstimatedDate);
		}

		public void TestWrapperMappingUnknown()
		{
			PlaceAndDateWrapper wrapperUnknown = new PlaceAndDateWrapper("XXDFDS", new ZDateTime(2006, 3, 3), ZDateTime.Empty, Factory);
			AssertEquals("XXDFDS", wrapperUnknown.Location.UNLOCO);
			AssertEquals(ZDateTime.Empty, wrapperUnknown.ActualDate);
			AssertEquals(new ZDateTime(2006, 3, 3), wrapperUnknown.Date);
			AssertEquals(new ZDateTime(2006, 3, 3), wrapperUnknown.EstimatedDate);
		}

		public void TestWrapperMappingJustADate()
		{
			PlaceAndDateWrapper wrapperJustADate = new PlaceAndDateWrapper(ZString.Empty, ZDateTime.Empty, new ZDateTime(2006, 3, 3), Factory);
			AssertEquals(ZString.Empty, wrapperJustADate.Location.UNLOCO);
			AssertEquals(new ZDateTime(2006, 3, 3), wrapperJustADate.ActualDate);
			AssertEquals(new ZDateTime(2006, 3, 3), wrapperJustADate.Date);
			AssertEquals(ZDateTime.Empty, wrapperJustADate.EstimatedDate);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
PlaceAndDate                                 (Default Field: Location)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Location                                Location
ActualDate                              DateTime
Date                                    DateTime
EstimatedDate                           DateTime
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Location : AUSYD - Sydney
Registry : (No Default Field Value Available on Registry)
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new PlaceAndDateWrapper("AUSYD", ZDateTime.Empty, ZDateTime.Empty, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new PlaceAndDateWrapper(ZString.Empty, ZDateTime.Empty, ZDateTime.Empty, Factory);
		}
	}
}
