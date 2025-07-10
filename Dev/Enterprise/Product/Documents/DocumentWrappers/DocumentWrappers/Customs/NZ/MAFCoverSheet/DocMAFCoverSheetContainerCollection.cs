using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers.Customs.NZ.MAFCoverSheet
{
	public class DocMAFCoverSheetContainerCollection : DocBaseWrapperCollection<DocMAFCoverSheetContainer>
	{
		public DocMAFCoverSheetContainerCollection(NZDocsMAFCSContainerCollection containers, ZInt firstContainer, ZInt numberOfRows)
			: base(containers.Factory)
		{
			for (int i = firstContainer; i <= (firstContainer + numberOfRows - 1); i++)
			{
				NZDocsMAFCSContainer container;
				if (containers.Count > i && containers[i] != null)
				{
					container = containers[i];
				}
				else
				{
					container = null;
				}
				DocMAFCoverSheetContainer containerWrapper = new DocMAFCoverSheetContainer(container, Factory, i + 1);
				containerWrapper.ContainerReference = i + 1;
				Add(containerWrapper);
			}
		}

		protected override DocumentWrapper WrapObject(object objectToWrap)
		{
			return new DocMAFCoverSheetContainer(null, Factory, 0);
		}
	}
}
