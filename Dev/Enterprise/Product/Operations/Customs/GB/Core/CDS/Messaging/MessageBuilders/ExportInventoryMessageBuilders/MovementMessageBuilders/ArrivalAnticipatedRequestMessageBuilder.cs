using CargoWise.Customs.GB.MessageDefinitions.CDS.CDSInventoryLinking;
using Enterprise.Customs.GB.Business;

namespace Enterprise.Customs.GB.CDS
{
	class ArrivalAnticipatedRequestMessageBuilder : MovementRequestMessageBuilder
	{
		public ArrivalAnticipatedRequestMessageBuilder(IUkCinvWrapper messageDataProvider, GbInventoryManagementMessageFunction.MasterOrDeclaration level) : base(messageDataProvider, level)
		{
		}

		protected override messageCodeMovement MessageCode => messageCodeMovement.EAA;
	}
}
