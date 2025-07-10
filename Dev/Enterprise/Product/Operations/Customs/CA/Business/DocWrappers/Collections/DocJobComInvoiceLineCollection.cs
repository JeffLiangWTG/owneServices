using System.Collections;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.CA.Business
{
	public class DocJobComInvoiceLineCollection : DocBaseJobComInvoiceLineCollection
	{
		public DocJobComInvoiceLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocJobComInvoiceLineCollection(Customs.Business.InvoiceLineCompleteCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public DocJobComInvoiceLineCollection(Customs.Business.BaseJobComInvoiceLineViewCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocJobComInvoiceLine this[int index]
		{
			get { return (DocJobComInvoiceLine)Elements[index]; }
		}

		public override void SortOnMergedLineNumber()
		{
			Sort(new MergedLineNumberSorter());
		}

		public override void SortOnMergedNumericLineNumber()
		{
			Sort(new MergedNumericLineNumberSorter());
		}

		#region MergedLineNumberSorter

		class MergedLineNumberSorter : IComparer
		{
			#region IComparer Members

			public int Compare(object x, object y)
			{
				var line1 = x as DocJobComInvoiceLine;
				var line2 = y as DocJobComInvoiceLine;

				var result = 0;
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
						result = line1.InvoicePageNo.CompareTo(line2.InvoicePageNo);
					}

					if (result == 0)
					{
						result = line1.InvoicePageRelativeLineNumber.CompareTo(line2.InvoicePageRelativeLineNumber);
					}
				}
				return result;
			}

			#endregion
		}

		#endregion

		#region MergedNumericLineNumberSorter

		class MergedNumericLineNumberSorter : IComparer
		{
			#region IComparer Members

			public int Compare(object x, object y)
			{
				var line1 = x as DocJobComInvoiceLine;
				var line2 = y as DocJobComInvoiceLine;

				var result = 0;
				if (line1 != null && line2 != null)
				{
					result = line1.MergedNumericLineNo.CompareTo(line2.MergedNumericLineNo);

					if (result == 0)
					{
						result = line1.InvoicePageNo.CompareTo(line2.InvoicePageNo);
					}

					if (result == 0)
					{
						result = line1.InvoicePageRelativeLineNumber.CompareTo(line2.InvoicePageRelativeLineNumber);
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
}
