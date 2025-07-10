using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class ProcessHeaderLinkWrapperCollection : ICollection<ProcessHeaderLink>
	{
		#region Constructors

		public ProcessHeaderLinkWrapperCollection(ProcessHeader header)
		{
			Parent = header;
			FromHeaderLinks = new ProcessHeaderLinkCollection(header, ProcessHeaderLinkSchema.FP_FH_HeaderFrom);
			ToHeaderLinks = new ProcessHeaderLinkCollection(header, ProcessHeaderLinkSchema.FP_FH_HeaderTo);

			foreach (var collection in AllCollections)
			{
				header.RegisterEditableChildObject(collection);
			}
		}

		#endregion

		#region Properties

		public ProcessHeader Parent { get; }

		public ProcessHeaderLinkCollection FromHeaderLinks { get; }
		public ProcessHeaderLinkCollection ToHeaderLinks { get; }

		IEnumerable<ProcessHeaderLinkCollection> AllCollections
		{
			get
			{
				yield return FromHeaderLinks;
				yield return ToHeaderLinks;
			}
		}

		#endregion

		#region ICollection<ProcessHeaderLink>

		public int Count
		{
			get
			{
				EnsureFetch();
				return AllCollections.Sum(s => s.Count);
			}
		}

		public bool IsReadOnly
		{
			get { return AllCollections.Cast<ICollection<ProcessHeaderLink>>().Any(a => a.IsReadOnly); }
		}

		public void Add(ProcessHeaderLink item)
		{
			ThrowNotSupported();
		}

		public void Clear()
		{
			ThrowNotSupported();
		}

		public bool Contains(ProcessHeaderLink item)
		{
			EnsureFetch();
			return AllCollections.Any(a => a.Contains(item));
		}

		public void CopyTo(ProcessHeaderLink[] array, int arrayIndex)
		{
			if (arrayIndex < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Index was less than zero.");
			}
			else if (array.Length + arrayIndex < Count)
			{
				throw new ArgumentException("Not enough space in array to fit entire collection.");
			}
			else
			{
				foreach (var item in this)
				{
					array[arrayIndex++] = item;
				}
			}
		}

		public IEnumerator<ProcessHeaderLink> GetEnumerator()
		{
			EnsureFetch();
			return AllCollections.SelectMany(a => a).GetEnumerator();
		}

		public bool Remove(ProcessHeaderLink item)
		{
			var removed = false;

			foreach (var collection in AllCollections.Cast<ICollection<ProcessHeaderLink>>())
			{
				if (collection.Remove(item))
				{
					removed = true;
				}
			}

			return removed;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region Utils

		void ThrowNotSupported()
		{
			throw new NotSupportedException("This operation makes no sense to do on an aggregating collection.");
		}

		public void AddFetchHints()
		{
			foreach (var collection in AllCollections)
			{
				Parent.Factory.AddFetchHint(ProcessHeaderLinkSchema.Instance, collection.CompleteFilter);
			}

			hasFetched = true;
		}

		void EnsureFetch()
		{
			if (!hasFetched)
			{
				AddFetchHints();
			}
		}

		bool hasFetched;

		#endregion
	}
}
