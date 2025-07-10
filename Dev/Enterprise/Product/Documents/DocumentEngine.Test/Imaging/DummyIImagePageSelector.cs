using System;
using System.Drawing;
using System.IO;
using CargoWise.BuildTools;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Imaging.Testing
{
	public class DummyIImagePageSelector : IDisposable, IImagePageSelector
	{
		[Obsolete("Please use DummyIImagePageSelector(string baseSourcePath) instead.", false)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
		public DummyIImagePageSelector() : this(TestCase.BaseSourcePath)
		{ /* To be removed */ }

		public DummyIImagePageSelector(string baseSourcePath)
		{
			fImage = Image.FromFile(Path.Combine(BuildConstants.LocalEnterprisePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\lowres_5pages.tif"));
		}

		public IImagePageSelector PageSelector
		{
			get { return this; }
		}

		public int TotalPages
		{
			get { return fImage.GetFrameCount(System.Drawing.Imaging.FrameDimension.Page); }
		}

		public int CurrentPageIndex
		{
			get
			{
				return fCurrentPageIndex;
			}
			set
			{
				if (fImage == null)
				{
					fCurrentPageIndex = -1;
				}
				else
				{
					if ((value >= 0) && (value < TotalPages))
					{
						fImage.SelectActiveFrame(System.Drawing.Imaging.FrameDimension.Page, value);
						fCurrentPageIndex = value;
					}
				}
			}
		}
		int fCurrentPageIndex;

		public Image CurrentImage
		{
			get { return fImage; }
		}

		public void Dispose()
		{
			fImage.Dispose();
			fImage = null;
		}

		Image fImage;
	}
}
