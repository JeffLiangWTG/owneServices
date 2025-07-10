using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	partial class BillOfLadingNumberCustomisationControl : RegistryBusinessObjectTemplateZUserControl
	{
		public BillOfLadingNumberCustomisationControl()
		{
			InitializeComponent();
		}

		public BillOfLadingNumberCustomisationControl(BillCustomisationRegistryDataType dataType)
		{
			InitializeComponent();
			Initialize(dataType);
		}

		#region Initialize

		internal void Initialize(IBillCustomisationRegistryDataType dataType)
		{
			codingOfHouseBillNumberGroupBox.Text = Res.GetString("facde58c-8ac5-4d00-8ab4-8071dc4f86af", "Coding of {0}", dataType.GeneratedNumberName);
			maxLengthDividerLabel.Text = Res.GetString("3f135de8-1964-4271-b000-643e5c3bfd59", "/{0}", dataType.MaxLength);

			SetupEnableMacroInsertion(dataType.EnableMacroInsertion, dataType.MacroType);
			SetupParentOptionPanel(dataType);
		}

		void SetupEnableMacroInsertion(bool enableMacroInsertion, Type macroType)
		{
			insertFieldButton.Visible = enableMacroInsertion;

			if (enableMacroInsertion)
			{
				Argument.NotNull(macroType, nameof(macroType));

				ControlDpiScalingHelper.SetHeight(codingOfHouseBillNumberGroupBox, codingOfHouseBillNumberGroupBox.Height + insertFieldButton.Height, false);
				elementsGrid.AllowSorting = false;
				insertFieldButton.Click += (s, e) => ShowMapTreePresenter(macroType);
				elementsGrid.CurrentCellChanged += ElementsGrid_CurrentCellChanged;
				detailColumnStyleInfo.CharacterCasing = CharacterCasing.Normal;
			}
			else
			{
				detailColumnStyleInfo.CharacterCasing = CharacterCasing.Upper;
			}
		}

		void SetupParentOptionPanel(IBillCustomisationRegistryDataType dataType)
		{
			if (string.IsNullOrEmpty(dataType.FountainPrefix))
			{
				parentOptionPanel.Visible = false;
				ControlDpiScalingHelper.SetHeight(codingOfHouseBillNumberGroupBox, codingOfHouseBillNumberGroupBox.Height - parentOptionPanel.Height, false);
			}
			else
			{
				removeFountainPrefixCheckBox.Text = Res.GetString("1a9e93a1-6abf-4c1a-a1f5-33c1fd987c0b", "Remove the '{0}' Prefix", dataType.FountainPrefix);
				useShipmentSequenceNumberCheckBox.Text = Res.GetString("{ED936D8B-7342-4C81-A297-FBF5E624E552}}", "Use {0} Sequence Number", dataType.SequenceNumberName);

				HideUseShipmentCheckBoxIfRequired(dataType);
				HideAllocateMasterBillNumberCheckBoxIfRequired(dataType);
			}
		}

		void HideUseShipmentCheckBoxIfRequired(IBillCustomisationRegistryDataType dataType)
		{
			if (dataType.Categories.HasFlag(NumberCustomisationElementCategories.SupplierBooking) ||
				dataType.Categories.HasFlag(NumberCustomisationElementCategories.ClientContract) ||
				dataType.Categories.HasFlag(NumberCustomisationElementCategories.WarehouseJob) ||
				dataType.Categories.HasFlag(NumberCustomisationElementCategories.Domestic) ||
				dataType.Categories.HasFlag(NumberCustomisationElementCategories.OceanCarrier))
			{
				useShipmentSequenceNumberCheckBox.Visible = false;
				removeFountainPrefixCheckBox.Location = useShipmentSequenceNumberCheckBox.Location;
			}
		}

		void HideAllocateMasterBillNumberCheckBoxIfRequired(IBillCustomisationRegistryDataType dataType)
		{
			if (!dataType.Categories.HasFlag(NumberCustomisationElementCategories.Consol))
			{
				autoAllocateMasterBillNumbersToConsolsCheckBox.Visible = false;
			}
		}

		#endregion

		void ElementsGrid_CurrentCellChanged(object sender, EventArgs e)
		{
			insertFieldButton.Enabled = CurrentCellCanAcceptMacro();
		}

		bool CurrentCellCanAcceptMacro()
		{
			var currentColumnName = elementsGrid.Columns[elementsGrid.CurrentCell.ColumnNumber].ColumnName;
			var elementName = GetCurrentCellElementName();
			var cellCanHaveMacro = elementName.Contains((NoResString)"Custom") && currentColumnName.Equals(detailColumnStyleInfo.ColumnName);
			return cellCanHaveMacro;
		}

		ZString GetCurrentCellElementName()
		{
			var currentElementColumnValue =
				elementsGrid[
					elementsGrid.CurrentCell.RowNumber,
					elementsGrid.Columns.IndexOf(column => column.ColumnName == elementNameColumnStyleInfo.ColumnName)];
			var elementName = new ZString(currentElementColumnValue);
			return elementName;
		}

		void ShowMapTreePresenter(Type macroType)
		{
			if (CurrentCellCanAcceptMacro())
			{
				using (var mapTreePresenter = ObjectFactory.Get<IMapTreePresentationManager>())
				{
					mapTreePresenter.ParentTypes = new Type[] { macroType };
					var result = mapTreePresenter.GetUserSelectionMacro();
					if (!string.IsNullOrEmpty(result))
					{
						elementsGrid[elementsGrid.CurrentCell.RowNumber, elementsGrid.CurrentCell.ColumnNumber] = new ZString(result);
					}
				}
			}
		}
	}
}
