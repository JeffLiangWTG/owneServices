using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	#region Pair Change Event

	public delegate void ZBoolDescriptionPairChangedEventHandler(ZBoolDescriptionPairChangedEventArgs e);

	public class ZBoolDescriptionPairChangedEventArgs : EventArgs
	{
		public ZBoolDescriptionPairChangedEventArgs(ZBoolDescriptionPair pair)
		{
			this.Pair = pair;
		}

		public readonly ZBoolDescriptionPair Pair;
	}

	#endregion

	public class ZBoolDescriptionPairList : IEnumerable<ZBoolDescriptionPair>
	{
		public ZBoolDescriptionPairList()
		{
		}

		#region Indexer

		public ZBoolDescriptionPair this[int index]
		{
			get { return List[index]; }
		}

		public ZBoolDescriptionPair this[string description]
		{
			get
			{
				foreach (ZBoolDescriptionPair pair in List)
				{
					if (pair.Description == description)
					{
						return pair;
					}
				}

				return null;
			}
		}

		#endregion

		#region Add

		public void AddNew(string description, ZBool value)
		{
			Add(new ZBoolDescriptionPair(description, value));
		}

		public void AddNew(ZGuid pK, string description, ZBool value)
		{
			Add(new ZBoolDescriptionPair(pK, description, value));
		}

		public void Add(ZBoolDescriptionPair pair)
		{
			List.Add(pair);
			pair.OnChanged += new EventHandler(Pair_OnChanged);

			if (OnListChanged != null && !IsOnChangedSuspended)
			{
				OnListChanged(new ZBoolDescriptionPairChangedEventArgs(pair));
			}
		}

		#endregion

		#region Count

		public int Count
		{
			get { return List.Count; }
		}

		#endregion

		#region Clear

		public void Clear()
		{
			List.Clear();
		}

		#endregion

		#region Change Events

		public event ZBoolDescriptionPairChangedEventHandler OnPairChanged;

		public event ZBoolDescriptionPairChangedEventHandler OnListChanged;

		public event ZBoolDescriptionPairChangedEventHandler OnPairChangedForBinding;

		public bool IsOnChangedSuspended
		{
			get { return OnChangedSempahore > 0; }
		}

		public void SuspendOnChanged()
		{
			OnChangedSempahore++;
		}

		public void ResumeOnChanged()
		{
			OnChangedSempahore--;

			if (OnChangedSempahore < 0)
			{
				OnChangedSempahore = 0;
				ErrorReporter.ReportOnce("The OnChangedSempahore was set to -1. This means a consumer has called ResumeOnChanged() without first calling SuspendOnChanged().");
			}
		}

		int OnChangedSempahore;

		void Pair_OnChanged(object sender, EventArgs e)
		{
			ZBoolDescriptionPair pair = (ZBoolDescriptionPair)sender;

			FireOnPairChangedForBinding(pair);
			FireOnPairChanged(pair);
		}

		void FireOnPairChangedForBinding(ZBoolDescriptionPair pair)
		{
			if (OnPairChangedForBinding != null && !IsOnChangedSuspended)
			{
				OnPairChangedForBinding(new ZBoolDescriptionPairChangedEventArgs(pair));
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void FireOnPairChanged(ZBoolDescriptionPair pair)
		{
			if (OnPairChanged != null && !IsOnChangedSuspended)
			{
				OnPairChanged(new ZBoolDescriptionPairChangedEventArgs(pair));
			}
		}

		#endregion

		#region IEnumerable Members

		IEnumerator<ZBoolDescriptionPair> IEnumerable<ZBoolDescriptionPair>.GetEnumerator()
		{
			return List.GetEnumerator();
		}

		#endregion

		#region List

		List<ZBoolDescriptionPair> List
		{
			get { return fList ?? (fList = new List<ZBoolDescriptionPair>()); }
		}

		List<ZBoolDescriptionPair> fList;

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return List.GetEnumerator();
		}

		#endregion
	}
}
