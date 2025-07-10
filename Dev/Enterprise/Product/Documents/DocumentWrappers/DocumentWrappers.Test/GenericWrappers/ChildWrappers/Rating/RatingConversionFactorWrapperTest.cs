using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(RatingConversionFactorWrapper))]
	sealed class RatingConversionFactorWrapperTest : GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			RatingConversionFactorWrapper wrapper = new RatingConversionFactorWrapper(ConversionFactor.Empty, Factory);
			AssertEquals("wrapper.Metric", "", wrapper.Metric);
		}

		public void TestPopulated()
		{
			CombineAssertions(delegate
			{
				var validFactor = new ConversionFactor(166, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
				var wrapper = new RatingConversionFactorWrapper(validFactor, Factory);
				AssertEquals("wrapper.Metric", validFactor.ToShortString(), wrapper.Metric);
			});

			CombineAssertions(delegate
			{
				var invalidFactor = new ConversionFactor(0, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
				var wrapper = new RatingConversionFactorWrapper(invalidFactor, Factory);
				AssertEquals("wrapper.Metric", ZString.Empty, wrapper.Metric);
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
			return new RatingConversionFactorWrapper(new ConversionFactor(1000m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres), Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Rating Conversion Factor                       (Default Field: Metric)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Metric                                  String
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new RatingConversionFactorWrapper(new ConversionFactor(1000m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres), Factory);
		}

		#endregion
	}
}
