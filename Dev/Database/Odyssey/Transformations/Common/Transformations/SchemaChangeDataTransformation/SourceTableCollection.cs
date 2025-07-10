using System;

namespace Enterprise.DbUpgrader.Transformation.Common
{
	using System.Collections;
	using System.Collections.Specialized;

	public interface ISourceTableCollection : IEnumerable
	{
		SourceTable this[string key] { get; }
		bool AreAllTablesCopied { get; }
		int Count { get; }
	}

	public class SourceTableCollection : ISourceTableCollection
	{
		public SourceTableCollection()
		{
		}

		#region ISourceTableCollection Members

		public SourceTable this[string key]
		{
			get { return (SourceTable)fList[key]; }
		}

		public IEnumerator GetEnumerator()
		{
			return fList.Values.GetEnumerator();
		}

		public bool AreAllTablesCopied
		{
			get
			{
				bool result = true;

				foreach (SourceTable table in fList.Values)
				{
					if (!table.IsTableCopied)
					{
						result = false;
						break;
					}
				}

				return result;
			}
		}

		public int Count
		{
			get { return fList.Count; }
		}

		#endregion

		public void Add(SourceTable table)
		{
			if (IsAnyTableCopied)
			{
				throw new InvalidOperationException("Cannot add more tables if some have been already copied.");
			}

			fList.Add(table.OriginalName, table);
		}

		readonly HybridDictionary fList = new HybridDictionary();

		bool IsAnyTableCopied
		{
			get
			{
				bool result = false;

				foreach (SourceTable table in fList.Values)
				{
					if (table.IsTableCopied)
					{
						result = true;
						break;
					}
				}

				return result;
			}
		}
	}
}
