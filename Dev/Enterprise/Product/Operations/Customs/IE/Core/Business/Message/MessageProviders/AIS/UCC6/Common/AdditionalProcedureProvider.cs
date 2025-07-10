using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class AdditionalProcedureProvider : ICcQualifierAdditionalProcedure
	{
		public AdditionalProcedureProvider(int sequenceNumber, AdditionalProcedureCode additionalProcedureCode)
		{
			this.additionalProcedureCode = Argument.NotNull(additionalProcedureCode, nameof(additionalProcedureCode));
			SequenceNumber = sequenceNumber.ToString();
		}

		readonly AdditionalProcedureCode additionalProcedureCode;

		public string SequenceNumber { get; }

		public string CcQualifier => null;

		public string AdditionalProcedure => additionalProcedureCode.CY_Code.Length == 7 ? additionalProcedureCode.CY_Code.SubstringSafe(4, 3).ToString() : string.Empty;
	}
}
