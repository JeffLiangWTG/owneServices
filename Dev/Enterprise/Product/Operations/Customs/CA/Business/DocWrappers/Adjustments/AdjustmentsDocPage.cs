using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public abstract class AdjustmentsDocPage : NonPersistentBusinessObject, IObsoleteValidation
	{
		public AdjustmentsDocPage()
		{
		}

		protected abstract AdjustmentsDocPage CreateNewPage();

		protected abstract AdjustmentsDocLine CreateNewLine(bool isEmpty = false);

		public virtual IEnumerable<AdjustmentsDocPage> GetPages(JobComInvoiceHeader subHeader, JobDeclaration declaration = null)
		{
			var result = new List<AdjustmentsDocPage>();

			var pairs = PairUpAccountAndClaimLine(subHeader).ToList();
			var emptyLine = CreateNewLine(true);
			var emptyPair = new AccountAndClaimPair(emptyLine, emptyLine);

			var firstPair = pairs.Count > 0 ? pairs[0] : emptyPair;
			var secondPair = pairs.Count > 1 ? pairs[1] : emptyPair;
			result.Add(GetFirstPage(subHeader, firstPair, secondPair));
			int i = 2;
			while (i < pairs.Count)
			{
				firstPair = pairs[i];
				secondPair = pairs.Count > i + 1 ? pairs[i + 1] : emptyPair;
				result.Add(GetSubSequentPage(subHeader, firstPair, secondPair));
				i += 2;
			}
			return result;
		}

		protected AdjustmentsDocPage GetSubSequentPage(JobComInvoiceHeader subHeader, AccountAndClaimPair pair1, AccountAndClaimPair pair2)
		{
			var result = CreateNewPage();
			if (pair1 != null)
			{
				result.AsAccountForDocLine1 = pair1.AccountLine;
				result.AsClaimForDocLine1 = pair1.ClaimLine;
			}
			if (pair2 != null)
			{
				result.AsAccountForDocLine2 = pair2.AccountLine;
				result.AsClaimForDocLine2 = pair2.ClaimLine;
			}
			SetMoreForPage(result, subHeader);
			return result;
		}

		AdjustmentsDocPage GetFirstPage(JobComInvoiceHeader subHeader, AccountAndClaimPair pair1, AccountAndClaimPair pair2)
		{
			var result = CreateNewPage();
			result.TransactionNumber = subHeader.JobDeclaration != null ? subHeader.JobDeclaration.TransactionNumber.ToString() : ZString.Empty;
			result.SubHeaderNo = subHeader.JZ_InvoiceNumber.StartsWith("NS") ? (ZString)"NS" : subHeader.JZ_InvoiceNumber;
			result.CountryOfOrigin = subHeader.CountryOfOrigin;
			result.PlaceOfExport = subHeader.PlaceOfExport;
			result.TariffTreatment = subHeader.CA_TreatmentCode;
			var directShipmentDate = subHeader.JZ_ValuationDateOverride;
			if (directShipmentDate.IsValid)
			{
				result.DirectShipmentMonth = directShipmentDate.ToString("MM");
				result.DirectShipmentDay = directShipmentDate.ToString("dd");
				result.DirectShipmentYear = directShipmentDate.ToString("yyyy");
			}
			result.CurrencyCode = subHeader.JZ_RX_NKInvoice_Currency;
			result.TimeLimit = subHeader.CA_TimeLimit.IsEmpty ? string.Empty : subHeader.CA_TimeLimit.ToString();
			result.TimeCode = subHeader.CA_TimeLimit.IsEmpty ? ZString.Empty : subHeader.CA_TimeLimitCode;
			if (pair1 != null)
			{
				result.AsAccountForDocLine1 = pair1.AccountLine;
				result.AsClaimForDocLine1 = pair1.ClaimLine;
			}
			if (pair2 != null)
			{
				result.AsAccountForDocLine2 = pair2.AccountLine;
				result.AsClaimForDocLine2 = pair2.ClaimLine;
			}
			SetMoreForPage(result, subHeader);
			return result;
		}

		protected virtual void SetMoreForPage(AdjustmentsDocPage page, JobComInvoiceHeader subHeader)
		{
		}

		IEnumerable<AccountAndClaimPair> PairUpAccountAndClaimLine(JobComInvoiceHeader subHeader)
		{
			var asAccountedSubHeader = subHeader.CorrespondingAsAccountedForInvoice;

			var result = new List<AccountAndClaimPair>();
			var claimLinesDict = subHeader.AsClaimForFilteredInvoiceLines.Cast<JobComInvoiceLine>().GroupBy(g => g.JI_ParentID).ToDictionary(d => d.Key, d => d.ToList());
			var accountLines = asAccountedSubHeader?.AsAccountForFilteredInvoiceLines.Cast<JobComInvoiceLine>().Where(asAccountLine => claimLinesDict.Keys.Contains(asAccountLine.PK)).OrderBy(x => x, new CAInvoiceLineComparer(false, JobComInvoiceLine.Schema.CA_OriginalLineNo, true));

			var emptyDocLine = CreateNewLine(true);
			var claimLinesComparer = new CAInvoiceLineComparer(false, JobComInvoiceLine.Schema.CA_OriginalLineNo, true);
			if (accountLines != null)
			{
				var tempLine = CreateNewLine();
				foreach (var accountLine in accountLines)
				{
					if (claimLinesDict.TryGetValue(accountLine.PK, out var childClaimLines))
					{
						var docAccountLines = tempLine.GetLinesOrderedByDuty(accountLine, subHeader.JobDeclaration.IsB3X).ToList();
						var docClaimLines = GetLines(childClaimLines.OrderBy(o => o, claimLinesComparer)).ToList();
						var i = 0;
						while (i < docAccountLines.Count || i < docClaimLines.Count)
						{
							var docAccountLine = i < docAccountLines.Count ? docAccountLines[i] : emptyDocLine;
							var docClaimLine = i < docClaimLines.Count ? docClaimLines[i] : emptyDocLine;
							i++;
							result.Add(new AccountAndClaimPair(docAccountLine, docClaimLine));
						}
						claimLinesDict.Remove(accountLine.PK);
					}
				}
			}
			var orphanClaimLines = claimLinesDict.SelectMany(s => s.Value).OrderBy(x => x, claimLinesComparer);
			var orphanDocClaimLines = GetLines(orphanClaimLines);
			foreach (var orphanDocClaimLine in orphanDocClaimLines)
			{
				result.Add(new AccountAndClaimPair(emptyDocLine, orphanDocClaimLine));
			}
			return result;
		}

		IEnumerable<AdjustmentsDocLine> GetLines(IEnumerable<JobComInvoiceLine> invoiceLines)
		{
			var tempLine = CreateNewLine();
			foreach (var invoiceLine in invoiceLines)
			{
				foreach (var docLine in tempLine.GetLinesOrderedByDuty(invoiceLine))
				{
					yield return docLine;
				}
			}
		}

		public AdjustmentsDocHeader Header { get; set; }
		public AdjustmentsDocFooter Footer { get; set; }
		public AdjustmentsDocLine AsAccountForDocLine1 { get; set; }
		public AdjustmentsDocLine AsAccountForDocLine2 { get; set; }
		public AdjustmentsDocLine AsClaimForDocLine1 { get; set; }
		public AdjustmentsDocLine AsClaimForDocLine2 { get; set; }
		public ZString SubHeaderNo { get; set; }
		public ZString CountryOfOrigin { get; set; }
		public ZString PlaceOfExport { get; set; }
		public ZString TariffTreatment { get; set; }
		public ZString DirectShipmentMonth { get; set; }
		public ZString DirectShipmentDay { get; set; }
		public ZString DirectShipmentYear { get; set; }
		public ZString CurrencyCode { get; set; }
		public ZString TimeLimit { get; set; }
		public ZString TimeCode { get; set; }
		public ZString TransactionNumber { get; private set; }

		protected class AccountAndClaimPair
		{
			public AccountAndClaimPair(AdjustmentsDocLine accountLine, AdjustmentsDocLine claimLine)
			{
				this.AccountLine = accountLine;
				this.ClaimLine = claimLine;
			}
			public readonly AdjustmentsDocLine AccountLine;
			public readonly AdjustmentsDocLine ClaimLine;
		}
	}
}
