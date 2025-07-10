using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	internal class StmNoteCollectionViewNonPrivateNotesWithContext : BusinessObjectCollectionView<StmNote>, IBusinessObjectCollectionWithMaster
	{
		public StmNoteCollectionViewNonPrivateNotesWithContext(Notes notes, StmNoteContexts validNoteContexts, ZGuid parentPKToExcludeNotes)
			: base(notes.ElementsInternal)
		{
			ParentPKToExcludeNotes = parentPKToExcludeNotes;
			ValidNoteContexts = validNoteContexts;
			Rebuild();
		}

		public ZGuid ParentPKToExcludeNotes { get; private set; }

		#region Rebuilding the View

		protected override void RebuildOnConstruction()
		{
			// don't rebuild as we have not yet set this.ValidNoteContexts
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			StmNote note = (StmNote)element;
			if (ParentPKToExcludeNotes.IsValid && note.ST_ParentID == ParentPKToExcludeNotes)
			{
				return false;
			}

			bool result = false;
			if (note.IsInternalOrClientVisibleOrAgentVisible)
			{
				if (note.ST_NoteContext == StmNoteContextUtils.StmNoteContextsAllToString)
				{
					result = true;
				}
				else if (note.ST_NoteContext.Length == 3
					&& Enum.IsDefined(typeof(StmNoteContextModule), note.ST_NoteContext.Substring(0, 1).ToString())
					&& Enum.IsDefined(typeof(StmNoteContextDirection), note.ST_NoteContext.Substring(1, 1).ToString())
					&& Enum.IsDefined(typeof(StmNoteContextFreightMode), note.ST_NoteContext.Substring(2, 1).ToString()))
				{
					StmNoteContexts noteContext = new StmNoteContexts();
					noteContext.Module = (StmNoteContextModule)Enum.Parse(typeof(StmNoteContextModule), note.ST_NoteContext.Substring(0, 1), true);
					noteContext.Direction = (StmNoteContextDirection)Enum.Parse(typeof(StmNoteContextDirection), note.ST_NoteContext.Substring(1, 1), true);
					noteContext.FreightMode = (StmNoteContextFreightMode)Enum.Parse(typeof(StmNoteContextFreightMode), note.ST_NoteContext.Substring(2, 1), true);

					if (!((StmNoteContextModule.W & ValidNoteContexts.Module) == StmNoteContextModule.W && noteContext.Module == StmNoteContextModule.A)
						&& (noteContext.Module & ValidNoteContexts.Module) == noteContext.Module
						&& (noteContext.Direction & ValidNoteContexts.Direction) == noteContext.Direction
						&& (noteContext.FreightMode & ValidNoteContexts.FreightMode) == noteContext.FreightMode)
					{
						result = true;
					}
				}
			}

			return result;
		}

		internal StmNoteContexts ValidNoteContexts
		{
			get { return fValidNoteContexts; }
			set { fValidNoteContexts = value; }
		}

		StmNoteContexts fValidNoteContexts;

		#endregion

		#region IBusinessObjectCollectionWithMaster Members

		public BusinessObject Master
		{
			get
			{
				switch (CollectionToFilter)
				{
					case IDependentBusinessObjectCollection dependentBusinessObjectCollection:
						return dependentBusinessObjectCollection.Master;

					case IBusinessObjectCollectionWithMaster collectionWithMaster:
						return collectionWithMaster.Master;

					default:
						return null;
				}
			}
		}

		#endregion
	}
}
