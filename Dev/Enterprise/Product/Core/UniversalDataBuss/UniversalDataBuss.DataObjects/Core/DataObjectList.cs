using System.Collections.Generic;
using System.Collections.ObjectModel;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	[CollectionAttributes("Content")]
	public sealed class DataObjectList<T> : Collection<T> where T : IDataObject
	{
		public DataObjectList()
		{
		}

		public DataObjectList(IEnumerable<T> content)
		{
			if (content != null)
			{
				foreach (var item in content)
				{
					Add(item);
				}
			}
		}

		public CollectionContent? Content { get; set; }
	}

	public enum CollectionContent { Complete, Partial }
}
