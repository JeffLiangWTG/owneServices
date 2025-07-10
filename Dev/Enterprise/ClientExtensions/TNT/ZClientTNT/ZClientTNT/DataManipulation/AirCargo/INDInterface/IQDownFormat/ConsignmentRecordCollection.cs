using System.Collections;

using CargoWise.Types;

namespace Enterprise.Client.TNT
{
	public class ConsignmentRecordCollection : IEnumerable
	{
		public ConsignmentRecordCollection()
		{
			List = new ArrayList();
		}

		public ConsignmentRecord this[int i]
		{
			get { return (ConsignmentRecord)List[i]; }
		}

		public ConsignmentRecord this[ZString houseBill]
		{
			get
			{
				ConsignmentRecord result = null;

				foreach (ConsignmentRecord record in List)
				{
					if (houseBill == record.HouseBill)
					{
						result = record;
						break;
					}
				}

				return result;
			}
		}

		public void Add(ConsignmentRecord value)
		{
			ConsignmentRecord existingRecord = this[value.HouseBill];
			if (existingRecord == null)
			{
				List.Add(value);
			}
			else
			{
				existingRecord.Merge(value);
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
