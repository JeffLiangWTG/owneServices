
using CargoWise.EntityFramework;
using Enterprise.Accounting.DataTransfer.Invoices.FlatFile;

namespace Enterprise.Client.STI.Navision
{
	public class AccHeaderFlatFileExporter : NavisionAccFlatFileExporter
	{
		public AccHeaderFlatFileExporter(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override AccountingFlatFileConverter Converter
		{
			get
			{
				if (fConverter == null)
				{
					fConverter = new AccHeaderFlatFileConverter(Notify, Factory);
				}
				return fConverter;
			}
		}
		AccountingFlatFileConverter fConverter;
	}
}
