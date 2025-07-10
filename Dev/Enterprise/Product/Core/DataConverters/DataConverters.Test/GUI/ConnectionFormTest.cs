using System.Windows.Forms;
using Enterprise.DataConverters.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DataConverters.Testing.GUI
{
	[TestedType(typeof(ConnectionForm))]
	sealed internal class ConnectionFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var importer = new DummyImporter(Factory);
			return new ConnectionForm(importer);
		}
	}
}
