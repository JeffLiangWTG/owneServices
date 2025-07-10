using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(EXIT1MessageTypeForm))]
	sealed class EXIT1MessageTypeFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new EXIT1MessageTypeForm(new EXIT1MessageType());
	}
}
