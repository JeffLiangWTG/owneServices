using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.JSInterop;
using Microsoft.AspNetCore.Components;
using WinzorFramework.Extensions;
using WinzorFramework.Telemetry;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public interface IDropFormParent
	{
		Font Font { get; }
		int MaxItemsToShowInDropDown { get; }
		string Code { get; }
		ZDropEdit.ShowInDropDownList ShowInDropDown { get; }
		bool ShowColorInDropDown { get; }
		int MinDropDownWidth { get; }
		ICodeDescription LastSelectedItem { get; }

		IList GetFilteredListForDropDown();
		bool IsItemValidForAutoComplete(ICodeDescription item);
		MultilingualString GetMultilingualValue(ICodeDescription item);
		bool ShouldCloseOnMouseDown(Point mouseLocation);

		void OnDropDownClosed();
		void OnItemSelected(ICodeDescription item, bool commitValue);

		bool ShowHorizontalScrollBar { get; }
		bool SupportsEmptyCode { get; }
		bool Focused { get; }

		ElementReference ElementReference { get; }
	}

	public partial class ZDropForm : Control
	{
		readonly Control parent;

		public override bool UseParentDivForLayout => false;

		public override bool RenderInPortal => true;

		protected virtual Control ExtraControl => null;

		public ZDropForm(IDropFormParent parentDropEdit)
		{
			TabStop = false;
			SetVisibleState(false);
			parent = (Control)parentDropEdit;
			ParentDropEdit = parentDropEdit;
			VisibleItems = parentDropEdit.MaxItemsToShowInDropDown;
			SelectItemFromCode();
		}

		// ZDropForm inherits Form and has no parent in CW1 (it does here for winzor render purpose), override Visible to omit parent as work-around
		public override bool Visible
		{
			get => ControlVisible;
			set => base.Visible = value;
		}

		protected sealed class ZVScrollBar : VScrollBar
		{
		}

		protected IDropFormParent ParentDropEdit;

		protected int VisibleItems;

		IList _list;
		protected IList List
		{
			get
			{
				if (_list == null)
				{
					RefreshList();
				}
				return _list;
			}
		}

		ItemToRender[] ListToRender;

		protected virtual int VerticalIndent => 1;

		protected virtual int ExtraSpaceHeight => 2;

		protected virtual int MinimumWidth => 0;
		protected int HighlightedItem = -1;
		protected virtual int NextItemIndex => checked(HighlightedItem + 1);
		protected virtual int PreviousItemIndex => checked(HighlightedItem - 1);
		protected virtual int DescriptionBackgroundOverlap => 8;
		protected virtual bool IncludeItemInCodeWidthCalculation(ICodeDescription item) => true;
		protected virtual IEnumerable<Rectangle> GetSpecialAreasCore() => Enumerable.Empty<Rectangle>();
		protected ZVScrollBar ScrollBar;

		protected internal virtual bool HandleClickInSpecialArea(Point locationForDropForm) => false;

		protected int GetStringWidth(string text)
		{
			return string.IsNullOrEmpty(text) ? 0 : TextRenderer.MeasureText(text, Font).Width;
		}

		protected int DescriptionWidth
		{
			get
			{
				if (ListToRender != null && ListToRender.Length > 0 && ParentDropEdit.ShowInDropDown != ZDropEdit.ShowInDropDownList.OnlyShowCode)
				{
					return ListToRender.Max(item => GetStringWidth(item.Description)) + PaddingAdjustment;
				}
				return 0;
			}
		}

		protected int CodeWidth
		{
			get
			{
				if (fCodeWidth < 0 && ListToRender != null && ListToRender.Length > 0 && ParentDropEdit.ShowInDropDown != ZDropEdit.ShowInDropDownList.OnlyShowDescription)
				{
					fCodeWidth = CodeWidthLeftAdjustment + ListToRender.Max(item => GetStringWidth(item.Code)) + PaddingAdjustment;
				}
				return fCodeWidth;
			}
		}

		/// <summary>
		/// zdropform__item--category left adjustment
		/// </summary>
		const int CodeWidthLeftAdjustment = 7;
		const int PaddingAdjustment = 14;
		int fCodeWidth = -1;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Text to measure")]
		int ItemHeight
		{
			get
			{
				if (itemHeight <= 0)
				{
					using (var graphics = new BGraphics())
					{
						itemHeight = (int)graphics.MeasureString("Item", Font, Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(1)).Height;
					}
				}

				return itemHeight;
			}
		}
		int itemHeight = -1;
		const int MaxDropWidth = 1350; // The current minimum width requirement for user screen is 1366px

		protected internal void SetSizing()
		{
			fCodeWidth = -1;
			var totalWidth = 0;
			if (ParentDropEdit.ShowInDropDown != ZDropEdit.ShowInDropDownList.OnlyShowDescription)
			{
				totalWidth += CodeWidth;
			}
			if (ParentDropEdit.ShowInDropDown != ZDropEdit.ShowInDropDownList.OnlyShowCode)
			{
				totalWidth += DescriptionWidth;
			}

			var count = List == null ? 0 : List.Count;
			if (count > VisibleItems)
			{
				totalWidth += ZGUISystemInformation.VerticalScrollBarWidth;
			}

			var itemsToDisplay = Math.Min(List == null ? 0 : List.Count, VisibleItems);
			Height = Math.Max(ItemHeight * itemsToDisplay, ItemHeight) + ExtraSpaceHeight;
			Width = Math.Min(MaxDropWidth, Math.Max(totalWidth , ParentDropEdit.MinDropDownWidth));

			if (ExtraControl != null)
			{
				Width = Math.Max(Width, ExtraControl.Width);
				Controls.Add(ExtraControl);
			}
		}

		protected string ZDropFormSizeStyleString => $"width:{Width}px;height:{Height}px;";

		const int zIndexOverride = short.MaxValue;
		int originalZIndex;
		public void ShowDropDown(Point pointInScreenCoords, bool mouseIsCurrentlyDown)
		{
			using var trace = TelemetryService.ActivitySource.StartActivity($"{nameof(ZDropForm)}.{nameof(ShowDropDown)}");
			RefreshList();
			SetSizing();
			Visible = true;
			Enabled = true;
			parent.WinzorSpecificControls.Add(this);
			if (parent.ZIndex > 0)
			{
				originalZIndex = parent.ZIndex;
				parent.ZIndex = zIndexOverride;
			}
		}

		public void HideDropDown()
		{
			Visible = false;
			Enabled = false;
			parent.WinzorSpecificControls.Remove(this);
			if (parent.ZIndex == zIndexOverride)
			{
				parent.ZIndex = originalZIndex;
			}
			ParentDropEdit.OnDropDownClosed();
		}

		public void Close() => HideDropDown();

		public void RefreshList()
		{
			_list = ParentDropEdit.GetFilteredListForDropDown();

			if (_list != null)
			{
				// calculate drop form item style in pre-render phase.
				hasCategory = _list.OfType<ICodeDescription>().Any(m => IsCategoryItem(m));

				var showCode = ParentDropEdit.ShowInDropDown is ZDropEdit.ShowInDropDownList.OnlyShowCode or ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
				var showDescription = ParentDropEdit.ShowInDropDown is ZDropEdit.ShowInDropDownList.OnlyShowDescription or ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
				var codeSpanClassString = ParentDropEdit.ShowInDropDown is ZDropEdit.ShowInDropDownList.OnlyShowCode && hasCategory ? "description--with-code" : string.Empty;
				var descriptionSpanClassString = ParentDropEdit.ShowInDropDown is ZDropEdit.ShowInDropDownList.OnlyShowDescription ? string.Empty : "description--with-code";

				ListToRender = _list.OfType<ICodeDescription>().Select((m, i) => {
					var item = new ItemToRender(m.PK, m.Code, this)
					{
						Description = showDescription ? m.Description : null,
						ItemClassString = ZDropFormItemClassString(m),
						IsCommonItem = IsCommonItem(m),
						IsCategoryItem = IsCategoryItem(m),
						ShowCode = showCode,
						ShowDescription = showDescription,
						ShowColor = ParentDropEdit.ShowColorInDropDown,
						CodeSpanClassString = codeSpanClassString,
						DescriptionSpanClassString = descriptionSpanClassString,
						ColorStyleString = ColorStyleString(m),
						Index = i,
						Title = RenderTooltipString(m),
						IsSelectable = IsCurrentRowSelectable,
						CodeStyleString = CodeStyleString
					};
					return item;
				}).ToArray();

				RenderItemsWithVirtualize = ListToRender.Length > 500;
				SetSizing();
				SelectItemFromCode();

				Invalidate();
			}
			else
			{
				ListToRender = Array.Empty<ItemToRender>();
			}
		}

		internal void SelectItemFromCode(string value = null)
		{
			HighlightedItem = -1;
			var currentText = value ?? ParentDropEdit.Code;

			if (!string.IsNullOrEmpty(currentText) || ParentDropEdit.SupportsEmptyCode)
			{
				foreach (var comparison in AutoCompleteSearchComparers)
				{
					var match = AutocompleteSearchHelper.FindIndex(ParentDropEdit, List, currentText, comparison, ParentDropEdit.LastSelectedItem);
					if (match >= 0)
					{
						HighlightedItem = match;
						NotifyRenderRequired();
						break;
					}
				}
			}
		}

		protected virtual IEnumerable<AutoCompleteStringComparisonType> AutoCompleteSearchComparers => new[] { AutoCompleteStringComparisonType.Exact, AutoCompleteStringComparisonType.StartsWith };

		async Task SelectItemAsync(ItemToRender itemToRender)
		{
			await InvokeWinzorDispatcherAsync(() =>
			{
				var item = List.OfType<ICodeDescription>().ElementAt(itemToRender.Index);
				if (ParentDropEdit.IsItemValidForAutoComplete(item))
				{
					if (TranslationFeedbackManager.InTranslationFeedbackMode())
					{
						ParentDropEdit.OnItemSelected(item, false);
						HideDropDown();
					}
					else
					{
						ParentDropEdit.OnItemSelected(item, true);
						HideDropDown();
					}
				}
			});
		}

		internal ICodeDescription SelectedItem
		{
			get { return (List != null && HighlightedItem < List.Count && HighlightedItem >= 0) ? (ICodeDescription)List[HighlightedItem] : null; }
		}

		internal ICodeDescription FirstItem
		{
			get { return List?.Cast<ICodeDescription>().FirstOrDefault(); }
		}

		public virtual bool IsItemSelectable(ICodeDescription item)
		{
			return item != null;
		}

		protected bool IsCurrentRowSelectable(int row)
		{
			return HighlightedItem == row && IsItemSelectable(SelectedItem);
		}

		internal bool HandleCommandKey(Keys keyData)
		{
			var keyWasHandled = true;
			switch (keyData)
			{
				case Keys.Down:
					if (List != null && HighlightedItem < List.Count - 1)
					{
						HighlightedItem = NextItemIndex;
					}
					break;

				case Keys.Up:
					if (HighlightedItem > 0)
					{
						HighlightedItem = PreviousItemIndex;
					}
					break;

				case Keys.PageDown:
					if (List != null && HighlightedItem < List.Count - 1)
					{
						var newIndex = Math.Max(HighlightedItem + VisibleItems - 1, VisibleItems - 1);
						HighlightedItem = Math.Min(List.Count - 1, newIndex);
					}
					break;

				case Keys.PageUp:
					if (HighlightedItem > 0)
					{
						var newIndex = HighlightedItem - VisibleItems + 1;
						HighlightedItem = Math.Max(0, newIndex);
					}
					break;

				default:
					keyWasHandled = false;
					break;
			}

			if (keyWasHandled && HighlightedItem != -1)
			{
				NotifyRenderRequired();
			}

			return keyWasHandled;
		}

#if DEBUG
		internal int HighlightedItem_Exposed
		{
			get { return HighlightedItem; }
			set { HighlightedItem = value; }
		}

		internal IList List_Exposed => List;
#endif

		protected bool RenderTooltip;

		protected virtual bool IsCommonItem(ICodeDescription item) => false;

		protected virtual bool IsCategoryItem(ICodeDescription item) => item is CategoryCodeDescriptionPair;

		protected virtual string ZDropFormItemClassString(ICodeDescription item)
		{
			var classString = "zdropform__item";
			if (!IsItemSelectable(item))
			{
				classString += " zdropform__item--unselectable";
			}
			return classString;
		}

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
			GetJSInterop<IDropFormJSInterop>()?.PreloadInterop();
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (HighlightedItem > -1)
			{
				await (GetJSInterop<IDropFormJSInterop>()?.ScrollToAsync(ElementReference, itemHeight, HighlightedItem) ?? Task.CompletedTask);
			}
			await base.OnAfterRenderAsync(firstRender);
		}

		string RenderTooltipString(ICodeDescription item)
		{
			return RenderTooltip ? item.Code : string.Empty;
		}

		string CodeStyleString(string itemStyleString, int index)
		{
			var result = string.Empty;
			if (index == 0 && ParentDropEdit.ShowInDropDown == ZDropEdit.ShowInDropDownList.ShowCodeAndDescription)
			{
				result = $"width:{100 * CodeWidth / Width}%;";
			}

			if (string.Equals(itemStyleString, "zdropform__item"))
			{
				result += $"height:{ItemHeight}px;";
			}
			return result;
		}

		string ColorStyleString(ICodeDescription item) =>
			$"background-color: {Color.FromName(item.Code.Replace(" ", "")).GetColorStyleValue()}; width: 20px";

		// ZDropForm has no width set so we must override to allow it to render.
		protected override bool ShouldRender => true;

		bool hasCategory;

		bool RenderItemsWithVirtualize { get; set; }

		public class ItemToRender : ICodeDescription
		{
			public ItemToRender(object pk, string code, ZDropForm parent)
			{
				PK = pk;
				Code = code;
				OnClick = EventCallback.Factory.Create<WebMouseEventArgs>(this, e => parent.SelectItemAsync(this));
			}

			public object PK { get; init; }

			public string Code { get; init; }

			public string Description { get; init; }

			public bool IsCommonItem { get; init; }

			public string ItemClassString { get; init; }

			public bool IsCategoryItem { get; init; }

			public int Index { get; init; }

			public string Title { get; init; }

			public bool ShowCode { get; init; }

			public bool ShowDescription { get; init; }

			public bool ShowColor { get; init; }

			public string CodeSpanClassString { get; init; }

			public string DescriptionSpanClassString { get; init; }

			public string ColorStyleString { get; init; }

			public Func<int, bool> IsSelectable { get; init; }

			public Func<string, int, string> CodeStyleString { get; init; }

			public EventCallback<WebMouseEventArgs> OnClick { get; init; }
		}
	}
}

#region Test
#if DEBUG

namespace Enterprise.ZArchitecture.GUI.Testing
{
	using Enterprise.ZArchitecture.GUI.Internal;

	class MockZDropForm : ZDropForm
	{
		public MockZDropForm(IDropFormParent parentDropEdit)
			: base(parentDropEdit)
		{ }

		public object HorizontalScrollBar_Exposed() => null;
	}
}

#endif
#endregion
