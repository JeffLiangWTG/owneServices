using System;
using CargoWise.Customs.DE.MessageContracts;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0
{
	public class EXTPREHeaderProvider : ExitPresentationHeaderProvider, IEXTPREHeader
	{
		public EXTPREHeaderProvider(CusExitReport report) : base(report)
		{
		}

		public DateTime ArrivalNotificationDateAndTime => (dateAndTime ?? (dateAndTime = new UniversalDateAndTimeProvider())).DateAndTime.ToUnspecified();
		UniversalDateAndTimeProvider dateAndTime;
	}
}
