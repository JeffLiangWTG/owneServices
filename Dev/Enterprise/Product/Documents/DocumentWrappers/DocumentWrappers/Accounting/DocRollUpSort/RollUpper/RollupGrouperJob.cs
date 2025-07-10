using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration.RollUpSort;
using Enterprise.DocumentWrappers.Accounting.DocRollUpSort.InvoiceDocLineRollUpper;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.Accounting.DocRollUpSort.RollUpper
{
	class RollupGrouperJob : BaseInvoiceDocRollUpper<ZString, ZString>
	{
		public RollupGrouperJob(BaseInvoiceDocLineRollUpper docLineRollUpper, DocARBaseInvoice docARInvoice, BusinessObjectFactory factory, DocARInvoiceLineCollection lines)
			: base(docLineRollUpper, docARInvoice, factory, lines)
		{
		}

		protected override ZString GetGroupId(IDocLine line)
		{
			var docARInvoiceLine = (DocARInvoiceLine)line;

			var lineJobHeader = docARInvoiceLine?.Line.Job;
			return lineJobHeader?.JH_JobNum ?? ZString.Empty;
		}

		protected override ZString GetDescriptionForRolledUpLine(DocARInvoiceLineCollection group, ZString groupId)
			=> Res.GetString("4F0AE958-3752-453a-8EB9-BA796B29A538", "Job: {0}", groupId);
	}
}
