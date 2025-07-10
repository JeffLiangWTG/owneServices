using System.Collections;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers.Customs.Base
{
	public abstract class DocBaseJobComInvoiceLineCollection : DocumentWrapperCollection
	{
		protected DocBaseJobComInvoiceLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected DocBaseJobComInvoiceLineCollection(InvoiceLineCompleteCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		protected DocBaseJobComInvoiceLineCollection(BaseJobComInvoiceLineViewCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocBaseJobComInvoiceLine this[int index]
		{
			get { return (DocBaseJobComInvoiceLine)base[index]; }
		}

		public void SortOnInvoiceHeaderThenProductCode()
		{
			this.Sort(new InvoiceLineSorter());
		}

		public void SortOnInvoiceLineNumber()
		{
			this.Sort(new InvoiceLineNumberSorter());
		}

		public virtual void SortOnMergedLineNumber()
		{
			this.Sort(new MergedLineNumberSorter());
		}

		public virtual void SortOnMergedNumericLineNumber()
		{
			this.Sort(new MergedNumericLineNumberSorter());
		}

		public void RemoveClassifiedLines()
		{
			foreach (DocBaseJobComInvoiceLine line in this.ToArray())
			{
				if (!line.Tariff.IsEmpty)
				{
					this.Remove(line);
				}
			}
		}
	}

	#region InvoiceLineNumberSorter

	internal class InvoiceLineNumberSorter : IComparer
	{
		#region IComparer Members

		public int Compare(object x, object y)
		{
			int result = 0;
			DocBaseJobComInvoiceLine line1 = x as DocBaseJobComInvoiceLine;
			DocBaseJobComInvoiceLine line2 = y as DocBaseJobComInvoiceLine;

			if (line1 != null && line2 != null)
			{
				if (line1.LineNo < line2.LineNo)
				{
					result = -1;
				}
				else if (line1.LineNo > line2.LineNo)
				{
					result = 1;
				}
			}
			return result;
		}

		#endregion
	}

	#endregion

	#region InvoiceLineSorter

	public class InvoiceLineSorter : IComparer
	{
		#region IComparer Members

		public int Compare(object x, object y)
		{
			int result = 0;
			DocBaseJobComInvoiceLine line1 = x as DocBaseJobComInvoiceLine;
			DocBaseJobComInvoiceLine line2 = y as DocBaseJobComInvoiceLine;

			if (line1 != null && line2 != null)
			{
				result = line1.Invoice.CompareTo(line2.Invoice);
				if (result == 0)
				{
					result = line1.PartNo.CompareTo(line2.PartNo);
				}
			}
			return result;
		}

		#endregion
	}

	#endregion

	#region MergedNumericLineNumberSorter

	public class MergedNumericLineNumberSorter : IComparer
	{
		#region IComparer Members

		public int Compare(object x, object y)
		{
			int result = 0;
			var line1 = x as DocBaseJobComInvoiceLine;
			var line2 = y as DocBaseJobComInvoiceLine;

			if (line1 != null & line2 != null)
			{
				result = line1.MergedNumericLineNo.CompareTo(line2.MergedNumericLineNo);
				if (result == 0)
				{
					result = line1.LineNo.CompareTo(line2.LineNo);
				}
			}
			return result;
		}

		#endregion
	}

	#endregion

	#region MergedLineNumberSorter

	internal class MergedLineNumberSorter : IComparer
	{
		#region IComparer Members

		public int Compare(object x, object y)
		{
			int result = 0;
			var line1 = x as DocBaseJobComInvoiceLine;
			var line2 = y as DocBaseJobComInvoiceLine;

			if (line1 != null && line2 != null)
			{
				int lineNo1, lineNo2;

				if (int.TryParse(line1.MergedLineNo, out lineNo1) && int.TryParse(line2.MergedLineNo, out lineNo2))
				{
					result = lineNo1.CompareTo(lineNo2);
				}
				else
				{
					result = line1.MergedLineNo.CompareTo(line2.MergedLineNo);
				}

				if (result == 0)
				{
					result = line1.LineNo.CompareTo(line2.LineNo);
				}
			}
			return result;
		}

		#endregion
	}

	#endregion
}
