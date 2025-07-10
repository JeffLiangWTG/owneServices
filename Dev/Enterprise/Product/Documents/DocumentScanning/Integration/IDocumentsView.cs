using CargoWise.Integration;

namespace Enterprise.DocumentScanning.Integration
{
	public interface IDocumentsView
	{
		IBusinessObjectCollectionView DocumentCollectionView { get; }
		IBusinessObjectCollectionView PDFFilesCollectionView { get; }
	}
}
