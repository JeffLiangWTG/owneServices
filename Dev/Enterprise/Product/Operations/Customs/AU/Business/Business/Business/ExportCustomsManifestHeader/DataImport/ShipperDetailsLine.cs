using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ShipperDetailsLine : DataLine
	{
		public ShipperDetailsLine(ManifestImporter importer, ExportCustomsManifestHeader header, string line)
			: base(importer, header, line)
		{
		}

		protected override void DoProcessing()
		{
			SetOwnerName(SafeSubstring(line, 18, 35));
		}

		internal void SetOwnerName(ZString ownerName)
		{
			importer.CurrentOwnerName = ownerName.Trim(' ');
			if (importer.NewLineAdded)
			{
				header.CurrentManifestLine.EL_GoodsOwner = ownerName.Trim(' ');
			}
		}
	}
}
