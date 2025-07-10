
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Forwarding.DataTransfer;

namespace Enterprise.Client.STI.Navision
{
	public class ConsolFlatFileExporter : JobFlatFileExporter
	{
		public ConsolFlatFileExporter(BusinessObjectFactory factory, ExportInstructions instructions, ZString exportFile, ZString shipmentNumber) : base(factory, instructions, exportFile, shipmentNumber)
		{
		}

		protected override IValueObjectDataAdapter DataAdapter
		{
			get
			{
				if (fDataAdapter == null)
				{
					fDataAdapter = new ForwardingConsolValueObjectDataAdapter();
				}
				return fDataAdapter;
			}
		}
		IValueObjectDataAdapter fDataAdapter;
	}
}
