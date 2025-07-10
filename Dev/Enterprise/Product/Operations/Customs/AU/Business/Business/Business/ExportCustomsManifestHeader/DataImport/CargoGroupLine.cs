using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CargoGroupLine : DataLine
	{
		public CargoGroupLine(ManifestImporter importer, ExportCustomsManifestHeader header, string line)
			: base(importer, header, line)
		{
		}

		internal void SetContainers(string numberOfContainersString)
		{
			if (header.CurrentManifestLine != null)
			{
				ZShort numberOfContainers;
				ZShort.TryParse(numberOfContainersString, out numberOfContainers);
				header.CurrentManifestLine.EL_NumberOfContainers += numberOfContainers;
			}
		}

		internal void SetPackages(string numberOfPackagesString)
		{
			if (header.CurrentManifestLine != null)
			{
				ZInt numberOfPackages;
				ZInt.TryParse(numberOfPackagesString, out numberOfPackages);
				header.CurrentManifestLine.EL_NumberOfPackages += numberOfPackages;
			}
		}

		protected override void DoProcessing()
		{
			SetCAN(SafeSubstring(line, 108, 15));
			SetContainers(SafeSubstring(line, 7, 10));
			SetPackages(SafeSubstring(line, 83, 10));
		}
	}
}
