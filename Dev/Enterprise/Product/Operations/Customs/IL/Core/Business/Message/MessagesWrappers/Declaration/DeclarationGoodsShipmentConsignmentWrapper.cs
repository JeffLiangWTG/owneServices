using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using Enterprise.Customs.IL.Business.Message.MessagesWrappers.Declaration;

namespace Enterprise.Customs.IL.Business
{
	public class DeclarationGoodsShipmentConsignmentWrapper : IDeclarationGoodsShipmentConsignment
	{
		DeclarationGoodsShipmentConsignmentWrapper(CusEntryInstruction entryInstruction, JobDeclaration jobDeclaration)
		{
			this.entryInstruction = entryInstruction;
			this.jobDeclaration = jobDeclaration;
		}

		public static DeclarationGoodsShipmentConsignmentWrapper NewOrNull(CusEntryInstruction entryInstruction, JobDeclaration jobDeclaration)
			=> entryInstruction != null && jobDeclaration != null ? new DeclarationGoodsShipmentConsignmentWrapper(entryInstruction, jobDeclaration) : null;

		public IDeclarationGoodsShipmentConsignmentLoadingLocation LoadingLocation => DeclarationGoodsShipmentConsignmentLoadingLocationWrapper.NewOrNull(jobDeclaration);

		public IDeclarationGoodsShipmentConsignmentDmExtensions DmExtensions => DeclarationGoodsShipmentConsignmentDmExtensionsWrapper.NewOrNull(entryInstruction, jobDeclaration);

		public IDeclarationGoodsShipmentConsignmentTransportContractDocument TransportContractDocument => DeclarationGoodsShipmentConsignmentTransportContractDocumentWrapper.NewOrNull(jobDeclaration);

		public IDeclarationGoodsShipmentConsignmentUnloadingLocation UnloadingLocation => DeclarationGoodsShipmentConsignmentUnloadingLocationWrapper.NewOrNull(jobDeclaration);

		readonly JobDeclaration jobDeclaration;
		readonly CusEntryInstruction entryInstruction;
	}
}
