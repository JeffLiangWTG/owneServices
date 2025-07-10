using System.Windows.Forms;
using Enterprise.MailManager.Business;
using NUnit.Framework;

namespace Enterprise.MailManager.GUI
{
	[TestedType(typeof(MailItemCopySenderForm))]
	class MailItemCopySenderFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new MailItemCopySenderForm(new MailItemCopySender());
		}
	}
}
