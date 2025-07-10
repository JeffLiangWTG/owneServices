using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using CargoWise.Types;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IL.Business.Message.MessagesWrappers.Declaration
{
	public class DeclarationGoodsShipmentConsignmentTransportContractDocumentWrapper : IDeclarationGoodsShipmentConsignmentTransportContractDocument
	{
		DeclarationGoodsShipmentConsignmentTransportContractDocumentWrapper(JobDeclaration jobDeclaration)
		{
			this.jobDeclaration = jobDeclaration;
		}

		public static DeclarationGoodsShipmentConsignmentTransportContractDocumentWrapper NewOrNull(JobDeclaration jobDeclaration)
			=> jobDeclaration != null ? new DeclarationGoodsShipmentConsignmentTransportContractDocumentWrapper(jobDeclaration) : null;

		public IIDType ID => IDTypeWrapper.NewOrNull(GetIdBasedOnTransportMode());

		public IDeclarationGoodsShipmentConsignmentTransportContractDocumentDMExt DmExtensions => DeclarationGoodsShipmentConsignmentTransportContractDocumentDMExtensionsWrapper.NewOrNull(jobDeclaration);

		public string IssueDateTime => jobDeclaration.JE_MasterBillIssuedDate.IsEmpty ? null : jobDeclaration.JE_MasterBillIssuedDate.ToCustomsDateTimeString();

		public ICodeType TypeCode => CodeTypeWrapper.NewOrNull(jobDeclaration.JE_TransportMeans);

		ZString GetIdBasedOnTransportMode()
			=> jobDeclaration?.JE_TransportMode.ToString() switch
			{
				TransportModes.Air => !jobDeclaration.JE_DateAtOrigin.IsEmpty ? jobDeclaration.JE_DateAtOrigin.Year.ToString() : (!jobDeclaration.JE_ExportDate.IsEmpty ? jobDeclaration.JE_ExportDate.Year.ToString() : (!jobDeclaration.JE_DateAtFinalDestination.IsEmpty ? jobDeclaration.JE_DateAtFinalDestination.Year.ToString() : string.Empty)),
				TransportModes.Sea or TransportModes.Road => jobDeclaration.JE_ManifestNumber,
				_ => ZString.Empty
			};

		readonly JobDeclaration jobDeclaration;
	}
}
