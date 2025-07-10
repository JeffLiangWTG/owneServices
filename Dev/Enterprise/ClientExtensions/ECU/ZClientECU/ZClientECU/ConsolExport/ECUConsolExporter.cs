using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Forwarding.DataTransfer;

namespace Enterprise.Client.ECU.ConsolExport
{
	public class ECUConsolExporter : FlatFileDataExporter
	{
		public ECUConsolExporter(BusinessObjectFactory factory)
			: base(factory)
		{
			FileID = ECUFileCounter.GetNewFileName();
		}

		protected override IFlatFileConverter CreateConverter(INotifications notifications)
		{
			return new ECUFlatFileConverter(notifications, Factory, FileID);
		}

		protected override IValueObjectDataAdapter DataAdapter
		{
			get { return new ForwardingConsolValueObjectDataAdapter(); }
		}

		public override ZString EnglishDescription
		{
			get { return "ECU Consol"; }
		}

		internal ZString FileID = "";

		protected override IFlatFileFormat FlatFileFormat
		{
			get { return new ECUFlatFileFormat(); }
		}

		protected override void PopulateExportInstructions(ExportInstructions instructions, IFlatFileConverter converter)
		{
			ECUFlatFileConverter dataConverter = converter as ECUFlatFileConverter;
			if (dataConverter != null)
			{
				if (!dataConverter.EmailAddress.IsEmpty)
				{
					instructions.MethodOfExport = ExportType.Email;
					instructions.EmailProperties.AddRecipient(dataConverter.EmailAddress);
					instructions.EmailProperties.Subject = "ECU Manifest Export";
					instructions.SpecifiedFilename = FileID.PadLeft(8, '0');
					instructions.UseUpperCaseFileExtension = true;
				}
			}
		}

		protected override bool IsValidToDeliver(INotifications notifications, IFlatFileConverter converter)
		{
			ECUFlatFileConverter dataConverter = converter as ECUFlatFileConverter;
			bool result = true;
			if (dataConverter != null)
			{
				bool canExport = !dataConverter.HasErrors;
				SetIsExportOK(canExport);
				result = canExport;
			}

			return result;
		}
	}
}
