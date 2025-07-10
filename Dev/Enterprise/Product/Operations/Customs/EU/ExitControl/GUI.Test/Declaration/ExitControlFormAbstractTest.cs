using System.Windows.Forms;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	[TestsSubclassesOf(typeof(ExitControlForm))]
	public abstract class ExitControlFormAbstractTest<TParent> : ZFormBasherTest
		where TParent : CusExitHeader
	{
		protected override Form GetFormToBashCore()
		{
			var exitHeader = Factory.New<TParent>();
			exitHeader.CXH_JobReference = "123";
			Factory.Save();

			var form = new ExitControlForm(exitHeader);
			form.ControllerID = ControllerIDs.Customs.EU.ExitControl;
			return form;
		}
	}
}
