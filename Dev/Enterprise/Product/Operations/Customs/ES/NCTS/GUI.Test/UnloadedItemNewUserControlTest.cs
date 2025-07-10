using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using NctsHeader = Enterprise.Customs.ES.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	class UnloadedItemNewUserControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestBillOfLadingTextBoxVisibility()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			using (var form = new NctsMovementForm(header))
			using (var control = new UnloadedItemNewUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					var billOfLadingTextBox = (ZTextBox)form.Controls.Find("BillOfLadingTextBox", true).FirstOrDefault();
					AssertEquals("BillOfLadingTextBox is invisible initially.", false, billOfLadingTextBox.Visible);

					header.ESNctsHeader.CEN_PreviousSummaryDeclaration = "TEST";
					AssertEquals("BillOfLadingTextBox is visible when Previous Summary Declaration is filled.", true, billOfLadingTextBox.Visible);
				});
			}
		}
	}
}
