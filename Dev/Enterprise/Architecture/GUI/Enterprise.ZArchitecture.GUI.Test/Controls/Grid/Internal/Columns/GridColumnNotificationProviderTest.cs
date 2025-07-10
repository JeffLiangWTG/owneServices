using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Core.Forms.Testing
{
	sealed class GridColumnNotificationProviderTest : NUnit.Framework.TestCase
	{
		public void TestNotificationIconHeightAndWidth()
		{
			var provider = new DummyGridColumnNotificationProvider(new DummyColumnStyle());
			AssertEquals(12, provider.NotificationIconHeight);
			AssertEquals(12, provider.NotificationIconWidth);
		}

		public void TestConstruction()
		{
			var style = new DummyColumnStyle();
			var provider = new DummyGridColumnNotificationProvider(style);
			AssertEquals(style, provider.ColumnStyle);
		}

		public void TestShrinkBoundsForNotificationIcon()
		{
			var provider = new DummyGridColumnNotificationProvider(new DummyColumnStyle());
			var rect = provider.ShrinkBoundsForNotificationIcon(new Rectangle(100, 200, 300, 400));
			AssertEquals(new Rectangle(112, 200, 288, 400), rect);
		}

		class DummyColumnStyle : IGridColumnStyle
		{
			#region IGridColumnStyle Members

			public string MappingName
			{
				get { throw new Exception("The method or operation is not implemented."); }
			}

			public bool HeaderTextWasDefaulted
			{
				get { throw new Exception("The method or operation is not implemented."); }
			}

			public DataGrid Grid
			{
				get { throw new Exception("The method or operation is not implemented."); }
			}

			public ResourceStringData CaptionResourceString
			{
				get; set;
			}

			#endregion
		}

		class DummyGridColumnNotificationProvider : GridColumnNotificationProvider
		{
			public DummyGridColumnNotificationProvider(IGridColumnStyle colStyle)
				: base(colStyle)
			{ }

			public override Rectangle PaintNotificationIconIfRequiredAndReturnRemainingAreaForPainting(Graphics graphics, CurrencyManager source, int paintingRowNum, Rectangle cellBounds, Brush backBrush)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			public override bool DoesCellHaveAnyNotifications(CurrencyManager source, int rowNum)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			public override Rectangle AdjustEditControlBoundsForNotificationIconIfRequired(Rectangle originalBounds, CurrencyManager source, int rowNum)
			{
				throw new Exception("The method or operation is not implemented.");
			}

#if !WINZOR
			public override Brush DecideBackgroundColorBrush(CurrencyManager source, int paintingRowNum, Brush defaultBrush)
			{
				throw new NotImplementedException();
			}
#endif
		}
	}
}
