using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers.Quotation
{
	public class DocQuotationLineCollection : DocumentWrapperCollection<DocRateLineItem>
	{
		public DocQuotationLineCollection(DocQuotation quotation, PricingPageRateLineFactory lineSetFactory, IEnumerable<RateEntry> entries, BusinessObjectFactory factory)
			: base(factory)
		{
			this.Quotation = quotation;
			this.entries = entries;
			this.lineSetFactory = lineSetFactory;
		}

		public DocQuotation Quotation { get; }

		public override void Load()
		{
			if (Quotation != null && lineSetFactory != null)
			{
				// matchContractNumber=false is temporary: this class doesn't seem to be used anymore, while LoadLineSets should match ContractNumber
				List<PricingPageRateLineList> lineSets = lineSetFactory.LoadLineSets(Quotation.Page, entries);
				AddDocRateLineItems(lineSets, Quotation.Page.ViewAgentRates);
				RemovePercentagesOfZeroRateLine();
			}
		}

		#region Implementation

		void AddDocRateLineItems(IEnumerable<PricingPageRateLineList> lineSets, bool viewAgentRates)
		{
			PricingPageRateLineList prevList = null;
			QuotationLineListHelper helper = new QuotationLineListHelper();

			foreach (PricingPageRateLineList set in lineSets)
			{
				QuotationLineList list = helper.GetLines(set, viewAgentRates);

				if (list.Count > 0)
				{
					if (prevList != null)
					{
						CreateSpacingDocRateLines(prevList[0]);
					}

					foreach (QuotationLine line in list)
					{
						Add(DocRateLineItem.New(line, Quotation, Factory));
					}

					prevList = set;
				}
			}
		}

		/// <summary>
		/// Used to create white space lines between each charge / rate line in the quotation.
		/// </summary>
		void CreateSpacingDocRateLines(RateLine line)
		{
			int numSpacingLines = Env.Registry.Rating.RateLineSpacing;

			for (int i = 0; i < numSpacingLines; i++)
			{
				Add(DocRateLineItem.New(QuotationLine.Spacing(line), Quotation, Factory));
			}
		}

		void RemovePercentagesOfZeroRateLine()
		{
			for (int i = Count - 1; i >= 0; i--)
			{
				RateLine line = this[i].QuotationLine.Master;

				if (IsPercentageOfZeroRateLine(line))
				{
					if (line.ChargeCode.AC_SuppressOnQuoteIfZero)
					{
						Remove(this[i]);
					}
					else
					{
						int j = i - 1;

						while (j >= 0 && this[j].QuotationLine.Master.PK == line.PK)
						{
							j--;
						}

						while (i > j + 1)
						{
							Remove(this[i]);
							i--;
						}

						this[i].QuotationLine.MakeEmpty();
					}
				}
			}
		}

		bool IsPercentageOfZeroRateLine(RateLine line)
		{
			var calculator = line.Calculator as PercentageCalculator;
			if (calculator != null &&
				calculator.ValueApplyTo == CalculatorConstants.Text.ChargeCode &&
				calculator.Minimum == 0m &&
				calculator.BaseRate == 0m &&
				calculator.Percent != 0m)
			{
				var percentOf = line.RateLineItems.Cast<RateLineItem>().FirstOrDefault(item => item.RateOperatorIsApplyTo())?.ChargeCode;
				if (percentOf != null && line.Lookups.IsChargeGroupApplicableToRateLine(percentOf.AC_ChargeGroup))
				{
					foreach (DocRateLineItem item in this)
					{
						if (item.QuotationLine.Master.TL_AC == percentOf.PK)
						{
							var isZeroRateLine = !item.QuotationLine.Master.HasValueForDocumentPrinting();
							return isZeroRateLine;
						}
					}

					return true;
				}
			}

			return false;
		}

		readonly IEnumerable<RateEntry> entries;
		readonly PricingPageRateLineFactory lineSetFactory;

		#endregion
	}
}
