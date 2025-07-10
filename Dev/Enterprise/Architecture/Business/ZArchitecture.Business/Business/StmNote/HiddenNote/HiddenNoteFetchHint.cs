using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	internal class HiddenNoteFetchHint : IFetchHint
	{
		public HiddenNoteFetchHint(HiddenNote note)
		{
			notesParentPK = note.Parent.NotesParentPK;
			notesParentTableName = note.Parent.NotesParentTableName;
			noteDescription = note.Description;
		}
		readonly ZGuid notesParentPK;
		readonly ZString notesParentTableName;
		readonly ZString noteDescription;

		#region IFetchHint Members

		public IEnumerable<SchemaColumn> LoadWithBlobs
		{
			get
			{
				return new SchemaColumn[] { StmNoteSchema.ST_NoteData, StmNoteSchema.ST_NoteText };
			}
		}

		public ZQuery GetQuery()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(StmNoteSchema.ST_ParentID, notesParentPK);
			filter.AddToFilter(StmNoteSchema.ST_Table, notesParentTableName);
			filter.AddToFilter(StmNoteSchema.ST_Description, noteDescription);
			filter.AddToFilter(StmNoteSchema.ST_NoteType, nameof(StmNoteVisibility.DOC));
			return filter;
		}

		public string TableName
		{
			get { return "StmNote"; }
		}

		public bool IsDataHintLoaded
		{
			get { return isDataHintLoaded; }
			set { isDataHintLoaded = value; }
		}
		bool isDataHintLoaded;

		IQueryHashKey IFetchHint.GetHashKeyObject()
		{
			return new FetchHint.EnumerableHashObject { notesParentPK, noteDescription };
		}

		void IFetchHint.GenerateQuery(QueryBuilder builder)
		{
			if (builder.IsEmpty)
			{
				var mainQuery = new ZQuery();
				mainQuery.AddToFilter(StmNoteSchema.ST_Table, notesParentTableName);
				mainQuery.AddToFilter(StmNoteSchema.ST_Description, noteDescription);
				mainQuery.AddToFilter(StmNoteSchema.ST_NoteType, nameof(StmNoteVisibility.DOC));

				builder.Init(mainQuery, StmNoteSchema.ST_ParentID);
			}

			builder.AddValue(notesParentPK);
		}

		string IFetchHint.BuilderKey
		{
			get { return GetType().FullName + noteDescription; }
		}

		bool IFetchHint.IsNeeded(QueryHistoryProvider historyProvider)
		{
			return !historyProvider.IsQueryCached(TableName, GetQuery());
		}

		#endregion
	}
}

#region Test
// functionally tested by TestFetchHintDecreasesDBLoadCount in HiddenNote
#endregion
