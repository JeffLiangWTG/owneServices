using System;
using CargoWise.Customs.DE.MessageContracts;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0
{
	public class ActiveBorderTransportMeansProvider : DepartureTransportMeansProvider, IActiveBorderTransportMeans
	{
		public ActiveBorderTransportMeansProvider(CusExitReport report) : base(report)
		{
		}

		public DateTime DepartureDateAndTime
		{
			get
			{
				var result = default(DateTime);
				var dateTime = report.CER_DateTime;
				if (dateTime.IsValid)
				{
					result = dateTime.ToUtcDateTime().ToUnspecified();
				}
				return result;
			}
		}

		public string Location => report.CER_Location;
	}
}
