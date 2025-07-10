using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
{
	class OthersProvider : IOthers
	{
		internal OthersProvider(CusEntryInstruction instruction)
		{
			this.instruction = instruction;
		}

		public bool? CalculationOfTheAmountOfTheImportDuty => instruction.ZG_Article86_3_UCC.IsEmpty ? null : instruction.ZG_Article86_3_UCC.EqualsIgnoringCase(Customs.Business.YesNoList.Codes.Yes);

		public string AdditionalInformation => instruction.AdditionalInformation;

		readonly CusEntryInstruction instruction;
	}
}
