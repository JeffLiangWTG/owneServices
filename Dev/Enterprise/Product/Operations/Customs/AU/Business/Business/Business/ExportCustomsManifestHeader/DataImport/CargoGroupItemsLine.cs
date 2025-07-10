namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CargoGroupItemsLine : DataLine
	{
		public CargoGroupItemsLine(ManifestImporter importer, ExportCustomsManifestHeader header, string line)
			: base(importer, header, line)
		{
		}

		#region Implementation

		protected override void DoProcessing()
		{
			SetDescription(SafeSubstring(line, 52, 30));
		}

		protected void SetDescription(string description)
		{
			if (header.CurrentManifestLine != null)
			{
				header.CurrentManifestLine.EL_GoodsDescription = description;
			}
		}

		#endregion
	}
}
