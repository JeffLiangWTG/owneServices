using CargoWise.Types;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Configurations
{
	public sealed class NctsConfiguration : EU.NCTS.Business.NctsConfiguration
	{
		protected override ZBool DocDataPlugInSupportForDepartureMovement => true;

		protected override ZBool FullLoadPortSupportCore => true;

		protected override ZBool UseAdditionalDeclarationTypeCore => true;

		protected override ZBool UsePresentationDateTimeCore => true;

		protected override ZBool UseDeclarantFallBackCore => true;

		protected override ZBool UseBranchOrgProxyFallBackCore => true;

		protected override ZBool UseCompanyOrgProxyFallBackCore => true;

		protected override ZBool UseGuaranteeGridValidationCore => true;

		protected override ZBool ClearExistingGuaranteesConfigurationCore => true;

		protected override EU.NCTS.Business.BillConfiguration GetNewBillConfiguration() => new BillConfiguration();

		protected override EU.NCTS.Business.GoodsItemsConfiguration GetNewGoodsItemsConfiguration() => new GoodsItemsConfiguration();

		protected override EU.NCTS.Business.ValidationRuleConfiguration GetNewValidationRuleConfiguration() => new ValidationRuleConfiguration();

		protected override EU.NCTS.Business.NctsEuOfficeCodeConfiguration GetNewNctsEuOfficeCodeConfiguration() => new NctsFrOfficeCodeConfiguration();

		protected override EU.NCTS.Business.MessageSendingConfiguration GetNewMessageSendingConfiguration() => new TP5MessageSendingConfiguration();

		protected override EU.NCTS.Business.MovementHeaderConfiguration GetNewMovementHeaderConfiguration() => new MovementHeaderConfiguration();

		protected override EU.NCTS.Business.GuaranteeConfiguration GetNewGuaranteeConfiguration() => new GuaranteeConfiguration();

		protected override EU.NCTS.Business.NctsContainerConfiguration GetNewNctsContainerConfiguration() => new NctsContainerConfiguration();

		protected override EU.NCTS.Business.NctsPackageConfiguration GetNewNctsPackageConfiguration() => new NctsPackageConfiguration();
	}
}
