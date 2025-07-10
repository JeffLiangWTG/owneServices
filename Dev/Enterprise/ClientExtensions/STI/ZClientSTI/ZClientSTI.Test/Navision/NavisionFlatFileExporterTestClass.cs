using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;

namespace Enterprise.Client.STI.Navision.Testing
{
	public class NavisionFlatFileExporterTestClass : NavisionFlatFileExporter
	{
		public NavisionFlatFileExporterTestClass() : base(new BusinessObjectFactory(), null, "")
		{
		}

		public NavisionFlatFileExporterTestClass(BusinessObjectFactory factory, ExportInstructions instructions, string fileName) : base(factory, instructions, fileName)
		{
		}

		protected override IValueObjectDataAdapter DataAdapter
		{
			get
			{
				return new OrganisationValueObjectDataAdapter();
			}
		}

		protected override IFlatFileConverter CreateConverter(INotifications notifications)
		{
			return new OrgFlatFileConverter(notifications, Factory);
		}

		public new bool AppendToFile
		{
			get
			{
				return base.AppendToFile;
			}
		}

		public new IFlatFileFormat FlatFileFormat
		{
			get
			{
				return base.FlatFileFormat;
			}
		}
	}
}
