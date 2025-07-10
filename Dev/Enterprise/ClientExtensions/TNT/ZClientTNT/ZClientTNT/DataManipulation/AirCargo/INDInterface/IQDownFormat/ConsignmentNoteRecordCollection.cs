using System.Collections;

using CargoWise.Types;

namespace Enterprise.Client.TNT
{
	public class ConsignmentNoteRecordCollection : IEnumerable
	{
		public ConsignmentNoteRecordCollection()
		{
			List = new ArrayList();
		}

		public ConsignmentNoteRecord this[int i]
		{
			get { return (ConsignmentNoteRecord)List[i]; }
		}

		public ConsignmentNoteRecord this[ZString houseBill, ZInt sequence]
		{
			get
			{
				ConsignmentNoteRecord result = null;

				foreach (ConsignmentNoteRecord record in List)
				{
					if (houseBill == record.HouseBill && sequence == record.Sequence)
					{
						result = record;
						break;
					}
				}

				return result;
			}
		}

		public void Add(ConsignmentNoteRecord value)
		{
			if (this[value.HouseBill, value.Sequence] == null)
			{
				List.Add(value);
			}
		}

		public int Count
		{
			get { return List.Count; }
		}

		readonly ArrayList List;

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return List.GetEnumerator();
		}

		#endregion
	}
}
