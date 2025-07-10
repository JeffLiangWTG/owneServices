using System.Drawing;

namespace Enterprise.BufferManagement.Business
{
	public interface ISectionSubHeadingAppearance
	{
		string SectionSubHeading { get; }
		string SectionSubHeadingDetailText { get; }
		Color SectionHeadingBackgroundColor { get; }
		Color SectionHeadingForegroundColor { get; }
	}

	public class LoadingBandsSubHeadingAppearance : ISectionSubHeadingAppearance
	{
		public string SectionSubHeading
		{
			get { return " "; } // This is necessary to make the label the correct size.
		}

		public string SectionSubHeadingDetailText
		{
			get { return Res.GetString("24d9f33e-bc2b-4a24-8f42-904b603a6925", "Loading acceptability bands in the background."); }
		}

		public Color SectionHeadingBackgroundColor
		{
			get { return Color.AliceBlue; }
		}

		public Color SectionHeadingForegroundColor
		{
			get { return Color.Black; }
		}
	}

	public class EmptySectionSubHeadingAppearance : ISectionSubHeadingAppearance
	{
		public string SectionSubHeading
		{
			get { return string.Empty; }
		}

		public string SectionSubHeadingDetailText
		{
			get { return string.Empty; }
		}

		public Color SectionHeadingBackgroundColor
		{
			get { return Color.Empty; }
		}

		public Color SectionHeadingForegroundColor
		{
			get { return Color.Empty; }
		}
	}
}
