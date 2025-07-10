using NUnit.Framework;

namespace Enterprise.DocumentScanning.GUI
{
	sealed class DocumentsZGridInternalsTest : TestCase
	{
#if !WINZOR
		public void TestConstructor()
		{
			using (var grid = new DocumentsZGrid())
			{
				Assert("Cut event handler should be initialised", grid.Cut != null);
				Assert("Copy event handler should be initialised", grid.Copy != null);
				Assert("Paste event handler should be initialised", grid.Paste != null);
			}
		}
#endif
	}
}
