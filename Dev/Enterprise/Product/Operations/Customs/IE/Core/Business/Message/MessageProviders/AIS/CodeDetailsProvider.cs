using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
{
	class CodeDetailsProvider : IAdditionalInformation
	{
		internal CodeDetailsProvider(CusEntryInstruction instruction)
		{
			this.instruction = instruction;
		}

		public string Code => instruction.ZG_IdOfGoodCode;

		public string Text => instruction.IdentificationofGoodsDetails;

		readonly CusEntryInstruction instruction;
	}
}
