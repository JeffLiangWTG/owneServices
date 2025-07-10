using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public class StmNoteCollection : DependentBusinessObjectCollection<StmNote, BusinessObject>
	{
		public StmNoteCollection(IStmNoteParent master, BusinessObjectFactory factory)
			: this(master, factory, false)
		{
		}

		internal StmNoteCollection(IStmNoteParent master, BusinessObjectFactory factory, bool allowMasterFactoryToBeDifferent)
			: base((BusinessObject)master, factory, allowMasterFactoryToBeDifferent)
		{
		}

		#region Additional Filter

		protected override ZQuery CreateAdditionalFilter()
		{
			var query = new StmNoteQuery();
			query.AddToFilter(StmNoteSchema.ST_Table, Master.TableName);
			return query;
		}

		#endregion

		#region Hooking up Relationships

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return StmNoteSchema.ST_ParentID; }
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			child[StmNote.Schema.ST_Table] = Master.TableName;
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			StmNote note = (StmNote)child;
			note.Master = (IStmNoteParent)Master;
			base.SetCollectionRelationships(note);
		}

		#endregion

		#region Deleting Element

		public event EventHandler<EventArgs> DeleteNoteNotAllowed;

		void OnDeleteNoteNotAllowed(BusinessObject elementToDelete)
		{
			if (DeleteNoteNotAllowed != null)
			{
				DeleteNoteNotAllowed(elementToDelete, EventArgs.Empty);
			}
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			bool hasEnvironment = false;

			if (EnvProxy.Instance == null)
			{
				ErrorReporter.ReportOnce("RemoveAndDeleteNote_NullEnvironment", "EnvProxy.Instance is not initialized.");
			}
			else if (EnvProxy.Instance.Security == null)
			{
				ErrorReporter.ReportOnce("RemoveAndDeleteNote_NullEnvironment",
					"EnvProxy.Instance.Security is not initialized. EnvProxy.Instance is of type " + EnvProxy.Instance.GetType().FullName);
			}
			else if (EnvProxy.Instance.Security.NotesDelete == null)
			{
				ErrorReporter.ReportOnce("RemoveAndDeleteNote_NullEnvironment",
					"EnvProxy.Instance.Security.NotesDelete is not initialized. EnvProxy.Instance is of type " + EnvProxy.Instance.GetType().FullName);
			}
			else
			{
				hasEnvironment = true;
			}

			if (hasEnvironment && !EnvProxy.Instance.Security.NotesDelete.IsAllowed)
			{
				OnDeleteNoteNotAllowed(elementToDelete);
			}
			else
			{
				base.RemoveAndDelete(elementToDelete);
			}
		}

		#endregion
	}
}
