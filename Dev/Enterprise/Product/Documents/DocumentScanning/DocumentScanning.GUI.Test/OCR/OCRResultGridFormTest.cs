using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.OCR.Testing
{
	[TestedType(typeof(OCRResultGridForm))]
	sealed class OCRResultGridFormTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new OCRResultGridForm();
		}

		#endregion
	}
}
