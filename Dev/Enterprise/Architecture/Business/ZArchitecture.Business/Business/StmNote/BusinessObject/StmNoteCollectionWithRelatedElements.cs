using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public class StmNoteCollectionWithRelatedElements : StmNoteNonDependentCollection, IBusinessObjectCollectionWithRelatedElements
	{
		public StmNoteCollectionWithRelatedElements(IStmNoteParent master)
			: base(((BusinessObject)master).Factory)
		{
			this.Master = master;
		}

		#region IBusinessObjectCollectionWithRelatedElements Members

		public void RemoveAllRelatedElements()
		{
			using (SuspendListChanged())
			{
				foreach (StmNote note in Elements.ToArray())
				{
					if (Contains(note) && IsRelatedNote(note))
					{
						Remove(note);
					}
				}
			}
		}

		protected virtual bool IsRelatedNote(StmNote note)
		{
			return note.ST_ParentID != ((BusinessObject)Master).PK;
		}

		protected readonly IStmNoteParent Master;

		#endregion
	}
}
