using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class MockZDropForm : ZDropForm
	{
		public MockZDropForm(IDropFormParent parentDropEdit)
			: base(parentDropEdit)
		{ }

		public bool NeedThrowException;

		public void OnPaintExposed(PaintEventArgs e)
		{
			IsPaintDescriptionBackgroundCalled = false;
			OnPaint(e);
		}
#if !WINZOR
		protected override void ThrowExceptionForTestIfNeeded()
		{
			if (NeedThrowException)
			{
				throw new Exception("TEST ERROR");
			}
		}

		protected override void PaintDescriptionBackground(Graphics graphics)
		{
			IsPaintDescriptionBackgroundCalled = true;
		}
#endif
		public bool IsPaintDescriptionBackgroundCalled { get; private set; }
#if !WINZOR
		public void PaintItemExposed(Graphics graphics, ICodeDescription item, Rectangle codeRect, bool isHighlighted, bool isHighlightedForMouseMove)
		{
			IsPaintDarkenedDescriptionItemCalled = false;
			ItemText = string.Empty;
			PaintItem(graphics, item, codeRect, isHighlighted, 0, isHighlightedForMouseMove);
		}

		protected override void PaintDarkenedDescriptionItem(Graphics graphics, Rectangle selectedItemRect, Brush descriptionBrush)
		{
			IsPaintDarkenedDescriptionItemCalled = true;
			DescriptionBrush = descriptionBrush;
		}
#endif
		public bool IsPaintDarkenedDescriptionItemCalled { get; private set; }
#if !WINZOR
		protected override void PaintItemText(ICodeDescription item, ZString text, Graphics graphics, Font font, Brush textBrush, Rectangle codeRectangle)
		{
			ItemText = text;
		}
#endif
		public ZString ItemText { get; private set; }
		public Brush DescriptionBrush { get; set; }

		public float DescriptionWidthExposed()
		{
			return DescriptionWidth;
		}

		public float CodeWidthExposed()
		{
			return CodeWidth;
		}
#if !WINZOR
		public string ItemSpacerExposed()
		{
			return ItemSpacer;
		}

		public HScrollBar HorizontalScrollBar_Exposed()
		{
			return HorizontalScrollBar;
		}
#endif
	}
}
