using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.OCR.Testing
{
	[TestedType(typeof(OCRResultForm))]
	sealed class OCRResultFormTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new OCRResultForm();
		}

		#endregion
	}
}
