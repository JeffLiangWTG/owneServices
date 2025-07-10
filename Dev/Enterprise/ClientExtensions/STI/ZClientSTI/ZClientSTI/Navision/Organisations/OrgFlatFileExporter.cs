
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;

namespace Enterprise.Client.STI.Navision
{
	public class OrgFlatFileExporter : NavisionFlatFileExporter
	{
		public OrgFlatFileExporter(BusinessObjectFactory factory, ExportInstructions instructions, ZString exportFile) : base(factory, instructions, exportFile)
		{
		}

		public override ZString EnglishDescription
		{
			get { return "Navision Organisations"; }
		}

		protected override IValueObjectDataAdapter DataAdapter
		{
			get
			{
				if (fDataAdapter == null)
				{
					fDataAdapter = new OrganisationValueObjectDataAdapter();
				}
				return fDataAdapter;
			}
		}
		IValueObjectDataAdapter fDataAdapter;

		protected override IFlatFileConverter CreateConverter(INotifications notifications)
		{
			return new OrgFlatFileConverter(notifications, Factory);
		}
	}
}
