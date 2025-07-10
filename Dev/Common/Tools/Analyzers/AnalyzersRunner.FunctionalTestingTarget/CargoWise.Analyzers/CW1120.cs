//CW1120:Do Not Get Icons Images From Rex Or Resources File Analyzer
using System.Drawing;
using System.Resources;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1120
	{
		public void GetObject(ResourceManager rm)
		{
			_ = rm.GetObject("someIcon") as Icon;
			_ = rm.GetObject("someBitmap") as Bitmap;
		}
	}
}
