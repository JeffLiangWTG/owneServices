using CargoWise.Common;
using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Manifest.Business.MessagesWrappers
{
	public class DeclarationConsignmentConsignmentItemAdditionalInformationWrapper : IDeclarationConsignmentConsignmentItemAdditionalInformation
	{
		DeclarationConsignmentConsignmentItemAdditionalInformationWrapper(AsycudaAdditionalInfo additionalInfo)
		{
			this.additionalInfo = Argument.NotNull(additionalInfo, nameof(additionalInfo));
		}

		public static IDeclarationConsignmentConsignmentItemAdditionalInformation NewOrNull(AsycudaAdditionalInfo additionalInfo)
			=> additionalInfo == null ? null : new DeclarationConsignmentConsignmentItemAdditionalInformationWrapper(additionalInfo);

		public ITextType Content => TextTypeWrapper.NewOrNull(additionalInfo.CSI_Description);

		public ICodeType StatementCode => CodeTypeWrapper.NewOrNull(additionalInfo.CSI_ReferenceNumber);

		public ICodeType StatementTypeCode => CodeTypeWrapper.NewOrNull(additionalInfo.CSI_Code);

		readonly AsycudaAdditionalInfo additionalInfo;
	}
}
