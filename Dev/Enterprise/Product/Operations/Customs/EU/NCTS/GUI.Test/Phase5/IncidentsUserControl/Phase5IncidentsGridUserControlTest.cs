using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using static Enterprise.Customs.Business.AutoCusInBondEvent.Schema;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class Phase5IncidentsGridUserControlTest : TestCaseWithFactory
	{
		public void TestAvailableColumns()
		{
			AssertSequencesEqual("Columns", new[] { BN_IncidentCode, BN_Information, BN_CustomsStatus, BN_EndorsementDate, BN_EndorsementAuthority, BN_EndorsementPlace, BN_EndorsementCountryCode, BN_TransportAtDepartureType, BN_TransportAtDepartureID, BN_RN_NKTransportAtDepartureIDNationality },
					incidentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new Phase5IncidentsGridUserControl();
			incidentsGrid = userControl.IncidentsGrid;
		}
		Phase5IncidentsGridUserControl userControl;
		ZGrid incidentsGrid;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
