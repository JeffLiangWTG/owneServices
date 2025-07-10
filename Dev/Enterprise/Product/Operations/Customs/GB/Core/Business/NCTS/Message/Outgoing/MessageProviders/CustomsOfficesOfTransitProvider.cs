using System;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class CustomsOfficesOfTransitProvider : ICustomsOfficeOfTransit
	{
		public CustomsOfficesOfTransitProvider(ZString officeCode, ZDateTime arrivalTime, ZInt sequence)
		{
			SequenceNumber = sequence;
			ReferenceNumber = officeCode;
			if (arrivalTime.IsValid)
			{
				ArrivalDateAndTimeEstimated = DateTime.SpecifyKind(DataProviderHelper.GetProviderDateTime(arrivalTime), DateTimeKind.Unspecified);
			}
		}

		public int SequenceNumber { get; }

		public string ReferenceNumber { get; }

		public DateTime? ArrivalDateAndTimeEstimated { get; }
	}
}
