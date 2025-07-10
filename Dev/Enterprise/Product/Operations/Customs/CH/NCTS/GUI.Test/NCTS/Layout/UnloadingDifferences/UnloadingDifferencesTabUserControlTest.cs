using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NctsHeader = Enterprise.Customs.CH.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

class UnloadingDifferencesTabUserControlTest : TestCaseWithFactory
{
	public void TestShowWarningBoxWhenBM_NoChangesToReportValueChanged()
	{
		const string message = "Not all entries in 'Unloading Differences' and 'House consignment' tabs have the value DEC." +
				"\r\nPress CANCEL if you want to check the unloaded state of the containers, seals, house consignments, goods items and packages." +
				"\r\nIf you press OK, all entries with the state blanks, MIS or DIF will be set to DEC. The entries with state NEW will be removed.";

		var nctsHeader = Factory.New<NctsHeaderForTesting>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks;
		nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = false;
		nctsHeader.ExistNonDECEntryCoreForTesting = true;
		using (var form = new ZForm(nctsHeader))
		using (var userControl = new UnloadingDifferencesTabUserControl())
		{
			form.Controls.Add(userControl);
			userControl.SetDataBinding(nctsHeader, "");
			form.Show();
			nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = true;
			AssertEquals("When ExistNonDECEntry, warningBox appear", message, UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	public class NctsHeaderForTesting : NctsHeader
	{
		public NctsHeaderForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool ExistNonDECEntryCoreForTesting { get; set; }
		protected override bool ExistNonDECEntryCore => ExistNonDECEntryCoreForTesting;
	}
}
