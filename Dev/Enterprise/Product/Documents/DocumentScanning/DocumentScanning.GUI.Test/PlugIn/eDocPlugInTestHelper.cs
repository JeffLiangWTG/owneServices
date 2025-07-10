using System.Windows.Forms;
using CargoWise.Interop.DataObjects;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.PlugIn.PlugInTesting
{
	sealed class MockDataObject : ZDataObject
	{
		public MockDataObject(DataObject data) : base(data)
		{
		}
	}

	sealed class MockEmbeddedRtfImageSource : ZDataObject, IEmbeddedRtfImageSource
	{
		public MockEmbeddedRtfImageSource(EmbeddedRtfImageFileInfo[] imageFiles)
		{
			this.ImageFiles = imageFiles;
		}

		public EmbeddedRtfImageFileInfo[] ImageFiles { get; private set; }

		public bool Valid
		{
			get { return true; }
		}

		public void FinaliseRtf()
		{
		}
	}

	[TestClass]
	sealed class ZDummyTemplateForm : ZTemplateForm
	{
		public ZDummyTemplateForm()
		{
		}
	}
}
