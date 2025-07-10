using System.Collections.Generic;
using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Manifest.Business.MessagesWrappers
{
	public class DeclarationConsignmentTransportContractDocumentWrapper : IDeclarationConsignmentTransportContractDocument
	{
		DeclarationConsignmentTransportContractDocumentWrapper(AsycudaTransportDocumentInfo asycudaTransportDocumentInfo, int sequenceNumber)
		{
			this.asycudaTransportDocumentInfo = asycudaTransportDocumentInfo;
			this.asycudaBill = asycudaTransportDocumentInfo.Parent;
			this.sequenceNumber = sequenceNumber;
		}

		public static DeclarationConsignmentTransportContractDocumentWrapper NewOrNull(AsycudaTransportDocumentInfo asycudaTransportDocumentInfo, int sequenceNumber = 1)
			=> asycudaTransportDocumentInfo != null ? new DeclarationConsignmentTransportContractDocumentWrapper(asycudaTransportDocumentInfo, sequenceNumber) : null;

		public ICodeType ConditionCode
		{
			get
			{
				if(sequenceNumber == 1)
				{
					return CodeTypeWrapper.NewOrNull(asycudaBill.ABL_Condition);
				}

				return CodeTypeWrapper.NewOrNull(null);
			}
		}

		public ICollection<IDeclarationConsignmentTransportContractDocumentDeconsolidator> Deconsolidator
			=> new List<IDeclarationConsignmentTransportContractDocumentDeconsolidator>().AsReadOnly();

		public IIDType Id => IDTypeWrapper.NewOrNull(asycudaTransportDocumentInfo.CSI_ReferenceNumber);

		public ITextType IssueLocation => null;

		public ICodeType TypeCode => CodeTypeWrapper.NewOrNull(asycudaTransportDocumentInfo.CSI_Code);

		readonly AsycudaTransportDocumentInfo asycudaTransportDocumentInfo;
		readonly AsycudaBill asycudaBill;
		readonly int sequenceNumber;
	}
}
