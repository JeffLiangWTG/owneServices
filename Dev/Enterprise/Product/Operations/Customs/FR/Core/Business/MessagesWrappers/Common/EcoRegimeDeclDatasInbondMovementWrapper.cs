using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class EcoRegimeDeclDatasInbondMovementWrapper : IEcoRegimeDeclDatas
	{
		public EcoRegimeDeclDatasInbondMovementWrapper(PreviousInbondMovement previousInbondMovement)
		{
			itemPreviousInbondMovement = Argument.NotNull(previousInbondMovement, "PreviousInbondMovement cannot be null");
		}

		public ZString CusDeclarationID => itemPreviousInbondMovement.Number;

		public ZString EcoRegimeDeclTypeCode => itemPreviousInbondMovement.Type;

		protected readonly PreviousInbondMovement itemPreviousInbondMovement;
	}
}
