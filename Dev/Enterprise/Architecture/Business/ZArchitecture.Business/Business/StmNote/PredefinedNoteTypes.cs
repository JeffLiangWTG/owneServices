using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.Business
{
	/// <summary>
	///	The Predefined Notes Types are maintained by Richard.
	///	*** Please see Richard or Geoff if you require a type that does not exist. ***
	/// </summary>
	public partial class PredefinedNoteTypes
	{
		#region Construction

		protected PredefinedNoteTypes()
		{
		}

		public static PredefinedNoteTypes Instance
		{
			get
			{
				return instance.Value.Value;
			}
		}

		readonly static Overridable<ThreadLocal<PredefinedNoteTypes>> instance = new Overridable<ThreadLocal<PredefinedNoteTypes>>(new ThreadLocal<PredefinedNoteTypes>(() => new PredefinedNoteTypes()));

		protected static void OverrideNewDelegate(Func<PredefinedNoteTypes> newDelegate)
		{
			instance.Value = new ThreadLocal<PredefinedNoteTypes>(newDelegate);
		}

		protected static void ResetNewDelegate()
		{
			instance.ResetValue();
		}

		#endregion

		#region All

		public PredefinedNoteType[] All
		{
			get
			{
				if (fAll == null)
				{
					ArrayList list = new ArrayList(AllWithoutCustomNotes);
					if (Enterprise.ZArchitecture.Business.CustomNotesProvider.Instance != null)
					{
						list.AddRange(Enterprise.ZArchitecture.Business.CustomNotesProvider.Instance.AllCustomNoteTypes);
					}

					fAll = (PredefinedNoteType[])list.ToArray(typeof(PredefinedNoteType));
				}
				return fAll;
			}
		}
		PredefinedNoteType[] fAll;

		public void ClearCacheOfAllNotes()
		{
			fAll = null;
			fAllWithoutCustomNotes = null;
			codesToAll = null;
			codesToAllWithoutCustomNotes = null;
		}

		PredefinedNoteType[] AllWithoutCustomNotes
		{
			get
			{
				if (fAllWithoutCustomNotes == null)
				{
					ArrayList list = new ArrayList();
					foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(this))
					{
						if (typeof(PredefinedNoteType).IsAssignableFrom(property.PropertyType))
						{
							list.Add(property.GetValue(this));
						}
					}

					fAllWithoutCustomNotes = (PredefinedNoteType[])list.ToArray(typeof(PredefinedNoteType));
				}
				return fAllWithoutCustomNotes;
			}
		}
		PredefinedNoteType[] fAllWithoutCustomNotes;

		#endregion

		#region Type from Description

		// Although Ratings would like a 1GB autorating log, there is something in Periodic Billing
		// that limits us to 1MB. Until that is fixed, we'll have to do with less than 1MB. I pick half a meg
		// to be safe.
		public readonly int AutoRatingAuditLog_MaxLength = 524288;
		public readonly int MessageInterpretation_MaxLength = Schema.StmNoteSchema.ST_NoteText.MaxLength / 2;   //2147483647 --> 1073741823 bytes
		public const int CustomsDeliveryInstructions_MaxLength = 1048576;

		public PredefinedNoteType NoteTypeByDescription(string description)
		{
			return NoteTypeByDescription(description, true);
		}

		public PredefinedNoteType NoteTypeByDescription(string description, bool includeCustomNotesInSearch)
		{
			PredefinedNoteType result;
			if (string.IsNullOrEmpty(description))
			{
				return null;
			}
			else if (includeCustomNotesInSearch)
			{
				return CodesToAll.TryGetValue(description, out result) ? result : All.FirstOrDefault(t => t.Description == description);
			}
			else
			{
				return CodesToAllWithoutCustomNotes.TryGetValue(description, out result) ? result : AllWithoutCustomNotes.FirstOrDefault(t => t.Description == description);
			}
		}

		#endregion

		#region Implementation

		Dictionary<string, PredefinedNoteType> CodesToAll
		{
			get { return codesToAll ?? (codesToAll = ToDictionary(All, _ => _.Code)); }
		}
		Dictionary<string, PredefinedNoteType> codesToAll;

		Dictionary<string, PredefinedNoteType> CodesToAllWithoutCustomNotes
		{
			get { return codesToAllWithoutCustomNotes ?? (codesToAllWithoutCustomNotes = ToDictionary(AllWithoutCustomNotes, _ => _.Code)); }
		}
		Dictionary<string, PredefinedNoteType> codesToAllWithoutCustomNotes;

		// Standard function from System.Linq doesn't work for some UI tests, where all resource strings are the same
		// and therefore all descriptions are the same (duplicate keys in the dictionary)
		Dictionary<string, PredefinedNoteType> ToDictionary(PredefinedNoteType[] noteTypes, Func<PredefinedNoteType, string> keySelector)
		{
			var result = new Dictionary<string, PredefinedNoteType>(StringComparer.OrdinalIgnoreCase);
			foreach (var noteType in noteTypes)
			{
				result[keySelector(noteType)] = noteType;
			}
			return result;
		}

		protected const bool IsUniqueInCollection = true;
		protected const bool IsReadOnlyAfterAdd = true;
		protected const bool IsTextOnly = true;
		protected const bool IsPopupLog = true;

		#endregion
	}
}
