using System.Windows.Forms;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	[TestedType(typeof(ExampleTemplateForm))]
	sealed class ExampleTemplateFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			Factory.Save();
			var result = new ExampleTemplateForm(header);
			return result;
		}
	}
}
