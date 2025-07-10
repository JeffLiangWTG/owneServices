using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.Presentation.GUI;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Moq;

namespace Enterprise.Accounting.TaxFramework.GUI.Testing
{
	public class InvoiceOtherTaxesControlTest : TestCaseWithFactory
	{
		public void TestTaxRecordsGrid()
		{
			using (var control = new InvoiceOtherTaxesControl())
			{
				var taxRecordsGrid = control.GetField("TaxRecordsGrid") as ZGrid;
				Assert(taxRecordsGrid.AllowReadOnlyRowsToBeDeleted);

				var expectedListOfColumns = new[]
				{
					$"{AccTaxTransaction.Schema.ATT_AT_TaxID} (ZGuidFindBoxColumnStyleInfo) IsVisible:True",
					$"{AccTaxTransaction.Schema.ATT_A9_TaxMessage} (ZGuidFindBoxColumnStyleInfo) IsVisible:True",
					$"ATT_Rate (ZCalcEditColumnStyleInfo) IsVisible:True",
					$"{AccTaxTransaction.Schema.ATT_RX_NKOSTaxCurrency} (ZTextBoxColumnStyleInfo) IsVisible:True",
					$"{AccTaxTransaction.Schema.ATT_OSTaxBaseAmount} (ZCalcEditColumnStyleInfo) IsVisible:True",
					$"{AccTaxTransaction.Schema.ATT_OSTaxAmount} (ZCalcEditColumnStyleInfo) IsVisible:True",
					$"{AccTaxTransaction.Schema.ATT_TaxAuthorityServiceCode} (ZTextBoxColumnStyleInfo) IsVisible:True",
					$"{AccTaxTransaction.Schema.ATT_TaxSystemCode} (ZTextBoxColumnStyleInfo) IsVisible:True",
					$"{AccTaxTransaction.Schema.ATT_LocalTaxBaseAmount} (ZCalcEditColumnStyleInfo) IsVisible:True",
					$"{AccTaxTransaction.Schema.ATT_LocalTaxAmount} (ZCalcEditColumnStyleInfo) IsVisible:True",
					$"{AccTaxTransaction.Schema.ATT_Basis} (ZTextBoxColumnStyleInfo) IsVisible:True",
					$"{AccTaxTransaction.Schema.ATT_AffectsSourceTransactionTotal} (ZCheckBoxColumnStyleInfo) IsVisible:True",
					$"{AccTaxTransaction.Schema.ATT_TaxDate} (ZDateEditColumnStyleInfo) IsVisible:True",
					$"{AccTaxTransaction.Schema.ATT_PostDate} (ZDateEditColumnStyleInfo) IsVisible:True",
					$"{AccTaxTransaction.Schema.ATT_Ledger} (ZTextBoxColumnStyleInfo) IsVisible:True",
					$"{AccTaxTransaction.Schema.ATT_TaxSuperType} (ZTextBoxColumnStyleInfo) IsVisible:False",
					$"IsInDatabase_ForBinding (ZCheckBoxColumnStyleInfo) IsVisible:False",
					$"{AccTaxTransaction.Schema.ATT_IsCancelled} (ZCheckBoxColumnStyleInfo) IsVisible:False",
					$"{AccTaxTransaction.Schema.ATT_RealisationDate} (ZDateEditColumnStyleInfo) IsVisible:False",
					$"ATT_AH_MatchTransaction_ForBinding (ZTextBoxColumnStyleInfo) IsVisible:False",
					$"TaxAuthorityCode (ZTextBoxColumnStyleInfo) IsVisible:False",
					$"{AccTaxTransaction.Schema.ATT_TaxAuthorityServiceCodeDescription} (ZTextBoxColumnStyleInfo) IsVisible:False",
				};
				var realListOfColumns = taxRecordsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => $"{x.ToString()} IsVisible:{x.IsVisible}").ToArray();
				AssertArrayEqualsByElements(expectedListOfColumns, realListOfColumns);

				AssertDateEditColumnStyle(taxRecordsGrid.GetColumnStyle(AccTaxTransaction.Schema.ATT_PostDate), ZDateTimePickerFormat.Short);
				AssertDateEditColumnStyle(taxRecordsGrid.GetColumnStyle(AccTaxTransaction.Schema.ATT_RealisationDate), ZDateTimePickerFormat.Short);
				AssertDateEditColumnStyle(taxRecordsGrid.GetColumnStyle(AccTaxTransaction.Schema.ATT_TaxDate), ZDateTimePickerFormat.Short);
			}

