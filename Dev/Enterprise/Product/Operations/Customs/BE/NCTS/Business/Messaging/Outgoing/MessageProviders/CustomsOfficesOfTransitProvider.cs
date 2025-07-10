using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CustomsOfficesOfTransitProvider : ICustomsOfficeOfTransit
	{
		public CustomsOfficesOfTransitProvider(ZString officeCode, ZDateTime arrivalTime, ZInt sequence)
		{
			SequenceNumber = sequence;
			ReferenceNumber = officeCode;
			this.arrivalTime = arrivalTime;
		}

		public int SequenceNumber { get; }

		public string ReferenceNumber { get; }

		public DateTime? ArrivalDateAndTimeEstimated =>	arrivalTime.IsValid ? DateTime.SpecifyKind(arrivalTime.ToUniversalBranchTime().ToDateTime(), DateTimeKind.Unspecified) : null;

		readonly ZDateTime arrivalTime;
	}
}
