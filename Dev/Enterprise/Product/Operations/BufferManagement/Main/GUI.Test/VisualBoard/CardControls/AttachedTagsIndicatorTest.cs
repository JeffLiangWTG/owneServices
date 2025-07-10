using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;
using Moq;

namespace Enterprise.BufferManagement.GUI.Test
{
	class AttachedTagsIndicatorTest : BMSTestCaseWithFactory
	{
		public void TestColor()
		{
			var parent = new Mock<ITaskCardComponentParent>();
			parent.Setup(m => m.IsPreview).Returns(false);
			parent.Setup(m => m.Task).Returns(Factory.New<ProcessTask>());
			parent.Setup(m => m.CardContent).Returns(MockCardContent(Color.Aqua));
			using (var control = new AttachedTagsIndicator(parent.Object))
			{
				control.SetupTags();

				AssertEquals(1, control.Tags.Count);
				AssertEquals(Color.Aqua, control.Tags[0]);
				AssertEquals(true, control.Visible);
			}
		}

		public void TestPreviewDefaults()
		{
			var parent = new Mock<ITaskCardComponentParent>();
			parent.Setup(m => m.Task).Returns(Factory.New<ProcessTask>());
			parent.Setup(m => m.IsPreview).Returns(true);
			using (var control = new AttachedTagsIndicator(parent.Object))
			{
				control.SetupTags();

				AssertEquals(true, control.Visible);
				AssertEquals(4, control.Tags.Count);
				AssertEquals(Color.Blue, control.Tags[0]);
			}
		}

		/** 
		 * These tests are !WINZOR out for two reasons 
		 * - These tests verify Tag color been drawn on screen using DrawToBitmapFixed, which is not available in Winzor.
		 * - Winzor has its own implementation of AttachedTagsIndicator that uses background color, its tested in AttachedTagsIndicatorTest under the Winzor project .
		**/
#if !WINZOR
		public void TestWidth_UnderFourTags()
		{
			var parent = new Mock<ITaskCardComponentParent>();
			parent.Setup(m => m.IsPreview).Returns(false);
			parent.Setup(m => m.Task).Returns(Factory.New<ProcessTask>());
			parent.Setup(m => m.CardContent).Returns(MockCardContent(Color.Black, Color.Blue));
			using (var control = new AttachedTagsIndicator(parent.Object))
			{
				control.SetupTags();

				AssertEquals(true, control.Visible);
				AssertEquals(2, control.Tags.Count);

				using (var bitmap = new Bitmap(control.Width, control.Height, PixelFormat.Format32bppPArgb))
				{
					control.DrawToBitmapFixed(bitmap, control.ClientRectangle);

					var offsetProvider = new Func<int, int>(index => index * control.Width / 4);

					var pixelGetter = new Func<int, Color>(position => bitmap.GetPixel(offsetProvider(position) + ControlDpiScalingHelper.ScaleToCurrentDpiY(10), ControlDpiScalingHelper.ScaleToCurrentDpiX(10)));

					AssertColorAtPosition(control, 0, pixelGetter);
					AssertColorAtPosition(control, 1, pixelGetter);
				}
			}
		}

		public void TestWidth_OverFourTags()
		{
			var parent = new Mock<ITaskCardComponentParent>();
			parent.Setup(m => m.IsPreview).Returns(false);
			parent.Setup(m => m.Task).Returns(Factory.New<ProcessTask>());
			parent.Setup(m => m.CardContent).Returns(MockCardContent(Color.AliceBlue, Color.SaddleBrown, Color.Honeydew, Color.IndianRed, Color.Tan));
			using (var control = new AttachedTagsIndicator(parent.Object))
			{
				control.SetupTags();

				using (var bitmap = new Bitmap(control.Width, control.Height, PixelFormat.Format32bppPArgb))
				{
					control.DrawToBitmapFixed(bitmap, control.ClientRectangle);

					var offsetProvider = new Func<int, int>(index => index * control.Width / 5);

					var pixelGetter = new Func<int, Color>(position => bitmap.GetPixel(offsetProvider(position) + ControlDpiScalingHelper.ScaleToCurrentDpiY(10), ControlDpiScalingHelper.ScaleToCurrentDpiX(10)));

					AssertColorAtPosition(control, 0, pixelGetter);
					AssertColorAtPosition(control, 1, pixelGetter);
					AssertColorAtPosition(control, 2, pixelGetter);
					AssertColorAtPosition(control, 3, pixelGetter);
					AssertColorAtPosition(control, 4, pixelGetter);
				}
			}
		}

