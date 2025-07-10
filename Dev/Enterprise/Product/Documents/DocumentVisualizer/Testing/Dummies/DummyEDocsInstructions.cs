using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DummyEDocsInstructions : IEDocsInstructions
	{
		public bool SaveCopyToEDocs { get; set; }
		public object Parent { get; set; }
	}
}