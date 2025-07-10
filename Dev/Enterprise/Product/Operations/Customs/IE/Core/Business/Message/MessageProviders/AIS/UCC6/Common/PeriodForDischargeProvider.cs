using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class PeriodForDischargeProvider : IPeriodForDischarge
	{
		public PeriodForDischargeProvider(CusEntryInstruction instruction)
		{
			this.instruction = Argument.NotNull(instruction, nameof(instruction));
		}
		protected readonly CusEntryInstruction instruction;

		public string Period => instruction.ZG_PeriodForDischarge.ToString();

		public bool AutomaticExtension => instruction.ZG_PeriodForDischargeAutoExtension;

		public string Details => instruction.PeriodForDischargeDetails;
	}
}
