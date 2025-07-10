using System.Drawing;
using CargoWise.IO;
using Enterprise.DocumentEngine.Imaging;

namespace Enterprise.DocumentEngine.PreviewableDocument
{
	public static class RotateDocument
	{
		public static string RotateActivePage(bool clockwise, IImagePageSelector imageSelector)
		{
			string rotatedImageFilename;
			var rotation = clockwise ? RotateFlipType.Rotate90FlipNone : RotateFlipType.Rotate90FlipXY;

			using (var dupPage = new DuplicatePage((Bitmap)imageSelector.CurrentImage))
			{
				RotatePage(dupPage.DuplicatedImage, rotation);
				using (var tempSelector = new StandardImagePageSelector(dupPage.DuplicatedImage))
				{
					rotatedImageFilename = CreateNewMultiPageImageAfterRotation(imageSelector, tempSelector);
				}
			}

			return rotatedImageFilename;
		}

		static void RotatePage(Image curPage, RotateFlipType reqRotateFlip)
		{
			curPage.RotateFlip(reqRotateFlip);
		}

		/// 

		/// <summary>
		/// Returns full pathname of newly created file.
		/// </summary>
		static string CreateNewMultiPageImageAfterRotation(IImagePageSelector originalPages, IImagePageSelector rotatedSingleImage)
		{
			var reqIndex = originalPages.CurrentPageIndex;
			var newFileName = Temp.GetTempFileNameWithExtension(Core.Constants.FileFormats.TIF);
			DocumentImagePageWriterUtils.CopyMultiPageImageToFileWithReplace(originalPages, newFileName, reqIndex, rotatedSingleImage);
			return newFileName;
		}
	}
}
