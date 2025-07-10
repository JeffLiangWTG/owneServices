using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using CargoWise.Types;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Manifest.Business.MessagesWrappers
{
	public class DeclarationConsignmentGovernmentAgencyGoodsItemAdditionalInformationWrapper : IDeclarationConsignmentGovernmentAgencyGoodsItemAdditionalInformation
	{
		DeclarationConsignmentGovernmentAgencyGoodsItemAdditionalInformationWrapper(ZString description, ZString referenceNumber, ZString code) =>
			(this.description, this.referenceNumber, this.code) = (description, referenceNumber, code);

		public static DeclarationConsignmentGovernmentAgencyGoodsItemAdditionalInformationWrapper NewOrNull(ZString description, ZString referenceNumber, ZString code) =>
			(description.IsEmpty && referenceNumber.IsEmpty && code.IsEmpty) ? null
			: new DeclarationConsignmentGovernmentAgencyGoodsItemAdditionalInformationWrapper(description, referenceNumber, code);

		public ITextType Content => TextTypeWrapper.NewOrNull(description);

		public ICodeType StatementCode => CodeTypeWrapper.NewOrNull(referenceNumber);

		public ICodeType StatementTypeCode => CodeTypeWrapper.NewOrNull(code);

		readonly ZString description;
		readonly ZString referenceNumber;
		readonly ZString code;
	}
}
