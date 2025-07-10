using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common.Drawing.Colors;
using CargoWise.Windows.UI;
#if WINZOR
using Microsoft.AspNetCore.Components;
#endif

namespace Enterprise.ZArchitecture.GUI.SearchBox
{
	[ToolboxItem(false)]
	public partial class ZSearchListBox : KListBox // We don't need ZArch here
	{
		public ZSearchListBox() => itemBorderPen = new Pen(BackColor);

		public new object DataSource // databinding source doesn't update the interface (ie the list in the listbox) https://docs.microsoft.com/en-us/dotnet/framework/winforms/data-sources-supported-by-windows-forms
		{
			get => base.DataSource;
			set
			{
				base.DataSource = value;
				base.BindingContext = new BindingContext();
				if (value != null)
				{
					((CurrencyManager)base.BindingContext[base.DataSource]).Refresh();
					SelectedIndex = FirstSelectableItemIndex();
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				itemBorderPen.Dispose();
			}

			base.Dispose(disposing);
		}

		public IEnumerable<IDisplayItem> ItemsAsDisplayItems => Items.Cast<IDisplayItem>();

		public bool DrawItemBorder { get; set; }

		public bool AlternateItemColours { get; set; } = true;

		public int ItemTextHeight { get; set; } = ControlDpiScalingHelper.ScaleToCurrentDpiY(16);

		public int ItemTextSpacing { get; set; } = ControlDpiScalingHelper.ScaleToCurrentDpiY(4);

		public int ItemBorder { get; set; } = ControlDpiScalingHelper.ScaleToCurrentDpiX(0);

		[Browsable(false)]
		public override DrawMode DrawMode => DrawMode.OwnerDrawVariable;

		public event EventHandler OnClose;

		readonly Pen itemBorderPen;

		#region Event Overrides

		protected override void OnMeasureItem(MeasureItemEventArgs e)
		{
			if (DesignMode)
			{
				base.OnMeasureItem(e);
				return;
			}

			e.ItemHeight = (ItemTextHeight * (Items[e.Index] as IDisplayItem).Data.Count()) + (ItemTextSpacing * 2);
		}

#if !WINZOR

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "e.Bounds will already be scaled")]
		[SuppressMessage("CargoWiseOne", "CW1040", Justification = "Bogus warning, all values are scaled")]
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "\"Not a user-visible string, only design time\"")]
		// https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.drawitemeventargs.drawbackground?view=netframework-4.7.2
		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			if (e.Index < 0 || e.Index >= Items.Count)
			{
				return;
			}

			var item = DesignMode
				? DisplayItemFactory.CreateSearchItem(new Action(() => { }), "design", "mode")
				: (Items[e.Index] as IDisplayItem);

			// this can happen when you mouse down the bottom of a long list - winforms allocates 2^n items so if you have 51 in a list it'll make the list box 64
			// items long and mousing over this region will return a valid index into the list, eg 57, but item 57 in the list is a null item so we're going to skip it
			if (item == null || !item.Data.Any())
			{
				return;
			}

			var selected = e.State.HasFlag(DrawItemState.Selected) && item.IsSelectable;
			var foreColour = selected ? item.Theme.SelectedForeColour : item.Theme.ForeColour;
			var backColour = selected ? item.Theme.SelectedBackColour : item.Theme.BackColour;

			if (AlternateItemColours && e.Index % 2 == 0)
			{
				backColour = ColourHelper.ShiftBrightness(backColour, -0.06f);
			}

			var itemBounds = e.Bounds;
			itemBounds.Inflate(-ItemBorder, -ItemBorder);
			e.Graphics.FillRectangle(BrushProvider.FromColor(backColour), itemBounds);
			RenderItemForeground(e, itemBounds, item, foreColour);

			if (DrawItemBorder)
			{
				using (var pen = new Pen(BackColor))
				{
					e.Graphics.DrawRectangle(pen, e.Bounds);
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "itemDrawRect is already scaled")]
		protected void RenderItemForeground(DrawItemEventArgs e, Rectangle itemDrawRect, IDisplayItem item, Color foreColour)
		{
			var font = new Font(e.Font, item.Theme.FontStyle);
			const TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter;

			itemDrawRect.Height -= ItemTextSpacing * 2;
			itemDrawRect.Y += ItemTextSpacing;
			var textRect = itemDrawRect;

			var count = item.Data.Count();
			if (count > 0)
			{
				textRect.Height /= count;
			}

			var i = 0;
			foreach (var data in item.Data)
			{
				textRect.Y = itemDrawRect.Y + (i * textRect.Height);
				TextRenderer.DrawText(e.Graphics, data, font, textRect, foreColour, flags);
				i++;
			}
		}

