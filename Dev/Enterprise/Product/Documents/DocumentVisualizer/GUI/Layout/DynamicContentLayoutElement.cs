using System.Collections.Generic;
using System.Drawing;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Presentation;

#if WINZOR
using System.Windows.Forms;
#endif

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class DynamicContentLayoutElement : LayoutElement<DynamicContent>, IDynamicContentLayoutElement, ILayoutElementInternals
	{
		public DynamicContentLayoutElement(DynamicContent element)
			: base(element)
		{
		}

		public DynamicContent Element => element;
		IText ITextLayoutElement.Text => Element;
#if WINZOR
		public Control ContentControl { get; set; }
#endif
		protected override IPainter Painter
		{
			get { return  painter ?? (painter = new DynamicContentPainter(Element)); }
		}

		DynamicContentPainter painter;

		IMacroScope IDynamicContentLayoutElement.Scope
		{
			get { return Element.Scope; }
		}

		public override IPageView PageView
		{
			get
			{
				return pageView;
			}
			set
			{
				if (pageView != value)
				{
					if (pageView != null)
					{
						element.RemoveOnValueChangedAction(RefreshPageOnValueChange);
					}

					pageView = value;

					if (value != null)
					{
						element.AddOnValueChangedAction(RefreshPageOnValueChange);
					}
				}
			}
		}

		IPageView pageView;

		void RefreshPageOnValueChange()
		{
			pageView?.Refresh();
		}

		IDictionary<string, IDynamicData> IDynamicContentLayoutElement.EditableData
		{
			get { return Element.EditableData; }
		}

		bool IDynamicContentLayoutElement.HasDynamicContent
		{
			get { return Element.HasDynamicContent; }
		}

		void IDynamicContentLayoutElement.CancelOverride()
		{
			Element.CancelOverride();
		}

		#region ILayoutElementInternals memebers

		string ILayoutElementInternals.Text => element.Content;
		IDocumentCell ILayoutElementInternals.Cell => element.Cell;
		RectangleF ILayoutElementInternals.PaintArea => painter?.PaintArea ?? RectangleF.Empty;
		RectangleF ILayoutElementInternals.LayoutArea => painter?.ContentLayoutRectangle ?? RectangleF.Empty;
		IFont ILayoutElementInternals.Font => element.Cell?.Format?.Font;
		IFont ILayoutElementInternals.DrawFont => painter?.DrawFont;
		float ILayoutElementInternals.Scale => painter?.Scale ?? 0f;

		#endregion
	}
}
