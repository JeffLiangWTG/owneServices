using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using static Enterprise.Customs.EU.Business.TemporaryStorageHelper;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing;

class TempStorageRegisterUserControlTest : TestCaseWithFactory
{
	public void TestLinesDetailsGroupBox()
	{
		CombineAssertions(() =>
		{
			var tempStorageRegHeader = Factory.New<CusTempStorageRegHeader>();
			userControl.SetDataBinding(tempStorageRegHeader, "");
			var dynamicDetailsPanel = userControl.DynamicDetailsPanel;
			AssertEquals("LineDetailsGroupBox Caption", "Line Details", userControl.LineDetailsGroupBox.CaptionResourceString.Caption);
			AssertEquals("DynamicDetailsPanel Dock", DockStyle.Fill, dynamicDetailsPanel.Dock);
			AssertEquals("Binding", ".", dynamicDetailsPanel.GetBindingMember());
			AssertEquals("Within LineDetailsGroupBox", expected: true, userControl.LineDetailsGroupBox.Controls.Contains(dynamicDetailsPanel));
		});
	}

	public void TestLinesGrid()
	{
		using var form = new ZForm(Factory.New<CusTempStorageRegHeader>());

		form.Controls.Add(userControl);
		form.Show();

		CombineAssertions(() =>
		{
			var linesGrid = userControl.LinesGrid;

			void AssertGridColumn(string columnName, int width, string caption, string groupName = null)
			{
				var columnStyle = linesGrid.GetColumnStyle(columnName);
				AssertEquals($"{columnName} Width", width, columnStyle.Width);
				AssertEquals($"{columnName} Caption", caption, linesGrid.GetColumnCaption(columnName));
				if (groupName != null)
				{
					AssertEquals($"{columnName} GroupName", groupName, columnStyle.GroupName.Caption);
				}
			}

			AssertEquals("LinesGroupBox Caption", "Lines", userControl.LinesGroupBox.CaptionResourceString.Caption);
			AssertEquals("Column Count", 13, linesGrid.ColumnStyles.Count);

			AssertGridColumn(CusTempStorageRegLine.Schema.SRL_LineNumber, width: 88, caption: "Line Number");
			AssertGridColumn(CusTempStorageRegLine.Schema.SRL_LocationOfGoods, width: 110, caption: "Location of Goods");
			AssertGridColumn(CusTempStorageRegLine.Schema.SRL_GoodsDescription, width: 150, caption: "Goods Description");
			AssertGridColumn(nameof(CusTempStorageRegLine.PackagesRemainingCalculated), width: 126, caption: "Packages Remaining");
			AssertGridColumn(nameof(CusTempStorageRegLine.GrossWeightRemainingCalculated), width: 126, caption: "Gross Weight Remaining");
			AssertGridColumn(CusTempStorageRegLine.Schema.SRL_PackageType, width: 90, caption: "Package Type");
			AssertGridColumn(CusTempStorageRegLine.Schema.SRL_UnionStatus, width: 85, caption: "Union Status");
			AssertGridColumn(nameof(CusTempStorageRegLine.BondAmountRemainingCalculated), width: 140, caption: "Liability Amount Remaining");

			AssertGridColumn(CusTempStorageRegLine.Schema.SRL_OwnerReferenceType,
							width: 134, caption: "Owner Reference Type", groupName: "Owner Reference");

			AssertGridColumn(CusTempStorageRegLine.Schema.SRL_OwnerReference,
							width: 149, caption: "Owner Reference Number", groupName: "Owner Reference");

			var ownerReferenceTypeColumnInfo = linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_OwnerReferenceType);
			var ownerReferenceColumnInfo = linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_OwnerReference);
			AssertEquals("SRL_OwnerReference GroupName is the same as SRL_OwnerReferenceType", ownerReferenceTypeColumnInfo.GroupName.Key, ownerReferenceColumnInfo.GroupName.Key);

			var limitDateColumnInfo = (ZDateEditColumnStyleInfo)linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_LimitDate);
			AssertEquals("SRL_LimitDate DateTimeFormat", ZDateTimePickerFormat.Short, limitDateColumnInfo.DateTimeFormat);
			AssertGridColumn(CusTempStorageRegLine.Schema.SRL_LimitDate, width: 73, caption: "Limit Date");

