using CargoWise.EntityFramework;

namespace Enterprise.Accounting.DataTransfer.GLJournals
{
	public class GLJournalLineForADAWCollection : NonPersistentBusinessObjectCollection<GLJournalLineForADAW>
	{
		public GLJournalLineForADAWCollection(BusinessObjectFactory factory, GLJournalHeaderForADAW header) : base(factory)
		{
			Header = header;
		}

		readonly GLJournalHeaderForADAW Header;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var line = new GLJournalLineForADAW(Factory, Header);
			line.Sequence = this.Count;

			return line;
		}
	}
}
