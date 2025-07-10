using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public class AWBExtraTextControl : RegistryZUserControl, IVerticalPlacementClient
	{
		public AWBExtraTextControl(ICodeDescriptionListControl codeDescriptionListControl)
		{
			Argument.NotNull(codeDescriptionListControl, "codeDescriptionListControl");

			this.codeDescriptionListControl = codeDescriptionListControl;
			this.verticalPlacementHelper = new VerticalPlacementHelper(this);

			Initialize();
		}

		readonly ICodeDescriptionListControl codeDescriptionListControl;
		readonly VerticalPlacementHelper verticalPlacementHelper;
		ZButton viewButton;

		public ReadOnlyCodeDescriptionPairList Data
		{
			get { return new ReadOnlyCodeDescriptionPairList((byte[])codeDescriptionListControl.Data); }
			set { codeDescriptionListControl.Data = value.ToXMLByteArray(); }
		}

		void Initialize()
		{
			Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 300);
			codeDescriptionListControl.Control.Dock = DockStyle.Top;
			Controls.Add(this.codeDescriptionListControl.Control);

			viewButton = new ZButton();
			viewButton.Name = "viewButton";
			viewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 23);
			viewButton.Click += (s, e) => ShowMapTreePresenter();
			viewButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AWBExtraTextControl|14b6d44a-5e1d-489c-be83-388144f3954e", "Insert Field", "Displays a list of all available fields that can be included on the Air Waybill.\r\nYou can choose a field and it will automatically be inserted into the registry item value.");
			Controls.Add(viewButton);
			viewButton.Enabled = !codeDescriptionListControl.ReadOnly;

			Resize += (s, e) => verticalPlacementHelper.AdjustVerticalPlacement();

			codeDescriptionListControl.Grid.CurrentCellChanged += (s, e) => verticalPlacementHelper.AdjustVerticalPlacement();
			codeDescriptionListControl.Grid.AllowSorting = false;
			codeDescriptionListControl.Grid.CursorChanged += (s, e) => RememberCursorPosition();
			codeDescriptionListControl.Grid.Enter += (s, e) => RememberCursorPosition();

			verticalPlacementHelper.AdjustVerticalPlacement();

			CaptionRenderingEnabled = true;
		}

		int currentRowIndex;
		bool codeColumnWasFocused;
		int selectionStart;
		int selectionLength;
		string currentText;
		bool entryPointIsKnown;

		void RememberCursorPosition()
		{
			if (codeDescriptionListControl.Grid.ContainsFocus && codeDescriptionListControl.Grid.Enabled && codeDescriptionListControl.Grid.Columns.Count == 2)
			{
				DataGridTextBox currentTextBox = (DataGridTextBox)((ZTextBoxColumnStyle)codeDescriptionListControl.Grid.Columns[codeDescriptionListControl.Grid.CurrentCell.ColumnNumber].ColumnStyle).EditControl;
				if (currentTextBox != null)
				{
					currentRowIndex = codeDescriptionListControl.Grid.CurrentCell.RowNumber;
					codeColumnWasFocused = codeDescriptionListControl.Grid.Columns[codeDescriptionListControl.Grid.CurrentCell.ColumnNumber].ColumnName == "Code";
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

		void ShowMapTreePresenter()
		{
			if (entryPointIsKnown)
			{
				using (var mapTreePresenter = ObjectFactory.Get<IMapTreePresentationManager>())
				{
					mapTreePresenter.ParentTypes = new Type[] { ObjectFactory.GetType<DocumentWrappers.IDocAWB>() };
					string result = mapTreePresenter.GetUserSelectionMacro();
					if (!string.IsNullOrEmpty(result))
					{
						string replacingString = currentText;
						replacingString = replacingString.Remove(selectionStart, selectionLength);
						replacingString = replacingString.Insert(selectionStart, result);

						CodeDescriptionPairList list = new CodeDescriptionPairList(this.Data);

						if (list.Count == currentRowIndex)
						{
							list.AddPair(string.Empty, string.Empty);
						}

						if (list.Count > currentRowIndex)
						{
							list[currentRowIndex] = codeColumnWasFocused
														? new CodeDescriptionPair(replacingString, list[currentRowIndex].Description)
														: new CodeDescriptionPair(list[currentRowIndex].Code, replacingString);
						}

						this.Data = list;
						entryPointIsKnown = false;
					}
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("259e5e84-1877-41f3-b7d5-1e2c13a514dd", "To insert a macro, please place the cursor inside the cell in the grid."));
			}
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			codeDescriptionListControl.ReadOnly = readOnly;
			viewButton.ReadOnly = readOnly;
		}

		#region IVerticalPlacementClient Members

		ZGrid[] IVerticalPlacementClient.Grids
		{
			get { return new ZGrid[] { codeDescriptionListControl.Grid }; }
		}

		int IVerticalPlacementClient.GridMaxHeight
		{
			get { return Height - viewButton.Height - 4 * ControlDpiScalingHelper.ScaleToCurrentDpiY(spacing); }
		}

		void IVerticalPlacementClient.SetGridHeight(int height)
		{
			ControlDpiScalingHelper.SetHeight(codeDescriptionListControl.Control, height, false);
		}

		void IVerticalPlacementClient.SetAdditionalControlsPosition(int top)
		{
			ControlDpiScalingHelper.SetTop(ref viewButton, top, false);
		}

		const int spacing = 6;

		#endregion
	}
}
