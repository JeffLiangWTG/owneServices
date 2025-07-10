using System;
using System.Collections;
using System.Xml;

namespace Enterprise.eHubMessaging.Business
{
	internal sealed class XPathCollectionEnumerator : IEnumerator
	{
		readonly IDictionaryEnumerator hashEnum;

		public XPathCollectionEnumerator(Hashtable xpathes)
		{
			hashEnum = xpathes.GetEnumerator();
		}

		public bool MoveNext()
		{
			return hashEnum.MoveNext();
		}

		public Object Current
		{
			get { return ((DictionaryEntry)hashEnum.Current).Value; }
		}

		public void Reset()
		{
			hashEnum.Reset();
		}
	}

	public class XPathCollection : ICollection
	{
		readonly Hashtable xpathes;
		int processCount;
		XPathReader reader;
		int key;
		XmlNamespaceManager nsManager;

		public XPathCollection()
		{
			xpathes = new Hashtable();
		}

		public XPathCollection(XmlNamespaceManager nsManager)
			: this()
		{
			this.nsManager = nsManager;
		}

		internal XPathReader SetReader
		{
			set { this.reader = value; }
		}

		internal int ProcessCount
		{
			get { return this.processCount; }
			set { this.processCount = value; }
		}

		public XmlNamespaceManager NamespaceManager
		{
			set { this.nsManager = value; }
			get { return this.nsManager; }
		}

		public void CopyTo(Array array, int index)
		{
		}

		internal bool MatchesAny(ArrayList list, int depth)
		{
			bool ret = false;

			if (list == null)
			{
				throw new ArgumentNullException(nameof(list));
			}

			list.Clear();

			foreach (XPathQuery expr in this)
			{
				if (expr.Match())
				{
					list.Add(expr.Key);
					ret = true;
				}
			}
			return ret;
		}

		internal bool CurrentContainAttributeQuery()
		{
			bool ret = false;
			foreach (XPathQuery expr in this)
			{
				if (expr.IsAttributeQuery())
				{
					ret = true;
					break;
				}
			}

			return ret;
		}

		internal void Advance(XPathReader reader)
		{
			foreach (XPathQuery expr in this)
			{
				expr.Advance(reader);
			}
		}

		internal void AdvanceUntil(XPathReader reader)
		{
			foreach (XPathQuery expr in this)
			{
				expr.AdvanceUntil(reader);
			}

			if (!CurrentContainAttributeQuery())
			{
				reader.ProcessAttribute = -1;
			}
		}

		internal bool MatchAnyQuery()
		{
			foreach (XPathQuery expr in this)
			{
				if (expr.Match())
				{
					return true;
				}
			}
			return false;
		}

		public bool Contains(XPathQuery expr)
		{
			return xpathes.ContainsValue(expr);
		}

		public bool Contains(string xpath)
		{
			bool ret = false;

			foreach (XPathQuery xpathexpr in xpathes)
			{
				if (xpathexpr.ToString() == xpath)
				{
					ret = true;
					break;
				}
			}

			return ret;
		}

		public int Add(string xpath)
		{
			XPathQuery xpathexpr;

			if (reader != null)
			{
				xpathexpr = new XPathQuery(xpath, reader.Depth);
				if (reader.ReadState == ReadState.Interactive)
				{
					xpathexpr.Advance(this.reader);
				}
			}
			else
			{
				xpathexpr = new XPathQuery(xpath);
			}

			xpathexpr.Key = key;

			xpathes.Add(key++, xpathexpr);

			return (key - 1);
		}

		public int Add(XPathQuery xpathexpr)
		{
			xpathexpr.Key = key;
			xpathes.Add(key++, xpathexpr);
			return (key - 1);
		}

		public XPathQuery this[int index]
		{
			get { return (XPathQuery)xpathes[index]; }
		}

		public int Count
		{
			get { return xpathes.Count; }
		}

		public Object SyncRoot
		{
			get { return this; }
		}

		public bool IsSynchronized
		{
			get { return false; }
		}

		public IEnumerator GetEnumerator()
		{
			return (new XPathCollectionEnumerator(xpathes));
		}

		public void Clear()
		{
			xpathes.Clear();
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public bool IsFixedSize
		{
			get { return false; }
		}

		public void Remove(XPathQuery xpathexpr)
		{
			xpathes.Remove(xpathexpr.Key);
		}

		public void Remove(string xpath)
		{
			foreach (XPathQuery xpathexpr in xpathes)
			{
				if (xpathexpr.ToString() == xpath)
				{
					Remove(xpathexpr.Key);
				}
			}
		}

		public void Remove(int index)
		{
			xpathes.Remove(index);
		}
	}
}
