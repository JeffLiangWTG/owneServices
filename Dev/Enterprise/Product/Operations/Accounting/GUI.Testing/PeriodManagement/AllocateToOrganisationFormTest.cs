using System.Windows.Forms;
using Enterprise.Accounting.Business.eNett;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.PeriodManagement.Testing
{
	[TestedType(typeof(AllocateToOrganisationForm))]
	public class AllocateToOrganisationFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			ComPayRegisteredOrganisation regOrg = new ComPayRegisteredOrganisation();
			regOrg.CusCode.OK_CustomsRegNo = "554455";
			Factory.Save();
			return new AllocateToOrganisationForm(regOrg);
		}
	}
}
