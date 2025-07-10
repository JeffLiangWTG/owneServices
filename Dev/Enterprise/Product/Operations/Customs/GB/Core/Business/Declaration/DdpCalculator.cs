using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class DdpCalculator
	{
		public static DdpCalculator Calculate(JobDeclaration declaration)
		{
			var ddpInvoices = declaration.Invoices.Where(x => x.JZ_IncoTerm == "DDP" && !x.JZ_RX_NKInvoice_Currency.IsEmpty).ToArray();
			return ddpInvoices.Length == 0 ? null : new DdpCalculator(ddpInvoices, new EU.Business.Declaration.DutyCalculatorStrategy(declaration));
		}

		DdpCalculator(BaseJobComInvoiceHeader[] invoices, EU.Business.Declaration.DutyCalculatorStrategy dutyCalculatorStrategy)
		{
			foreach (var invoice in invoices)
			{
				foreach (var line in invoice.InvoiceLines.Cast<JobComInvoiceLine>())
				{
					if (line.JI_Tariff.IsEmpty || line.JI_LinePrice.IsEmpty || line.JI_CountryOfOrigin.IsEmpty || line.JI_PrimaryPreference.IsEmpty)
					{
						linesMissingCommodityPreferenceOriginOrPrice.Add(line);
					}
					else if (((IEnumerable<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument>)line.SupportingDocuments).Any(x => x.CSI_Code == "9WKS"))
					{
						linesAlreadyHave9WKSWorksheet.Add(line);
					}
					else
					{
						var dutyRate = dutyCalculatorStrategy.CalculateAdValoremDutyRateForInvoiceLine(line);
						if (dutyRate.HasValue)
						{
							var newPrice = decimal.Round(line.JI_LinePrice / (1 + dutyRate.Value / 100), 2);
							var doc = line.SupportingDocuments.AddNew("9WKS", line.Declaration.JE_DeclarationReference);
							doc.CSI_Description = $"DDP adjustment from {line.JI_LinePrice} with rate {dutyRate.Value:0.###}% down to {newPrice}";
							line.JI_LinePrice = newPrice;
						}
						else
						{
							linesNotAttractingSimpleAdValoremDuty.Add(line);
						}
					}
				}
			}
		}

		public IReadOnlyList<JobComInvoiceLine> LinesMissingCommodityPreferenceOriginOrPrice => linesMissingCommodityPreferenceOriginOrPrice;
		public IReadOnlyList<JobComInvoiceLine> LinesNotAttractingSimpleAdValoremDuty => linesNotAttractingSimpleAdValoremDuty;
		public IReadOnlyList<JobComInvoiceLine> LinesAlreadyHave9WKSWorksheet => linesAlreadyHave9WKSWorksheet;

		public static DdpCalculator NoValidInvoicesFound => null;

		readonly List<JobComInvoiceLine> linesMissingCommodityPreferenceOriginOrPrice = new ();
		readonly List<JobComInvoiceLine> linesNotAttractingSimpleAdValoremDuty = new ();
		readonly List<JobComInvoiceLine> linesAlreadyHave9WKSWorksheet = new ();
	}
}
