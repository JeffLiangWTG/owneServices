using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class CC013CHeaderProvider : CC013CCC015CDeclarationDataHeaderProvider, ICC013C
	{
		public CC013CHeaderProvider(NctsHeader nctsHeader) : base(nctsHeader) { }

		public override string MessageType => Constants.MessageTypes.CC013C;
	}
}
