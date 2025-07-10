using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.GUI.Testing
{
	[TestedType(typeof(AdditionalContingencyDataForm))]
	sealed class AdditionalContingencyDataFormTest : ZFormBasherTest
	{
		public void TestVisibility1()
		{
			var addData = new AdditionalContingencyData(Factory);
			using (var form = new AdditionalContingencyDataForm(addData))
			{
				form.ControlVisibility();
				form.Show();
				Assert(form.originPremiseID.Visible);
				Assert(form.originPremiseIDExpain.Visible);
				Assert(form.originPremiseIDLabel.Visible);
			}
		}

		public void TestVisibility2()
		{
			var addData = new AdditionalContingencyData(Factory);
			addData.IsOriginPremiseReadOnly = true;
			using (var form = new AdditionalContingencyDataForm(addData))
			{
				form.ControlVisibility();
				form.Show();
				Assert(!form.originPremiseID.Visible);
				Assert(!form.originPremiseIDExpain.Visible);
				Assert(!form.originPremiseIDLabel.Visible);
			}
		}

		protected override Form GetFormToBashCore() => new AdditionalContingencyDataForm(new AdditionalContingencyData(Factory));
	}
}
