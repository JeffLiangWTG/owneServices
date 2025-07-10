using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	#region class ZFilterStripDropEdit

	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis]
	[ToolboxItem(false)]
	public partial class ZFilterStripDropEdit : ZDropEdit
	{
		public ZFilterStripDropEdit()
			: base()
		{
			DropDownClosed += ZFilterStripDropEdit_DropDownClosed;
			CurrentText = String.Empty;
			ProposedText = String.Empty;
		}

		protected override ZDropButton NewDropButton()
		{
			return new ZFilterStripDropButton();
		}

		protected override ZDropCodeBox NewCodeBox()
		{
			return new ZFilterStripDropCodeBox();
		}

		internal string ProposedText { get; set; }
		internal string CurrentText { get; set; }

		public bool FormattingEnabled { get; set; }

		void ZFilterStripDropEdit_DropDownClosed(object sender, EventArgs e)
		{
			CurrentText = String.Empty;
			ProposedText = (Text == FilterStrip.SelectFilterDescriptionText) ? String.Empty : Text;
		}

		IList filteredList;
		protected override IList GetFilteredListForDropDown()
		{
			var baseList = base.GetFilteredListForDropDown();
			if (baseList != null)
			{
				if (ShouldFilterList(CurrentText, ProposedText))
				{
					this.filteredList = (ShouldUseBaseList(this.filteredList, CurrentText, ProposedText))
						? GetFilteredList(baseList, ProposedText)
						: GetFilteredList(this.filteredList, ProposedText);

					CurrentText = ProposedText;
				}
			}

			return this.filteredList ?? (this.filteredList = baseList);
		}

		static bool ShouldFilterList(string currentText, string proposedText)
		{
			return string.IsNullOrEmpty(proposedText)
				|| !string.Equals(currentText, proposedText, StringComparison.OrdinalIgnoreCase);
		}

		static bool ShouldUseBaseList(IList sourceList, string currentText, string proposedText)
		{
			return string.IsNullOrEmpty(currentText)
				|| !proposedText.StartsWith(currentText, StringComparison.OrdinalIgnoreCase)
				|| sourceList == null
				|| sourceList.Count == 0;
		}

		IList GetFilteredList(IList sourceList, string proposedText)
		{
			var newList = new CodeDescriptionPairList();

			if (sourceList != null)
			{
				ICodeDescription categorySeparator = null;
				ICodeDescription categoryItem = null;
				var newCategory = false;

				foreach (ICodeDescription item in sourceList)
				{
					if (Helper.IsSeparator(item))
					{
						newCategory = true;
						if (categorySeparator == null)
						{
							categorySeparator = item;
						}
					}
					else if (Helper.IsCategoryItem(item))
					{
						newCategory = true;
						categoryItem = item;
					}
					else
					{
						var code = item.GetMultilingualCode().ToString();
						if (code.Contains(proposedText, StringComparison.OrdinalIgnoreCase))
						{
							if (code.Equals(proposedText, StringComparison.OrdinalIgnoreCase)
								|| code.Trim().Equals(proposedText, StringComparison.OrdinalIgnoreCase))
							{
								return null;
							}

							if (newCategory)
							{
								newCategory = false;

								if (categorySeparator != null)
								{
									newList.Add(categorySeparator);
								}

								if (categoryItem != null)
								{
									newList.Add(categoryItem);
									categoryItem = null;
								}
							}

							newList.Add(item);
						}
					}
				}
			}

			return newList;
		}

		protected override bool IsItemValidForAutoCompleteCore(ICodeDescription item)
		{
			return Helper.IsSelectableItem(item);
		}

		ZFilterStripDropHelper helper;
		ZFilterStripDropHelper Helper => LazyInitializer.EnsureInitialized(ref helper);
	}

	#endregion

	#region class ZFilterStripDropButton

	[ToolboxItem(false)]
	public class ZFilterStripDropButton : ZDropButton
	{
		protected override ZDropForm NewDropForm()
		{
			return new ZFilterStripDropForm((ZFilterStripDropEdit)ParentDropEdit);
		}

		internal void RefreshList()
		{
			DropDown?.RefreshList();
		}
	}

	#endregion

	#region class ZFilterStripDropForm

	[ToolboxItem(false)]
	public partial class ZFilterStripDropForm : ZDropForm
	{
#if !WINZOR
		readonly KToolTip toolTip;
		readonly System.Windows.Forms.Timer timer;
		readonly IContainer container = new Container();
#endif

		[SuppressMessage("Microsoft.Mobility", "CA1601:DoNotUseTimersThatPreventPowerStateChanges", Justification = "Timer is stopped after one cycle, therefore will not fire more than once per second")]
		public ZFilterStripDropForm(ZFilterStripDropEdit parentDropEdit)
			: base(parentDropEdit)
		{
#if !WINZOR
			toolTip = new KToolTip(container) { ShowAlways = true };

			timer = new System.Windows.Forms.Timer(container) { Interval = 500 };
			timer.Tick += Timer_Tick;

			Load += (o, e) => timer.Start();
#else
			RenderTooltip = true;
#endif
		}

		protected new ZFilterStripDropEdit ParentDropEdit
		{
			get { return (ZFilterStripDropEdit)base.ParentDropEdit; }
		}

		#region List

		protected override IEnumerable<AutoCompleteStringComparisonType> AutoCompleteSearchComparers =>
			new[] { AutoCompleteStringComparisonType.Exact, AutoCompleteStringComparisonType.StartsWith, AutoCompleteStringComparisonType.Contains };

		#endregion // List

		#region Selecting an Item

#if !WINZOR
		[SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is not a button")]
		void Timer_Tick(object sender, EventArgs e)
		{
			timer.Stop();
			var element = ItemAtLocation(PointToClient(Cursor.Position));
			if (element != null)
			{
				toolTip.SetToolTip(this, element.GetMultilingualCode());
			}
		}

		protected override void HighlightItem(int highlightedItemIndex)
		{
			base.HighlightItem(highlightedItemIndex);

			toolTip.Hide(this);
			timer.Start();
		}
#endif

		public override bool IsItemSelectable(ICodeDescription item)
		{
			return Helper.IsSelectableItem(item) && base.IsItemSelectable(item);
		}

		protected override int NextItemIndex
		{
			get
			{
				var result = base.NextItemIndex;

				while (result < List.Count - 1 && !IsItemSelectable((ICodeDescription)List[result]))
				{
					result++;
				}

				if (!IsItemSelectable((ICodeDescription)List[result]))
				{
					result = HighlightedItem;   // Don't move past last visible item
				}

				return result;
			}
		}

		protected override int PreviousItemIndex
		{
			get
			{
				var result = base.PreviousItemIndex;

				while (result > 0 && !IsItemSelectable((ICodeDescription)List[result]))
				{
					result--;
				}

				return result;
			}
		}

		#endregion

		#region Painting an Item

		protected override int DescriptionBackgroundOverlap
		{
			get { return 11; } // help separate the line from the codes when the codebox is small
		}

		protected override bool IncludeItemInCodeWidthCalculation(ICodeDescription item)
		{
			return !(item is CategoryCodeDescriptionPair);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]

#if !WINZOR
		protected override void PaintItemText(ICodeDescription item, ZString text, Graphics graphics, Font font, Brush textBrush, Rectangle codeRectangle)
		{
			const int leftIndent = 8;
			ControlDpiScalingHelper.SetX(ref codeRectangle, codeRectangle.X + ControlDpiScalingHelper.ScaleToCurrentDpiX(leftIndent), false);
			ControlDpiScalingHelper.SetWidth(ref codeRectangle, codeRectangle.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(leftIndent), false);
			var commonText = "  (Common)"; 

			if (Helper.IsCategoryItem(item))
			{
				font = CategoryFont;

				var textWidth = TextRendererHelper.MeasureText(graphics, item.Code, font).Width;
				float textStart = codeRectangle.X;

				// draw line 1

				var x1 = ControlDpiScalingHelper.ScaleToCurrentDpiX(5);
				var y1 = (int)(codeRectangle.Y + codeRectangle.Height / 2.0f - ControlDpiScalingHelper.ScaleToCurrentDpiY(3));
				var w1 = (int)textStart - x1;
				var h1 = ControlDpiScalingHelper.ScaleToCurrentDpiY(2);
				ControlPaint.DrawBorder(graphics, ControlDpiScalingHelper.NewScaledRectangle(x1, y1, w1, h1, false), SystemColors.ControlDark, ButtonBorderStyle.Dotted);

				// draw line 2

				var x2 = (int)(textStart + textWidth) - ControlDpiScalingHelper.ScaleToCurrentDpiX(2);
				var y2 = y1;
				var w2 = codeRectangle.Right - ScrollBarWidth - x2 - ControlDpiScalingHelper.ScaleToCurrentDpiX(5);
				var h2 = h1;
				ControlPaint.DrawBorder(graphics, ControlDpiScalingHelper.NewScaledRectangle(x2, y2, w2, h2, false), SystemColors.ControlDark, ButtonBorderStyle.Dotted);

				ControlDpiScalingHelper.SetY(ref codeRectangle, codeRectangle.Y - ControlDpiScalingHelper.ScaleToCurrentDpiY(3), false);
				ControlDpiScalingHelper.SetX(ref codeRectangle, (int)textStart, false);

				// cover the dotted separator that is displayed when Descriptions are visible
				if (ParentDropEdit.ShowDescriptionInDropDown)
				{
					graphics.FillRectangle(SystemBrushes.ControlLight,
						DottedLineBounds.X, codeRectangle.Y, DottedLineBounds.Width, codeRectangle.Height);
				}
			}
			else if (Helper.IsFilterAlreadySelected(item))
			{
				textBrush = SystemBrushes.GrayText;
				if (Helper.IsExclusiveItem(item))
				{
					font = ExclusiveItemFont;
				}
			}
			else if (Helper.IsExclusiveItem(item))
			{
				textBrush = Brushes.Blue;
				font = ExclusiveItemFont;
			}

			base.PaintItemText(item, text, graphics, font, textBrush, codeRectangle);

			if (Helper.IsCommonItem(item))
			{
				var itemTextWidth = (int)Math.Ceiling(TextRendererHelper.MeasureText(graphics, text, font).Width);

				var commonTextRect = ControlDpiScalingHelper.NewScaledRectangle(codeRectangle.X + itemTextWidth, codeRectangle.Y,
						  codeRectangle.Width - codeRectangle.X - itemTextWidth, codeRectangle.Height, false);
				TextRendererHelper.DrawText(graphics, commonText, CommonItemFont, commonTextRect, SystemBrushes.ControlDarkDark);
			}
		}

		int ScrollBarWidth
		{
			get { return (ScrollBar != null) ? ScrollBar.Width : 0; }
		}

		Font CategoryFont
		{
			get
			{
				if (fCategoryFont == null)
				{
					fCategoryFont = new Font(OFont.NormalFontName, 9.0f, FontStyle.Bold);
				}
				return fCategoryFont;
			}
		}

		Font CommonItemFont
		{
			get
			{
				if (fCommonItemFont == null)
				{
					fCommonItemFont = new Font(OFont.NormalFontName, 8.0f, FontStyle.Regular);
				}
				return fCommonItemFont;
			}
		}

		Font ExclusiveItemFont
		{
			get
			{
				if (fExclusiveItemFont == null)
				{
					fExclusiveItemFont = new Font(OFont.NormalFontName, 8.0f, FontStyle.Bold);
				}
				return fExclusiveItemFont;
			}
		}

		Font fCategoryFont;
		Font fCommonItemFont;
		Font fExclusiveItemFont;
#endif

		#endregion

		#region Helper

		ZFilterStripDropHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new ZFilterStripDropHelper();
				}
				return fHelper;
			}
		}

		ZFilterStripDropHelper fHelper;

		#endregion // Helper

		[SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Justification = "Code analysis doesnt like the ?. syntax on the fonts dispose calls")]
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
#if !WINZOR
				container.Dispose();
				fCategoryFont?.Dispose();
				fCommonItemFont?.Dispose();
				fExclusiveItemFont?.Dispose();
#endif
			}

			base.Dispose(disposing);
		}
	}

	#endregion

	#region class ZFilterStripDropHelper

	public class ZFilterStripDropHelper
	{
		public bool IsCategoryItem(ICodeDescription item)
		{
			return (item is CategoryCodeDescriptionPair);
		}

		public bool IsCommonItem(ICodeDescription item)
		{
			var filter = item as ModuleFilter;
			return (filter != null && filter.IsCommon);
		}

		public bool IsExclusiveItem(ICodeDescription item)
		{
			var filter = item as ModuleFilter;
			return (filter != null && filter.IsExclusiveHelper);
		}

		public bool IsFilterAlreadySelected(ICodeDescription item)
		{
			var filter = item as ModuleFilter;
			return (filter != null && filter.IsActive);
		}

		public bool IsSeparator(ICodeDescription item)
		{
			return item != null && string.IsNullOrWhiteSpace(item.Code);
		}

		public bool IsSelectableItem(ICodeDescription item)
		{
			return item != null && !IsCategoryItem(item) && !IsSeparator(item);
		}
	}

	#endregion
}
