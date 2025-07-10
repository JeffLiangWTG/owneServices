using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class ContentRefreshedEventArgs : EventArgs
	{
		public ContentRefreshedEventArgs(bool requiresFullRefresh)
		{
			RequiresFullRefresh = requiresFullRefresh;
		}

		public bool RequiresFullRefresh { get; private set; }
	}

	[DebuggerDisplay("Row={Row} Col={Column} Type={ContentType} Label={Label} Zone={Zone} TimeIndex={TimeIndex}")]
	public class CellContent
	{
		public CellContent(int row, int column, CellContentType contentType, BMComponentSectionConfiguration sectionConfiguration = null, List<ZGuid> preConstraintPKs = null, List<ZGuid> postConstraintPKs = null, List<ZGuid> constraintPKs = null)
		{
			Row = row;
			Column = column;
			ContentType = contentType;
			TimeIndex = -1;

			this.PreConstraintPKs = preConstraintPKs;
			this.PostConstraintPKs = postConstraintPKs;
			this.ConstraintPKs = constraintPKs;

			if (sectionConfiguration != null)
			{
				Orientation = sectionConfiguration.OrientationValue;
				ShowChildComponentZones = sectionConfiguration.ShowChildComponentZones;
			}
		}

		public List<ZGuid> PreConstraintPKs { get; }
		public List<ZGuid> PostConstraintPKs { get; }
		public List<ZGuid> ConstraintPKs { get; }

		public int Row { get; private set; }
		public int Column { get; private set; }
		public CellContentType ContentType { get; private set; }
		public string Label { get; set; }
		public bool IsBoldLabel { get; set; }
		public string BorderThickness { get; set; }
		public CardSortType CardSortType { get; set; }
		public CellContent CollapsedToCell { get; set; }

		public int PrimaryAxis
		{
			get { return Orientation == BMBoardSectionOrientation.Vertical ? Column : Row; }
		}

		public int SecondaryAxis
		{
			get { return Orientation == BMBoardSectionOrientation.Vertical ? Row : Column; }
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "This isn't used for pixel values.")]
		public Point Coordinates => new Point(Column, Row);

		internal BMBoardSectionOrientation Orientation { get; set; }
		bool ShowChildComponentZones { get; set; }

		public int? Zone
		{
			get { return zone; }
			set
			{
				zone = value;

				if (zone != null && (!ContentType.In(CellContentType.CCRHeading, CellContentType.SubComponentZoneHeading) || zone == 0))
				{
					if (Label.IsNullOrEmpty())
					{
						Label = Res.GetString("dfdcdb78-9240-4d33-b94b-63b6ec0ebbd3", "Zone {0}", zone);
					}
				}
			}
		}
		int? zone;

		public Dictionary<ZGuid, int> SubComponentZones
		{
			get
			{
				if (subComponentZones == null)
				{
					subComponentZones = new Dictionary<ZGuid, int>();
				}
				return subComponentZones;
			}
		}
		Dictionary<ZGuid, int> subComponentZones;

		public int? CCRHeaderZone { get; set; }
		public bool IsLastAgeIndexForZone { get; set; }
		public int TimeIndex { get; set; }
		public decimal TimePercent { get; set; }
		public bool IsLastCell { get; set; }
		public bool IsFirstCell { get; set; }
		public ConstraintStatus ConstraintStatus { get; set; }
		public int? ZoneOverride { get; set; }

		public bool IsInPreConstraint => PreConstraintPKs != null && SubComponentZones.Any() && PreConstraintPKs.Any(c => c.In(SubComponentZones.Keys));
		public bool IsInPostConstraint => PostConstraintPKs != null && SubComponentZones.Any() && PostConstraintPKs.Any(c => c.In(SubComponentZones.Keys));

		public bool IsConstraintAtRisk(IEnumerable<ZGuid> componentPKs, bool isPreConstraintPosition)
		{
			return SubComponentZones.Where(zone => componentPKs.Contains(zone.Key)).Any(zone => zone.Value < 2)
				|| (ShowChildComponentZones && IsInPostConstraint && isPreConstraintPosition);
		}

		public Color? BackColor
		{
			get { return backColor; }
			set
			{
				backColor = value;
				OnBackgroundColorChanged();
			}
		}
		Color? backColor;

		public Color? BackgroundFadeColor
		{
			get { return backgroundFadeColor; }
			set
			{
				backgroundFadeColor = value;
				OnBackgroundColorChanged();
			}
		}
		Color? backgroundFadeColor;

		public Color? ForeColor { get; set; }

		public IVisualBoardChannel Channel { get; set; }
		public IVisualBoardChannel SecondaryChannel { get; set; }
		public TimeSpan TimeInCell { get; set; }
		public ZGuid SubComponentHeadingPK { get; set; }

		void OnBackgroundColorChanged()
		{
			BackgroundColorChanged?.Invoke(this, EventArgs.Empty);
		}

		public event EventHandler BackgroundColorChanged;

#if DEBUG
		public
#else
		internal
#endif
		void OnCardsRefreshed(bool fullRefresh = true)
		{
			ContentRefreshed?.Invoke(this, new ContentRefreshedEventArgs(fullRefresh));
		}

		public event EventHandler<ContentRefreshedEventArgs> ContentRefreshed;

		public BMBoardSectionOrientation GetHeaderOrientation(BMComponentSectionConfiguration sectionConfiguration)
		{
			if (!ContentType.IsHeadingType())
			{
				throw new InvalidOperationException("Can only get the orientation of header cells");
			}

			if (sectionConfiguration.IsBuffer)
			{
				if ((new[] { CellContentType.AgeHeading, CellContentType.ZoneHeading, CellContentType.SubComponentZoneHeading, CellContentType.CCRHeading }).Contains(ContentType))
				{
					return GetOpposite(sectionConfiguration.OrientationValue);
				}
				else
				{
					return sectionConfiguration.OrientationValue;
				}
			}
			else
			{
				if (SecondaryChannel != null || (new[] { CellContentType.AgeHeading, CellContentType.SubComponentZoneHeading, CellContentType.CCRHeading }).Contains(ContentType))
				{
					return GetOpposite(sectionConfiguration.OrientationValue);
				}
				else
				{
					return sectionConfiguration.OrientationValue;
				}
			}
		}

		static BMBoardSectionOrientation GetOpposite(BMBoardSectionOrientation orientation)
		{
			return orientation == BMBoardSectionOrientation.Horizontal ? BMBoardSectionOrientation.Vertical : BMBoardSectionOrientation.Horizontal;
		}

		internal bool HasSameChannel(IVisualBoardChannel otherChannel)
		{
			var channel = Channel;

			if (channel == null)
			{
				return false;
			}
			else
			{
				return channel.EntityType == otherChannel.EntityType
					&& channel.ChannelEntityCode == otherChannel.ChannelEntityCode
					&& channel.EntityPK == otherChannel.EntityPK;
			}
		}
	}
}
