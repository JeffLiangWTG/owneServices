using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class EDocsInstructions : IEDocsInstructions
	{
		public EDocsInstructions(bool saveCopyToEDocs, object parent)
		{
			SaveCopyToEDocs = saveCopyToEDocs;
			Parent = parent;
		}

		public bool SaveCopyToEDocs { get; }
		public object Parent { get; }
	}
}