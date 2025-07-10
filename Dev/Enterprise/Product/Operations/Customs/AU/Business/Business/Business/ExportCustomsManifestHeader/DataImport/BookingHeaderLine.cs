using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class BookingHeaderLine : DataLine
	{
		public BookingHeaderLine(ManifestImporter importer, ExportCustomsManifestHeader header, string line)
			: base(importer, header, line)
		{
		}

		internal void SetCountryOfDestination(ZString country)
		{
			importer.CurrentCountryOfDestination = country;
		}

		protected override void DoProcessing()
		{
			importer.NewLineAdded = false;
			importer.CurrentCountryOfDestination = ZString.Empty;
			importer.CurrentOwnerName = ZString.Empty;

			SetCountryOfDestination(SafeSubstring(line, 198, 2));
			SetCAN(SafeSubstring(line, 379, 15));
		}
	}
}
