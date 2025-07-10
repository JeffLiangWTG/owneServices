using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

class TransportAndPackagingTabUserControlTest : TestCaseWithFactory
{
	public void TestTransportBorderGroupBoxVisibility() => CombineAssertions(() =>
	{
		var nctsHeader = Factory.New<Business.NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var movement = nctsHeader.MovementHeader;

		using (var form = new Phase5DepartureMovementForm(nctsHeader))
		using (var control = new TransportAndPackagingTabUserControl())
		{
			form.Controls.Add(control);
			form.Show();

			var transportBorderGroupBox = control.FindSingle<ZGroupBox>("TransportBorderGroupBox");

			AssertEquals($"BM_InBondEntryType='{movement.BM_InBondEntryType}' TransportBorderGroupBox.Visible", true, transportBorderGroupBox.Visible);

			movement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;
			AssertEquals($"BM_InBondEntryType='{movement.BM_InBondEntryType}' TransportBorderGroupBox.Visible", false, transportBorderGroupBox.Visible);
		}
	});
}
