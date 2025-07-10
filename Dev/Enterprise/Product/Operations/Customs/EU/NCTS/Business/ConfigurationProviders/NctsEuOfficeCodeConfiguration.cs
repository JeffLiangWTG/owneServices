using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsEuOfficeCodeConfiguration
	{
		public NctsEuOfficeCodeConfiguration()
		{
		}

		public virtual ZBool AutomaticSequenceNumberEnabled => true;

		public INctsEuOfficeCodeValidationDecider GetValidationDecider(NctsHeader header) => GetValidationDeciderCore(header);

		protected virtual INctsEuOfficeCodeValidationDecider GetValidationDeciderCore(NctsHeader header)
		{
			INctsEuOfficeCodeValidationDecider result = null;
			if (header != null)
			{
				if (header.IsPhase5Departure)
				{
					result = GetNctsEuOfficeCodeDeparturePhase5ValidationDecider();
				}
				else if (header.IsPhase5Arrival)
				{
					result = GetNctsEuOfficeCodeArrivalPhase5ValidationDecider();
				}
			}

			return result;
		}

		protected virtual INctsEuOfficeCodePhase5ValidationDecider GetNctsEuOfficeCodeDeparturePhase5ValidationDecider() => new NctsEuOfficeCodeDeparturePhase5ValidationDecider();

		protected virtual INctsEuOfficeCodePhase5ValidationDecider GetNctsEuOfficeCodeArrivalPhase5ValidationDecider() => new NctsEuOfficeCodeArrivalPhase5ValidationDecider();
	}
}
