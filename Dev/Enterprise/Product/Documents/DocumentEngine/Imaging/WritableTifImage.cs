using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using CargoWise.Common;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.Imaging
{
	public class WritableTifImage : Disposable
	{
		public WritableTifImage(string outputFilename)
		{
			this.outputFilename = outputFilename;
		}

		public void AddPage(Image pageToAdd)
		{
			try
			{
				if (image == null)
				{
					tempFile = TempFile.NewWithExtension("tif");
					pageToAdd.Save(tempFile.Filename);              // saves only the active page in a multipage doc
					image = Image.FromFile(tempFile.Filename);
					image.Save(outputFilename, FileSaveHelper.GetTiffEncoder(), GetEncoderParameters(EncoderValue.MultiFrame));
				}
				else
				{
					image.SaveAdd(pageToAdd, GetEncoderParameters(EncoderValue.FrameDimensionPage));
				}
			}
			catch (ExternalException)
			{
				Dispose();
				throw;
			}
		}

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				try
				{
					if (image != null)
					{
						image.SaveAdd(GetEncoderParameters(EncoderValue.Flush));
						image.Dispose();
					}
				}
				finally
				{
					if (tempFile != null)
					{
						tempFile.Dispose();
					}
				}
			}
		}

		EncoderParameters GetEncoderParameters(EncoderValue value)
		{
			var @params = new EncoderParameters();
			@params.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)value);
			return @params;
		}

		Image image;
		TempFile tempFile;
		readonly string outputFilename;
	}
}
