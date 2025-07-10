using System;
using System.Collections;
using System.Linq;

using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business
{
	public class CodeSet : IEnumerable, ICloneable
	{
		public void Add(ZString code)
		{
			if (!code.IsEmpty && !SelectedCodes.Contains(code))
			{
				SelectedCodes[code] = null;
				OnChanged(EventArgs.Empty);
			}
		}

		public void Remove(ZString code)
		{
			if (Contains(code))
			{
				SelectedCodes.Remove(code);
				OnChanged(EventArgs.Empty);
			}
		}

		public bool Contains(ZString code)
		{
			return SelectedCodes.Contains(code);
		}

		public int Count
		{
			get { return SelectedCodes.Count; }
		}

		public void Clear()
		{
			if (!IsEmpty)
			{
				SelectedCodes.Clear();
				OnChanged(EventArgs.Empty);
			}
			OnClearComplete();
		}

		protected virtual void OnClearComplete()
		{
		}

		public bool IsEmpty
		{
			get { return Count == 0; }
		}

		public ZQuery GetMultipleCodeFilter(SchemaStringColumn filteredCodeColumn)
		{
			// odd select to handle ZString and string objects
			return SelectedCodes.Keys.Count > 0 ? new ZQuery(filteredCodeColumn, SelectedCodes.Keys.Cast<object>().Select(x => x.ToString()).OrderBy(x => x)) : new ZQuery();
		}

		#region Changed Event

		public event EventHandler Changed;

		protected virtual void OnChanged(EventArgs e)
		{
			if (Changed != null)
			{
				Changed(this, e);
			}
		}

		#endregion

		#region ICloneable

		public CodeSet Clone()
		{
			CodeSet result = (CodeSet)MemberwiseClone();
			result.SelectedCodes = (Hashtable)SelectedCodes.Clone();
			return result;
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		#endregion

		#region IEnumerable

		IEnumerator IEnumerable.GetEnumerator()
		{
			return SelectedCodes.Keys.GetEnumerator();
		}

		#endregion

		Hashtable SelectedCodes = new Hashtable();
	}
}
