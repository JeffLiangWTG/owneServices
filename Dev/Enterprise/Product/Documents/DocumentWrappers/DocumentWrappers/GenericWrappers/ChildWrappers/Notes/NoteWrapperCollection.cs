using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DocumentEngineCore.DocWrappers.CustomIndexerList(typeof(PreDefinedNoteTypeDescriptionPairList))]
	public class NoteWrapperCollection : GenericWrapperCollection<NoteWrapper>
	{
		public NoteWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			notesSummaryDictionary = new Dictionary<string, NoteSummaryWrapper>(StringComparer.OrdinalIgnoreCase);
		}

		public NoteWrapperCollection(BusinessObject parentBO, bool shouldGetNotesFromRelatedBOs, BusinessObjectFactory factory)
			: this(factory)
		{
			AddNotes(parentBO, shouldGetNotesFromRelatedBOs);
		}

		public NoteWrapperCollection(BusinessObject parentBO, BusinessObjectFactory factory)
			: this(parentBO, false, factory)
		{
		}

		readonly Dictionary<string, NoteSummaryWrapper> notesSummaryDictionary;
		IStmNoteParent noteParent;

		#region AddNotes

		public void AddNotes(BusinessObject parentBO, bool shouldIncludeRelated = false)
		{
			noteParent = parentBO as IStmNoteParent;
			if (noteParent != null && noteParent.Notes != null)
			{
				List<ZString> allNoteDescriptions = new List<ZString>();
				allNoteDescriptions.AddRange(GetAllNotesDescriptions(noteParent.Notes));

				if (shouldIncludeRelated)
				{
					foreach (BusinessObject relatedBO in noteParent.BusinessObjectsWithRelatedNotes)
					{
						IStmNoteParent relatedNoteParent = relatedBO as IStmNoteParent;
						if (relatedNoteParent != null)
						{
							allNoteDescriptions.AddRange(GetAllNotesDescriptions(relatedNoteParent.Notes));
						}
					}
				}

				List<StmNote> allNotes = new List<StmNote>();
				foreach (ZString noteDescription in allNoteDescriptions.Distinct())
				{
					allNotes.AddRange(noteParent.Notes.FindByDescription(noteDescription, shouldIncludeRelated));
				}

				WrapUniqueNotes(allNotes);
			}
		}

		#endregion

		#region Implementation

		IEnumerable<ZString> GetAllNotesDescriptions(Notes notes)
		{
			return notes.GetAllNotes().Cast<StmNote>()
					.Where(note => note.ST_DescriptionInDatabase != PredefinedNoteTypes.Instance.AutoRatingAuditLog.Code)
					.Select(note => note.ST_DescriptionInDatabase);
		}

		void WrapUniqueNotes(List<StmNote> relatedNotes)
		{
			if (relatedNotes != null && relatedNotes.Count > 0)
			{
				relatedNotes = relatedNotes.Distinct(new StmNoteComparer()).ToList();
				foreach (StmNote note in relatedNotes)
				{
					AddNoteWrapper(note, relatedNotes);
				}
			}
		}

		class StmNoteComparer : IEqualityComparer<StmNote>
		{
			public bool Equals(StmNote first, StmNote second)
			{
				return first.ST_DescriptionInDatabase.Equals(second.ST_DescriptionInDatabase) && first.ST_NoteDataAsText.Equals(second.ST_NoteDataAsText);
			}

			public int GetHashCode(StmNote note)
			{
				return note.ST_NoteDataAsText.GetHashCode() ^ note.ST_DescriptionInDatabase.GetHashCode();
			}
		}

		void AddNoteWrapper(StmNote note, List<StmNote> notes)
		{
			NoteSummaryWrapper wrapperSummary = null;

			if (!notesSummaryDictionary.TryGetValue(note.ST_DescriptionInDatabase, out wrapperSummary))
			{
				var notesWithSameDescription = notes.Where(n => n.ST_DescriptionInDatabase == note.ST_DescriptionInDatabase).ToArray();
				wrapperSummary = new NoteSummaryWrapper(notesWithSameDescription, Factory);

				notesSummaryDictionary.Add(note.ST_DescriptionInDatabase, wrapperSummary);
			}

			Add(new NoteIndividualWrapper(note, Factory));
		}

		#endregion

		#region GetRow

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Index suffix")]
		protected override DocumentEngineCore.DocWrappers.IBODocDataProvider GetRow(ZString index)
		{
			const char divider = ':';
			int dividerPosition = index.IndexOf(divider);

			NoteWrapper result = null;
			if (dividerPosition < 0)
			{
				if (notesSummaryDictionary != null && notesSummaryDictionary.TryGetValue(index, out var summaryWrapper))
				{
					result = summaryWrapper;
				}
			}
			else
			{
				ZString suffix = index.SubstringSafe(dividerPosition + 1);
				ZString noteDescription = index.SubstringSafe(0, dividerPosition);

				if (suffix == "First")
				{
					result = this.Cast<NoteWrapper>().OrderBy(note => note.CreatedDate).FirstOrDefault(note => note.DescriptionInDatabase == noteDescription);
				}
				else if (suffix == "Last")
				{
					result = this.Cast<NoteWrapper>().OrderByDescending(note => note.CreatedDate).FirstOrDefault(note => note.DescriptionInDatabase == noteDescription);
				}
				else if (suffix == "All" && noteParent != null)
				{
					if (!AllNotesSummaryDictionary.TryGetValue(noteDescription, out result))
					{
						var wrapperSummary = new NoteSummaryWrapper(noteParent.Notes.FindByDescription(noteDescription, true, false), Factory);

						AllNotesSummaryDictionary.Add(noteDescription, wrapperSummary);

						result = wrapperSummary;
					}
				}
			}

			return result ?? base.GetRow(index);
		}

		Dictionary<string, NoteWrapper> AllNotesSummaryDictionary { get; } = new Dictionary<string, NoteWrapper>(StringComparer.OrdinalIgnoreCase);

		#endregion
	}

	public class PreDefinedNoteTypeDescriptionPairList : CodeDescriptionPairList
	{
		public PreDefinedNoteTypeDescriptionPairList()
		{
			foreach (PredefinedNoteType noteType in PredefinedNoteTypes.Instance.All)
			{
				Add(new CodeElement(ZGuid.NewZGuid(), noteType.Description, ""));
			}
		}
	}
}
