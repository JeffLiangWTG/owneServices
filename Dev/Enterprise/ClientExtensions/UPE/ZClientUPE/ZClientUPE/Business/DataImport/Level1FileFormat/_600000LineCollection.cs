using System.Collections;

namespace Enterprise.Client.UPE.Business.DataImport.Level1FileFormat
{
	public class _600000LineCollection : CollectionBase
	{
		public _600000Line this[int i]
		{
			get { return (_600000Line)List[i]; }
		}

		public int Add(_600000Line value)
		{
			return List.Add(value);
		}
	}
}
