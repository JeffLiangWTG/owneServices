using CargoWise.Common;
using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Manifest.Business.MessagesWrappers
{
	public class DeclarationConsignmentConsignmentItemCommodityClassificationWrapper : IDeclarationConsignmentConsignmentItemCommodityClassification
	{
		DeclarationConsignmentConsignmentItemCommodityClassificationWrapper(string typeCode, string id)
		{
			this.typeCode = typeCode;
			this.id = id;
		}

		internal static IDeclarationConsignmentConsignmentItemCommodityClassification NewOrNull(string typeCode, string id)
			=> typeCode.IsNullOrEmpty() || id.IsNullOrEmpty() ? null : new DeclarationConsignmentConsignmentItemCommodityClassificationWrapper(typeCode, id);

		public IIDType Id => IDTypeWrapper.NewOrNull(id);

		public ICodeType IdentificationTypeCode => CodeTypeWrapper.NewOrNull(typeCode);

		readonly string typeCode;
		readonly string id;
	}
}
