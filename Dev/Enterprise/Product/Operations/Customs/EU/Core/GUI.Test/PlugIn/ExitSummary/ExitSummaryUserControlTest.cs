using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	sealed class ExitSummaryUserControlTest : ExitSummaryUserControlForVirtualPropertiesTest<ExitSummaryUserControl>
	{
		public void TestCXI_NetMassUQ_IsADropEdit()
		{
			using (var control = new ExitSummaryUserControlForTest())
			{
				AssertType<ZDropEditColumnStyleInfo>(control.ItemsGridForTest.GetColumnStyle(CusExitItem.Schema.CXI_NetMassUQ));
			}
		}

		public void TestCXI_GrossMassUQ_IsADropEdit()
		{
			using (var control = new ExitSummaryUserControlForTest())
			{
				AssertType<ZDropEditColumnStyleInfo>(control.ItemsGridForTest.GetColumnStyle(CusExitItem.Schema.CXI_GrossMassUQ));
			}
		}

		public void TestB5_UnitType_IsADropEdit()
		{
			using (var control = new ExitSummaryUserControlForTest())
			{
				AssertType<ZDropEditColumnStyleInfo>(control.PackingGridForTest.GetColumnStyle(CusExitItemPackage.Schema.B5_UnitType));
			}
		}

		public void TestItemsGridColumns()
		{
			using (var control = new ExitSummaryUserControlForTest())
			{
				var expectedColumns = new[]
				{
					CusExitItem.Schema.CXI_LineNumber, CusExitItem.Schema.CXI_NetMass, CusExitItem.Schema.CXI_NetMassUQ,
					CusExitItem.Schema.CXI_GrossMass, CusExitItem.Schema.CXI_GrossMassUQ, CusExitItem.Schema.CXI_Status
				};
				AssertContainsExactElementsInAnyOrder(expectedColumns, control.ItemsGridForTest.ColumnStyles.Cast<ZTextBoxColumnStyleInfo>().Select(x => x.ColumnName));
			}
		}

		public void TestPackingDetailsGridColumns()
		{
			using (var control = new ExitSummaryUserControlForTest())
			{
				var expectedColumns = new[] { CusExitItemPackage.Schema.B5_UnitType, CusExitItemPackage.Schema.B5_UnitCount, CusExitItemPackage.Schema.B5_MarksAndNumbers };
				AssertContainsExactElementsInAnyOrder(expectedColumns, control.PackingGridForTest.ColumnStyles.Cast<ZTextBoxColumnStyleInfo>().Select(x => x.ColumnName));
			}
		}

		public void TestMovementsGridColumns()
		{
			using (var control = new ExitSummaryUserControlForTest())
			{
				AssertContainsExactElementsInAnyOrder(MovementColumnDetails.Select(x => x.ColumnName), control.MovementsGridForTest.ColumnStyles.Cast<ZTextBoxColumnStyleInfo>().Select(x => x.ColumnName));
			}
		}

		public void TestMovementsGridColumnCaptions()
		{
			using (var control = new ExitSummaryUserControlForTest())
			{
				AssertContainsExactElementsInAnyOrder(MovementColumnDetails.Select(x => x.ColumnCaption), control.MovementsGridForTest.ColumnStyles.Cast<ZTextBoxColumnStyleInfo>().Select(x => x.CaptionResourceString.Caption));
			}
		}

		public void TestMovementsGridColumnWidths()
		{
			using (var control = new ExitSummaryUserControlForTest())
			{
				foreach (var (columnName, _, columnWidth) in MovementColumnDetails)
				{
					AssertEquals(columnName, columnWidth, control.MovementsGridForTest.GetColumnStyle(columnName).Width);
				}
			}
		}

		public void TestMovementsGridCaptionVisible()
		{
			using (var control = new ExitSummaryUserControlForTest())
			{
				AssertEquals(false, control.MovementsGridForTest.CaptionVisible);
			}
		}

		public void TestControlsVisibility()
		{
			var exitHeader = Factory.New<CusExitControlHeader>();

			using (var form = new ZForm(exitHeader))
			using (var control = new ExitSummaryUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					AssertControlVisibleInitially<ZOrgAddressControl>(control, "AgentOrgAddressControl");
					AssertControlVisibleInitially<ZTextBox>(control, "ReferenceNumberTextBox");
					AssertControlVisibleInitially<ZGrid>(control, "MovementsGrid");
					AssertControlVisibleInitially<ZTextBox>(control, "MovementReferenceNumberTextBox");
					AssertControlVisibleInitially<ZCodeFindBox>(control, "CustomsOfficeCodeFindBox");
					AssertControlVisibleInitially<ZDateEdit>(control, "ArrivalNotificationDateDateEdit");
					AssertControlVisibleInitially<ZDateEdit>(control, "ExitDateDateEdit");
					AssertControlVisibleInitially<ZTextBox>(control, "TransportIdTextBox");
					AssertControlVisibleInitially<ZDropEdit>(control, "StatusDropEdit");
					AssertControlVisibleInitially<ZOrgAddressControl>(control, "CarrierOrgAddressControl");
					AssertControlVisibleInitially<ZGroupBox>(control, "MessagesGroupBox");
					AssertControlVisibleInitially<ZGrid>(control, "ItemsGrid");
					AssertControlVisibleInitially<ZGrid>(control, "PackingGrid");
				});
			}
		}

		public void TestMessagesTabUserControlType()
		{
			using (var exitSummaryUserControl = new ExitSummaryUserControl())
			{
				var messagesUserControl = exitSummaryUserControl.FindSingle<ZDynamicControlCreationUserControl>("MessagesUserControl");
				AssertEquals("MessagesTab is correct type", typeof(MessagesTabUserControl), messagesUserControl.UserControlType);
			}
		}

		void AssertControlVisibleInitially<T>(Control parentControl, ZString name) where T : Control
		{
			var control = (T)parentControl.Controls.Find(name, true).Single();
			AssertEquals($"{parentControl} is visible initially.", true, control.Visible);
		}

		static (string ColumnName, string ColumnCaption, int ColumnWidth)[] MovementColumnDetails => new[]
		{
			(CusExitDetail.Schema.CED_MovementReferenceNumber, "MRN", 229),
			(CusExitDetail.Schema.CED_CustomsOffice, "Exit Customs Office", 117),
			(CusExitDetail.Schema.CED_ArrivalNotificationDate, "Arrival Notification Date", 138),
			(CusExitDetail.Schema.CED_ArrivalNotificationPlace, "Arrival Notification Place", 320),
			(CusExitDetail.Schema.CED_ExitDate, "Exit Date", 67),
			(CusExitDetail.Schema.CED_TransportID, "Transport ID", 324),
			(CusExitDetail.Schema.CED_Status, "Status", 53)
		};
	}

	public class ExitSummaryUserControlForTest : ExitSummaryUserControl
	{
		public ExitSummaryUserControlForTest() : base()
		{
		}

		public ZGrid ItemsGridForTest => ItemsGrid;

		public ZGrid PackingGridForTest => PackingGrid;

		public ZGrid MovementsGridForTest => MovementsGrid;
	}
}
