using System.IO;
using CargoWise.IO;

namespace Enterprise.DocumentEngine.Imaging
{
	/// <summary>
	/// ImageFileReaderNoLockUsingTempFile - make a temporary copy of an image file, and read the copy.
	/// This prevents locking of the original file.
	/// you MUST call dispose() on this object, otherwise the temporary file image does not get released.
	/// </summary>
	public class ImageFileReaderNoLockUsingTempFile : BaseImageFileReader
	{
		public ImageFileReaderNoLockUsingTempFile(string aImageFilePath)
		{
			fTempFileName = Temp.GetTempFileNameWithExtension(Core.Constants.FileFormats.TIF);
			File.Copy(aImageFilePath, fTempFileName, true);
			// Ensure file is not Read-Only. Otherwise, you won't be able to delete it later.
			File.SetAttributes(fTempFileName, FileAttributes.Normal);

			fReader = new ImageFileReaderWithLock(fTempFileName);
		}

		public override IImagePageSelector PageSelector
		{
			get { return fReader.PageSelector; }
		}

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				if (fReader != null)
				{
					fReader.Dispose();
					fReader = null;
				}

				if (!string.IsNullOrEmpty(fTempFileName))
				{
					if (File.Exists(fTempFileName))
					{
						File.Delete(fTempFileName);
					}
					fTempFileName = "";
				}
			}
		}

		#region Implementation

		ImageFileReaderWithLock fReader;
		string fTempFileName = "";

		#endregion
	}
}
