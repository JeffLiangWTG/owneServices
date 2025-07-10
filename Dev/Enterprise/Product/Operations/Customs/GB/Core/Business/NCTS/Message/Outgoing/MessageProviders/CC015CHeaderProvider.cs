using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class CC015CHeaderProvider : CC013CCC015CDeclarationDataHeaderProvider, ICC015C
	{
		public CC015CHeaderProvider(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		public override string MessageType => Constants.MessageTypes.CC015C;
	}
}
