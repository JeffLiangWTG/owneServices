using System.Windows.Forms;
using Enterprise.Customs.GB.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.Testing
{
	[TestedType(typeof(CusPermitForm))]
	public class CusPermitFormBasherTest : EU.GUI.Testing.CusPermitFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new CusPermitForm(Factory.NewWithValidTestData<CusPermitHeader>());
		}
	}
}
