using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class AttachedTagsIndicator : ZUserControl
	{
		public AttachedTagsIndicator(ITaskCardComponentParent componentParent, BMBoardSectionOrientation controlOrientation = BMBoardSectionOrientation.Horizontal)
		{
			orientation = controlOrientation;
			parent = componentParent;
			Tags = Array.Empty<Color>();
		}

		readonly ITaskCardComponentParent parent;
		readonly BMBoardSectionOrientation orientation;

		public IList<Color> Tags { get; private set; }

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource != null)
			{
				SetupTags();
			}
		}

		public void SetupTags()
		{
			if (!IsDisposed && !Disposing)
			{
				var cardContent = parent.CardContent;

				var colors = parent.IsPreview ? new[] { Color.Blue, Color.Green, Color.Yellow, Color.Brown } : TagProvider.GetOrderedColors(cardContent.Definitions, cardContent.ApplicableTagMagnitudes);

				if (colors.Length != Tags.Count || !colors.SequenceEqual(Tags))
				{
					Invalidate();
					Tags = colors;
				}
			}
		}

#if !WINZOR

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			if (Tags.Count > 0)
			{
				var displacement = GetDisplacement(Tags.Count);

				for (int i = 0; i < Tags.Count; i++)
				{
					DrawColorBox(e.Graphics, displacement, i);
				}
			}
		}

#endif

		int GetDisplacement(int colorCount)
		{
			var displacement = orientation == BMBoardSectionOrientation.Horizontal ? Width : Height;
			return Math.Min(displacement / colorCount, displacement / 4);
		}

#if !WINZOR

		void DrawColorBox(Graphics graphics, int displacement, int position)
		{
			using (var brush = new SolidBrush(Tags[position]))
			using (var pen = new Pen(brush))
			{
				if (orientation == BMBoardSectionOrientation.Horizontal)
				{
					var rectangle = ControlDpiScalingHelper.NewScaledRectangle(x: displacement * position, y: 0, width: displacement, height: Height, isInStandardDpi: false);
					graphics.FillRectangle(brush, rectangle);
					graphics.DrawRectangle(pen, rectangle);
				}
				else
				{
					var rectangle = ControlDpiScalingHelper.NewScaledRectangle(x: 0, y: displacement * position, width: Width, height: displacement, isInStandardDpi: false);
					graphics.FillRectangle(brush, rectangle);
					graphics.DrawRectangle(pen, rectangle);
				}
			}
		}

#endif
	}
}
