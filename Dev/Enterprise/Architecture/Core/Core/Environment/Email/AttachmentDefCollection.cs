using System;
using System.Collections;

namespace Enterprise.ZArchitecture.Environment
{
	// Implementation based on help file for CollectionBase.
	public class AttachmentDefCollection : CollectionBase
	{
		public AttachmentDef this[int index]
		{
			get
			{
				return (AttachmentDef)List[index];
			}
			set
			{
				List[index] = value;
			}
		}

		public int Add(AttachmentDef value)
		{
			return List.Add(value);
		}

		public int IndexOf(AttachmentDef value)
		{
			return List.IndexOf(value);
		}

		public void Insert(int index, AttachmentDef value)
		{
			List.Insert(index, value);
		}

		public void Remove(AttachmentDef value)
		{
			List.Remove(value);
		}

		public bool Contains(AttachmentDef value)
		{
			// If value is not of type AttachmentDef, this will return false.
			return List.Contains(value);
		}

		protected override void OnInsert(int index, Object value)
		{
			ValidateType(value);
		}

		protected override void OnRemove(int index, Object value)
		{
			ValidateType(value);
		}

		protected override void OnSet(int index, Object oldValue, Object newValue)
		{
			ValidateType(newValue);
		}

		protected override void OnValidate(Object value)
		{
			ValidateType(value);
		}

		protected void ValidateType(object value)
		{
			if (value != null && !(value is AttachmentDef))
			{
				throw new ArgumentException("value must be of type AttachmentDef.", nameof(value));
			}
		}
	}
}
