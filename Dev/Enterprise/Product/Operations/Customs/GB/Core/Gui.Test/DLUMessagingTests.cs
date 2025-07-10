using System.Windows.Forms;
using Enterprise.Customs.GB.Chief.Messaging.DLU;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.DLU.Testing
{
	[TestedType(typeof(DLUMessageForm))]
	class DLUMessageFormBasher : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new DLUMessageForm(Factory.New<DLUMessage>());
		}
	}
}
