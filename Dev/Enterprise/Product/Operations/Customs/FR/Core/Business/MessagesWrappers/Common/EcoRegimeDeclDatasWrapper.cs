using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class EcoRegimeDeclDatasWrapper : IEcoRegimeDeclDatas
	{
		public EcoRegimeDeclDatasWrapper(PreviousDocument previousDoc)
		{
			this.itemPreviousDoc = Argument.NotNull(previousDoc, "PreviousDocument cannot be null");
		}

		public ZString CusDeclarationID => itemPreviousDoc?.CSI_ReferenceNumber ?? ZString.Empty;

		public ZString EcoRegimeDeclTypeCode => itemPreviousDoc?.CSI_Code ?? ZString.Empty;

		protected readonly PreviousDocument itemPreviousDoc;
	}
}
