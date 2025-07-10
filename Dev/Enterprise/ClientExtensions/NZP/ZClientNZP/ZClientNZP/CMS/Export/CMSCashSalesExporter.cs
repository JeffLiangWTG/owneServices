using CargoWise.EntityFramework;
using Enterprise.Accounting.DataTransfer.Invoices.FlatFile;

namespace Enterprise.Client.NZP.CMS
{
	public class CMSCashSalesExporter : CMSDataExporter
	{
		public CMSCashSalesExporter(int batchNumber, BusinessObjectFactory factory) : base(batchNumber, factory)
		{
		}

		#region Converter

		protected override AccountingFlatFileConverter Converter
		{
			get
			{
				if (fConverter == null)
				{
					fConverter = new CMSCashSalesConverter(Notify, Factory);
				}
				return fConverter;
			}
		}
		CMSCashSalesConverter fConverter;

		#endregion
	}
}
