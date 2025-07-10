using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZDataGridItem : DataGridItem, INotificationProvider
	{
		public ZDataGridItem(int itemIndex, int dataSetIndex, ListItemType itemType)
			: base(itemIndex, dataSetIndex, itemType)
		{
		}

		#region Overrides

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not a code smell")]
		protected override void OnDataBinding(EventArgs e)
		{
			base.OnDataBinding(e);

			if (DataItem is BusinessObject bizO)
			{
				var includeReferenceKey = ((ZDataGrid)Parent?.Parent)?.IncludeItemDataRefKey ?? false;

				if (includeReferenceKey)
				{
					Attributes.Add("ref", bizO.PK.ToString());
				}
				RowNotifications = bizO.HasRowNotifications ? bizO.RowNotifications : new ZNotificationCollector(bizO, true, true, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);
			}
			else
			{
				RowNotifications = NotificationCollection.Empty;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "css style attribute value should not be translated")]
		protected override void RenderChildren(HtmlTextWriter writer)
		{
			if (ItemType == ListItemType.Pager)
			{
				this.Cells[0].ColumnSpan = VisibleColumnsCount;
			}

			base.RenderChildren(writer);

			bool isItem = ItemType == ListItemType.Item || ItemType == ListItemType.AlternatingItem;
			bool isHeader = ItemType == ListItemType.Header;

			if (isHeader || isItem)
			{
				Dictionary<ZNewRowColumn, int> newRowColumns = GetNewRowColumns();

				int newRowCount = 0;

				foreach (ZNewRowColumn newRowColumn in newRowColumns.Keys)
				{
					if (newRowColumn.Visible)
					{
						if (isHeader && newRowColumn.Collapsable)
						{
							// Add Header Cell for the ShowHide Button column
							writer.RenderBeginTag(HtmlTextWriterTag.Td);
							writer.RenderEndTag();
						}

						if (isItem)
						{
							// Creating Unique Id for New Row
							string newRowId = this.ClientID + "_" + newRowCount.ToString();
							int firstControlToPutInNewRowIndex = 0;

							int columnIndex = newRowColumns[newRowColumn];
							TableCell cell = Cells[columnIndex];
							ZExpandCollapseButton button = cell.Controls.Count > 0 ? cell.Controls[0] as ZExpandCollapseButton : null;

							if (newRowColumn.Collapsable && button != null)
							{
								// Render ExpandCollapse Button in a new Cell of current Row
								writer.RenderBeginTag(HtmlTextWriterTag.Td);
								button.ContentControlID = newRowId;
								button.RenderControl(writer);
								writer.RenderEndTag();
								// Moving to next control after ExpandCollapse Button
								firstControlToPutInNewRowIndex++;
							}
							// Closing current Row
							writer.RenderEndTag(); // </tr>

							// Starting new Row
							writer.AddAttribute(HtmlTextWriterAttribute.Id, newRowId);
							if (newRowColumn.Collapsable && button != null && !button.Expand)
							{
								writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
							}
							writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>

							// Adding a single Cell
							writer.AddAttribute(HtmlTextWriterAttribute.Colspan, VisibleColumnsCount.ToString());

							if (!String.IsNullOrEmpty(newRowColumn.ItemStyle.CssClass))
							{
								writer.AddAttribute(HtmlTextWriterAttribute.Class, newRowColumn.ItemStyle.CssClass);
							}

							writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>

							if (String.IsNullOrEmpty(newRowColumn.BindTo))
							{
								writer.Write("&nbsp;");
							}
							else
							{
								// Render controls in the new Row
								for (int i = firstControlToPutInNewRowIndex; i < cell.Controls.Count; i++)
								{
									cell.Visible = true;
									cell.Controls[i].RenderControl(writer);
								}
							}

							// Closing Cell
							writer.RenderEndTag(); // </td>

							newRowCount++;
						}
					}
				}
			}
		}

		Dictionary<ZNewRowColumn, int> GetNewRowColumns()
		{
			DataGrid parent = this.Parent.Parent as DataGrid
				?? this.Parent.Parent.Parent as DataGrid;
			Dictionary<ZNewRowColumn, int> newRowColumns = new Dictionary<ZNewRowColumn, int>();

			for (int i = 0; i < parent.Columns.Count; i++)
			{
				ZNewRowColumn newRowColumn = parent.Columns[i] as ZNewRowColumn;

				if (newRowColumn != null)
				{
					if (i < parent.Columns.Count - 1 && !(parent.Columns[i + 1] is ZNewRowColumn))
					{
						throw new InvalidOperationException("A ZNewRowColumn in the Columns collection can only be followed by another ZNewRowColumn.");
					}
					newRowColumns.Add(newRowColumn, i);
				}
			}

			return newRowColumns;
		}

		#endregion

		#region VisibleColumnsCount

		int VisibleColumnsCount
		{
			get
			{
				ZDataGrid parent = (ZDataGrid)this.Parent.Parent;
				int result = parent.AllowMultiLineSelection ? 1 : 0;

				foreach (DataGridColumn column in parent.Columns)
				{
					ZNewRowColumn newRowColumn = column as ZNewRowColumn;
					if (column.Visible || (newRowColumn != null && newRowColumn.Visible && newRowColumn.Collapsable))
					{
						result++;
					}
				}
				return result;
			}
		}

		#endregion

		#region INotificationProvider Members

		public IEnumerable<INotification> Notifications
		{
			get { return RowNotifications ?? NotificationCollection.Empty; }
		}
		IEnumerable<INotification> RowNotifications;

		bool INotificationProvider.HasNotifications()
		{
			return Notifications.HasNotifications();
		}

		bool INotificationProvider.HasNotifications(INotificationType type)
		{
			return Notifications.HasNotifications(type);
		}

		INotificationType INotificationProvider.GetHighestSeverityNotificationType()
		{
			return Notifications.GetHighestSeverityNotificationType();
		}

		#endregion

		#region Internal Properties

		internal void RenderChildrenInternal(HtmlTextWriter writer) => RenderChildren(writer);

		#endregion
	}
}
