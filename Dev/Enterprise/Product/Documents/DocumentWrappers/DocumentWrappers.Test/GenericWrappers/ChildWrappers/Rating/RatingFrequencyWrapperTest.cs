using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(RatingFrequencyWrapper))]
	sealed class RatingFrequencyWrapperTest : GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			RatingFrequencyWrapper wrapper = new RatingFrequencyWrapper(0, FrequencyList.Codes.Empty, Factory);
			AssertEquals("wrapper.Value", 0, wrapper.Value);
			AssertEquals("wrapper.Unit.Code", "", wrapper.Unit.Code);
			AssertEquals("wrapper.Unit.Description", "", wrapper.Unit.Description);
			AssertEquals("wrapper.FriendlyText", "", wrapper.FriendlyText);
		}

		public void TestPopulated()
		{
			RatingFrequencyWrapper wrapper = new RatingFrequencyWrapper(2, FrequencyList.Codes.Days, Factory);
			AssertEquals("wrapper.Value", 2, wrapper.Value);
			AssertEquals("wrapper.Unit.Code", "Days", wrapper.Unit.Code);
			AssertEquals("wrapper.Unit.Description", "Every X days", wrapper.Unit.Description);
			AssertEquals("wrapper.FriendlyText", "Every 2 Days", wrapper.FriendlyText);
		}

		public void TestWrapperFriendlyText()
		{
			AssertEquals("2 per Day", new RatingFrequencyWrapper(2, FrequencyList.Codes.Daily, Factory).FriendlyText);
			AssertEquals("3 per Week", new RatingFrequencyWrapper(3, FrequencyList.Codes.Week, Factory).FriendlyText);
			AssertEquals("4 per Fortnight", new RatingFrequencyWrapper(4, FrequencyList.Codes.Fortnight, Factory).FriendlyText);
			AssertEquals("5 per Month", new RatingFrequencyWrapper(5, FrequencyList.Codes.Monthly, Factory).FriendlyText);

			AssertEquals("Every Day", new RatingFrequencyWrapper(1, FrequencyList.Codes.Days, Factory).FriendlyText);
			AssertEquals("Every 2 Days", new RatingFrequencyWrapper(2, FrequencyList.Codes.Days, Factory).FriendlyText);
		}

		public void TestIgnoreCodeCasing()
		{
			// One off quotations provide the code in uppercase.
			AssertEquals("2 per Day", new RatingFrequencyWrapper(2, FrequencyList.Codes.Daily.ToUpper(), Factory).FriendlyText);
			AssertEquals("3 per Week", new RatingFrequencyWrapper(3, FrequencyList.Codes.Week.ToUpper(), Factory).FriendlyText);
			AssertEquals("4 per Fortnight", new RatingFrequencyWrapper(4, FrequencyList.Codes.Fortnight.ToUpper(), Factory).FriendlyText);
			AssertEquals("5 per Month", new RatingFrequencyWrapper(5, FrequencyList.Codes.Monthly.ToUpper(), Factory).FriendlyText);

			AssertEquals("Every Day", new RatingFrequencyWrapper(1, FrequencyList.Codes.Days.ToUpper(), Factory).FriendlyText);
			AssertEquals("Every 2 Days", new RatingFrequencyWrapper(2, FrequencyList.Codes.Days.ToUpper(), Factory).FriendlyText);
		}

		#region Implementation

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Registry : (No Default Field Value Available on Registry)
Unit : Daily - X per day
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new RatingFrequencyWrapper(1, FrequencyList.Codes.Daily, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Rating Frequency                         (Default Field: FriendlyText)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Unit                                    Unit
FriendlyText                            String
Value                                   Int
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new RatingFrequencyWrapper(1, FrequencyList.Codes.Daily, Factory);
		}

		#endregion
	}
}
