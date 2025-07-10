using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	public class AcceptabilityBandResultCollection : List<AcceptabilityBandResult>
	{
		public AcceptabilityBandResultCollection()
			: base()
		{
		}

		public void SetAccurateAsOfTimeUtc(ZDateTime actualTime)
		{
			foreach (var result in this)
			{
				result.AccurateAsOfTimeUtc = actualTime;
			}
		}
	}
}
