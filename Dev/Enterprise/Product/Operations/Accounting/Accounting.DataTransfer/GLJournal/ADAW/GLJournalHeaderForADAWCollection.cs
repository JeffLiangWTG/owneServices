using CargoWise.EntityFramework;

namespace Enterprise.Accounting.DataTransfer.GLJournals
{
	public class GLJournalHeaderForADAWCollection : NonPersistentBusinessObjectCollection<GLJournalHeaderForADAW>
	{
		public GLJournalHeaderForADAWCollection(BusinessObjectFactory factory, string headerType) : base(factory)
		{
			HeaderType = headerType;
		}

		readonly string HeaderType;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var header = new GLJournalHeaderForADAW(Factory, HeaderType);
			header.Sequence = this.Count;

			return header;
		}
	}
}
