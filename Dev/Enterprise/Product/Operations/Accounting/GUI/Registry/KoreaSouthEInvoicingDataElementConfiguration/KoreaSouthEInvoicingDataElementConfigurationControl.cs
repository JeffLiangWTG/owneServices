using System;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.Accounting.Business.EInvoicing.KoreaSouth;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngine.GUI.ReflectiveFieldMap;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Res = Enterprise.Accounting.GUI.Res;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class KoreaSouthEInvoicingDataElementConfigurationControl : RegistryZUserControl
	{
		public KoreaSouthEInvoicingDataElementConfigurationControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ConfigurationGrid.ReadOnly = readOnly;
			MacroButton.ReadOnly = readOnly;
		}

		void RememberCursorPosition(object sender, EventArgs e)
		{
			if (ConfigurationGrid.ContainsFocus && ConfigurationGrid.Enabled)
			{
				var currentTextBox = ((ZTextBoxColumnStyle)ConfigurationGrid.Columns[ConfigurationGrid.CurrentCell.ColumnNumber].ColumnStyle).EditControl as DataGridTextBox;
				if (currentTextBox != null && currentTextBox.Enabled)
				{
					currentRowIndex = ConfigurationGrid.CurrentCell.RowNumber;
					selectionStart = currentTextBox.SelectionStart;
					selectionLength = currentTextBox.SelectionLength;
					currentText = currentTextBox.Text;
					entryPointIsKnown = true;
				}
				else
				{
					entryPointIsKnown = false;
				}
			}
		}

		void ShowMapTreePresenter(object sender, EventArgs e)
		{
			if (entryPointIsKnown)
			{
				using (var mapTreePresenter = ObjectFactory.Get<IMapTreePresentationManager>())
				using (ZFormModaliser.SetTemporaryDelegateToCallBeforeShowingFormsOrDialogs(OnMapTreeFormShown))
				{
					mapTreePresenter.ParentTypes = TypesForMapPresenter;
					var result = mapTreePresenter.GetUserSelectionMacro();

					if (!string.IsNullOrEmpty(result))
					{
						var replacingString = currentText;
						replacingString = replacingString.Remove(selectionStart, selectionLength);
						replacingString = replacingString.Insert(selectionStart, result);

						var list = (KoreaSouthEInvoicingDataElementConfigurationCollection)DataSource;
						list[currentRowIndex].Configuration = replacingString;
						entryPointIsKnown = false;
					}
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("920ca5f3-adbc-48dd-8a67-417b510fdbde", "To insert a macro, please place the cursor inside the cell in the grid."));
			}
		}

		void OnMapTreeFormShown(object form)
		{
			if (form is MapTreeForm mapTreeForm)
			{
				mapTreeForm.ShowEditor = false;
				mapTreeForm.macrosTab.TabVisible = false;
			}
		}

		Type[] TypesForMapPresenter => new[] { typeof(KoreaSouthEInvoicingDataElementProvider) };

		int currentRowIndex;
		int selectionStart;
		int selectionLength;
		string currentText;
		bool entryPointIsKnown;
	}
}
