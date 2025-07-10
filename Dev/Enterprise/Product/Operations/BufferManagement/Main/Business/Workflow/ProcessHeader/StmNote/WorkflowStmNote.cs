using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class WorkflowStmNote : StmNote
	{
		public WorkflowStmNote(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ProcessHeader Parent { get; private set; }

		#region Get / Create

		public static WorkflowStmNote GetForParent(ProcessHeader processHeader)
		{
			var query = new ZQuery(StmNoteSchema.ST_ParentID, processHeader.PK);
			query.AddToFilter(StmNoteSchema.ST_Table, ProcessHeader.Schema.TableName);
			query.AddToFilter(StmNoteSchema.ST_NoteType, WorkflowStmNote.WorkflowNoteType);

			var note = processHeader.Factory.LoadTop1<WorkflowStmNote>(query);
			if (note != null)
			{
				note.Parent = processHeader;
			}

			return note;
		}

		public static WorkflowStmNote GetOrCreateForParent(ProcessHeader processHeader)
		{
			var note = GetForParent(processHeader);
			if (note == null)
			{
				note = processHeader.Factory.New<WorkflowStmNote>();
				note.Parent = processHeader;

				using (note.SuspendSettingHasChanges())
				{
					note.ST_ParentID = processHeader.PK;
				}
			}

			return note;
		}

		#endregion

		#region BusinessObject Overrides

		public override bool IsSavedByFactory
		{
			get { return IsInDatabase || !ST_NoteDataAsText.IsEmpty; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			ST_NoteType = WorkflowNoteType;
			ST_Description = WorkflowNoteType;
			ST_IsCustomDescription = true;
			ST_Table = ProcessHeader.Schema.TableName;
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (IsInDatabase && ST_NoteDataAsText.IsEmpty)
			{
				Delete();
			}
		}

		protected override StmNoteValidation GetNewValidation()
		{
			return new WorkflowStmNoteValidation(this);
		}

		#endregion

		#region StmNote Overrides

		public override CodeDescriptionPairList ST_NoteType_List
		{
			get { return Factory.GetCachedValue("WorkflowStmNote.ST_NoteType_List", () => new CodeDescriptionPairList { new CodeDescriptionPair(WorkflowNoteType, string.Empty) }); }
		}

		public override NoteTypeCollection ST_Description_List
		{
			get { return description_List ?? (description_List = new NoteTypeCollection()); }
			protected set { description_List = value; }
		}

		NoteTypeCollection description_List;

		#endregion

		public const string WorkflowNoteType = "WFL";
	}
}