			AssertGridColumn(CusTempStorageRegLine.Schema.SRL_CustomsStatus, width: 110, caption: "Customs Status");
			AssertGridColumn(CusTempStorageRegLine.Schema.SRL_PackageMarks, width: 149, caption: "Marks & Numbers");
		});
	}

	public void TestGuaranteeGroupBoxDynamicLayoutPanel()
	{
		var tempStorageRegHeader = Factory.New<CusTempStorageRegHeader>();
		userControl.SetDataBinding(tempStorageRegHeader, "");
		var dynamicGuaranteePanel = userControl.DynamicGuaranteePanel;
		CombineAssertions(() =>
		{
			AssertEquals("DynamicGuaranteePanel Dock", DockStyle.Fill, dynamicGuaranteePanel.Dock);
			AssertEquals("Binding", nameof(tempStorageRegHeader.Guarantee), dynamicGuaranteePanel.GetBindingMember());
			AssertEquals("Within GuaranteeGroupBox", expected: true, userControl.GuaranteeGroupBox.Controls.Contains(dynamicGuaranteePanel));

			DynamicLayoutPanelTest.AssertControlsOrder(dynamicGuaranteePanel,
			nameof(EU.GUI.GuaranteeGroupBoxControlBag.BondNumberCodeFindBox),

			nameof(EU.GUI.GuaranteeGroupBoxControlBag.AmountCalcDropEdit));
		});
	}

	public void TestHeaderGroupBoxDynamicLayoutPanel()
	{
		var tempStorageRegHeader = Factory.New<CusTempStorageRegHeader>();
		userControl.SetDataBinding(tempStorageRegHeader, "");
		var dynamicHeaderPanel = userControl.DynamicHeaderPanel;
		CombineAssertions(() =>
		{
			AssertEquals("DynamicHeaderPanel Dock", DockStyle.Fill, dynamicHeaderPanel.Dock);
			AssertEquals("Binding", ".", dynamicHeaderPanel.GetBindingMember());
			AssertEquals("Within HeaderGroupBox", expected: true, userControl.HeaderGroupBox.Controls.Contains(dynamicHeaderPanel));

			DynamicLayoutPanelTest.AssertControlsOrder(dynamicHeaderPanel,
			nameof(TempStorageRegisterHeaderControlBag.InternalReferenceTextBox),
			nameof(TempStorageRegisterHeaderControlBag.PreviousReferenceTypeDropEdit),
			nameof(TempStorageRegisterHeaderControlBag.DDTNumberUserControl),
			nameof(TempStorageRegisterHeaderControlBag.PreviousReferenceNumberTextBox),
			nameof(TempStorageRegisterHeaderControlBag.ArrivalDateEdit),
			nameof(TempStorageRegisterHeaderControlBag.PresentationDateEdit),
			nameof(TempStorageRegisterHeaderControlBag.StatusDropEdit));
		});
	}

	public void TestPremisesGroupBoxDynamicLayoutPanel()
	{
		var tempStorageRegHeader = Factory.New<CusTempStorageRegHeader>();
		userControl.SetDataBinding(tempStorageRegHeader, "");
		var dynamicPremisesPanel = userControl.DynamicPremisesPanel;
		CombineAssertions(() =>
		{
			AssertEquals("DynamicPremisesPanel Dock", DockStyle.Fill, dynamicPremisesPanel.Dock);
			AssertEquals("Binding", nameof(tempStorageRegHeader.Premises), dynamicPremisesPanel.GetBindingMember());
			AssertEquals("Within PremisesGroupBox", expected: true, userControl.PremisesGroupBox.Controls.Contains(dynamicPremisesPanel));

			DynamicLayoutPanelTest.AssertControlsOrder(dynamicPremisesPanel,
			nameof(TempStorageRegisterPremisesControlBag.PremisesCodeTextBox),
			nameof(TempStorageRegisterPremisesControlBag.PremisesDescriptionTextBox),
			nameof(TempStorageRegisterPremisesControlBag.LocationDropEdit));
		});
	}

	public void TestHeaderGroupBox()
	{
		CombineAssertions(() =>
		{
			var headerGroupBox = userControl.HeaderGroupBox;
			AssertEquals("Caption", "Header", headerGroupBox.CaptionResourceString.Caption);
			AssertEquals("Visibility for ES", expected: true, headerGroupBox.Visible);
		});
	}

	public void TestGuaranteeGroupBox()
	{
		CombineAssertions(() =>
		{
			var guaranteeGroupBox = userControl.GuaranteeGroupBox;
			AssertEquals("Caption", "Guarantee", guaranteeGroupBox.CaptionResourceString.Caption);
			AssertEquals("Visibility for ES", expected: true, guaranteeGroupBox.Visible);
		});
	}

	public void TestPremisesGroupBox()
	{
		CombineAssertions(() =>
		{
			var premisesGroupBox = userControl.PremisesGroupBox;
			AssertEquals("Caption", "Premises", premisesGroupBox.CaptionResourceString.Caption);
			AssertEquals("Visibility for ES", expected: true, premisesGroupBox.Visible);
		});
	}

	public void TestLinesTabControl()
	{
		CombineAssertions(() =>
		{
			var linesTabControl = userControl.LinesTabControl;
			AssertNotNull("LinesTabControl is not null", linesTabControl);
			AssertEquals("LinesTabControl tab pages", expected: 3, linesTabControl.TabCount);
		});
	}

	public void TestItemsDynamicLayoutPanel()
	{
		var tempStorageRegHeader = Factory.New<CusTempStorageRegHeader>();
		userControl.SetDataBinding(tempStorageRegHeader, "");
		var itemsDynamicLayoutPanel = userControl.ItemsDynamicLayoutPanel;
		CombineAssertions(() =>
		{
			AssertEquals("ItemsDynamicLayoutPanel Dock", DockStyle.Fill, itemsDynamicLayoutPanel.Dock);
			AssertEquals("Binding", ".", itemsDynamicLayoutPanel.GetBindingMember());
			AssertEquals("Within ItemsTabPage", expected: true, userControl.ItemsTabPage.Controls.Contains(itemsDynamicLayoutPanel));
		});
	}

	public void TestLineDetailsTabPage()
	{
		CombineAssertions(() =>
		{
			var lineDetailsTabPage = userControl.LineDetailsTabPage;
			AssertNotNull("LineDetailsTabPage is not null", lineDetailsTabPage);
			AssertEquals("LineDetailsTabPage tab pages", "Line Details", lineDetailsTabPage.CaptionResourceString.Caption);
		});
	}

	public void TestItemsTabPage()
	{
		CombineAssertions(() =>
		{
			var itemsTabPage = userControl.ItemsTabPage;
			AssertNotNull("ItemsTabPage is not null", itemsTabPage);
			AssertEquals("ItemsTabPage tab pages", "Items", itemsTabPage.CaptionResourceString.Caption);
		});
	}

	public void TestTransactionsTabPage()
	{
		CombineAssertions(() =>
		{
			var transactionsTabPage = userControl.TransactionsTabPage;
			AssertNotNull("TransactionsTabPage is not null", transactionsTabPage);
			AssertEquals("TransactionsTabPage tab pages", "Transactions", transactionsTabPage.CaptionResourceString.Caption);
		});
	}

	public void TestTransactionsUserControl()
	{
		CombineAssertions(() =>
		{
			var transactionsUserControl = userControl.TransactionsUserControl;
			AssertNotNull("TransactionsUserControl is not null", transactionsUserControl);
			AssertType<TempStorageRegTransactionUserControl>("TransactionsUserControl type", transactionsUserControl);
		});
	}

	public void TestRegLineGridMenuItems()
	{
		var testHeader = Factory.New<CusTempStorageRegHeader>();

		using (var form = new ZForm(testHeader))
		using (var userControl = new TempStorageRegisterUserControl())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(testHeader, ".");
			form.Show();

			AssertNotNull("Context menu to Assign Location and Reference exists", userControl.LinesGrid.ContextMenu.MenuItems.FindByText("Assign Location + Reference"));
		}
	}

	#region Assign Location and Reference

	public void TestAssignLocationAndReferenceClick_Cancel()
	{
		var testHeader = SetUpRegHeader();
		var regLine = SetUpRegLine(testHeader);
		Factory.Save();

		using (var userControl = new TempStorageRegisterUserControl())
		{
			userControl.SetDataBinding(testHeader, ZString.Empty);
			userControl.LinesGrid.SetDataBinding(testHeader.CusTempStorageRegLines, ZString.Empty);

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			var linesGrid = userControl.LinesGrid;

			var assignLocationAndReferenceMenuItem = linesGrid.ContextMenu.MenuItems.FindByText("Assign Location + Reference");
			linesGrid.Select();
			linesGrid.Focus();

			assignLocationAndReferenceMenuItem.PerformClick();

			CombineAssertions(() =>
			{
				AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

				linesGrid.SelectAllElements();
				assignLocationAndReferenceMenuItem.PerformClick();
				AssertEquals("RegLine SRL_LocationOfGoods has not been assigned", "Location", regLine.SRL_LocationOfGoods);
				AssertEquals("RegLine SRL_OwnerReference has not been assigned", "Reference", regLine.SRL_OwnerReference);
			});
		}
	}

	public void TestAssignLocationAndReferenceClick_EmptyTextBox()
	{
		var testHeader = SetUpRegHeader();
		var regLine = SetUpRegLine(testHeader);
		Factory.Save();

		var requestDataToUpdateLocationAndReference = new DataToUpdateLocationAndReference()
		{
			Location = ZString.Empty,
			EmptyLocation = ZBool.False,
			Reference = ZString.Empty,
			EmptyReference = ZBool.False,
		};

		using (var userControl = new TempStorageRegisterUserControlForTesting(requestDataToUpdateLocationAndReference))
		{
			userControl.SetDataBinding(testHeader, ZString.Empty);
			userControl.LinesGrid.SetDataBinding(testHeader.CusTempStorageRegLines, ZString.Empty);

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var linesGrid = userControl.LinesGrid;

			var assignLocationAndReferenceMenuItem = linesGrid.ContextMenu.MenuItems.FindByText("Assign Location + Reference");
			linesGrid.Select();
			linesGrid.Focus();

			linesGrid.SelectAllElements();
			assignLocationAndReferenceMenuItem.PerformClick();

			CombineAssertions(() =>
			{
				AssertEquals("RegLine SRL_LocationOfGoods has not been assigned when text is empty", "Location", regLine.SRL_LocationOfGoods);
				AssertEquals("RegLine SRL_OwnerReference has not been assigned when text is empty", "Reference", regLine.SRL_OwnerReference);
			});
		}
	}

	public void TestAssignLocationAndReferenceClick_EmptyIsChecked()
	{
		var testHeader = SetUpRegHeader();
		var regLine = SetUpRegLine(testHeader);
		Factory.Save();

		var requestDataToUpdateLocationAndReference = new DataToUpdateLocationAndReference()
		{
			Location = ZString.Empty,
			EmptyLocation = ZBool.True,
			Reference = ZString.Empty,
			EmptyReference = ZBool.True,
		};

		using (var userControl = new TempStorageRegisterUserControlForTesting(requestDataToUpdateLocationAndReference))
		{
			userControl.SetDataBinding(testHeader, ZString.Empty);
			userControl.LinesGrid.SetDataBinding(testHeader.CusTempStorageRegLines, ZString.Empty);

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var linesGrid = userControl.LinesGrid;

			var assignLocationAndReferenceMenuItem = linesGrid.ContextMenu.MenuItems.FindByText("Assign Location + Reference");
			linesGrid.Select();
			linesGrid.Focus();

			linesGrid.SelectAllElements();
			assignLocationAndReferenceMenuItem.PerformClick();

			CombineAssertions(() =>
			{
				AssertEquals("RegLine SRL_LocationOfGoods is Empty when EmptyLocation is true", ZString.Empty, regLine.SRL_LocationOfGoods);
				AssertEquals("RegLine SRL_OwnerReference is Empty when EmptyReference is true", ZString.Empty, regLine.SRL_OwnerReference);
			});
		}
	}

	public void TestAssignLocationAndReferenceClick_TextBoxIsNotEmptyAndEmptyIsNotChecked()
	{
		var testHeader = SetUpRegHeader();
		var regLine = SetUpRegLine(testHeader);
		Factory.Save();

		var requestDataToUpdateLocationAndReference = new DataToUpdateLocationAndReference()
		{
			Location = "NewLocation",
			EmptyLocation = ZBool.False,
			Reference = "NewReference",
			EmptyReference = ZBool.False,
		};

		using (var userControl = new TempStorageRegisterUserControlForTesting(requestDataToUpdateLocationAndReference))
		{
			userControl.SetDataBinding(testHeader, ZString.Empty);
			userControl.LinesGrid.SetDataBinding(testHeader.CusTempStorageRegLines, ZString.Empty);

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var linesGrid = userControl.LinesGrid;

			var assignLocationAndReferenceMenuItem = linesGrid.ContextMenu.MenuItems.FindByText("Assign Location + Reference");
			linesGrid.Select();
			linesGrid.Focus();

			linesGrid.SelectAllElements();
			assignLocationAndReferenceMenuItem.PerformClick();

			CombineAssertions(() =>
			{
				AssertEquals("RegLine SRL_LocationOfGoods is the Location text assigned when text is not empty and EmptyLocation is not true", "NewLocation", regLine.SRL_LocationOfGoods);
				AssertEquals("RegLine SRL_OwnerReference is the Reference text assigned when text is not empty and EmptyReference is not true", "NewReference", regLine.SRL_OwnerReference);
			});
		}
	}

	public void TestAssignLocationAndReferenceClick_MultipleLines()
	{
		var testHeader = SetUpRegHeader();
		var regLine1 = SetUpRegLine(testHeader, location: "Location1", reference: "Reference1");
		var regLine2 = SetUpRegLine(testHeader, 2, "Location2", "Reference2");
		regLine2.SRL_CustomsStatus = "CLS";
		var regLine3 = SetUpRegLine(testHeader, 3, "Location3", "Reference3");
		Factory.Save();

		var requestDataToUpdateLocationAndReference = new DataToUpdateLocationAndReference()
		{
			Location = "NewLocation",
			EmptyLocation = ZBool.False,
			Reference = ZString.Empty,
			EmptyReference = ZBool.True,
		};

		using (var userControl = new TempStorageRegisterUserControlForTesting(requestDataToUpdateLocationAndReference))
		{
			userControl.SetDataBinding(testHeader, ZString.Empty);
			userControl.LinesGrid.SetDataBinding(testHeader.CusTempStorageRegLines, ZString.Empty);

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var linesGrid = userControl.LinesGrid;

			var assignLocationAndReferenceMenuItem = linesGrid.ContextMenu.MenuItems.FindByText("Assign Location + Reference");
			linesGrid.Select();
			linesGrid.Focus();

			linesGrid.SelectAllElements();
			assignLocationAndReferenceMenuItem.PerformClick();

			CombineAssertions(() =>
			{
				AssertEquals("RegLine1 SRL_LocationOfGoods is the Location text assigned when text is not empty and EmptyLocation is not true", "NewLocation", regLine1.SRL_LocationOfGoods);
				AssertEquals("RegLine1 SRL_OwnerReference is Empty when EmptyReference is true", ZString.Empty, regLine1.SRL_OwnerReference);
				AssertEquals("RegLine2 SRL_LocationOfGoods is not assign cause Status is CLS", "Location2", regLine2.SRL_LocationOfGoods);
				AssertEquals("RegLine2 SRL_OwnerReference is not assign cause Status is CLS", "Reference2", regLine2.SRL_OwnerReference);
				AssertEquals("RegLine3 SRL_LocationOfGoods is the Location text assigned when text is not empty and EmptyLocation is not true", "NewLocation", regLine3.SRL_LocationOfGoods);
				AssertEquals("RegLine3 SRL_OwnerReference is Empty when EmptyReference is true", ZString.Empty, regLine3.SRL_OwnerReference);
			});
		}
	}

	#endregion

	protected override void SetUp()
	{
		base.SetUp();
		userControl = new TempStorageRegisterUserControl();
	}
	TempStorageRegisterUserControl userControl;

	protected override void TearDown()
	{
		base.TearDown();
		userControl.Dispose();
	}

	class TempStorageRegisterUserControlForTesting : TempStorageRegisterUserControl
	{
		public TempStorageRegisterUserControlForTesting(DataToUpdateLocationAndReference dataToUpdateLocationAndReference = null) : base()
		{
			requestDataToUpdateLocationAndReference = dataToUpdateLocationAndReference;
		}

		readonly DataToUpdateLocationAndReference requestDataToUpdateLocationAndReference;

		protected override DataToUpdateLocationAndReference GetAssignLocationAndReferenceForm() => requestDataToUpdateLocationAndReference;
	}

	CusTempStorageRegHeader SetUpRegHeader()
	{
		var testHeader = Factory.New<CusTempStorageRegHeader>();
		testHeader.SRH_AppCode = "123";
		testHeader.SRH_Status = "OK";
		testHeader.SRH_Reference = "TEST";
		return testHeader;
	}

	CusTempStorageRegLine SetUpRegLine(CusTempStorageRegHeader header, int line = 1, string location = "Location", string reference = "Reference")
	{
		var regLine = header.CusTempStorageRegLines.AddNew();
		regLine.SRL_LineNumber = line;
		regLine.SRL_LocationOfGoods = location;
		regLine.SRL_OwnerReference = reference;
		return regLine;
	}
}
