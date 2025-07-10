using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.CA.Services;

namespace Enterprise.Customs.CA.Business
{
	public class AIRSValidationQueriedLineCollection : List<IAIRSValidationQueriedLine>
	{
		public void AddNewOrUpdateExistingOGD(JobComInvoiceLine invoiceLine)
		{
			var line = new AIRSOGDValidationQueriedLine(invoiceLine);
			var existingLine = GetExistingAIRSOGDValidationQueriedLine(line);

			if (existingLine == null)
			{
				line.Commodity = GenerateNewCommodity();
				this.Add(line);
			}
			else
			{
				line = existingLine;
			}

			line.InvoiceLinePKs.Add(invoiceLine.PK);
		}

		public void AddNewOrUpdateExistingIID(JobComInvoiceLine invoiceLine)
		{
			var line = new AIRSIIDValidationQueriedLine(invoiceLine);
			var existingLine = GetExistingAIRSIIDValidationQueriedLine(line);

			if (existingLine == null)
			{
				line.Commodity = GenerateNewCommodity();
				this.Add(line);
			}
			else
			{
				line = existingLine;
			}

			line.InvoiceLinePKs.Add(invoiceLine.PK);
		}

		ZInt GenerateNewCommodity()
		{
			return !this.Any() ? 1 : (this.Cast<AIRSValidationQueriedLine>().Max(l => l.Commodity) + 1);
		}

		AIRSOGDValidationQueriedLine GetExistingAIRSOGDValidationQueriedLine(AIRSOGDValidationQueriedLine line)
		{
			return this.Cast<AIRSOGDValidationQueriedLine>().FirstOrDefault(l => l.Equals(line));
		}

		AIRSIIDValidationQueriedLine GetExistingAIRSIIDValidationQueriedLine(AIRSIIDValidationQueriedLine line)
		{
			return this.Cast<AIRSIIDValidationQueriedLine>().FirstOrDefault(l => l.Equals(line));
		}
	}
}
