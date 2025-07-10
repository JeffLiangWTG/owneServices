using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class JobInvoiceDescriptionControl : RegistryZUserControl
	{
		public JobInvoiceDescriptionControl()
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
			if (ConfigurationGrid.ContainsFocus && ConfigurationGrid.Enabled && ConfigurationGrid.Columns.Count == 4)
			{
				DataGridTextBox currentTextBox = ((ZTextBoxColumnStyle)ConfigurationGrid.Columns[ConfigurationGrid.CurrentCell.ColumnNumber].ColumnStyle).EditControl as DataGridTextBox;
				if (currentTextBox != null)
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

		int currentRowIndex;
		int selectionStart;
		int selectionLength;
		string currentText;
		bool entryPointIsKnown;

		void ShowMapTreePresenter(object sender, EventArgs e)
		{
			if (entryPointIsKnown)
			{
				using (var mapTreePresenter = ObjectFactory.Get<IMapTreePresentationManager>())
				{
					var list = (JobInvoiceDescriptionCollection)DataSource;
					string jobType = "";
					if (currentRowIndex < list.Count && list[currentRowIndex] != null)
					{
						jobType = list[currentRowIndex].JobType;
					}
					mapTreePresenter.ParentTypes = GetTypesForMapPresenter(jobType);
					string result = mapTreePresenter.GetUserSelectionMacro();
					if (!string.IsNullOrEmpty(result))
					{
						string replacingString = currentText;
						replacingString = replacingString.Remove(selectionStart, selectionLength);
						replacingString = replacingString.Insert(selectionStart, result);

						if (list.Count == currentRowIndex)
						{
							list.AddNew();
						}

						if (list.Count > currentRowIndex)
						{
							list[currentRowIndex].InvoiceDescription = replacingString;
						}

						entryPointIsKnown = false;
					}
				}
			}
			else
			{
				Globals.Message.Show(Enterprise.Accounting.GUI.Res.GetString("920ca5f3-adbc-48dd-8a67-417b510fdbde", "To insert a macro, please place the cursor inside the cell in the grid."));
			}
		}

		internal Type[] GetTypesForMapPresenter(string jobType)
		{
			var result = new List<Type>();

			var consumerTypes = JobInvoicingConsumerTypes.NewOnlyJobInvoicingTypes();
			if (consumerTypes.ContainsCode(jobType))
			{
				var wrapperCreator = ObjectFactory.Get<IDocFreightWrapperCreator>();
				var docWrapperType = wrapperCreator.GetFreightWrapperType(consumerTypes[jobType].BizoType);

				if (docWrapperType != null)
				{
					result.Add(docWrapperType);
				}
				else
				{
					result.Add(consumerTypes[jobType].BizoType);
				}
			}
			else
			{
				result.Add(typeof(JobHeader));
			}

			return result.ToArray();
		}
	}
}

