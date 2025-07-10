using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineIntegration.RollUpSort;
using Enterprise.DocumentWrappers.Accounting.DocRollUpSort.InvoiceDocLineRollUpper;

namespace Enterprise.DocumentWrappers.Accounting.DocRollUpSort.RollUpper
{
	class RollUpGrouperByJob : BaseInvoiceDocRollUpper<ZGuid, ZString>
	{
		public RollUpGrouperByJob(BaseInvoiceDocLineRollUpper docLineRollUpper, DocARBaseInvoice docARInvoice, BusinessObjectFactory factory, DocARInvoiceLineCollection lines)
			: base(docLineRollUpper, docARInvoice, factory, lines)
		{
		}

		protected override ZGuid GetGroupId(IDocLine line)
		{
			var docARInvoiceLine = (DocARInvoiceLine)line;

			var lineJob = docARInvoiceLine?.Line.Job;
			return lineJob?.PK ?? ZGuid.Empty;
		}

		protected override ZString GetDescriptionForRolledUpLine(DocARInvoiceLineCollection group, ZGuid groupId)
		{
			var docARInvoiceLineCollection = group;
			return group.Count > 0
				? DocARBaseInvoice.GetLineDescription(DocARBaseInvoice.GetInvoiceModule(docARInvoiceLineCollection[0].JobHeader.JobHeader), ((DocumentWrapper)group[0]).WrappedObject as InvoicingLineBase)
				: ZString.Empty;
		}
	}
}
