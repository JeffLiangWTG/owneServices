using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class KREntryLineDetailsViewCollection : ActiveBusinessObjectCollection<KREntryLineDetailsView>
	{
		public KREntryLineDetailsViewCollection(BusinessObjectFactory factory, string entryNum = "") : base(factory)
		{
			this.entryNum = entryNum;
		}
		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			if (!entryNum.IsEmpty)
			{
				query.AddToFilter(KREntryLineDetailsViewSchema.KEL_EntryNum, entryNum);
			}

			return query;
		}
		protected override bool AllowNew => false;

		readonly ZString entryNum;
	}
}
