using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Manifest.Business.MessagesWrappers
{
	public class DeclarationConsignmentUndgContactCommunicationWrapper : IDeclarationConsignmentUNDangerousGoodsCommunication
	{
		DeclarationConsignmentUndgContactCommunicationWrapper(string id, string typeId)
		{
			this.id = id;
			this.typeId = typeId;
		}

		internal static IDeclarationConsignmentUNDangerousGoodsCommunication NewOrNull(string id, string typeId) => string.IsNullOrEmpty(id) ? null : new DeclarationConsignmentUndgContactCommunicationWrapper(id, typeId);

		public IIDType Id => IDTypeWrapper.NewOrNull(id);

		public IIDType TypeId => IDTypeWrapper.NewOrNull(typeId);

		readonly string id;
		readonly string typeId;
	}
}
