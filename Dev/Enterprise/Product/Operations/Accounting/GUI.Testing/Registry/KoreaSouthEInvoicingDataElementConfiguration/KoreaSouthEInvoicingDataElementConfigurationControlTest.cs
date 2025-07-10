using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Registry.GUI;
using Enterprise.DocumentEngine.GUI.ReflectiveFieldMap;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(KoreaSouthEInvoicingDataElementConfigurationControl))]
	public class KoreaSouthEInvoicingDataElementConfigurationControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new KoreaSouthEInvoicingDataElementConfigurationCollection
		{
			new KoreaSouthEInvoicingDataElementConfiguration(EInvoicingKoreaSouthConstants.InvoiceTypeList.OriginalInvoice, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine1),
		};

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var koreaSouthEInvoicingDataElementConfigurationControl = control as KoreaSouthEInvoicingDataElementConfigurationControl;
			AssertNotNull(koreaSouthEInvoicingDataElementConfigurationControl);

			var grid = koreaSouthEInvoicingDataElementConfigurationControl.FindSingleOrDefault<ZGrid>("ConfigurationGrid");
			AssertNotNull(grid);

			var button = koreaSouthEInvoicingDataElementConfigurationControl.FindSingleOrDefault<ZButton>("MacroButton");
			AssertNotNull(button);

			return grid.ReadOnly && button.ReadOnly;
		}

		public void TestMacroButtonWithFocusOnANewLine()
		{
			using (var control = GetNewControl() as KoreaSouthEInvoicingDataElementConfigurationControl)
			using (var form = new ZForm())
			{
				var businessEntity = GetNewBusinessEntity();
				control.SetDataBinding(businessEntity, null);
				form.Controls.Add(control);
				form.Show();

				var button = control.Controls.Find("MacroButton", true)[0] as ZButton;
				var grid = control.Controls.Find("ConfigurationGrid", true)[0] as ZGrid;

				AssertEquals("Precondition: not focus on grid", false, grid.ContainsFocus);
				AssertNoExceptionThrown(() => button.PerformClick());
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("To insert a macro, please place the cursor inside the cell in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				grid.Focus();
				AssertEquals("Precondition: focus on grid", true, grid.ContainsFocus);
				AssertNoExceptionThrown(() => button.PerformClick());
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOnMapTreeFormShown()
		{
			using (var control = GetNewControl() as KoreaSouthEInvoicingDataElementConfigurationControl)
			using (var form = new ZForm())
			{
				var businessEntity = GetNewBusinessEntity();
				control.SetDataBinding(businessEntity, null);
				form.Controls.Add(control);
				form.Show();

				var button = control.Controls.Find("MacroButton", true)[0] as ZButton;
				var grid = control.Controls.Find("ConfigurationGrid", true)[0] as ZGrid;

				grid.Focus();
				ZFormModaliser.ShowDialogsInTest = true;
				AssertNoExceptionThrown(() => button.PerformClick());

				var mapTreeForm = ZFormModaliser.LastFormShownDialogForTest as MapTreeForm;
				AssertNotNull(mapTreeForm);
				AssertEquals(false, mapTreeForm.ShowEditor);
				AssertEquals(false, mapTreeForm.macrosTab.TabVisible);
			}
		}

		public void TestReplaceConfiguration()
		{
			using (var control = GetNewControl() as KoreaSouthEInvoicingDataElementConfigurationControl)
			using (var form = new ZForm())
			{
				var businessEntity = GetNewBusinessEntity() as KoreaSouthEInvoicingDataElementConfigurationCollection;
				businessEntity[0].Configuration = "<Marco1>";
				control.SetDataBinding(businessEntity, null);
				form.Controls.Add(control);
				form.Show();

				var button = control.Controls.Find("MacroButton", true)[0] as ZButton;
				var grid = control.Controls.Find("ConfigurationGrid", true)[0] as ZGrid;
				grid.Focus();

				var mock = new Mock<IMapTreePresentationManager>();
				mock.Setup(x => x.GetUserSelectionMacro()).Returns("<Marco2>");
				using (ObjectFactory.Substitute(mock.Object))
				{
					AssertNoExceptionThrown(() => button.PerformClick());
					AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("<Marco1> should be updated to <Marco2><Marco1>", "<Marco2><Marco1>", businessEntity[0].Configuration);

					grid.Focus();
					grid.CurrentCell = new DataGridCell(0, 2);
					var selectedTextBox = (grid.Columns[2].ColumnStyle as ZTextBoxColumnStyle).EditControl as DataGridTextBox;
					selectedTextBox.Select(2, 8);
					form.Cursor = Cursors.Arrow;
					form.Cursor = Cursors.WaitCursor;   // To invoke the CursorChanged event

					AssertNoExceptionThrown(() => button.PerformClick());
					AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("<Marco2><Marco1> should be updated to <M<Marco2>arco1>", "<M<Marco2>arco1>", businessEntity[0].Configuration);
				}
			}
		}
	}
}
