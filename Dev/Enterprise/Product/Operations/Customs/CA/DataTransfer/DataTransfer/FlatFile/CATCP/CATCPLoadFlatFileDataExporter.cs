using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Customs.CA.DataTransfer
{
	public class CATCPLoadFlatFileDataExporter : FlatFileDataExporter
	{
		public CATCPLoadFlatFileDataExporter()
			: base(new BusinessObjectFactory())
		{
		}

		public CATCPLoadFlatFileDataExporter(BusinessObjectFactory factory, ExportInstructions instructions, ZString exportedFileToAppendTo)
			: base(factory, instructions, exportedFileToAppendTo)
		{
		}

		public override ZString EnglishDescription => "Export TCP Data";

		protected override Enterprise.DataTransfer.Integration.IValueObjectDataAdapter DataAdapter => new CATCPLoadFileDataAdapter();

		protected override IFlatFileFormat FlatFileFormat => new CATCPLoadFileFormat();

		protected override IFlatFileConverter CreateConverter(INotifications notifications) => new CATCPLoadFileConverter(notifications, new BusinessObjectFactory());
	}
}
