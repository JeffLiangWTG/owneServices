using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Modules
{
	/// <summary>
	/// This class is used to allow people to store extra info with their PKs (eg, type of objects).
	/// </summary>
	public class ZPKCollection : IEnumerable<PKData>
	{
		public ZPKCollection(List<ZGuid> guids)
			: this(ConvertToPKDataList(guids))
		{
		}

		static List<PKData> ConvertToPKDataList(List<ZGuid> guids)
		{
			return guids == null ? null : guids.Select(x => new PKData() { PK = x }).ToList();
		}

		public ZPKCollection(List<PKData> guids)
		{
			Rebuild(guids);
		}

		public void Rebuild(List<ZGuid> guids)
		{
			Rebuild(ConvertToPKDataList(guids));
		}

		public void Rebuild(List<PKData> guids)
		{
			if (ListChanging != null)
			{
				ListChanging(this, null);
			}

			fGuids = guids;
			if (ListChanged != null)
			{
				ListChanged(this, null);
			}
		}

		public event EventHandler ListChanged;
		public event EventHandler ListChanging;

		public PKData this[int i]
		{
			get { return GetPKByIndex(i); }
		}

		protected virtual PKData GetPKByIndex(int i)
		{
			return fGuids[i];
		}

		public ZInt Count
		{
			get { return GetCount(); }
		}

		protected virtual ZInt GetCount()
		{
			return fGuids.Count;
		}

		public ZInt IndexOf(PKData pK)
		{
			return fGuids.IndexOf(pK);
		}

		public ZInt IndexOf(ZGuid pK)
		{
			return fGuids.FindIndex(x => x.PK == pK);
		}

		protected List<PKData> fGuids;

		public IEnumerator<PKData> GetEnumerator()
		{
			return fGuids.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
