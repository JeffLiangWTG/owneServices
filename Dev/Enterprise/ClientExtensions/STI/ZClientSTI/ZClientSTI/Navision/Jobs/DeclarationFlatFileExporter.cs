
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;

namespace Enterprise.Client.STI.Navision
{
	public class DeclarationFlatFileExporter : JobFlatFileExporter
	{
		public DeclarationFlatFileExporter(BusinessObjectFactory factory, ExportInstructions instructions, ZString exportFile, ZString shipmentNumber) : base(factory, instructions, exportFile, shipmentNumber)
		{
		}

		protected override IValueObjectDataAdapter DataAdapter
		{
			get
			{
				if (fDataAdapter == null)
				{
					fDataAdapter = DeclarationValueObjectDataAdapter.New();
				}
				return fDataAdapter;
			}
		}
		IValueObjectDataAdapter fDataAdapter;
	}
}
