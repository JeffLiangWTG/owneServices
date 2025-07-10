using System;
using System.IO;
using Enterprise.DocumentVisualizer.Core;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class WebDrawingTest : TestCase
	{
		#region TestGetImage

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetImage()
		{
			var url = GetImageFilePath();

			var drawing = new WebDrawing(url);

			AssertNotNull("image", drawing.ImageData);
		}

		#endregion

		#region Implementation

		Uri GetImageFilePath()
		{
			var imageFilePath = Path.Combine(TestCase.BaseSourcePath,
				@"Enterprise\Product\Documents\DocumentVisualizer\GUI\Resources\tools.png");

			Assert("prerequisite: test image exists", File.Exists(imageFilePath));

			return new Uri(imageFilePath);
		}

		#endregion
	}
}
