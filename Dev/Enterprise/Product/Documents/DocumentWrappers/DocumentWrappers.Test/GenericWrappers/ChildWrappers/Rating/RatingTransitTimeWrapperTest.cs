using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(RatingTransitTimeWrapper))]
	sealed class RatingTransitTimeWrapperTest : GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			RatingTransitTimeWrapper wrapper = new RatingTransitTimeWrapper("", Factory);
			AssertEquals("", wrapper.Value);
			AssertEquals("", wrapper.FriendlyText);
		}

		public void TestFriendlyText()
		{
			AssertEquals("Overnight", new RatingTransitTimeWrapper(RatingConstants.TransitTimes.Overnight, Factory).FriendlyText);
			AssertEquals("Same Day", new RatingTransitTimeWrapper(RatingConstants.TransitTimes.SameDay, Factory).FriendlyText);
			AssertEquals("1 Day", new RatingTransitTimeWrapper("1", Factory).FriendlyText);
			AssertEquals("2 Days", new RatingTransitTimeWrapper("2", Factory).FriendlyText);
			AssertEquals("48 Days", new RatingTransitTimeWrapper("48", Factory).FriendlyText);
		}

		#region Implementation

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Registry : (No Default Field Value Available on Registry)
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new RatingTransitTimeWrapper(RatingConstants.TransitTimes.Overnight, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Rating Transit Time                      (Default Field: FriendlyText)
======================================================================
Name                                    Type
----------------------------------------------------------------------
FriendlyText                            String
Value                                   String
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new RatingTransitTimeWrapper(RatingConstants.TransitTimes.Overnight, Factory);
		}

		#endregion
	}
}