#endif

		protected override void OnSelectedIndexChanged(EventArgs e)
		{
			base.OnSelectedIndexChanged(e);

			if (SelectedIndex >= 0 && !(SelectedItem as IDisplayItem).IsSelectable)
			{
				SelectedIndex = FirstSelectableItemIndex();
			}
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			if (Visible)
			{
				SelectedIndex = FirstSelectableItemIndex();
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			var index = IndexFromPoint(e.Location);
			if (index < 0 || index >= Items.Count || !(Items[index] as IDisplayItem).IsSelectable)
			{
				return;
			}

			SelectedIndex = index;
		}

		protected override void OnMouseClick(MouseEventArgs e)
		{
			base.OnMouseClick(e);

			var index = IndexFromPoint(e.Location);
			if (index != NoMatches && index != ushort.MaxValue)
			{
				ExecuteSelectedItem();
			}
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			base.OnKeyPress(e);

			if (e.KeyChar == (char)Keys.Enter)
			{
				ExecuteSelectedItem();
			}

			if (e.KeyChar == (char)Keys.Escape)
			{
				OnClose?.Invoke(this, new EventArgs());
			}
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			if (e.KeyCode == Keys.Down && SelectedIndex != -1 && SelectedIndex < Items.Count - 1)
			{
				var next = NextSelectableItemIndex();
				if (SelectedIndex != next)
				{
					SelectedIndex = next;
					e.Handled = true;
#if WINZOR
					InvokeRenderDispatcher(async () =>
					{
						await selectedElement.FocusAsync();
					});
#endif
				}
			}

			if (e.KeyCode == Keys.Up && SelectedIndex > 0)
			{
				var prev = PreviousSelectableItemIndex();
				if (SelectedIndex != prev)
				{
					SelectedIndex = prev;
					e.Handled = true;
#if WINZOR
					InvokeRenderDispatcher(async () => await selectedElement.FocusAsync());
#endif
				}
			}
		}

		#endregion

		#region Functionality

		public void SendKeyDown(KeyEventArgs e) => OnKeyDown(e);

		public void SendKeyPress(KeyPressEventArgs e) => OnKeyPress(e);

		protected internal int FirstSelectableItemIndex()
		{
			for (var i = 0; i < Items.Count; ++i)
			{
				if ((Items[i] as IDisplayItem).IsSelectable)
				{
					return i;
				}
			}
			return -1;
		}

		protected internal int NextSelectableItemIndex()
		{
			var currentIndex = SelectedIndex + 1;
			while (currentIndex < Items.Count)
			{
				if ((Items[currentIndex] as IDisplayItem).IsSelectable)
				{
					return currentIndex;
				}
				currentIndex++;
			}

			return SelectedIndex;
		}

		protected internal int PreviousSelectableItemIndex()
		{
			var currentIndex = SelectedIndex - 1;
			while (currentIndex >= 0)
			{
				if ((Items[currentIndex] as IDisplayItem).IsSelectable)
				{
					return currentIndex;
				}
				currentIndex--;
			}

			return SelectedIndex;
		}

		internal void ExecuteSelectedItem()
		{
			if (SelectedIndex < 0 || SelectedIndex >= Items.Count)
			{
				return;
			}

			if (SelectedItem is IDisplayItem selectedItem && selectedItem.IsSelectable)
			{
				selectedItem.Select();
				OnClose?.Invoke(this, new EventArgs());
			}
		}

		#endregion
	}
}
