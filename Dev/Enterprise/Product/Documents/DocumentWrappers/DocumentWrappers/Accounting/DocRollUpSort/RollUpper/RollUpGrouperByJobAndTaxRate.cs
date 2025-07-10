using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineIntegration.RollUpSort;
using Enterprise.DocumentWrappers.Accounting.DocRollUpSort.InvoiceDocLineRollUpper;

namespace Enterprise.DocumentWrappers.Accounting.DocRollUpSort.RollUpper
{
	class RollUpGrouperByJobAndTaxRate : BaseInvoiceDocRollUpper<ZString, ZString>
	{
		public RollUpGrouperByJobAndTaxRate(BaseInvoiceDocLineRollUpper docLineRollUpper, DocARBaseInvoice docARInvoice, BusinessObjectFactory factory, DocARInvoiceLineCollection lines)
			: base(docLineRollUpper, docARInvoice, factory, lines)
		{
			EliminateCommentLinesIfNecessary(lines);
		}

		void EliminateCommentLinesIfNecessary(DocARInvoiceLineCollection lines)
		{
			// for Rollup by Job and tax rate, we only want to show comment line for a a job if and only if the job has no other types of charge lines.
			var jobsWithNonCommentLines = new HashSet<ZGuid>();
			var jobsWithCommentLines = new HashSet<ZGuid>();

			foreach (DocARInvoiceLine line in lines)
			{
				if (line.IsCommentLine)
				{
					jobsWithCommentLines.Add(line.Line.AL_JH);
				}
				else
				{
					jobsWithNonCommentLines.Add(line.Line.AL_JH);
				}
			}

			var jobsToExcludeCommentLines = jobsWithNonCommentLines.Intersect(jobsWithCommentLines);
			var commentlinesToBeRemoved = new List<DocARInvoiceLine>();

			foreach (DocARInvoiceLine line in lines)
			{
				if (line.IsCommentLine && jobsToExcludeCommentLines.Contains(line.Line.AL_JH))
				{
					commentlinesToBeRemoved.Add(line);
				}
			}

			lines.RemoveRange(commentlinesToBeRemoved);
		}

		protected override ZString GetGroupId(IDocLine line)
		{
			var docARInvoiceLine = (DocARInvoiceLine)line;

			var lineJob = docARInvoiceLine?.Line.Job;
			var lineTaxRate = docARInvoiceLine?.Line.TaxRate;
			return (lineJob?.PK ?? ZGuid.Empty).ToString() +
				(lineTaxRate?.PK ?? ZGuid.Empty).ToString();
		}

		protected override ZString GetDescriptionForRolledUpLine(DocARInvoiceLineCollection group, ZString groupId)
		{
			var docARInvoiceLineCollection = group;

			ZString result;
			if (group.Count > 0)
			{
				if (docARInvoiceLineCollection[0].JobHeader == null)
				{
					result = docARInvoiceLineCollection[0].LineDescription;
				}
				else
				{
					result = DocARBaseInvoice.GetLineDescription(DocARBaseInvoice.GetInvoiceModule(docARInvoiceLineCollection[0].JobHeader.JobHeader), ((DocumentWrapper)group[0]).WrappedObject as InvoicingLineBase);
				}
			}
			else
			{
				result = ZString.Empty;
			}
			return result;
		}
	}
}
