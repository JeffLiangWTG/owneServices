using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;

namespace Enterprise.Customs.IE.H7.Business
{
	public class AdditionalProcedureProvider : ICcQualifierAdditionalProcedure
	{
		public AdditionalProcedureProvider(int sequenceNumber, string additionalProcedureCode)
		{
			SequenceNumber = sequenceNumber.ToString();
			AdditionalProcedure = additionalProcedureCode;
		}

		public string SequenceNumber { get; }

		public string CcQualifier => null;

		public string AdditionalProcedure { get; }
	}
}
