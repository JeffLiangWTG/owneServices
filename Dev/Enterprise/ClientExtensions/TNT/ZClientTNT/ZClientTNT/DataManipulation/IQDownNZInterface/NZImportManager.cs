
using CargoWise.EntityFramework;
using CargoWise.Types;
namespace Enterprise.Client.TNT.NZ
{
	public class NZImportManager : Exit2ImportManager
	{
		public NZImportManager(BusinessObjectFactory factory, string filePath)
			: base(factory, filePath)
		{
		}

		protected override QuantumMawbCollection QuantumMawbCollectionCore
		{
			get
			{
				if (fQuantumMawbCollectionCore == null)
				{
					fQuantumMawbCollectionCore = new NZQuantumMawbCollection(Factory, fExit2FileInfo.FullName);
					fQuantumMawbCollectionCore.LoadFromFile();
				}
				return fQuantumMawbCollectionCore;
			}
		}
		QuantumMawbCollection fQuantumMawbCollectionCore;

		protected override ZString QuantumFileProcessedDirectory
		{
			get { return TNTDataRegistry.Instance.QuantumFileProcessedDirectoryForManualImport; }
		}
	}
}
