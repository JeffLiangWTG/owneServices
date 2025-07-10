using System.Windows.Forms;
using Enterprise.DataConverters.CustomsFiles.NZ.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DataConverters.Testing.CustomsFiles.NZ.GUI
{
	[TestedType(typeof(ClassificationAndPartImporterForm))]
	sealed internal class ClassificationAndPartImporterFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ClassificationAndPartImporterForm();
		}
	}
}
