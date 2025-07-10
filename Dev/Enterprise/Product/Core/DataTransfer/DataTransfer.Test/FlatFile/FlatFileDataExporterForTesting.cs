using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.DataTransfer.Business.Testing
{
	public class FlatFileDataExporterForTesting : FlatFileDataExporter
	{
		public FlatFileDataExporterForTesting(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public FlatFileDataExporterForTesting(BusinessObjectFactory factory, ExportInstructions instructions, ZString exportedFileToAppendTo)
			: base(factory, instructions, exportedFileToAppendTo)
		{
		}

		public IValueObject ConvertToIValueObjectForTesting(BusinessObject bizObj, IValueObjectDataAdapter adapter, int iteration, int count, INotifications notifications)
		{
			return base.ConvertToIValueObject(bizObj, adapter, iteration, count, notifications);
		}

		public IValueObjectDataAdapter DataAdapterForTesting
		{
			get { return DataAdapter; }
		}

		public override ZString EnglishDescription
		{
			get { return "Test Data"; }
		}

		public IFlatFileFormat FlatFileFormatForTesting
		{
			get { return FlatFileFormat; }
		}

		protected override IValueObjectDataAdapter DataAdapter
		{
			get { return new OrganisationValueObjectDataAdapter(); }
		}

		protected override IFlatFileFormat FlatFileFormat
		{
			get { return new CustomFlatFileFormatForTesting(); }
		}

		protected override IFlatFileConverter CreateConverter(INotifications notifications)
		{
			return new OrgFlatFileConverterForTesting(notifications, Factory);
		}

		public ExportMethod GetExportMethodForTesting(ExportInstructions instructions, INotifications notifications)
		{
			return base.GetExportMethod(instructions, notifications);
		}

		public new BusinessObjectFactory Factory
		{
			get { return base.Factory; }
		}

		public new void SetIsExportOK(bool value)
		{
			base.SetIsExportOK(value);
		}

		public new void ExportData(BusinessObjectReader objectReader, INotifications notifications)
		{
			base.ExportData(objectReader, notifications);
		}

		protected override bool IsValidToDeliver(INotifications notifications, IFlatFileConverter converter)
		{
			return ValidToDeliver;
		}
		public bool ValidToDeliver = true;
	}
}