			void AssertDateEditColumnStyle(ZGridColumnInfo columnStyle, ZDateTimePickerFormat dateFormat)
			{
				AssertType<ZDateEditColumnStyleInfo>(columnStyle);
				var dateColumnStyle = (ZDateEditColumnStyleInfo)columnStyle;
				AssertEquals(columnStyle.ColumnName, dateFormat, dateColumnStyle.DateTimeFormat);
			}
		}

		public void TestLinesGrid()
		{
			using (var form = new ZForm())
			using (var control = new InvoiceOtherTaxesControl())
			{
				form.Controls.Add(control);
				form.Show();

				var linesGrid = control.GetField("LinesGrid") as ZGrid;
				Assert("ReadOnly", linesGrid.ReadOnly);

				var expectedListOfColumns = new[]
				{
					"ChargeCode (ZTextBoxColumnStyleInfo) IsVisible:True",
					"ChargeCodeDescription (ZTextBoxColumnStyleInfo) IsVisible:True",
					"Branch (ZTextBoxColumnStyleInfo) IsVisible:True",
					"TaxBranch (ZTextBoxColumnStyleInfo) IsVisible:True",
					"SupplyType (ZTextBoxColumnStyleInfo) IsVisible:True",
					"Department (ZTextBoxColumnStyleInfo) IsVisible:True",
					"Job (ZTextBoxColumnStyleInfo) IsVisible:True",
					"GovtChargeCode (ZTextBoxColumnStyleInfo) IsVisible:True",
					"Currency (ZTextBoxColumnStyleInfo) IsVisible:True",
					"OSExTaxAmount (ZCalcEditColumnStyleInfo) IsVisible:True",
					"OSTaxAmount (ZCalcEditColumnStyleInfo) IsVisible:True",
					"OSTotalAmount (ZCalcEditColumnStyleInfo) IsVisible:True",
					"LocalExTaxAmount (ZCalcEditColumnStyleInfo) IsVisible:True",
					"LocalTaxAmount (ZCalcEditColumnStyleInfo) IsVisible:True",
					"LocalTotalAmount (ZCalcEditColumnStyleInfo) IsVisible:True",
				};
				var realListOfColumns = linesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => $"{x.ToString()} IsVisible:{x.IsVisible}").ToArray();
				AssertArrayEqualsByElements(expectedListOfColumns, realListOfColumns);
			}
		}

		public void TestSuspendTaxRecordsGridListChanged()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			using (var form = new ZForm())
			using (var control = new InvoiceOtherTaxesControl())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				var taxRecordsGrid = control.GetField("TaxRecordsGrid") as ZGrid;

				AssertNull("TaxTransactionCollection is empty", taxRecordsGrid.List);
				AssertEquals("SuspendTaxRecordsGridListChanged returns DisposableAction.NoAction as collection is empty", DisposableAction.NoAction, control.SuspendTaxRecordsGridListChanged());

				var invoiceFormPresentationProviderMock = new Mock<IInvoiceFormPresentationProvider>();
				control.Initialize(invoiceFormPresentationProviderMock.Object);
				control.Bind(TaxFrameworkObjectFactory.GetInvoicingBaseForDisplayOtherTaxes(invoice), "TaxRecordTransactionLinePivotForDisplay");

				var taxTransaction = Factory.New<AccTaxTransaction>();
				taxTransaction.ATT_AH = invoice.PK;
				AssertNotNull("TaxTransactionCollection is not empty", taxRecordsGrid.List);
				AssertNotEquals("SuspendTaxRecordsGridListChanged return is Not DisposableAction.NoAction as collection is not empty", DisposableAction.NoAction, control.SuspendTaxRecordsGridListChanged());
			}
		}

		public void TestDataBindingChangesWhenPointingToTransactionLinesButtonPressed()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			using (var form = new ZForm())
			using (var control = new InvoiceOtherTaxesControl())
			{
				form.Controls.Add(control);
				form.Show();

				var invoiceFormPresentationProviderMock = new Mock<IInvoiceFormPresentationProvider>();
				control.Initialize(invoiceFormPresentationProviderMock.Object);
				control.Bind(TaxFrameworkObjectFactory.GetInvoicingBaseForDisplayOtherTaxes(invoice), "TaxRecordTransactionLinePivotForDisplay");
				Application.DoEvents();

				var pointingToTransactionLines = control.GetField("PointingToTransactionLines") as ZToolStripButton;
				var linesGrid = control.GetField("LinesGrid") as ZGrid;
				var taxRecordsGrid = control.GetField("TaxRecordsGrid") as ZGrid;

				AssertEquals("TransactionLinesForOtherTaxesDisplay", linesGrid.BindTo);
				AssertEquals("TaxTransactionCollection", taxRecordsGrid.BindTo);

				pointingToTransactionLines.PerformClick();
				Assert(pointingToTransactionLines.Checked);
				AssertEquals("TransactionLinesForOtherTaxesDisplay", linesGrid.BindTo);
				AssertEquals("TaxTransactionCollection", taxRecordsGrid.BindTo);

				pointingToTransactionLines.PerformClick();
				Assert(!pointingToTransactionLines.Checked);
				AssertEquals("TransactionLinesForOtherTaxesDisplay", linesGrid.BindTo);
				AssertEquals("TaxTransactionCollection", taxRecordsGrid.BindTo);

				var taxTransaction = Factory.New<AccTaxTransaction>();
				taxTransaction.ATT_AH = invoice.PK;
				pointingToTransactionLines.PerformClick();
				Assert(pointingToTransactionLines.Checked);
				AssertEquals("TaxTransactionCollection", taxRecordsGrid.BindTo);
				AssertEquals("TaxTransactionCollection.TransactionLinesLinkedToOtherTaxesCollection", linesGrid.BindTo);
			}
		}

		public void TestDataBindingChangesWhenPointingToOtherTaxesButtonPressed()
		{
			var mockITaxFrameworkConfigurationHelper = TaxFrameworkTestObjectCreator.SetupMockTaxFrameworkConfigurationHelper();
			mockITaxFrameworkConfigurationHelper.Setup(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), It.IsAny<ZString>())).Returns(true);

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			using (var form = new ZForm())
			using (var control = new InvoiceOtherTaxesControl())
			{
				form.Controls.Add(control);
				form.Show();

				var invoiceFormPresentationProviderMock = new Mock<IInvoiceFormPresentationProvider>();
				control.Initialize(invoiceFormPresentationProviderMock.Object);
				control.Bind(TaxFrameworkObjectFactory.GetInvoicingBaseForDisplayOtherTaxes(invoice), "TaxRecordTransactionLinePivotForDisplay");
				Application.DoEvents();

				var pointingToOtherTaxes = control.GetField("PointingToOtherTaxes") as ZToolStripButton;
				var linesGrid = control.GetField("LinesGrid") as ZGrid;
				var taxRecordsGrid = control.GetField("TaxRecordsGrid") as ZGrid;

				AssertEquals("TransactionLinesForOtherTaxesDisplay", linesGrid.BindTo);
				AssertEquals("TaxTransactionCollection", taxRecordsGrid.BindTo);

				pointingToOtherTaxes.PerformClick();
				Assert(pointingToOtherTaxes.Checked);
				AssertEquals("TransactionLinesForOtherTaxesDisplay", linesGrid.BindTo);
				AssertEquals("TaxTransactionCollection", taxRecordsGrid.BindTo);

				pointingToOtherTaxes.PerformClick();
				Assert(!pointingToOtherTaxes.Checked);
				AssertEquals("TransactionLinesForOtherTaxesDisplay", linesGrid.BindTo);
				AssertEquals("TaxTransactionCollection", taxRecordsGrid.BindTo);

				TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.USD, 2m, 100m);
				TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice).IsTaxTransactionsCalculatedBeforePosting = true;
				pointingToOtherTaxes.PerformClick();
				Assert(pointingToOtherTaxes.Checked);
				AssertEquals("TransactionLinesForOtherTaxesDisplay", linesGrid.BindTo);
				AssertEquals("TransactionLinesForOtherTaxesDisplay.OtherTaxesLinkedToTransactionLinesCollection", taxRecordsGrid.BindTo);
			}
		}

		public void TestProcessWhenOtherTaxesAddedOrRemoved()
		{
			var mockITaxFrameworkConfigurationHelper = TaxFrameworkTestObjectCreator.SetupMockTaxFrameworkConfigurationHelper();
			mockITaxFrameworkConfigurationHelper.Setup(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), It.IsAny<ZString>())).Returns(true);

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			using (var form = new ZForm())
			using (var control = new InvoiceOtherTaxesControl())
			{
				form.Controls.Add(control);
				form.Show();

				var invoiceFormPresentationProviderMock = new Mock<IInvoiceFormPresentationProvider>();
				control.Initialize(invoiceFormPresentationProviderMock.Object);
				control.Bind(TaxFrameworkObjectFactory.GetInvoicingBaseForDisplayOtherTaxes(invoice), "TaxRecordTransactionLinePivotForDisplay");
				Application.DoEvents();

				var pointingToTransactionLines = control.GetField("PointingToTransactionLines") as ZToolStripButton;
				var pointingToOtherTaxes = control.GetField("PointingToOtherTaxes") as ZToolStripButton;
				var linesGrid = control.GetField("LinesGrid") as ZGrid;
				var taxRecordsGrid = control.GetField("TaxRecordsGrid") as ZGrid;

				var taxTransaction = Factory.New<AccTaxTransaction>();
				taxTransaction.ATT_AH = invoice.PK;
				pointingToTransactionLines.PerformClick();
				Assert(pointingToTransactionLines.Checked);
				Assert(!pointingToOtherTaxes.Checked);
				AssertEquals("TaxTransactionCollection", taxRecordsGrid.BindTo);
				AssertEquals("TaxTransactionCollection.TransactionLinesLinkedToOtherTaxesCollection", linesGrid.BindTo);

				control.ProcessWhenOtherTaxesAddedOrRemoved();
				Assert(!pointingToTransactionLines.Checked);
				Assert(!pointingToOtherTaxes.Checked);
				AssertEquals("TransactionLinesForOtherTaxesDisplay", linesGrid.BindTo);
				AssertEquals("TaxTransactionCollection", taxRecordsGrid.BindTo);

				TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.USD, 2m, 100m);
				TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice).IsTaxTransactionsCalculatedBeforePosting = true;
				pointingToOtherTaxes.PerformClick();
				Assert(!pointingToTransactionLines.Checked);
				Assert(pointingToOtherTaxes.Checked);
				AssertEquals("TransactionLinesForOtherTaxesDisplay", linesGrid.BindTo);
				AssertEquals("TransactionLinesForOtherTaxesDisplay.OtherTaxesLinkedToTransactionLinesCollection", taxRecordsGrid.BindTo);

				control.ProcessWhenOtherTaxesAddedOrRemoved();
				Assert(!pointingToTransactionLines.Checked);
				Assert(!pointingToOtherTaxes.Checked);
				AssertEquals("TransactionLinesForOtherTaxesDisplay", linesGrid.BindTo);
				AssertEquals("TaxTransactionCollection", taxRecordsGrid.BindTo);
			}
		}

		public void TestPressButtonAndAssertOtherUnchecked()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			using (var form = new ZForm())
			using (var control = new InvoiceOtherTaxesControl())
			{
				form.Controls.Add(control);
				form.Show();

				var invoiceFormPresentationProviderMock = new Mock<IInvoiceFormPresentationProvider>();
				control.Initialize(invoiceFormPresentationProviderMock.Object);
				control.Bind(TaxFrameworkObjectFactory.GetInvoicingBaseForDisplayOtherTaxes(invoice), "TaxRecordTransactionLinePivotForDisplay");
				Application.DoEvents();

				var pointingToOtherTaxes = control.GetField("PointingToOtherTaxes") as ZToolStripButton;
				var pointingToTransactionLines = control.GetField("PointingToTransactionLines") as ZToolStripButton;

				PressButtonAndAssertOtherUnchecked(pointingToOtherTaxes, pointingToTransactionLines);
				PressButtonAndAssertOtherUnchecked(pointingToTransactionLines, pointingToOtherTaxes);
			}
		}

		void PressButtonAndAssertOtherUnchecked(ZToolStripButton mainButton, ZToolStripButton otherButton)
		{
			Assert("Precondition: mainButton is unchecked", !mainButton.Checked);
			Assert("Precondition: otherButton is unchecked", !otherButton.Checked);

			otherButton.PerformClick();
			Assert(otherButton.Checked);
			Assert(!mainButton.Checked);

			mainButton.PerformClick();
			Assert(mainButton.Checked);
			Assert("Other button gets unpressed", !otherButton.Checked);

			mainButton.PerformClick();
			Assert("Main button gets unpressed", !mainButton.Checked);
			Assert(!otherButton.Checked);
		}

		public void TestDataSourceType()
		{
			using (var control = new InvoiceOtherTaxesControl())
			{
				AssertEquals("DataSourceType is AccTaxRecordTransactionLinePivotForDisplay", typeof(AccTaxRecordTransactionLinePivotForDisplay), control.DataSourceType);
			}
		}

		public void TestSupplyTypeColumnVisibility()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));

			var invoiceFormPresentationProviderMock = new Mock<IInvoiceFormPresentationProvider>();

			AssertSupplyTypeColumnVisibility("When IsSupplyTypeColumnVisible() returns false", false);
			AssertSupplyTypeColumnVisibility("When IsSupplyTypeColumnVisible() returns true", true);

			void AssertSupplyTypeColumnVisibility(string message, bool isColumnVisible)
			{
				invoiceFormPresentationProviderMock.Setup(x => x.IsSupplyTypeColumnVisible()).Returns(isColumnVisible);

				using (var form = new ZForm())
				using (var control = new InvoiceOtherTaxesControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.Initialize(invoiceFormPresentationProviderMock.Object);
					control.Bind(TaxFrameworkObjectFactory.GetInvoicingBaseForDisplayOtherTaxes(invoice), "TaxRecordTransactionLinePivotForDisplay");
					Application.DoEvents();

					var linesGrid = control.GetField("LinesGrid") as ZGrid;

					invoiceFormPresentationProviderMock.Verify(x => x.IsSupplyTypeColumnVisible());
					AssertEquals(message, !isColumnVisible, linesGrid.GetColumnStyle("SupplyType").IsUnavailable);
				}
			}
		}

		public void TestTaxBranchColumnVisibility()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));

			var invoiceFormPresentationProviderMock = new Mock<IInvoiceFormPresentationProvider>();

			AssertTaxBranchColumnVisibility("When IsTaxBranchColumnVisible() returns false", false);
			AssertTaxBranchColumnVisibility("When IsTaxBranchColumnVisible() returns true", true);

			void AssertTaxBranchColumnVisibility(string message, bool isColumnVisible)
			{
				invoiceFormPresentationProviderMock.Setup(x => x.IsTaxBranchColumnVisible()).Returns(isColumnVisible);

				using (var form = new ZForm())
				using (var control = new InvoiceOtherTaxesControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.Initialize(invoiceFormPresentationProviderMock.Object);
					control.Bind(TaxFrameworkObjectFactory.GetInvoicingBaseForDisplayOtherTaxes(invoice), "TaxRecordTransactionLinePivotForDisplay");
					Application.DoEvents();

					var linesGrid = control.GetField("LinesGrid") as ZGrid;

					invoiceFormPresentationProviderMock.Verify(x => x.IsTaxBranchColumnVisible());
					AssertEquals(message, !isColumnVisible, linesGrid.GetColumnStyle("TaxBranch").IsUnavailable);
				}
			}
		}

		public void TestOnAfterFirstBinding_ThrowsException_WhenPresentationProviderNotInitialised()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));

			using (var form = new ZForm())
			using (var control = new InvoiceOtherTaxesControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertExceptionThrown<InvalidOperationException>("Initialize() not called before Bind()",
					"presentationProvider must be initialised by invoking Initialize() method before binding the control to datasource.",
					() => control.Bind(TaxFrameworkObjectFactory.GetInvoicingBaseForDisplayOtherTaxes(invoice), "TaxRecordTransactionLinePivotForDisplay"));
			}
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (testObjectCreator == null)
				{
					testObjectCreator = new TestObjectCreator(Factory);
				}
				return testObjectCreator;
			}
		}
		TestObjectCreator testObjectCreator;

		TaxFrameworkTestObjectCreator TaxFrameworkTestObjectCreator
		{
			get { return taxFrameworkTestObjectCreator ?? (taxFrameworkTestObjectCreator = new TaxFrameworkTestObjectCreator(Factory)); }
		}
		TaxFrameworkTestObjectCreator taxFrameworkTestObjectCreator;
	}
}
