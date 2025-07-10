using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Testing.Dummies
{
	sealed class DummyPage : IPage
	{
		public double Width { get; set; }
		public double Height { get; set; }
		public IDocument Document { get; set; }
		public IRange Range { get; set; }
		public IMacroScope Scope { get; set; }
		public string Name { get; set; }

		public void Dispose()
		{
		}
	}
}