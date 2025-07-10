using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PricingPageTableRowWrapper))]
	sealed class PricingPageTableRowWrapperTest : GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			var tariff = Factory.New<CompanyTariff>();
			var entry1 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL);
			var wrapper = new PricingPageTableRowWrapper(new RateEntry[] { entry1 }, Factory);

			AssertEquals("SubRows.Count", 0, wrapper.SubRows.Count);
			AssertEquals("OtherCharges.Count", 0, wrapper.OtherCharges.Count);
		}

		#region Implementation

		protected override ZString ExpectedDefaultFormatting
			=> @"
Registry : (No Default Field Value Available on Registry)
";

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var tariff = Factory.New<CompanyTariff>();
			var entry1 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL);
			var entry2 = tariff.AddRateEntry(RatingConstants.RateCategory.LCL);

			var page = new PricingPage(entry1, Factory, PricingPageStyle.Landscape);
			page.AddRateEntry(entry2);

			return new PricingPageTableRowWrapper(new RateEntry[] { entry1, entry2 }, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Pricing Page Table Row
======================================================================
Name                                    Type
----------------------------------------------------------------------
Index                                   Int

OtherCharges                            Pricing Page Line Collection
SubRows                                 Pricing Page Table Sub Row Collection
Entries                                 Rate Entry Collection
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			var tariff = Factory.New<CompanyTariff>();
			var entry1 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL);
			var entry2 = tariff.AddRateEntry(RatingConstants.RateCategory.LCL);

			var page = new PricingPage(entry1, Factory, PricingPageStyle.Landscape);
			page.AddRateEntry(entry2);

			return new PricingPageTableRowWrapper(new RateEntry[] { entry1, entry2 }, Factory);
		}

		#endregion
	}
}
