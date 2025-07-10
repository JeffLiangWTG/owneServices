using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class BillOfDischargeProvider : IBillOfDischarge
	{
		public BillOfDischargeProvider(CusEntryInstruction instruction)
		{
			this.instruction = Argument.NotNull(instruction, nameof(instruction));
		}
		protected readonly CusEntryInstruction instruction;

		public bool UseOfTheBillOfDischarge => instruction.ZG_BillOfDischargeIsNecessary;

		public string Deadline => instruction.ZG_BillOfDischargeDeadline.ToString();

		public string Details => instruction.BillOfDischargeDetails;
	}
}
