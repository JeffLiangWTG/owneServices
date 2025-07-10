using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsActualConsigneeJobDocAddressValidation : JobDocAddressValidation
	{
		public NctsActualConsigneeJobDocAddressValidation(AutoJobDocAddress parent, NctsHeaderMessageSendingObject nctsHeaderMessageSendingObject)
			: base(parent)
		{
			NctsHeaderMessageSendingObject = Argument.NotNull(nctsHeaderMessageSendingObject, nameof(nctsHeaderMessageSendingObject));
			NctsHeader = NctsHeaderMessageSendingObject.NctsHeader;
		}

		protected NctsHeaderMessageSendingObject NctsHeaderMessageSendingObject { get; }

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();
			CheckRuleTR0021ForOrg(Parent.OrganisationPK, Parent.OrganisationPKInfo);
			CheckRuleC0215ForOrg(Parent.OrganisationPK, Parent.OrganisationPKInfo);
		}

		void CheckRuleTR0021ForOrg(ZGuid orgPk, ZPropertyInfo orgPkInfo)
		{
			if (ValidationDecider.IsRuleTR0021Active
				&& orgPk.IsEmpty
				&& NctsHeaderMessageSendingObject.ActualOfficeOfDestination.IsEmpty
				&& !NctsHeaderMessageSendingObject.QueryInformation.IsEmpty)
			{
				orgPkInfo.AddMessageError(NctsHeaderValidationHelper.TR0021ValidationMessage);
			}
		}

		void CheckRuleC0215ForOrg(ZGuid orgPk, ZPropertyInfo orgPkInfo)
		{
			if (NctsHeader.Configuration.ValidationRuleConfiguration.IsRuleC0215Active
				&& NctsHeaderMessageSendingObject.ActualOfficeOfDestination.IsEmpty
				&& !NctsHeaderMessageSendingObject.AdditionalText.IsEmpty
				&& orgPk.IsEmpty)
			{
				orgPkInfo.AddMessageError(Res.GetString("0872BF32-8D00-4BA4-A408-0D60BC2FF552", "{0} You have not selected Actual Consignee.", ValidationRuleCodeConstants.C0215.GetRuleCodeMessagePrefix()));
			}
		}

		NctsHeader NctsHeader { get; }

		INctsHeaderMessageSendingObjectValidationDecider ValidationDecider => NctsHeader.Configuration.MessageSendingConfiguration.GetValidationDecider();
	}
}
