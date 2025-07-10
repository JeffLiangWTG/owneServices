using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Forwarding.DataTransfer;

namespace Enterprise.Client.MFI.CaroTrans.Export
{
	public class CaroTransFlatFileDataExporter : FlatFileDataExporter
	{
		public CaroTransFlatFileDataExporter(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override IValueObjectDataAdapter DataAdapter
		{
			get { return new ForwardingConsolValueObjectDataAdapter(); }
		}

		protected override IFlatFileFormat FlatFileFormat
		{
			get { return new CaroTransFlatFileFormat(); }
		}

		protected override IFlatFileConverter CreateConverter(INotifications notificationSubscriber)
		{
			return new CaroTransFileConverter(notificationSubscriber, new BusinessObjectFactory());
		}

		protected override ExportInstructions GetExportInstructions()
		{
			Instructions = new CaroTransExportInstructions();
			Instructions.FileExtension = FileExtensionType;
			PopulateExportInstructions(Instructions, Converter);

			return Instructions;
		}

		protected override void PopulateExportInstructions(ExportInstructions instructions, IFlatFileConverter converter)
		{
			SetInstructions(instructions, converter);
		}

		protected virtual void SetInstructions(ExportInstructions instructions, IFlatFileConverter converter)
		{
			CaroTransFileConverter fileConverter = (CaroTransFileConverter)converter;
			instructions.MethodOfExport = ExportType.Email;
			instructions.SpecifiedFilename = Constants.FileNamePrefix + fileConverter.GetNameOfFileFromConsolDetails();
		}

		public override ZString EnglishDescription
		{
			get { return "CaroTrans"; }
		}
	}
}
