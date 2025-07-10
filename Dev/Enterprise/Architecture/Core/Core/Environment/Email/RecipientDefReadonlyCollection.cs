using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Enterprise.ZArchitecture.Environment
{
	public class RecipientDefReadonlyCollection : ICollection
	{
		public RecipientDefReadonlyCollection(List<RecipientDef> masterCollection)
		{
			inner = masterCollection;
		}

		readonly List<RecipientDef> inner;

		#region wrapper methods and properties

		public RecipientDef this[int index]
		{
			get { return inner[index]; }
		}

		public StringCollection ToStringCollection()
		{
			StringCollection result = new StringCollection();
			foreach (RecipientDef recipient in inner)
			{
				result.Add(recipient.Email);
			}
			return result;
		}

		/// <summary>
		/// Returns a single string containing all email addresses, separated with "; ".
		/// e.g. returns "dan; bob; jim"
		/// </summary>
		/// <returns>"dan; bob; jim" etc, the default separator char is "; "</returns>
		public string RecipientsAsDelimitedString()
		{
			return RecipientsAsDelimitedString("; ");
		}

		/// <summary>
		/// Returns a single string containing all email addresses, separated with whatever you want.
		/// e.g. returns "dan,bob,jim"
		/// </summary>
		/// <param name="delimiter">The string you wish to stick between the addresses, for example "," or ", "</param>
		/// <returns>"dan,bob,jim" etc</returns>
		public string RecipientsAsDelimitedString(string delimiter)
		{
			List<string> result = new List<string>();
			foreach (RecipientDef recipient in inner)
			{
				result.Add(recipient.Email);
			}

			return string.Join(delimiter, result.ToArray());
		}

		public void Clear()
		{
			inner.Clear();
		}

		public bool Contains(string address)
		{
			foreach (RecipientDef recipient in inner)
			{
				if (recipient.Email == address)
				{
					return true;
				}
			}
			return false;
		}

		#endregion

		#region ICollection Members

		/// <summary>
		/// not implemented
		/// </summary>
		/// <param name="array"></param>
		/// <param name="index"></param>
		public void CopyTo(Array array, int index)
		{
		}

		public int Count
		{
			get { return inner.Count; }
		}

		public bool IsSynchronized
		{
			get { return ((ICollection)inner).IsSynchronized; }
		}

		public object SyncRoot
		{
			get { return ((ICollection)inner).SyncRoot; }
		}

		#endregion

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			return inner.GetEnumerator();
		}

		#endregion
	}
}
