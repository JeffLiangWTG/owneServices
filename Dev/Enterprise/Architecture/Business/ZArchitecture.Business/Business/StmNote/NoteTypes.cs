using System;
using System.Collections;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Integration.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business
{
	public class NoteTypeCollection : INoteTypeCollection
	{
		/// <summary>
		/// A collection of PredefinedNoteType objects, as used by the base BusinessObject.
		/// </summary>
		public NoteTypeCollection()
		{
			List = new CodeDescriptionPairList();
		}

		internal CodeDescriptionPairList List;

		#region Adding Note Types

		public void Add(PredefinedNoteType noteType)
		{
			if (!List.Contains(noteType))
			{
				List.Add(noteType);
			}
		}

		public void Add(INoteTypeCollection listOfNoteTypes)
		{
			foreach (PredefinedNoteType noteType in listOfNoteTypes)
			{
				Add(noteType);
			}
		}

		#endregion

		#region Searching for Note Types

		public ZBool IsPredefinedNoteTypeByDescription(ZString description)
		{
			return NoteTypeByDescription(description) != null;
		}

		internal ZString DefaultVisibilityForDescription(ZString description)
		{
			PredefinedNoteType noteType = NoteTypeByDescription(description)
				?? PredefinedNoteTypes.Instance.NoteTypeByDescription(description);

			return (noteType == null) ? ZString.Empty : (ZString)noteType.DefaultVisibility.ToString();
		}

		protected internal ZBool IsOnlyOneAllowedForDescription(ZString description)
		{
			PredefinedNoteType noteType = NoteTypeByDescription(description)
				?? PredefinedNoteTypes.Instance.NoteTypeByDescription(description);

			return !(noteType == null) && noteType.IsOnlyOneAllowed;
		}

		protected internal PredefinedNoteType NoteTypeByDescription(ZString description)
		{
			var desc = description.Trim();
			foreach (PredefinedNoteType noteType in List)
			{
				if (noteType.Code.Trim().Equals(desc, StringComparison.OrdinalIgnoreCase) || noteType.Description.Trim().Equals(desc, StringComparison.OrdinalIgnoreCase))
				{
					return noteType;
				}
			}
			return null;
		}

		#endregion

		#region IList Members

		bool IList.IsReadOnly
		{
			get { return List.IsReadOnly; }
		}

		object IList.this[int index]
		{
			get { return List[index]; }
			set { List[index] = (ICodeDescription)value; }
		}

		void IList.RemoveAt(int index)
		{
			List.RemoveAt(index);
		}

		void IList.Insert(int index, object value)
		{
			List.Insert(index, value);
		}

		void IList.Remove(object value)
		{
			List.Remove((ICodeDescription)value);
		}

		bool IList.Contains(object value)
		{
			return ((IList)List).Contains(value);
		}

		void IList.Clear()
		{
			List.Clear();
		}

		int IList.IndexOf(object value)
		{
			return List.IndexOf((ICodeDescription)value);
		}

		int IList.Add(object value)
		{
			return List.Add((ICodeDescription)value);
		}

		bool IList.IsFixedSize
		{
			get { return List.IsFixedSize; }
		}

		#endregion

		#region ICollection Members

		bool ICollection.IsSynchronized
		{
			get { return List.IsSynchronized; }
		}

		public int Count
		{
			get { return List.Count; }
		}

		void ICollection.CopyTo(Array array, int index)
		{
			List.CopyTo(array, index);
		}

		object ICollection.SyncRoot
		{
			get { return List.SyncRoot; }
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return List.GetEnumerator();
		}

		#endregion
	}
}
