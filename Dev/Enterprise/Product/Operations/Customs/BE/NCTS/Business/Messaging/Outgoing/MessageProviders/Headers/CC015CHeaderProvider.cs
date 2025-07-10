using CargoWise.Customs.BE.MessageContracts.Interfaces;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC015CHeaderProvider : CC013CCC015CDeclarationDataHeaderProvider, ICC015C
	{
		public CC015CHeaderProvider(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		public override string MessageType => Constants.MessageTypes.CC015C;
	}
}
