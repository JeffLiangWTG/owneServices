using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
{
	class EconomicConditionsProvider : IEconomicConditions
	{
		internal EconomicConditionsProvider(CusEntryInstruction instruction)
		{
			this.instruction = instruction;
		}

		public string ProcessingProcedure => instruction.ZG_ProcessingProcedureCode;

		public string Details => instruction.ProcessingProcedureDetails;

		readonly CusEntryInstruction instruction;
	}
}
