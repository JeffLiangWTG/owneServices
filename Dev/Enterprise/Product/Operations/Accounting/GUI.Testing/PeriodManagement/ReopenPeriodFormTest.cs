using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.PeriodManagement.Testing
{
	[TestedType(typeof(ReopenPeriodForm))]
	public class ReopenPeriodFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ReopenPeriodForm(Factory.New<AccPeriodManagement>());
		}
	}
}
