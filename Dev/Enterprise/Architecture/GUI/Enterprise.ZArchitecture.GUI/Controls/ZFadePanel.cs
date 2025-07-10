using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZFadePanel : ZPanel
	{
		#region Properties

		public Color FadeStartColor
		{
			get { return fadeStartColor; }
			set
			{
				fadeStartColor = value;
				Invalidate();
			}
		}
		Color fadeStartColor;

		public Color FadeEndColor
		{
			get { return fadeEndColor; }
			set
			{
				fadeEndColor = value;
				Invalidate();
			}
		}
		Color fadeEndColor;

		[DefaultValue(0f)]
		public float GradientAngle
		{
			get { return gradientAngle; }
			set
			{
				gradientAngle = value;
				Invalidate();
			}
		}
		float gradientAngle;

		[DefaultValue(null)]
		public float? GradientStartPercent
		{
			get { return gradientStartPercent; }
			set
			{
				gradientStartPercent = value;

				if (value == targetFadePercent)
				{
					EndFadeAnimation();
				}

				RequirePercentage(value);
				Invalidate();
			}
		}
		float? gradientStartPercent;

		[DefaultValue(null)]
		public float? GradientSizePercent
		{
			get { return gradientSizePercent; }
			set
			{
				gradientSizePercent = value;

				RequirePercentage(value);
				Invalidate();
			}
		}
		float? gradientSizePercent;

		public bool IsFadeContinuouslyChanging { get; private set; }

		#endregion

		#region Fancy Stuff

		public void SetFadePercentProgressively(float newFadePercent)
		{
			targetFadePercent = (float)Math.Round(newFadePercent, 2); // Math.Round lets me use floats

			if (!gradientStartPercent.HasValue)
			{
				GradientStartPercent = 0;
			}

			if (GradientStartPercent != targetFadePercent)
			{
				if (timer == null)
				{
					AnimateFade();
				}
			}
			else
			{
				// Setting the fade percent to exactly the right number at the exact right millisecond should reward the user by ending the fade animation.
				EndFadeAnimation();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Mobility", "CA1601:DoNotUseTimersThatPreventPowerStateChanges")]
		void AnimateFade()
		{
			timer = new Timer();
			timer.Interval = 10;

			timer.Tick += (s, e) =>
			{
				var startValue = (float)Math.Round(GradientStartPercent ?? 0.0f, 2); // Math.Round lets me use floats
				var step = startValue < targetFadePercent ? 0.01f : -0.01f;
				GradientStartPercent = (float)Math.Round(startValue + step, 2); // Math.Round lets me use floats
			};

			IsFadeContinuouslyChanging = true;
			timer.Start();
		}

		void EndFadeAnimation()
		{
			timer?.Dispose();
			timer = null;
			IsFadeContinuouslyChanging = false;
		}

		Timer timer;
		float targetFadePercent;

		public void ForceCompleteAnimation()
		{
			GradientStartPercent = targetFadePercent;
		}

		#endregion

		#region Paint
#if !WINZOR

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			if (FadeStartColor.IsEmpty || FadeEndColor.IsEmpty)
			{
				base.OnPaintBackground(e);
			}
			else
			{
				ColorBlend blend = null;
				if (GradientStartPercent.HasValue && GradientSizePercent.HasValue)
				{
					blend = new ColorBlend
					{
						Colors = new[] { FadeStartColor, FadeStartColor, FadeEndColor, FadeEndColor },
						Positions = new[] { 0f, GradientStartPercent.Value, GradientStartPercent.Value + GradientSizePercent.Value, 1f },
					};
				}

				this.PaintGradientBackground(FadeStartColor, FadeEndColor, GradientAngle, e.Graphics, blend);
			}
		}

#endif
		#endregion

		#region Implementation

		static void RequirePercentage(float? value)
		{
			if (value.HasValue && (value < 0f || value > 1f))
			{
				throw new ArgumentException("Value must be between 0 and 1");
			}
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			EndFadeAnimation();
			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}
