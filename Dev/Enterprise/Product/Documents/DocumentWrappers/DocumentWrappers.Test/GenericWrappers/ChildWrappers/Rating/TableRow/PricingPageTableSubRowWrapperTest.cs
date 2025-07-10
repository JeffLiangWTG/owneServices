using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[TestedType(typeof(PricingPageTableSubRowWrapper))]
	sealed class PricingPageTableSubRowWrapperTest : GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();
			RateEntry entry1 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL);

			PricingPage page = new PricingPage(entry1, Factory, PricingPageStyle.Landscape);

			PricingPageTableSubRowWrapper wrapper = new PricingPageTableSubRowWrapper(Factory);

			AssertEquals("Columns.Count", 0, wrapper.Columns.Count);
		}

		#region Implementation

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Charge :  is null
ConversionFactor :  is null
Currency :  is null
Registry : (No Default Field Value Available on Registry)
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();
			RateEntry entry1 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL);
			RateEntry entry2 = tariff.AddRateEntry(RatingConstants.RateCategory.LCL);

			PricingPage page = new PricingPage(entry1, Factory, PricingPageStyle.Landscape);
			page.AddRateEntry(entry2);

			return new PricingPageTableSubRowWrapper(Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Pricing Page Table Sub Row
======================================================================
Name                                    Type
----------------------------------------------------------------------
Charge                                  CodeAndDescription
Currency                                Currency
ConversionFactor                        Rating Conversion Factor
CaptionGroup                            Int
Index                                   Int
Split                                   String
UseOnlyActualWeightMeasure              Bool

Columns                                 Pricing Page Table Column Collection
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();
			RateEntry entry1 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL);
			RateEntry entry2 = tariff.AddRateEntry(RatingConstants.RateCategory.LCL);

			PricingPage page = new PricingPage(entry1, Factory, PricingPageStyle.Landscape);
			page.AddRateEntry(entry2);

			return new PricingPageTableSubRowWrapper(Factory);
		}

		#endregion
	}
}
