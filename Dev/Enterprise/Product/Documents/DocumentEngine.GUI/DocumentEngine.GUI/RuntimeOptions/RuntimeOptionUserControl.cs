using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis]
	[SuppressControlRequiresTextBasher]
	internal class RuntimeOptionUserControl : ZUserControl
	{
		// Subclasses should override this
		public virtual void SetFilter(FilterField filter)
		{
			if (!ExpectedFilterType().IsAssignableFrom(filter.GetType()))
			{
				throw new ArgumentException("Filter controls of type " + GetType() + " can only be used with fields of type " + ExpectedFilterType(), nameof(filter));
			}
		}

		public int DesiredCaptionWidth => DesiredCaptionWidthCore;

		protected virtual int DesiredCaptionWidthCore
		{
			get
			{
				switch (Controls.Count)
				{
					case 1:
						return GetCaptionWidth(Controls[0]);

					case 2:
						var label = Controls.OfType<ZLabel>().Single();
						return GetCaptionWidth(label) + label.Padding.Horizontal;

					default:
						throw new FormatException(GetType().FullName + " doesn't match the standard of RuntimeOptionUserContorl");
				}
			}
		}

		public void ChangeLabelSizeForAlignment(int descriptionSize) => ChangeLabelSizeForAlignmentCore(descriptionSize);

		protected virtual void ChangeLabelSizeForAlignmentCore(int descriptionSize)
		{
			Control controller;
			var descriptionLabel = Controls.OfType<ZLabel>().FirstOrDefault();
			if (descriptionLabel != null)
			{
				ControlDpiScalingHelper.SetWidth(descriptionLabel, descriptionSize - descriptionLabel.Left, false);
				controller = Controls.Cast<Control>().Except(new[] { descriptionLabel }).Single();
			}
			else
			{
				controller = Controls[0];
			}

			ControlDpiScalingHelper.SetLeft(controller, descriptionSize, false);
			ControlDpiScalingHelper.SetWidth(controller, Width - controller.Left, false);
		}

		// Subclasses should override this
		// Can't make it abstract because of the form designer
		public virtual Type ExpectedFilterType()
		{
			throw new NotImplementedException("Subclasses must override this method to return the type of field they can bind to");
		}

		protected string EscapeMnemonics(string text)
		{
			return text.Replace("&", "&&");
		}

		protected int GetCaptionWidth(Control control)
		{
			var renderer = control.GetExtension<ILabelCaptionRenderer>();

			return renderer != null ? TextRenderer.MeasureText(renderer.Caption, renderer.Font).Width : 0;
		}
	}
}
