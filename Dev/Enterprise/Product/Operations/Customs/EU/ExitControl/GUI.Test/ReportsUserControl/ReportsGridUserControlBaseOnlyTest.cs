using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.ExitControl.Business;
using static Enterprise.Customs.ExitControlBase.Business.AutoCusExitReport.Schema;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing;

sealed class ReportsGridUserControlBaseOnlyTest : ReportGridUserControlAbstractTest
{
	public override void TestAvailableColumns()
	{
		AssertSequencesEqual("Columns", new[] {
			CER_Type, CER_CXC_Consignment, CusExitReport.Schema.CER_Calc_Discrepancies, CER_TransportMode, CER_TransportType, CER_TransportID, CER_RN_NKTransportNationality,
			CER_Location, CER_DateTime, CER_OfficeOfExit, CER_Status, nameof(CusExitReport.StatusDescription),
			CER_MessageStatus, nameof(CusExitReport.MessageStatusDescription)
		},
		reportsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
	}

	public override void TestColumn_CER_Type()
	{
		CombineAssertions(() =>
		{
			var columnInfo = reportsGrid.GetColumnStyle(CER_Type);
			AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
			AssertEquals("Width", 83, columnInfo.Width);
		});
	}

	public override void TestColumn_CER_Location()
	{
		CombineAssertions(() =>
		{
			var columnInfo = reportsGrid.GetColumnStyle(CER_Location);
			AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
			AssertEquals("Width", 120, columnInfo.Width);
		});
	}

	public void TestColumn_CER_DateTime()
	{
		CombineAssertions(() =>
		{
			var columnInfo = reportsGrid.GetColumnStyle(CER_DateTime);
			AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
			AssertEquals("Width", 136, columnInfo.Width);
		});
	}
}
