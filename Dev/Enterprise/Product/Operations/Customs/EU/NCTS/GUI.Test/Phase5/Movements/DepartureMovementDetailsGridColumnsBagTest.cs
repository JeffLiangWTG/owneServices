using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class DepartureMovementDetailsGridColumnsBagTest : TestCase
	{
		public void TestTransportAtDepartureDropEdit()
		{
			AssertNotNull(ColumnsBag.TransportAtDepartureDropEdit);
			var columnInfo = ColumnsBag.TransportAtDepartureDropEdit.CreateGridColumnInfo() as ZTextBoxColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "BM_TransportAtDeparture", columnInfo.ColumnName);
			AssertEquals("Width", 100, columnInfo.Width);
		}

		public void TestInlandTransportModeDropEdit()
		{
			AssertNotNull(ColumnsBag.InlandTransportModeDropEdit);
			var columnInfo = ColumnsBag.InlandTransportModeDropEdit.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "BM_InlandTransportMode", columnInfo.ColumnName);
			AssertEquals("Width", 100, columnInfo.Width);
		}

		public void TestFromWarehouseAddressDropEdit()
		{
			AssertNotNull(ColumnsBag.FromWarehouseAddressDropEdit);
			var columnInfo = ColumnsBag.FromWarehouseAddressDropEdit.CreateGridColumnInfo() as ZAddressDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "BM_OA_WarehouseAddress", columnInfo.ColumnName);
			AssertEquals("Width", 125, columnInfo.Width);
		}

		public void TestFromWarehousOrganisationFindBox()
		{
			AssertNotNull(ColumnsBag.FromWarehouseOrganisationFindBox);
			var columnInfo = ColumnsBag.FromWarehouseOrganisationFindBox.CreateGridColumnInfo() as ZOrganisationFindBoxColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "FromWarehouseOrgPK", columnInfo.ColumnName);
			AssertEquals("Width", 100, columnInfo.Width);
		}

		public void TestDepartureStatusDropEdit()
		{
			AssertNotNull(ColumnsBag.DepartureStatusDropEdit);
			var columnInfo = ColumnsBag.DepartureStatusDropEdit.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "BM_CustomsStatus", columnInfo.ColumnName);
			AssertEquals("Width", 100, columnInfo.Width);
		}

		public void TestMessageStatusDropEdit()
		{
			AssertNotNull(ColumnsBag.MessageStatusDropEdit);
			var columnInfo = ColumnsBag.MessageStatusDropEdit.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "BM_MessageStatus", columnInfo.ColumnName);
			AssertEquals("Width", 100, columnInfo.Width);
		}

		public void TestPhaseStatusDropEdit()
		{
			AssertNotNull(ColumnsBag.PhaseStatusDropEdit);
			var columnInfo = ColumnsBag.PhaseStatusDropEdit.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "BM_Phase", columnInfo.ColumnName);
			AssertEquals("Width", 100, columnInfo.Width);
		}

		public void TestDepartureStatusDescriptionTextEdit()
		{
			AssertNotNull(ColumnsBag.DepartureStatusDescriptionTextEdit);
			var columnInfo = ColumnsBag.DepartureStatusDescriptionTextEdit.CreateGridColumnInfo() as ZTextBoxColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "CustomsStatusDescription", columnInfo.ColumnName);
			AssertEquals("Width", 175, columnInfo.Width);
		}

		public void TestMessageStatusDescriptionTextEdit()
		{
			AssertNotNull(ColumnsBag.MessageStatusDescriptionTextEdit);
			var columnInfo = ColumnsBag.MessageStatusDescriptionTextEdit.CreateGridColumnInfo() as ZTextBoxColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "MessageStatusDescription", columnInfo.ColumnName);
			AssertEquals("Width", 225, columnInfo.Width);
		}

		public void TestPhaseStatusDescriptionTextEdit()
		{
			AssertNotNull(ColumnsBag.PhaseStatusDescriptionTextEdit);
			var columnInfo = ColumnsBag.PhaseStatusDescriptionTextEdit.CreateGridColumnInfo() as ZTextBoxColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "PhaseStatusDescription", columnInfo.ColumnName);
			AssertEquals("Width", 150, columnInfo.Width);
		}

		DepartureMovementDetailsGridColumnsBag ColumnsBag => DepartureMovementDetailsGridColumnsBag.Instance;
	}
}
