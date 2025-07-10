using CargoWise.EntityFramework;

namespace Enterprise.Accounting.DataTransfer.GLJournals
{
	public class GLJournalLineSubAccountForADAWCollection : NonPersistentBusinessObjectCollection<GLJournalLineSubAccountForADAW>
	{
		public GLJournalLineSubAccountForADAWCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new GLJournalLineSubAccountForADAW(Factory);
		}
	}
}
