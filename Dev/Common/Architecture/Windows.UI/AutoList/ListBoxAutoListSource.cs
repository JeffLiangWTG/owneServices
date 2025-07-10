using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// An abstract implementation of IAutoListSource that uses a ListBox to show the items.
	/// </summary>
	public abstract class ListBoxAutoListSource : IAutoListSource
	{
		#region Constants

		const int ListBoxWidthIncrement = 25;
		const int DefaultItemsToShow = 10;

		#endregion

		readonly ITypeDescriptorContext context;

		protected ListBoxAutoListSource(ITypeDescriptorContext context)
		{ this.context = context; }

		/// <summary>
		/// Create a new ListBox object for the auto-list.
		/// </summary>
		protected virtual ListBox NewListControl()
		{ return new ListBox(); }

		/// <summary>
		/// Update the contents of the ListBox.
		/// </summary>
		protected abstract bool UpdateListControl(ListBox listControl, TextBoxBase textBox);

		protected virtual bool HasSelection(ListBox listControl)
		{ return listControl.SelectedIndex != -1; }

		/// <summary>
		/// Replace the text in the text box to the contents of the selected item of the given list box.
		/// </summary>
		protected virtual void ReplaceText(ListBox listControl, TextBoxBase textBox)
		{
			string newText = GetListControlSelectedText(listControl);
			if (newText != null)
			{
				textBox.Text = newText;
				textBox.SelectionStart = textBox.Text.Length;
				textBox.SelectionLength = 0;
			}
		}

		protected virtual void OnTextBoxKeyDown(ListBox listControl, KeyEventArgs e)
		{
			int pageScroll = (listControl.Height / listControl.ItemHeight) - 1;

			int newIndex = listControl.SelectedIndex;
			if (e.KeyCode == Keys.Up)
			{
				newIndex--;
			}

			if (e.KeyCode == Keys.Down)
			{
				newIndex++;
			}

			if (e.KeyCode == Keys.PageUp)
			{
				newIndex -= pageScroll;
			}

			if (e.KeyCode == Keys.PageDown)
			{
				newIndex += pageScroll;
			}

			if (newIndex < 0)
			{
				newIndex = 0;
			}
			if (newIndex >= listControl.Items.Count)
			{
				newIndex = listControl.Items.Count - 1;
			}
			if (listControl.SelectedIndex != newIndex)
			{
				listControl.SelectedIndex = newIndex;
			}
		}

		protected virtual void OnTextBoxKeyUp(ListBox listControl, KeyEventArgs e)
		{
		}

		protected virtual void OnTextBoxLostFocus()
		{
		}

		/// <summary>
		/// Get the selected item text of the given ListBox. Null if there is no selection.
		/// </summary>
		protected virtual string GetListControlSelectedText(ListBox listControl)
		{
			string result;
			if (!string.IsNullOrEmpty(listControl.ValueMember))
			{
				result = listControl.SelectedValue == null ? null : listControl.SelectedValue.ToString();
			}
			else
			{
				result = listControl.SelectedItem == null ? null : listControl.SelectedItem.ToString();
			}
			return result;
		}

		/// <summary>
		/// Update the size of the given list box.
		/// </summary>
		protected void UpdateListControlSize(ListBox listControl, string widestItemText)
		{
			Size newSize = new Size();

			// the height is the minimum of the items to show and the number of items available
			ControlDpiScalingHelper.SetHeight(ref newSize, ItemsToShow * listControl.ItemHeight + ControlDpiScalingHelper.ScaleToCurrentDpiY(5), false);

			// if the items havn't changed keep the list box the same width
			// otherwise calculate the maximum item width
			using (Graphics g = listControl.CreateGraphics())
			{
				ControlDpiScalingHelper.SetWidth(
					ref newSize,
					(int)Math.Max(newSize.Width, g.MeasureString(widestItemText, listControl.Font).Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(ListBoxWidthIncrement)),
					false);
			}
			listControl.Size = newSize;
		}

		/// <summary>
		/// Get or set the number of items to show on the list box.
		/// </summary>
		public virtual int ItemsToShow
		{ get { return DefaultItemsToShow; } }

		#region IAutoListSource Members

		public ITypeDescriptorContext Context
		{ get { return this.context; } }

		Control IAutoListSource.NewListControl()
		{
			ListBox result = this.NewListControl();
			result.Click += new EventHandler(OnListBox_Click);
			result.DoubleClick += new EventHandler(OnListBox_DoubleClick);
			return result;
		}

		bool IAutoListSource.UpdateListControl(Control listControl, TextBoxBase textBox)
		{
			bool result = this.UpdateListControl((ListBox)listControl, textBox);
			return result;
		}

		bool IAutoListSource.HasSelection(Control listControl)
		{ return this.HasSelection((ListBox)listControl); }

		void IAutoListSource.ReplaceText(Control listControl, TextBoxBase textBox)
		{ this.ReplaceText((ListBox)listControl, textBox); }

		void IAutoListSource.OnTextBoxKeyDown(Control listControl, KeyEventArgs e)
		{ this.OnTextBoxKeyDown((ListBox)listControl, e); }

		void IAutoListSource.OnTextBoxKeyUp(Control listControl, KeyEventArgs e)
		{ this.OnTextBoxKeyUp((ListBox)listControl, e); }

		public virtual event EventHandler ValueCommitRequired;

		protected void OnValueCommitRequired(EventArgs e)
		{
			if (ValueCommitRequired != null)
			{
				ValueCommitRequired(this, e);
			}
		}

		void IAutoListSource.OnTextBoxLostFocus()
		{ this.OnTextBoxLostFocus(); }

		#endregion

		#region Implementation

		void OnListBox_Click(object sender, EventArgs e)
		{ OnListBoxClick((ListBox)sender, Form.MousePosition); }

		void OnListBox_DoubleClick(object sender, EventArgs e)
		{ OnListBoxDoubleClick((ListBox)sender, Form.MousePosition); }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		protected void OnListBoxClick(ListBox sender, Point mouseScreenPosition)
		{
			ListBox list = sender;
			Point clientPoint = list.PointToClient(mouseScreenPosition);
			int index = list.IndexFromPoint(clientPoint.X, clientPoint.Y);
			if (index != -1)
			{
				list.SelectedIndex = index;
			}
		}

		protected void OnListBoxDoubleClick(ListBox sender, Point mouseScreenPosition)
		{
			Point mousePosition = sender.PointToClient(mouseScreenPosition);
			int index = sender.IndexFromPoint(mousePosition);
			if (HasSelection(sender) && index != ListBox.NoMatches)
			{
				OnValueCommitRequired(EventArgs.Empty);
			}
		}

		#endregion
	}
}
