using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CustomsOfficesOfExitForTransitProvider : ICustomsOfficeOfExitForTransit
	{
		public CustomsOfficesOfExitForTransitProvider(string officeCode, ZInt sequence)
		{
			SequenceNumber = sequence;
			ReferenceNumber = officeCode;
		}

		public int SequenceNumber { get; }

		public string ReferenceNumber { get; }
	}
}
