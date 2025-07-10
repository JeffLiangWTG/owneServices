using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class SummaryLineCollection : NonPersistentBusinessObjectCollection<SummaryLine>
	{
		public SummaryLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public void PopulateFromSummarySections(SummarySection[] summarySections)
		{
			if (summarySections != null)
			{
				foreach (SummarySection summarySection in summarySections)
				{
					PopulateFromSummarySection(summarySection);
				}
			}
		}

		public bool HasMultipleTaxes
		{
			get
			{
				bool hasMultipleTaxes = false;
				foreach (SummaryLine line in this)
				{
					if (line.TaxCode != this[0].TaxCode)
					{
						hasMultipleTaxes = true;
						break;
					}
				}

				return hasMultipleTaxes;
			}
		}

		public void ShowTaxCodes(bool show)
		{
			foreach (SummaryLine line in this)
			{
				if (!show)
				{
					line.TaxCode = ZString.Empty;
				}
				else
				{
					line.Header.TaxCode = "Tax Code";
				}
			}
		}

		public void PopulateFromSummarySection(SummarySection summarySection)
		{
			if (summarySection != null)
			{
				foreach (SummaryLine summaryLine in summarySection.Lines)
				{
					summaryLine.Header = summarySection.Header;
					Add(summaryLine);
				}
			}
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SummaryLine(Factory);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion
	}
}

