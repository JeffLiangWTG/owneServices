using System.Windows.Forms;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(CusCAeMHMasterForm))]
	sealed class CusCAeMHMasterFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var form = new CusCAeMHMasterForm(Factory.New<CusCAeMHMaster>());
			form.ControllerID = ControllerIDs.Customs.CA.CAHouseBilleManifest;
			return form;
		}
	}
}
