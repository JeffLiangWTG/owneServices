using CargoWise.EntityFramework;
using Enterprise.Accounting.DataTransfer.Invoices.FlatFile;

namespace Enterprise.Client.NZP.CMS
{
	public class CMSCreditSalesExporter : CMSDataExporter
	{
		public CMSCreditSalesExporter(int batchNumber, BusinessObjectFactory factory) : base(batchNumber, factory)
		{
		}

		#region Converter

		protected override AccountingFlatFileConverter Converter
		{
			get
			{
				if (fConverter == null)
				{
					fConverter = new CMSCreditSalesConverter(Notify, Factory);
				}
				return fConverter;
			}
		}
		CMSCreditSalesConverter fConverter;

		#endregion
	}
}
