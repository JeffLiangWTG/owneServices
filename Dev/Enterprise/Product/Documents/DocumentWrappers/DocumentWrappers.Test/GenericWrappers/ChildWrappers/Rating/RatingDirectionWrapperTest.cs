using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(RatingDirectionWrapper))]
	sealed class RatingDirectionWrapperTest : GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			RatingDirectionWrapper wrapper = new RatingDirectionWrapper(false, false, Factory);
			AssertEquals("wrapper.IsOriginLocal", false, wrapper.IsOriginLocal);
			AssertEquals("wrapper.IsDestinationLocal", false, wrapper.IsDestinationLocal);
			AssertEquals("wrapper.Code", "CXT", wrapper.Code);
			AssertEquals("wrapper.FriendlyText", "Cross Trade", wrapper.Description);
		}

		public void TestPopulated()
		{
			CombineAssertions(delegate
			{
				RatingDirectionWrapper wrapper = new RatingDirectionWrapper(true, false, Factory);
				AssertEquals("wrapper.IsOriginLocal", true, wrapper.IsOriginLocal);
				AssertEquals("wrapper.IsDestinationLocal", false, wrapper.IsDestinationLocal);
				AssertEquals("wrapper.Code", "EXP", wrapper.Code);
				AssertEquals("wrapper.Description", "Export", wrapper.Description);
			});

			CombineAssertions(delegate
			{
				RatingDirectionWrapper wrapper = new RatingDirectionWrapper(false, true, Factory);
				AssertEquals("wrapper.IsOriginLocal", false, wrapper.IsOriginLocal);
				AssertEquals("wrapper.IsDestinationLocal", true, wrapper.IsDestinationLocal);
				AssertEquals("wrapper.Code", "IMP", wrapper.Code);
				AssertEquals("wrapper.Description", "Import", wrapper.Description);
			});

			CombineAssertions(delegate
			{
				RatingDirectionWrapper wrapper = new RatingDirectionWrapper(true, true, Factory);
				AssertEquals("wrapper.IsOriginLocal", true, wrapper.IsOriginLocal);
				AssertEquals("wrapper.IsDestinationLocal", true, wrapper.IsDestinationLocal);
				AssertEquals("wrapper.Code", "DOM", wrapper.Code);
				AssertEquals("wrapper.Description", "Domestic", wrapper.Description);
			});
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
			return new RatingDirectionWrapper(true, false, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Rating Direction                          (Default Field: Description)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Code                                    String
Description                             String
IsDestinationLocal                      Bool
IsOriginLocal                           Bool
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new RatingDirectionWrapper(false, true, Factory);
		}

		#endregion
	}
}
