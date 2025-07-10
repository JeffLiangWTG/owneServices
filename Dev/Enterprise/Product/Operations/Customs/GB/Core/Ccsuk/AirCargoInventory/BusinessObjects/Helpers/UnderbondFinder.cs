using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	class UnderbondFinder
	{
		public UnderbondFinder(ICcsukCusAwb awb)
		{
			this.awb = awb;
		}

		internal CusUnderbond FindUnderbond(ZString newCustomsActionCode)
		{
			// Note that if Awb's current status is CA then we cannot be very smart about finding an underbond of the right flavour, 
			// but if the status is (e.g.) CB we can limit to only (e.g.) ISRs.

			var underbondsSource = new List<CusUnderbond>();
			switch (awb.CustomsActionCode)
			{
				case CustomsStatusCodes.Codes.ReleasedForTranshipmentRemoval:
					underbondsSource.AddRange(awb.TSRs.ToArray<CusUnderbond>());
					break;
				case CustomsStatusCodes.Codes.ReleasedForInterAirportRemoval:
					underbondsSource.AddRange(awb.IARs.ToArray<CusUnderbond>());
					break;
				case CustomsStatusCodes.Codes.ReleasedForInterShedRemoval:
					underbondsSource.AddRange(awb.ISRs.ToArray<CusUnderbond>());
					break;

				default:
					underbondsSource.AddRange(awb.TSRs.ToArray<CusUnderbond>());
					underbondsSource.AddRange(awb.IARs.ToArray<CusUnderbond>());
					underbondsSource.AddRange(awb.ISRs.ToArray<CusUnderbond>());
					break;
			}

			return (from CusUnderbond ub
					in underbondsSource
					where ub.SplitReferenceToWhichThisRemovalPertains == awb.SplitReference
						&& (ub.C4_Status == EDIMessage.Status.Acknowledged || ub.C4_Status == EDIMessage.Status.Pending)
					orderby ub.C4_SendersMessageReference
					select ub).LastOrDefault();
		}
		readonly ICcsukCusAwb awb;
	}
}
