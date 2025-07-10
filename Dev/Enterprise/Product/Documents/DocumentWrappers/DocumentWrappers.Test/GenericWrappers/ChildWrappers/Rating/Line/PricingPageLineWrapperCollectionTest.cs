using System;
using System.Text;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.RatingEnums;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PricingPageLineWrapperCollection))]
	sealed class PricingPageLineWrapperCollectionTest : GenericWrapperCollectionTest<PricingPageLineWrapperCollection>
	{
		public void TestPopulate()
		{
			DocumentsDataRegistry.Instance.AlternativeRateFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			GlbCompany.CurrentCompany.SetCountry("AU");

			var tariff = Factory.New<CompanyTariff>();

			RateEntry entry1 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NL", "", "20GP");
			entry1.RateLines.RemoveAndDeleteAll();

			RateLine line1a = entry1.AddRateLine("FRT", UnitCalculator.Code, RatingConstants.Units.CN, "AUD");
			line1a.TL_RateDesc = "Freight";
			((UnitCalculator)line1a.Calculator).PerUnit = 150m;

			RateLine line1b = entry1.AddRateLine("BAF", UnitCalculator.Code, RatingConstants.Units.CN, "AUD");
			line1b.TL_RateDesc = "Baf";
			((UnitCalculator)line1b.Calculator).PerUnit = 15m;

			RateEntry entry2 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NL", "", "20RE");
			entry2.RateLines.RemoveAndDeleteAll();

			RateLine line2a = entry2.AddRateLine("FRT", UnitCalculator.Code, RatingConstants.Units.CN, "AUD");
			line2a.TL_RateDesc = "Freight";
			((UnitCalculator)line2a.Calculator).PerUnit = 200m;

			RateLine line2b = entry2.AddRateLine("BAF", UnitCalculator.Code, RatingConstants.Units.CN, "AUD");
			line2b.TL_RateDesc = "Baf";
			((UnitCalculator)line2b.Calculator).PerUnit = 20m;

			Factory.Save();

			var page = new PricingPage(entry1, Factory, PricingPageStyle.Standard);
			page.AddRateEntry(entry2);
			var collection = new PricingPageLineWrapperCollection(page, new PricingPageRateLineFactory(EntryTypes.Freight), Factory);

			const string expected = @"
[1,1]Freight||||Australia -> Netherlands
[1,2]20GP|AUD|150.00|per Container|Australia -> Netherlands
[1,3]20RE|AUD|200.00|per Container|Australia -> Netherlands
[2,1]Baf||||Australia -> Netherlands
[2,2]20GP|AUD|15.00|per Container|Australia -> Netherlands
[2,3]20RE|AUD|20.00|per Container|Australia -> Netherlands
";

			AssertMultilineASCIIEquals("", expected, Render(collection));
		}

		#region Implementation

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();
			RateEntry entry = tariff.AddRateEntry("FCL");
			RateLine line = entry.AddRateLine("FRT", FlatCalculator.Code);
			line.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)50m;

			PricingPage page = new PricingPage(entry, Factory, PricingPageStyle.Standard);

			return new PricingPageLineWrapper(page, QuotationLine.Header(line, QuotationLineType.Mandatory | QuotationLineType.UseChargeDescription), 1, 2, Factory);
		}

		protected override PricingPageLineWrapperCollection GetNewDocumentWrapperCollection()
		{
			var tariff = Factory.NewWithValidTestData<CompanyTariff>();
			var entry = tariff.AddRateEntry("FCL", "ALL", "NL", "AU");

			var lsFactory = new PricingPageRateLineFactory(EntryTypes.Freight);
			var page = new PricingPage(entry, Factory, PricingPageStyle.Standard);

			return new PricingPageLineWrapperCollection(page, lsFactory, Factory);
		}

		string Render(PricingPageLineWrapperCollection collection)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine();

			foreach (PricingPageLineWrapper wrapper in collection)
			{
				builder.Append('[');
				builder.Append(wrapper.SetIndexNum);
				builder.Append(',');
				builder.Append(wrapper.LineIndexNum);
				builder.Append(']');
				builder.Append(wrapper.Description);
				builder.Append('|');
				builder.Append(wrapper.Currency);
				builder.Append('|');
				builder.Append(wrapper.Amount);
				builder.Append('|');
				builder.Append(wrapper.Units);
				builder.Append('|');
				builder.Append(wrapper.Origin.Name);
				builder.Append(" -> ");
				builder.Append(wrapper.Destination.Name);
				builder.AppendLine();
			}

			return builder.ToString();
		}

		#endregion
	}
}
