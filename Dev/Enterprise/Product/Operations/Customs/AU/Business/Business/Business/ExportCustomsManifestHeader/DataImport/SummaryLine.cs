using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SummaryLine : DataLine
	{
		public SummaryLine(ManifestImporter importer, ExportCustomsManifestHeader header, string line)
			: base(importer, header, line)
		{
		}

		protected override void DoProcessing()
		{
			ZString emptyContainersString = SafeSubstring(line, 3, 10);
			ZString fullContainersString = SafeSubstring(line, 13, 10);
			SetEmptyContainers(emptyContainersString);
			SetContainers(emptyContainersString, fullContainersString);
			SetPackages(SafeSubstring(line, 98, 10));
		}

		internal void SetEmptyContainers(ZString emptyContainersString)
		{
			ZShort numberOfEmpties;
			ZShort.TryParse(emptyContainersString, out numberOfEmpties);
			header.ED_NoOfEmptyContainers = numberOfEmpties;
		}

		internal void SetContainers(ZString emptyContainersString, ZString fullContainersString)
		{
			ZShort numberOfEmpties;
			ZShort.TryParse(emptyContainersString, out numberOfEmpties);

			ZShort numberOfContainers;
			ZShort.TryParse(fullContainersString, out numberOfContainers);

			header.ED_NoOfContainer = numberOfContainers;
		}

		internal void SetPackages(string packagesString)
		{
			header.ED_NoOfPacks = int.Parse(packagesString);
		}
	}
}
