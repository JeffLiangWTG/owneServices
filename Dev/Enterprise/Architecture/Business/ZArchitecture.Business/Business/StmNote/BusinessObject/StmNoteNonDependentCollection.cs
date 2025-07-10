using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public abstract class StmNoteNonDependentCollection<T> : BusinessObjectCollection<T> where T : StmNote
	{
		protected StmNoteNonDependentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected StmNoteNonDependentCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
			: base(factory, additionalFilter)
		{
		}
	}

	public class StmNoteNonDependentCollection : StmNoteNonDependentCollection<StmNote>
	{
		public StmNoteNonDependentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public StmNoteNonDependentCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
			: base(factory, additionalFilter)
		{
		}
	}
}
