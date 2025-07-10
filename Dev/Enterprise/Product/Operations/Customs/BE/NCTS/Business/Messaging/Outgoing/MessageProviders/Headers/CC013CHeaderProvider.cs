using CargoWise.Customs.BE.MessageContracts.Interfaces;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC013CHeaderProvider : CC013CCC015CDeclarationDataHeaderProvider, ICC013C
	{
		public CC013CHeaderProvider(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		public override string MessageType => Constants.MessageTypes.CC013C;

		protected override ITransitOperation GetTransitOperation() => new CC013CTransitOperationProvider(nctsHeader);
	}
}
