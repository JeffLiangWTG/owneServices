using System.Drawing;
using Enterprise.DocumentEngine.Imaging;

namespace Enterprise.DocumentScanning.Business
{
	public class StandardImagePageSelectorForTesting : StandardImagePageSelector
	{
		public void SetImageForTesting(Image image)
		{
			base.SetImage(image);
		}
	}
}
