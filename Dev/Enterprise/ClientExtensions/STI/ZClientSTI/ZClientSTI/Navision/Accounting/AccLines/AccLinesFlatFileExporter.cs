using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.Invoices.FlatFile;

namespace Enterprise.Client.STI.Navision
{
	public class AccLinesFlatFileExporter : NavisionAccFlatFileExporter
	{
		public AccLinesFlatFileExporter(ZInt batchNumber, BusinessObjectFactory factory) : base(factory)
		{
			FilterProvider.CurrentBatchNo = batchNumber;
		}

		protected override AccountingFlatFileConverter Converter
		{
			get
			{
				if (fConverter == null)
				{
					fConverter = new AccLinesFlatFileConverter(Notify, Factory);
				}
				return fConverter;
			}
		}
		AccountingFlatFileConverter fConverter;
	}
}
