using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	#region Described Color

	public sealed class DescribedColor
	{
		public DescribedColor(string usage, ColorTheme theme, string colorPropertyName)
		{
			this.Usage = usage;
			this.ColorPropertyName = colorPropertyName;
			this.Theme = theme;
		}

		public readonly string Usage;
		public readonly ColorTheme Theme;
		public readonly string ColorPropertyName;

		public Color Color
		{
			get { return (Color)Theme.GetType().GetProperty(ColorPropertyName).GetValue(Theme, null); }
			set { Theme.GetType().GetProperty(ColorPropertyName).SetValue(Theme, value.ToArgb() != 0 ? Color.FromArgb(255, value) : value, null); }
		}
	}

	#endregion

	[WTG.StaticAnalysis.Annotation.CodeAlive("Reflection use in SystemDataRegistry.Instance.ColorTheme")]
	public class ColorTheme : NonPersistentBusinessObject, IColorTheme, IObsoleteValidation
	{
		public ColorTheme(ColorTheme baseColorTheme)
		{
			var properties = typeof(IColorTheme).GetProperties().Select(c => typeof(ColorTheme).GetProperty(c.Name));
			foreach (var p in properties)
			{
				p.SetValue(this, p.GetValue(baseColorTheme));
			}
		}

		public ColorTheme(MultilingualString name, bool canBeModified)
		{
			this.Name = name;
			this.CanBeModified = canBeModified;
		}

		public ColorTheme()
			: this(ResString.GetMultilingualString("6f51d938-61a4-484e-959d-64ba080836cc", "Unknown"), true)
		{
		}

		public readonly bool CanBeModified;

		#region Name

		public MultilingualString Name
		{
			get { return name; }
			set
			{
				if (value == null)
				{
					value = (NoResString)"";
				}
				if (name != value)
				{
					CheckMaximumLength(NameInfo, value.GetUnresolvedString());
					name = value;
					NameInfo.RefreshBinding();
				}
			}
		}

		MultilingualString name;

		public ZPropertyInfo NameInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(Name));
			}
		}

		public bool Name_ReadOnly
		{
			get { return !CanBeModified; }
		}

		#endregion

		#region Colors

		public DescribedColor[] ChosenColors
		{
			get
			{
				return new DescribedColor[]
				{
					new DescribedColor(Res.GetString("fa1681ee-2c7f-46C7-b4ff-000b36100326", "Main Form - Background"), this, nameof(MainFormBackgroundColor)),
					new DescribedColor(Res.GetString("6a4d31dd-0d86-4a42-8792-97d35fbb208e", "Main Form - Toolbar"), this, nameof(ToolbarColor)),
					new DescribedColor(Res.GetString("c0e3ee79-a050-4047-81d9-fed679be7abc", "Main Form - Filter Background"), this, nameof(FilterBackgroundColor)),

					new DescribedColor(Res.GetString("f5acfe93-3832-4161-aef8-9ba8178061d6", "Title Bar - Background"), this, nameof(TitleBarBackground)),
					new DescribedColor(Res.GetString("7638e5c6-0f12-47f4-bfd3-de5c0277e221", "Title Bar - Text"), this, nameof(TitleBarText)),

					new DescribedColor(Res.GetString("f979fade-9cde-4dc0-a490-59e79a67abd5", "Form - Background"), this, nameof(FormBackgroundColor)),
					new DescribedColor(Res.GetString("18e0c099-e0a8-450f-8f23-ee7e7acd3017", "Form - Tab Background"), this, nameof(TabBackgroundColor)),
					new DescribedColor(Res.GetString("a9a32771-fc85-4ca5-9330-070685ac4a3a", "Form - Buttons"), this, nameof(ButtonColor)),

					new DescribedColor(Res.GetString("51897027-5358-4cf8-bc96-183851a5819d", "Grid - Background"), this, nameof(GridBackgroundColor)),
					new DescribedColor(Res.GetString("a2c85411-8395-47b4-9d00-abef1233a535", "Grid - Alternating Row Color"), this, nameof(GridAlternatingRowColor)),
					new DescribedColor(Res.GetString("b24dd89f-3953-44e0-9ff8-6fa6b64b6bce", "Grid - Read Only Cell Color"), this, nameof(GridReadOnlyColor)),

					new DescribedColor(Res.GetString("09b6e2b3-024a-4efb-a18e-b88f02a478f3", "Navigation Bar - Background"), this, nameof(NavBarBackgroundColor)),
					new DescribedColor(Res.GetString("6dea3034-02ca-4527-8d3b-247a8c750495", "Navigation Bar - Tile Text"), this, nameof(NavBarTextColor)),
					new DescribedColor(Res.GetString("3c29bd80-b48f-4f95-a491-b362fe16a66c", "Navigation Bar - Tile Background"), this, nameof(NavBarButtonColor1)),
					new DescribedColor(Res.GetString("4e320d11-8daa-4428-bc00-f21a7776dea6", "Navigation Bar - Group Background"), this, nameof(NavBarGroupBackground1)),
					new DescribedColor(Res.GetString("24295cbf-120b-45d1-8c51-91aea50527ba", "Navigation Bar - Selected Tile"), this, nameof(NavBarGroupSelected1)),
					new DescribedColor(Res.GetString("d75d6f96-2480-4f38-8fb5-0235c2ebe8a1", "Navigation Bar - Group Header Background"), this, nameof(NavBarGroupHeaderBackground)),
					new DescribedColor(Res.GetString("214bb347-634b-455b-878a-c79fb3e3eb8e", "Navigation Bar - Recent/Favorite Background"), this, nameof(NavBarRecentPanelBackground))
				};
			}
		}

		public virtual Color FormBackgroundColor { get; set; }
		public virtual Color TabBackgroundColor { get; set; }
		public virtual Color MainFormBackgroundColor { get; set; }
		public virtual Color GridBackgroundColor { get; set; }
		public virtual Color FilterBackgroundColor { get; set; }
		public virtual Color GridAlternatingRowColor { get; set; }
		public virtual Color GridReadOnlyColor { get; set; }
		public virtual Color ToolbarColor { get; set; }
		public virtual Color NavBarBackgroundColor { get; set; }
		public virtual Color NavBarTextColor { get; set; }
		public virtual Color NavBarButtonColor1 { get; set; }
		public virtual Color NavBarGroupBackground1 { get; set; }
		public virtual Color NavBarGroupSelected1 { get; set; }
		public virtual Color ButtonColor { get; set; }
		public virtual Color NavBarGroupHeaderBackground { get; set; }
		public virtual Color NavBarRecentPanelBackground { get; set; }
		public virtual Color TitleBarBackground
		{
			get => (titleBarBackground.IsEmpty || titleBarBackground.ToArgb().Equals(SystemColors.Control.ToArgb())) ? NavBarButtonColor1 : titleBarBackground;
			set => titleBarBackground = value;
		}
		Color titleBarBackground;

		public virtual Color TitleBarText
		{
			get => (titleBarText.IsEmpty || titleBarText.ToArgb().Equals(SystemColors.Control.ToArgb())) ? NavBarTextColor : titleBarText;
			set => titleBarText = value;
		}
		Color titleBarText;

		#endregion
	}
}
