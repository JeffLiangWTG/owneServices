
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.STI.Navision
{
	public abstract class JobFlatFileExporter : NavisionFlatFileExporter
	{
		public JobFlatFileExporter(BusinessObjectFactory factory, ExportInstructions instructions, ZString exportFile, ZString shipmentNumber) : base(factory, instructions, exportFile)
		{
			this.ShipmentNumber = shipmentNumber;
		}

		public override ZString EnglishDescription
		{
			get { return "Navision Job Export"; }
		}

		protected override IFlatFileConverter CreateConverter(INotifications notifications)
		{
			return new JobFlatFileConverter(notifications, Factory, ShipmentNumber);
		}

		readonly ZString ShipmentNumber;
	}
}
