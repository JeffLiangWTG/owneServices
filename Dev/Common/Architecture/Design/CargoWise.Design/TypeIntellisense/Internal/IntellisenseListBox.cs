using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common.Testing;
using CargoWise.Windows.UI;

namespace CargoWise.Design.TypeIntellisense
{
	/// <summary>
	/// The ListBox that is used on the type intellisense auto-list.
	/// </summary>
	[DesignerSerializer(typeof(Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	internal class IntellisenseListBox : ListBox
	{
		int savedTopIndexRelativeToSelectedIndex;
		bool autoSetOutlineSelectedItemSuspended;
		readonly Color initialBackColor;

		#region Constants

		const int SCROLLBAR_WIDTH = 16;
		const int MINIMUM_WIDTH = 150;

		#endregion

		public IntellisenseListBox()
		{
			DisposableLeakListener.Instance.RegisterDisposable(this);
			DrawMode = DrawMode.OwnerDrawFixed;
			ItemHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(16);
			ControlDpiScalingHelper.SetWidth(this, MINIMUM_WIDTH, true);
			ControlDpiScalingHelper.SetHeight(this, ItemHeight * 10, false);
			DisplayMember = "DisplayName";
			initialBackColor = BackColor;
		}

		public new IntellisenseDataSource DataSource
		{
			get { return (IntellisenseDataSource)base.DataSource; }
			set
			{
				if (DataSource != null)
				{
					DataSource.PartialNameChanged -= new EventHandler(DataSource_PartialNameChanged);
					DataSource.BeforeListChanged -= new ListChangedEventHandler(DataSource_BeforeListChanged);
					DataSource.AfterListChanged -= new ListChangedEventHandler(DataSource_AfterListChanged);
					DataSource.WidestDisplayNameChanged -= new EventHandler(OnDataSource_WidestDisplayNameChanged);
					DataSource.FullyPopulatedChanged -= new EventHandler(OnDataSource_FullyPopulatedChanged);
				}
				base.DataSource = value;
				if (DataSource != null)
				{
					DataSource.PartialNameChanged += new EventHandler(DataSource_PartialNameChanged);
					DataSource.BeforeListChanged += new ListChangedEventHandler(DataSource_BeforeListChanged);
					DataSource.AfterListChanged += new ListChangedEventHandler(DataSource_AfterListChanged);
					DataSource.WidestDisplayNameChanged += new EventHandler(OnDataSource_WidestDisplayNameChanged);
					DataSource.FullyPopulatedChanged += new EventHandler(OnDataSource_FullyPopulatedChanged);
				}
				OnDataSource_FullyPopulatedChanged(this, EventArgs.Empty);
			}
		}

		void OnDataSource_FullyPopulatedChanged(object sender, EventArgs e)
		{
			if (!Disposing &&
				!IsDisposed &&
				IsHandleCreated &&
				InvokeRequired)
			{
				try
				{
					Invoke(new EventHandler(OnDataSource_FullyPopulatedChanged), new object[] { sender, e });
				}
				catch (ObjectDisposedException)
				{ }
				catch (InvalidOperationException)
				{ }
			}
			else if (IsHandleCreated)
			{
				if (DataSource != null && DataSource.FullyPopulated)
				{
					BackColor = initialBackColor;
				}
				else
				{
					Color c = initialBackColor;
					BackColor = Color.FromArgb(
						c.R,
						c.G > 10 ? c.G - 30 : c.G,
						c.B > 10 ? c.B - 30 : c.B);
				}
			}
		}

		public override int SelectedIndex
		{
			get { return base.SelectedIndex; }
			set
			{
				if (value < Items.Count)
				{
					base.SelectedIndex = value;
				}
				if (!autoSetOutlineSelectedItemSuspended)
				{
					OutlineSelectedItem = false;
				}
			}
		}

		public bool OutlineSelectedItem
		{
			get { return outlineSelectedItem; }
			set
			{
				if (value != OutlineSelectedItem)
				{
					outlineSelectedItem = value;
					if (SelectedIndex != -1)
					{
						Invalidate();
					}
				}
			}
		}
		bool outlineSelectedItem;

		#region Implementation

		void DataSource_PartialNameChanged(object sender, EventArgs e)
		{
			if (Parent != null && IsHandleCreated)
			{
				UpdateSelectedItem();
			}
		}

		void DataSource_BeforeListChanged(object sender, ListChangedEventArgs e)
		{
			SuspendDrawItems();
			savedTopIndexRelativeToSelectedIndex = SelectedIndex - TopIndex;
			autoSetOutlineSelectedItemSuspended = true;
		}

		void DataSource_AfterListChanged(object sender, ListChangedEventArgs e)
		{
			autoSetOutlineSelectedItemSuspended = false;
			if (DataSource != null)
			{
				// update the top index so it doesnt change the scroll position on us
				int new_top_index = SelectedIndex - savedTopIndexRelativeToSelectedIndex;
				if (new_top_index >= DataSource.Count)
				{
					new_top_index = DataSource.Count - 1;
				}
				TopIndex = new_top_index;

				// if an item is added, see if we can select it so the user gets an update of the closest match
				if (e.ListChangedType == ListChangedType.ItemAdded)
				{
					UpdateSelectedItemInLightOfNewlyAddedItem(e.NewIndex);
				}

				ResumeDrawItems();
			}
		}

		void OnDataSource_WidestDisplayNameChanged(object sender, EventArgs e)
		{
			if (IsHandleCreated)
			{
				using (Graphics g = CreateGraphics())
				{
					int new_width = ControlDpiScalingHelper.UnscaleFromCurrentDpiX((int)g.MeasureString(DataSource.WidestDisplayName, Font, int.MaxValue).Width) + SCROLLBAR_WIDTH;
					if (new_width < MINIMUM_WIDTH)
					{
						new_width = MINIMUM_WIDTH;
					}
					ControlDpiScalingHelper.SetWidth(this, new_width, true);
				}
			}
		}

		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			if (!drawItemSuspended)
			{
				base.OnDrawItem(e);
				if (OutlineSelectedItem)
				{
					e.Graphics.FillRectangle(GetSolidBrush(BackColor), e.Bounds);
				}
				else
				{
					if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
					{
						e.Graphics.FillRectangle(SystemBrushes.MenuHighlight, e.Bounds);
					}
					else
					{
						e.Graphics.FillRectangle(GetSolidBrush(BackColor), e.Bounds);
					}
				}

				Image i = (e.Index == -1) ? null : GetImage(e.Index);
				if (i != null)
				{
					int margin = (e.Bounds.Height - i.Height) / 2;
					Rectangle iconBounds = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledRectangle(
						e.Bounds.Left + margin, e.Bounds.Top + margin,
						ControlDpiScalingHelper.UnscaleFromCurrentDpiX(i.Width), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(i.Height));
					e.Graphics.DrawImage(i, iconBounds);

					int textTopMargin = (ItemHeight - e.Font.Height) / 2;
					if (OutlineSelectedItem)
					{
						e.Graphics.DrawString(
							GetItemText(((IList)DataSource)[e.Index]), e.Font, GetSolidBrush(ForeColor),// e.ForeColor),
							iconBounds.Right + margin, e.Bounds.Top + textTopMargin);
					}
					else
					{
						e.Graphics.DrawString(
							GetItemText(((IList)DataSource)[e.Index]), e.Font, GetSolidBrush(e.ForeColor),
							iconBounds.Right + margin, e.Bounds.Top + textTopMargin);
					}
				}

				if (OutlineSelectedItem && e.Index == SelectedIndex)
				{
					ControlPaint.DrawFocusRectangle(e.Graphics, e.Bounds, ForeColor, BackColor);
				}
			}
			else
			{
				regionsToInvalidateAfterResumeDrawItem.Add(e.Bounds);
			}
		}

		bool drawItemSuspended;
		readonly ArrayList regionsToInvalidateAfterResumeDrawItem = new ArrayList();
		void SuspendDrawItems()
		{
			drawItemSuspended = true;
		}

		void ResumeDrawItems()
		{
			drawItemSuspended = false;
			foreach (Rectangle rect in regionsToInvalidateAfterResumeDrawItem)
			{
				Invalidate(rect);
			}
			regionsToInvalidateAfterResumeDrawItem.Clear();
		}

		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);
			if (Parent != null && IsHandleCreated)
			{
				BeginInvoke(new MethodInvoker(delegate
				{ OutlineSelectedItem = true; }));
			}
		}

		void UpdateSelectedItem()
		{
			if (DataSource.Count > 0)
			{
				if (string.IsNullOrEmpty(DataSource.RightMostPartialName))
				{
					SelectedIndex = 0;
					OutlineSelectedItem = true;
				}
				else
				{
					int index = DataSource.FindNearestMatch();
					if (index != -1)
					{
						SetSelectedIndexAndCentreSelection(index);
						if (!DataSource[index].DisplayName.StartsWith(DataSource.RightMostPartialName, StringComparison.OrdinalIgnoreCase))
						{
							OutlineSelectedItem = true;
						}
						else
						{
							OutlineSelectedItem = false;
						}
					}
				}
			}
		}

		void UpdateSelectedItemInLightOfNewlyAddedItem(int newlyAddedItem)
		{
			if (!string.IsNullOrEmpty(DataSource.RightMostPartialName) &&
				DataSource[newlyAddedItem].DisplayName.StartsWith(DataSource.RightMostPartialName, StringComparison.OrdinalIgnoreCase))
			{
				if (SelectedItem == null ||
					((IntellisenseDataSourceItem)SelectedItem).DisplayName.ToUpperInvariant() != DataSource.RightMostPartialName.ToUpperInvariant())
				{
					SetSelectedIndexAndCentreSelection(newlyAddedItem);
				}
			}
		}

		void SetSelectedIndexAndCentreSelection(int index)
		{
			SelectedIndex = index;
			int top_index = index - (Height / ItemHeight) / 2;
			TopIndex = top_index < 0 ? 0 : top_index;
		}

		Image GetImage(int index)
		{
			IntellisenseDataSourceItem item = DataSource == null ? null : (IntellisenseDataSourceItem)((IList)DataSource)[index];
			return item == null ? null : IntellisenseListIcons.GetImage(item.Type);
		}

		SolidBrush brush;
		Brush GetSolidBrush(Color color)
		{
			if (brush == null || brush.Color != color)
			{
				if (brush != null)
				{
					brush.Dispose();
				}
				brush = new SolidBrush(color);
			}
			return brush;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				if (DataSource != null)
				{
					DataSource.Dispose();
				}
				if (brush != null)
				{
					brush.Dispose();
				}
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
			}
		}

		#endregion
	}
}
