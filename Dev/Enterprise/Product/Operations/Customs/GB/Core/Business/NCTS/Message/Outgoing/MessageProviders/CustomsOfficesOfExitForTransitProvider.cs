using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Business.NCTS
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
