using System.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUCChapterCollection : DependentBusinessObjectCollection<AUCChapter, AUCSection>
	{
		public AUCChapterCollection(AUCSection parentSection, BusinessObjectFactory factory)
			: base(parentSection, factory)
		{
		}

		public override void Load()
		{
			base.Load();
			Sort(AUCChapter.Schema.UH_Chapter, ListSortDirection.Ascending);
		}
	}
}
