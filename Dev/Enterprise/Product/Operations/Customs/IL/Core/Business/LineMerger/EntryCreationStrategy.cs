using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Business
{
	public class EntryCreationStrategy : Customs.Business.EntryCreationStrategy
	{
		public EntryCreationStrategy(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public override MergeKey GetKeyForLine(BaseJobComInvoiceLine invoiceLine)
		{
			var result = base.GetKeyForLine(invoiceLine);
			var line = (JobComInvoiceLine)invoiceLine;
			result.Add(line.JI_JZ);
			result.Add(GetPreviousDocumentsKey(line));
			return result;
		}

		static ZString GetPreviousDocumentsKey(JobComInvoiceLine line)
		{
			const char delimiter = (char)31;

			var orderedKeys = line.PreviousDocuments.Select(r =>
				r.CSI_Code + delimiter +
				r.CSI_ReferenceNumber + delimiter +
				r.CSI_ReferenceNumber2 + delimiter +
				r.CSI_UnitOfQuantity
				).Distinct().OrderBy(r => r);

			return string.Join(System.Environment.NewLine, orderedKeys);
		}
	}
}
