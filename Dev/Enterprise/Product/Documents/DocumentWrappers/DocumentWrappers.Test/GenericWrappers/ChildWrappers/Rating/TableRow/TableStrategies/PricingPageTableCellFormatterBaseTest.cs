using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.DocumentPrinting.DocAmount;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	internal abstract class PricingPageTableCellFormatterBaseTest<T> : TestCaseWithFactory
			where T : PricingPageTableCellFormatter
	{
		#region Implementation

		protected void AssertSuccess(RateLine line, params string[] expected)
		{
			AssertSuccess("", line, expected);
		}
		protected void AssertSuccess(string message, RateLine line, params string[] expected)
		{
			AssertionCount++;

			var values = DocAmount.Create(count: expected.Length);
			FillSequential(values);

			bool expressable = Formatter.TryFormat(line, values);
			bool error = !expressable;

			if (!error)
			{
				for (int i = 0; i < values.Length; i++)
				{
					if (values[i].AmountAsString != expected[i])
					{
						error = true;
						break;
					}
				}
			}

			if (error)
			{
				StringBuilder builder = new StringBuilder();
				builder.Append(Html(message));
				builder.Append("<br /><br />");

				if (!expressable)
				{
					builder.Append("expected expressability: ");
					builder.Append(HtmlFormatGoodValue(true));
					builder.Append("actual expressability: ");
					builder.Append(HtmlFormatBadValue(false));
				}

				for (int i = 0; i < values.Length; i++)
				{
					if (values[i].AmountAsString != expected[i])
					{
						builder.Append("expected value[");
						builder.Append(i);
						builder.Append("]: ");
						builder.Append(HtmlFormatGoodValue(expected[i]));
						builder.Append("acutal value[");
						builder.Append(i);
						builder.Append("]: ");
						builder.Append(HtmlFormatBadValue(values[i]));
					}
				}

				HtmlFail(builder.ToString());
			}
		}

		protected void AssertFailure(RateLine line)
		{
			AssertFailure("", line);
		}
		protected void AssertFailure(string message, RateLine line)
		{
			const string SeeBelow = "See Below";

			AssertionCount++;

			var values = DocAmount.Create(count: Formatter.RequiredColumnCount);
			FillSequential(values);

			bool expressable = Formatter.TryFormat(line, values);
			bool error = expressable;

			if (!error)
			{
				if (values[0].AmountAsString != SeeBelow)
				{
					error = true;
				}
				else
				{
					for (int i = 1; i < values.Length; i++)
					{
						if (values[i] != null)
						{
							error = true;
							break;
						}
					}
				}
			}

			if (error)
			{
				StringBuilder builder = new StringBuilder();
				builder.Append(Html(message));
				builder.Append("<br /><br />");

				if (expressable)
				{
					builder.Append("expected expressability: ");
					builder.Append(HtmlFormatGoodValue(false));
					builder.Append("actual expressability: ");
					builder.Append(HtmlFormatBadValue(true));
				}

				for (int i = 0; i < values.Length; i++)
				{
					string expected = (i == 0 ? SeeBelow : null);

					if (values[i].AmountAsString != expected)
					{
						builder.Append("expected value[");
						builder.Append(i);
						builder.Append("]: ");
						builder.Append(HtmlFormatGoodValue(expected));
						builder.Append("acutal value[");
						builder.Append(i);
						builder.Append("]: ");
						builder.Append(HtmlFormatBadValue(values[i]));
					}
				}

				HtmlFail(builder.ToString());
			}
		}

		protected CompanyTariff Tariff
		{
			get { return tariff ?? (tariff = Factory.New<CompanyTariff>()); }
		}
		CompanyTariff tariff;

		protected RateEntry TariffEntry
		{
			get
			{
				if (entry == null)
				{
					entry = Tariff.AddRateEntry(RatingConstants.RateCategory.FCL);
					entry.RateLines.RemoveAndDeleteAll();
				}
				return entry;
			}
		}
		RateEntry entry;

		protected RateLine TariffLine
		{
			get { return line ?? (line = TariffEntry.AddRateLine("FRT", UnitCalculator.Code, RatingConstants.Units.CN, "AUD")); }
		}
		RateLine line;

		protected OrgHeader Client
		{
			get
			{
				if (client == null)
				{
					client = Factory.NewWithValidTestData<OrgHeader>();
					client.OH_Code = "Client";
					client.CompanyData.RateTariffLevels.SetLevel("DEF", Tariff.TH_GlobalRateLevel);
				}
				return client;
			}
		}
		OrgHeader client;

		protected ClientRate Rate
		{
			get
			{
				if (rate == null)
				{
					rate = Factory.New<ClientRate>();
					rate.TH_OH = Client.PK;
				}

				return rate;
			}
		}
		ClientRate rate;

		protected RateEntry RateEntry
		{
			get
			{
				if (rateEntry == null)
				{
					rateEntry = Rate.AddRateEntry(RatingConstants.RateCategory.FCL);
					rateEntry.RateLines.RemoveAndDeleteAll();
				}
				return rateEntry;
			}
		}
		RateEntry rateEntry;

		protected RateLine RateLine
		{
			get { return rateLine ?? (rateLine = RateEntry.AddRateLine("FRT", UnitCalculator.Code, RatingConstants.Units.CN, "AUD")); }
		}
		RateLine rateLine;

		protected T Formatter
		{
			get { return formatter ?? (formatter = NewFormatter()); }
		}
		protected abstract T NewFormatter();
		protected void ResetFormatter()
		{
			formatter = null;
		}
		T formatter;

		protected static void SetUnit(RateLine line, ZDecimal perUnit)
		{
			line.TL_RateCalculator = UnitCalculator.Code;
			((UnitCalculator)line.Calculator).PerUnit = perUnit;
		}

		protected static void SetFlat(RateLine line, ZDecimal baseRate)
		{
			line.TL_RateCalculator = FlatCalculator.Code;
			((FlatCalculator)line.Calculator).BaseRate = baseRate;
		}

		protected static void SetFlatPlusPerUnit(RateLine line, ZDecimal baseRate, ZDecimal perUnit)
		{
			line.TL_RateCalculator = FlatPlusPerUnitCalculator.Code;
			FlatPlusPerUnitCalculator calc = (FlatPlusPerUnitCalculator)line.Calculator;
			calc.BaseRate = baseRate;
			calc.PerUnit = perUnit;
		}

		protected static void SetMinimumOrPerUnit(RateLine line, ZDecimal minimum, ZDecimal perUnit)
		{
			line.TL_RateCalculator = MinimumOrPerUnitCalculator.Code;
			MinimumOrPerUnitCalculator calc = (MinimumOrPerUnitCalculator)line.Calculator;
			calc.Minimum = minimum;
			calc.PerUnit = perUnit;
		}

		protected static void SetFirstPlusAdditional(RateLine line, ZDecimal first, ZDecimal additional)
		{
			line.TL_RateCalculator = FirstPlusAdditionalCalculator.Code;
			FirstPlusAdditionalCalculator calc = (FirstPlusAdditionalCalculator)line.Calculator;
			calc.First = first;
			calc.Additional = additional;
		}

		protected static void SetCompanyTariffBased(RateLine line, ZDecimal minimum, ZDecimal baseRate, ZDecimal percent, ZDecimal perUnit)
		{
			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			CompanyTariffOrCostBasedCalculator calc = (CompanyTariffOrCostBasedCalculator)line.Calculator;
			calc.Minimum = minimum;
			calc.BaseRate = baseRate;
			calc.Percent = percent;
			calc.PerUnit = perUnit;
		}

		void FillSequential(DocAmount[] values)
		{
			for (int i = 0; i < values.Length; i++)
			{
				values[i] = DocAmount.Create((NoResString)i.ToString());
			}
		}

		#endregion
	}
}
