using System.Collections;

namespace Enterprise.DocumentScanning.Business
{
	public class DocumentResultCollection : CollectionBase
	{
		public DocumentResult this[int index]
		{
			get { return ((DocumentResult)List[index]); }
			set { List[index] = value; }
		}

		public int Add(DocumentResult value)
		{
			return List.Add(value);
		}

		public int IndexOf(DocumentResult value)
		{
			return List.IndexOf(value);
		}

		public void Remove(DocumentResult value)
		{
			List.Remove(value);
		}

		public bool Contains(DocumentResult value)
		{
			return List.Contains(value);
		}

		public int Length
		{
			get { return List.Count; }
		}
	}
}
