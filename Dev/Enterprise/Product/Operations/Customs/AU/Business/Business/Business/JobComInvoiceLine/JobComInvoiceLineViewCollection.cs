using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class JobComInvoiceLineViewCollection : BaseJobComInvoiceLineViewCollection
	{
		public JobComInvoiceLineViewCollection(JobComInvoiceHeader parentJobComInvoiceHeader, Customs.Business.InvoiceLineCompleteCollection completeCollection)
			: base(parentJobComInvoiceHeader, completeCollection)
		{
		}

		public JobComInvoiceLineViewCollection(JobComInvoiceHeader parent, Customs.Business.InvoiceLineDependentCollection completeCollection)
			: base(parent, completeCollection)
		{
		}

		public new JobComInvoiceLine this[int index]
		{
			get { return (JobComInvoiceLine)Elements[index]; }
		}

		public virtual new JobComInvoiceLine AddNew()
		{
			return (JobComInvoiceLine)base.AddNew();
		}

		public new JobComInvoiceLine GetByLineNo(ZShort lineNo)
		{
			return (JobComInvoiceLine)base.GetByLineNo(lineNo);
		}

		public JobComInvoiceLine AddNewAndAdjustLinePriceWithInvoiceHeader()
		{
			JobComInvoiceLine newLine = null;
			if (InvoiceHeader != null)
			{
				newLine = AddNew();
				newLine.JI_LinePrice = newLine.JI_Calc_LinesTotal;
			}
			else
			{
				ErrorReporter.ReportOnce("An InvoiceLineCollection without InvoiceHeader is unable to adjust the line price of the new invoice line.", "An InvoiceLineCollection without InvoiceHeader is unable to adjust the line price of the new invoice line.");
			}
			return newLine;
		}

		public bool HasUnclassifiedLines()
		{
			foreach (JobComInvoiceLine line in this)
			{
				if (line.JI_Tariff.IsEmpty)
				{
					return true;
				}
			}

			return false;
		}

		public void ValidateInvoiceLinesClassifications()
		{
			foreach (JobComInvoiceLine line in this)
			{
				line.OldValidation.ValidateJI_CC();
			}
		}

		public bool HasNature20 => Factory.GetValue(ref cachedHasNature20, GetHasNature20);

		CachedProperty<bool> cachedHasNature20;

		bool GetHasNature20()
		{
			bool result = false;
			foreach (JobComInvoiceLine invoiceLine in this)
			{
				if (invoiceLine.JI_IsPackToBondForLine)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		protected override void OnCloning()
		{
			base.OnCloning();
			guidConversionTable = new Dictionary<string, string>();
		}

		protected override void OnClone(ZGuid linePK, ZGuid clonedLinePK)
		{
			base.OnClone(linePK, clonedLinePK);
			guidConversionTable.Add(linePK.ToString(), clonedLinePK.ToString());
		}

		Dictionary<string, string> guidConversionTable;

		protected override void OnClonedFinalised()
		{
			base.OnClonedFinalised();
			foreach (JobComInvoiceLine line in this)
			{
				string newGuidString;
				guidConversionTable.TryGetValue(line.AddInfo.ZA_RelatedLinePK_Hidden, out newGuidString);
				if (newGuidString != null)
				{
					line.AddInfo.ZA_RelatedLinePK_Hidden = newGuidString;
				}
			}
		}

		public void CreateContainersForLines()
		{
			foreach (JobComInvoiceLine line in this)
			{
				line.CreateContainerFromAddInfo();
			}
		}

		public void SynchroniseQuarantineLineProcesses()
		{
			foreach (JobComInvoiceLine line in this)
			{
				line.QuarantineExDocLine?.Processes.SynchroniseProcessAddresses();
			}
		}

		public void MarkQuarantineLineAsNeedingValidation()
		{
			foreach (JobComInvoiceLine line in this)
			{
				line.MarkAllQuarantineAsNeedingValidation();
			}
		}
	}
}
