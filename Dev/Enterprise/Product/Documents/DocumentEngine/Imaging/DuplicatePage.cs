using System;
using System.Drawing;
using System.IO;
using CargoWise.Common;
using CargoWise.IO;

namespace Enterprise.DocumentEngine.Imaging
{
	/// <summary>
	/// DuplicatePage - class to create a standalone bitmap, which contains
	/// a duplicate of the passed bitmap page. If the source bitmap is multi-page, then
	/// only the current active page is duplicated.
	/// 
	/// Advantage is that you don't need to be able to get the active page to manipulate
	/// (Since the native classes don't support being able to get the active page)
	/// 
	/// You must call Dispose to release resources after you are finished with the duplicate.
	/// </summary>
	public sealed class DuplicatePage : Disposable
	{
		public DuplicatePage(Bitmap sourceImage)
		{
			if (sourceImage == null)
			{
				throw new ArgumentNullException(nameof(sourceImage));
			}
			fTempFileName = Temp.GetTempFileNameWithExtension(Core.Constants.FileFormats.TIF);
			sourceImage.Save(fTempFileName); // only active page saved
			fDuplicatedImage = (Bitmap)Image.FromFile(fTempFileName);
		}

		public Bitmap DuplicatedImage
		{
			get { return fDuplicatedImage; }
		}

		#region Implementation

		Bitmap fDuplicatedImage;
		readonly string fTempFileName;

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				if (fDuplicatedImage != null)
				{
					fDuplicatedImage.Dispose();
					fDuplicatedImage = null;

					if (File.Exists(fTempFileName))
					{
						File.Delete(fTempFileName);
					}
				}
			}
		}

		#endregion
	}
}
