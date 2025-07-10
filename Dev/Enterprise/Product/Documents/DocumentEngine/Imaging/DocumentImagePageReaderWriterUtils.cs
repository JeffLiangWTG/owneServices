using System.Drawing;

namespace Enterprise.DocumentEngine.Imaging
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
	public static class DocumentImagePageWriterUtils
	{
		/// <summary>
		/// Replaces the single ReplacementSingleImage at ReplacementIndex in the 
		/// SourceMultiPageImage. The result is written out to the OutputFilePath.
		/// </summary>
		public static void CopyMultiPageImageToFileWithReplace(IImagePageSelector sourceMultiPageImage,
			string outputFilePath, int replacementIndex, IImagePageSelector replacementSingleImage)
		{
			using (var tifImage = new WritableTifImage(outputFilePath))
			{
				var totalSourcePages = sourceMultiPageImage.TotalPages;
				if ((replacementIndex >= 0) && (replacementIndex < totalSourcePages))
				{
					var savedPageIndex = sourceMultiPageImage.CurrentPageIndex;
					try
					{
						for (int pageIndex = 0; pageIndex < totalSourcePages; pageIndex++)
						{
							Bitmap page;
							if (replacementIndex == pageIndex)
							{
								page = (Bitmap)replacementSingleImage.CurrentImage;
							}
							else
							{
								sourceMultiPageImage.CurrentPageIndex = pageIndex;
								page = (Bitmap)sourceMultiPageImage.CurrentImage;
							}
							tifImage.AddPage(page);
						}
					}
					finally
					{
						if (savedPageIndex >= 0)
						{
							sourceMultiPageImage.CurrentPageIndex = savedPageIndex;
						}
					}
				}
			}
		}

		/// <summary>
		/// Returns number of pages inserted. 
		/// All pages of SourceMultiPageImageFile will be inserted to the existing OutputFilePath, which must exist.
		/// The new pages will be inserted after InsertAfterIndex. 
		/// Normally, InsertAfterIndex >= 0 && InsertAfterIndex less than CurrentTotalPages. 
		/// If InsertAfterIndex == -1, then insert to the front.
		/// </summary>
		public static int InsertMultiPageImageToFile(string sourceMultiPageImageFile, string outputFilePath, int insertAfterIndex)
		{
			int pagesAppended = 0;

			using (var reader = new ImageFileReaderNoLockUsingTempFile(outputFilePath))
			{
				if (reader.PageSelector.TotalPages > 0 && insertAfterIndex >= -1 && insertAfterIndex < reader.PageSelector.TotalPages)
				{
					using (var tifImage = new WritableTifImage(outputFilePath))
					{
						// Add starting pages from original document.
						for (int ii = 0; ii <= insertAfterIndex; ii++)
						{
							reader.PageSelector.CurrentPageIndex = ii;
							tifImage.AddPage((Bitmap)reader.PageSelector.CurrentImage);
						}

						using (var sourceMultiPageImage = new ImageFileReaderWithLock(sourceMultiPageImageFile))
						{
							// Add new pages to original document.
							for (int ii = 0; ii < sourceMultiPageImage.PageSelector.TotalPages; ii++)
							{
								sourceMultiPageImage.PageSelector.CurrentPageIndex = ii;
								tifImage.AddPage((Bitmap)sourceMultiPageImage.PageSelector.CurrentImage);
								pagesAppended++;
							}
						}

						// Now add the rest of the original pages
						for (int ii = insertAfterIndex + 1; ii < reader.PageSelector.TotalPages; ii++)
						{
							reader.PageSelector.CurrentPageIndex = ii;
							tifImage.AddPage((Bitmap)reader.PageSelector.CurrentImage);
						}
					}
				}
			}
			return pagesAppended;
		}
	}
}
