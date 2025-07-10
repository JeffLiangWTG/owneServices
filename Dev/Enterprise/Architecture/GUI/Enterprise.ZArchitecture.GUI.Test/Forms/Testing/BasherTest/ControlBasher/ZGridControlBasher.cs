using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Controls;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class ZGridControlBasher : IControlBasher
	{
		public void Bash(Control control, INotifications notifications)
		{
			BashGrid((ZGrid)control, notifications);
		}

		#region BashGrid

		static readonly int GridColumnsThatHaveBeenBashedKey = ControlExtensions.CreateUserDataKey();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Can be used in debugging when call from BashGrid() is uncommented")]
		void CheckIfGridBindsToInterface(ZGrid grid, INotifications notifications)
		{
			if (IsGridBoundToInterfaceWithoutSupportGridColour())
			{
				notifications.AddError(ControlDescription.GetControlPath(grid) + " **** Interface without SupportGridColour has been bound to this grid, grid colour might be disabled");
				notifications.AddError("grid.ElementType = " + grid.ElementType?.Name + " | grid.ElementTypeFromCollection = " + grid.ElementTypeFromCollection?.Name);
			}

			bool IsGridBoundToInterfaceWithoutSupportGridColour()
			{
				if (grid.ElementType != null)
				{
					return grid.ElementType.IsInterface && !Attribute.IsDefined(grid.ElementType, typeof(SupportGridColourAttribute));
				}

				if (grid.ElementTypeFromCollection != null)
				{
					return grid.ElementTypeFromCollection.IsInterface && !Attribute.IsDefined(grid.ElementTypeFromCollection, typeof(SupportGridColourAttribute));
				}

				return false;
			}
		}

		void BashGrid(ZGrid grid, INotifications notifications)
		{
			var bashedGridColumns = GetBashedGridColumns(grid);
			ExposeAllColumns(grid, notifications, true);

			// Enable this to check for violating grids and interface
			// CheckIfGridBindsToInterface(grid, notifications);

			var gridHasEditableRows = (grid.List != null) && (((grid.List.Count > 0) || grid.List.AllowNew));
			if (!grid.ReadOnly && gridHasEditableRows)
			{
				for (var i = 0; i < 2; i++)
				{
					for (var j = 0; j < grid.TableStyles[0].GridColumnStyles.Count; j++)
					{
						var doEvents = ShouldDoEventsForEverySendKeyToGridColumn(grid, j);
						var mappingName = grid.TableStyles[0].GridColumnStyles[grid.CurrentCell.ColumnNumber].MappingName;
						if (!bashedGridColumns.ContainsKey(mappingName))
						{
							var info = grid.GetColumnStyle(mappingName);
							TestKeyStrokeHelper.SendKeyToControl(grid, Keys.D1, doEvents);
							if (i == 1)
							{
								bashedGridColumns.Add(mappingName, null);
							}
						}

						TestKeyStrokeHelper.SendKeyToControl(grid, Keys.Right, doEvents);
					}

					Application.DoEvents();
					TestKeyStrokeHelper.SendKeyToControl(grid, Keys.Down);
				}
			}

			VerifyGridColumns(grid, notifications);
			TestAllGridColumnsCanBeExportedToExcel(grid, notifications);

			if (grid.ShowImportDataMenuItem && string.IsNullOrEmpty(grid.GridId))
			{
				notifications.AddError(ControlDescription.GetControlPath(grid) + " has no GridID but wants to support DataImportWizard. Please disable data wizard by setting DisableImportDataMenuItem=true or provide a GridID. The legacy key for this grid is " + grid.DataImportWizardKey);
			}
		}

		static bool ShouldDoEventsForEverySendKeyToGridColumn(ZGrid grid, int column)
		{
			var result = false;
			var checkBoxColumn = grid.TableStyles[0].GridColumnStyles[column] as ZCheckBoxColumnStyle;
			if (checkBoxColumn != null)
			{
				result = checkBoxColumn.Checked;
			}

			return result;
		}

		Dictionary<string, object> GetBashedGridColumns(ZGrid grid)
		{
			var mappingNames = (Dictionary<string, object>)grid.GetUserData(GridColumnsThatHaveBeenBashedKey);
			if (mappingNames == null)
			{
				mappingNames = new Dictionary<string, object>();
				grid.SetUserData(GridColumnsThatHaveBeenBashedKey, mappingNames);
			}
			return mappingNames;
		}

		void VerifyGridColumns(ZGrid grid, INotifications notifications)
		{
			foreach (ZGridColumnInfo columnInfo in grid.ColumnStyles)
			{
				VerifyColumn(grid, columnInfo, notifications);
			}
		}

		#endregion

		#region VerifyColumn

		void VerifyColumn(ZGrid grid, ZGridColumnInfo columnInfo, INotifications notifications)
		{
			var findBoxColumn = columnInfo as ZCodeFindBoxColumnStyleInfo;
			if (findBoxColumn != null)
			{
				VerifyFindBoxColumn(grid, findBoxColumn, notifications);
			}

			var dropEditColumn = columnInfo as ZDropEditColumnStyleInfo;
			if (dropEditColumn != null)
			{
				VerifyDropEditList(grid, dropEditColumn, notifications);
			}

			if (grid.ListManager != null && grid.ListManager.Position != -1 && !(columnInfo is ZTranslatableTextBoxColumnStyleInfo))
			{
				var dataProperty = grid.ListManager.GetItemProperties()[columnInfo.ColumnName];
				if (dataProperty != null)
				{
					var translatableFieldAttribute = dataProperty.GetAttributeFromMostSpecificComponentType(typeof(TranslatableDataFieldAttribute)) ?? dataProperty.GetAttributeFromMostSpecificComponentType(typeof(LinkedTranslatableDataFieldAttribute));
					if (translatableFieldAttribute != null)
					{
						notifications.AddError(ControlDescription.GetControlPath(grid) + " - " + columnInfo.ColumnName + ": Use " + columnInfo.ColumnName + "Multilingual instead");
					}
				}
			}
		}

		void VerifyFindBoxColumn(ZGrid grid, ZCodeFindBoxColumnStyleInfo info, INotifications notifications)
		{
			if (EnsureListManagerWithAtLeast1Element(grid) && grid.ListManager.Position >= 0)
			{
				var property = grid.ListManager.GetItemProperties()[info.ColumnName] ?? ((IOverridablePropertyDescriptor)info).PropertyDescriptor;
				if (property != null && !property.IsReadOnly)
				{
					var moduleIdCheckExcluded =
						SuppressCheckControlModuleIdAttribute.IsApplied(grid) ||
						SuppressCheckControlModuleIdAttribute.IsApplied(info) ||
						TypeDescriptor.GetAttributes(info)[typeof(SuppressCheckControlModuleIdAttribute)] != null;
					var lookupListCheckExcluded =
						SuppressCheckControlLookupListAttribute.IsApplied(grid) ||
						SuppressCheckControlLookupListAttribute.IsApplied(info) ||
						TypeDescriptor.GetAttributes(info)[typeof(SuppressCheckControlLookupListAttribute)] != null;

					if (!moduleIdCheckExcluded || !lookupListCheckExcluded)
					{
						var listMember = ListMemberVerification.VerifyLookupList(grid, grid.ListManager, info.ColumnName, info.BindToList.Replace(".", "+"), notifications);
						var list = listMember == null ? null : (IList)listMember.GetValue(grid.ListManager.GetCurrent());
						if (!moduleIdCheckExcluded &&
							list != null &&
							info.ModuleID == ModuleIDs.NotAssigned &&
							ZMetaData.GetModuleId(list) == ModuleIDs.NotAssigned)
						{
							notifications.AddError(ControlDescription.GetControlPath(grid) + " - " + info.ColumnName + ": a ModuleID was not specified on the lookup collection class or on the control.");
						}
					}
				}
			}
		}

		void VerifyDropEditList(ZGrid grid, ZDropEditColumnStyleInfo dropEditColumn, INotifications notifications)
		{
			if (EnsureListManagerWithAtLeast1Element(grid))
			{
				var dataProperty = grid.ListManager.GetItemProperties()[dropEditColumn.ColumnName];
				if (dataProperty == null || !dataProperty.IsReadOnly)
				{
					var lookupListCheckExcluded =
						SuppressCheckControlLookupListAttribute.IsApplied(grid) ||
						TypeDescriptor.GetAttributes(dropEditColumn)[typeof(SuppressCheckControlLookupListAttribute)] != null;

					if (!lookupListCheckExcluded)
					{
						ListMemberVerification.VerifyLookupList(grid, grid.ListManager, dropEditColumn.ColumnName, dropEditColumn.BindToList.Replace(".", "+"), notifications);
					}
				}
			}
		}

		static bool EnsureListManagerWithAtLeast1Element(ZGrid grid)
		{
			var result = false;
			if (grid.ListManager != null)
			{
				result = true;
				var bindingList = grid.ListManager.List as IBindingList;
				if (grid.ListManager.Position == -1 &&
					bindingList != null &&
					bindingList.AllowNew)
				{
					try
					{
						bindingList.AddNew();
					}
					catch (NoConcreteTypeException)
					{
					}
				}

				if (grid.ListManager.Position == -1)
				{
					result = false;
				}
			}
			return result;
		}

		#endregion

		#region ExposeAllColumns / ExposeAllColumnsInAllGrids

		internal static void ExposeAllColumns(ZGrid grid, INotifications notifications)
		{
			ExposeAllColumns(grid, notifications, false);
		}

		internal static void ExposeAllColumns(ZGrid grid, INotifications notifications, bool checkColumnWidths)
		{
			if (grid != null)
			{
				grid.ExposeAllColumns();
				Application.DoEvents();

				if (grid.TableStyles != null && grid.TableStyles.Count > 0)
				{
					var columnsWithDifferentKeys = new List<string>();
					var columns = new Dictionary<string, string>();

					foreach (ZGridColumnInfo columnInfo in grid.ColumnStyles)
					{
						var columnStyle = grid.TableStyles[0].GridColumnStyles[columnInfo.ColumnName];

						if (columnStyle != null && columnStyle.PropertyDescriptor == null)
						{
							notifications.AddError(ControlDescription.GetControlPath(grid) + " - Column with Caption='" + columnInfo.Caption + "' ColumnName='" + columnInfo.ColumnName + "' does not have a valid ColumnName. If ColumnName is indeed defined, check that your grid is properly bound (e.g. the binding collection is not null) in your test.");
						}

						if (columnInfo.GroupName != null && columnInfo.GroupName.Caption != null)
						{
							if (!columns.ContainsKey(columnInfo.GroupName.Caption))
							{
								columns.Add(columnInfo.GroupName.Caption, columnInfo.GroupName.Key);
							}
							else if (columnInfo.GroupName.Key != columns[columnInfo.GroupName.Caption])
							{
								columnsWithDifferentKeys.Add(columnInfo.GroupName.Caption);
							}
						}

						if (columnInfo.GroupName != null && !string.IsNullOrEmpty(columnInfo.GroupName.Key) && string.IsNullOrEmpty(columnInfo.GroupName.Caption))
						{
							notifications.AddError(ControlDescription.GetControlPath(grid) + " - Group with Key = '" + columnInfo.GroupName.Key + "' Has a null or empty string set as Caption. Check that all GroupNames have valid Captions");
						}

						var zGridColumnStyle = columnStyle as ZGridColumnStyle;

						/*
						if (!TestingState.IsRunningOnDAT &&
							checkColumnWidths &&
							zGridColumnStyle != null &&
							TextRenderer.MeasureText(zGridColumnStyle.HeaderText, zGridColumnStyle.HeaderFont).Width + 4 > columnStyle.Width)
						{
							int proposedWidth = TextRenderer.MeasureText(zGridColumnStyle.HeaderText, zGridColumnStyle.HeaderFont).Width + 4;
							string message = string.Format("   >>> <B>DEVELOPER ONLY CHECK : Column header text is too long (or column is too small)</B> <<<\r\n{0}  Column '{1}' with Caption='{2}' and Width={3}.  Proposed width for this caption={4}",
								ControlDescription.GetControlPath(grid), columnInfo.ColumnName, zGridColumnStyle.HeaderText, columnStyle.Width, proposedWidth);
							notifications.AddError(message);
						}
						*/

						if (columnStyle is ZTextBoxColumnStyle textStyle && textStyle.ColumnInfo.PasswordChar != '\0')
						{
							BusinessObject bizo = null;
							try
							{
								bizo = new BusinessObjectFactory().GetNull(grid.ElementTypeFromCollection);
							}
							catch (ZException)
							{
								bizo = (BusinessObject)grid.ElementTypeFromCollection.GetConstructor(BindingFlags.Public | BindingFlags.Instance, Type.DefaultBinder, Array.Empty<Type>(), null).Invoke(Array.Empty<object>());
								if (bizo == null)
								{
									bizo = (BusinessObject)grid.ElementTypeFromCollection.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, Type.DefaultBinder, Array.Empty<Type>(), null).Invoke(Array.Empty<object>());
								}
							}
							if (!MetaData.GetPassword(bizo, columnStyle.PropertyDescriptor))
							{
								notifications.AddError(ControlDescription.GetControlPath(grid) + " - Column with Caption='" + columnInfo.Caption + "' ColumnName='" + columnInfo.ColumnName + "' has PasswordChar set, but the corresponding property isn't annotated [Password].");
							}
						}

						MissingResourceStringChecker.Check(zGridColumnStyle, grid, notifications);
					}

					foreach (var caption in columnsWithDifferentKeys.Distinct())
					{
						notifications.AddError(ControlDescription.GetControlPath(grid) + " - Group with Caption = '" + caption + "' Has multiple, different, Keys. Check that all GroupNames on the same grid with the same Caption have the same Key.");
					}
				}
			}
		}

		public static void ExposeAllColumnsInAllGrids(Control control, INotifications notifications)
		{
			foreach (Control next in control.Controls)
			{
				var grid = next as ZGrid;
				if (grid != null)
				{
					ExposeAllColumns(grid, notifications);
				}
				ExposeAllColumnsInAllGrids(next, notifications);
			}
		}

		internal static void TestAllGridColumnsCanBeExportedToExcel(ZGrid grid, INotifications notifications)
		{
			if (grid != null && grid.ShowExportToExcelMenuItem)
			{
				Application.DoEvents();
				var type = grid.ElementTypeFromCollection;

				foreach (var column in grid.Columns)
				{
					if (!(column.ColumnStyle.PropertyDescriptor.PropertyType.IsInterface && column.ColumnStyle.PropertyDescriptor.PropertyType == typeof(IZType)))
					{
						var zInterface = column.ColumnStyle.PropertyDescriptor.PropertyType.GetInterface(nameof(IZType));
						if (zInterface == null)
						{
							var message = string.Format(ControlDescription.GetControlPath(grid) +
								" - The property '{0}' of data source bound to ZGrid  should implement IZType interface for exporting to excel.", column.ColumnName);
							notifications.AddError(message);
						}
						else
						{
							var descriptorName = column.ColumnStyle.PropertyDescriptor.Name;
							if (column.ColumnName != descriptorName && column.ColumnName != descriptorName + ZDescriptionCodeFindBoxColumnStyle.ColumnNameSuffix)
							{
								var message = string.Format(ControlDescription.GetControlPath(grid) +
								" - The property '{0}' of data source bound to ZGrid should match property name case sensitively if set by ColumnName for exporting to excel.", column.ColumnName);
								notifications.AddError(message);
							}
						}
					}
				}
			}
		}

		#endregion
	}
}