		public void TestHeight_UnderFourTags()
		{
			var parent = new Mock<ITaskCardComponentParent>();
			parent.Setup(m => m.IsPreview).Returns(false);
			parent.Setup(m => m.Task).Returns(Factory.New<ProcessTask>());
			parent.Setup(m => m.CardContent).Returns(MockCardContent(Color.AliceBlue, Color.SaddleBrown));
			using (var control = new AttachedTagsIndicator(parent.Object, BMBoardSectionOrientation.Vertical))
			{
				control.SetupTags();

				AssertEquals(true, control.Visible);
				AssertEquals(2, control.Tags.Count);

				using (var bitmap = new Bitmap(control.Width, control.Height, PixelFormat.Format32bppPArgb))
				{
					control.DrawToBitmapFixed(bitmap, control.ClientRectangle);

					var offsetProvider = new Func<int, int>(index => index * control.Height / 4);

					var pixelGetter = new Func<int, Color>(position => bitmap.GetPixel(ControlDpiScalingHelper.ScaleToCurrentDpiY(10), offsetProvider(position) + ControlDpiScalingHelper.ScaleToCurrentDpiX(10)));

					AssertColorAtPosition(control, 0, pixelGetter);
					AssertColorAtPosition(control, 1, pixelGetter);
				}
			}
		}

		public void TestHeight_OverFourTags()
		{
			var parent = new Mock<ITaskCardComponentParent>();
			parent.Setup(m => m.IsPreview).Returns(false);
			parent.Setup(m => m.Task).Returns(Factory.New<ProcessTask>());
			parent.Setup(m => m.CardContent).Returns(MockCardContent(Color.AliceBlue, Color.SaddleBrown, Color.Honeydew, Color.IndianRed, Color.Tan));
			using (var control = new AttachedTagsIndicator(parent.Object, BMBoardSectionOrientation.Vertical))
			{
				control.SetupTags();

				AssertEquals(true, control.Visible);
				AssertEquals(5, control.Tags.Count);

				using (var bitmap = new Bitmap(control.Width, control.Height, PixelFormat.Format32bppPArgb))
				{
					control.DrawToBitmapFixed(bitmap, control.ClientRectangle);

					var offsetProvider = new Func<int, int>(index => index * control.Height / 5);

					var pixelGetter = new Func<int, Color>(position => bitmap.GetPixel(ControlDpiScalingHelper.ScaleToCurrentDpiY(10), offsetProvider(position) + ControlDpiScalingHelper.ScaleToCurrentDpiX(10)));

					AssertColorAtPosition(control, 0, pixelGetter);
					AssertColorAtPosition(control, 1, pixelGetter);
					AssertColorAtPosition(control, 2, pixelGetter);
					AssertColorAtPosition(control, 3, pixelGetter);
					AssertColorAtPosition(control, 4, pixelGetter);
				}
			}
		}
		
		static void AssertColorAtPosition(AttachedTagsIndicator control, int position, Func<int, Color> pixelGetter)
		{
			CombineAssertions(() =>
			{
				var source = control.Tags[position];
				Color target = pixelGetter(position);
				AssertEquals("R:", source.R, target.R);
				AssertEquals("G:", source.G, target.G);
				AssertEquals("B:", source.B, target.B);
			});
		}
#endif

		public void TestVisible_NoColor()
		{
			var parent = new Mock<ITaskCardComponentParent>();
			parent.Setup(m => m.IsPreview).Returns(false);
			parent.Setup(m => m.Task).Returns(Factory.New<ProcessTask>());
			parent.Setup(m => m.CardContent).Returns(MockCardContent());
			using (var control = new AttachedTagsIndicator(parent.Object))
			{
				control.SetupTags();

				AssertEquals(0, control.Tags.Count);
				AssertEquals("Being invisible causes repositioning glitches.", true, control.Visible);
			}
		}

		#region Helpers

		ICardContent MockCardContent(params Color[] colors)
		{
			var definition = VisualBoardsTestHelper.CreateTagDefinition(Factory, "AZE");

			for (int i = 0; i < colors.Length; i++)
			{
				VisualBoardsTestHelper.CreateTagMagnitude(definition, "CZ" + i, color: colors[i]);
			}

			Factory.Save();

			return new TinyCardContent
			{
				ApplicableTagMagnitudes = new HashSet<ZGuid>(definition.Magnitudes.Select(m => m.PK)).ToImmutableHashSet(),
				Definitions = new TagDefinitionCache(Factory),
			};
		}

		class TinyCardContent : ICardContent
		{
			public ImmutableHashSet<ZGuid> ApplicableTagMagnitudes { get; set; }
			public object Bindable { get; set; }
			public Color BorderColor { get; set; }
			public SizedButtonBorderStyle BorderStyle { get; set; }
			public ICardCapacityDto CapacityDto { get; set; }
			public CardType CardType { get; set; }
			public TagDefinitionCache Definitions { get; set; }
			public ZGuid Identifier { get; set; }
			public bool IsCurrent { get; set; }
			public ZDateTime LastEditTime { get; set; }
			public ZString NoteText { get; set; }
			public ZGuid TaskIdentifier { get; set; }
			public ITaskOrderable TaskOrderable { get; set; }
			public ZGuid WorkflowIdentifier { get; set; }
			public ZString WorkflowType { get; set; }
			public TValue GetCustomAttribute<TValue>(StaticControlProperty key)
			{
				return default(TValue);
			}

			public ZString DisplayTextForDebugging { get; set; }
		}

		#endregion
	}
}
