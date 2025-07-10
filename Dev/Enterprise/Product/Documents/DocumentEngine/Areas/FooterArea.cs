using System.Collections.Generic;
namespace Enterprise.DocumentEngine.Areas
{
	internal abstract class FooterArea : Area
	{
		public FooterArea(int start, int end, Report report, string parameters)
			: base(start, end, report, parameters)
		{
		}

		protected FooterArea() { }

		public override List<Area> Parents
		{
			get
			{
				List<Area> result = new List<Area>();
				foreach (Area area in (ParentReport.Renderer.Pages[RenderedToPageNumber - 1].Areas))
				{
					if (area is SectionBodyArea && !area.ShouldDelete)
					{
						result.Add(area);
					}
				}
				return result;
			}
		}

		public virtual int FooterRankForVisualisation
		{
			get { return 0; }
		}
	}
}
