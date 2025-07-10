using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Base template for any ZTemplate Column
	/// </summary>
	public abstract class ZItemTemplate : ITemplate
	{
		public ZItemTemplate(ZTemplateColumn column)
		{
			this.Column = column;
		}

		protected ZTemplateColumn Column;

		#region ITemplate Members

		public void InstantiateIn(Control container)
		{
			InstantiateInCore(container);
		}

		protected internal ISelfBindingWebControl Control
		{
			get;
			set;
		}

		protected virtual void InstantiateInCore(Control container)
		{
			Control = GetControl();
			if (Control != null)
			{
				Control.BindTo = Column.BindTo;
				if (this.Column.EditItemTemplate == this || this is IEditItemTemplate)
				{
					if (Column.EditorWidth > 0)
					{
						((WebControl)Control).Width = Column.EditorWidth;
					}
				}
				container.Controls.Add((Control)Control);
				container.DataBinding += new EventHandler(container_DataBinding);
			}
			if (Column != null && Column.ZOwner != null && Column.ZOwner.AutoSizeColumns)
			{
				SetupAutoSizeColumnControl(Control);
			}

			TableCell cell = container as TableCell;
			if (cell != null)
			{
				cell.Wrap = !Column.NoWrap;
			}
		}

		#endregion

		protected virtual void SetupAutoSizeColumnControl(ISelfBindingWebControl innerControl)
		{
			Column.ItemStyle.Width = Unit.Pixel(10);
		}

		protected internal abstract ISelfBindingWebControl GetControl();

		protected virtual void container_DataBinding(object sender, EventArgs e)
		{
			TableCell cell = sender as TableCell;
			if (cell != null)
			{
				DataGridItem item = cell.NamingContainer as DataGridItem;
				if (item != null)
				{
					foreach (Control ctrl in cell.Controls)
					{
						if (ctrl is ISelfBindingWebControl)
						{
							try
							{
								((ISelfBindingWebControl)ctrl).Bind(item.DataItem);
							}
							catch (Exception ex) when (!ex.IsCriticalException()) { }
						}
					}
					ChangeCell(cell);
				}
			}
		}

		protected virtual void ChangeCell(TableCell cell)
		{
		}
	}
}
