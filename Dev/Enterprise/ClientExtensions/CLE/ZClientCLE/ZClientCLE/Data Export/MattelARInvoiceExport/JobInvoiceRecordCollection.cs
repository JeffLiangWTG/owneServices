using System.Collections;

namespace Enterprise.Client.CLE.MattelARInvoiceExport
{
	public class JobInvoiceRecordCollection : IEnumerable
	{
		public JobInvoiceRecordCollection()
		{
			List = new ArrayList();
		}

		public JobInvoiceRecord this[int i]
		{
			get { return (JobInvoiceRecord)List[i]; }
		}

		public void Add(JobInvoiceRecord value)
		{
			List.Add(value);
		}

		public void Clear()
		{
			List.Clear();
		}

		public int Count
		{
			get { return List.Count; }
		}

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return List.GetEnumerator();
		}

		#endregion

		readonly ArrayList List;
	}
}
