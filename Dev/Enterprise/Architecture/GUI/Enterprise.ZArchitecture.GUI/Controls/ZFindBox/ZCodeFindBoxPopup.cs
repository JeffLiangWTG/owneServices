using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	[SuppressFormDesignerAnalysis]
	public partial class ZCodeFindBoxPopup : ZChildForm, IFindBoxPopup
	{
		public ZCodeFindBoxPopup()
			: this("")
		{
		}

		public ZCodeFindBoxPopup(string popupCaption)
		{
			Text = popupCaption;
		}

		#region IFindBoxPopup Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "used in exception message for developers only")]
		public virtual void ShowModal(IFindBox findBox, Form parentForm)
		{
			this.FindBox = findBox;

			InitialiseListAndGrid(findBox);
			PrepareControls(findBox);
			LoadList();

			ZFormModaliser.Show(this, parentForm);

			if (Grid.TableStyles.Count == 0 ||
				Grid.TableStyles[0].GridColumnStyles[0].PropertyDescriptor == null ||
				(Grid.TableStyles[0].GridColumnStyles.Count > 1 && Grid.TableStyles[0].GridColumnStyles[1].PropertyDescriptor == null))
			{
				var columnStylesCount = 0;
				var columnStylesWithNullPropertyDescriptor = "";
				if (Grid.TableStyles.Count != 0)
				{
					columnStylesCount = Grid.TableStyles[0].GridColumnStyles.Count;
					foreach (DataGridColumnStyle style in Grid.TableStyles[0].GridColumnStyles)
					{
						if (style.PropertyDescriptor == null)
						{
							columnStylesWithNullPropertyDescriptor += "\r\nName: " + style.MappingName + ", Style: " + style.ToString();
						}
					}
				}
				var errorReport = @"Error: Cannot show popup due to internal exception. 
Table Styles.Count: {0}
Column Styles.Count: {1}
Columns With Null Property Descriptor: {2}
Parent Type: {3}
Parent Caption: {4}
FindBox Type: {5}
FindBox.ListProvider Type: {6}
FindBox.ListProvider.List Type: {7}
FindBox.ListProvider.List TypeOfElements: {8}";
				ErrorReporter.ReportOnce(string.Format(errorReport, Grid.TableStyles.Count, columnStylesCount, columnStylesWithNullPropertyDescriptor, parentForm.GetType().ToString(),
																														parentForm.Text, findBox.GetType().ToString(), findBox.ListProvider.GetType().ToString(),
																														findBox.ListProvider.List?.GetType().ToString(), findBox.ListProvider.List?.TypeOfElements?.GetType().ToString()));
				Globals.Message.ShowError(Res.GetString("37b33bb3-893e-482b-a83f-6825b8256218", "Popup cannot be shown due to internal error"));
				this.Close();
			}
		}

		public virtual SilentSelectResult SelectFromPopupWithoutDisplaying(IFindBox findBox, EmbeddedModulePopup popup)
		{
			return SilentSelectResult.None;
		}

		void IFindBoxPopup.SelectRowByPK(ZGuid pK)
		{
		}

		#endregion

		#region Implementation

		protected IBusinessObjectCollection List;
		protected IFindBox FindBox;

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (keyData == (Keys.Control | Keys.Enter))
			{
				var frontMostActiveControl = ActiveControl.GetFrontMostActiveControl();
				ActiveControl = FindBtn;
				FindBtn.PerformClick();

				if (Grid.List.Count > 0)
				{
					ActiveControl = Grid;
					Grid.Select(0);
				}
				else
				{
					ActiveControl = frontMostActiveControl;
				}

				return true;
			}
			else
			{
				return base.ProcessDialogKey(keyData);
			}
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == Keys.Escape)
			{
				Close();
				return true;
			}
			else
			{
				return base.ProcessCmdKey(ref msg, keyData);
			}
		}

		#region Event Handlers

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			Grid.SetScrollBarsInvisible();
			if (List != null && List.Count > 0)
			{
				Grid.Select(0);
			}
		}

		void Grid_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left && e.Clicks == 2)
			{
				var hitInfo = ((DataGrid)sender).HitTest(e.X, e.Y);
				if (hitInfo.Type == DataGrid.HitTestType.Cell || hitInfo.Type == DataGrid.HitTestType.RowHeader)
				{
					AcceptSelection();
				}
			}
		}

		void Grid_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.Enter)
			{
				AcceptSelection();
				e.Handled = true;
			}
		}

		void FindBtn_Click(object sender, EventArgs e)
		{
			LoadList();
		}

		void OKBtn_Click(object sender, EventArgs e)
		{
			AcceptSelection();
		}

		void CancelBtn_Click(object sender, EventArgs e)
		{
			RejectSelection();
		}

		#endregion

		protected void InitialiseListAndGrid(IFindBox findBox)
		{
			if (findBox.ListProvider.List != List)
			{
				List = findBox.ListProvider.List;

				SetupGridColumns();

				Grid.SetDataBinding(null, "");
				Grid.SetDataBinding(List, "");
			}
		}

		void SetupGridColumns()
		{
			try
			{
				var codePropertyName = CodePropertyAttribute.CodePropertyNameFromType(List.TypeOfElements);
				var descriptionPropertyName = DescriptionPropertyAttribute.DescriptionPropertyNameFromType(List.TypeOfElements);
				if (!string.IsNullOrEmpty(codePropertyName))
				{
					CodeColumnStyle.ColumnName = codePropertyName;
				}

				if (!string.IsNullOrEmpty(descriptionPropertyName))
				{
					DescriptionColumnStyle.ColumnName = descriptionPropertyName;
				}

				if (codePropertyName == descriptionPropertyName)
				{
					if (Grid.ColumnStyles.Count > 1)
					{
						Grid.ColumnStyles.RemoveAt(1);
					}

					DescriptionTextBox.Visible = false;
				}
			}
			catch (NoCodePropertyException)
			{
				return;
			}
		}

		protected void PrepareControls(IFindBox findBox)
		{
			CodeTextBox.Text = "";
			DescriptionTextBox.Text = "";
		}

		public bool HandleGridTab(Keys keyData)
		{
			return ProcessDialogKey(keyData);
		}

		#region Code & Description Schema Columns

		SchemaColumn CodeSchemaColumn
		{
			get
			{
				if (fCodeSchemaColumn == null)
				{
					var codePropertyName = CodePropertyAttribute.CodePropertyNameFromType(List.TypeOfElements);
					var tableName = BusinessObjectFactory.GetTableNameFromType(List.TypeOfElements);

					fCodeSchemaColumn = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(codePropertyName, tableName);
				}

				return fCodeSchemaColumn;
			}
		}

		SchemaColumn DescriptionSchemaColumn
		{
			get
			{
				if (fDescriptionSchemaColumn == null)
				{
					var descriptionPropertyName = DescriptionPropertyAttribute.DescriptionPropertyNameFromType(List.TypeOfElements);
					var tableName = BusinessObjectFactory.GetTableNameFromType(List.TypeOfElements);

					fDescriptionSchemaColumn = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(descriptionPropertyName, tableName);
				}

				return fDescriptionSchemaColumn;
			}
		}

		SchemaColumn fCodeSchemaColumn;
		SchemaColumn fDescriptionSchemaColumn;

		#endregion

		protected virtual void LoadList()
		{
			if (List != null)
			{
				var query = new ZQuery();
				if (CodeSchemaColumn != null)
				{
					query.AddToFilter(CodeSchemaColumn, SQLComparisonOperator.StartsWith, CodeTextBox.Text.Trim());
				}

				if (DescriptionSchemaColumn != null)
				{
					query.AddToFilter(DescriptionSchemaColumn, SQLComparisonOperator.Contains, DescriptionTextBox.Text.Trim());
				}

				query.MaximumRows = 100;
				if (List is BusinessObjectCollection)
				{
					if (CodeSchemaColumn != null)
					{
						query.OrderBy = CodeSchemaColumn.Name;
					}
				}
				else
				{
					List.ApplySort(new SortInfo(CodeSchemaColumn.Name, System.ComponentModel.ListSortDirection.Ascending));
				}

				try
				{
					Grid.SuspendLayout();
					if (Grid.ListManager.Bindings.Count > 0)
					{
						Grid.ListManager.SuspendBinding();
					}
					LoadCollection(List, query);
				}
				finally
				{
					Grid.ListManager.Refresh();
					Grid.ListManager.ResumeBinding();
					Grid.ResumeLayout();
				}

				if (List.Count > 0)
				{
					Grid.Select(0);
				}
			}
		}

		void LoadCollection(IBusinessObjectCollection collection, ZQuery additionalFilter)
		{
			var legacyCollection = collection as BusinessObjectCollection;
			if (legacyCollection != null)
			{
				var completeFilter = new ZQuery();
				var legacyCollectionAdditionalFilter = ((ILegacyBusinessObjectCollectionInternals)legacyCollection).AdditionalFilter;
				if (legacyCollectionAdditionalFilter != null)
				{
					completeFilter.AddToFilter(legacyCollectionAdditionalFilter);
				}

				completeFilter.AddToFilter(additionalFilter);
				completeFilter.MaximumRows = additionalFilter.MaximumRows;

				legacyCollection.Load(completeFilter);
			}
			var activeCollection = collection as IActiveBusinessObjectCollection;
			if (activeCollection != null)
			{
				activeCollection.AdditionalFilter = additionalFilter;
			}
		}

		protected void AcceptSelection()
		{
			ICodeDescription selectedElement = null;

			if (Grid.ListManager == null)
			{
				return;
			}

			if (Grid.ListManager.Position > -1)
			{
				selectedElement = Grid.ListManager.GetCurrent() as ICodeDescription;
				ActiveControl = CodeTextBox;
			}

			DialogResult = DialogResult.OK;

			if (selectedElement != null)
			{
				FindBox.Code = selectedElement.Code;
				FindBox.Description = selectedElement.Description.Trim();
			}

			Close();
		}

		void RejectSelection()
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		#endregion
	}
}

