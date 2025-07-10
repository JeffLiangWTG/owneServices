using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class InvoiceLineViewCollection : Customs.Business.InvoiceLineViewCollection<JobComInvoiceLine>
	{
		protected class LineComparer : PropertyComparer
		{
			public LineComparer(PropertyDescriptor propertyDescriptor, ListSortDirection direction)
				: base(propertyDescriptor, direction)
			{
			}

			#region IComparer Members

			public override int Compare(BusinessObject x, BusinessObject y)
			{
				JobComInvoiceLine lineX = (JobComInvoiceLine)x;
				JobComInvoiceLine lineY = (JobComInvoiceLine)y;
				int result = 0;
				if (lineX == null && lineY != null)
				{
					return -1;
				}
				else if (lineX != null && lineY == null)
				{
					return 1;
				}
				else if (lineX == null && lineY == null)
				{
					return 0;
				}

				if (x != y)
				{
					if (lineX == lineY.ParentLine || lineX == lineY.ChildLine)
					{
						return lineX.JI_LinePrefix.CompareTo(lineY.JI_LinePrefix);//'P' < 'T'
					}
					if (lineX.ParentLine != null && lineX.ParentLine.ParentLine == null)
					{
						return Compare(lineX.ParentLine, y);
					}
					else if (lineY.ParentLine != null && lineY.ParentLine.ParentLine == null)
					{
						return Compare(x, lineY.ParentLine);
					}

					if (PropertyDescriptor.DisplayName == JobComInvoiceLine.Schema.JI_Calc_Invoice
						|| PropertyDescriptor.DisplayName == JobComInvoiceLine.Schema.JI_LineNo)
					{
						if (lineX.InvoiceHeader != null && lineY.InvoiceHeader != null)
						{
							result = lineX.InvoiceHeader.JZ_InvoiceNumber.CompareTo(lineY.InvoiceHeader.JZ_InvoiceNumber);
						}
						if (result == 0)
						{
							result = lineX.JI_LineNo.CompareTo(lineY.JI_LineNo);
						}
						if (result == 0)
						{
							result = lineX.PK.CompareTo(lineY.PK);
						}
						result = Direction == ListSortDirection.Ascending ? result : -result;
					}
					else
					{
						result = base.Compare(x, y);
						if (result == 0)
						{
							result = lineX.JI_LinePrefix.CompareTo(lineY.JI_LinePrefix);
						}
						if (result == 0)
						{
							result = lineX.InvoiceHeader.JZ_InvoiceNumber.CompareTo(lineY.InvoiceHeader.JZ_InvoiceNumber);
						}
						if (result == 0)
						{
							result = lineX.JI_LineNo.CompareTo(lineY.JI_LineNo);
						}
					}
				}
				return result;
			}

			#endregion
		}

		protected override void CopyLastLineDetailsToNewLinesIfEnabled(JobComInvoiceLine newLine, JobComInvoiceLine previousLine)
		{
			base.CopyLastLineDetailsToNewLinesIfEnabled(newLine, previousLine);
			if (CopyLastLineDetailsToNewLines)
			{
				CopyRFPDetailsIfNeeded(newLine, previousLine);
			}
		}

		void CopyRFPDetailsIfNeeded(JobComInvoiceLine newLine, JobComInvoiceLine previousLine)
		{
			var declaration = Declaration;
			if (declaration?.IsQuarantine ?? ZBool.False)
			{
				previousLine.QuarantineExDocLine.Clone(newLine, GetJobDocAddressesPKPairs(declaration));

				if (declaration?.IsAQISCertificateRequest ?? ZBool.False)
				{
					previousLine.RFPNumbers.Clone(newLine);
				}
			}
		}

		static Dictionary<ZGuid, ZGuid> GetJobDocAddressesPKPairs(JobDeclaration declaration)
		{
			var addresses = (IDocAddresses)declaration?.Shipment ?? declaration;
			return addresses?.DocAddresses
				.FindDocAddressesByType(DocAddressType.AQISProcessingEstablishment)
				.Where(address => !address.IsEmpty)
				.ToDictionary(x => x.PK, x => x.PK);
		}

		protected override IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			return new LineComparer(property, direction);
		}

		public InvoiceLineViewCollection(JobDeclaration parentDeclaration)
			: base(parentDeclaration)
		{
		}

		new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}
	}
}
