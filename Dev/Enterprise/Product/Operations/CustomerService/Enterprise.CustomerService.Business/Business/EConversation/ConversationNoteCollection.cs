using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CustomerService.Business
{
	public class ConversationNoteCollection : StmNoteCollection
	{
		public ConversationNoteCollection(IStmNoteParent master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		/// <summary>
		/// Indexer. Determines the type of elements.
		/// </summary>
		public new ConversationNote this[int index]
		{
			get { return (ConversationNote)Elements[index]; }
		}

		public virtual new ConversationNote AddNew()
		{
			return (ConversationNote)base.AddNew();
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var q = new ZQuery(StmNoteSchema.ST_NoteType, SQLComparisonOperator.Equal, nameof(StmNoteVisibility.DOC));
			q.AddToFilter(StmNoteSchema.ST_Description, SQLComparisonOperator.Equal, ConversationNote.Description);
			q.AddToFilter(StmNoteSchema.ST_NoteText, SQLComparisonOperator.NotEqual, "");
			q.IncludeBlob(StmNoteSchema.ST_NoteText);
			return q;
		}
	}
}
