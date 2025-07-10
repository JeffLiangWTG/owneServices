using CargoWise.EntityFramework;
using Enterprise.Accounting.DataTransfer.Invoices.FlatFile;

namespace Enterprise.Client.NZP.CMS
{
	public class CMSCreditNotesExporter : CMSDataExporter
	{
		public CMSCreditNotesExporter(int batchNumber, BusinessObjectFactory factory) : base(batchNumber, factory)
		{
		}

		#region Converter

		protected override AccountingFlatFileConverter Converter
		{
			get
			{
				if (fConverter == null)
				{
					fConverter = new CMSCreditNotesConverter(Notify, Factory);
				}
				return fConverter;
			}
		}
		CMSCreditNotesConverter fConverter;

		#endregion
	}
}
