using System;
using System.Windows.Forms;
using Enterprise.DocumentScanning.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IStorageDocsViewer : IDisposable
	{
		void Initialise(Control parentControl, IEDocsPlugIn plugIn);
		void View(IeDocBase doc, bool readOnly);
	}
}
