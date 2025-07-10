using System;
using System.Drawing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.BufferManagement.Business
{
	/// <summary>
	/// The intent of this class is to store all data that is rendered on the ChannelHeaderControl.
	/// The ChannelHeaderControl should not direct it's own loading.
	/// </summary>
	[Immutable]
	public class ChannelHeadingViewModel
	{
		public ChannelHeadingViewModel(ThumbnailViewModel thumbnails, BackgroundViewModel background, StatusViewModel status)
		{
			this.thumbnails = thumbnails;
			this.background = background;
			this.status = status;
		}

		readonly ThumbnailViewModel thumbnails;
		readonly BackgroundViewModel background;
		readonly StatusViewModel status;

		public ThumbnailViewModel Thumbnails => thumbnails;
		public BackgroundViewModel Background => background;
		public StatusViewModel Status => status;

		#region ThumbnailsViewModel

		[Immutable]
		public class ThumbnailViewModel
		{
			public ThumbnailViewModel(ImageWithTooltip statusImage, ZString timeConsideredCCR, CCRCandidacy ccrStatus, bool isPersistentlyOverloaded)
			{
				this.statusImage = statusImage;
				this.timeConsideredCCR = timeConsideredCCR;
				this.ccrStatus = ccrStatus;
				this.isPersistentlyOverloaded = isPersistentlyOverloaded;
			}

			readonly ImageWithTooltip statusImage;
			readonly ZString timeConsideredCCR;
			readonly CCRCandidacy ccrStatus;
			readonly bool isPersistentlyOverloaded;

			public ImageWithTooltip StatusImage => statusImage;
			public ZString TimeConsideredCCR => timeConsideredCCR;
			public CCRCandidacy CCRStatus => ccrStatus;
			public bool IsPersistentlyOverloaded => isPersistentlyOverloaded;
		}

		#endregion

		#region BackgroundViewModel

		[Immutable]
		public class BackgroundViewModel
		{
			public BackgroundViewModel(Color? fadeStartColor, float fadePercent, bool hasFade)
			{
				isBackgroundFade = true;
				this.fadeStartColor = fadeStartColor;
				this.fadePercent = fadePercent;
				this.hasFade = hasFade;
			}

			public BackgroundViewModel(Color backColor, Color foreColor)
			{
				this.backColor = backColor;
				this.foreColor = foreColor;
			}

			readonly bool isBackgroundFade;
			readonly Color? fadeStartColor;
			readonly float? fadePercent;
			readonly Color? backColor;
			readonly Color? foreColor;
			readonly bool hasFade;

			public Color GetFadeStartColor(Color defaultBackColor)
			{
				if (!hasFade)
				{
					return defaultBackColor;
				}
				else
				{
					return fadeStartColor ?? defaultBackColor.FadeTowardsBlack(4f);
				}
			}

			public bool IsBackgroundFade => isBackgroundFade;
			public float FadePercent => GetOrThrow(fadePercent);
			public Color BackColor => GetOrThrow(backColor);
			public Color ForeColor => GetOrThrow(foreColor);
		}

		#endregion

		#region StatusViewModel

		[Immutable]
		public class StatusViewModel
		{
			public StatusViewModel(bool isResourceChannel, ZString statusText, ZString toolTipStatusText, ZGuid riskComponentPK)
			{
				this.isResourceChannel = isResourceChannel;
				this.statusText = statusText;
				this.toolTipStatusText = toolTipStatusText;
				this.riskComponentPK = riskComponentPK;
			}

			readonly bool isResourceChannel;
			readonly ZString statusText;
			readonly ZString toolTipStatusText;
			readonly ZGuid riskComponentPK;

			public ZString StatusText => statusText;
			public ZString ToolTipStatusText => toolTipStatusText;
			public bool IsResourceChannel => isResourceChannel;
			public ZGuid RiskComponentPK => riskComponentPK;
		}

		#endregion

		#region Utils

		static T GetOrThrow<T>(T? value)
			where T : struct
		{
			if (value.HasValue)
			{
				return value.Value;
			}
			else
			{
				throw new InvalidOperationException("You should not be touching uninitialised types on this class.");
			}
		}

		#endregion
	}
}
