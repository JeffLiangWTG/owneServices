using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class BulkUpdateModeCollection : NonPersistentBusinessObject, IEnumerable<IBindableBooleanItem>, IObsoleteValidation
	{
		readonly List<BulkUpdateMode> BulkUpdateModes = new List<BulkUpdateMode>();
		public IEnumerator<IBindableBooleanItem> GetEnumerator()
		{
			foreach (BulkUpdateMode element in BulkUpdateModes)
			{
				yield return element;
			}
		}

		public BulkUpdateMode SelectedOrder
		{
			get
			{
				foreach (BulkUpdateMode s in this)
				{
					if (s.Selected)
					{
						return s;
					}
				}

				return new BulkUpdateMode("", 0);
			}
			set
			{
				for (int i = 0; i < Count; i++)
				{
					this[i].Selected = this[i] == value;
				}
				if (SelectedOrder == null)
				{
					throw new InvalidOperationException("There is no such BulkUpdateModes in the collection.");
				}
			}
		}

		public BulkUpdateMode Add(string displayName, int recordsAffected = 0)
		{
			return Add(new BulkUpdateMode(displayName, recordsAffected));
		}

		public BulkUpdateMode Add(BulkUpdateMode mode)
		{
			RegisterEditableChildObject(mode);
			BulkUpdateModes.Add(mode);
			return mode;
		}

		public int Count
		{
			get
			{
				return BulkUpdateModes.Count;
			}
		}

		public BulkUpdateMode this[int index]
		{
			get
			{
				return BulkUpdateModes[index];
			}
			internal set
			{
				UnRegisterEditableChildObject(BulkUpdateModes[index]);
				BulkUpdateModes[index] = value;
				RegisterEditableChildObject(BulkUpdateModes[index]);
			}
		}

		public new BulkUpdateMode this[string modeCode]
		{
			get
			{
				foreach (BulkUpdateMode mode in this)
				{
					if (mode.ModeCode == modeCode)
					{
						return mode;
					}
				}
				return null;
			}
		}

		public void Clear()
		{
			foreach (BulkUpdateMode mode in BulkUpdateModes)
			{
				UnRegisterEditableChildObject(mode);
			}
			BulkUpdateModes.Clear();
		}

		public BulkUpdateMode DefaultMode { get; private set; }

		public void UpdateDefaultOrder()
		{
			foreach (BulkUpdateMode mode in this)
			{
				if (mode.Selected)
				{
					DefaultMode = mode;
					break;
				}
			}
		}
	}
}
