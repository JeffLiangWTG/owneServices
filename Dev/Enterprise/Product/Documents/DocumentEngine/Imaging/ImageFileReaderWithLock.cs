using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Enterprise.DocumentEngine.Exceptions;

namespace Enterprise.DocumentEngine.Imaging
{
	/// <summary>
	/// Note: Must call Dispose, otherwise image file does not get released. 
	/// </summary>
	public class ImageFileReaderWithLock : BaseImageFileReader
	{
		public ImageFileReaderWithLock(string filename)
		{
			try
			{
				FileName = filename;

				if (!string.IsNullOrEmpty(FileName) && new FileInfo(FileName).Length > 0)
				{
					image = Image.FromFile(filename);
				}
			}
			catch (IOException ex)
			{
				Dispose();

				throw new UnreadableDocumentException(ex);
			}
			catch (Exception ex) when (ex is OutOfMemoryException || (ex is ExternalException && ex.Message.Contains("GDI+")))
			{
				Dispose();

				throw new CorruptedDocumentException(null, ex);
			}
		}

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				imageSelector?.Dispose();
				image?.Dispose();
			}
		}

		public override IImagePageSelector PageSelector => imageSelector ?? (imageSelector = new StandardImagePageSelector(image, FileName));

		public string FileName { get; set; }

		#region Implementation

		StandardImagePageSelector imageSelector;

		readonly Image image;

		#endregion
	}
}
