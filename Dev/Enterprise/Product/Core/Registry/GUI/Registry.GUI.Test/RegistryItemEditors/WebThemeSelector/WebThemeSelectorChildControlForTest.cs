using System.IO;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(WebThemeSelectorChildControl))]
	sealed class WebThemeSelectorChildControlForTest : WebThemeSelectorChildControl
	{
			public bool CheckImageData { get; set; }

			public int ImageDataLength { get; set; }

			protected override void RefreshPreviewPictureBox(Stream stream, string name)
			{
				if (!CheckImageData)
				{
					base.RefreshPreviewPictureBox(stream, name);

					return;
				}

				using (var memoryStream = new MemoryStream())
				{
					stream.CopyTo(memoryStream);
					ImageDataLength = memoryStream.ToArray().Length;
				}
			}

			public bool IsEmptyImage_Exposed(byte[] data)
			{
				return base.IsEmptyImage(data);
			}
	}
}
